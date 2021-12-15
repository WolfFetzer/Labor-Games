using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class PathManager : MonoBehaviour
{
    //Debug only
    /*
    public StreetSegment startSegment;
    public StreetSegment targetSegment;
    public bool startAtOne = true;
    public Car car;
    */
    
    public bool update = false;

    public static List<Car> Cars = new List<Car>();

    public static void RemoveCar(Car car)
    {
        if (Cars.Contains(car))
        {
            Cars.Remove(car);
            Destroy(car.gameObject);
            Debug.Log("Destroyed");
        }
    }

    private void FixedUpdate()
    {
        if (update)
        {
            update = false;

            int count = Intersection.Intersections.Count;
            if (count < 2)
            {
                Debug.Log("Not enough intersections");
                return;
            }
            
            
            Debug.Log("Spawn");
            int random = (int) (Random.value * count);
            Intersection startIntersection = Intersection.Intersections[random];

            int random2;
            int i = 0;
            do
            {
                random2 = (int) (Random.value * count);
            } while (random == random2 || i++ < 10);

            Intersection targetIntersection = Intersection.Intersections[random2];
            
            
            PathGraph pg = new PathGraph();
            
            PathNode p = 
                pg.CalculateShortestWay(new PathNode(startIntersection), new PathNode(targetIntersection));

            Debug.Log(p.WritePath());
            
            Car car = 
                Instantiate(GameManager.Instance.carPrefab, startIntersection.transform.position, Quaternion.identity).GetComponent<Car>();
            car.Move(p);
            Cars.Add(car);

            /*
            if (startSegment == null)
            {
                Debug.Log("Select a Street Segment");
                return;
            }

            if (car != null)
            {
                Destroy(car.gameObject);
            }

            Vector3 pos;
            Intersection startIntersection;
            
            if (startAtOne)
            {
                pos = startSegment.edge1[0] + 0.5f * (startSegment.edge1[1] - startSegment.edge1[0]);
                startIntersection = GetStartIntersection(1, 0);
            }
            else
            {
                pos = startSegment.edge2[0] + 0.5f * (startSegment.edge2[1] - startSegment.edge2[0]);
                startIntersection = GetStartIntersection(0, 1);
            }

            Debug.Log("Startintersection: " + startIntersection);

            PathGraph pg = new PathGraph();

            Intersection targetIntersection;
            if (targetSegment.Intersections[0] != null)
            {
                targetIntersection = targetSegment.Intersections[0];
            }
            else
            {
                targetIntersection = targetSegment.Intersections[1];
            }
            
            PathNode p = 
                pg.CalculateShortestWay(new PathNode(startIntersection), new PathNode(targetIntersection));

            Debug.Log(p.WritePath());
            
            car = 
                Instantiate(GameManager.Instance.carPrefab, pos, Quaternion.identity).GetComponent<Car>();
            car.Move(p);
            */
        }
    }

    
    private Intersection GetStartIntersection(int firstChoice, int secondChoice)
    {
        /*
        if (startSegment.Intersections[firstChoice] != null)
        {
            return startSegment.Intersections[firstChoice];
        }
        if (startSegment.Intersections[secondChoice] != null)
        {
            return startSegment.Intersections[secondChoice];
        }
        */

        return null;
    }

    ////////////////////////////////////////////////////////////////////////////////////////////////////////
    
    
}

[Serializable]
public class PathGraph
{
    public PathNode way;

    public PathNode CalculateShortestWay(PathNode a, PathNode b)
    {
        List<PathNode> openList = new List<PathNode>();
        //List<PathNode> closedList = new List<PathNode>();
        Dictionary<Intersection, bool> closedDict = new Dictionary<Intersection, bool>();
        Dictionary<Intersection, PathNode> nodeDict = new Dictionary<Intersection, PathNode>();

        a.CalculateCost(b);

        openList.Add(a);
        PathNode currentNode = a;

        var index = 0;
        while (index < 1000)
        {
            index++;
            float currentLowestFCost = float.MaxValue;
            foreach (var node in openList)
                if (node.fCost < currentLowestFCost)
                {
                    currentLowestFCost = node.fCost;
                    currentNode = node;
                }

            Debug.Log("Index currentNode: " + currentNode.intersection.name + ", Lowest fCost: " + currentNode.fCost);

            openList.Remove(currentNode);
            //closedList.Add(currentNode);
            if (!closedDict.ContainsKey(currentNode.intersection))
            {
                closedDict.Add(currentNode.intersection, true);
            }

            if (currentNode.intersection == b.intersection)
            {
                Debug.Log(currentNode.WritePath());
                way = currentNode;
                return currentNode;
            }

            foreach (IntersectionEdge edge in currentNode.intersection.Edges)
            {
                //if (closedList.Contains(edge.Node)) continue;
                if (closedDict.ContainsKey(edge.Intersection)) continue;

                PathNode node;
                if (nodeDict.ContainsKey(edge.Intersection))
                {
                    node = nodeDict[edge.Intersection];
                }
                else
                {
                    node = new PathNode(edge.Intersection);
                }
                float newPath = currentNode.gCost + edge.Segment.Cost;
                
                node.CalculateHCost(b);

                float newFCost = newPath + node.hCost;

                if (newFCost < node.fCost || !openList.Contains(node))
                {
                    node.fCost = newFCost;
                    node.gCost = newPath;
                    node.parent = currentNode;
                    node.edge = edge;

                    if (!openList.Contains(node)) openList.Add(node);
                }
            }
        }

        Debug.Log("Not found");
        return null;
    }
}

[Serializable]
public class PathNode
{
    public Vector2 position;

    public float gCost;
    public float hCost;
    public float fCost = float.MaxValue;
    public PathNode parent;
    public Intersection intersection;
    public IntersectionEdge edge;

    public PathNode(Intersection intersection)
    {
        this.intersection = intersection;
    }

    public void Reset()
    {
        gCost = 0;
        hCost = 0;
        fCost = int.MaxValue;
        parent = null;
    }

    public void CalculateCost(PathNode target)
    {
        gCost = 0;
        CalculateHCost(target);
        fCost = gCost + hCost;
    }

    public void CalculateHCost(PathNode target)
    {
        hCost = (int) (5f * (position - target.position).magnitude);
    }

    public Vector3 GetPosition()
    {
        return new Vector3(position.x, 0f, position.y);
    }

    public string WritePath()
    {
        return parent != null ? parent.WritePath() + ", " + intersection.name : intersection.name;
    }

    public Stack<List<Vector3>> GetWaypoints()
    {
        Stack<List<Vector3>> stack = new Stack<List<Vector3>>();

        PathNode currentNode = this;
        while (currentNode.parent != null)
        {
            stack.Push(currentNode.edge.wayPoints);
            currentNode = currentNode.parent;
        }
        
        return stack;
    }
}
