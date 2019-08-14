using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class CAngleUtil{

    public static float GetAngle(Vector3 vDir)
    {
        Vector3 v = new Vector3(0, 1, 0);

        float cost = Vector3.Dot(vDir, v);

        float fAngle;
        if (vDir.x < 0.0f) fAngle = (float)-Math.Acos(cost);
        else fAngle = (float)Math.Acos(cost);

        fAngle *= 57.29577951f;

        while (fAngle < 0.0f) fAngle += 360.0f;
        while (fAngle > 360.0f) fAngle -= 360.0f;

        return fAngle;
    }

    public static Vector3 GetDirection(Vector3 vSrc, Vector3 vDest)
    {
        Vector3 vDir = vDest - vSrc;

        return vDir.normalized;
    }
}
