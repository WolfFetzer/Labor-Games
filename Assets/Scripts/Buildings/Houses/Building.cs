using System;
using System.Collections;
using System.Collections.Generic;
using Buildings.BuildingArea;
using Pathfinding;
using Population;
using UnityEngine;

public abstract class Building : MonoBehaviour
{
    public AreaField Field { get; set; }

    public abstract void EnterBuilding(Human human);
    public abstract void LeaveBuilding(Human human);
}
