using System.Collections.Generic;
using Streets;
using UnityEngine;

namespace Pathfinding
{
    public class PathNode
    {
        public float fCost = float.MaxValue;
        public float gCost;
        public float hCost;

        private Vector2 Position => Intersection.transform.position;
        public PathNode Parent { get; set; }
        public Intersection Intersection { get; }
        public IntersectionEdge Edge { get; set; }
        
        
        public PathNode(Intersection intersection)
        {
            Intersection = intersection;
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
            return Parent != null ? Parent.WritePath() + ", " + Intersection.name : Intersection.name;
        }

        public Stack<IntersectionEdge> GetWaypoints()
        {
            Stack<IntersectionEdge> stack = new Stack<IntersectionEdge>();

            PathNode currentNode = this;
            while (currentNode != null)
            {
                /*currentNode.edge.wayPoints*/
                stack.Push(currentNode.Edge);
                currentNode = currentNode.Parent;
            }

            return stack;
        }
    }
}