using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DayNightMaterialChanger : MonoBehaviour
{
    public List<ChangeableMaterialSlot> MaterialSlots;
    private MeshRenderer _renderer;

    private void Start()
    {
        _renderer = GetComponent<MeshRenderer>();
        TimeManager.Instance.RegisterForDayNightUpdate(OnDayTime, OnNightTime);
        if (TimeManager.Instance.GameTime.DayNightStatus == DayNightType.Day) OnDayTime(); 
        else OnNightTime();
    }

    private void OnDayTime()
    {
        Material[] materials = _renderer.materials;
        foreach (ChangeableMaterialSlot slot in MaterialSlots)
        {
            materials[slot.index] = slot.materialWithoutLight;
        }

        _renderer.materials = materials;
    }

    private void OnNightTime()
    {
        Material[] materials = _renderer.materials;
        foreach (ChangeableMaterialSlot slot in MaterialSlots)
        {
            materials[slot.index] = slot.materialWithLight;
        }

        _renderer.materials = materials;
    }

    private void OnDisable()
    {
        TimeManager.Instance.UnregisterForDayNightUpdate(OnDayTime, OnNightTime);
    }
}

[System.Serializable]
public class ChangeableMaterialSlot
{
    public int index;
    public Material materialWithoutLight;
    public Material materialWithLight;
}
