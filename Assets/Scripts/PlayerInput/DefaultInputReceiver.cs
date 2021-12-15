using UnityEngine;

namespace PlayerInput
{
    public class DefaultInputReceiver : IPlayerInputReceiver
    {
        public void UpdateMousePosition(Camera camera)
        {
        }

        public void OnLeftMouseClicked(Camera camera)
        {
            Debug.Log("Left Mouse: Default");
        }
    
        public void OnRightMouseClicked(Camera camera)
        {
            Debug.Log("Right Mouse: Default");
        }
    }
}