using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Intersection : MonoBehaviour
{
    private static int _counter;

    public int ID { get; private set; }
    public List<IntersectionEdge> Edges { get; } = new List<IntersectionEdge>();

    //TODO richtig umsetzen nach debug
    public void AddNeibhor(string nameOfObject, Intersection intersection, StreetSegment segment, List<Vector3> wayPoints)
    {
        Debug.Log( "Ausgelöst von " + nameOfObject + ", Edge von: " + name);
        Edges.Add(new IntersectionEdge(intersection, segment, wayPoints));
        
        //TODO Entfernen
        if (Edges.Count > 1)
        {
            string s = Edges.Count + ": ";
            foreach (IntersectionEdge edge in Edges)
            {
                s += edge.ToString() + " || ";
            }
            
            Debug.Log(name + ": " + s);
        }
    }

    public void RemoveNeighbor(Intersection intersection)
    {
        //TODO Verbessern
        for (int i = 0; i < Edges.Count; i++)
        {
            if (Edges[i].Intersection == intersection)
            {
                Debug.Log("Found: " + intersection.name);
                Edges.RemoveAt(i);
                return;
            }
        }
    }
    
    private void Awake()
    {
        ID = _counter;
        name = "Intersection " + _counter++;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(transform.position, 1f);
        
        Vector3 pos1, pos2;
        pos1 = transform.position;
        pos1.y += 0.1f;
        
        foreach (IntersectionEdge intersectionEdge in Edges)
        {
            //Draw center line
            Gizmos.color = Color.red;
            pos2 = intersectionEdge.Intersection.transform.position;
            pos2.y += 0.1f;
            Gizmos.DrawLine(pos1, pos2);
            
            //Draw waypoints
            Gizmos.color = Color.cyan;
            for (int i = 0; i < intersectionEdge.wayPoints.Count - 1; i++)
            {
                Gizmos.DrawLine(intersectionEdge.wayPoints[i], intersectionEdge.wayPoints[i+1]);
                Gizmos.DrawSphere(intersectionEdge.wayPoints[i+1], 0.5f);
            }
        }
    }
}

public class IntersectionEdge
{
    public Intersection Intersection { get; set; }
    public StreetSegment Segment { get; private set; }
    public List<Vector3> wayPoints;

    public IntersectionEdge(Intersection intersection, StreetSegment segment, List<Vector3> wayPoints)
    {
        Intersection = intersection;
        Segment = segment;
        this.wayPoints = wayPoints;

        string s = "";
        foreach (Vector3 wayPoint in wayPoints)
        {
            s += wayPoint + ", ";
        }
        
        Debug.Log("Connected to: " + intersection.name+ " | " + s);
    }

    public override string ToString()
    {
        string s = Intersection.name + ": ";

        foreach (Vector3 wayPoint in wayPoints)
        {
            s += wayPoint + ", ";
        }
        
        return s;
    }
}
