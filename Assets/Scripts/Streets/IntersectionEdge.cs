using UnityEngine;
using System.Collections.Generic;

namespace Streets
{
    public class IntersectionEdge
    {
        public List<Vector3> WayPoints { get; }
        public Intersection Intersection { get; }
        public StreetSegment Segment { get; }

        public IntersectionEdge(Intersection intersection, StreetSegment segment, List<Vector3> wayPoints)
        {
            Intersection = intersection;
            Segment = segment;
            WayPoints = wayPoints;
        }
    }
}