using System;
using Population;
using UnityEngine;

namespace Buildings.Houses
{
    public class IndustrialBuilding : WorkBuilding
    {
        public override void EnterBuilding(Human human)
        {
            if (human.Occupation == Occupation.Working)
            {
                presentWorkers.Add(human);
            }
            
            Debug.Log(human + " entered the Industrialbuilding");

            human.CurrentPosition = this;
        }

        public override void LeaveBuilding(Human human)
        {
            presentWorkers.Remove(human);
        }
    }
}