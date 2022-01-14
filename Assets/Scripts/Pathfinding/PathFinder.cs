using System;
using System.Collections;
using System.Collections.Generic;
using Pathfinding;
using Streets;
using UnityEngine;

[Serializable]
public static class PathFinder
{
    public static PathNode CalculateShortestPath(Building start, Building target)
    {
        //Check if start and target are on the same street
        if (start.Field.Parent.StreetSegment == target.Field.Parent.StreetSegment) 
            return GetPathOnSameStreetSegment(start, target);
        
        //Otherwise check if the street is connected to an intersection
        if (!start.Field.Parent.StreetSegment.IsConnected) 
            return null;
        
        //Calculate the path on the street system
        PathNode startingPathNode = GetStartingPathNode(start);
        StreetSegment segment = target.Field.Parent.StreetSegment;

        PathNode targetNode;
        bool tookFirst = false;

        if (segment.IntersectionAtStart != null)
        {
            targetNode = new PathNode(segment.IntersectionAtStart);
            tookFirst = true;
        }
        else if (segment.IntersectionAtEnd != null)
        {
            targetNode = new PathNode(segment.IntersectionAtEnd);
        }
        else return null;

        PathNode path = CalculateShortestPath(startingPathNode, targetNode);
        
        //Check if went one intersection to far
        if (tookFirst && segment.IntersectionAtEnd != null && path.Parent.Intersection == segment.IntersectionAtEnd)
        {
            path = path.Parent;
        }

        PathNode finalNode = GetEndingPathNode(target, path);
        finalNode.Parent = path;

        return finalNode;
    }

    private static PathNode GetPathOnSameStreetSegment(Building start, Building target)
    {
        List<Vector3> waypoints;
        if (start.Field.Parent == target.Field.Parent)
        {
            //Only drive to the right
            if (start.Field.Index < target.Field.Index)
            {
                waypoints = GetBuildingExitPointsWithoutCrossing(start.transform, start.Field.Parent.StreetSegment);
                AddBuildingEnterPointsWithoutCrossing(ref waypoints, target.transform, target.Field.Parent.StreetSegment);
            }
            //Cross the street twice
            else
            {
                waypoints = GetBuildingExitPointsWithCrossing(start.transform, start.Field.Parent.StreetSegment);
                AddBuildingEnterPointsWithCrossing(ref waypoints, target.transform, target.Field.Parent.StreetSegment);
            }
        }
        else
        {
            if (start.Field.Index <= target.Field.Parent.FieldAmount - (target.Field.Index + 1))
            {
                waypoints = GetBuildingExitPointsWithoutCrossing(start.transform, start.Field.Parent.StreetSegment);
                AddBuildingEnterPointsWithCrossing(ref waypoints, target.transform, target.Field.Parent.StreetSegment);
            }
            else
            {
                waypoints = GetBuildingExitPointsWithCrossing(start.transform, start.Field.Parent.StreetSegment);
                AddBuildingEnterPointsWithoutCrossing(ref waypoints, target.transform, target.Field.Parent.StreetSegment);
            }
        }

        return new PathNode(null) {Edge = new IntersectionEdge(null, start.Field.Parent.StreetSegment, waypoints)};
    }

    private static PathNode GetStartingPathNode(Building start)
    {
        Intersection intersection;
        StreetSegment segment = start.Field.Parent.StreetSegment;
        List<Vector3> startingPoints;
        Transform startTransform = start.transform;

        if (start.Field.Parent.IsRightSide)
        {
            if (segment.IntersectionAtEnd != null)
            {
                intersection = segment.IntersectionAtEnd;
                startingPoints = GetBuildingExitPointsWithoutCrossing(startTransform, segment);
                startingPoints.Add(segment.edge1[segment.edge1.Count - 1]);
            }
            else
            {
                intersection = segment.IntersectionAtStart;
                startingPoints = GetBuildingExitPointsWithCrossing(startTransform, segment);
                startingPoints.Add(segment.edge2[segment.edge2.Count - 1]);
            }
        }
        else
        {
            if (segment.IntersectionAtStart != null)
            {
                intersection = segment.IntersectionAtStart;
                startingPoints = GetBuildingExitPointsWithoutCrossing(startTransform, segment);
                startingPoints.Add(segment.edge2[segment.edge2.Count - 1]);
            }
            else
            {
                intersection = segment.IntersectionAtEnd;
                startingPoints = GetBuildingExitPointsWithCrossing(startTransform, segment);
                startingPoints.Add(segment.edge1[segment.edge1.Count - 1]);
            }
        }
        
        return new PathNode(intersection) {Edge = new IntersectionEdge(intersection, segment, startingPoints)};
    }

    private static List<Vector3> GetBuildingExitPointsWithoutCrossing(Transform transform, StreetSegment segment)
    {
        List<Vector3> exitPoints = new List<Vector3>();
        Vector3 position = transform.position;
        exitPoints.Add(position);
        exitPoints.Add(position + transform.forward * (segment.Info.trackWidth * 0.5f));

        return exitPoints;
    }
    
    private static List<Vector3> GetBuildingExitPointsWithCrossing(Transform transform, StreetSegment segment)
    {
        List<Vector3> exitPoints = new List<Vector3>();
        Vector3 position = transform.position;
        exitPoints.Add(position);
        exitPoints.Add(position + transform.forward * (segment.Info.lanes * segment.Info.trackWidth + segment.Info.trackWidth * 0.5f));

        return exitPoints;
    }

    private static PathNode GetEndingPathNode(Building target, PathNode pathNode)
    {
        Transform targetTransform = target.transform;
        List<Vector3> lastPoints = new List<Vector3>();

        Vector3 intersectionPosition = pathNode.Intersection.transform.position;
        StreetSegment segment = target.Field.Parent.StreetSegment;
        
        if (Vector3.Distance(intersectionPosition, segment.edge1[0]) < Vector3.Distance(intersectionPosition, segment.edge2[0]))
        {
            lastPoints.Add(segment.edge1[0]);
            
            if (target.Field.Parent.IsRightSide)
            {
                AddBuildingEnterPointsWithoutCrossing(ref lastPoints, targetTransform, segment);
            }
            else
            {
                AddBuildingEnterPointsWithCrossing(ref lastPoints, targetTransform, segment);
            }
        }
        else
        {
            lastPoints.Add(segment.edge2[0]);
            
            if (target.Field.Parent.IsRightSide)
            {
                AddBuildingEnterPointsWithCrossing(ref lastPoints, targetTransform, segment);
            }
            else
            {
                AddBuildingEnterPointsWithoutCrossing(ref lastPoints, targetTransform, segment);
            }
        }

        return new PathNode(null) {Edge = new IntersectionEdge(null, target.Field.Parent.StreetSegment, lastPoints)};
    }

    private static void AddBuildingEnterPointsWithoutCrossing(ref List<Vector3> listToAddTo, Transform target, StreetSegment segment)
    {
        Vector3 targetPos = target.position;
        listToAddTo.Add(targetPos + target.forward * (segment.Info.trackWidth * 0.5f));
        listToAddTo.Add(targetPos);
    }
    
    private static void AddBuildingEnterPointsWithCrossing(ref List<Vector3> listToAddTo, Transform target, StreetSegment segment)
    {
        Vector3 targetPos = target.position;
        listToAddTo.Add(targetPos + target.forward * (segment.Info.lanes * segment.Info.trackWidth + segment.Info.trackWidth * 0.5f));
        listToAddTo.Add(targetPos);
    }

    private static PathNode CalculateShortestPath(PathNode start, PathNode target)
    {
        List<PathNode> openList = new List<PathNode>();
        Dictionary<Intersection, bool> closedDict = new Dictionary<Intersection, bool>();

        start.CalculateCost(target);

        openList.Add(start);
        PathNode currentNode = start;

        int index = 0;
        while (index < 1000)
        {
            index++;
            float currentLowestFCost = float.MaxValue;
            foreach (PathNode node in openList)
                if (node.fCost < currentLowestFCost)
                {
                    currentLowestFCost = node.fCost;
                    currentNode = node;
                }
            
            openList.Remove(currentNode);
            
            if (!closedDict.ContainsKey(currentNode.Intersection))
            {
                closedDict.Add(currentNode.Intersection, true);
            }

            //Found
            if (currentNode.Intersection == target.Intersection)
            {
                return currentNode;
            }

            foreach (IntersectionEdge edge in currentNode.Intersection.Edges)
            {
                if (closedDict.ContainsKey(edge.Intersection)) continue;

                PathNode node  = new PathNode(edge.Intersection);
                float newPath = currentNode.gCost + edge.Segment.Cost;
                
                node.CalculateHCost(target);
                float newFCost = newPath + node.hCost;

                if (newFCost < node.fCost || !openList.Contains(node))
                {
                    node.fCost = newFCost;
                    node.gCost = newPath;
                    node.Parent = currentNode;
                    node.Edge = edge;

                    if (!openList.Contains(node)) openList.Add(node);
                }
            }
        }

        Debug.Log("Not found");
        return null;
    }
}
