using System.Collections.Generic;
using System.Linq;
using System.Text;
using Runtime.Game.Interfaces;
using Runtime.Game.ScriptableData;
using UnityEngine;
using VContainer;

namespace Runtime.Game.UI
{
    public abstract class TaskUI : AbstractGameScreenUI
    {
        [Inject] protected IFoodGroupProvider FoodGroupProvider { get; set; }

        protected string GenerateCurrentTask()
        {
            var foodGroup = FoodGroupProvider?.GetCurrentFoodGroup();
            if (foodGroup == null)
                return string.Empty;

            return GetTaskString(foodGroup.label, foodGroup.Rights);
        }

        private static string GetTaskString(string foodGroupLabel, IReadOnlyList<FoodWithIcon> rights)
        {
            if (rights.Any(x => x.Icon == null))
                return string.Empty;

            var sb = new StringBuilder();
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