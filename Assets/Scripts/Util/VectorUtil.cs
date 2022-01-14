using System;
using UnityEngine;

namespace Util
{
    public static class VectorUtil
    {
        public static Vector3 GetNormal(Vector3 pointA, Vector3 pointB)
        {
            return new Vector3(-(pointB.z - pointA.z), 0f, pointB.x - pointA.x).normalized;
        }

        public static Vector3 GetNormal(Vector3 direction)
        {
            return new Vector3(-direction.z, 0f, direction.x).normalized;
        }
        
        public static Vector3 GetIntersectionPoint(float a1X, float a1Z, float a2X, float a2Z, float b1X, float b1Z, float b2X,
            float b2Z, float y)
        {
            float tmp = (b2X - b1X) * (a2Z - a1Z) - (b2Z - b1Z) * (a2X - a1X);
            if (tmp == 0)
            {
                Debug.Log("Not found");
                return new Vector3();
            }

            float mu = ((a1X - b1X) * (a2Z - a1Z) - (a1Z - b1Z) * (a2X - a1X)) / tmp;

            return new Vector3(
                b1X + (b2X - b1X) * mu,
                y,
                b1Z + (b2Z - b1Z) * mu
            );
        }

        public static float GetLinePositionDistance(Vector3 startPoint, Vector3 tangent, Vector3 pointToCheck, Vector3 normal)
        {
            float a1X = pointToCheck.x;
            float a1Z = pointToCheck.z;
            float a2X = pointToCheck.x + normal.x;
            float a2Z = pointToCheck.z + normal.z;

            float b1X = startPoint.x;
            float b1Z = startPoint.z;
            float b2X = startPoint.x + tangent.x;
            float b2Z = startPoint.z + tangent.z;
            
            float tmp = (b2X - b1X) * (a2Z - a1Z) - (b2Z - b1Z) * (a2X - a1X);
            if (tmp == 0)
            {
                Debug.Log("Not found");
                return 0f;
            }

            return ((a1X - b1X) * (a2Z - a1Z) - (a1Z - b1Z) * (a2X - a1X)) / tmp;
        }
    }
}