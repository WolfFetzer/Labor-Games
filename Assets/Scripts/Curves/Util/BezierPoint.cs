using System;
using UnityEngine;

namespace Curves.Util
{
    [Serializable]
    public class BezierPoint
    {
        public Vector3 Position;
        public Vector3 Tangent;
        public Vector2 Normal2D;


        public BezierPoint()
        {
        }

        public BezierPoint(Vector3 start, Vector3 handle, Vector3 end, float time)
        {
            Position = GetPosition(start, handle, end, time);
            Tangent = GetTangent(start, handle, end, time);
            Normal2D = GetNormal2D();
        }

        private Vector3 GetPosition(Vector3 start, Vector3 handle, Vector3 end, float time)
        {
            return BezierUtil.QuadraticLerp(start, handle, end, time);
            ;
        }

        private Vector3 GetTangent(Vector3 start, Vector3 handle, Vector3 end, float time)
        {
            //TODO verbessern
            return
            (
                2 * time * start -
                2 * start -
                4 * time * handle +
                2 * handle +
                2 * time * end
            ).normalized;
        }

        private Vector2 GetNormal2D()
        {
            return Vector2.Perpendicular(new Vector2(Tangent.x, Tangent.z));
        }
    }
}