using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ToggleButton : MonoBehaviour
{
    private PlayerController _player;
    [SerializeField] private GameMode gameMode;
    [SerializeField] private Image image;
    private bool _isActive;
    [SerializeField] private Color activatedColor;
    [SerializeField] private Color disabledColor;
    
    private void Start()
    {
        _player = PlayerController.Instance;
        _player.RegisterForInputModeChanged(OnInputModeChanged);
    }

    private void OnEnable()
    {
        if (_player != null) _player.RegisterForInputModeChanged(OnInputModeChanged);
    }

    private void OnDisable()
    {
        _player.UnregisterForInputModeChanged(OnInputModeChanged);
    }

    public void SetInputMode()
    {
        //Reset to default if already Selected
        _player.SetInputMode(_isActive ? GameMode.DefaultMode : gameMode);
    }

    private void OnInputModeChanged(GameMode mode)
    {
        if (mode == gameMode)
        {
            _isActive = true;
            image.color = activatedColor;
        }
        else if (mode != GameMode.UI)
        {
            _isActive = false;
            image.color = disabledColor;
        }
    }
}
