using System;
using Pathfinding;
using Population;
using UnityEngine;

namespace TimeSystem
{
    public class TimedTask : IComparable<TimedTask>
    {
        private static int Counter;

        private int ID;
        public GameTime GameTime { get; }
        public bool IsDaily { get; }
        private Action _action;

        public TimedTask(GameTime time, Action action, bool isDaily = false)
        {
            GameTime = time;
            _action = action;
            IsDaily = isDaily;
            ID = Counter++;
        }

        public void Execute()
        {
            _action.Invoke();
        }

        public int CompareTo(TimedTask other)
        {
            int i = GameTime.CompareTo(other.GameTime);
            if (i == 0)
            {
                if (ID == other.ID) return 0;
                return -1;
            }
            return i;
        }

        public override string ToString()
        {
            return GameTime.ToString();
        }
    }
}