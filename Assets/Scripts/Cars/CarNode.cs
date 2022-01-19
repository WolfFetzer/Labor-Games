using System.Collections.Generic;
using Streets;
using UnityEngine;

namespace Cars
{
    public class CarNode
    {
        public List<Vector3> WayPoints { get; }
        public int MaxSpeed { get; }
        public Lane Lane { get; }

        public CarNode(List<Vector3> wayPoints, int maxSpeed, Lane lane)
        {
            WayPoints = wayPoints;
            MaxSpeed = maxSpeed;
            //Lane = lane;
        }
    }
}