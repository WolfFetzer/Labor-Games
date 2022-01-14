using System;
using Buildings.Houses;
using Pathfinding;
using Streets;
using TimeSystem;
using UnityEngine;

namespace Population
{
    [Serializable]
    public class Human
    {
        [SerializeField] private string firstName;
        public string FirstName { get => firstName;
            private set => firstName = value;
        }
        
        [SerializeField] private string lastName;
        public string LastName { get => lastName;
            private set => lastName = value;
        }
        
        [SerializeField] private int age;
        public int Age { get => age;
            private set => age = value;
        }
        
        public ResidentialBuilding Home { get; set; }
        public WorkBuilding Workplace { get; set; }
        public Building CurrentPosition { get; set; }
        
        [SerializeField] private Gender gender;
        public Gender Gender { get => gender;
            private set => gender = value;
        }
        
        [SerializeField] private Occupation occupation;
        public Occupation Occupation 
        { 
            get => occupation;
            set => occupation = value;
        }
        

        public Human(string firstName, string lastName, int age, Gender gender)
        {
            FirstName = firstName;
            LastName = lastName;
            Age = age;
            Gender = gender;
        }

        public bool DriveHome()
        {
            if (CurrentPosition == null)
            {
                Debug.Log(ToString() + " is already driving and can't go home");
                return false;
            }

            PathNode path = PathFinder.CalculateShortestPath(CurrentPosition, Home);
            Car car = SpawnCar(CurrentPosition);
            Occupation = Occupation.Home;
            car.Move(path, Home, this);
            return true;
        }

        public bool DriveToWork()
        {
            if (CurrentPosition == null)
            {
                Debug.Log(ToString() + " is already driving and missed work");
                return false;
            }
            PathNode path = PathFinder.CalculateShortestPath(CurrentPosition, Workplace);
            Car car = SpawnCar(CurrentPosition);
            Occupation = Occupation.Working;
            car.Move(path, Workplace, this);
            return true;
        }
        
        public bool DriveShopping()
        {
            if (CurrentPosition == null)
            {
                Debug.Log(ToString() + " is already driving and can't go shopping");
                return false;
            }
            Building randomBuilding = PopulationManager.Instance.GetRandomCommercialBuilding();
            if (randomBuilding == null) return false;
            
            PathNode path = PathFinder.CalculateShortestPath(CurrentPosition, randomBuilding);
            Car car = SpawnCar(CurrentPosition);
            Occupation = Occupation.Shopping;
            car.Move(path, randomBuilding, this);
            return true;
        }

        private Car SpawnCar(Building building)
        {
            Car car = UnityEngine.Object.Instantiate(GameManager.Instance.carPrefab, building.transform.position, Quaternion.identity).GetComponent<Car>();
            CurrentPosition.LeaveBuilding(this);
            CurrentPosition = null;
            return car;
        }

        public override string ToString()
        {
            return $"{FirstName} {LastName}, Age: {Age}, {Gender}";
        }
    }

    public enum Gender
    {
        Male, Female
    }

    public enum Occupation
    {
        Home,
        Shopping,
        Working
    }
}