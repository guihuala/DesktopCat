namespace DesktopPet.Pet.Behavior
{
    public interface IPetBehavior
    {
        string Id { get; }
        bool IsInterruptible { get; }
        bool CanEnter(PetContext context);
        float GetScore(PetContext context);
        void Enter(PetContext context);
        void Tick(PetContext context, float deltaTime);
        bool IsComplete(PetContext context);
        void Exit(PetContext context);
    }
}
