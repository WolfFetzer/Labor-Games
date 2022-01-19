using System.Collections.Generic;
using Cars;
using Curves.Util;
using Streets;
using UnityEngine;

namespace Pathfinding
{
    public class PathNode
    {
        public float fCost = float.MaxValue;
        public float gCost;
        public float hCost;

        private Vector2 Position => Edge.Intersection.transform.position;
        public PathNode Parent { get; set; }

        private Lane _edge;

        public Lane Edge
        {
            get => _edge;
            set
            {
                _edge = value;
                Waypoints = _edge.WayPoints;
            }
        }

        public List<Vector3> Waypoints { get; private set; }


        public PathNode(Lane edge)
        {
            Edge = edge;
        }
        
        public PathNode(List<Vector3> waypoints, Lane edge)
        {
            _edge = edge;
            Waypoints = waypoints;
            //Edge = new Lane(waypoints, edge.Segment);
            //Edge.AddIntersection(edge.Intersection, null);
        }

        public void Reset()
        {
            gCost = 0;
            hCost = 0;
            fCost = int.MaxValue;
            Parent = null;
        }

        public void CalculateCost(PathNode target)
        {
            gCost = 0;
            CalculateHCost(target);
            fCost = gCost + hCost;
        }

        public void CalculateHCost(PathNode target)
        {
            hCost = (int) (5f * (Position - target.Position).magnitude);
        }

        public string WritePath()
        {
            return Parent != null ? Parent.WritePath() + ", " + Edge.Intersection.name : Edge.Intersection.name;
        }

        public Stack<CarNode> GetWaypoints()
        {
            Stack<CarNode> stack = new Stack<CarNode>();
            PathNode currentNode = this;
            while (currentNode != null)
            {
                stack.Push(new CarNode(currentNode.Waypoints, currentNode.Edge.Segment.Info.speed, currentNode.Edge));
                if (currentNode.Parent != null) stack.Push(CreateCurveWaypoints(currentNode));
                currentNode = currentNode.Parent;
            }
            
            return stack;
        }

        private CarNode CreateCurveWaypoints(PathNode node)
        {
            List<Vector3> nodePoints = new List<Vector3>();
            List<Vector3> parPoints = node.Parent.Waypoints;
            StreetSegment segment = node.Parent.Edge.Segment;
            Vector3 dir1 = (parPoints[parPoints.Count - 1] - parPoints[parPoints.Count - 2]).normalized;
            Vector3 point1 = parPoints[parPoints.Count - 1] + dir1 * (segment.Info.lanes * segment.Info.trackWidth);

            parPoints = node.Waypoints;
            segment = node.Edge.Segment;
            Vector3 dir2 = (parPoints[0] - parPoints[1]).normalized;
            Vector3 point2 = parPoints[0] + dir2 * (segment.Info.lanes * segment.Info.trackWidth);

                    
            Vector3 dir = point2 - point1;
            Vector3 normal = (dir1 + dir2).normalized;
            Vector3 handle = point1 + 0.5f * dir + dir.magnitude * 0.5f * normal;

            nodePoints.Add(point1);
            nodePoints.Add(BezierUtil.QuadraticLerp(point1, handle, point2, 0.25f));
            nodePoints.Add(BezierUtil.QuadraticLerp(point1, handle, point2, 0.5f));
            nodePoints.Add(BezierUtil.QuadraticLerp(point1, handle, point2, 0.75f));
            nodePoints.Add(point2);
            
            return new CarNode(nodePoints, 20, node.Edge);
        }
    }
}