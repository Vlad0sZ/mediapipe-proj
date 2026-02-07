using Runtime.Infrastructure.Scenes;
using VContainer;
using VContainer.Unity;

namespace Runtime.Scope
{
    public class MenuLifetime : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder) =>
            builder.Register<ISceneLoader, SceneLoader>(Lifetime.Scoped);
    }
}