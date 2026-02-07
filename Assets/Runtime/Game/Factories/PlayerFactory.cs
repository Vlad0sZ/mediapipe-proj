using System.Collections.Generic;
using Runtime.CharacterPersonalization;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Runtime.Game.Factories
{
    public class PlayerFactory : MonoBehaviour, IPlayerFactory
    {
        [SerializeField] private Transform spawnParent;
        [SerializeField] private GameObject playerPrefab;

        private Dictionary<string, int> _skinSettings;

        private GameObject _playerInstance;
        private IObjectResolver _objectResolver;

        [Inject]
        public void Construct(IObjectResolver objectResolver) =>
            _objectResolver = objectResolver;


        private void Start()
        {
            if (spawnParent == null)
                spawnParent = transform;
        }


        public void SpawnPlayer(Vector3 position)
        {
            if (_playerInstance == null)
                _playerInstance = _objectResolver.Instantiate(playerPrefab, spawnParent);
            else
                _playerInstance.SetActive(true);


            var customizer = _playerInstance.GetComponentInChildren<CharacterCustomizer>();
            if (customizer)
            {
                if (_skinSettings == null || _skinSettings.Count == 0)
                    customizer.ResetItems();
                else
                    customizer.ApplyCharacter(_skinSettings);
            }

            _playerInstance.transform.localPosition = position;
        }

        public void RemovePlayer()
        {
            if (_playerInstance == null)
                return;

            _playerInstance.SetActive(false);
        }

        public GameObject GetPlayer()
        {
            return _playerInstance;
        }

        public void SetupPlayer(Dictionary<string, int> skinSettings) =>
            _skinSettings = skinSettings;

        private void OnDestroy()
        {
            if (_playerInstance)
                Destroy(_playerInstance);
        }
    }
}