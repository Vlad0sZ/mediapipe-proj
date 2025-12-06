using VContainer;

namespace Runtime.Machine
{
    public class StateFactory : IStateFactory
    {
        private readonly IObjectResolver _objectResolver;

        public StateFactory(IObjectResolver objectResolver) => 
            _objectResolver = objectResolver;

        public TState CreateState<TState>() where TState : IState => 
            _objectResolver.Resolve<TState>();
    }
}