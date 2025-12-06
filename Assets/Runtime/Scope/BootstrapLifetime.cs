using Runtime.Game.Timers;
using Runtime.Infrastructure;
using Runtime.Infrastructure.Interfaces;
using Runtime.Infrastructure.Scenes;
using Runtime.Infrastructure.Video;
using Runtime.Machine;
using Runtime.Machine.States;
using Runtime.UI;
using Runtime.UI.Interfaces;
using VContainer;
using VContainer.Unity;
using ITimer = System.Threading.ITimer;

namespace Runtime.Scope
{
    public class BootstrapLifetime : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            RegisterSingletonServices(builder);
            RegisterStateMachine(builder);

            builder.Register<IWebCamInitializer, WebCamInitializer>(Lifetime.Scoped);
            builder.RegisterComponentInHierarchy<UIController>().As<ICanvas>();
            builder.RegisterEntryPoint<BootstrapEntryPoint>();
        }


        private static void RegisterSingletonServices(IContainerBuilder builder)
        {
            builder.RegisterComponentOnNewGameObject<CoroutineScope>(Lifetime.Singleton)
                .DontDestroyOnLoad()
                .As<ICoroutineScope>();

            builder.Register<ISceneLoader, SceneLoader>(Lifetime.Singleton);
        }

        private static void RegisterStateMachine(IContainerBuilder builder)
        {
            builder.Register<IStateMachine, StateMachine>(Lifetime.Singleton);
            builder.Register<IStateFactory, StateFactory>(Lifetime.Singleton);

            builder.Register<BootstrapState>(Lifetime.Scoped);
            builder.Register<NoWebCamState>(Lifetime.Scoped);
            builder.Register<LoadingGameState>(Lifetime.Scoped);
        }
    }
}