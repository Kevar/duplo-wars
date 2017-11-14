using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public static class Utils
{
    public static void AddRange<T>(this List<T> list, params T[] items)
    {
        list.AddRange(new List<T>(items));
    }

	public static void CartesianToPolar(Vector3 cartesian, out float rotationRadian, out float elevationRadian, out float radius)
    {
        float xzLen = new Vector2(cartesian.x, cartesian.z).magnitude;

        rotationRadian = Mathf.Atan2(cartesian.x, cartesian.z);
        elevationRadian = Mathf.Atan2(-cartesian.y, xzLen);
        radius = cartesian.magnitude;
    }

    public static Vector3 PolarToCartesian(float rotationRadian, float elevationRadian, float radius)
    {
        return Quaternion.Euler(elevationRadian * Mathf.Rad2Deg, rotationRadian * Mathf.Rad2Deg, 0) * new Vector3(0, 0, 1) * radius;
    }
}
