using UnityEngine;

namespace PlayerInput
{
    public interface IPlayerInputReceiver
    {
        void UpdateMousePosition(Camera camera);

        void OnLeftMouseClicked(Camera camera);
    
        void OnRightMouseClicked(Camera camera);
    }
}