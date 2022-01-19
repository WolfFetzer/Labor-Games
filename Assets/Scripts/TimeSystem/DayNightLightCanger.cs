using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DayNightLightCanger : MonoBehaviour
{
    public List<LightSlot> LightSlots;

    private void Start()
    {
        TimeManager.Instance.RegisterForDayNightUpdate(OnDayTime, OnNightTime);
        if (TimeManager.Instance.GameTime.DayNightStatus == DayNightType.Day) OnDayTime(); 
        else OnNightTime();
    }

    private void OnDayTime()
    {
        foreach (LightSlot slot in LightSlots)
        {
            slot.light.enabled = false;
        }
    }

    private void OnNightTime()
    {
        foreach (LightSlot slot in LightSlots)
        {
            slot.light.enabled = true;
        }
    }

    private void OnDisable()
    {
        TimeManager.Instance.UnregisterForDayNightUpdate(OnDayTime, OnNightTime);
    }
}

[System.Serializable]
public class LightSlot
{
    public Light light;
}
