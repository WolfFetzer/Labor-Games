using System.Collections.Generic;
using Streets;
using UnityEngine;

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
    private readonly LayerMask _floorLayerMask = LayerMask.GetMask("Floor");
    private readonly LayerMask _streetLayerMask = LayerMask.GetMask("Street");
    private bool _isActive;
    private Vector3 _lastPoint;

    private StreetSegment _lastSegment;
    
    //Start = where the construction starts, end = where the street ends
    private PlacementMode _startPlacementMode;
    private PlacementMode _endPlacementMode;

    private MeshRenderer _previewRenderer;
    private Transform _previewTransform;

    public ConstructionManager()
    {
        _cursorTransform = Object.Instantiate(GameManager.Instance.streetCursorPrefab).transform;
        _cursorStreetPreview = _cursorTransform.GetComponent<StreetPreview>();
    }

    public void UpdateMousePosition(Camera camera)
    {
        Ray ray = camera.ScreenPointToRay(Input.mousePosition);

        if (_isActive)
        {
            if (Physics.Raycast(ray, out RaycastHit streetHit, 10000f, _streetLayerMask))
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
            //If a street is hit directly
            if (Physics.Raycast(ray, out RaycastHit streetHit, 10000f, _streetLayerMask))
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
        if (Physics.Raycast(ray, out RaycastHit hit, 10000f, _streetLayerMask))
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
        Intersection startIntersection = null;
        Intersection endIntersection = null;
        Vector3 point;

        if (Physics.Raycast(ray, out RaycastHit hit, 10000f, _streetLayerMask))
        {
            //Get the hit segment and the center point
            StreetSegment hitSegment = hit.transform.GetComponent<StreetSegment>();
            point = GetStreetPoint(hitSegment, hit, out _endPlacementMode);

            if (_endPlacementMode == PlacementMode.Split && _startPlacementMode == PlacementMode.Split)
            {
                endIntersection = SplitStreetSegment(ref hitSegment, point);
            }
            else if (_endPlacementMode == PlacementMode.AdditionAtTheBeginning)
            {
                if (_startPlacementMode == PlacementMode.AdditionAtTheBeginning)
                {
                    
                }
                else if (_startPlacementMode == PlacementMode.AdditionAtTheEnd)
                {
                    
                }
                List<Vector3> points = hitSegment.StreetPoints;
                points.Insert(0, _lastPoint);
                StreetSegment segment = StreetGenerator.GenerateComplexStreetGameObject(points).GetComponent<StreetSegment>();
                segment.Intersections[0] = hitSegment.Intersections[0];
                segment.Intersections[1] = hitSegment.Intersections[1];
            
                Object.Destroy(hitSegment.gameObject);

                _isActive = false;
                ResetPreviewStreet();
            
                return;
            }
            else if (_endPlacementMode == PlacementMode.AdditionAtTheEnd)
            {
                List<Vector3> points = hitSegment.StreetPoints;
                points.Add(_lastPoint);
                StreetSegment segment = StreetGenerator.GenerateComplexStreetGameObject(points).GetComponent<StreetSegment>();
                segment.Intersections[0] = hitSegment.Intersections[0];
                segment.Intersections[1] = hitSegment.Intersections[1];
            
                Object.Destroy(hitSegment.gameObject);

                _isActive = false;
                ResetPreviewStreet();
            
                return;
            }
            
        }
        else if (Physics.Raycast(ray, out hit, 10000f, _floorLayerMask))
        {
            point = hit.point;
            _endPlacementMode = PlacementMode.Default;
        }
        else return;
        
        if (_startPlacementMode == PlacementMode.Split)
        {
            startIntersection = SplitStreetSegment(ref _lastSegment, _lastPoint);
        }
        else if (_startPlacementMode == PlacementMode.AdditionAtTheBeginning)
        {
            List<Vector3> points = _lastSegment.StreetPoints;
            points.Insert(0, point);
            StreetSegment segment = StreetGenerator.GenerateComplexStreetGameObject(points).GetComponent<StreetSegment>();
            segment.Intersections[0] = _lastSegment.Intersections[0];
            segment.Intersections[1] = _lastSegment.Intersections[1];
            
            Object.Destroy(_lastSegment.gameObject);
            _lastSegment = null;

            _isActive = false;
            ResetPreviewStreet();
            
            return;
        }
        else if (_startPlacementMode == PlacementMode.AdditionAtTheEnd)
        {
            List<Vector3> points = _lastSegment.StreetPoints;
            points.Add(point);
            StreetSegment segment = StreetGenerator.GenerateComplexStreetGameObject(points).GetComponent<StreetSegment>();
            segment.Intersections[0] = _lastSegment.Intersections[0];
            segment.Intersections[1] = _lastSegment.Intersections[1];
            
            Object.Destroy(_lastSegment.gameObject);
            _lastSegment = null;

            _isActive = false;
            ResetPreviewStreet();
            
            return;
        }
        
        _isActive = false;

        StreetSegment newSegment =
            StreetGenerator.GenerateStraightGameObject(_lastPoint, point).GetComponent<StreetSegment>();
        newSegment.Intersections[0] = startIntersection;
        newSegment.Intersections[1] = endIntersection;

        //Connect the new Intersection if both ends hit a street
        if (startIntersection != null && endIntersection != null)
        {
            startIntersection.AddNeibhor("Zeile 108", endIntersection, newSegment, newSegment.edge1);
            endIntersection.AddNeibhor("Zeile 109", startIntersection, newSegment, newSegment.edge2);
        }

        ResetPreviewStreet();
        
        Debug.Log(_startPlacementMode.ToString() + ", " + _endPlacementMode.ToString());
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
        Vector3 halfStreetThickness = dir * (0.5f * splitSegment.Thickness);

        //Creation of the intersection
        Intersection segmentIntersection = Object.Instantiate(GameManager.Instance.intersectionPrefab, splitPoint,
            Quaternion.identity).GetComponent<Intersection>();

        //Creation of the first split street
        StreetSegment segment =
            StreetGenerator
                .GenerateStraightGameObject(splitSegment.StreetPoints[0], splitPoint - halfStreetThickness)
                .GetComponent<StreetSegment>();
        segment.Intersections[0] = splitSegment.Intersections[0];
        segment.Intersections[1] = segmentIntersection;

        if (segment.Intersections[0] != null)
        {
            segment.Intersections[0].RemoveNeighbor(splitSegment.Intersections[1]);
            segment.Intersections[0].AddNeibhor("Zeile 160", segmentIntersection, segment, segment.edge1);
            segmentIntersection.AddNeibhor("Zeile 161", segment.Intersections[0], segment, segment.edge2);
        }

        //Creation of the second split street
        segment = StreetGenerator
            .GenerateStraightGameObject(splitPoint + halfStreetThickness, splitSegment.StreetPoints[1])
            .GetComponent<StreetSegment>();
        segment.Intersections[0] = segmentIntersection;
        segment.Intersections[1] = splitSegment.Intersections[1];

        if (segment.Intersections[1] != null)
        {
            segment.Intersections[1].RemoveNeighbor(splitSegment.Intersections[0]);
            segment.Intersections[1].AddNeibhor("Zeile 168", segmentIntersection, segment, segment.edge2);
            segmentIntersection.AddNeibhor("Zeile 169", splitSegment.Intersections[1], segment, segment.edge1);
        }

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
                
                intersectionPoint = GetIntersectionPoint(
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
                
                Vector3 pointA = GetIntersectionPoint(
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
                
                Vector3 pointB = GetIntersectionPoint(
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
                
                intersectionPoint = GetIntersectionPoint(
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

            intersectionPoint = GetIntersectionPoint(
                streetHit.point.x, streetHit.point.z,
                streetHit.point.x + normal.x, streetHit.point.z + normal.y,
                points[0].x, points[0].z,
                points[1].x, points[1].z,
                points[0].y
            );
        }

        //TODO Methode für Rückgabe der Informationen wenn es ein Endpunkt ist
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

    private Vector3 GetIntersectionPoint(float a1x, float a1z, float a2x, float a2z, float b1x, float b1z, float b2x,
        float b2z, float y)
    {
        float tmp = (b2x - b1x) * (a2z - a1z) - (b2z - b1z) * (a2x - a1x);
        if (tmp == 0)
        {
            Debug.Log("Not found");
            return new Vector3();
        }

        float mu = ((a1x - b1x) * (a2z - a1z) - (a1z - b1z) * (a2x - a1x)) / tmp;

        return new Vector3(
            b1x + (b2x - b1x) * mu,
            y,
            b1z + (b2z - b1z) * mu
        );
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