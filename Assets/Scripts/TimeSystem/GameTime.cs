using System;

namespace TimeSystem
{
    public class GameTime : IComparable<GameTime>
    {
        public int Hour { get; private set; }
        public int Minute { get; private set; }
        public int Day { get; private set; }
    
        public GameTime()
        {
            
        }
    
        public GameTime(int hour, int minute, int day)
        {
            Hour = hour;
            Minute = minute;
            Day = day;
        }
    
        public void NextMinute()
        {
            Minute++;
            if (Minute > 59)
            {
                Minute = 0;
                Hour++;
                if (Hour > 23)
                {
                    Hour = 0;
                    Day++;
                }
            }
        }

        public void NextDay()
        {
            Day++;
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