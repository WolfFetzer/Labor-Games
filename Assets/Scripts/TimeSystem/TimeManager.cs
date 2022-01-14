using System;
using System.Collections;
using System.Collections.Generic;
using Pathfinding;
using Population;
using TimeSystem;
using UnityEngine;
using Util;

public class TimeManager : Singleton<TimeManager>
{
    private bool _isPaused = true;
    public GameTime GameTime { get; } = new GameTime();

    private Action OnTimeChanged;
    private Action<float> OnSpeedChanged;

    private float _realTimeRatio = 0.5f;
    private float _currentTimeProgression;

    private SortedSet<TimedTask> scheduledTasks = new SortedSet<TimedTask>();

    public void SetSpeed(TimeSpeed timeSpeed)
    {
        switch (timeSpeed)
        {
            case TimeSpeed.Paused:
                _isPaused = true;
                Car.SpeedMultiplier = 0f;
                return;
            case TimeSpeed.Default:
                _realTimeRatio = 0.5f;
                Car.SpeedMultiplier = 2f;
                break;
            case TimeSpeed.TwoTimes:
                _realTimeRatio = 0.25f;
                Car.SpeedMultiplier = 4f;
                break;
            case TimeSpeed.FourTimes:
                _realTimeRatio = 0.125f;
                Car.SpeedMultiplier = 8f;
                break;
        }
        
        _isPaused = false;
    }

    private void FixedUpdate()
    {
        if (_isPaused) return;
        
        _currentTimeProgression += Time.fixedDeltaTime;
        if (_currentTimeProgression >= _realTimeRatio)
        {
            _currentTimeProgression -= _realTimeRatio;
            GameTime.NextMinute();
            OnTimeChanged?.Invoke();

            if (scheduledTasks.Count > 0 && scheduledTasks.Min.GameTime.CompareTo(GameTime) < 1)
            {
                TimedTask task = scheduledTasks.Min;
                scheduledTasks.Remove(task);
                task.Execute();
                if (task.IsDaily)
                {
                    task.GameTime.NextDay();
                    AddTimedTask(task);
                }
            }
        }
    }

    public void AddTimedTask(TimedTask task)
    {
        Debug.Log("Added Task. Time at " + task.GameTime);
        scheduledTasks.Add(task);
    }

    public bool RemoveTimedTask(TimedTask task)
    {
        return scheduledTasks.Remove(task);
    }

    public void Register(Action action)
    {
        OnTimeChanged += action;
    }

    public void Unregister(Action action)
    {
        OnTimeChanged -= action;
    }
}


