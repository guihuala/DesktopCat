using UnityEngine;

namespace DesktopPet.Config
{
    [CreateAssetMenu(menuName = "Desktop Pet/Pet Tuning", fileName = "PetTuningConfig")]
    public sealed class PetTuningConfig : ScriptableObject
    {
        [Header("Initial stats")]
        [Range(0f, 100f)] public float initialEnergy = 70f;
        [Range(0f, 100f)] public float initialHunger = 20f;

        [Header("Stat rates per real-time minute")]
        [Min(0f)] public float hungerGainPerMinute = 1.5f;
        [Min(0f)] public float wanderEnergyCostPerMinute = 2f;
        [Min(0f)] public float napEnergyRecoveryPerMinute = 8f;
        [Min(0f)] public float sleepEnergyRecoveryPerMinute = 18f;

        [Header("Behaviour thresholds")]
        [Range(0f, 100f)] public float napEnterEnergy = 45f;
        [Range(0f, 100f)] public float napExitEnergy = 70f;
        [Range(0f, 100f)] public float sleepEnterEnergy = 20f;
        [Range(0f, 100f)] public float sleepExitEnergy = 85f;
        [Min(0.1f)] public float decisionInterval = 2f;
        [Min(0f)] public float minimumBehaviourDuration = 4f;

        [Header("Movement")]
        [Min(0.01f)] public float walkSpeed = 0.65f;
        [Min(0.01f)] public float turnSpeed = 360f;
        [Min(0.01f)] public float arrivalDistance = 0.08f;
        [Min(0.1f)] public float movementTimeout = 12f;
        [Min(0f)] public float wanderRadius = 1.5f;

        [Header("Interaction")]
        [Min(0f)] public float clickCooldown = 0.75f;
        [Min(0f)] public float feedAmountPerMinute = 60f;
        [Range(0f, 100f)] public float eatingExitHunger = 10f;
        [Range(0f, 1f)] public float callResponseChance = 0.75f;
        [Range(0f, 1f)] public float sleepingCallResponseChance = 0.1f;
        [Min(0f)] public float approachCooldown = 30f;
        [Min(0f)] public float activeApproachBonus = 0.65f;
        [Min(0f)] public float normalApproachBonus = 0.15f;

        public static PetTuningConfig CreateRuntimeDefaults()
        {
            var config = CreateInstance<PetTuningConfig>();
            config.hideFlags = HideFlags.HideAndDontSave;
            return config;
        }

        private void OnValidate()
        {
            napEnterEnergy = Mathf.Max(sleepEnterEnergy, napEnterEnergy);
            napExitEnergy = Mathf.Max(napEnterEnergy, napExitEnergy);
            sleepExitEnergy = Mathf.Max(napExitEnergy, sleepExitEnergy);
        }
    }
}
