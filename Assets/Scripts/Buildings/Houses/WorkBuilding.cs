using System;
using System.Collections.Generic;
using Population;
using TimeSystem;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Buildings.Houses
{
    public abstract class WorkBuilding : Building
    {
        [SerializeField] protected int maxWorkers = 6;
        [SerializeField] protected List<Human> workers;
        [SerializeField] protected List<Human> presentWorkers;
        public int StartHour { get; private set; }
        public int EndHour { get; private set; }

        public bool HasWork => workers.Count < maxWorkers;

        private TimedTask _startWorkTask;
        private TimedTask _endWorkTask;

        private void Awake()
        {
            workers = new List<Human>(maxWorkers);
            presentWorkers = new List<Human>(maxWorkers);
            StartHour = Random.Range(5, 12);
            EndHour = StartHour + 8;

            _startWorkTask = new TimedTask(new GameTime(StartHour, 0, TimeManager.Instance.GameTime.Day + 1),
                StartWorkDay,
                true);
            
            TimeManager.Instance.AddTimedTask(_startWorkTask);

            _endWorkTask = new TimedTask(new GameTime(EndHour, 0, TimeManager.Instance.GameTime.Day + 1), 
                EndWorkDay,
                true);
            
            TimeManager.Instance.AddTimedTask(_endWorkTask);
        }

        private void StartWorkDay()
        {
            foreach (Human worker in workers)
            {
                worker.DriveToWork();
            }
        }

        private void EndWorkDay()
        {
            foreach (Human presentWorker in presentWorkers)
            {
                presentWorker.DriveHome();
            }
            
            presentWorkers.Clear();
        }

        protected void CloseWorkBuilding()
        {
            TimeManager.Instance.RemoveTimedTask(_endWorkTask);
            
            foreach (Human worker in workers)
            {
                workers.Remove(worker);
                PopulationManager.Instance.AddUnemployed(worker);
            }
            
            EndWorkDay();
        }

        public void Employ(Human human)
        {
            workers.Add(human);
            human.Workplace = this;
            //Debug.Log("[WorkBuilding] Employed: " + human);
        }
    }
}