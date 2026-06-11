using System;
using System.IO;
using DesktopPet.Events;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DesktopPet.Save
{
    [DefaultExecutionOrder(-100)]
    public class SaveManager : MonoBehaviour
    {
        private const string SaveFileName = "desktop_pet_save.json";
        private const float AutoSaveDelay = 0.5f;

        private static SaveManager instance;

        [SerializeField] private WindowController windowController;
        [SerializeField] private Transform petRoot;
        [SerializeField] private string petRootName = "Monkey";
        [SerializeField] private bool autoSave = true;

        private readonly CompositeSubscription subscriptions = new CompositeSubscription();
        private DesktopPetSaveData data;
        private string savePath;
        private bool pendingSave;
        private float saveTimer;
        private bool isApplyingLoadedData;

        public static DesktopPetSaveData Data => instance != null ? instance.data : null;
        public static string SavePath => instance != null ? instance.savePath : string.Empty;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            if (instance != null)
            {
                return;
            }

            var existing = FindObjectOfType<SaveManager>();
            if (existing != null)
            {
                instance = existing;
                return;
            }

            var go = new GameObject("SaveManager");
            DontDestroyOnLoad(go);
            instance = go.AddComponent<SaveManager>();
        }

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);
            savePath = Path.Combine(Application.persistentDataPath, SaveFileName);
            Load();
        }

        private void Start()
        {
            ResolveSceneReferences();
            SubscribeEvents();
            ApplyLoadedData();
        }

        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void Update()
        {
            if (!pendingSave)
            {
                return;
            }

            saveTimer -= Time.unscaledDeltaTime;
            if (saveTimer <= 0f)
            {
                Save();
            }
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                Save();
            }
        }

        private void OnApplicationQuit()
        {
            Save();
        }

        private void OnDestroy()
        {
            subscriptions.Clear();
            if (instance == this)
            {
                instance = null;
            }
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            ResolveSceneReferences();
            ApplyLoadedData();
        }

        public void Load()
        {
            data = new DesktopPetSaveData();

            if (!File.Exists(savePath))
            {
                return;
            }

            try
            {
                var json = File.ReadAllText(savePath);
                var loaded = JsonUtility.FromJson<DesktopPetSaveData>(json);
                if (loaded != null)
                {
                    data = loaded;
                    EnsureDataShape();
                }
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"Failed to load desktop pet save at {savePath}: {exception.Message}");
                data = new DesktopPetSaveData();
            }
        }

        public void Save()
        {
            if (data == null)
            {
                data = new DesktopPetSaveData();
            }

            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(savePath));
                var json = JsonUtility.ToJson(data, true);
                var tempPath = savePath + ".tmp";
                File.WriteAllText(tempPath, json);

                if (File.Exists(savePath))
                {
                    File.Delete(savePath);
                }

                File.Move(tempPath, savePath);
                pendingSave = false;
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"Failed to save desktop pet data at {savePath}: {exception.Message}");
            }
        }

        public void MarkDirty()
        {
            if (!autoSave || isApplyingLoadedData)
            {
                return;
            }

            pendingSave = true;
            saveTimer = AutoSaveDelay;
        }

        private void ResolveSceneReferences()
        {
            if (windowController == null)
            {
                windowController = FindObjectOfType<WindowController>();
            }

            if (petRoot == null)
            {
                var pet = GameObject.Find(petRootName);
                petRoot = pet != null ? pet.transform : null;
            }
        }

        private void ApplyLoadedData()
        {
            isApplyingLoadedData = true;

            if (windowController != null)
            {
                windowController.SetWindowOptions(
                    data.window.alwaysOnTop,
                    data.window.borderless,
                    data.window.transparentBackground,
                    data.window.clickThrough,
                    data.window.allowDrag);

                if (data.window.hasPosition)
                {
                    windowController.SetWindowPosition(data.window.positionX, data.window.positionY);
                }
            }

            if (petRoot != null)
            {
                petRoot.localScale = Vector3.one * data.pet.scale;
                GameEventBus.Publish(new PetScaleChangedEvent(data.pet.scale));
            }

            isApplyingLoadedData = false;
        }

        private void SubscribeEvents()
        {
            subscriptions.Clear();
            subscriptions.Add(GameEventBus.Subscribe<WindowSettingsChangedEvent>(OnWindowSettingsChanged));
            subscriptions.Add(GameEventBus.Subscribe<WindowMovedEvent>(OnWindowMoved));
            subscriptions.Add(GameEventBus.Subscribe<PetScaleChangedEvent>(OnPetScaleChanged));
        }

        private void OnWindowSettingsChanged(WindowSettingsChangedEvent gameEvent)
        {
            data.window.alwaysOnTop = gameEvent.AlwaysOnTop;
            data.window.borderless = gameEvent.Borderless;
            data.window.transparentBackground = gameEvent.TransparentBackground;
            data.window.clickThrough = gameEvent.ClickThrough;
            data.window.allowDrag = gameEvent.AllowDrag;
            MarkDirty();
        }

        private void OnWindowMoved(WindowMovedEvent gameEvent)
        {
            data.window.hasPosition = true;
            data.window.positionX = gameEvent.Position.x;
            data.window.positionY = gameEvent.Position.y;
            MarkDirty();
        }

        private void OnPetScaleChanged(PetScaleChangedEvent gameEvent)
        {
            data.pet.scale = gameEvent.Scale;
            MarkDirty();
        }

        private void EnsureDataShape()
        {
            if (data.window == null)
            {
                data.window = new WindowSettingsData();
            }

            if (data.pet == null)
            {
                data.pet = new PetSettingsData();
            }

            if (data.audio == null)
            {
                data.audio = new AudioSettingsData();
            }

            if (data.privacy == null)
            {
                data.privacy = new PrivacySettingsData();
            }
        }

        private sealed class CompositeSubscription
        {
            private readonly System.Collections.Generic.List<IDisposable> items = new System.Collections.Generic.List<IDisposable>();

            public void Add(IDisposable item)
            {
                if (item != null)
                {
                    items.Add(item);
                }
            }

            public void Clear()
            {
                for (var i = 0; i < items.Count; i++)
                {
                    items[i].Dispose();
                }

                items.Clear();
            }
        }
    }
}
