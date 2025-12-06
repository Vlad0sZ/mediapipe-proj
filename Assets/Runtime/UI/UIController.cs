using System.Collections.Generic;
using System.Linq;
using Runtime.UI.Interfaces;
using UnityEngine;

namespace Runtime.UI
{
    public class UIController : MonoBehaviour, ICanvas
    {
        [SerializeField] private AbstractUIScreen[] screens;

        private Dictionary<string, AbstractUIScreen> _screen;

        private void Awake() =>
            _screen = screens.ToDictionary(x => x.ScreenName);

        public IScreen GetScreen(string screenKey)
        {
            if (_screen.TryGetValue(screenKey, out var screen) && screen != null)
                return screen;

            return null;
        }
    }
}