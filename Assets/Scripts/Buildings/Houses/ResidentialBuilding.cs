using System;
using System.Collections.Generic;
using Population;
using UnityEngine;

namespace Buildings.Houses
{
    public class ResidentialBuilding : Building
    {
        [SerializeField] private int maxResidents = 4;
        [SerializeField] private List<Human> residents;
        [SerializeField] private List<Human> presentResidents;

        public bool HasSpace => residents.Count < maxResidents;

        private void Awake()
        {
            residents = new List<Human>(maxResidents);
            presentResidents = new List<Human>(maxResidents);
        }

        /// <summary>
        /// Used if a new human lives in this residential building now.
        /// </summary>
        public void MoveIn(Human human)
        {
            residents.Add(human);
            human.Home = this;
            EnterBuilding(human);
        }

        public void MoveOut(Human human)
        {
            residents.Remove(human);
            human.Home = null;
        }
        
        public override void EnterBuilding(Human human)
        {
            Debug.Log( human + " entered residential building");
            presentResidents.Add(human);
            human.CurrentPosition = this;
            
            PopulationManager.Instance.AddIdleHuman(human);
        }

        public override void LeaveBuilding(Human human)
        {
            presentResidents.Remove(human);
        }
    }
}