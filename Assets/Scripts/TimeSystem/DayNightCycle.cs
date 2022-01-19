using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DayNightCycle : MonoBehaviour
{
    [SerializeField] private Light sun;
    [SerializeField] private Light moon;
    private Transform _transform;

    private void Start()
    {
        _transform = transform;
        _transform.rotation = Quaternion.Euler(0.25f * TimeManager.Instance.GameTime.MinuteOfDay - 90f, 0f, 0f);
        TimeManager.Instance.Register(TimeUpdate);
        TimeManager.Instance.RegisterForDayNightUpdate(OnDayTime, OnNightTime);
        if (TimeManager.Instance.GameTime.DayNightStatus == DayNightType.Day) OnDayTime(); 
        else OnNightTime();
    }

    private void TimeUpdate()
    {
        _transform.Rotate(Vector3.right, 0.25f);
    }

    private void OnDayTime()
    {
        moon.shadows = LightShadows.None;
        sun.shadows = LightShadows.Soft;
    }

    private void OnNightTime()
    {
        sun.shadows = LightShadows.None;
        moon.shadows = LightShadows.Soft;
    }

    private void OnDisable()
    {
        TimeManager.Instance?.Unregister(TimeUpdate);
        TimeManager.Instance?.UnregisterForDayNightUpdate(OnDayTime, OnNightTime);
    }
}

public enum DayNightType
{
    Day, Night
}
