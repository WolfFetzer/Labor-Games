using UnityEngine;

namespace PlayerInput
{
    public interface IPlayerInputReceiver
    {
        void UpdateMousePosition();

        void OnLeftMouseClicked();
    
        void OnRightMouseClicked();
    }
}