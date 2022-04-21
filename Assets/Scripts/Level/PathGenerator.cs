using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathGenerator : MonoBehaviour
{
    List<Points> openPoints = new List<Points>();
    List<Points> closedPoints = new List<Points>();

    List<Points> pathPoints = new List<Points>();

    List<Vector3> _grid;
    LayerMask _obstacleLayer;
    float _sizeModifier;

    #region Initiate, Find and Clear Path
    public void InitPathFinding(List<Vector3> new_Grid, LayerMask new_obstacleLayer, float new_sizeModifier)
    {
        _grid = new_Grid;
        _obstacleLayer = new_obstacleLayer;
        _sizeModifier = new_sizeModifier;
    }

    public bool CheckPath(Vector3 pointStart, Vector3 pointEnd)
    {
        ClearPath();

        openPoints.Add(getPoint(pointStart, pointStart, pointEnd, 0));

        while(true)
        {
            if (openPoints.Count == 0)
                return false;
            else if (openPoints[0].pointPosition == pointEnd)
                return true;

            Points[] directions = getDirections(openPoints[0].pointPosition, openPoints[0].pointPosition, pointEnd, openPoints[0].G + 1);

            for (int i = 0; i < 4; i++)
            {
                if (evaluatePoint(directions[i]))
                    openPoints.Add(directions[i]);
            }

            closedPoints.Add(openPoints[0]);
            openPoints.Remove(openPoints[0]);
            openPoints.Sort((p1, p2) => p1.F.CompareTo(p2.F));
        }
    }

    public List<Points> GetPath(Vector3 pointStart, Vector3 pointEnd)
    {
        if (!CheckPath(pointStart, pointEnd))
            Debug.Log("Couldn't find any path to end");
        else
        {

            pathPoints.Clear();

            pathPoints.Add(openPoints[0]);
            pathPoints.Add(new Points());

            int i = 1;
            while (true)
            {
                pathPoints[i] = closedPoints.Find(p => p.pointPosition == pathPoints[i - 1].parentPosition);

                if (pathPoints[i].pointPosition == pointStart)
                    break;
                else pathPoints.Add(new Points());

                i++;
            }

            pathPoints.Reverse();
            return pathPoints;
        }
        return null;
    }

    void ClearPath()
    {
        openPoints.Clear();
        closedPoints.Clear();
    }
    #endregion

    #region Get and Evaluate Directions
    Points[] getDirections(Vector3 currentPosition, Vector3 parentPosition, Vector3 goalPoint, int count)
    {
        Points[] directions = new Points[4];

        directions[0] = getPoint(new Vector3(currentPosition.x + _sizeModifier, currentPosition.y, currentPosition.z), parentPosition, goalPoint, count);
        directions[1] = getPoint(new Vector3(currentPosition.x, currentPosition.y, currentPosition.z - _sizeModifier), parentPosition, goalPoint, count);
        directions[2] = getPoint(new Vector3(currentPosition.x - _sizeModifier, currentPosition.y, currentPosition.z), parentPosition, goalPoint, count);
        directions[3] = getPoint(new Vector3(currentPosition.x, currentPosition.y, currentPosition.z + _sizeModifier), parentPosition, goalPoint, count);

        return directions;
    }

    public bool evaluatePoint(Points point)
    {
        Points evaluatePoint = new Points();

        evaluatePoint = closedPoints.Find(p => p.pointPosition == point.pointPosition);
        
        if (evaluatePoint != null)
            return false;

        evaluatePoint = openPoints.Find(p => p.pointPosition == point.pointPosition);
        
        if (evaluatePoint != null)
            return false;

        if (!_grid.Contains(point.pointPosition))
            return false;

        if (Physics.CheckBox(point.pointPosition, Vector3.one / 2, transform.rotation, _obstacleLayer))
            return false;

        return true;
    }
    #endregion

    #region Get Values
    public Points getPoint(Vector3 pointPosition, Vector3 parentPosition, Vector3 goalPosition, int cost)
    {
        Points newPoint = new Points();

        newPoint.H = Vector3.Distance(pointPosition, goalPosition);
        newPoint.G = cost;
        newPoint.F = newPoint.H + newPoint.G;
        newPoint.pointPosition = pointPosition;
        newPoint.parentPosition = parentPosition;

        return newPoint;
    }
    #endregion

    private void OnDrawGizmosSelected()
    {
        for (int i = 0; i < openPoints.Count; i++)
        {
            Gizmos.color = Color.red;

            Gizmos.DrawCube(openPoints[i].pointPosition, Vector3.one / 2);
        }

        for (int i = 0; i < closedPoints.Count; i++)
        {
            Gizmos.color = Color.blue;

            Gizmos.DrawCube(closedPoints[i].pointPosition, Vector3.one / 2);
        }

        for (int i = 0; i < pathPoints.Count; i++)
        {
            Gizmos.color = Color.cyan;

            Gizmos.DrawCube(pathPoints[i].pointPosition, Vector3.one / 2);
        }
    }
}

[System.Serializable]
public class Points
{
    public float H;
    public int G;
    public float F;
    public Vector3 pointPosition;
    public Vector3 parentPosition;
}