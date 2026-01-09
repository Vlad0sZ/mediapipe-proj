using System;
using JetBrains.Annotations;
using VContainer;

namespace Runtime.Machine
{
    [UsedImplicitly]
    public sealed class StateFactory : IStateFactory
    {
        private readonly IObjectResolver _objectResolver;

        public StateFactory(IObjectResolver objectResolver) =>
            _objectResolver = objectResolver;

        public IAsyncState CreateState(Type stateType)
        {
            var state = _objectResolver.Resolve(stateType);
            if (state is IAsyncState st)
                return st;

            return default;
        }
    }
}