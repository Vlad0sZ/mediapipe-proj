using SensorPack.KinectCore.Runtime;
using UnityEngine;

namespace Runtime
{
    public sealed class JointRotationControl : JointControl
    {
        [Header("IK Target")] public Transform target; // IK Target, который будет поворачиваться

        [Header("Joints")] public KinectInterop.JointType wristJoint = KinectInterop.JointType.WristLeft;
        public KinectInterop.JointType thumbJoint = KinectInterop.JointType.ThumbLeft;
        public KinectInterop.JointType handTipJoint = KinectInterop.JointType.HandTipLeft;

        [Header("Settings")] public float smoothFactor = 5f;

        public override void UpdateBones()
        {
            if (CalculateHandOrientation(out Quaternion desiredRotation))
            {
                target.rotation = Quaternion.Slerp(
                    target.rotation,
                    desiredRotation,
                    smoothFactor * Time.deltaTime);
            }
        }


        private bool CalculateHandOrientation(out Quaternion rotation)
        {
            rotation = Quaternion.identity;

            // Получаем позиции трёх точек руки
            if (!IsJointTracked(wristJoint, out Vector3 wristPos) ||
                !IsJointTracked(thumbJoint, out Vector3 thumbPos) ||
                !IsJointTracked(handTipJoint, out Vector3 handTipPos))
                return false;

            // Z-направление: от запястья к большому пальцу (направление ладони вперед)
            Vector3 zDirection = (thumbPos - wristPos).normalized;

            // Y-направление: от запястья к кончику указательного пальца (направление "вверх ладони")
            Vector3 yDirection = (handTipPos - wristPos).normalized;

            // X-направление: перпендикуляр к Y и Z (боковое направление ладони)
            Vector3 xDirection = Vector3.Cross(yDirection, zDirection).normalized;
            yDirection = Vector3.Cross(zDirection, xDirection); // ортогонализируем

            // Создаем матрицу ориентации: X, Y, Z → локальные оси target
            var matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, Vector3.one);
            matrix.SetTRS(Vector3.zero, Quaternion.identity, Vector3.one);
            matrix.SetColumn(0, xDirection); // локальный X
            matrix.SetColumn(1, yDirection); // локальный Y  
            matrix.SetColumn(2, zDirection); // локальный Z

            rotation = matrix.rotation;
            return true;
        }

        private bool IsJointTracked(KinectInterop.JointType joint, out Vector3 pos)
        {
            if (!this.TryGetJointPositionIfTracked(joint, false, out pos))
                return false;

            pos.y = 1f - pos.y;
            return true;
        }
    }
}