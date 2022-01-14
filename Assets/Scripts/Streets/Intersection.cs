using System;
using System.Collections.Generic;
using Streets;
using UnityEngine;
using Util;

public class Intersection : MonoBehaviour
{
    private static int _idCounter;

    public List<IntersectionEdge> Edges { get; } = new List<IntersectionEdge>();
    
    //public List<Vector3Tuple> SegmentStartingPoints { get; } = new List<Vector3Tuple>();
    public List<Vector3Tuple> SegmentStartingPoints = new List<Vector3Tuple>();
    
    
    public List<StreetSegment> Segments { get; } = new List<StreetSegment>();

    private void Awake()
    {
        name = $"Intersection {_idCounter++}";
    }

    private List<Vector3Tuple> ins = new List<Vector3Tuple>(); 
    private List<Vector3Tuple> outs = new List<Vector3Tuple>(); 
    
    public void AddSegment(StreetSegment segment, StreetDirection direction)
    {
        if (direction == StreetDirection.Forward)
        {
            if (segment.IntersectionAtEnd != null)
            {
                Edges.Add(new IntersectionEdge(segment.IntersectionAtEnd, segment, segment.edge1));
            }

            SegmentStartingPoints.Add(new Vector3Tuple()
            {
                StartingPoint = segment.StreetPoints[0],
                Direction = (segment.StreetPoints[1] - segment.StreetPoints[0]).normalized
            });
            
            //
            //
            Vector3Tuple newIn = new Vector3Tuple()
            {
                StartingPoint = segment.edge2[segment.edge2.Count - 1],
                Direction = (segment.edge2[segment.edge2.Count - 1] - segment.edge2[segment.edge2.Count - 2]).normalized
            };
            
            ins.Add(newIn);
            foreach (Vector3Tuple tuple in outs)
            {
                //_selectedStreetInfo.lanes * 2 * _selectedStreetInfo.trackWidth
                wayPoints.Add(
                    new List<Vector3>()
                    {
                        newIn.StartingPoint + 4f * newIn.Direction, 
                        tuple.StartingPoint + 4f * tuple.Direction
                    });
            }
            
            outs.Add(new Vector3Tuple()
            {
                StartingPoint = segment.edge1[0],
                Direction = (segment.edge1[0] - segment.edge1[1]).normalized
            });
        }
        else
        {
            if (segment.IntersectionAtStart != null)
            {
                Edges.Add(new IntersectionEdge(segment.IntersectionAtStart, segment, segment.edge2));
            }
            
            SegmentStartingPoints.Add(new Vector3Tuple()
            {
                StartingPoint = segment.StreetPoints[segment.StreetPoints.Count - 1],
                Direction = (segment.StreetPoints[segment.StreetPoints.Count - 2] - segment.StreetPoints[segment.StreetPoints.Count - 1]).normalized
            });
            //
            //
            Vector3Tuple newIn = new Vector3Tuple()
            {
                StartingPoint = segment.edge1[segment.edge1.Count - 1],
                Direction = (segment.edge1[segment.edge1.Count - 1] - segment.edge1[segment.edge1.Count - 2]).normalized
            };
            
            ins.Add(newIn);

            foreach (Vector3Tuple tuple in outs)
            {
                wayPoints.Add(new List<Vector3>(){newIn.StartingPoint + 4f * newIn.Direction, tuple.StartingPoint + 4f * tuple.Direction});
            }
            
            outs.Add(new Vector3Tuple()
            {
                StartingPoint = segment.edge2[0],
                Direction = (segment.edge2[0] - segment.edge2[1]).normalized
            });
            //
            //
            //
            /*
            foreach (StreetSegment streetSegment in Segments)
            {
            
            }
            */
            //
            //
            //
            //
            //
        }

        Segments.Add(segment);
    }
    //
    //
    //
    //
    //
    public List<List<Vector3>> wayPoints = new List<List<Vector3>>(); 

    public void RemoveNeighbor(Intersection intersection)
    {
        Debug.Log("Remove");
        
        for (int i = 0; i < Edges.Count; i++)
            if (Edges[i].Intersection == intersection)
            {
                Edges.RemoveAt(i);
                //SegmentStartingPoints.RemoveAt(i);
                return;
            }
    }

    public void RemoveSegment(StreetSegment segment)
    {
        Segments.Remove(segment);
    }

    public void UpdateMesh(Mesh mesh)
    {
        GetComponent<MeshFilter>().mesh = mesh;
    }
    
    private void OnDrawGizmos()
    {
        Vector3 pos1 = transform.position;
        pos1.y += 0.1f;

        foreach (IntersectionEdge intersectionEdge in Edges)
        {
            //Draw center line
            Gizmos.color = Color.red;
            Vector3 pos2 = intersectionEdge.Intersection.transform.position;
            pos2.y += 0.1f;
            Gizmos.DrawLine(pos1, pos2);

            //Draw waypoints
            Gizmos.color = Color.cyan;
            for (int i = 0; i < intersectionEdge.WayPoints.Count - 1; i++)
            {
                Gizmos.DrawLine(intersectionEdge.WayPoints[i], intersectionEdge.WayPoints[i + 1]);
                Gizmos.DrawSphere(intersectionEdge.WayPoints[i + 1], 0.5f);
            }
        }

        Gizmos.color = Color.magenta;
        foreach (List<Vector3> list in wayPoints)
        {
            foreach (Vector3 vector3 in list)
            {
                Gizmos.DrawSphere(vector3, 0.25f);
            }
        }
        foreach (List<Vector3> list in wayPoints)
        {
            for (int i = 0; i < list.Count - 1; i++)
            {
                Gizmos.DrawLine(list[i], list[i + 1]);
            }
        }
    }
}