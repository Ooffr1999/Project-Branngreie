using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MapGridGenerator
{
    public static List<Vector3> GenerateMapGrid(int mapWidth, int mapDepth, Vector3 startPosition, float mapSizeOffset)
    {
        List<Vector3> mapGridAreas = new List<Vector3>();

        for (int z = 0; z < mapDepth; z++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                Vector3 area = new Vector3();

                area.x = startPosition.x + x;
                area.z = startPosition.z + z;

                area.y = startPosition.y;

                mapGridAreas.Add(area * mapSizeOffset);
            }
        }

        return mapGridAreas;
    }

    public static Vector3 getGridSquareFromPosition(List<Vector3> grid, Vector3 position)
    {
        Vector3 closestPoint = Vector3.zero;

        for (int i = 0; i < grid.Count; i++)
        {
            if (Vector3.Distance(grid[i], position) < Vector3.Distance(closestPoint, position))
                closestPoint = grid[i];
        }

        return closestPoint;
    }
}