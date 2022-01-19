using System;
using System.Collections;
using System.Collections.Generic;
using TimeSystem;
using TMPro;
using UnityEngine;

public class ClockUi : MonoBehaviour
{
    public TextMeshProUGUI textMesh;
    
    private void TimeUpdate()
    {
        textMesh.text = $"{TimeManager.Instance.GameTime.Hour:00}:{TimeManager.Instance.GameTime.Minute:00}";
    }
    
    private void Start()
    {
        TimeManager.Instance?.Register(TimeUpdate);
    }

    private void OnDisable()
    {
        TimeManager.Instance?.Unregister(TimeUpdate);
    }

    public void SetSpeed(TimeSpeed timeSpeed)
    {
        TimeManager.Instance.SetSpeed(timeSpeed);
    }
}
