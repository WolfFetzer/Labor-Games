using System;
using System.Collections.Generic;
using Buildings.BuildingArea;
using Streets;
using UnityEngine;
using Util;


public class StreetSegment : MonoBehaviour
{
    private static int Counter;
    public List<Vector3> StreetPoints { get; private set; }

    public BuildingArea[] BuildingAreas { get; set; } = new BuildingArea[2];

    public bool IsConnected => Lane1.IsConnected || Lane2.IsConnected;
    public StreetInfo Info { get; private set; }

    public float Length { get; private set; }
    public float Speed { get; private set; } = 50;
    public float Cost { get; private set; }

    public Lane Lane1 { get; private set; }
    public Lane Lane2 { get; private set; }
    
    
    private void Awake()
    {
        name = "Street " + Counter++;
    }

    private void OnDestroy()
    {
        if(Lane1.IsConnected) Lane1.Intersection.RemoveLane(Lane2);
        if(Lane2.IsConnected) Lane2.Intersection.RemoveLane(Lane1);
    }

    public void Init(List<Vector3> points, StreetInfo info)
    {
        StreetPoints = points;
        Info = info;
        
        List<Vector3> edge1 = new List<Vector3>();
        List<Vector3> edge2 = new List<Vector3>();

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

        Lane1 = new Lane(edge1, this);
        Lane2 = new Lane(edge2, this);

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

        Cost = Length * 10f / Speed;
    }

    private void OnDrawGizmos()
    {
        if (!IsConnected)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawCube(transform.position, Vector3.one);
        }

        Gizmos.color = Color.green;
        Gizmos.DrawCube(Lane1.WayPoints[Lane1.WayPoints.Count - 1], new Vector3(0.2f, 1f, 0.2f));

        Gizmos.color = Color.blue;
        Gizmos.DrawCube(Lane2.WayPoints[Lane2.WayPoints.Count - 1], new Vector3(0.2f, 1f, 0.2f));
    }
}