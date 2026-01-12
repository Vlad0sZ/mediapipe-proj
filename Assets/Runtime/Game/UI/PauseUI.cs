using System.Text;
using TMPro;
using UnityEngine;

namespace Runtime.Game.UI
{
    public sealed class PauseUI : TaskUI
    {
        [SerializeField] private TMP_Text textComponent;

        protected override void OnScreenShowing()
        {
            base.OnScreenShowing();
            textComponent.text = GenerateTask();
        }

        private string GenerateTask()
        {
            var task = GenerateCurrentTask();
            var sb = new StringBuilder();
            sb.AppendLine("Игра на паузе, чтобы продолжить - поднимите руки вверх!");
            sb.AppendLine("Задание на сегодня:").Append(task);

            return sb.ToString();
        }
    }
}