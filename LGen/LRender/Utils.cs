using UnityEngine;

namespace LGen.LRender 
{ 
    public class Utils
    {
        //public static Vector3 PolarToCartesian(float distance, float yaw, float pitch) 
        //{ 
        //    // distance, yaw, pitch - where yaw and pitch are in radians

        //    float r = distance;
        //    //float theta = yaw;
        //    //float phi = pitch;

        //    //return new Vector3
        //    //(
        //    //    r * Mathf.Cos(phi) * Mathf.Sin(theta),
        //    //    r * Mathf.Sin(phi) * Mathf.Sin(theta),
        //    //    r * Mathf.Cos(theta)
        //    //);

        //    float elevation = pitch;
        //    float heading = yaw;
        //    return new Vector3
        //    (
        //        r * Mathf.Cos(elevation) * Mathf.Sin(heading), 
        //        r * Mathf.Sin(elevation), 
        //        r * Mathf.Cos(elevation) * Mathf.Cos(heading)
        //    );
        //}

        public static Quaternion GetQuaternion(float roll, float pitch, float yaw)
        {
            return Quaternion.Euler(new Vector3(Mathf.Rad2Deg*pitch, 1, 1)) * Quaternion.Euler(new Vector3(1, Mathf.Rad2Deg*yaw, 1)) * Quaternion.Euler(new Vector3(1, 1, Mathf.Rad2Deg*roll));
        }

        public static Vector3 ApplyRollPitchYaw(float roll, float pitch, float yaw, Vector3 vec)
        {
            //return Quaternion.Euler(new Vector3(Mathf.Rad2Deg*pitch, Mathf.Rad2Deg*yaw, Mathf.Rad2Deg*roll)) * vec;
            return GetQuaternion(roll, pitch, yaw) * vec;
        }
    }
}