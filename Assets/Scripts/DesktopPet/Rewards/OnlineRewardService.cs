using DesktopPet.Events;
using DesktopPet.Save;
using UnityEngine;

namespace DesktopPet.Rewards
{
    [DefaultExecutionOrder(-50)]
    public sealed class OnlineRewardService : MonoBehaviour
    {
        [SerializeField] private OnlineRewardConfig config;
        private double elapsedSeconds;
        private double lastRealtime;
        private float saveCheckpoint;

        public double ElapsedSeconds => elapsedSeconds;
        public double IntervalSeconds => config != null ? config.intervalSeconds : 1800d;
        public int PendingRewards { get; private set; }
        public int MaxPendingRewards => config != null ? config.maxPendingRewards : 6;
        public double SecondsUntilNext => PendingRewards >= MaxPendingRewards ? 0d : Mathf.Max(0f, (float)(IntervalSeconds - elapsedSeconds));

        private void Awake()
        {
            if (config == null) config = Resources.Load<OnlineRewardConfig>("Config/OnlineRewardConfig");
            if (config == null)
            {
                config = ScriptableObject.CreateInstance<OnlineRewardConfig>();
                config.hideFlags = HideFlags.HideAndDontSave;
            }
            var saved = SaveManager.Data != null ? SaveManager.Data.onlineReward : null;
            if (saved != null)
            {
                elapsedSeconds = System.Math.Max(0d, saved.elapsedSeconds);
                PendingRewards = Mathf.Clamp(saved.pendingRewards, 0, MaxPendingRewards);
            }
            NormalizeProgress();
            lastRealtime = Time.realtimeSinceStartupAsDouble;
            PublishProgress();
        }

        private void Update()
        {
            var now = Time.realtimeSinceStartupAsDouble;
            var delta = System.Math.Max(0d, now - lastRealtime);
            lastRealtime = now;
            if (PendingRewards < MaxPendingRewards) AddOnlineSeconds(delta, false);
            saveCheckpoint += Time.unscaledDeltaTime;
            if (saveCheckpoint >= config.saveCheckpointSeconds)
            {
                saveCheckpoint = 0f;
                Persist();
            }
        }

        public void AddDebugSeconds(double seconds)
        {
            AddOnlineSeconds(System.Math.Max(0d, seconds), true);
            Persist();
        }

        public bool TryConsumePendingReward()
        {
            if (PendingRewards <= 0) return false;
            PendingRewards--;
            Persist();
            PublishProgress();
            return true;
        }

        private void AddOnlineSeconds(double seconds, bool publish)
        {
            if (PendingRewards >= MaxPendingRewards) return;
            elapsedSeconds += seconds;
            var previousPending = PendingRewards;
            NormalizeProgress();
            if (publish || PendingRewards != previousPending) PublishProgress();
            if (PendingRewards != previousPending) Persist();
        }

        private void NormalizeProgress()
        {
            while (elapsedSeconds >= IntervalSeconds && PendingRewards < MaxPendingRewards)
            {
                elapsedSeconds -= IntervalSeconds;
                PendingRewards++;
            }
            if (PendingRewards >= MaxPendingRewards) elapsedSeconds = 0d;
        }

        private void Persist()
        {
            if (SaveManager.Data == null) return;
            if (SaveManager.Data.onlineReward == null) SaveManager.Data.onlineReward = new OnlineRewardSaveData();
            SaveManager.Data.onlineReward.elapsedSeconds = elapsedSeconds;
            SaveManager.Data.onlineReward.pendingRewards = PendingRewards;
            SaveManager.MarkDataDirty();
        }

        private void PublishProgress()
        {
            GameEventBus.Publish(new OnlineRewardProgressChangedEvent(elapsedSeconds, IntervalSeconds, PendingRewards, MaxPendingRewards));
        }

        private void OnApplicationPause(bool paused)
        {
            if (paused) Persist();
        }

        private void OnApplicationQuit() => Persist();
    }
}
