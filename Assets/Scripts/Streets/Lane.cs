using System;
using System.Collections.Generic;
using UnityEngine;

namespace Streets
{
    [System.Serializable]
    public class Lane : IEquatable<Lane>
    {
        private static int IdCounter;
        
        public int _id;

        public Intersection _intersection;
        public Intersection Intersection
        {
            get => _intersection;
            private set
            {
                _intersection = value;
                IsConnected = _intersection != null;
            }
        }
        public bool IsConnected { get; private set; }
        public List<Vector3> WayPoints { get; }
        public StreetSegment Segment { get; }


        

        public Lane(List<Vector3> wayPoints, StreetSegment segment)
        {
            _id = IdCounter++;
            WayPoints = wayPoints;
            Segment = segment;
        }

        public void AddIntersection(Intersection intersectionToAdd, Intersection intersectionOtherDirection)
        {
            Intersection = intersectionToAdd;
            if (intersectionOtherDirection != null)
            {
                intersectionOtherDirection.AddLane(this);
            }
        }

        public override string ToString()
        {
            return $"{Segment}, {Intersection}, {IsConnected}";
        }

        public bool Equals(Lane other)
        {
            return _id == other._id;
        }
    }
}