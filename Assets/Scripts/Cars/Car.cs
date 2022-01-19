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
    private Building _targetBuilding;
    [SerializeField] private float speed;
    [SerializeField] private float acceleration = 3f;
    [SerializeField] private float deceleration = 30f;
    [SerializeField] private float breakDistance = 6f;
    [SerializeField] private float maxSegmentSpeed;
    [SerializeField] private float targetDistance;
    [Range(2f, 10f)]
    [SerializeField] private float rayDistance = 8f;
    [SerializeField] private Transform rayPoint;
    [SerializeField] private LayerMask layer;
    private float _minDistance = 0.1f;
    private static float kmhToMsMultiplier = 0.2777777f;
    public static float SpeedMultiplier = 2f;
    private Stack<CarNode> _stack;
    private CarNode _node;
    private Vector3 _nextWaypoint;
    private int _index;
    private Car _blockingCar;
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

    private void Update()
    {
        if (Wait || !_hasTarget || SpeedMultiplier == 0f) return;
        
        if (_node == null)
        {
            if (_stack.Count > 0)
            {
                NextCarNode();
            }
            else
            {
                ReachedTarget();
                return;
            }
        }

        Ray ray = new Ray(rayPoint.position, rayPoint.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, rayDistance, layer) && hit.distance < breakDistance)
        {
            _blockingCar = hit.transform.GetComponent<Car>();
            if (hit.distance < breakDistance || _blockingCar._node.Lane == _node.Lane && _blockingCar.speed == 0 || _blockingCar.speed < speed)
            {
                if(speed < 2f && _blockingCar.speed < 2f && targetDistance < _blockingCar.targetDistance) UpdateSpeed();
                else
                {
                    Break(hit);
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

    private void Break(RaycastHit hit)
    {
        float factor = (breakDistance - hit.distance) / breakDistance;
        speed -= deceleration * factor * SpeedMultiplier * Time.deltaTime;
        if (speed < 0f) speed = 0f;
    }

    private void NextCarNode()
    {
        _node = _stack.Pop();
        maxSegmentSpeed = _node.MaxSpeed * kmhToMsMultiplier;
                
        _index = 0;
        _nextWaypoint = _node.WayPoints[_index];
        CalculateDirection();
    }

    private void ReachedTarget()
    {
        _hasTarget = false;
        _targetBuilding.EnterBuilding(Driver);
        Driver.CurrentPosition = _targetBuilding;
        Destroy(gameObject);
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
        float dist = Vector3.Distance(transform.position, _nextWaypoint);
        if (dist > targetDistance)
        {
            transform.position = _nextWaypoint;
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
        if (_index >= _node.WayPoints.Count)
        {
            _node = null;
        }
        else
        {
            _nextWaypoint = _node.WayPoints[_index];
            CalculateDirection();
        }
    }

    private void CalculateDirection()
    {
        _direction = (_nextWaypoint - transform.position).normalized;
        transform.rotation = Quaternion.LookRotation(_direction);
    }

    public void Move(PathNode node, Building target, Human driver)
    {
        _targetBuilding = target;
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
