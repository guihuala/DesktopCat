using DesktopPet.Config;
using DesktopPet.Pet.Movement;
using DesktopPet.Pet.Presentation;
using DesktopPet.Pet.State;
using DesktopPet.Activity;

namespace DesktopPet.Pet.Behavior
{
    public sealed class PetContext
    {
        public readonly PetStateController State;
        public readonly PetMovementController Movement;
        public readonly PetPresentationController Presentation;
        public readonly PetTuningConfig Tuning;
        public PlayerActivityTracker Activity;
        public bool FeedRequested;
        public bool CallRequested;
        public bool ForceCallResponse;
        public float LastApproachTime = float.NegativeInfinity;
        public float LastNapTime = float.NegativeInfinity;

        public PetContext(PetStateController state, PetMovementController movement, PetPresentationController presentation, PetTuningConfig tuning)
        { State = state; Movement = movement; Presentation = presentation; Tuning = tuning; }
    }
}
