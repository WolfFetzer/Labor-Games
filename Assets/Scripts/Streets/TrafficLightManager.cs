using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Util;

public class TrafficLightManager : Singleton<TrafficLightManager>
{
    [Range(1, 20)]
    [SerializeField] private int switchTime = 5;
    [Range(1, 10)]
    [SerializeField] private int waitTime = 2;

    public bool IsWaitingTime { get; private set; }
    private bool _isWaiting;
    private int time;
    
    public Material redMaterial;
    public Material yellowMaterial;
    public Material greenMaterial;

    private int _currentTime;
    public Action OnSwitchTrafficLights;
    
    private void TimeUpdate()
    {
        _currentTime++;
        _currentTime %= time;
        if (_currentTime != 0) return;

        if (IsWaitingTime)
        {
            time = switchTime;
        }
        else
        {
            time = waitTime;
        }

        IsWaitingTime = !IsWaitingTime;
        
        OnSwitchTrafficLights?.Invoke();
    }
    
    private void Start()
    {
        time = switchTime;
        TimeManager.Instance.Register(TimeUpdate);
    }

    private void OnDisable()
    {
        TimeManager.Instance.Unregister(TimeUpdate);
    }
}
