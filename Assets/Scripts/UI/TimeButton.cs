using System.Collections;
using System.Collections.Generic;
using TimeSystem;
using UnityEngine;
using UnityEngine.UI;

public class TimeButton : MonoBehaviour
{
    [SerializeField] private TimeSpeed timeSpeed;
    [SerializeField] private ClockUi clockUi;

    public void OnClick()
    {
        clockUi.SetSpeed(timeSpeed);
    }
}
