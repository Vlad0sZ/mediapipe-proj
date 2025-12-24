using System.Collections.Generic;
using R3;
using Runtime.Game.Interfaces;
using Runtime.Game.ScriptableData;
using UnityEngine;
using UnityEngine.Pool;
using Random = UnityEngine.Random;

namespace Runtime.Game
{
    public sealed class ObjectSpawner : MonoBehaviour, IObjectSpawner
    {
        public abstract record SpawnEvent(GameObject Object)
        {
            public sealed record Created(GameObject Object) : SpawnEvent(Object);

            public sealed record Released(GameObject Object) : SpawnEvent(Object);
        }

        [SerializeField] private GameObject prefab;

        [SerializeField] private Rect spawnRect;

        private readonly Subject<SpawnEvent> _spawnSubject = new Subject<SpawnEvent>();
        private readonly Subject<bool> _spawnProcessSubject = new Subject<bool>();
        private readonly List<GameObject> _activeObjects = new List<GameObject>(128);

        private IObjectPool<GameObject> _objectPool;
        private float _spawnTime;
        private bool _isRunning;
        private GameSettings.SpawnSettings _spawnSettings;

        private bool Running
        {
            get => _isRunning;
            set
            {
                if (_isRunning == value)
                    return;
                
                _isRunning = value;
                _spawnProcessSubject.OnNext(value);
            }
        }

        public Observable<SpawnEvent> OnObjectSpawned => _spawnSubject;
        public Observable<bool> OnSpawnProcess => _spawnProcessSubject;

        public void ReleaseObject(GameObject releaseObject) =>
            _objectPool.Release(releaseObject);

        public void Setup(GameSettings.SpawnSettings payload) =>
            _spawnSettings = payload;

        public void StartSpawn() => 
            Running = true;

        public void StopSpawn()
        {
            Running = false;
            ClearObjects();
        }

        private void Awake()
        {
            _objectPool = new ObjectPool<GameObject>(
                createFunc: CreateItem,
                actionOnGet: OnGet,
                actionOnRelease: OnRelease,
                actionOnDestroy: OnDestroyItem,
                collectionCheck: true,
                defaultCapacity: 10
            );
        }

        private void Update()
        {
            if (!Running)
                return;

            _spawnTime += Time.deltaTime;

            if (_spawnTime < _spawnSettings.spawnDelay)
                return;

            _spawnTime = 0;
            SpawnObject(_spawnSettings.maxObjectPerSpawn);
        }

        private void ClearObjects()
        {
            if (_activeObjects.Count <= 0)
                return;

            for (int i = _activeObjects.Count - 1; i >= 0; i--)
                ReleaseObject(_activeObjects[i]);
            _activeObjects.Clear();
        }

        private void OnDrawGizmos()
        {
            var center = (Vector3) spawnRect.center;
            var position = transform.position + center;
            Gizmos.DrawWireCube(position, spawnRect.size);
        }

        private void SpawnObject(int maxObjects)
        {
            var max = Mathf.Max(1, maxObjects);
            var countToSpawn = Random.Range(1, max);
            for (int i = 1; i <= countToSpawn; i++)
                Spawn();
        }

        private void Spawn()
        {
            var spawnerPos = transform.position;
            var y = spawnRect.center.y;
            var min = spawnRect.min + (Vector2) spawnerPos;
            var max = spawnRect.max + (Vector2) spawnerPos;
            var randomPoint = Vector2.Lerp(min, max, Random.value);
            var position = new Vector3(randomPoint.x, y, spawnerPos.z);

            var go = _objectPool.Get();
            go.transform.position = position;
            var rig = go.GetComponent<Rigidbody>();
            rig.WakeUp();
        }

        private GameObject CreateItem()
        {
            var obj = Instantiate(prefab, transform, false);
            obj.name = $"PooledObject {Random.value}";
            obj.SetActive(false);
            return obj;
        }

        private void OnGet(GameObject obj)
        {
            obj.SetActive(true);
            _activeObjects.Add(obj);
            _spawnSubject.OnNext(new SpawnEvent.Created(obj));
        }

        private void OnRelease(GameObject obj)
        {
            obj.SetActive(false);
            _activeObjects.Remove(obj);
            _spawnSubject.OnNext(new SpawnEvent.Released(obj));
        }

        private static void OnDestroyItem(GameObject obj) => Destroy(obj);
    }
}