using System;
using UnityEngine;

namespace Runtime
{
    public sealed class BoneLookAtTarget : MonoBehaviour, IWeightSetter
    {
        [Header("References")] public Transform bone; // кость персонажа
        public Transform target; // цель

        [Header("Bone Settings")] public Vector3 localEndDirection = Vector3.up;

        [Header("Smoothing")] public float rotationSmooth = 10f;

        [Min(0.001f)] [Header("Gizmos")] public float size = 0.1f;
        public Color color = Color.yellow;
        public bool followTargetRotation = false;

        [Range(0f, 1f)] public float weight = 1f; // 0 = выкл, 1 = полное влияние

        private Quaternion _initialBoneRotation;

        public void SetWeight(float w) =>
            this.weight = w;

        private void Reset()
        {
            bone = transform;
        }

        private void Awake()
        {
            if (bone == null)
                bone = transform;

            _initialBoneRotation = bone.rotation;
        }

        private void LateUpdate()
        {
            if (bone == null || target == null || weight <= 0f)
                return;

            // 1. Получаем направление на цель
            Vector3 toTarget = target.position - bone.position;
            if (toTarget.sqrMagnitude < 1e-6f)
                return;

            // 2. Базовая ориентация: смотрим на цель.
            // Используем target.up в качестве вектора "вверх", чтобы кость 
            // ориентировалась в пространстве согласно наклону цели.
            Quaternion lookRot = Quaternion.LookRotation(toTarget, target.up);

            // 3. Корректировка направления самой кости
            // (Чтобы кость смотрела на цель именно той стороной, которая указана в localEndDirection)
            Quaternion correction = Quaternion.Inverse(Quaternion.LookRotation(localEndDirection));
            Quaternion finalTargetRotation = lookRot * correction;

            // 4. Добавляем вращение по оси Y (Twist) от Target
            if (followTargetRotation)
            {
                // Вычисляем, насколько target повернут вокруг своей оси Y относительно базового LookRotation
                // И применяем это вращение к нашей финальной ориентации
                float twistAngle = Vector3.SignedAngle(target.up, lookRot * Vector3.up, toTarget);
                // Однако, проще всего позволить Quaternion.LookRotation сделать работу через target.up,
                // а если нужно полное соответствие, используем:
                finalTargetRotation = Quaternion.Slerp(finalTargetRotation, target.rotation * correction, weight);
            }

            // 5. Интерполяция
            float t = rotationSmooth * Time.deltaTime; // weight уже учтен в Slerp выше или в финальном шаге
            bone.rotation = Quaternion.Slerp(bone.rotation, finalTargetRotation, t * weight);
        }

        private void OnDrawGizmos()
        {
            if (target == null) return;

            // Сохраняем старую матрицу Gizmos, чтобы не испортить отрисовку других элементов
            Matrix4x4 oldMatrix = Gizmos.matrix;

            // Устанавливаем матрицу Gizmos равной матрице цели (позиция + поворот + масштаб)
            Gizmos.matrix = target.localToWorldMatrix;

            // 1. Рисуем куб на месте target
            // Так как мы изменили матрицу, куб (0,0,0) будет нарисован прямо в центре target
            Gizmos.color = color;
            Gizmos.DrawWireCube(Vector3.zero, Vector3.one * size);

            // Полупрозрачный центр куба
            Gizmos.color = new Color(color.r, color.g, color.b, 0.2f);
            Gizmos.DrawCube(Vector3.zero, Vector3.one * size);

            // 2. Рисуем стрелки (оси)
            // В локальном пространстве target: X (right), Y (up), Z (forward)
            float lineLength = size * 2f;

            // Ось X (Красная)
            Gizmos.color = Color.red;
            Gizmos.DrawRay(Vector3.zero, Vector3.right * lineLength);
            DrawArrowHead(Vector3.right * lineLength, Vector3.right);

            // Ось Y (Зеленая)
            Gizmos.color = Color.green;
            Gizmos.DrawRay(Vector3.zero, Vector3.up * lineLength);
            DrawArrowHead(Vector3.up * lineLength, Vector3.up);

            // Ось Z (Синяя)
            Gizmos.color = Color.blue;
            Gizmos.DrawRay(Vector3.zero, Vector3.forward * lineLength);
            DrawArrowHead(Vector3.forward * lineLength, Vector3.forward);

            // Возвращаем матрицу в исходное состояние
            Gizmos.matrix = oldMatrix;
        }

// Вспомогательный метод для рисования "наконечника" стрелки маленьким кубиком
        private void DrawArrowHead(Vector3 pos, Vector3 direction)
        {
            Gizmos.DrawCube(pos, Vector3.one * (size * 0.3f));
        }
    }
}