using DesktopPet.Pet.State;
using UnityEngine;

namespace DesktopPet.Pet.Presentation
{
    public sealed class PetPresentationController : MonoBehaviour
    {
        [SerializeField] private Animator animator;
        [SerializeField] private string behaviourParameter = "Behaviour";
        [SerializeField] private string movingParameter = "Moving";

        private void Awake()
        {
            if (animator == null) animator = GetComponentInChildren<Animator>();
        }

        public void Present(PetBehaviourId behaviour, bool moving)
        {
            if (animator == null || animator.runtimeAnimatorController == null) return;
            SetIntegerIfPresent(behaviourParameter, (int)behaviour);
            SetBoolIfPresent(movingParameter, moving);
        }

        private void SetIntegerIfPresent(string parameter, int value)
        {
            foreach (var item in animator.parameters)
                if (item.name == parameter && item.type == AnimatorControllerParameterType.Int) { animator.SetInteger(parameter, value); return; }
        }

        private void SetBoolIfPresent(string parameter, bool value)
        {
            foreach (var item in animator.parameters)
                if (item.name == parameter && item.type == AnimatorControllerParameterType.Bool) { animator.SetBool(parameter, value); return; }
        }
    }
}
