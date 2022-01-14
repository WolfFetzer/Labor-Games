using System;
using UnityEngine;

namespace PlayerInput
{
    public class DefaultInputReceiver : MonoBehaviour, IPlayerInputReceiver
    {
        [SerializeField] private LayerMask selectableLayerMask;
        private Camera _camera;


        private void Start()
        {
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
            
        }

        public void OnLeftMouseClicked()
        {
            Ray ray = _camera.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit, 10000f, selectableLayerMask))
            {
                Building house = hit.collider.GetComponent<Building>();
                if (house == null) return;
                
                Debug.Log(house);
            }
        }
    
        public void OnRightMouseClicked()
        {
            Debug.Log("Right Mouse: Default");
        }
    }
    
}