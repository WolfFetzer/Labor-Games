using System.Collections.Generic;
using Streets;
using UnityEngine;
using Util;

public enum PlacementMode
{
    Default,
    Split,
    AdditionAtTheBeginning,
    AdditionAtTheEnd,
    Intersection
}

public class ConstructionManager
{
    private const float MinEdgeDistance = 5f;

    private readonly StreetPreview _cursorStreetPreview;

    private readonly Transform _cursorTransform;
    private readonly LayerMask _floorLayerMask;
    private readonly LayerMask _streetLayerMask;
    private readonly LayerMask _intersectionLayerMask;
    private bool _isActive;
    private Vector3 _lastPoint;
    private StreetSegment _lastSegment;
    private Intersection _lastIntersection;
    
    //Start = where the construction starts, end = where the street ends
    private PlacementMode _startPlacementMode;
    private PlacementMode _endPlacementMode;

    private MeshRenderer _previewRenderer;
    private Transform _previewTransform;

    private StreetInfo _selectedStreetInfo;

    public ConstructionManager(LayerMask floorLayerMask, LayerMask streetLayerMask, LayerMask intersectionLayerMask, StreetInfo info)
    {
        _floorLayerMask = floorLayerMask;
        _streetLayerMask = streetLayerMask;
        _intersectionLayerMask = intersectionLayerMask;
        _selectedStreetInfo = info;
        _cursorTransform = Object.Instantiate(GameManager.Instance.streetCursorPrefab).transform;
        _cursorStreetPreview = _cursorTransform.GetComponent<StreetPreview>();
    }

    public void UpdateMousePosition(Camera camera)
    {
        Ray ray = camera.ScreenPointToRay(Input.mousePosition);

        if (_isActive)
        {
            if (Physics.Raycast(ray, out RaycastHit intersectionHit, 10000f, _intersectionLayerMask))
            {
                Intersection intersection = intersectionHit.transform.GetComponent<Intersection>();

                // Check to ensure a new street cannot be connected to itself if its only a straight
                //if (segment == _lastSegment && segment.StreetPoints.Count < 3) return;
                
                //Vector3 point = GetStreetPoint(segment, streetHit, out PlacementMode placementMode);

                _cursorTransform.position = intersection.transform.position;
                DrawPreview(intersection.transform.position);
            }
            else if (Physics.Raycast(ray, out RaycastHit streetHit, 10000f, _streetLayerMask))
            {
                StreetSegment segment = streetHit.transform.GetComponent<StreetSegment>();
                
                // Check to ensure a new street cannot be connected to itself if its only a straight
                if (segment == _lastSegment && segment.StreetPoints.Count < 3) return;
                
                Vector3 point = GetStreetPoint(segment, streetHit, out PlacementMode placementMode);

                _cursorTransform.position = point;
                DrawPreview(point);
            }
            else if (Physics.Raycast(ray, out RaycastHit floorHit, 10000f, _floorLayerMask))
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

                _cursorTransform.position = floorHit.point;
                DrawPreview(floorHit.point);
            }
        }
        else
        {
            if (Physics.Raycast(ray, out RaycastHit intersectionHit, 10000f, _intersectionLayerMask))
            {
                Intersection intersection = intersectionHit.transform.GetComponent<Intersection>();
                Vector3 point = intersection.transform.position;

                _cursorStreetPreview.CanBePlaced = true;
                _cursorTransform.position = point;
            }
            //If a street is hit directly
            else if (Physics.Raycast(ray, out RaycastHit streetHit, 10000f, _streetLayerMask))
            {
                StreetSegment segment = streetHit.transform.GetComponent<StreetSegment>();
                Vector3 point = GetStreetPoint(segment, streetHit, out PlacementMode placementMode);

                _cursorStreetPreview.CanBePlaced = true;
                _cursorTransform.position = point;
            }
            else if (Physics.Raycast(ray, out RaycastHit floorHit, 10000f, _floorLayerMask))
            {
                //TODO Check if to close to place. Needs to be cancelled or put over the road.
                RaycastHit[] hits = Physics.SphereCastAll(ray, 2f, 1000f, _streetLayerMask);
                if (hits.Length == 0)
                    _cursorStreetPreview.CanBePlaced = true;
                else
                    _cursorStreetPreview.CanBePlaced = false;

                _cursorTransform.position = floorHit.point;
            }
        }
    }

    public void PlaceConstruction(Camera camera)
    {
        Ray ray = camera.ScreenPointToRay(Input.mousePosition);
        if (_isActive)
        {
            FinishConstruction(ray);
        }
        else if (_cursorStreetPreview.CanBePlaced)
        {
            StartConstruction(ray);
        }
    }

    private void StartConstruction(Ray ray)
    {
        if (Physics.Raycast(ray, out RaycastHit hit, 10000f, _intersectionLayerMask))
        {
            Transform transform = hit.transform;
            _lastIntersection = transform.GetComponent<Intersection>();
            _lastPoint = transform.position;
            _startPlacementMode = PlacementMode.Intersection;
        }
        else if (Physics.Raycast(ray, out hit, 10000f, _streetLayerMask))
        {
            _lastSegment = hit.transform.GetComponent<StreetSegment>();
            _lastPoint = GetStreetPoint(_lastSegment, hit, out _startPlacementMode);
        }
        else if (Physics.Raycast(ray, out hit, 10000f))
        {
            _lastSegment = null;
            _lastPoint = hit.point;
            _startPlacementMode = PlacementMode.Default;
        }
        else return;

        _previewTransform = Object.Instantiate(GameManager.Instance.prefab, hit.point, Quaternion.identity).transform;
        _isActive = true;
    }

    private void FinishConstruction(Ray ray)
    {
        Intersection endIntersection = null;

        //Hit intersection
        if (Physics.Raycast(ray, out RaycastHit hit, 10000f, _intersectionLayerMask))
        {
            Transform intersectionTransform = hit.transform;
            endIntersection = intersectionTransform.GetComponent<Intersection>();
            
            Vector3 endPoint = GenerateCrossingInsetPoint(intersectionTransform.position, _lastPoint, _selectedStreetInfo);
            FinishPlacement(endPoint, endIntersection);
            StreetGenerator.GenerateIntersectionMesh(endIntersection);
        }
        //Hit street segment
        else if (Physics.Raycast(ray, out hit, 10000f, _streetLayerMask))
        {
            //Get the hit segment and the center point
            StreetSegment hitSegment = hit.transform.GetComponent<StreetSegment>();
            Vector3 point = GetStreetPoint(hitSegment, hit, out _endPlacementMode);

            if (_endPlacementMode == PlacementMode.Split)
            {
                endIntersection = SplitStreetSegment(ref hitSegment, point);
                FinishPlacement(GenerateCrossingInsetPoint(point, _lastPoint, _selectedStreetInfo), endIntersection);
                StreetGenerator.GenerateIntersectionMesh(endIntersection);
            }
            else
            {
                Intersection startIntersection = null;
                if (_endPlacementMode == PlacementMode.AdditionAtTheBeginning)
                {
                    if (_startPlacementMode == PlacementMode.Default)
                    {
                        List<Vector3> points = hitSegment.StreetPoints;
                        points.Insert(0, _lastPoint);
                        StreetSegment segment = StreetGenerator.GenerateComplexStreetGameObject(points, _selectedStreetInfo).GetComponent<StreetSegment>();
                        segment.IntersectionAtEnd = hitSegment.IntersectionAtEnd;
            
                        Object.Destroy(hitSegment.gameObject);
                    }
                    else if (_startPlacementMode == PlacementMode.Split)
                    {
                        startIntersection = SplitStreetSegment(ref _lastSegment, _lastPoint);
                    
                        List<Vector3> points = hitSegment.StreetPoints;
                        points.Insert(0, _lastPoint);
                        StreetSegment segment = StreetGenerator.GenerateComplexStreetGameObject(points, _selectedStreetInfo).GetComponent<StreetSegment>();
                        segment.IntersectionAtStart = startIntersection;
                        segment.IntersectionAtEnd = hitSegment.IntersectionAtEnd;
                    
                        StreetGenerator.GenerateIntersectionMesh(startIntersection);
                    
                        Object.Destroy(hitSegment.gameObject);
                    }
                    else if (_startPlacementMode == PlacementMode.AdditionAtTheBeginning)
                    {
                        List<Vector3> startPoints = _lastSegment.StreetPoints;
                        startPoints.Reverse();
                        List<Vector3> points = hitSegment.StreetPoints;
                        points.InsertRange(0, startPoints);
                        StreetSegment segment = StreetGenerator.GenerateComplexStreetGameObject(points, _selectedStreetInfo).GetComponent<StreetSegment>();
                        segment.IntersectionAtStart = _lastSegment.IntersectionAtEnd;
                        segment.IntersectionAtEnd = hitSegment.IntersectionAtEnd;
                    
                        Object.Destroy(_lastSegment.gameObject);
                        Object.Destroy(hitSegment.gameObject);
                    }
                    else if (_startPlacementMode == PlacementMode.AdditionAtTheEnd)
                    {
                        List<Vector3> points = _lastSegment.StreetPoints;
                        points.AddRange(hitSegment.StreetPoints);
                        StreetSegment segment = StreetGenerator.GenerateComplexStreetGameObject(points, _selectedStreetInfo).GetComponent<StreetSegment>();
                        segment.IntersectionAtStart = _lastSegment.IntersectionAtStart;
                        segment.IntersectionAtEnd = hitSegment.IntersectionAtEnd;
            
                        Object.Destroy(_lastSegment.gameObject);
                        Object.Destroy(hitSegment.gameObject);
                    }
                }
                else if (_endPlacementMode == PlacementMode.AdditionAtTheEnd)
                {
                    if (_startPlacementMode == PlacementMode.Default)
                    {
                        List<Vector3> points = hitSegment.StreetPoints;
                        points.Add(_lastPoint);
                        StreetSegment segment = StreetGenerator.GenerateComplexStreetGameObject(points, _selectedStreetInfo).GetComponent<StreetSegment>();
                        segment.IntersectionAtStart = hitSegment.IntersectionAtStart;
            
                        Object.Destroy(hitSegment.gameObject);
                    }
                    else if (_startPlacementMode == PlacementMode.Split)
                    {
                        startIntersection = SplitStreetSegment(ref _lastSegment, _lastPoint);
                    
                        List<Vector3> points = hitSegment.StreetPoints;
                        points.Add(_lastPoint);
                        StreetSegment segment = StreetGenerator.GenerateComplexStreetGameObject(points, _selectedStreetInfo).GetComponent<StreetSegment>();
                        segment.IntersectionAtStart = hitSegment.IntersectionAtStart;
                        segment.IntersectionAtEnd = startIntersection;
                    
                        Object.Destroy(hitSegment.gameObject);
                    }
                    else if (_startPlacementMode == PlacementMode.AdditionAtTheBeginning)
                    {
                        List<Vector3> points = hitSegment.StreetPoints;
                        points.AddRange(_lastSegment.StreetPoints);
                        StreetSegment segment = StreetGenerator.GenerateComplexStreetGameObject(points, _selectedStreetInfo).GetComponent<StreetSegment>();
                        segment.IntersectionAtStart = hitSegment.IntersectionAtStart;
                        segment.IntersectionAtEnd = _lastSegment.IntersectionAtEnd;
                    
                        Object.Destroy(_lastSegment.gameObject);
                        Object.Destroy(hitSegment.gameObject);
                    }
                    else if (_startPlacementMode == PlacementMode.AdditionAtTheEnd)
                    {
                        List<Vector3> startPoints = _lastSegment.StreetPoints;
                        startPoints.Reverse();
                        List<Vector3> points = hitSegment.StreetPoints;
                        points.AddRange(startPoints);
                        StreetSegment segment = StreetGenerator.GenerateComplexStreetGameObject(points, _selectedStreetInfo).GetComponent<StreetSegment>();
                        segment.IntersectionAtStart = hitSegment.IntersectionAtStart;
                        segment.IntersectionAtEnd = _lastSegment.IntersectionAtStart;
                    
                        Object.Destroy(_lastSegment.gameObject);
                        Object.Destroy(hitSegment.gameObject);
                    }
                }
            }
        }
        //Hit floor
        else if (Physics.Raycast(ray, out hit, 10000f, _floorLayerMask))
        {
            _endPlacementMode = PlacementMode.Default;
            FinishPlacement(hit.point, null);
        }
        else return;

        _lastIntersection = null;
        _lastSegment = null;
        _isActive = false;
        ResetPreviewStreet();
    }

    private void FinishPlacement(Vector3 endPoint, Intersection endIntersection)
    {
        Vector3 insetPoint;
        
        switch (_startPlacementMode)
                {
                    case PlacementMode.Default:
                        GenerateStraightStreet(_lastPoint, endPoint, null, endIntersection);
                        break;
                    case PlacementMode.Intersection:
                        insetPoint = GenerateCrossingInsetPoint(_lastPoint, endPoint, _selectedStreetInfo);
                        GenerateStraightStreet(insetPoint, endPoint, _lastIntersection, endIntersection);
                        StreetGenerator.GenerateIntersectionMesh(_lastIntersection);
                        break;
                    case PlacementMode.Split:
                        Intersection startIntersection = SplitStreetSegment(ref _lastSegment, _lastPoint);
                        insetPoint = GenerateCrossingInsetPoint(_lastPoint, endPoint, _selectedStreetInfo);
                        GenerateStraightStreet(insetPoint, endPoint, startIntersection, endIntersection);
                        StreetGenerator.GenerateIntersectionMesh(startIntersection);
                        break;
                    case PlacementMode.AdditionAtTheBeginning:
                    {
                        List<Vector3> points = _lastSegment.StreetPoints;
                        points.Insert(0, endPoint);
                        StreetSegment segment = StreetGenerator.GenerateComplexStreetGameObject(points, _selectedStreetInfo).GetComponent<StreetSegment>();
                        segment.IntersectionAtStart = endIntersection;
                        segment.IntersectionAtEnd = _lastSegment.IntersectionAtEnd;
            
                        Object.Destroy(_lastSegment.gameObject);
                        break;
                    }
                    case PlacementMode.AdditionAtTheEnd:
                    {
                        List<Vector3> points = _lastSegment.StreetPoints;
                        points.Add(endPoint);
                        StreetSegment segment = StreetGenerator.GenerateComplexStreetGameObject(points, _selectedStreetInfo).GetComponent<StreetSegment>();
                        segment.IntersectionAtStart = _lastSegment.IntersectionAtStart;
                        segment.IntersectionAtEnd = endIntersection;
            
                        Object.Destroy(_lastSegment.gameObject);
                        break;
                    }
                }
    }

    private Vector3 GenerateCrossingInsetPoint(Vector3 from, Vector3 to, StreetInfo info)
    {
        Vector3 direction = (to - from).normalized;
        return from + info.lanes * 2 * info.trackWidth * direction;
    }

    private StreetSegment GenerateStraightStreet(Vector3 start, Vector3 end, Intersection startIntersection, Intersection endIntersection)
    {
        StreetSegment segment = StreetGenerator.GenerateStraightGameObject(start, end, _selectedStreetInfo).GetComponent<StreetSegment>();
        segment.IntersectionAtStart = startIntersection;
        segment.IntersectionAtEnd = endIntersection;

        if (startIntersection != null)
        {
            startIntersection.AddSegment(segment, StreetDirection.Forward);
        }
        if (endIntersection != null)
        {
            endIntersection.AddSegment(segment, StreetDirection.Backward);
        }

        return segment;
    }

    private void ResetPreviewStreet()
    {
        Object.Destroy(_previewTransform.gameObject);
        _previewTransform = null;
    }

    public void Reset()
    {
        if (_isActive)
        {
            _isActive = false;
            Object.Destroy(_previewTransform.gameObject);
            _lastSegment = null;
        }
    }

    private Intersection SplitStreetSegment(ref StreetSegment splitSegment, Vector3 splitPoint)
    {
        Vector3 dir = (splitPoint - splitSegment.StreetPoints[0]).normalized;
        Vector3 halfStreetThickness = dir * (splitSegment.Info.lanes * 2 * splitSegment.Info.trackWidth);

        //Creation of the intersection
        Intersection segmentIntersection = Object.Instantiate(GameManager.Instance.intersectionPrefab, splitPoint,
            Quaternion.identity).GetComponent<Intersection>();
        segmentIntersection.transform.parent = GameManager.Instance.intersectionTransform;

        //Creation of the first split street
        StreetSegment segment =
            StreetGenerator
                .GenerateStraightGameObject(splitSegment.StreetPoints[0], splitPoint - halfStreetThickness, _selectedStreetInfo)
                .GetComponent<StreetSegment>();
        segment.IntersectionAtStart = splitSegment.IntersectionAtStart;
        segment.IntersectionAtEnd = segmentIntersection;

        if (segment.IntersectionAtStart != null)
        {
            segment.IntersectionAtStart.RemoveNeighbor(splitSegment.IntersectionAtEnd);
            segment.IntersectionAtStart.AddSegment(segment, StreetDirection.Forward);
        }
        segmentIntersection.AddSegment(segment, StreetDirection.Backward);

        //Creation of the second split street
        segment = StreetGenerator
            .GenerateStraightGameObject(splitPoint + halfStreetThickness, splitSegment.StreetPoints[1], _selectedStreetInfo)
            .GetComponent<StreetSegment>();
        segment.IntersectionAtStart = segmentIntersection;
        segment.IntersectionAtEnd = splitSegment.IntersectionAtEnd;

        if (segment.IntersectionAtEnd != null)
        {
            segment.IntersectionAtEnd.RemoveNeighbor(splitSegment.IntersectionAtStart);
            segment.IntersectionAtEnd.AddSegment(segment, StreetDirection.Backward);
        }
        segmentIntersection.AddSegment(segment, StreetDirection.Forward);

        //Destroy the old segment
        Object.Destroy(splitSegment.gameObject);
        splitSegment = null;

        return segmentIntersection;
    }

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
            else if(index > 0)
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
                {
                    intersectionPoint = pointA;
                }
                else
                {
                    intersectionPoint = pointB;
                }
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

        if (Vector3.Distance(segment.StreetPoints[segment.StreetPoints.Count - 1], intersectionPoint) <= MinEdgeDistance)
        {
            placementMode = PlacementMode.AdditionAtTheEnd;
            return segment.StreetPoints[segment.StreetPoints.Count - 1];
        }
        
        placementMode = PlacementMode.Split;
        return intersectionPoint;
    }

    private void DrawPreview(Vector3 point)
    {
        Vector3 direction = point - _lastPoint;
        direction.y = 0f;

        float magnitude = direction.magnitude;
        if (magnitude < 1f)
        {
            _previewTransform.localScale = new Vector3(0f, 0f, 0f);
        }
        else
        {
            Vector3 scale = new Vector3(1f, 0.05f, magnitude);
            _previewTransform.position = direction * 0.5f + _lastPoint;
            _previewTransform.localScale = scale;
            _previewTransform.localRotation = Quaternion.LookRotation(direction);
        }
    }
}