using System;
using Population;
using TMPro;
using UnityEngine;

namespace PlayerInput
{
    public class DefaultInputReceiver : MonoBehaviour, IPlayerInputReceiver
    {
        [SerializeField] private LayerMask carLayerMask;
        [SerializeField] private LayerMask selectableLayerMask;
        [SerializeField] private TextMeshProUGUI textMesh;
        [SerializeField] private GameObject panel;
        private Camera _camera;

        private void OnDisable()
        {
            panel.SetActive(false);
        }

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
            if (Physics.Raycast(ray, out RaycastHit hit, 10000f, carLayerMask))
            {
                Human human = hit.collider.GetComponent<Car>().Driver;
                textMesh.text =
                    $"Firstname:\n{human.FirstName}\n\nLastname:\n{human.LastName}\n\nGender:\n{human.Gender}\n\nAge:\n{human.Age}\n\nOccupation:\n{human.Occupation}\n";
                panel.SetActive(true);
            }
            else if (Physics.Raycast(ray, out hit, 10000f, selectableLayerMask))
            {
                Building house = hit.collider.GetComponent<Building>();
                if (house == null) return;
                
                Debug.Log(house);
            }
            else
            {
                panel.SetActive(false);
            }
        }
    
        public void OnRightMouseClicked()
        {
            Debug.Log("Right Mouse: Default");
        }
    }
    
}