using System;
using UnityEngine;

namespace Buildings.BuildingArea
{
    public class BuildingArea : MonoBehaviour
    {
        [SerializeField] private float fieldDepth = 10f;
        [SerializeField] private int fieldWidth = 10;

        public int FieldAmount { get; private set; }
        private AreaField[] _areaFields;
        public StreetSegment StreetSegment { get; private set; }
        public bool IsRightSide { get; private set; }

        private BoxCollider _collider;
        private MeshFilter _meshFilter;

        private void Awake()
        {
            _collider = GetComponent<BoxCollider>();
            _meshFilter = GetComponent<MeshFilter>();
        }


        public void Init(float sideShift, float width, StreetSegment segment)
        {
            StreetSegment = segment;
            
            transform.localPosition = new Vector3(sideShift, 0f, 0f);
            FieldAmount = (int) width / fieldWidth;
            _areaFields = new AreaField[FieldAmount];
            
            _collider.size = new Vector3(fieldDepth, 0.01f, _areaFields.Length * fieldWidth);

            float sideCorrectedDepth = fieldDepth;

            if (sideShift < 0f)
            {
                sideCorrectedDepth = -fieldDepth;
                name = "Left area";
                IsRightSide = false;
                _meshFilter.mesh = CreateLeftSideMesh();

                float startZ = _areaFields.Length / 2f * fieldWidth - fieldWidth * 0.5f;
                for (int i = 0; i < _areaFields.Length; i++)
                {
                    float z = startZ - i * fieldWidth;
                    _areaFields[i] = new AreaField(new Vector3(0f, 0f, z), i, this);
                }
            }
            else
            {
                name = "Right area";
                IsRightSide = true;
                _meshFilter.mesh = CreateRightSideMesh();

                float startZ = -_areaFields.Length / 2f * fieldWidth + fieldWidth * 0.5f;
                for (int i = 0; i < _areaFields.Length; i++)
                {
                    float z = startZ + i * fieldWidth;
                    _areaFields[i] = new AreaField(new Vector3(0f, 0f, z), i, this);
                }
            }

            _collider.center = new Vector3(sideCorrectedDepth * 0.5f, 0f, 0f);
        }

        public void SetAreaType(float dist, AreaType type)
        {
            int pos = (int) dist / 10;
            
            Mesh mesh = _meshFilter.mesh;

            int startIndex = pos * 4;
            Vector2[] uvs = mesh.uv;

            switch (type)
            {
                case AreaType.None:
                    uvs[startIndex++] = new Vector2(1f, 0f);
                    uvs[startIndex++] = new Vector2(1f, 0.5f);
                    uvs[startIndex++] = new Vector2(0.5f, 0f);
                    uvs[startIndex] = new Vector2(0.5f, 0.5f);
                    break;
                case AreaType.Residential:
                    uvs[startIndex++] = new Vector2(0.5f, 0.5f);
                    uvs[startIndex++] = new Vector2(0.5f, 1f);
                    uvs[startIndex++] = new Vector2(0f, 0.5f);
                    uvs[startIndex] = new Vector2(0f, 1f);
                    break;
                case AreaType.Commercial:
                    uvs[startIndex++] = new Vector2(0.5f, 0f);
                    uvs[startIndex++] = new Vector2(0.5f, 0.5f);
                    uvs[startIndex++] = new Vector2(0f, 0f);
                    uvs[startIndex] = new Vector2(0f, 0.5f);
                    break;
                case AreaType.Industrial:
                    uvs[startIndex++] = new Vector2(1f, 0.5f);
                    uvs[startIndex++] = new Vector2(1f, 1f);
                    uvs[startIndex++] = new Vector2(0.5f, 0.5f);
                    uvs[startIndex] = new Vector2(0.5f, 1f);
                    break;

            }

            mesh.uv = uvs;

            _areaFields[pos].Type = type;
        }

        private Mesh CreateRightSideMesh()
        {
            Mesh mesh = new Mesh();
            InitArrays(out Vector3[] vertices, out int[] triangles, out Vector2[] uvs);

            float startZ = -_areaFields.Length / 2f * fieldWidth;

            int index = 0;
            int triIndex = 0;
            int uvIndex = 0;

            for (int i = 0; i < _areaFields.Length; i++)
            {
                float z = startZ + i * fieldWidth;
                vertices[index++] = new Vector3(0f, 0.01f, z);
                vertices[index++] = new Vector3(fieldDepth, 0.01f, z);
                vertices[index++] = new Vector3(0f, 0.01f, z + fieldWidth);
                vertices[index++] = new Vector3(fieldDepth, 0.01f, z + fieldWidth);

                int startTri = i * 4;
                triangles[triIndex++] = startTri;
                triangles[triIndex++] = startTri + 3;
                triangles[triIndex++] = startTri + 1;

                triangles[triIndex++] = startTri;
                triangles[triIndex++] = startTri + 2;
                triangles[triIndex++] = startTri + 3;

                uvs[uvIndex++] = new Vector2(1f, 0f);
                uvs[uvIndex++] = new Vector2(1f, 0.5f);
                uvs[uvIndex++] = new Vector2(0.5f, 0f);
                uvs[uvIndex++] = new Vector2(0.5f, 0.5f);
            }

            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.uv = uvs;

            mesh.RecalculateBounds();
            mesh.RecalculateNormals();

            return mesh;
        }

        private Mesh CreateLeftSideMesh()
        {
            Mesh mesh = new Mesh();
            InitArrays(out Vector3[] vertices, out int[] triangles, out Vector2[] uvs);

            float startZ = _areaFields.Length / 2f * fieldWidth;

            int index = 0;
            int triIndex = 0;
            int uvIndex = 0;

            for (int i = 0; i < _areaFields.Length; i++)
            {
                float z = startZ - i * 10f;
                vertices[index++] = new Vector3(0f, 0.01f, z);
                vertices[index++] = new Vector3(-fieldDepth, 0.01f, z);
                vertices[index++] = new Vector3(0f, 0.01f, z - fieldWidth);
                vertices[index++] = new Vector3(-fieldDepth, 0.01f, z - fieldWidth);

                int startTri = i * 4;
                triangles[triIndex++] = startTri;
                triangles[triIndex++] = startTri + 3;
                triangles[triIndex++] = startTri + 1;

                triangles[triIndex++] = startTri;
                triangles[triIndex++] = startTri + 2;
                triangles[triIndex++] = startTri + 3;

                uvs[uvIndex++] = new Vector2(1f, 0f);
                uvs[uvIndex++] = new Vector2(1f, 0.5f);
                uvs[uvIndex++] = new Vector2(0.5f, 0f);
                uvs[uvIndex++] = new Vector2(0.5f, 0.5f);
            }

            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.uv = uvs;

            mesh.RecalculateBounds();
            mesh.RecalculateNormals();

            return mesh;
        }

        private void InitArrays(out Vector3[] vertices, out int[] triangles, out Vector2[] uvs)
        {
            vertices = new Vector3[_areaFields.Length * 4];
            triangles = new int[_areaFields.Length * 6];
            uvs = new Vector2[_areaFields.Length * 4];
        }
    }
}