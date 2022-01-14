using System;
using PlayerInput;
using UnityEngine;
using UnityEngine.EventSystems;
using Util;

public class PlayerController : Singleton<PlayerController>
{
    [SerializeField] private float movementSpeed = 1f;
    [SerializeField] private float wheelSpeed = 1f;

    [SerializeField] private DefaultInputReceiver defaultInputReceiver;
    [SerializeField] private ConstructionInputReceiver constructionInputReceiver;
    [SerializeField] private BuildingAreaInputReceiver buildingAreaInputReceiver;
    
    private Transform _transform;
    private bool _currentlyInUiMode = false;

    private Action<GameMode> onGameModeChanged;

    private void Start()
    {
        _transform = GetComponent<Transform>();
    }

    private void Update()
    {
        UpdateCameraInput();

        if (EventSystem.current.IsPointerOverGameObject())
        {
            if (!_currentlyInUiMode)
            {
                _currentlyInUiMode = true;
                SetInputMode(GameMode.UI);
            }
        }
        else
        {
            if (_currentlyInUiMode)
            {
                _currentlyInUiMode = false;
                SetInputMode(GameManager.Instance.GameMode);
            }
        }

        if (Input.GetKeyDown(KeyCode.Alpha1)) SetInputMode(GameMode.DefaultMode);
        else if (Input.GetKeyDown(KeyCode.Alpha2)) SetInputMode(GameMode.ConstructionMode);
        else if (Input.GetKeyDown(KeyCode.Alpha3)) SetInputMode(GameMode.AreaMode);
    }

    private void UpdateCameraInput()
    {
        float frameSpeed = movementSpeed * Time.deltaTime;
        float mouseWheel = -Input.GetAxis("Mouse ScrollWheel") * wheelSpeed;

        _transform.position +=
            new Vector3
            (
                frameSpeed * Input.GetAxisRaw("Horizontal"),
                mouseWheel,
                frameSpeed * Input.GetAxisRaw("Vertical")
            );
    }

    public void SetInputMode(GameMode mode)
    {
        switch (mode)
        {
            case GameMode.DefaultMode:
                GameManager.Instance.GameMode = mode;
                constructionInputReceiver.enabled = false;
                buildingAreaInputReceiver.enabled = false;
                defaultInputReceiver.enabled = true;
                break;
            case GameMode.ConstructionMode:
                GameManager.Instance.GameMode = mode;
                defaultInputReceiver.enabled = false;
                buildingAreaInputReceiver.enabled = false;
                constructionInputReceiver.enabled = true;
                break;
            case GameMode.AreaMode:
                GameManager.Instance.GameMode = mode;
                defaultInputReceiver.enabled = false;
                constructionInputReceiver.enabled = false;
                buildingAreaInputReceiver.enabled = true;
                break;
            case GameMode.UI:
                defaultInputReceiver.enabled = false;
                constructionInputReceiver.enabled = false;
                buildingAreaInputReceiver.enabled = false;
                break;
        }
        
        onGameModeChanged?.Invoke(mode);
    }

    public void RegisterForInputModeChanged(Action<GameMode> action)
    {
        onGameModeChanged += action;
    }
    
    public void UnregisterForInputModeChanged(Action<GameMode> action)
    {
        onGameModeChanged -= action;
    }
}