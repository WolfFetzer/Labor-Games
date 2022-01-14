using System;
using System.Collections.Generic;
using Population;
using TimeSystem;

namespace Buildings.Houses
{
    public class CommercialBuilding : WorkBuilding
    {
        private List<Human> _customers = new List<Human>();


        private void Start()
        {
            PopulationManager.Instance.AddCommercialBuildingForCustomers(this);
        }

        public override void EnterBuilding(Human human)
        {
            if (human.Occupation == Occupation.Working)
            {
                presentWorkers.Add(human);
            }
            else if (human.Occupation == Occupation.Shopping)
            {
                _customers.Add(human);
            }

            AddCustomerTime();

            human.CurrentPosition = this;
        }

        public override void LeaveBuilding(Human human)
        {
            if (human.Occupation == Occupation.Shopping)
            {
                _customers.Remove(human);
            }
            else
            {
                presentWorkers.Remove(human);
            }
        }

        private void RemoveCustomer()
        {
            if (_customers.Count > 0)
            {
                _customers[0].DriveHome();
            }
        }

        private void AddCustomerTime()
        {
            GameTime currentTime = TimeManager.Instance.GameTime;
            int hour = currentTime.Hour + 1;
            int day = currentTime.Day;
            if (hour > 23)
            {
                hour = 0;
                day++;
            }

            TimedTask task = new TimedTask(new GameTime(hour, currentTime.Minute, day), RemoveCustomer);
            TimeManager.Instance.AddTimedTask(task);
        }

        private void OnDisable()
        {
            foreach (Human customer in _customers)
            {
                customer.DriveHome();
            }
            
            CloseWorkBuilding();
        }
    }
}