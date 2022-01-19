using System;
using System.Collections;
using System.Collections.Generic;
using Cars;
using Pathfinding;
using Population;
using Streets;
using UnityEngine;

public class Car : MonoBehaviour
{
    public Human Driver { get; private set; }

    private Vector3 _direction;
    private bool _hasTarget;
    private Building _target;

    [SerializeField] private float speed;
    [SerializeField] private float acceleration = 0.2f;
    [SerializeField] private float deceleration = 3f;
    [SerializeField] private float breakDistance = 3f;
    [SerializeField] private float maxSegmentSpeed;
    [SerializeField] private float targetDistance;

    private float _minDistance = 0.1f;
    
    private Stack<CarNode> _stack;
    public CarNode Node { get; private set; }

    private bool _wait;
    public bool Wait
    {
        get => _wait;
        set
        {
            if(_wait == value) return;
            _wait = value;
            speed = 0f;
        }
    }
    
    private Vector3 _currentWaypoint;
    private int _index;
    
    private static float kmhToMsMultiplier = 0.2777777f;

    [SerializeField] private Transform rayPoint;
    
    [Range(2f, 10f)]
    [SerializeField] private float rayDistance = 3f;

    [SerializeField] private LayerMask layer;
    
    public static float SpeedMultiplier = 2f;
    private Car _blockingCar;

    private void Update()
    {
        if (Wait || !_hasTarget || SpeedMultiplier == 0f) return;
        
        if (Node == null)
        {
            if (_stack.Count > 0)
            {
                Node = _stack.Pop();
                maxSegmentSpeed = Node.MaxSpeed * kmhToMsMultiplier;
                
                _index = 0;
                _currentWaypoint = Node.WayPoints[_index];
                CalculateDirection();
            }
            else
            {
                _hasTarget = false;
                _target.EnterBuilding(Driver);
                Driver.CurrentPosition = _target;
                Destroy(gameObject);
                return;
            }
        }

        Ray ray = new Ray(rayPoint.position, rayPoint.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, rayDistance, layer) && hit.distance < breakDistance)
        {
            _blockingCar = hit.transform.GetComponent<Car>();
            if (hit.distance < breakDistance || _blockingCar.Node.Lane == Node.Lane && _blockingCar.speed == 0 || _blockingCar.speed < speed)
            {
                if(speed < 2f && _blockingCar.speed < 2f && targetDistance < _blockingCar.targetDistance) UpdateSpeed();
                else
                {
                    float factor = (breakDistance - hit.distance) / breakDistance;
                    speed -= deceleration * factor * SpeedMultiplier * Time.deltaTime;
                    if (speed < 0f)
                    {
                        speed = 0f;
                    }
                }
                
            }
            else
            {
                UpdateSpeed();   
            }
        }
        else
        {
            UpdateSpeed();
        }
        
        transform.position += speed * SpeedMultiplier * Time.deltaTime * _direction;
        UpdateTargetDistance();
        
        if (targetDistance < _minDistance)
        {
            UpdateIndex();
        }
    }

    private void UpdateSpeed()
    {
        if (speed < maxSegmentSpeed)
        {
            speed += acceleration * SpeedMultiplier * Time.deltaTime;
            if (speed > maxSegmentSpeed) speed = maxSegmentSpeed;
        }
        else
        {
            speed -= deceleration * SpeedMultiplier * Time.deltaTime;
        }
    }

    private void UpdateTargetDistance()
    {
        float dist = Vector3.Distance(transform.position, _currentWaypoint);
        if (dist > targetDistance)
        {
            transform.position = _currentWaypoint;
            UpdateIndex();
        }
        else
        {
            targetDistance = dist;
        }
    }

    private void UpdateIndex()
    {
        targetDistance = float.MaxValue;
        _index++;
        if (_index >= Node.WayPoints.Count)
        {
            Node = null;
        }
        else
        {
            _currentWaypoint = Node.WayPoints[_index];
            CalculateDirection();
        }
    }

    private void CalculateDirection()
    {
        _direction = (_currentWaypoint - transform.position).normalized;
        transform.rotation = Quaternion.LookRotation(_direction);
    }

    public void Move(PathNode node, Building target, Human driver)
    {
        _target = target;
        Driver = driver;
        _stack = node.GetWaypoints();
        targetDistance = float.MaxValue;
        _hasTarget = true;
    }

    private void OnDrawGizmosSelected()
    {
        if (_blockingCar != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, _blockingCar.transform.position);
            Gizmos.DrawCube(_blockingCar.transform.position, new Vector3(1f, 10f, 1f));
        }
    }
}
