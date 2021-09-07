using UnityEngine;

namespace LGen.LRender 
{ 
    public class Utils
    {
        public static Vector3 PolarToCartesian(float distance, float yaw, float pitch) 
        { 
            // distance, yaw, pitch - where yaw and pitch are in radians

            float r = distance;
            float theta = yaw;
            float phi = pitch;

            return new Vector3
            (
                r * Mathf.Cos(phi) * Mathf.Sin(theta),
                r * Mathf.Sin(phi) * Mathf.Sin(theta),
                r * Mathf.Cos(theta)
            );
        }
    }
}