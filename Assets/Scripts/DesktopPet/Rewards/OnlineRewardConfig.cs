using UnityEngine;

namespace DesktopPet.Rewards
{
    [CreateAssetMenu(menuName = "Desktop Pet/Online Reward Config", fileName = "OnlineRewardConfig")]
    public sealed class OnlineRewardConfig : ScriptableObject
    {
        [Min(1f)] public float intervalSeconds = 1800f;
        [Min(1)] public int maxPendingRewards = 6;
        [Min(1f)] public float saveCheckpointSeconds = 10f;
    }
}
