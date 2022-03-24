using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LevelGenerator))]
public class PathGenerator : MonoBehaviour
{
    public bool drawPoints = true;

    public Vector3 start;
    public Vector3 end;

    int _mapWidth, _mapDepth;
    float _sizeModifier;

    bool done;

    public List<Vector3> gridPoints = new List<Vector3>();

    int PathSegment;

    public List<Points> openPoints = new List<Points>();
    public List<Points> closedPoints = new List<Points>();

    public void GenerateGrid(int mapWidth, int mapDepth, Vector3 startPosition, float sizeModifier)
    {
        gridPoints = MapGridGenerator.GenerateMapGrid(mapWidth, mapDepth, startPosition, sizeModifier);
        _mapWidth = mapWidth;
        _mapDepth = mapDepth;
        _sizeModifier = sizeModifier;
    }

    public void SearchForPath()
    {
        Points currentPoint = getPoint(start);
        openPoints.Add(currentPoint);
        PathSegment++;

        for (int c = 0; c < 10; c++)
        {
            if (currentPoint.pointPosition == end)
            {
                done = true;
                return;
            }

            Vector3[] directions = getDirections(currentPoint.pointPosition);

            for (int i = 0; i < 4; i++)
            {
                Points newPoint = getPoint(directions[i]);
                
                if (closedPoints.Contains(newPoint))
                    return;

                if (!gridPoints.Contains(newPoint.pointPosition))
                    return;

                if (newPoint.F < currentPoint.F)
                {
                    closedPoints.Add(currentPoint);
                    openPoints.Add(newPoint);
                    currentPoint = newPoint;
                }
                else closedPoints.Add(newPoint);
            }

            PathSegment++;
        }

    }

    Vector3[] getDirections(Vector3 currentPosition)
    {
        Vector3[] directions = new Vector3[4];

        directions[0] = new Vector3(currentPosition.x + _sizeModifier, currentPosition.y, currentPosition.z);
        directions[1] = new Vector3(currentPosition.x, currentPosition.y, currentPosition.z - _sizeModifier);
        directions[2] = new Vector3(currentPosition.x - _sizeModifier, currentPosition.y, currentPosition.z);
        directions[3] = new Vector3(currentPosition.x, currentPosition.y, currentPosition.z + _sizeModifier);

        return directions;
    }

    void ClearPath()
    {
        openPoints.Clear();
        closedPoints.Clear();   
    }

    Points getPoint(Vector3 pointPosition)
    {
        Points newPoint = new Points();

        newPoint.H = GetH(pointPosition);
        newPoint.G = PathSegment;
        newPoint.F = GetF(newPoint);
        newPoint.pointPosition = pointPosition;

        return newPoint;
    }

    #region Generate Important Positions
    public void GetStartAndEndPositions(Vector3 startDoorPosition, Vector3 endDoorPosition)
    {
        start = getPointClosestToPosition(startDoorPosition);
        end = getPointClosestToPosition(endDoorPosition);
    }

    Vector3 getPointClosestToPosition(Vector3 pointPosition)
    {
        Vector3 closestPoint = Vector3.zero;

        for (int i = 0; i < gridPoints.Count; i++)
        {
            if (Vector3.Distance(gridPoints[i], pointPosition) < Vector3.Distance(closestPoint, pointPosition))
                closestPoint = gridPoints[i];
        }

        return closestPoint;
    }
    #endregion

    #region GetValues
    public float GetH(Vector3 pointPosition)
    {
        return Vector3.Distance(pointPosition, end);
    }

    public float GetF(Points point)
    {
        return point.H + point.G;
    }
    #endregion

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying && drawPoints)
            return;

        for (int i = 0; i < gridPoints.Count; i++)
        {
            Gizmos.color = Color.green;

            if (gridPoints[i] == start)
                Gizmos.color = Color.white;
            else if (gridPoints[i] == end)
                Gizmos.color = Color.black;

            Gizmos.DrawCube(gridPoints[i], Vector3.one / 2);
        }

        for (int i = 0; i < openPoints.Count; i++)
        {
            Gizmos.color = Color.red;

            Gizmos.DrawCube(openPoints[i].pointPosition, Vector3.one / 2);
        }
    }
}

[System.Serializable]
public class Points
{
    public float H;
    public float G;
    public float F;
    public Vector3 pointPosition;
    public Points parentPoint;
}