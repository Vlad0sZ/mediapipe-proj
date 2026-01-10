using Runtime.Game.Interfaces;
using Runtime.Game.ScriptableData;
using Runtime.Game.Types;
using UnityEngine;

namespace Runtime.Game.UI
{
    public abstract class GameControl
    {
        public abstract void PrepareGame();

        public abstract void OnGameStarted();

        public abstract void OnGamePaused();

        public abstract void OnGameResumed();

        public abstract void OnGameStopped();
    }



    
    public class GameUI : AbstractGameScreenUI
    {
        [SerializeField] private GameObject timerUI;
        [SerializeField] private GameObject scoreUI;
        [SerializeField] private GameObject endlessUI;
        
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