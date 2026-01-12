using TMPro;
using UnityEngine;

namespace Runtime.Game.UI
{
    public class HintText : MonoBehaviour
    {
        [SerializeField] private TMP_Text textComponent;

        public string Text
        {
            get => textComponent.text;
            set => textComponent.text = value;
        }
    }
}