using System;
using Mediapipe.Tasks.Components.Containers;
using Runtime.Types;
using SensorPack.KinectCore.Runtime;
using UnityEngine;

namespace Runtime
{
    public static class Extensions
    {
        public static Vector3 ToVector(this NormalizedLandmark landmark)
        {
            return new Vector3(landmark.x, landmark.y, landmark.z);
        }

        public static Vector3 ToVector(this Landmark landmark)
        {
            return new Vector3(landmark.x, landmark.y, landmark.z);
        }

        public static Vector3 Inverse(this Vector3 v)
        {
            return new Vector3(v.x, 1f - v.y, v.z);
        }

        public static Vector3 ToWorld(this Vector2 normalizedPosition, Camera camera, float zOffset = 0f)
        {
            return camera.ViewportToWorldPoint(new Vector3(normalizedPosition.x, normalizedPosition.y,
                camera.nearClipPlane + zOffset));
        }

        public static Vector3 ToWorld(this Vector3 normalizedPosition, Camera camera, float zOffset = 0f)
        {
            return camera.ViewportToWorldPoint(new Vector3(normalizedPosition.x, normalizedPosition.y,
                camera.nearClipPlane + zOffset));
        }

        public static float Max(this float a, float b) =>
            Mathf.Max(a, b);

        public static float AsMinMaxNext(this Vector2 value)
        {
            var min = Mathf.Min(value.x, value.y);
            var max = Mathf.Min(value.x, value.y);
            return UnityEngine.Random.Range(min, max);
        }


        public static float GetNormalizedCoordinate(this KinectInterop.BodyData bodyData, KinectInterop.JointType jointType,
            Coordinates coordinate) =>
            bodyData.joint[(int) jointType].kinectPos[(int) coordinate];
    }
}