using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class CMapManager
{
    Tilemap mTilemap = null;
    float mMinX;
    float mMaxX;
    float mMinY;
    float mMaxY;

    public void Init(Tilemap map)
    {
        mTilemap = map;
        BoundsInt bound = map.cellBounds;

        int minX = bound.xMin;
        int maxX = bound.xMax;

        int minY = bound.yMin;
        int maxY = bound.yMax;

        Debug.Log(string.Format("{0}-{1}, {2}-{3}", minX, maxX, minY, maxY));

        Vector3Int min = bound.min;
        Vector3Int max = bound.max;

        min.x += 1;
        min.y += 1;
        max.x -= 1;
        max.y -= 1;

        Vector3 worldMin = mTilemap.CellToWorld(min);
        Vector3 worldMax = mTilemap.CellToWorld(max);

        Debug.Log(string.Format("min : {0}", worldMin));
        Debug.Log(string.Format("max : {0}", worldMax));

        mMinX = worldMin.x;
        mMaxX = worldMax.x;
        mMinY = worldMin.y;
        mMaxY = worldMax.y;
    }

    public bool CheckInside(Vector3 pos)
    {
        if (pos.x < mMinX || pos.x > mMaxX ||
            pos.y < mMinY || pos.y > mMaxY)
            return false;

        return true;
    }

    public void ValidatePos(ref Vector3 pos)
    {
        if (pos.x < mMinX)
            pos.x = mMinX;
        else if (pos.x > mMaxX)
            pos.x = mMaxX;

        if (pos.y < mMinY)
            pos.y = mMinY;
        else if (pos.y > mMaxY)
            pos.y = mMaxY;
    }
}
