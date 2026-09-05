using System;
using DesktopPet.Config;
using DesktopPet.Events;
using UnityEngine;

namespace DesktopPet.Pet.State
{
    public enum PetBehaviourId { Idle, Wander, Nap, Sleep, Eat, ApproachCamera }

    public sealed class PetStateController : MonoBehaviour
    {
        [SerializeField] private PetTuningConfig tuning;
        [SerializeField, Range(0f, 100f)] private float energy = 70f;
        [SerializeField, Range(0f, 100f)] private float hunger = 20f;

        public event Action<float, float> StatsChanged;
        public float Energy => energy;
        public float Hunger => hunger;
        public PetBehaviourId CurrentBehaviour { get; private set; } = PetBehaviourId.Idle;
        public float BehaviourStartedAt { get; private set; }
        public bool IsUninterruptible { get; private set; }
        public PetTuningConfig Tuning => tuning;

        public void Initialize(PetTuningConfig config, bool resetStats = true)
        {
            tuning = config != null ? config : PetTuningConfig.CreateRuntimeDefaults();
            if (resetStats)
            {
                SetStats(tuning.initialEnergy, tuning.initialHunger);
            }
        }

        private void Awake()
        {
            if (tuning == null) Initialize(null);
        }

        private void Update()
        {
            AddHunger(tuning.hungerGainPerMinute * Time.deltaTime / 60f);
            if (CurrentBehaviour != PetBehaviourId.Nap && CurrentBehaviour != PetBehaviourId.Sleep)
                AddEnergy(-tuning.awakeEnergyCostPerMinute * Time.deltaTime / 60f);
        }

        public void SetStats(float newEnergy, float newHunger)
        {
            var clampedEnergy = Mathf.Clamp(newEnergy, 0f, 100f);
            var clampedHunger = Mathf.Clamp(newHunger, 0f, 100f);
            if (Mathf.Approximately(energy, clampedEnergy) && Mathf.Approximately(hunger, clampedHunger)) return;
            energy = clampedEnergy;
            hunger = clampedHunger;
            StatsChanged?.Invoke(energy, hunger);
            GameEventBus.Publish(new PetStatsChangedEvent(energy, hunger));
        }

        public void AddEnergy(float amount) => SetStats(energy + amount, hunger);
        public void AddHunger(float amount) => SetStats(energy, hunger + amount);

        public void SetBehaviour(PetBehaviourId behaviour, bool uninterruptible)
        {
            CurrentBehaviour = behaviour;
            IsUninterruptible = uninterruptible;
            BehaviourStartedAt = Time.time;
            GameEventBus.Publish(new PetBehaviourChangedEvent(behaviour.ToString(), uninterruptible));
        }
    }
}
