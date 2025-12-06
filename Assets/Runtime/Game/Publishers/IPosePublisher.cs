using System;
using System.Collections.Generic;
using R3;
using VContainer.Unity;

namespace Runtime.Game.Publishers
{
    public interface IPosePublisher : ITickable, IDisposable
    {
        public Observable<List<PlayerBody>> Bodies { get; }
    }
}