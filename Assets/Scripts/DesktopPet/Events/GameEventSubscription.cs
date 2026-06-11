using System;

namespace DesktopPet.Events
{
    public sealed class GameEventSubscription : IDisposable
    {
        private Action unsubscribe;

        public GameEventSubscription(Action unsubscribe)
        {
            this.unsubscribe = unsubscribe;
        }

        public void Dispose()
        {
            unsubscribe?.Invoke();
            unsubscribe = null;
        }
    }
}
