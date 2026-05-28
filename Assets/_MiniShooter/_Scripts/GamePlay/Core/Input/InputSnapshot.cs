using UnityEngine;

namespace Core.Input
{
    public struct InputSnapshot
    {
        public readonly InputContext Context;
        public readonly Vector2 MoveInput;
        public readonly Vector2 LookInput;
        public readonly Input AttackInput;
        public readonly Input JumpInput;
        public readonly Input CrouchInput;
        public readonly Input SprintInput;
        public readonly Input InteractInput;
        public readonly Input ReloadInput;

        public InputSnapshot(InputContext context, Vector2 moveInput, Vector2 lookInput, Input attackInput, Input jumpInput, Input crouchInput, Input sprintInput, Input interactInput, Input reloadInput)
        {
            Context = context;
            MoveInput = moveInput;
            LookInput = lookInput;
            AttackInput = attackInput;
            JumpInput = jumpInput;
            CrouchInput = crouchInput;
            SprintInput = sprintInput;
            InteractInput = interactInput;
            ReloadInput = reloadInput;
        }
    }
}