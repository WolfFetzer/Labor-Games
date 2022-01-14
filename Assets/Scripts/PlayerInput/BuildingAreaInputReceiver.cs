using System;
using Buildings;
using Buildings.BuildingArea;
using UnityEngine;

namespace PlayerInput
{
    public class BuildingAreaInputReceiver : MonoBehaviour, IPlayerInputReceiver
    {
        [SerializeField] private LayerMask areaLayerMask;
        private BuildingAreaManager _buildingAreaManager;
        private Camera _camera;

        private AreaType _type;
        public AreaType AreaType
        {
            get => _type;
            set
            {
                if (value == _type) return;

                _type = value;
                Debug.Log(_type);
                onAreaTypeChanged.Invoke(_type);
            }
        }
        
        private Action<AreaType> onAreaTypeChanged;

        private void Start()
        {
            _buildingAreaManager = new BuildingAreaManager(areaLayerMask);
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
            _buildingAreaManager.UpdateMousePosition(_camera);
        }

        public void OnLeftMouseClicked()
        {
            _buildingAreaManager.SetArea(_camera, AreaType);
        }

        public void OnRightMouseClicked()
        {
            
        }

        public void RegisterForAreaTypeChanged(Action<AreaType> action)
        {
            onAreaTypeChanged += action;
        }
    
        public void UnregisterForAreaTypeChanged(Action<AreaType> action)
        {
            onAreaTypeChanged -= action;
        }
    }
}