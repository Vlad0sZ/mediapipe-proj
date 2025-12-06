using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Runtime.Game.Factories
{
    public class PlayerFactory : MonoBehaviour, IPlayerFactory
    {
        [SerializeField] private Transform spawnParent;
        [SerializeField] private GameObject playerPrefab;

        private GameObject _playerInstance;
        private IObjectResolver _objectResolver;

        [Inject]
        public void Construct(IObjectResolver objectResolver) => _objectResolver = objectResolver;


        private void Start()
        {
            if (spawnParent == null)
                spawnParent = transform;
        }


        public void SpawnPlayer(Vector3 position)
        {
            if (_playerInstance == null)
            {
                _playerInstance = _objectResolver.Instantiate(playerPrefab, spawnParent);
            }
            else
            {
                _playerInstance.SetActive(true);
            }

            _playerInstance.transform.localPosition = position;
        }

        public void RemovePlayer()
        {
            if (_playerInstance == null)
                return;

            _playerInstance.SetActive(false);
        }

        private void OnDestroy()
        {
            if (_playerInstance)
                Destroy(_playerInstance);
        }
    }
}