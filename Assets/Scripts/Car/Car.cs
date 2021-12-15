using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Car : MonoBehaviour
{
    private Vector3 _startPoint;
    private Vector3 _destination;
    private Vector3 _direction;

    private bool _hasTarget = false;

    public float speed = 3f;
    private float _minDistance = 0.6f;

    private PathNode _pathNode;
    private Stack<List<Vector3>> _stack;
    private List<Vector3> _currentWaypoints;
    private Vector3 _currentWaypoint;
    private int _index = 0;

    public void SetDestination(Vector3 start, Vector3 destination)
    {
        _startPoint = start;
        _destination = destination;
        _direction = (_destination - _startPoint).normalized;
        _hasTarget = true;
        
        Debug.Log(_startPoint + ", " + destination + ", Direction: " + _direction);
    }

    private void Update()
    {
        if (_hasTarget)
        {
            if (_currentWaypoints == null)
            {
                if (_stack.Count > 0)
                {
                    _currentWaypoints = _stack.Pop();
                    _index = 0;
                    _currentWaypoint = _currentWaypoints[_index];
                    CalculateDirection();
                }
                else
                {
                    _hasTarget = false;
                    return;
                }
            }

            transform.position += speed * Time.deltaTime * _direction;

            if (Vector3.Distance(transform.position, _currentWaypoint) < _minDistance)
            {
                UpdateIndex();
            }
        }
    }

    private void UpdateIndex()
    {
        _index++;
        if (_index >= _currentWaypoints.Count)
        {
            _currentWaypoints = null;
        }
        else
        {
            _currentWaypoint = _currentWaypoints[_index];
            CalculateDirection();
        }
    }

    private void CalculateDirection()
    {
        _direction = (_currentWaypoint - transform.position).normalized;
    }

    public void Move(PathNode node)
    {
        _pathNode = node;

        _stack = node.GetWaypoints();

        /*
        for (int i = 0; i < stack.Count; i++)
        {
            Debug.Log(stack.Pop());
        }
        */
        
        _hasTarget = true;
    }
}
