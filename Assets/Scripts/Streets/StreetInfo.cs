using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "new StreetInfo", menuName = "Streets/Street Info", order = 1)]
public class StreetInfo : ScriptableObject
{
    [Range(1, 4)]
    public int lanes = 1;
    [Range(3f, 5f)]
    public float trackWidth = 3f;

    [Range(20, 120)]
    public int speed = 50;
    public bool isCrossable = false;
}
