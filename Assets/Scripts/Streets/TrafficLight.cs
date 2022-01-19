using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrafficLight : MonoBehaviour
{
    public bool isRed { get; private set; } = true;
    [SerializeField] private MeshRenderer renderer;

    public void SetGreen()
    {
        renderer.material = TrafficLightManager.Instance.greenMaterial;
        isRed = false;
    }

    public void SetYellow()
    {
        renderer.material = TrafficLightManager.Instance.yellowMaterial;
        isRed = false;
    }

    public void SetRed()
    {
        renderer.material = TrafficLightManager.Instance.redMaterial;
        isRed = true;
    }
    
    private void OnTriggerEnter(Collider other)
    {
        Car car = other.GetComponent<Car>();
        
        if(car == null) return;

        car.Wait = isRed;
    }

    private void OnTriggerStay(Collider other)
    {
        Car car = other.GetComponent<Car>();
        if(car == null) return;

        car.Wait = isRed;
    }

    private void OnDrawGizmos()
    {
        if (!isRed) Gizmos.color = Color.green;
        else Gizmos.color = Color.red;
        
        Gizmos.DrawCube(transform.position + new Vector3(0f, 3f, 0f), new Vector3(1f, 1f, 1f));
    }
}
