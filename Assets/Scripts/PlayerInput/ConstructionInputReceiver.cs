using System;
using Streets;
using UnityEngine;

namespace PlayerInput
{
    public class ConstructionInputReceiver : MonoBehaviour, IPlayerInputReceiver
    {
        [SerializeField] private LayerMask floorLayerMask;
        [SerializeField] private LayerMask streetLayerMask;
        [SerializeField] private LayerMask intersectionLayerMask;
        [SerializeField] private StreetInfo streetInfo;
        private ConstructionManager _constructionManager;
        private Camera _camera;

        private void Start()
        {
            _constructionManager = new ConstructionManager(floorLayerMask, streetLayerMask, intersectionLayerMask, streetInfo);
            _camera = GetComponent<Camera>();
        }

        private void Update()
        {
            UpdateMousePosition();
            
            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                OnLeftMouseClicked();
            }
            else if (Input.GetKeyDown(KeyCode.Mouse1))
            {
                OnRightMouseClicked();
            }
        }

        public void UpdateMousePosition()
        {
            _constructionManager.UpdateMousePosition(_camera);
        }

        public void OnLeftMouseClicked()
        {
            //Todo check money
            _constructionManager.PlaceConstruction(_camera);
        }

        public void OnRightMouseClicked()
        {
            _constructionManager.Reset();
        }
    }
}