using DesktopPet.Pet.Behavior;
using DesktopPet.Pet.State;
using UnityEngine;

namespace DesktopPet
{
    public sealed class PrototypeDebugPanel : MonoBehaviour
    {
        [SerializeField] private KeyCode toggleKey = KeyCode.F7;
        private PetStateController state;
        private PetBehaviorBrain brain;
        private bool visible;
        private Rect windowRect = new Rect(12f, 12f, 270f, 255f);

        private void Awake()
        {
            state = GetComponent<PetStateController>();
            brain = GetComponent<PetBehaviorBrain>();
        }

        private void Update()
        {
            if (Input.GetKeyDown(toggleKey)) visible = !visible;
        }

        private void OnGUI()
        {
            if (!visible || state == null || brain == null) return;
            windowRect = GUI.Window(GetInstanceID(), windowRect, DrawWindow, "Desktop Pet Debug (F7)");
        }

        private void DrawWindow(int id)
        {
            GUILayout.Label($"Behaviour: {brain.CurrentBehaviourId}");
            GUILayout.Label($"Energy: {state.Energy:0.0}   Hunger: {state.Hunger:0.0}");
            var energy = GUILayout.HorizontalSlider(state.Energy, 0f, 100f);
            var hunger = GUILayout.HorizontalSlider(state.Hunger, 0f, 100f);
            if (!Mathf.Approximately(energy, state.Energy) || !Mathf.Approximately(hunger, state.Hunger))
                state.SetStats(energy, hunger);
            GUILayout.BeginHorizontal();
            ForceButton("Idle"); ForceButton("Wander");
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            ForceButton("Nap"); ForceButton("Sleep");
            GUILayout.EndHorizontal();
            if (GUILayout.Button("Restore defaults")) state.SetStats(state.Tuning.initialEnergy, state.Tuning.initialHunger);
            GUILayout.Label(state.IsUninterruptible ? "Locked by uninterruptible behaviour" : "Behaviour can be interrupted");
            GUI.DragWindow(new Rect(0f, 0f, windowRect.width, 24f));
        }

        private void ForceButton(string behaviourId)
        {
            if (GUILayout.Button(behaviourId)) brain.ForceBehaviour(behaviourId);
        }
    }
}
