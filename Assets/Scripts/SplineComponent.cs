﻿using UnityEngine;
using System.Collections.Generic;

namespace Splines
{
    public enum SplineType
    {
        Linear,
        Hermite
    }

    [ExecuteInEditMode]
    public class SplineComponent : MonoBehaviour, ISpline
    {
        public bool closed = false;

        public SplineType splineType;

        public List<Vector3> points = new List<Vector3>();
        public float? length;

        SplineIndex _uniformindex;
        SplineIndex Index
        {
            get
            {
                if (_uniformindex == null) _uniformindex = new SplineIndex(this);
                return _uniformindex;
            }
        }

        public int ControlPointCount
        {
            get
            {
                return points.Count;  
            }
        }

        public Vector3 GetNonUniformPoint(float t)
        {
            switch (points.Count)
            {
                case 0:
                    return Vector3.zero;
                case 1:
                    return transform.TransformPoint(points[0]);
                case 2:
                    return transform.TransformPoint(Vector3.Lerp(points[0], points[1], t));
                case 3:
                    return transform.TransformPoint(points[1]);
                default:
                    switch (splineType)
                    {
                        case SplineType.Hermite:
                            return Hermite(t);
                        case SplineType.Linear:
                            return Linear(t);
                    }
                    throw new System.NotImplementedException();
            }
        }

        Vector3 Linear(float t)
        {
            var count = points.Count - (closed ? 0 : 1);
            var i = Mathf.Min(Mathf.FloorToInt(t * (float)count), count - 1);
            var u = t * count - i;
            var a = GetPointByIndex(i);
            var b = GetPointByIndex(i + 1);
            return transform.TransformPoint(a + (b - a) * u);
        }

        private Vector3 Hermite(float t)
        {
            var count = points.Count - (closed ? 0 : 3);
            var i = Mathf.Min(Mathf.FloorToInt(t * (float)count), count - 1);
            var u = t * (float)count - (float)i;
            var a = GetPointByIndex(i);
            var b = GetPointByIndex(i + 1);
            var c = GetPointByIndex(i + 2);
            var d = GetPointByIndex(i + 3);
            return transform.TransformPoint(Interpolate(a, b, c, d, u));
        }

        internal Vector3 Interpolate(Vector3 a, Vector3 b, Vector3 c, Vector3 d, float u)
        {
            return (
                0.5f *
                (
                    (-a + 3f * b - 3f * c + d) *
                    (u * u * u) +
                    (2f * a - 5f * b + 4f * c - d) *
                    (u * u) +
                    (-a + c) *
                    u + 2f * b
                )
            );
        }

        Vector3 GetPointByIndex(int i)
        {
            if (i < 0) i += points.Count;
            return points[i % points.Count];
        }

        public Vector3 GetRight(float t)
        {
            var A = GetPoint(t - 0.001f);
            var B = GetPoint(t + 0.001f);
            var delta = (B - A);
            return new Vector3(-delta.z, 0, delta.x).normalized;
        }

        public Vector3 GetForward(float t)
        {
            var A = GetPoint(t - 0.001f);
            var B = GetPoint(t + 0.001f);
            return (B - A).normalized;
        }

        public Vector3 GetUp(float t)
        {
            var A = GetPoint(t - 0.001f);
            var B = GetPoint(t + 0.001f);
            var delta = (B - A).normalized;
            return Vector3.Cross(delta, GetRight(t));
        }

        public Vector3 GetPoint(float t)
        {
            return Index.GetPoint(t);
        } 

        public Vector3 GetLeft(float t)
        {
            return -GetRight(t);
        }

        public Vector3 GetDown(float t)
        {
            return -GetUp(t);
        } 

        public Vector3 GetBackward(float t)
        {
            return -GetForward(t);
        }

        public float GetLength(float step = 0.001f)
        {
            var D = 0f;
            var A = GetNonUniformPoint(0);
            for (var t = 0f; t < 1f; t += step)
            {
                var B = GetNonUniformPoint(t);
                var delta = (B - A);
                D += delta.magnitude;
                A = B;
            }
            return D;
        }

        public Vector3 GetDistance(float distance)
        {
            if (length == null) { length = GetLength(); }
            return _uniformindex.GetPoint(distance / length.Value);
        }

        public Vector3 FindClosest(Vector3 worldPoint)
        {
            var smallestDelta = float.MaxValue;
            var step = 1f / 1024;
            var closestPoint = Vector3.zero;
            for (var i = 0; i <= 1024; i++)
            {
                var p = GetPoint(i * step);
                var delta = (worldPoint - p).sqrMagnitude;
                if (delta < smallestDelta)
                {
                    closestPoint = p;
                    smallestDelta = delta;
                }
            }
            return closestPoint;
        }

        public void InsertControlPoint(int index, Vector3 position)
        {
            ResetIndex();
            if (index >= points.Count)
                points.Add(position);
            else
                points.Insert(index, position);
        }

        public void RemoveControlPoint(int index)
        {
            ResetIndex();
            points.RemoveAt(index);
        }

        public Vector3 GetControlPoint(int index)
        {
            return points[index];
        }

        public void SetControlPoint(int index, Vector3 position)
        {
            ResetIndex();
            points[index] = position;
        }

        public void ResetIndex()
        {
            _uniformindex = null;
            //length = null;
        }

        void Reset()
        {
            points = new List<Vector3>() {
                Vector3.forward * 3,
                Vector3.forward * 6,
                Vector3.forward * 9,
                Vector3.forward * 12
            };
        }

        void OnValidate()
        {
            if (_uniformindex != null) _uniformindex.ReIndex();
        }

        Vector3 ISpline.GetNonUniformPoint(float t)
        {
            throw new System.NotImplementedException();
        }

        Vector3 ISpline.GetPoint(float t)
        {
            throw new System.NotImplementedException();
        }

        Vector3 ISpline.GetLeft(float t)
        {
            throw new System.NotImplementedException();
        }

        Vector3 ISpline.GetRight(float t)
        {
            throw new System.NotImplementedException();
        }

        Vector3 ISpline.GetUp(float t)
        {
            throw new System.NotImplementedException();
        }

        Vector3 ISpline.GetDown(float t)
        {
            throw new System.NotImplementedException();
        }

        Vector3 ISpline.GetForward(float t)
        {
            throw new System.NotImplementedException();
        }

        Vector3 ISpline.GetBackward(float t)
        {
            throw new System.NotImplementedException();
        }

        float ISpline.GetLength(float stepSize)
        {
            throw new System.NotImplementedException();
        }

        Vector3 ISpline.GetControlPoint(int index)
        {
            throw new System.NotImplementedException();
        }

        void ISpline.SetControlPoint(int index, Vector3 position)
        {
            throw new System.NotImplementedException();
        }

        void ISpline.InsertControlPoint(int index, Vector3 position)
        {
            throw new System.NotImplementedException();
        }

        void ISpline.RemoveControlPoint(int index)
        {
            throw new System.NotImplementedException();
        }

        Vector3 ISpline.GetDistance(float distance)
        {
            throw new System.NotImplementedException();
        }

        Vector3 ISpline.FindClosest(Vector3 worldPoint)
        {
            throw new System.NotImplementedException();
        }
    }
}