using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Runtime.Game.Interfaces;
using Runtime.Game.ScriptableData;
using Runtime.Game.Types;
using TMPro;
using UnityEngine;

namespace Runtime.Game.UI
{
    public class PrepareTaskUI : MonoBehaviour, IGameModePayload, IFoodPayload
    {
        [SerializeField] private TMP_Text taskTextComponent;

        private GameMode _currentMode;

        public void Setup(IGameModeSettings payload) =>
            _currentMode = payload.CurrentMode;

        public void Setup(FoodObjects.FoodGroup payload)
        {
            var label = payload.label;
            var rights = payload.Rights;
            var taskText = GameModeToTask(label, rights);
            taskTextComponent.text = taskText;
        }

        private string GameModeToTask(string label, IEnumerable<FoodWithIcon> rights)
        {
            switch (_currentMode)
            {
                case GameMode.Training:
                    return GenerateTrainingTask();
                case GameMode.Classic:
                    return GenerateTask(label, rights);
                case GameMode.Endless:
                    return GenerateEndlessTask(label, rights);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static string GenerateEndlessTask(string label, IEnumerable<FoodWithIcon> rights)
        {
            var sb = new StringBuilder();
            sb.Append("<b> Готовка на выживание! </b>")
                .AppendLine()
                .AppendLine(
                    "В этом режиме можно ставить рекорды! Время неограничено, но игра закончена, если пропустишь слишком много ингридиентов!");
            sb.AppendLine()
                .Append("Лови только те предметы, которые входят в состав ")
                .Append(label)
                .AppendLine(":");

            var joinRights = string.Join(" ", rights.Select(r => $"<sprite name=\"{r.Icon.name}\">"));
            sb.Append("<size=200%>")
                .Append(joinRights)
                .Append("</size>");

            return sb.ToString();
        }

        private static string GenerateTrainingTask()
        {
            var sb = new StringBuilder();
            sb.Append("<b> Это обучающий уровень! </b>")
                .AppendLine()
                .Append("Следуй указаниям во время игры");

            return sb.ToString();
        }


        private static string GenerateTask(string foodGroupLabel, IEnumerable<FoodWithIcon> rights)
        {
            var sb = new StringBuilder();
            sb.Append("<b> Сегодня на ужин - ")
                .Append(foodGroupLabel)
                .AppendLine("</b>");

            sb.Append("За отведённое время собери хороший ужин!");

            sb.AppendLine()
                .Append("Лови только те предметы, которые входят в состав ")
                .Append(foodGroupLabel)
                .AppendLine(":");

            var joinRights = string.Join(" ", rights.Select(r => $"<sprite name=\"{r.Icon.name}\">"));
            sb.Append("<size=200%>")
                .Append(joinRights)
                .Append("</size>");

            return sb.ToString();
        }
    }
}