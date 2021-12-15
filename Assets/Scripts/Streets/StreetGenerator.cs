using System.Collections.Generic;
using Curves.Util;
using UnityEngine;

namespace Streets
{
    public static class StreetGenerator
    {
        public static Mesh GenerateStraight(Vector3 start, Vector3 end)
        {
            Mesh mesh = new Mesh();

            Vector3 direction = end - start;

            float length = direction.magnitude;

            Vector2 tangent2D = new Vector2(direction.x, direction.z);
            Vector2 normal2D = Vector2.Perpendicular(tangent2D).normalized;
            Vector3 normal = new Vector3(normal2D.x, 0f, normal2D.y);

            Debug.Log("Normal length: " + normal2D.magnitude);

            start.y += 0.1f;
            end.y += 0.1f;

            Vector3[] vertices = new Vector3[4];
            int[] triangles = new int[6];

            vertices[0] = start + normal;
            vertices[1] = start - normal;
            vertices[2] = end + normal;
            vertices[3] = end - normal;

            triangles[0] = 0;
            triangles[1] = 3;
            triangles[2] = 1;
            triangles[3] = 0;
            triangles[4] = 2;
            triangles[5] = 3;

            mesh.vertices = vertices;
            mesh.triangles = triangles;

            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            return mesh;
        }

        public static GameObject GenerateStraightGameObject(Vector3 start, Vector3 end)
        {
            float thickness = 3f;
            GameObject go = new GameObject {layer = 6};

            Vector3 direction = end - start;
            float length = direction.magnitude;
            float halfLength = length / 2f;

            Vector3 position = 0.5f * direction + start;
            go.transform.position = position;
            go.transform.localRotation = Quaternion.LookRotation(direction, Vector3.up);

            BoxCollider collider = go.AddComponent<BoxCollider>();
            collider.size = new Vector3(thickness, 1f, length);

            MeshRenderer renderer = go.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = GameManager.Instance.streetMaterial;

            MeshFilter filter = go.AddComponent<MeshFilter>();

            StreetSegment segment = go.AddComponent<StreetSegment>();
            segment.Init(new List<Vector3> {start, end}, thickness);

            Mesh mesh = new Mesh();

            Vector3[] vertices = new Vector3[4];
            int[] triangles = new int[6];
            Vector2[] uvs = new Vector2[4];
            float halfThickness = segment.Thickness * 0.5f;

            vertices[0] = new Vector3(-halfThickness, 0.01f, -halfLength);
            vertices[1] = new Vector3(halfThickness, 0.01f, -halfLength);
            vertices[2] = new Vector3(-halfThickness, 0.01f, halfLength);
            vertices[3] = new Vector3(halfThickness, 0.01f, halfLength);

            triangles[0] = 0;
            triangles[1] = 3;
            triangles[2] = 1;
            triangles[3] = 0;
            triangles[4] = 2;
            triangles[5] = 3;

            uvs[0] = new Vector2(0, 0);
            uvs[1] = new Vector2(1, 0);
            uvs[2] = new Vector2(0, 1);
            uvs[3] = new Vector2(1, 1);

            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.uv = uvs;

            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            filter.mesh = mesh;

            return go;
        }

        public static GameObject GenerateComplexStreetGameObject(List<Vector3> points)
        {
            int curveSteps = 5;

            Debug.Log("Generate Complex");
            float thickness = 3f;
            GameObject go = new GameObject {layer = 6};

            Vector3 pos = points[0];
            pos.y = 0.01f;
            go.transform.position = pos;

            MeshRenderer renderer = go.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = GameManager.Instance.streetMaterial;
            MeshFilter filter = go.AddComponent<MeshFilter>();

            StreetSegment segment = go.AddComponent<StreetSegment>();
            segment.Init(points, thickness);

            Mesh mesh = new Mesh();

            Vector3[] vertices = CreateVertices(points, curveSteps, segment);
            int[] triangles = CreateIndices(vertices.Length);

            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.uv = CreateUvs(vertices.Length);

            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            filter.mesh = mesh;

            MeshCollider collider = go.AddComponent<MeshCollider>();
            collider.sharedMesh = mesh;

            return go;
        }

        private static Vector2[] CreateUvs(int pointAmount)
        {
            List<Vector2> uvs = new List<Vector2>();
            Vector2[] points =
            {
                new Vector2(0, 0),
                new Vector2(1, 0),
                new Vector2(0, 1),
                new Vector2(1, 1)
            };

            for (int i = 0; i < pointAmount; i++)
            {
                uvs.Add(points[i%4]);
            }

            Debug.Log(pointAmount + ", " + uvs.Count);

            return uvs.ToArray();
        }

        private static int[] CreateIndices(int pointAmount)
        {
            List<int> triangles = new List<int>();
            int end = pointAmount - 2;

            for (int i = 0; i < end; i += 2)
            {
                triangles.Add(i);
                triangles.Add(i + 3);
                triangles.Add(i + 1);

                triangles.Add(i);
                triangles.Add(i + 2);
                triangles.Add(i + 3);
            }

            return triangles.ToArray();
        }

        private static Vector3[] CreateVertices(List<Vector3> points, int curveSteps, StreetSegment segment)
        {
            float curveDistanceMultiplier = 4f;
            Vector3[] vertices = new Vector3[(points.Count - 1) * 4 + (points.Count - 2) * 2 * curveSteps];
            float halfThickness = segment.Thickness * 0.5f;
            int vertIndex = 0;

            //Start points without inset
            Vector3 normal = CalculateNormal(points[0], points[1]);
            vertices[vertIndex++] = halfThickness * normal;
            vertices[vertIndex++] = -halfThickness * normal;

            for (int i = 1; i < points.Count - 1; i++)
            {
                //Verts before the curve
                Vector3 dir = CalculateNormalizedDirection(points[i], points[i - 1]) * curveDistanceMultiplier;
                normal = CalculateNormal(points[i - 1], points[i]);
                vertices[vertIndex++] = points[i] + dir - points[0] + halfThickness * normal;
                vertices[vertIndex++] = points[i] + dir - points[0] - halfThickness * normal;

                //Curve
                float timeStep = 1f / (curveSteps + 1);
                float time = timeStep;

                Vector3 tangent1 = (points[i - 1] - points[i]).normalized;
                Vector3 tangent2 = (points[i + 1] - points[i]).normalized;
                Vector3 handle = points[i] - (tangent1 + tangent2).normalized * 0.5f;
                Vector3 start = points[i] + curveDistanceMultiplier * tangent1;
                Vector3 end = points[i] + curveDistanceMultiplier * tangent2;

                for (int j = 0; j < curveSteps; j++)
                {
                    Vector3 point = MathUtil.QuadraticLerp(start, handle, end, time);
                    normal = MathUtil.GetQuadraticBezierNormal(start, handle, end, time);

                    vertices[vertIndex++] = point - points[0] + halfThickness * normal;
                    vertices[vertIndex++] = point - points[0] - halfThickness * normal;

                    time += timeStep;
                }

                //Verts after the curve
                dir = CalculateNormalizedDirection(points[i], points[i + 1]) * curveDistanceMultiplier;
                normal = CalculateNormal(points[i], points[i + 1]);
                vertices[vertIndex++] = points[i] + dir - points[0] + halfThickness * normal;
                vertices[vertIndex++] = points[i] + dir - points[0] - halfThickness * normal;
            }

            //End points without inset
            normal = CalculateNormal(points[points.Count - 2], points[points.Count - 1]);
            vertices[vertIndex++] = points[points.Count - 1] - points[0] + halfThickness * normal;
            vertices[vertIndex++] = points[points.Count - 1] - points[0] - halfThickness * normal;

            return vertices;
        }

        private static Vector3 CalculateNormal(Vector3 pointA, Vector3 pointB)
        {
            Vector2 direction2d = new Vector2(pointB.x - pointA.x, pointB.z - pointA.z);
            Vector2 normal2d = Vector2.Perpendicular(direction2d);
            return new Vector3(normal2d.x, 0f, normal2d.y).normalized;
        }

        private static Vector3 CalculateNormalizedDirection(Vector3 pointA, Vector3 pointB)
        {
            return (pointB - pointA).normalized;
        }
    }
}