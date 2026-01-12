using UnityEngine;

namespace Runtime.Game
{
    [RequireComponent(typeof(RectTransform))]
    public class ClampPivotByScreenX : MonoBehaviour
    {
        [SerializeField] private RectTransform target;
        [SerializeField] private float tolerance = 0f;

        private void Awake()
        {
            if (target == null)
                target = GetComponent<RectTransform>();
        }

        private void LateUpdate()
        {
            if (target == null)
                return;

            var screenWidth = Screen.width;

            // Получаем границы rect в screen space
            var minX = float.MaxValue;
            var maxX = float.MinValue;

            // Берем четыре угла RectTransform и переводим в экранные координаты
            var worldCorners = new Vector3[4];
            target.GetWorldCorners(worldCorners);

            for (int i = 0; i < 4; i++)
            {
                var screenPoint = worldCorners[i];
                minX = Mathf.Min(minX, screenPoint.x);
                maxX = Mathf.Max(maxX, screenPoint.x);
            }

            var pivot = target.pivot;
            var changed = false;

            // Если вылезли за правый край — уходим pivot на правую сторону
            if (maxX > screenWidth + tolerance && pivot.x < 1f)
            {
                pivot.x = 1f;
                changed = true;
            }
            // Если вылезли за левый край — pivot на левую сторону
            else if (minX < -tolerance && pivot.x != 0f)
            {
                pivot.x = 0f;
                changed = true;
            }

            if (changed)
                SetPivotKeepingPosition(target, pivot);
        }

        private static void SetPivotKeepingPosition(RectTransform rect, Vector2 newPivot)
        {
            if (rect == null)
                return;

            var parent = rect.parent as RectTransform;
            if (parent == null)
            {
                rect.pivot = newPivot;
                return;
            }

            // Сохраняем мировую позицию центра
            Vector3 worldPos = rect.position;

            rect.pivot = newPivot;
            rect.position = worldPos;
        }
    }
}