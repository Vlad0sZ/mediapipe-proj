using System;
using Mediapipe.Tasks.Components.Containers;
using Runtime.Types;
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

        public static PlayerBody.VisibleJoint GetWorld(this PlayerBody playerBody, JointType jointType) =>
            playerBody.GetWorld((int) jointType);

        public static PlayerBody.VisibleJoint GetNormalized(this PlayerBody playerBody, JointType jointType) =>
            playerBody.GetNormalized((int) jointType);

        public static float GetWorldCoordinate(this PlayerBody playerBody, JointType jointType,
            Coordinates coordinate) =>
            playerBody.GetWorld(jointType).position[(int) coordinate];

        public static float GetNormalizedCoordinate(this PlayerBody playerBody, JointType jointType,
            Coordinates coordinate) =>
            playerBody.GetNormalized(jointType).position[(int) coordinate];
    }
}