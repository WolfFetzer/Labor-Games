using System;
using UnityEngine;

namespace TimeSystem
{
    public class GameTime : IComparable<GameTime>
    {
        public int Hour { get; private set; }
        public int Minute { get; private set; }
        public int Day { get; private set; }
        public int MinuteOfDay { get; private set; }
        public float PercentageOfDay { get; private set; }
        public DayNightType DayNightStatus { get; private set; } = DayNightType.Night;
        private Action OnDayNightStatusChanged;
    
        public GameTime()
        {
            
        }
    
        public GameTime(int hour, int minute, int day)
        {
            Hour = hour;
            Minute = minute;
            Day = day;

            MinuteOfDay = hour * 60 + Minute;
            PercentageOfDay = MinuteOfDay / 1440f;
        }
        
        public GameTime(int hour, int minute, int day, Action onDayNightStatusChanged) : this(hour, minute, day)
        {
            OnDayNightStatusChanged = onDayNightStatusChanged;
        }
    
        public void NextMinute()
        {
            Minute++;
            MinuteOfDay++;
            PercentageOfDay += 0.00069444f;
            
            if (Minute <= 59) return;
            
            Minute = 0;
            Hour++;
            CheckDayNightStatus();
            
            if (Hour <= 23) return;
            
            Hour = 0;
            MinuteOfDay = 0;
            PercentageOfDay = 0;
            Day++;
        }

        public void NextDay()
        {
            Day++;
        }

        private void CheckDayNightStatus()
        {
            if (Hour > 5 && Hour < 18)
            {
                if (DayNightStatus == DayNightType.Day) return;
                DayNightStatus = DayNightType.Day;
            }
            else
            {
                if (DayNightStatus == DayNightType.Night) return;
                DayNightStatus = DayNightType.Night;
            }
            
            OnDayNightStatusChanged?.Invoke();
        }
    
        public int CompareTo(GameTime other)
        {
            if (Day < other.Day) return -1;
            if (Day > other.Day) return 1;
            if (Hour < other.Hour) return -1;
            if (Hour > other.Hour) return 1;
            if (Minute < other.Minute) return -1;
            if (Minute > other.Minute) return 1;
            return 0;
        }
        
        public override string ToString()
        {
            return $"{Hour:00}:{Minute:00}";
        }
    }
}