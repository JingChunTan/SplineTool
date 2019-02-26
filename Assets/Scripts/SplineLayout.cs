using UnityEditor;
using UnityEngine;

namespace Splines
{
    [ExecuteInEditMode]
    public class SplineLayout : MonoBehaviour
    {
        public SplineComponent spline;
        public int count = 0;

        void Reset()
        {
            spline = GetComponent<SplineComponent>();
        }

        void OnDrawGizmos()
        {
            for (var i = 0; i < count; i++)
            {
                var n = i * 1f / count;
                var p = spline.GetPoint(n);
                var normal = spline.GetRight(n);
                Gizmos.color = Color.green;
                Gizmos.DrawLine(p, p + normal);
            }
        }

        [DrawGizmo(GizmoType.NonSelected)]
        static void DrawGizmosLoRes(SplineComponent spline, GizmoType gizmoType)
        {
            Gizmos.color = Color.white;
            DrawGizmo(spline, 64);
        }

        [DrawGizmo(GizmoType.Selected)]
        static void DrawGizmosHiRes(SplineComponent spline, GizmoType gizmoType)
        {
            Gizmos.color = Color.white;
            DrawGizmo(spline, 1024);
        }

        static void DrawGizmo(SplineComponent spline, int stepCount)
        {
            if (spline.points.Count > 0)
            {
                var P = 0f;
                var start = spline.GetNonUniformPoint(0);
                var step = 1f / stepCount;
                do
                {
                    P += step;
                    var here = spline.GetNonUniformPoint(P);
                    Gizmos.DrawLine(start, here);
                    start = here;
                } while (P + step <= 1);
            }
        }

    }
}