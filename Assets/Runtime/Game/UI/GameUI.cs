using Runtime.Game.Interfaces;
using Runtime.Game.ScriptableData;
using Runtime.Game.Types;
using UnityEngine;
using VContainer;

namespace Runtime.Game.UI
{
    public class GameUI : AbstractGameScreenUI
    {
        [SerializeField] private GameObject timerUI;
        [SerializeField] private GameObject scoreUI;
        [SerializeField] private GameObject endlessUI;

        private IGameModeSettings _gameModeSettings;

        [Inject]
        public void Construct(IGameModeSettings gameModeSettings)
        {
            _gameModeSettings = gameModeSettings;
        }


        protected override void OnScreenShowing()
        {
            var mode = _gameModeSettings.CurrentMode;
            timerUI.SetActive(mode != GameMode.Endless);
            scoreUI.SetActive(mode != GameMode.Endless);
            endlessUI.SetActive(mode == GameMode.Endless);
        }

        protected override void OnScreenHidden()
        {
            timerUI.SetActive(false);
            scoreUI.SetActive(false);
            endlessUI.SetActive(false);
        }
    }


    /*
     * Если Training -
     * 1. подписываемся на IObjectSpawner - при первом спавне ждем N секунд, потом:
     * - Оставнавливаем спавн
     * - Останавливаем падение
     * - Показываем UI на объекте с подсказкой
     * - После продолжаем спавн
     *
     * 2. То же самое для верных и не верных объектов
     * 3. Ждем N секунд, показываем UI на Score
     * 4. Потом ждем N секунд, показываем UI на Timer
     * 5. Показываем финальный UI.
     *
     * Для Classic все готово
     *
     * Если Endless -
     *
     * 1. Убираем Timer
     * 2. Показываем Score как текст
     * 3. В конце кнопка для показа рекордов и установки рекордов
     */
}