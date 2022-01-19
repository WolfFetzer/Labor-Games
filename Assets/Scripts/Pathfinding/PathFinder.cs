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

        if (segment.Lane1.IsConnected)
        {
            targetNode = new PathNode(segment.Lane1);
            tookFirst = true;
        }
        else if (segment.Lane2.IsConnected)
        {
            targetNode = new PathNode(segment.Lane2);
        }
        else return null;

        PathNode path = CalculateShortestPath(startingPathNode, targetNode);
        
        //Check if went one intersection to far
        if (tookFirst && segment.Lane2.IsConnected && path.Parent != null && path.Parent.Edge.Intersection == segment.Lane2.Intersection)
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

        
        return new PathNode(waypoints, start.Field.Parent.StreetSegment.Lane1);
    }

    private static PathNode GetStartingPathNode(Building start)
    {
        Lane edge;
        StreetSegment segment = start.Field.Parent.StreetSegment;
        List<Vector3> startingPoints;
        Transform startTransform = start.transform;

        if (start.Field.Parent.IsRightSide)
        {
            if (segment.Lane1.IsConnected)
            {
                edge = segment.Lane1;
                startingPoints = GetBuildingExitPointsWithoutCrossing(startTransform, segment);
            }
            else
            {
                edge = segment.Lane2;
                startingPoints = GetBuildingExitPointsWithCrossing(startTransform, segment);
            }
        }
        else
        {
            if (segment.Lane2.IsConnected)
            {
                edge = segment.Lane2;
                startingPoints = GetBuildingExitPointsWithoutCrossing(startTransform, segment);
            }
            else
            {
                edge = segment.Lane1;
                startingPoints = GetBuildingExitPointsWithCrossing(startTransform, segment);
            }
        }
        
        startingPoints.Add(edge.WayPoints[edge.WayPoints.Count - 1]);
        return new PathNode(startingPoints, edge);
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

        Vector3 intersectionPosition = pathNode.Edge.Intersection.transform.position;
        StreetSegment segment = target.Field.Parent.StreetSegment;
        
        if (Vector3.Distance(intersectionPosition, segment.Lane1.WayPoints[0]) < Vector3.Distance(intersectionPosition, segment.Lane2.WayPoints[0]))
        {
            lastPoints.Add(segment.Lane1.WayPoints[0]);
            
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
            lastPoints.Add(segment.Lane2.WayPoints[0]);
            
            if (target.Field.Parent.IsRightSide)
            {
                AddBuildingEnterPointsWithCrossing(ref lastPoints, targetTransform, segment);
            }
            else
            {
                AddBuildingEnterPointsWithoutCrossing(ref lastPoints, targetTransform, segment);
            }
        }

        return new PathNode(lastPoints, segment.Lane1);
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
            
            if (!closedDict.ContainsKey(currentNode.Edge.Intersection))
            {
                closedDict.Add(currentNode.Edge.Intersection, true);
            }

            //Found
            if (currentNode.Edge.Intersection == target.Edge.Intersection)
            {
                return currentNode;
            }

            foreach (Lane edge in currentNode.Edge.Intersection.Edges)
            {
                if(!edge.IsConnected) continue;
                if (closedDict.ContainsKey(edge.Intersection)) continue;

                PathNode node  = new PathNode(edge);
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
