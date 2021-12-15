using System;
using System.Collections.Generic;
//TODO entfernen
#if UNITY_EDITOR
    using UnityEditor;
#endif
using UnityEngine;

public class StreetSegment : MonoBehaviour
{
    public static int Counter;
    public List<Vector3> StreetPoints { get; private set; }

    public Intersection[] Intersections { get; } = new Intersection[2];

    public float Length { get; private set; }
    public float Speed { get; private set; } = 50;
    public float Cost { get; private set; }

    public float Thickness { get; private set; }
    
    public List<Vector3> edge1;
    public List<Vector3> edge2;
    
    private void Awake()
    {
        name = "Street " + Counter++;
    }

    public void Init(List<Vector3> points, float thickness)
    {
        StreetPoints = points;
        Thickness = thickness;
        
        edge1 = new List<Vector3>();
        edge2 = new List<Vector3>();

        CalculateLength(points);
        
        Vector3 laneOffset = CalculateLaneOffset(0);
        edge1.Add(points[0] - laneOffset);
        edge2.Add(points[points.Count - 1] + laneOffset);
        
        for (int i = 1; i < points.Count - 1; i++)
        {
            laneOffset = (CalculateLaneOffset(i-1) + CalculateLaneOffset(i)).normalized;
            edge1.Add(points[i] - laneOffset);
            edge2.Add(points[points.Count - 1 - i] + laneOffset);
            
            Debug.Log(i);
        }
        Debug.Log("Points: " + points.Count);
        
        laneOffset = CalculateLaneOffset(points.Count - 2);
        edge1.Add(points[points.Count - 1] - laneOffset);
        edge2.Add(points[0] + laneOffset);
    }

    private Vector3 CalculateLaneOffset(int index)
    {
        Debug.Log(index + ", " + (index + 1));
        
        Vector2 dir2D = new Vector2(StreetPoints[index + 1].x - StreetPoints[index].x, StreetPoints[index + 1].z - StreetPoints[index].z).normalized;
        Vector2 normal2D = Vector2.Perpendicular(dir2D);
        Vector3 normal = new Vector3(normal2D.x, 0f, normal2D.y).normalized;
        
        //TODO anpassen: 0.25 da 2 lanes und jeweils mittig platzier
        return Thickness * 0.25f * normal;
    }

    private void CalculateLength(List<Vector3> points)
    {
        Length = 0f;
        for (int i = 0; i < points.Count - 1; i++)
        {
            Length += Vector3.Distance(points[i], points[i + 1]);
        }

        //TODO Maßstab festlegen und eingeben
        Length *= 10f;
        Cost = Length / Speed;
    }

    private void OnDrawGizmos()
    {
        if (Intersections[0] == null && Intersections[1] == null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawCube(transform.position, Vector3.one);
        }

        Gizmos.color = Color.green;
        foreach (Vector3 waypoint in edge1)
        {
            Gizmos.DrawCube(waypoint, new Vector3(0.2f, 1f, 0.2f));
            Gizmos.color = Color.magenta;
        }
        Gizmos.color = Color.blue;
        foreach (Vector3 waypoint in edge2)
        {
            Gizmos.DrawCube(waypoint, new Vector3(0.2f, 1f, 0.2f));
            Gizmos.color = Color.yellow;
        }

#if UNITY_EDITOR
        Handles.Label(transform.position, Intersections[0]?.name + ", " + Intersections[1]?.name + 
                                          "\nLength: " + Length + ", Speed: " + Speed +", Cost: " + Cost);
#endif
        
    }
    
    
}