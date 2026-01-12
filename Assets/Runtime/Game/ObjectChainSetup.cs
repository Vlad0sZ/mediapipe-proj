using System;
using System.Collections.Generic;
using R3;
using Runtime.Game.Interfaces;
using Runtime.Game.Spawner;
using UnityEngine;

namespace Runtime.Game
{
    public sealed class ObjectChainSetup : ObjectSpawnerOwner, ISpawnerSetup
    {
        private IDisposable _disposable;
        private readonly List<ISpawnerChain> _spawnerChains = new();

        public override void Configure(IObjectSpawner objectSpawner)
        {
            var createdSub = objectSpawner.OnObjectSpawned
                .Where(x => x is ObjectSpawner.SpawnEvent.Created)
                .Select(x => x.Object)
                .Subscribe(Created);

            var releasedSub = objectSpawner.OnObjectSpawned
                .Where(x => x is ObjectSpawner.SpawnEvent.Released)
                .Select(x => x.Object)
                .Subscribe(Released);

            _disposable = Disposable.Combine(createdSub, releasedSub);
        }

        public override void Deconstruct() => 
            _disposable?.Dispose();

        public void AddSpawnerChain(ISpawnerChain chain)
        {
            if (!_spawnerChains.Contains(chain))
                _spawnerChains.Add(chain);
        }

        public void RemoveSpawnerChain(ISpawnerChain chain)
        {
            if (chain == null || !_spawnerChains.Contains(chain))
                return;

            _spawnerChains.Remove(chain);
            (chain as IDisposable)?.Dispose();
        }

        private void Created(GameObject obj)
        {
            foreach (var spawnerChain in _spawnerChains)
                spawnerChain.OnSpawned(obj);
        }

        private void Released(GameObject obj)
        {
            foreach (var spawnerChain in _spawnerChains)
                spawnerChain.OnReleased(obj);
        }
    }
}