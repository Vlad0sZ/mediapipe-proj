using Runtime.Game.Interfaces;
using TMPro;
using UnityEngine;

namespace Runtime.Game.UI
{
    public class RecordRow : MonoBehaviour, ISetupPayload<UserRecord>
    {
        [SerializeField] private TMP_Text placeText;
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private TMP_Text scoreText;

        public void Setup(UserRecord payload)
        {
            placeText.text = $"# {payload.place}.";
            nameText.text = payload.userName;
            scoreText.text = payload.userScore > 0 ? payload.userScore.ToString() : string.Empty;
        }
    }
}