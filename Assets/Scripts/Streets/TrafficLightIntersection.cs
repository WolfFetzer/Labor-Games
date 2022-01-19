using System;
using System.Collections;
using System.Collections.Generic;
using Streets;
using UnityEngine;

public class TrafficLightIntersection : Intersection
{
    private List<TrafficLight> _trafficLights = new List<TrafficLight>();
    private int _index;
    private int _maxCycle = 1;

    private void SwitchLights()
    {
        if (TrafficLightManager.Instance.IsWaitingTime)
        {
            foreach (TrafficLight t in _trafficLights)
            {
                if(!t.isRed) t.SetYellow();
                //t.SetRed();
            }
        }
        else
        {
            _index++;
            _index %= _maxCycle;
            
            foreach (TrafficLight t in _trafficLights)
            {
                t.SetRed();
            }
            while (_index < _trafficLights.Count)
            {
                _trafficLights[_index].SetGreen();
                _index += _maxCycle;
            }
        }
    }

    public override void UpdateMesh(Mesh mesh)
    {
        base.UpdateMesh(mesh);
        
        if(Edges.Count < 3) return;
        
        foreach (Transform lights in transform)
        {
            Destroy(lights.gameObject);
        }
        _trafficLights.Clear();
        
        int i = 0;
        foreach (Lane edge in Edges)
        {
            Vector3 dir = edge.WayPoints[1] - edge.WayPoints[0];
            Vector3 normal = new Vector3(-dir.z, 0f, dir.x).normalized;
            StreetInfo info = edge.Segment.Info;
            Vector3 pos = edge.WayPoints[0] + info.trackWidth * normal;
            GameObject go = Instantiate(GameManager.Instance.trafficLightPrefab, pos, Quaternion.LookRotation(dir, Vector3.up));
            go.transform.parent = transform;
            go.name = "Light " + i++;
            _trafficLights.Add(go.GetComponent<TrafficLight>());
        }
        
        _maxCycle = (_trafficLights.Count + 1) / 2;
    }

    private void OnEnable()
    {
        TrafficLightManager.Instance.OnSwitchTrafficLights += SwitchLights;
    }

    private void OnDisable()
    {
        TrafficLightManager.Instance.OnSwitchTrafficLights -= SwitchLights;
    }
}
