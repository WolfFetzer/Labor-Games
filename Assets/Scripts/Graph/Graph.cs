using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

public class Graph : MonoBehaviour
{
    public WeightedGraph graph = new WeightedGraph();

    private void OnDrawGizmos()
    {
        if (graph.nodes == null) return;

        foreach (var node in graph.nodes)
        {
            foreach (var edge in node.Edges)
            {
                //Handles.color = Color.yellow;
                //Handles.DrawAAPolyLine(node.GetPosition(), edge.Node.GetPosition());
                //Handles.Label(node.GetPosition() + 0.5f * (edge.Node.GetPosition() - node.GetPosition()),"" + edge.Distance + ", Cost: " + edge.Cost + ", Speed: " + edge.Speed);
            }

            //Handles.color = Color.black;
            //Handles.DrawSolidDisc(node.GetPosition(), Vector3.up, 1f);

            //Handles.color = Color.black;
            //Handles.Label(node.GetPosition(),"Index: " + node.index + "\ngCost: " + node.gCost + "\nfCost: " + node.fCost + "\nhCost: " +node.hCost);
        }

        var way = graph.way;

        while (way.parent != null)
        {
            //Handles.color = Color.green;
            //Handles.DrawAAPolyLine(10f, way.GetPosition(), way.parent.GetPosition());

            way = way.parent;
        }
    }
}

[Serializable]
public class WeightedGraph
{
    public List<GraphNode> nodes;

    public GraphNode way;

    public void AddNode()
    {
        nodes.Add(new GraphNode());
    }

    public void RandomizeCosts()
    {
        foreach (var node in nodes)
        foreach (var edge in node.Edges)
            edge.Distance = Random.Range(0, 1000);
    }

    public void ConnectNodes(GraphNode a, GraphNode b)
    {
        var edge = new GraphEdge(a, b);

        a.Edges.Add(edge);

        var otherEdge = new GraphEdge(a, edge.Distance) {Speed = edge.Speed};
        otherEdge.CalculateCost();
        b.Edges.Add(otherEdge);
    }

    public GraphNode CalculateShortestWay(GraphNode a, GraphNode b)
    {
        foreach (var node in nodes) node.Edges.Clear();

        ConnectNodes(nodes[0], nodes[1]);
        ConnectNodes(nodes[0], nodes[2]);

        ConnectNodes(nodes[1], nodes[3]);
        ConnectNodes(nodes[1], nodes[4]);

        ConnectNodes(nodes[2], nodes[4]);
        ConnectNodes(nodes[2], nodes[5]);

        ConnectNodes(nodes[3], nodes[4]);
        ConnectNodes(nodes[3], nodes[6]);

        ConnectNodes(nodes[5], nodes[4]);

        ConnectNodes(nodes[4], nodes[6]);


        //RandomizeCosts();

        foreach (GraphNode node in nodes) node.Reset();


        List<GraphNode> openList = new List<GraphNode>();
        List<GraphNode> closedList = new List<GraphNode>();

        a.CalculateCost(b);

        openList.Add(a);
        GraphNode currentNode = a;

        var index = 0;
        while (index < 10)
        {
            index++;
            var currentLowestFCost = int.MaxValue;
            foreach (var node in openList)
                if (node.fCost < currentLowestFCost)
                {
                    currentLowestFCost = node.fCost;
                    currentNode = node;
                }

            Debug.Log("Index currentNode: " + currentNode.index + ", Lowest fCost: " + currentNode.fCost);

            openList.Remove(currentNode);
            closedList.Add(currentNode);

            if (currentNode == b)
            {
                Debug.Log(currentNode.WritePath());
                way = currentNode;
                return currentNode;
            }

            foreach (var edge in currentNode.Edges)
            {
                if (closedList.Contains(edge.Node)) continue;

                int newPath = currentNode.gCost + /*edge.Distance*/ edge.Cost;
                edge.Node.CalculateHCost(b);

                var newFCost = newPath + edge.Node.hCost;

                if (newFCost < edge.Node.fCost || !openList.Contains(edge.Node))
                {
                    edge.Node.fCost = newFCost;
                    edge.Node.gCost = newPath;
                    edge.Node.parent = currentNode;

                    if (!openList.Contains(edge.Node)) openList.Add(edge.Node);
                }
            }
        }

        Debug.Log("Not found");
        return null;
    }
}

[Serializable]
public class GraphNode
{
    public int index;
    public Vector2 position;

    public int gCost;
    public int hCost;
    public int fCost = int.MaxValue;
    public GraphNode parent;
    public List<GraphEdge> Edges { get; set; } = new List<GraphEdge>();

    public void Reset()
    {
        gCost = 0;
        hCost = 0;
        fCost = int.MaxValue;
        parent = null;
    }

    public void CalculateCost(GraphNode target)
    {
        gCost = 0;
        CalculateHCost(target);
        fCost = gCost + hCost;
    }

    public void CalculateHCost(GraphNode target)
    {
        hCost = (int) (5f * (position - target.position).magnitude);
    }

    public Vector3 GetPosition()
    {
        return new Vector3(position.x, 0f, position.y);
    }

    public string WritePath()
    {
        return parent != null ? index + ", " + parent.WritePath() : " " + index;
    }
}

public class GraphEdge
{
    public GraphEdge(GraphNode start, GraphNode target)
    {
        Node = target;
        Distance = (int) (100f * (start.position - target.position).magnitude);
        CalculateCost();
    }

    public GraphEdge(GraphNode target, int distance)
    {
        Node = target;
        Distance = distance;
        CalculateCost();
    }

    public int Distance { get; set; }
    public GraphNode Node { get; set; }

    public int Speed { get; set; } = 50;

    public int Cost { get; set; }

    public void CalculateCost()
    {
        Cost = 10 * Distance / Speed;
    }
}