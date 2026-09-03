using System.Collections.Generic;
using DesktopPet.Config;
using DesktopPet.Pet.Movement;
using DesktopPet.Pet.Presentation;
using DesktopPet.Pet.State;
using UnityEngine;
using DesktopPet.Events;
using DesktopPet.Activity;

namespace DesktopPet.Pet.Behavior
{
    public sealed class PetBehaviorBrain : MonoBehaviour
    {
        [SerializeField] private PetStateController state;
        [SerializeField] private PetMovementController movement;
        [SerializeField] private PetPresentationController presentation;
        private readonly List<IPetBehavior> behaviours = new List<IPetBehavior>();
        private PetContext context;
        private IPetBehavior current;
        private float nextDecisionTime;

        public string CurrentBehaviourId => current != null ? current.Id : "None";
        public float CurrentBehaviourDuration => state != null ? Mathf.Max(0f, Time.time - state.BehaviourStartedAt) : 0f;
        public float SecondsUntilDecision => Mathf.Max(0f, nextDecisionTime - Time.time);
        public PlayerActivityLevel ActivityLevel => context != null && context.Activity != null
            ? context.Activity.Level
            : PlayerActivityLevel.Idle;

        public void Initialize(PetTuningConfig tuning)
        {
            state = state != null ? state : GetComponent<PetStateController>();
            movement = movement != null ? movement : GetComponent<PetMovementController>();
            presentation = presentation != null ? presentation : GetComponent<PetPresentationController>();
            state.Initialize(tuning);
            movement.Initialize(tuning);
            context = new PetContext(state, movement, presentation, tuning);
            context.Activity = FindObjectOfType<PlayerActivityTracker>();
            behaviours.Clear();
            behaviours.Add(new SleepBehavior());
            behaviours.Add(new EatBehavior());
            behaviours.Add(new ApproachCameraBehavior());
            behaviours.Add(new NapBehavior());
            behaviours.Add(new WanderBehavior());
            behaviours.Add(new IdleBehavior());
            SwitchTo(behaviours[behaviours.Count - 1]);
        }

        private void Start()
        {
            if (context == null) Initialize(state != null && state.Tuning != null ? state.Tuning : PetTuningConfig.CreateRuntimeDefaults());
        }

        private void Update()
        {
            if (context == null || current == null) return;
            current.Tick(context, Time.deltaTime);
            presentation.Present(state.CurrentBehaviour, movement.IsMoving);
            if (!current.IsComplete(context) && (Time.time < nextDecisionTime || !current.IsInterruptible)) return;
            SelectBestBehaviour();
        }

        public bool ForceBehaviour(string id)
        {
            var target = behaviours.Find(item => item.Id == id);
            if (target == null || context == null) return false;
            SwitchTo(target);
            return true;
        }

        public string GetDebugCandidateSummary()
        {
            if (context == null) return "Brain not initialized";
            var lines = new List<string>(behaviours.Count);
            foreach (var candidate in behaviours)
            {
                var available = candidate == current || candidate.CanEnter(context);
                var score = available ? candidate.GetScore(context).ToString("0.00") : "blocked";
                lines.Add($"{candidate.Id}: {score}");
            }
            return string.Join("   ", lines);
        }

        public void RequestFeed()
        {
            context.FeedRequested = true;
            nextDecisionTime = Time.time;
        }

        public bool RequestCall(bool forceResponse)
        {
            if (current != null && !current.IsInterruptible)
            {
                GameEventBus.Publish(new PetFeedbackEvent("猫咪正在熟睡……", false));
                return false;
            }
            var chance = state.CurrentBehaviour == PetBehaviourId.Sleep ? context.Tuning.sleepingCallResponseChance : context.Tuning.callResponseChance;
            context.CallRequested = forceResponse || Random.value <= chance;
            context.ForceCallResponse = forceResponse;
            GameEventBus.Publish(new PetInteractionEvent("call", transform.position));
            GameEventBus.Publish(new PetFeedbackEvent(context.CallRequested ? "猫咪听见了！" : "猫咪假装没听见", context.CallRequested));
            nextDecisionTime = Time.time;
            return context.CallRequested;
        }

        private void SelectBestBehaviour()
        {
            IPetBehavior best = null;
            var bestScore = float.MinValue;
            foreach (var candidate in behaviours)
            {
                if (candidate == current || !candidate.CanEnter(context)) continue;
                var score = candidate.GetScore(context) + Random.Range(0f, 0.05f);
                if (score > bestScore) { best = candidate; bestScore = score; }
            }
            if (best != null) SwitchTo(best);
            else nextDecisionTime = Time.time + context.Tuning.decisionInterval;
        }

        private void SwitchTo(IPetBehavior next)
        {
            current?.Exit(context);
            current = next;
            current.Enter(context);
            nextDecisionTime = Time.time + Mathf.Max(context.Tuning.decisionInterval, context.Tuning.minimumBehaviourDuration);
        }

        private abstract class BehaviorBase : IPetBehavior
        {
            public abstract string Id { get; }
            public virtual bool IsInterruptible => true;
            public abstract PetBehaviourId StateId { get; }
            public virtual bool CanEnter(PetContext context) => true;
            public abstract float GetScore(PetContext context);
            public virtual void Enter(PetContext context) => context.State.SetBehaviour(StateId, !IsInterruptible);
            public abstract void Tick(PetContext context, float deltaTime);
            public abstract bool IsComplete(PetContext context);
            public virtual void Exit(PetContext context) { }
        }

        private sealed class IdleBehavior : BehaviorBase
        {
            private float until;
            public override string Id => "Idle";
            public override PetBehaviourId StateId => PetBehaviourId.Idle;
            public override float GetScore(PetContext context) => 0.1f;
            public override void Enter(PetContext context) { base.Enter(context); context.Movement.Stop(); until = Time.time + Random.Range(3f, 7f); }
            public override void Tick(PetContext context, float deltaTime) { }
            public override bool IsComplete(PetContext context) => Time.time >= until;
        }

        private sealed class WanderBehavior : BehaviorBase
        {
            public override string Id => "Wander";
            public override PetBehaviourId StateId => PetBehaviourId.Wander;
            public override bool CanEnter(PetContext context) => context.State.Energy > context.Tuning.napEnterEnergy;
            public override float GetScore(PetContext context) => Mathf.InverseLerp(context.Tuning.napEnterEnergy, 100f, context.State.Energy) + 0.15f;
            public override void Enter(PetContext context) { base.Enter(context); context.Movement.MoveToRandomPoint(); }
            public override void Tick(PetContext context, float deltaTime) => context.State.AddEnergy(-context.Tuning.wanderEnergyCostPerMinute * deltaTime / 60f);
            public override bool IsComplete(PetContext context) => !context.Movement.IsMoving;
            public override void Exit(PetContext context) => context.Movement.Stop();
        }

        private sealed class NapBehavior : BehaviorBase
        {
            public override string Id => "Nap";
            public override PetBehaviourId StateId => PetBehaviourId.Nap;
            public override bool CanEnter(PetContext context) => context.State.Energy <= context.Tuning.napEnterEnergy && context.State.Energy > context.Tuning.sleepEnterEnergy;
            public override float GetScore(PetContext context) => 1f - context.State.Energy / 100f;
            public override void Enter(PetContext context) { base.Enter(context); context.Movement.Stop(); }
            public override void Tick(PetContext context, float deltaTime) => context.State.AddEnergy(context.Tuning.napEnergyRecoveryPerMinute * deltaTime / 60f);
            public override bool IsComplete(PetContext context) => context.State.Energy >= context.Tuning.napExitEnergy;
        }

        private sealed class SleepBehavior : BehaviorBase
        {
            private bool reachedBed;
            public override string Id => "Sleep";
            public override bool IsInterruptible => false;
            public override PetBehaviourId StateId => PetBehaviourId.Sleep;
            public override bool CanEnter(PetContext context) => context.State.Energy <= context.Tuning.sleepEnterEnergy;
            public override float GetScore(PetContext context) => 2f;
            public override void Enter(PetContext context) { base.Enter(context); reachedBed = !context.Movement.MoveToBed(); }
            public override void Tick(PetContext context, float deltaTime)
            {
                if (!reachedBed && !context.Movement.IsMoving) reachedBed = true;
                if (reachedBed) context.State.AddEnergy(context.Tuning.sleepEnergyRecoveryPerMinute * deltaTime / 60f);
            }
            public override bool IsComplete(PetContext context) => reachedBed && context.State.Energy >= context.Tuning.sleepExitEnergy;
            public override void Exit(PetContext context) => context.Movement.Stop();
        }

        private sealed class EatBehavior : BehaviorBase
        {
            private bool reachedFood;
            public override string Id => "Eat";
            public override PetBehaviourId StateId => PetBehaviourId.Eat;
            public override bool CanEnter(PetContext context) => context.FeedRequested;
            public override float GetScore(PetContext context) => 3f + context.State.Hunger / 100f;
            public override void Enter(PetContext context) { base.Enter(context); context.FeedRequested = false; reachedFood = !context.Movement.MoveToFood(); }
            public override void Tick(PetContext context, float deltaTime)
            {
                if (!reachedFood && !context.Movement.IsMoving) reachedFood = true;
                if (reachedFood) context.State.AddHunger(-context.Tuning.feedAmountPerMinute * deltaTime / 60f);
            }
            public override bool IsComplete(PetContext context) => reachedFood && context.State.Hunger <= context.Tuning.eatingExitHunger;
            public override void Exit(PetContext context) => context.Movement.Stop();
        }

        private sealed class ApproachCameraBehavior : BehaviorBase
        {
            private float lingerUntil;
            public override string Id => "ApproachCamera";
            public override PetBehaviourId StateId => PetBehaviourId.ApproachCamera;
            public override bool CanEnter(PetContext context)
            {
                return (context.CallRequested || Time.time - context.LastApproachTime >= context.Tuning.approachCooldown)
                    && context.State.CurrentBehaviour != PetBehaviourId.Eat;
            }
            public override float GetScore(PetContext context)
            {
                if (context.CallRequested) return 4f;
                if (context.Activity == null) return 0.05f;
                return context.Activity.Level == PlayerActivityLevel.Active ? context.Tuning.activeApproachBonus
                    : context.Activity.Level == PlayerActivityLevel.Normal ? context.Tuning.normalApproachBonus : 0.02f;
            }
            public override void Enter(PetContext context)
            {
                base.Enter(context);
                context.CallRequested = false;
                context.ForceCallResponse = false;
                context.LastApproachTime = Time.time;
                context.Movement.MoveToCamera();
                lingerUntil = float.PositiveInfinity;
            }
            public override void Tick(PetContext context, float deltaTime)
            {
                if (!context.Movement.IsMoving && float.IsPositiveInfinity(lingerUntil)) lingerUntil = Time.time + 4f;
            }
            public override bool IsComplete(PetContext context) => Time.time >= lingerUntil;
            public override void Exit(PetContext context) => context.Movement.Stop();
        }
    }
}
