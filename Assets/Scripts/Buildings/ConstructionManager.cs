using System.Collections.Generic;
using Streets;
using UnityEngine;
using Util;

public enum PlacementMode
{
    Floor,
    Split,
    AdditionAtTheBeginning,
    AdditionAtTheEnd,
    Intersection
}

public enum RaycastHitInfo
{
    None,
    Floor,
    Intersection,
    Street
}

public class ConstructionManager
{
    //Min Distance to the edge before snapping and being handled as addition
    private const float MinEdgeDistance = 5f;

    private readonly StreetPreview _cursorStreetPreview;
    private readonly Transform _cursorTransform;
    private readonly LayerMask _floorLayerMask;
    private readonly LayerMask _intersectionLayerMask;
    private readonly LayerMask _streetLayerMask;
    private bool _isActive;
    private Intersection _lastIntersection;
    private Vector3 _lastPoint;
    private StreetSegment _lastSegment;
    private Transform _previewTransform;
    private readonly StreetInfo _streetInfo;

    //Start = where the construction starts, end = where the street ends
    private PlacementMode _startPlacementMode;
    private PlacementMode _endPlacementMode;

    public ConstructionManager(LayerMask floorLayerMask, LayerMask streetLayerMask, LayerMask intersectionLayerMask,
        StreetInfo info)
    {
        _floorLayerMask = floorLayerMask;
        _streetLayerMask = streetLayerMask;
        _intersectionLayerMask = intersectionLayerMask;
        _streetInfo = info;
        _cursorTransform = Object.Instantiate(GameManager.Instance.streetCursorPrefab).transform;
        _cursorStreetPreview = _cursorTransform.GetComponent<StreetPreview>();
    }

    public void UpdateMousePosition(Camera camera)
    {
        switch (Raycast(camera, out RaycastHit hit))
        {
            case RaycastHitInfo.None: return;
            case RaycastHitInfo.Floor:
                HandleFloorUpdate(hit);
                return;
            case RaycastHitInfo.Intersection:
                HandleIntersectionUpdate(hit);
                return;
            case RaycastHitInfo.Street:
                HandleStreetUpdate(hit);
                return;
        }

        /*
         * Anglechecks
         * Vector3 dir1 = segment.StreetPoints[1] - segment.StreetPoints[0];
                Vector3 dir2 = streetHit.point - _lastPoint;
                float angle = Vector3.SignedAngle(dir1, dir2, Vector3.up);
                if (angle > 0)
                {
                    if (angle > 35f && angle < 145f)
                    {
                        Debug.Log("Angle (linke Seite) ist ok: " + angle);
                    }
                    else Debug.Log("Angle (linke Seite) ist nicht ok: " + angle);
                }
                else
                {
                    if (angle < -35f && angle > -145f)
                    {
                        Debug.Log("Angle (rechte Seite) ist ok: " + angle);
                    }
                    else Debug.Log("Angle (rechte Seite) ist nicht ok: " + angle);
                }
         */
    }

    public void PressedLeftMouse(Camera camera)
    {
        switch (Raycast(camera, out RaycastHit hit))
        {
            case RaycastHitInfo.None: return;
            case RaycastHitInfo.Floor:
                HandleFloorPlacement(hit);
                return;
            case RaycastHitInfo.Intersection:
                HandleIntersectionPlacement(hit);
                return;
            case RaycastHitInfo.Street:
                HandleStreetPlacement(hit);
                return;
        }
    }

    public void Reset()
    {
        if (_isActive)
        {
            _isActive = false;
            Object.Destroy(_previewTransform.gameObject);
            _previewTransform = null;
            _lastSegment = null;
            _lastIntersection = null;
        }
    }

    
    #region Mouse Position Update

    private void HandleFloorUpdate(RaycastHit hit)
    {
        if (_isActive)
        {
            //Checks for 180° Additions
            /*
            if (_startPlacementMode == PlacementMode.Addition)
            {
                Vector2 endPoint = new Vector2(floorHit.point.x, floorHit.point.z);
                Vector2 startPoint = new Vector2(_lastPoint.x, _lastPoint.z);
                Vector2 pointDir = (endPoint - startPoint).normalized;

                Vector2 streetDir;
                if (_lastPoint == _lastSegment.StreetPoints[0])
                {
                    streetDir = new Vector2(
                        _lastSegment.StreetPoints[0].x - _lastSegment.StreetPoints[1].x,
                        _lastSegment.StreetPoints[0].z - _lastSegment.StreetPoints[1].z
                    ).normalized;
                }
                else
                {
                    streetDir = new Vector2(
                        _lastSegment.StreetPoints[1].x - _lastSegment.StreetPoints[0].x,
                        _lastSegment.StreetPoints[1].z - _lastSegment.StreetPoints[0].z
                    ).normalized;
                }

                float dot = Vector2.Dot(pointDir, streetDir);
                
                if (dot > 0.99f)
                {
                    Vector2 normal = Vector2.Perpendicular(streetDir);

                    Vector3 intersectionPoint = GetIntersectionPoint(
                        endPoint.x, endPoint.y,
                        endPoint.x + normal.x, endPoint.y + normal.y,
                        _lastSegment.StreetPoints[0].x, _lastSegment.StreetPoints[0].z,
                        _lastSegment.StreetPoints[1].x, _lastSegment.StreetPoints[1].z,
                        floorHit.point.y
                    );

                    floorHit.point = intersectionPoint;
                }
            }
            */
            
            CheckPerpendicularSnappingAfterFloorHit(ref hit);

            _cursorTransform.position = hit.point;
            DrawPreview();
        }
        else
        {
            _cursorStreetPreview.CanBePlaced = !Physics.CheckSphere(hit.point, 4f, _streetLayerMask);
            _cursorTransform.position = hit.point;
        }
    }

    private void HandleIntersectionUpdate(RaycastHit hit)
    {
        if (_isActive)
        {
            Intersection intersection = hit.transform.GetComponent<Intersection>();
            _cursorTransform.position = intersection.transform.position;
            DrawPreview();
        }
        else
        {
            Intersection intersection = hit.transform.GetComponent<Intersection>();
            Vector3 point = intersection.transform.position;

            _cursorStreetPreview.CanBePlaced = true;
            _cursorTransform.position = point;
        }
    }

    private void HandleStreetUpdate(RaycastHit hit)
    {
        if (_isActive)
        {
            StreetSegment segment = hit.transform.GetComponent<StreetSegment>();

            // Check to ensure a new street cannot be connected to itself if its only a straight
            if (segment == _lastSegment && segment.StreetPoints.Count < 3) return;
            
            CheckPerpendicularSnappingAfterStreetHit(ref hit, segment);

            Vector3 point = GetStreetPoint(segment, hit, out PlacementMode placementMode);

            _cursorTransform.position = point;
            DrawPreview();
        }
        else
        {
            StreetSegment segment = hit.transform.GetComponent<StreetSegment>();
            Vector3 point = GetStreetPoint(segment, hit, out PlacementMode placementMode);

            _cursorStreetPreview.CanBePlaced = true;
            _cursorTransform.position = point;
        }
    }

    #endregion

    
    #region Left Mouse Input

    private void HandleFloorPlacement(RaycastHit hit)
    {
        if (_isActive)
        {
            _endPlacementMode = PlacementMode.Floor;
            CheckPerpendicularSnappingAfterFloorHit(ref hit);
            HandlePlacementStart(hit.point, null);

            Reset();
        }
        else if (_cursorStreetPreview.CanBePlaced)
        {
            _lastSegment = null;
            _startPlacementMode = PlacementMode.Floor;
            SetActive(hit, hit.point);
        }
    }

    private void HandleIntersectionPlacement(RaycastHit hit)
    {
        if (_isActive)
        {
            Intersection endIntersection = hit.transform.GetComponent<Intersection>();
            HandlePlacementStart(GenerateCrossingInsetPoint(hit.transform.position, _lastPoint, _streetInfo), endIntersection);
            StreetGenerator.GenerateIntersectionMesh(endIntersection);

            Reset();
        }
        else if (_cursorStreetPreview.CanBePlaced)
        {
            _lastIntersection = hit.transform.GetComponent<Intersection>();
            _startPlacementMode = PlacementMode.Intersection;
            SetActive(hit, _lastIntersection.transform.position);
        }
    }

    private void HandleStreetPlacement(RaycastHit hit)
    {
        if (_isActive)
        {
            //Get the hit segment and the center point
            StreetSegment segment = hit.transform.GetComponent<StreetSegment>();
            CheckPerpendicularSnappingAfterStreetHit(ref hit, segment);
            Vector3 point = GetStreetPoint(segment, hit, out _endPlacementMode);

            if (_endPlacementMode == PlacementMode.Split)
            {
                Intersection endIntersection = SplitStreetSegment(ref segment, point);
                HandlePlacementStart(GenerateCrossingInsetPoint(point, _lastPoint, _streetInfo), endIntersection);
                StreetGenerator.GenerateIntersectionMesh(endIntersection);
            }
            else if(_endPlacementMode == PlacementMode.AdditionAtTheBeginning)
            {
                Intersection endIntersection = AddStreetSegment(ref segment, PlacementMode.AdditionAtTheBeginning);
                HandlePlacementStart(GenerateCrossingInsetPoint(point, _lastPoint, _streetInfo), endIntersection);
                StreetGenerator.GenerateIntersectionMesh(endIntersection);
            }
            else
            {
                Intersection endIntersection = AddStreetSegment(ref segment, PlacementMode.AdditionAtTheEnd);
                HandlePlacementStart(GenerateCrossingInsetPoint(point, _lastPoint, _streetInfo), endIntersection);
                StreetGenerator.GenerateIntersectionMesh(endIntersection);
            }

            Reset();
        }
        else if (_cursorStreetPreview.CanBePlaced)
        {
            _lastSegment = hit.transform.GetComponent<StreetSegment>();
            SetActive(hit, GetStreetPoint(_lastSegment, hit, out _startPlacementMode));
        }
    }

    private void HandlePlacementStart(Vector3 endPoint, Intersection endIntersection)
    {
        Vector3 insetPoint;
        Intersection startIntersection;

        switch (_startPlacementMode)
        {
            case PlacementMode.Floor:
                GenerateStraightStreet(_lastPoint, endPoint, null, endIntersection);
                break;
            case PlacementMode.Intersection:
                insetPoint = GenerateCrossingInsetPoint(_lastPoint, endPoint, _streetInfo);
                GenerateStraightStreet(insetPoint, endPoint, _lastIntersection, endIntersection);
                StreetGenerator.GenerateIntersectionMesh(_lastIntersection);
                break;
            case PlacementMode.Split:
                startIntersection = SplitStreetSegment(ref _lastSegment, _lastPoint);
                insetPoint = GenerateCrossingInsetPoint(_lastPoint, endPoint, _streetInfo);
                GenerateStraightStreet(insetPoint, endPoint, startIntersection, endIntersection);
                StreetGenerator.GenerateIntersectionMesh(startIntersection);
                break;
            //Handles the additional placement
            default:
                startIntersection = AddStreetSegment(ref _lastSegment, _startPlacementMode);
                insetPoint = GenerateCrossingInsetPoint(_lastPoint, endPoint, _streetInfo);
                GenerateStraightStreet(insetPoint, endPoint, startIntersection, endIntersection);
                StreetGenerator.GenerateIntersectionMesh(startIntersection);
                break;
        }
    }
    
    #endregion

    
    #region Util

    private void GenerateStraightStreet(Vector3 start, Vector3 end, Intersection startIntersection, Intersection endIntersection)
    {
        StreetSegment segment = StreetGenerator.GenerateStraightGameObject(start, end, _streetInfo).GetComponent<StreetSegment>();
        segment.Lane1.AddIntersection(endIntersection, startIntersection);
        segment.Lane2.AddIntersection(startIntersection, endIntersection);
    }

    private Intersection AddStreetSegment(ref StreetSegment addToSegment, PlacementMode placementMode)
    {
        StreetInfo info = addToSegment.Info;
        Vector3 intersectionPoint;
        //Vector3 startingPoint = addToSegment.StreetPoints[0];
        //Lane oldLane = addToSegment.Lane2;
        Vector3 startingPoint;
        Lane oldLane;
        if (placementMode == PlacementMode.AdditionAtTheBeginning)
        {
            intersectionPoint = addToSegment.StreetPoints[0];
            startingPoint = addToSegment.StreetPoints[1];
            oldLane = addToSegment.Lane1;
        }
        else
        {
            intersectionPoint = addToSegment.StreetPoints[1];
            startingPoint = addToSegment.StreetPoints[0];
            oldLane = addToSegment.Lane2;
        }
        Vector3 inset = GenerateInsetDirection(startingPoint, intersectionPoint, info);
        
        //Destroy the old segment
        Object.Destroy(addToSegment.gameObject);
        addToSegment = null;
        
        //Creation of the intersection
        Intersection segmentIntersection = Object.Instantiate(GameManager.Instance.intersectionPrefab, intersectionPoint, Quaternion.identity).GetComponent<Intersection>();
        segmentIntersection.transform.parent = GameManager.Instance.intersectionTransform;

        //Creation of the first split street
        StreetSegment segment = StreetGenerator.GenerateStraightGameObject(startingPoint, intersectionPoint - inset, info).GetComponent<StreetSegment>();
        segment.Lane1.AddIntersection(segmentIntersection, oldLane.Intersection);
        segment.Lane2.AddIntersection(oldLane.Intersection, segmentIntersection);

        return segmentIntersection;
    }

    private Intersection SplitStreetSegment(ref StreetSegment splitSegment, Vector3 splitPoint)
    {
        StreetInfo info = splitSegment.Info;
        Vector3 startingPoint = splitSegment.StreetPoints[0];
        Vector3 endingPoint = splitSegment.StreetPoints[1];
        Lane oldLane1 = splitSegment.Lane1;
        Lane oldLane2 = splitSegment.Lane2;
        Vector3 inset = GenerateInsetDirection(startingPoint, splitPoint, info);

        //Destroy the old segment
        Object.Destroy(splitSegment.gameObject);
        splitSegment = null;

        //Creation of the intersection
        Intersection segmentIntersection = Object.Instantiate(GameManager.Instance.intersectionPrefab, splitPoint, Quaternion.identity).GetComponent<Intersection>();
        segmentIntersection.transform.parent = GameManager.Instance.intersectionTransform;

        //Creation of the first split street
        StreetSegment segment = StreetGenerator.GenerateStraightGameObject(startingPoint, splitPoint - inset, info).GetComponent<StreetSegment>();
        segment.Lane1.AddIntersection(segmentIntersection, oldLane2.Intersection);
        segment.Lane2.AddIntersection(oldLane2.Intersection, segmentIntersection);

        //Creation of the second split street
        segment = StreetGenerator.GenerateStraightGameObject(splitPoint + inset, endingPoint, _streetInfo).GetComponent<StreetSegment>();
        segment.Lane1.AddIntersection(oldLane1.Intersection, segmentIntersection);
        segment.Lane2.AddIntersection(segmentIntersection, oldLane1.Intersection);

        return segmentIntersection;
    }
    
    private RaycastHitInfo Raycast(Camera camera, out RaycastHit hit)
    {
        Ray ray = camera.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out hit, 10000f, _intersectionLayerMask)) return RaycastHitInfo.Intersection;
        if (Physics.Raycast(ray, out hit, 10000f, _streetLayerMask)) return RaycastHitInfo.Street;
        if (Physics.Raycast(ray, out hit, 10000f, _floorLayerMask)) return RaycastHitInfo.Floor;
        return RaycastHitInfo.None;
    }

    private void DrawPreview()
    {
        Vector3 direction = _cursorTransform.position - _lastPoint;
        direction.y = 0f;

        float magnitude = direction.magnitude;
        if (magnitude < 3f)
        {
            _previewTransform.localScale = new Vector3(0f, 0f, 0f);
        }
        else
        {
            Vector3 scale = new Vector3(_streetInfo.lanes * 2 * _streetInfo.trackWidth, 0.05f,
                magnitude);
            _previewTransform.position = direction * 0.5f + _lastPoint;
            _previewTransform.localScale = scale;
            _previewTransform.localRotation = Quaternion.LookRotation(direction);
        }
    }
    
    private void SetActive(RaycastHit hit, Vector3 lastPoint)
    {
        _lastPoint = lastPoint;
        _previewTransform = Object.Instantiate(GameManager.Instance.prefab, hit.point, Quaternion.identity).transform;
        _isActive = true;
    }
    
    #endregion

    #region Math

    private Vector3 GetStreetPoint(StreetSegment segment, RaycastHit streetHit, out PlacementMode placementMode)
    {
        int indexA = 0;
        int indexB = 1;
        List<Vector3> points = segment.StreetPoints;
        Vector3 intersectionPoint;

        //Points have to be sorted if it's not a straight street
        if (points.Count > 2)
        {
            float minDistance = float.MaxValue;
            int index = 0;
            int currentIndex = 0;

            foreach (Vector3 p in points)
            {
                float dist = Vector3.Distance(streetHit.point, p);
                if (dist < minDistance)
                {
                    minDistance = dist;
                    index = currentIndex;
                }

                currentIndex++;
            }

            if (index == points.Count - 1)
            {
                indexA = index - 1;
                indexB = index;

                Vector2 dir = new Vector2(points[indexB].x - points[indexA].x, points[indexB].z - points[indexA].z);
                Vector2 normal = Vector2.Perpendicular(dir);

                intersectionPoint = VectorUtil.GetIntersectionPoint(
                    streetHit.point.x, streetHit.point.z,
                    streetHit.point.x + normal.x, streetHit.point.z + normal.y,
                    points[indexA].x, points[indexA].z,
                    points[indexB].x, points[indexB].z,
                    points[indexA].y
                );
            }
            else if (index > 0)
            {
                indexA = index - 1;
                indexB = index;

                Vector2 dir1 = new Vector2(points[indexB].x - points[indexA].x, points[indexB].z - points[indexA].z);
                Vector2 normal1 = Vector2.Perpendicular(dir1);

                Vector3 pointA = VectorUtil.GetIntersectionPoint(
                    streetHit.point.x, streetHit.point.z,
                    streetHit.point.x + normal1.x, streetHit.point.z + normal1.y,
                    points[indexA].x, points[indexA].z,
                    points[indexB].x, points[indexB].z,
                    points[indexA].y
                );

                dir1 = new Vector2(points[indexB].x - points[indexA].x, points[indexB].z - points[indexA].z);
                normal1 = Vector2.Perpendicular(dir1);

                indexA = index;
                indexB = index + 1;

                Vector3 pointB = VectorUtil.GetIntersectionPoint(
                    streetHit.point.x, streetHit.point.z,
                    streetHit.point.x + normal1.x, streetHit.point.z + normal1.y,
                    points[indexA].x, points[indexA].z,
                    points[indexB].x, points[indexB].z,
                    points[indexA].y
                );

                if (Vector3.Distance(pointA, streetHit.point) < Vector3.Distance(pointB, streetHit.point))
                    intersectionPoint = pointA;
                else
                    intersectionPoint = pointB;
            }
            else
            {
                Vector2 dir = new Vector2(points[indexB].x - points[indexA].x, points[indexB].z - points[indexA].z);
                Vector2 normal = Vector2.Perpendicular(dir);

                intersectionPoint = VectorUtil.GetIntersectionPoint(
                    streetHit.point.x, streetHit.point.z,
                    streetHit.point.x + normal.x, streetHit.point.z + normal.y,
                    points[indexA].x, points[indexA].z,
                    points[indexB].x, points[indexB].z,
                    points[indexA].y
                );
            }
        }
        //Less than 2 StreetPoints
        else
        {
            Vector2 dir = new Vector2(points[indexB].x - points[indexA].x, points[indexB].z - points[indexA].z);
            Vector2 normal = Vector2.Perpendicular(dir);

            intersectionPoint = VectorUtil.GetIntersectionPoint(
                streetHit.point.x, streetHit.point.z,
                streetHit.point.x + normal.x, streetHit.point.z + normal.y,
                points[0].x, points[0].z,
                points[1].x, points[1].z,
                points[0].y
            );
        }

        if (Vector3.Distance(segment.StreetPoints[0], intersectionPoint) <= MinEdgeDistance)
        {
            placementMode = PlacementMode.AdditionAtTheBeginning;
            return segment.StreetPoints[0];
        }

        if (Vector3.Distance(segment.StreetPoints[segment.StreetPoints.Count - 1], intersectionPoint) <=
            MinEdgeDistance)
        {
            placementMode = PlacementMode.AdditionAtTheEnd;
            return segment.StreetPoints[segment.StreetPoints.Count - 1];
        }

        placementMode = PlacementMode.Split;
        return intersectionPoint;
    }

    private Vector3 GenerateCrossingInsetPoint(Vector3 from, Vector3 to, StreetInfo info)
    {
        return from + GenerateInsetDirection(from, to, info);
    }

    private Vector3 GenerateInsetDirection(Vector3 from, Vector3 to, StreetInfo info)
    {
        Vector3 direction = (to - from).normalized;
        return info.lanes * 2 * info.trackWidth * direction;
    }

    private void CheckPerpendicularSnappingAfterFloorHit(ref RaycastHit hit)
    {
        switch (_startPlacementMode)
        {
            case PlacementMode.Split:
                CheckForPerpendicularPoint(ref hit, _lastSegment.StreetPoints);
                break;
            case PlacementMode.Intersection:
                CheckForPerpendicularPoint(ref hit, _lastIntersection.Edges[0].WayPoints);
                break;
        }
    }
    
    private void CheckPerpendicularSnappingAfterStreetHit(ref RaycastHit hit, StreetSegment segment)
    {
        switch (_startPlacementMode)
        {
            case PlacementMode.Split:
                CheckForPerpendicularPoint(ref hit, _lastSegment.StreetPoints);
                break;
            case PlacementMode.Intersection:
                CheckForPerpendicularPoint(ref hit, _lastIntersection.Edges[0].WayPoints);
                break;
            case PlacementMode.Floor:
                CheckForPerpendicularPoint(ref hit, segment.StreetPoints);
                break;
        }
    }
    
    private void CheckForPerpendicularPoint(ref RaycastHit hit, List<Vector3> streetPoints)
    {
        Vector2 placementDir = new Vector2(hit.point.x - _lastPoint.x, hit.point.z - _lastPoint.z).normalized;
        Vector2 streetDir = new Vector2(streetPoints[0].x - streetPoints[1].x, streetPoints[0].z - streetPoints[1].z).normalized;

        float dot = Vector2.Dot(placementDir, streetDir);
        dot *= dot;
                
        if (dot < 0.01f)
        {
            Vector2 normal = Vector2.Perpendicular(streetDir);
            Vector3 intersectionPoint = VectorUtil.GetIntersectionPoint(
                _lastPoint.x, _lastPoint.z,
                _lastPoint.x + normal.x, _lastPoint.z + normal.y,
                hit.point.x, hit.point.z,
                hit.point.x + streetDir.x, hit.point.z + streetDir.y,
                0f
            );
            hit.point = intersectionPoint;
        }
    }
    
    #endregion

    #region Safe

    /*
     * private void HandleStreetPlacement(RaycastHit hit)
    {
        if (_isActive)
        {
            //Get the hit segment and the center point
            StreetSegment hitSegment = hit.transform.GetComponent<StreetSegment>();
            Vector3 point = GetStreetPoint(hitSegment, hit, out _endPlacementMode);

            if (_endPlacementMode == PlacementMode.Split)
            {
                Intersection endIntersection = SplitStreetSegment(ref hitSegment, point);
                HandlePlacementStart(GenerateCrossingInsetPoint(point, _lastPoint, _streetInfo), endIntersection);
                StreetGenerator.GenerateIntersectionMesh(endIntersection);
            }
            else
            {
                Intersection startIntersection = null;
                if (_endPlacementMode == PlacementMode.AdditionAtTheBeginning)
                {
                    if (_startPlacementMode == PlacementMode.Floor)
                    {
                        List<Vector3> points = hitSegment.StreetPoints;
                        points.Insert(0, _lastPoint);
                        StreetSegment segment = StreetGenerator
                            .GenerateComplexStreetGameObject(points, _streetInfo).GetComponent<StreetSegment>();

                        segment.Lane1.AddIntersection(hitSegment.Lane1.Intersection, null);

                        Object.Destroy(hitSegment.gameObject);
                    }
                    else if (_startPlacementMode == PlacementMode.Split)
                    {
                        startIntersection = SplitStreetSegment(ref _lastSegment, _lastPoint);

                        List<Vector3> points = hitSegment.StreetPoints;
                        points.Insert(0, _lastPoint);
                        StreetSegment segment = StreetGenerator
                            .GenerateComplexStreetGameObject(points, _streetInfo).GetComponent<StreetSegment>();

                        segment.Lane1.AddIntersection(hitSegment.Lane1.Intersection, startIntersection);
                        segment.Lane2.AddIntersection(startIntersection, hitSegment.Lane1.Intersection);

                        StreetGenerator.GenerateIntersectionMesh(startIntersection);

                        Object.Destroy(hitSegment.gameObject);
                    }
                    else if (_startPlacementMode == PlacementMode.AdditionAtTheBeginning)
                    {
                        List<Vector3> startPoints = _lastSegment.StreetPoints;
                        startPoints.Reverse();
                        List<Vector3> points = hitSegment.StreetPoints;
                        points.InsertRange(0, startPoints);
                        StreetSegment segment = StreetGenerator
                            .GenerateComplexStreetGameObject(points, _streetInfo).GetComponent<StreetSegment>();

                        segment.Lane1.AddIntersection(hitSegment.Lane1.Intersection, _lastSegment.Lane1.Intersection);
                        segment.Lane2.AddIntersection(_lastSegment.Lane1.Intersection, hitSegment.Lane1.Intersection);

                        Object.Destroy(_lastSegment.gameObject);
                        Object.Destroy(hitSegment.gameObject);
                    }
                    else if (_startPlacementMode == PlacementMode.AdditionAtTheEnd)
                    {
                        List<Vector3> points = _lastSegment.StreetPoints;
                        points.AddRange(hitSegment.StreetPoints);
                        StreetSegment segment = StreetGenerator
                            .GenerateComplexStreetGameObject(points, _streetInfo).GetComponent<StreetSegment>();

                        segment.Lane1.AddIntersection(hitSegment.Lane1.Intersection, _lastSegment.Lane2.Intersection);
                        segment.Lane2.AddIntersection(_lastSegment.Lane2.Intersection, hitSegment.Lane1.Intersection);

                        Object.Destroy(_lastSegment.gameObject);
                        Object.Destroy(hitSegment.gameObject);
                    }
                }
                else if (_endPlacementMode == PlacementMode.AdditionAtTheEnd)
                {
                    if (_startPlacementMode == PlacementMode.Floor)
                    {
                        List<Vector3> points = hitSegment.StreetPoints;
                        points.Add(_lastPoint);
                        StreetSegment segment = StreetGenerator
                            .GenerateComplexStreetGameObject(points, _streetInfo).GetComponent<StreetSegment>();

                        segment.Lane2.AddIntersection(hitSegment.Lane2.Intersection, null);

                        Object.Destroy(hitSegment.gameObject);
                    }
                    else if (_startPlacementMode == PlacementMode.Split)
                    {
                        startIntersection = SplitStreetSegment(ref _lastSegment, _lastPoint);

                        List<Vector3> points = hitSegment.StreetPoints;
                        points.Add(_lastPoint);
                        StreetSegment segment = StreetGenerator
                            .GenerateComplexStreetGameObject(points, _streetInfo).GetComponent<StreetSegment>();

                        segment.Lane1.AddIntersection(startIntersection, hitSegment.Lane2.Intersection);
                        segment.Lane2.AddIntersection(hitSegment.Lane2.Intersection, startIntersection);

                        Object.Destroy(hitSegment.gameObject);
                    }
                    else if (_startPlacementMode == PlacementMode.AdditionAtTheBeginning)
                    {
                        List<Vector3> points = hitSegment.StreetPoints;
                        points.AddRange(_lastSegment.StreetPoints);
                        StreetSegment segment = StreetGenerator
                            .GenerateComplexStreetGameObject(points, _streetInfo).GetComponent<StreetSegment>();

                        segment.Lane1.AddIntersection(_lastSegment.Lane1.Intersection, hitSegment.Lane2.Intersection);
                        segment.Lane2.AddIntersection(hitSegment.Lane2.Intersection, _lastSegment.Lane1.Intersection);

                        Object.Destroy(_lastSegment.gameObject);
                        Object.Destroy(hitSegment.gameObject);
                    }
                    else if (_startPlacementMode == PlacementMode.AdditionAtTheEnd)
                    {
                        List<Vector3> startPoints = _lastSegment.StreetPoints;
                        startPoints.Reverse();
                        List<Vector3> points = hitSegment.StreetPoints;
                        points.AddRange(startPoints);
                        StreetSegment segment = StreetGenerator
                            .GenerateComplexStreetGameObject(points, _streetInfo).GetComponent<StreetSegment>();

                        segment.Lane1.AddIntersection(_lastSegment.Lane2.Intersection, hitSegment.Lane2.Intersection);
                        segment.Lane2.AddIntersection(hitSegment.Lane2.Intersection, _lastSegment.Lane2.Intersection);

                        Object.Destroy(_lastSegment.gameObject);
                        Object.Destroy(hitSegment.gameObject);
                    }
                }
            }

            Reset();
        }
        else if (_cursorStreetPreview.CanBePlaced)
        {
            _lastSegment = hit.transform.GetComponent<StreetSegment>();
            SetActive(hit, GetStreetPoint(_lastSegment, hit, out _startPlacementMode));
        }
    }

    private void HandlePlacementStart(Vector3 endPoint, Intersection endIntersection)
    {
        Vector3 insetPoint;

        switch (_startPlacementMode)
        {
            case PlacementMode.Floor:
                GenerateStraightStreet(_lastPoint, endPoint, null, endIntersection);
                break;
            case PlacementMode.Intersection:
                insetPoint = GenerateCrossingInsetPoint(_lastPoint, endPoint, _streetInfo);
                GenerateStraightStreet(insetPoint, endPoint, _lastIntersection, endIntersection);
                StreetGenerator.GenerateIntersectionMesh(_lastIntersection);
                break;
            case PlacementMode.Split:
                Intersection startIntersection = SplitStreetSegment(ref _lastSegment, _lastPoint);
                insetPoint = GenerateCrossingInsetPoint(_lastPoint, endPoint, _streetInfo);
                GenerateStraightStreet(insetPoint, endPoint, startIntersection, endIntersection);
                StreetGenerator.GenerateIntersectionMesh(startIntersection);
                break;
            case PlacementMode.AdditionAtTheBeginning:
            {
                List<Vector3> points = _lastSegment.StreetPoints;
                points.Insert(0, endPoint);
                StreetSegment segment = StreetGenerator.GenerateComplexStreetGameObject(points, _streetInfo)
                    .GetComponent<StreetSegment>();

                segment.Lane1.AddIntersection(_lastSegment.Lane1.Intersection, endIntersection);
                segment.Lane2.AddIntersection(endIntersection, _lastSegment.Lane1.Intersection);

                Object.Destroy(_lastSegment.gameObject);
                break;
            }
            case PlacementMode.AdditionAtTheEnd:
            {
                List<Vector3> points = _lastSegment.StreetPoints;
                points.Add(endPoint);
                StreetSegment segment = StreetGenerator.GenerateComplexStreetGameObject(points, _streetInfo)
                    .GetComponent<StreetSegment>();

                segment.Lane1.AddIntersection(endIntersection, _lastSegment.Lane2.Intersection);
                segment.Lane2.AddIntersection(_lastSegment.Lane2.Intersection, endIntersection);

                Object.Destroy(_lastSegment.gameObject);
                break;
            }
        }
     */

    #endregion
}