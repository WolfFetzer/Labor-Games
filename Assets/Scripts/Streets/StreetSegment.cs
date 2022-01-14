using System;
using System.Collections.Generic;
using Buildings.BuildingArea;
//TODO entfernen
#if UNITY_EDITOR
    using UnityEditor;
#endif
using UnityEngine;
using Util;

/// <summary>
/// Forward means that the streetpoint direction is ascending, backwards decending
/// </summary>
public enum StreetDirection
{
    Forward, Backward
}

public class StreetSegment : MonoBehaviour
{
    public static int Counter;
    public List<Vector3> StreetPoints { get; private set; }

    public BuildingArea[] BuildingAreas { get; set; } = new BuildingArea[2];

    public bool IsConnected { get; private set; }
    private Intersection[] _intersections = new Intersection[2];
    public Intersection IntersectionAtStart
    {
        get => _intersections[0];
        set
        {
            _intersections[0] = value;
            IsConnected = true;
        } 
    }
    
    public Intersection IntersectionAtEnd 
    {
        get => _intersections[1];
        set
        {
            _intersections[1] = value;
            IsConnected = true;
        } 
    }
    //public Intersection[] Intersections { get; } = new Intersection[2];

    public StreetInfo Info { get; private set; }

    public float Length { get; private set; }
    public float Speed { get; private set; } = 50;
    public float Cost { get; private set; }
    
    public List<Vector3> edge1;
    public List<Vector3> edge2;
    
    private void Awake()
    {
        name = "Street " + Counter++;
    }

    private void OnDestroy()
    {
        _intersections[0]?.RemoveSegment(this);
        _intersections[1]?.RemoveSegment(this);
    }

    public void Init(List<Vector3> points, StreetInfo info)
    {
        StreetPoints = points;
        Info = info;
        
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
        }

        laneOffset = CalculateLaneOffset(points.Count - 2);
        edge1.Add(points[points.Count - 1] - laneOffset);
        edge2.Add(points[0] + laneOffset);

        float length = (StreetPoints[1] - StreetPoints[0]).magnitude;
        float offset = info.trackWidth * info.lanes;
        
        BuildingAreas[0] = Instantiate(GameManager.Instance.buildingAreaPrefab, transform).GetComponent<BuildingArea>();
        BuildingAreas[0].Init(offset, length, this);

        BuildingAreas[1] = Instantiate(GameManager.Instance.buildingAreaPrefab, transform).GetComponent<BuildingArea>();
        BuildingAreas[1].Init(-offset, length, this);
    }

    private Vector3 CalculateLaneOffset(int index)
    {
        Vector3 normal = VectorUtil.GetNormal(StreetPoints[index], StreetPoints[index + 1]);
        return Info.trackWidth * 0.5f * normal;
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
        if (!IsConnected)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawCube(transform.position, Vector3.one);
        }

        Gizmos.color = Color.green;
        Gizmos.DrawCube(edge1[edge1.Count - 1], new Vector3(0.2f, 1f, 0.2f));

        Gizmos.color = Color.blue;
        Gizmos.DrawCube(edge2[edge2.Count - 1], new Vector3(0.2f, 1f, 0.2f));
        foreach (Vector3 waypoint in edge2)

#if UNITY_EDITOR
        Handles.Label(transform.position, _intersections[0]?.name + ", " + _intersections[1]?.name + 
                                          "\nLength: " + Length + ", Speed: " + Speed +", Cost: " + Cost);
#endif
    }
}