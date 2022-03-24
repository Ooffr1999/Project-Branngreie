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
}