using System;
using System.Collections;
using System.Collections.Generic;
using Pathfinding;
using Population;
using Streets;
using UnityEngine;

public class Car : MonoBehaviour
{
    public Human driver;
    
    private Vector3 _startPoint;
    private Vector3 _destination;
    private Vector3 _direction;

    private bool _hasTarget = false;
    private Building _target;

    public float acceleration = 5f;
    public float speed = 3f;
    private float _minDistance = 0.6f;
    
    private Stack<IntersectionEdge> _stack;
    private IntersectionEdge _edge;
    private Vector3 _currentWaypoint;
    private int _index = 0;

    public static float SpeedMultiplier = 2f;

    private void Update()
    {
        if (!_hasTarget) return;
        
        if (_edge == null)
        {
            if (_stack.Count > 0)
            {
                _edge = _stack.Pop();
                speed = _edge.Segment.Speed * 1000 / 3600;
                _index = 0;
                _currentWaypoint = _edge.WayPoints[_index];
                CalculateDirection();
            }
            else
            {
                _hasTarget = false;
                _target.EnterBuilding(driver);
                driver.CurrentPosition = _target;
                Destroy(gameObject);
                return;
            }
        }
        transform.position += speed * SpeedMultiplier * Time.deltaTime * _direction;

        if (Vector3.Distance(transform.position, _currentWaypoint) < _minDistance)
        {
            UpdateIndex();
        }
    }

    private void UpdateIndex()
    {
        _index++;
        if (_index >= _edge.WayPoints.Count)
        {
            _edge = null;
        }
        else
        {
            _currentWaypoint = _edge.WayPoints[_index];
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
        this.driver = driver;
        _stack = node.GetWaypoints();
        _hasTarget = true;
    }
}
