using System;
using System.Collections.Generic;
using Streets;
using UnityEngine;
using Util;

public class Intersection : MonoBehaviour
{
    private static int IdCounter;
    public List<Lane> Edges { get; } = new List<Lane>();

    private void Awake()
    {
        name = $"Intersection {IdCounter++}";
    }

    public void AddLane(Lane lane)
    {
        if(Edges.Count != 0)
        {
            Vector3 dir1 = Edges[0].WayPoints[1] - Edges[0].WayPoints[0];
            Vector3 dir2 = lane.WayPoints[1] - lane.WayPoints[0];
            float angle = Vector3.SignedAngle(dir1, dir2, Vector3.up);
            if (angle < 0) angle += 360f;
            
            for (int i = 1; i < Edges.Count; i++)
            {
                dir2 = Edges[i].WayPoints[1] - Edges[i].WayPoints[0];
                float otherAngle = Vector3.SignedAngle(dir1, dir2, Vector3.up);
                if (otherAngle < 0) otherAngle += 360f;

                if (angle < otherAngle)
                {
                    Edges.Insert(i, lane);
                    return;
                }
            }
        }
        Edges.Add(lane);
    }

    public void RemoveLane(Lane lane)
    {
        Edges.Remove(lane);
    }

    public virtual void UpdateMesh(Mesh mesh)
    {
        GetComponent<MeshFilter>().mesh = mesh;
    }
    
    private void OnDrawGizmos()
    {
        Vector3 pos1 = transform.position;
        pos1.y += 0.1f;
        
        foreach (Lane edge in Edges)
        {
            //Draw waypoints
            Gizmos.color = Color.cyan;
            for (int i = 0; i < edge.WayPoints.Count - 1; i++)
            {
                Gizmos.DrawLine(edge.WayPoints[i], edge.WayPoints[i + 1]);
                Gizmos.DrawSphere(edge.WayPoints[i + 1], 0.5f);
                
                Gizmos.color = Color.green;
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        foreach (Lane edge in Edges)
        {
            if(!edge.IsConnected) continue;
            Vector3 pos = transform.position;
            Vector3 iPos = edge.Intersection.transform.position;
            Gizmos.DrawSphere(pos, 0.25f);
            Gizmos.DrawSphere(iPos, 0.25f);
            Gizmos.DrawLine(pos, iPos);
        }
    }
}