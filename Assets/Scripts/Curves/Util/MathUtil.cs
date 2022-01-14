using UnityEngine;

namespace Curves.Util
{
    public static class BezierUtil
    {
        public static Vector3 QuadraticLerp(Vector3 start, Vector3 handle, Vector3 end, float time)
        {
            var ab = Vector3.Lerp(start, handle, time);
            var bc = Vector3.Lerp(handle, end, time);

            return Vector3.Lerp(ab, bc, time);
        }

        public static Vector3 CubicLerp(Vector3 p0, Vector3 h0, Vector3 p1, Vector3 h1, float time)
        {
            var a = QuadraticLerp(p0, h0, h1, time);
            var b = QuadraticLerp(h0, h1, p1, time);

            return Vector3.Lerp(a, b, time);
        }

        public static Vector3 GetTangent(Vector3 start, Vector3 handle, Vector3 end, float time)
        {
            return GetSecondDerivative(start, handle, end, time).normalized;
        }

        public static Vector3 GetQuadraticBezierNormal(Vector3 start, Vector3 handle, Vector3 end, float time)
        {
            Vector3 tangent = GetTangent(start, handle, end, time);
            Vector2 normal = Vector2.Perpendicular(new Vector2(tangent.x, tangent.z));
            return new Vector3(normal.x, 0f, normal.y);
        }

        public static float GetAcceleration(Vector3 start, Vector3 handle, Vector3 end)
        {
            return GetThirdDerivative(start, handle, end).magnitude;
        }

        private static Vector3 GetSecondDerivative(Vector3 start, Vector3 handle, Vector3 end, float time)
        {
            return 2 * (1 - time) * (handle - start) + 2 * time * (end - handle);
        }

        private static Vector3 GetThirdDerivative(Vector3 start, Vector3 handle, Vector3 end)
        {
            return 2 * (start - 2 * handle + end);
        }
    }
}