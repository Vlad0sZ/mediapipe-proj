using System;
using System.Text;
using Runtime.Game.Interfaces;
using Runtime.Game.Types;
using TMPro;
using UnityEngine;

namespace Runtime.Game.UI
{
    public class PrepareTaskUI : TaskUI, IGameModePayload
    {
        [SerializeField] private TMP_Text taskTextComponent;

        private GameMode _currentMode;

        public void Setup(IGameModeSettings payload) =>
            _currentMode = payload.CurrentMode;

        protected override void OnScreenShowing()
        {
            var food = this.FoodGroupProvider.GetCurrentFoodGroup();
            taskTextComponent.text = GameModeToTask(food.label);
        }

        private string GameModeToTask(string label)
        {
            return _currentMode switch
            {
                GameMode.Training => GenerateTrainingTask(),
                GameMode.Classic => GenerateTask(label, GenerateCurrentTask()),
                GameMode.Endless => GenerateEndlessTask(GenerateCurrentTask()),
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        private static string GenerateTrainingTask()
        {
            var sb = new StringBuilder();
            sb.Append("<b> Это обучающий уровень! </b>")
                .AppendLine()
                .Append("Следуй указаниям во время игры");

            return sb.ToString();
        }


        private static string GenerateEndlessTask(string task)
        {
            var sb = new StringBuilder();
            sb.Append("<b> Готовка на выживание! </b>")
                .AppendLine()
                .AppendLine(
                    "В этом режиме можно ставить рекорды! Время неограничено, но игра закончена, если пропустишь слишком много ингридиентов!");

            sb.Append(task);

            return sb.ToString();
        }

        private static string GenerateTask(string foodGroupLabel, string task)
        {
            var sb = new StringBuilder();
            sb.Append("<b> Сегодня на ужин - ")
                .Append(foodGroupLabel)
                .AppendLine("</b>");

            sb.Append("За отведённое время собери хороший ужин!");
            sb.Append(task);

            return sb.ToString();
        }
    }
}