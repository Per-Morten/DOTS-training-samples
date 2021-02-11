using UnityEngine;
using Unity.Mathematics;

public static class DBG
{
    public static void Line(float3 from, float3 too, Color color)
    {
        Debug.DrawLine(from, too, color);
    }

    // TODO: Update to allow me to send in a plane that we draw the rect on
    public static void RectXZ(float2 min, float2 max, float height, Color color)
    {
        Debug.DrawLine(new float3(min.x, height, min.y), new float3(max.x, height, min.y), color);
        Debug.DrawLine(new float3(min.x, height, min.y), new float3(min.x, height, max.y), color);
        Debug.DrawLine(new float3(min.x, height, max.y), new float3(max.x, height, max.y), color);
        Debug.DrawLine(new float3(max.x, height, max.y), new float3(max.x, height, min.y), color);
    }

    public static void Point(float3 point, Color color)
    {
        var onezero = new float2(1.0f, 0.0f);
        Debug.DrawLine(point - onezero.xyy, point + onezero.xyy, color);
        Debug.DrawLine(point - onezero.yxy, point + onezero.yxy, color);
        Debug.DrawLine(point - onezero.yyx, point + onezero.yyx, color);
    }
}

