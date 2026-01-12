using System;
using System.Text;
using Runtime.Game.Interfaces;
using Runtime.Game.Publishers;
using Runtime.Game.Types;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace Runtime.Game.UI
{
    public class EndScreenUI : AbstractGameScreenUI
    {
        [SerializeField] private TMP_Text textComponent;
        [SerializeField] private Button recordButton;
        
        private IScorePublisher _scorePublisher;
        private IGameModeSettings _gameModeSettings;
        private IRecordsStorage _recordsStorage;

        [Inject]
        public void Construct(IScorePublisher scorePublisher, IGameModeSettings gameModeSettings,
            IRecordsStorage recordsStorage)
        {
            _recordsStorage = recordsStorage;
            _scorePublisher = scorePublisher;
            _gameModeSettings = gameModeSettings;
        }

        protected override void OnScreenShowing() =>
            UpdateText();

        private void UpdateText()
        {
            var mode = _gameModeSettings.CurrentMode;
            recordButton.gameObject.SetActive(mode == GameMode.Endless);
            textComponent.text = mode switch
            {
                GameMode.Training => GetTrainingText(),
                GameMode.Classic => GetClassicText(),
                GameMode.Endless => GetEndlessText(),
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        private string GetEndlessText()
        {
            var sb = new StringBuilder();
            var scoreModel = _scorePublisher.Score;
            var player = _gameModeSettings.GetPlayerName();
            var record = _recordsStorage.AddRecord(player, scoreModel.PositiveScore);

            sb.Append(player)
                .AppendLine(", готовка закончена!");

            sb.Append("Ты набрал: ")
                .Append(scoreModel.PositiveScore)
                .Append(" очков.");

            if (record.place > 0)
            {
                sb.Append("Поздравляем! Ты обновил рекорды, теперь ты на ");
                sb.Append(record.place)
                    .AppendLine(" месте!");
            }
            else
            {
                sb.Append("Но в рекорды ты не попал. Попробуешь еще раз?");
            }

            return sb.ToString();
        }


        private string GetClassicText()
        {
            var sb = new StringBuilder();
            var levelTime = _gameModeSettings.GetLevelTime();
            var scoreModel = _scorePublisher.Score;

            sb.Append("За ");
            if (levelTime.Minutes > 0)
                sb.Append(levelTime.Minutes)
                    .Append(" мин ");

            if (levelTime.Seconds > 0)
                sb.Append(levelTime.Seconds)
                    .Append(" сек ");

            if (scoreModel.Progress() > 0)
            {
                sb.Append("тебе удалось приготовить ужин на ");
                sb.AppendFormat("{0:0}", scoreModel.Progress() * 100)
                    .AppendLine("%!");
            }
            else
            {
                sb.Append("ты постарался, но ужин не вышел!")
                    .Append("Попробуешь еще раз?");
            }

            return sb.ToString();
        }


        private string GetTrainingText()
        {
            var sb = new StringBuilder();
            sb.AppendLine("Отлично! Теперь ты можешь начать игру.");
            sb.Append(
                "Чтобы вернуться в меню, нажми на кнопку \"В меню\", а если захочешь выйти из этой игры нажми на кнопку \"Выход\"");


            return sb.ToString();
        }
    }
}