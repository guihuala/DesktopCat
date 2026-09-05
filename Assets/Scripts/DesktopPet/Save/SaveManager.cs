using System;
using System.IO;
using DesktopPet.Events;
using DesktopPet.Pet.State;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DesktopPet.Save
{
    [DefaultExecutionOrder(-100)]
    public class SaveManager : MonoBehaviour
    {
        private const string SaveFileName = "desktop_pet_save.json";
        private const float AutoSaveDelay = 0.5f;
        private const int CurrentSaveVersion = 4;
        private const float PetStatsCheckpointSeconds = 10f;

        private static SaveManager instance;

        [SerializeField] private WindowController windowController;
        [SerializeField] private Transform petRoot;
        [SerializeField] private PetStateController petState;
        [SerializeField] private string petRootName = "Cat";
        [SerializeField] private bool autoSave = true;

        private readonly CompositeSubscription subscriptions = new CompositeSubscription();
        private DesktopPetSaveData data;
        private string savePath;
        private bool pendingSave;
        private float saveTimer;
        private bool isApplyingLoadedData;
        private float lastPetStatsCheckpoint;

        public static DesktopPetSaveData Data => instance != null ? instance.data : null;
        public static string SavePath => instance != null ? instance.savePath : string.Empty;

        public static void MarkDataDirty()
        {
            if (instance != null) instance.MarkDirty();
        }

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

            if (TryLoadFile(savePath, out var loaded, out var primaryError))
            {
                data = loaded;
                MigrateAndRepair();
                return;
            }

            var backupPath = savePath + ".bak";
            if (TryLoadFile(backupPath, out loaded, out var backupError))
            {
                data = loaded;
                MigrateAndRepair();
                Debug.LogWarning($"主存档损坏，已从备份恢复。原因：{primaryError}");
                try { File.Copy(backupPath, savePath, true); }
                catch (Exception exception) { Debug.LogWarning($"恢复主存档文件失败：{exception.Message}"); }
                return;
            }

            Debug.LogWarning($"主存档与备份均无法读取，将使用新存档。主存档：{primaryError}；备份：{backupError}");
            data = new DesktopPetSaveData();
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
                var backupPath = savePath + ".bak";
                File.WriteAllText(tempPath, json);

                if (File.Exists(savePath))
                {
                    File.Copy(savePath, backupPath, true);
                }
                File.Copy(tempPath, savePath, true);
                File.Delete(tempPath);
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

            if (petState == null && petRoot != null)
            {
                petState = petRoot.GetComponent<PetStateController>();
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

            if (petState != null && data.pet.hasRuntimeStats)
            {
                petState.SetStats(data.pet.energy, data.pet.hunger);
            }

            isApplyingLoadedData = false;
        }

        private void SubscribeEvents()
        {
            subscriptions.Clear();
            subscriptions.Add(GameEventBus.Subscribe<WindowSettingsChangedEvent>(OnWindowSettingsChanged));
            subscriptions.Add(GameEventBus.Subscribe<WindowMovedEvent>(OnWindowMoved));
            subscriptions.Add(GameEventBus.Subscribe<PetScaleChangedEvent>(OnPetScaleChanged));
            subscriptions.Add(GameEventBus.Subscribe<DayNightModeChangedEvent>(OnDayNightModeChanged));
            subscriptions.Add(GameEventBus.Subscribe<PetStatsChangedEvent>(OnPetStatsChanged));
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

        private void OnDayNightModeChanged(DayNightModeChangedEvent gameEvent)
        {
            data.appearance.dayNightMode = gameEvent.Mode;
            MarkDirty();
        }

        private void OnPetStatsChanged(PetStatsChangedEvent gameEvent)
        {
            data.pet.hasRuntimeStats = true;
            data.pet.energy = Mathf.Clamp(gameEvent.Energy, 0f, 100f);
            data.pet.hunger = Mathf.Clamp(gameEvent.Hunger, 0f, 100f);
            if (Time.unscaledTime - lastPetStatsCheckpoint < PetStatsCheckpointSeconds) return;
            lastPetStatsCheckpoint = Time.unscaledTime;
            MarkDirty();
        }

        private static bool TryLoadFile(string path, out DesktopPetSaveData loaded, out string error)
        {
            loaded = null;
            error = string.Empty;
            if (!File.Exists(path)) { error = "文件不存在"; return false; }
            try
            {
                loaded = JsonUtility.FromJson<DesktopPetSaveData>(File.ReadAllText(path));
                if (loaded != null) return true;
                error = "内容为空";
            }
            catch (Exception exception) { error = exception.Message; }
            return false;
        }

        private void MigrateAndRepair()
        {
            var sourceVersion = Mathf.Max(1, data.saveVersion);
            EnsureDataShape();
            if (sourceVersion > CurrentSaveVersion)
            {
                Debug.LogWarning($"存档版本 {sourceVersion} 高于当前支持版本 {CurrentSaveVersion}，将只读取已知字段。");
                return;
            }
            if (sourceVersion < 3)
            {
                data.pet.hasRuntimeStats = false;
            }
            if (sourceVersion < 4)
            {
                // The preview-only onboarding flow replaced the earlier version
                // that selected the live gameplay cat. Re-run this one step only;
                // furniture, settings, and all other progress remain untouched.
                data.appearance.hasChosenPet = false;
            }
            data.saveVersion = CurrentSaveVersion;
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

            if (data.appearance == null)
            {
                data.appearance = new AppearanceSettingsData();
            }

            if (data.onlineReward == null)
            {
                data.onlineReward = new OnlineRewardSaveData();
            }

            if (data.furnitureInventory == null)
            {
                data.furnitureInventory = new FurnitureInventorySaveData();
            }

            if (data.furnitureInventory.items == null)
            {
                data.furnitureInventory.items = new System.Collections.Generic.List<FurnitureItemSaveData>();
            }

            if (data.furnitureInventory.discoveredIds == null)
            {
                data.furnitureInventory.discoveredIds = new System.Collections.Generic.List<string>();
            }

            if (data.furniturePlacement == null)
            {
                data.furniturePlacement = new FurniturePlacementSaveData();
            }

            if (data.furniturePlacement.items == null)
            {
                data.furniturePlacement.items = new System.Collections.Generic.List<FurniturePlacementItemSaveData>();
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
