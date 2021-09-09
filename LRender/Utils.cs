using UnityEngine;

namespace LGen.LRender 
{ 
    public class Utils
    {
        public static Vector3 ApplyRollPitchYaw(float roll, float pitch, float yaw, Vector3 vec)
        {
            return Quaternion.Euler(new Vector3(Mathf.Rad2Deg*pitch, Mathf.Rad2Deg*yaw, Mathf.Rad2Deg*roll)) * vec;
        }
    }
}