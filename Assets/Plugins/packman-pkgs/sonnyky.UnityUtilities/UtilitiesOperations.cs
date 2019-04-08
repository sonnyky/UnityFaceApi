using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace TinkerExtensions
{
    public static partial class Utilities
    {
        public static Vector2 xz(this Vector3 vv)
        {
            return new Vector2(vv.x, vv.z);
        }

        public static float FlatDistance(this Vector3 from, Vector3 unto)
        {
            Vector2 a = from.xz();
            Vector2 b = unto.xz();
            return Vector2.Distance(a, b);
        }
        public static bool HasParameterOfType(this Animator self, string name, AnimatorControllerParameterType type)
        {
            var parameters = self.parameters;
            foreach (var currParam in parameters)
            {
                if (currParam.type == type && currParam.name == name)
                {
                    return true;
                }
            }
            return false;
        }

        public static Vector3 ApplyHomography(Matrix4x4 homography, Vector3 point)
        {
            return homography.MultiplyPoint3x4(point);
        }

    }
}