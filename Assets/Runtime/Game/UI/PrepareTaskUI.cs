using System.Collections.Generic;
using System.Linq;
using System.Text;
using Runtime.Game.Interfaces;
using Runtime.Game.ScriptableData;
using TMPro;
using UnityEngine;

namespace Runtime.Game.UI
{
    public class PrepareTaskUI : MonoBehaviour, ISetupPayload<FoodObjects.FoodGroup>
    {
        [SerializeField] private TMP_Text taskTextComponent;


        public void Setup(FoodObjects.FoodGroup payload)
        {
            var label = payload.label;
            var rights = payload.Rights;
            var taskText = GenerateTask(label, rights);
            taskTextComponent.text = taskText;
        }

        private static string GenerateTask(string foodGroupLabel, IEnumerable<FoodWithIcon> rights)
        {
            var sb = new StringBuilder();
            sb.Append("<b> Сделаем ")
                .Append(foodGroupLabel)
                .AppendLine("?</b>");

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