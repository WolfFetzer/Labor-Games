using Streets;
using UnityEngine;

namespace PlayerInput
{
    public class ConstructionInputReceiver : IPlayerInputReceiver
    {
        private readonly ConstructionManager _constructionManager = new ConstructionManager();

        public void UpdateMousePosition(Camera camera)
        {
            _constructionManager.UpdateMousePosition(camera);
        }

        public void OnLeftMouseClicked(Camera camera)
        {
            //Todo check money
            _constructionManager.PlaceConstruction(camera);
        }

        public void OnRightMouseClicked(Camera camera)
        {
            _constructionManager.Reset();
        }
    }
}