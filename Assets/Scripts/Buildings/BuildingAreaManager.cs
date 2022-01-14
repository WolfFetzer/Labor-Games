using Buildings.BuildingArea;
using UnityEngine;
using Util;

public class BuildingAreaManager
{
    private readonly LayerMask _areaLayerMask;

    public BuildingAreaManager(LayerMask areaLayerMask)
    {
        _areaLayerMask = areaLayerMask;
    }
    
    public void UpdateMousePosition(Camera camera)
    {
        
    }

    public void SetArea(Camera camera, AreaType areaType)
    {
        Ray ray = camera.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, 10000f, _areaLayerMask))
        {
            BuildingArea area = hit.collider.GetComponent<BuildingArea>();

            BoxCollider collider = (BoxCollider) hit.collider;
            Vector3 normal = (hit.transform.position - hit.transform.parent.position).normalized;
            Vector3 tangent = new Vector3(-normal.z, 0f, normal.x).normalized;
            Vector3 startPoint = collider.bounds.center - collider.size.z * 0.5f * tangent;

            float distance = VectorUtil.GetLinePositionDistance(startPoint, tangent, hit.point, normal);
            area.SetAreaType(distance, areaType);
        }
    }
}