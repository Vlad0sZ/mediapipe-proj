using Runtime.Infrastructure;
using Runtime.Infrastructure.Interfaces;
using Runtime.Infrastructure.Scenes;
using Runtime.Infrastructure.Video;
using Runtime.UI;
using Runtime.UI.Interfaces;
using VContainer;
using VContainer.Unity;

namespace Runtime.Scope
{
    public class BootstrapLifetime : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            RegisterSingletonServices(builder);
            
            builder.Register<ISceneLoader, SceneLoader>(Lifetime.Scoped);
            builder.Register<IWebCamInitializer, WebCamInitializer>(Lifetime.Scoped);
            builder.RegisterComponentInHierarchy<UIController>().As<ICanvas>();
            builder.RegisterEntryPoint<BootstrapEntryPoint>();
        }


        private static void RegisterSingletonServices(IContainerBuilder builder)
        {
            builder.RegisterComponentOnNewGameObject<CoroutineScope>(Lifetime.Singleton)
                .DontDestroyOnLoad()
                .As<ICoroutineScope>();
        }
    }
}