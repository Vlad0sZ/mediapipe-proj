using System;
using UnityEngine;

namespace Runtime
{
    public sealed class BoneLookAtTarget : MonoBehaviour
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

            // ------ PERPLEXITY 1.0
            // Vector3 boneEndDirWorld = bone.TransformDirection(localEndDirection.normalized);
            //
            // Vector3 toTarget = target.position - bone.position;
            // if (toTarget.sqrMagnitude < 1e-6f)
            //     return;
            //
            // Vector3 toTargetDir = toTarget.normalized;
            //
            // Quaternion lookRotDelta = Quaternion.FromToRotation(boneEndDirWorld, toTargetDir);
            // Quaternion lookRot = lookRotDelta * bone.rotation;
            //
            // if (followTargetRotation)
            // {
            //     Quaternion targetRot = target.rotation;
            //     lookRot = Quaternion.Slerp(lookRot, targetRot, weight);
            // }
            //
            // float t = rotationSmooth * Time.deltaTime * weight;
            // bone.rotation = Quaternion.Slerp(bone.rotation, lookRot, t);

            // Vector3 boneStart = bone.position;
            // Vector3 toTarget = (target.position - boneStart);
            // if (toTarget.sqrMagnitude < 1e-6f)
            //     return;
            //
            // Vector3 toTargetDir = toTarget.normalized;
            // Vector3 boneEndDirWorld = bone.TransformDirection(localEndDirection.normalized);
            //
            // Quaternion fromTo = Quaternion.FromToRotation(boneEndDirWorld, toTargetDir);
            // Quaternion desiredRotation = fromTo * bone.rotation;
            //
            // // Учитываем weight в повороте
            // float t = rotationSmooth * Time.deltaTime * weight;
            // bone.rotation = Quaternion.Slerp(
            //     bone.rotation,
            //     desiredRotation,
            //     t
            // );

            // ------ PERPLEXITY 2.0
            // 1. Текущее направление конца кости в world space
            // Vector3 boneEndDirWorld = bone.TransformDirection(localEndDirection.normalized);
            //
            // // 2. Направление на target
            // Vector3 toTarget = target.position - bone.position;
            // if (toTarget.sqrMagnitude < 1e-6f)
            //     return;
            //
            // Vector3 toTargetDir = toTarget.normalized;
            //
            // // 3. Поворот, чтобы «конец» кости смотрел на target
            // Quaternion fromTo = Quaternion.FromToRotation(boneEndDirWorld, toTargetDir);
            // Quaternion lookRot = fromTo * bone.rotation;
            //
            // if (followTargetRotation)
            // {
            //     // Можно частично подмешивать ориентацию target, но позже мы отфильтруем твист
            //     lookRot = Quaternion.Slerp(lookRot, target.rotation, weight);
            // }
            //
            // // 4. Убираем твист вокруг продольной оси кости
            //
            // // Базовая ось твиста в world space (ось, вокруг которой НЕ хотим вращаться)
            // Vector3 twistAxisWorld = bone.TransformDirection(localEndDirection.normalized);
            //
            // // Переводим текущий и целевой поворот в относительный вид
            // Quaternion currentRot = bone.rotation;
            //
            // // Разложение: currentRot⁻¹ * lookRot = delta
            // Quaternion delta = Quaternion.Inverse(currentRot) * lookRot;
            //
            // // Из delta убираем вращение вокруг twistAxis
            //
            // // Берём направление какой‑то ортогональной оси (например, локальный up)
            // Vector3 refAxisWorld = bone.TransformDirection(Vector3.up);
            // if (Vector3.Dot(refAxisWorld, twistAxisWorld) > 0.99f)
            //     refAxisWorld = bone.TransformDirection(Vector3.right);
            //
            // // Повернём reference-ось delta-поворотом
            // Vector3 refAfter = delta * refAxisWorld;
            //
            // // Проецируем этот вектор на плоскость, перпендикулярную twistAxis
            // Vector3 refBeforeOnPlane = Vector3.ProjectOnPlane(refAxisWorld, twistAxisWorld).normalized;
            // Vector3 refAfterOnPlane = Vector3.ProjectOnPlane(refAfter, twistAxisWorld).normalized;
            //
            // if (refBeforeOnPlane.sqrMagnitude < 1e-6f || refAfterOnPlane.sqrMagnitude < 1e-6f)
            // {
            //     // если проекция выродилась — просто интерполируем без фильтра
            //     ApplyRotation(currentRot, lookRot);
            //     return;
            // }
            //
            // // Находим «безтвистовый» поворот, который вращает refBeforeOnPlane в refAfterOnPlane
            // Quaternion deltaNoTwist = Quaternion.FromToRotation(refBeforeOnPlane, refAfterOnPlane);
            //
            // // Итоговая целевая ориентация без твиста
            // Quaternion lookRotNoTwist = currentRot * deltaNoTwist;
            //
            // ApplyRotation(currentRot, lookRotNoTwist);


            // --------- GEMINI
            // 1. Получаем направление на цель в мировом пространстве
            // Vector3 toTarget = target.position - bone.position;
            // if (toTarget.sqrMagnitude < 1e-6f)
            //     return;
            //
            // // 2. Нам нужно определить, что для кости является "верхом" (Up), 
            // // чтобы она не крутилась вокруг своей оси.
            // // Если мы хотим запретить вращение по локальной оси Y, 
            // // мы берем текущий локальный Up кости и переводим его в World Space.
            // // Важно: берем Up от родителя или изначальный, чтобы он не "плыл" вместе с костью.
            // Vector3 upConstraint = (bone.parent != null) ? bone.parent.up : Vector3.up;
            //
            // // 3. Создаем вращение. 
            // // forward: направление на цель (toTarget)
            // // upwards: наш ограничитель (upConstraint) - это не даст кости крутиться по Y
            // Quaternion targetRotation = Quaternion.LookRotation(toTarget, upConstraint);
            //
            // // 4. Корректировка localEndDirection.
            // // LookRotation направляет ось Z (forward) на цель. 
            // // Если ваша кость в Unity направлена "концом" по другой оси (например, по Y),
            // // нужно добавить корректирующий поворот.
            // Quaternion correction = Quaternion.Inverse(Quaternion.LookRotation(localEndDirection));
            // targetRotation *= correction;
            //
            // // 5. Плавное применение с учетом веса
            // float t = rotationSmooth * Time.deltaTime * weight;
            // bone.rotation = Quaternion.Slerp(bone.rotation, targetRotation, t);


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