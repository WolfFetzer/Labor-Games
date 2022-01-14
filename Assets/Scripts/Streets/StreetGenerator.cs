using System.Collections.Generic;
using Curves.Util;
using UnityEngine;
using Util;

namespace Streets
{
    public static class StreetGenerator
    {
        public static GameObject GenerateStraightGameObject(Vector3 start, Vector3 end, StreetInfo info)
        {
            GameObject go = new GameObject {layer = 6};

            Vector3 direction = end - start;
            float length = direction.magnitude;
            float halfLength = length / 2f;

            Vector3 position = 0.5f * direction + start;
            go.transform.parent = GameManager.Instance.streetTransform;
            go.transform.position = position;
            go.transform.localRotation = Quaternion.LookRotation(direction, Vector3.up);

            BoxCollider collider = go.AddComponent<BoxCollider>();
            collider.size = new Vector3( info.lanes * 2 * info.trackWidth, 1f, length);

            MeshRenderer renderer = go.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = GameManager.Instance.streetMaterial;

            MeshFilter filter = go.AddComponent<MeshFilter>();

            StreetSegment segment = go.AddComponent<StreetSegment>();
            segment.Init(new List<Vector3> {start, end}, info);

            Mesh mesh = new Mesh();

            Vector3[] vertices = new Vector3[4];
            int[] triangles = new int[6];
            Vector2[] uvs = new Vector2[4];
            float halfThickness = info.lanes * info.trackWidth;/* segment.Thickness * 0.5f;*/

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

        public static GameObject GenerateComplexStreetGameObject(List<Vector3> points, StreetInfo info)
        {
            int curveSteps = 5;

            Debug.Log("Generate Complex");

            GameObject go = new GameObject {layer = 6};
            go.transform.parent = GameManager.Instance.streetTransform;

            Vector3 pos = points[0];
            pos.y = 0.01f;
            go.transform.position = pos;

            MeshRenderer renderer = go.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = GameManager.Instance.streetMaterial;
            MeshFilter filter = go.AddComponent<MeshFilter>();

            StreetSegment segment = go.AddComponent<StreetSegment>();
            segment.Init(points, info);

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

        public static void GenerateIntersectionMesh(Intersection intersection)
        {
            Mesh mesh = new Mesh();
            mesh.name = intersection.name + " Mesh";
            List<Vector3> vertices = new List<Vector3>();
            List<int> triangles = new List<int>();

            Vector3 center = intersection.transform.position;
            Vector3[] intersectionDirections = new Vector3[intersection.SegmentStartingPoints.Count];

            int index = 0;
            foreach (Vector3Tuple tuple in intersection.SegmentStartingPoints)
            {
                intersectionDirections[index++] = (tuple.StartingPoint - center).normalized;
            }

            //float intersectionHalfWidth = 1.5f;
            
            //Anglecheck
            /*
            if (Vector3.SignedAngle(-intersectionDirections[0], intersectionDirections[2], Vector3.up) < 0f)
            {
                Vector3 straightDirection = intersectionDirections[1];
                intersectionDirections[1] = intersectionDirections[2];
                intersectionDirections[2] = straightDirection;
            }
            */
            StreetInfo info = intersection.Segments[0].Info;
            
            Vector3 firstDirection = intersectionDirections[0] * (info.lanes * info.trackWidth);
                
            Vector3 normal = new Vector3(-firstDirection.z, 0f, firstDirection.x);
                
            vertices.Add(new Vector3(-firstDirection.x + normal.x, 0.01f, -firstDirection.z + normal.z));
            vertices.Add(new Vector3(firstDirection.x + normal.x, 0.01f, firstDirection.z + normal.z));
            vertices.Add(new Vector3(-firstDirection.x - normal.x, 0.01f, -firstDirection.z - normal.z));
            vertices.Add(new Vector3(firstDirection.x - normal.x, 0.01f, firstDirection.z - normal.z));

            triangles.Add(0);
            triangles.Add(1);
            triangles.Add(3);
            triangles.Add(0);
            triangles.Add(3);
            triangles.Add(2);
            
            int triangleIndex = 4;
            for (int i = 0; i < intersectionDirections.Length; i++)
            {
                info = intersection.Segments[i].Info;

                Vector3 direction = intersectionDirections[i] * (info.lanes * info.trackWidth);
                normal = new Vector3(-direction.z, 0f, direction.x);

                vertices.Add(new Vector3(direction.x + normal.x, 0.01f, direction.z + normal.z));
                vertices.Add(new Vector3(direction.x - normal.x, 0.01f, direction.z - normal.z));

                Vector3 streetDirection = intersection.SegmentStartingPoints[i].Direction * (info.lanes * info.trackWidth);
                normal = new Vector3(-streetDirection.z, 0f, streetDirection.x);

                vertices.Add(new Vector3(direction.x + streetDirection.x + normal.x, 0.01f, direction.z + streetDirection.z + normal.z));
                vertices.Add(new Vector3(direction.x + streetDirection.x - normal.x, 0.01f, direction.z + streetDirection.z - normal.z));

                triangles.Add(triangleIndex);
                triangles.Add(triangleIndex + 3);
                triangles.Add(triangleIndex + 1);
                triangles.Add(triangleIndex);
                triangles.Add(triangleIndex + 2);
                triangles.Add(triangleIndex + 3);

                triangleIndex += 4;
            }


            mesh.vertices = vertices.ToArray();
            mesh.triangles = triangles.ToArray();
            
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            
            intersection.UpdateMesh(mesh);
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
            float halfThickness = segment.Info.lanes * segment.Info.lanes; /*.Thickness * 0.5f;*/
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
                    Vector3 point = BezierUtil.QuadraticLerp(start, handle, end, time);
                    normal = BezierUtil.GetQuadraticBezierNormal(start, handle, end, time);

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