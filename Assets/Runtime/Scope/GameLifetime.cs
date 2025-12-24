using Mediapipe.Unity.Sample;
using Runtime.Game;
using Runtime.Game.Embient;
using Runtime.Game.Factories;
using Runtime.Game.Interfaces;
using Runtime.Game.Publishers;
using Runtime.Game.Timers;
using Runtime.Infrastructure;
using Runtime.Infrastructure.Interfaces;
using Runtime.Infrastructure.Video;
using Runtime.Machine;
using Runtime.Machine.States;
using Runtime.UI;
using Runtime.UI.Interfaces;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Runtime.Scope
{
    public class GameLifetime : LifetimeScope
    {
        [SerializeField] private BaseRunner publisherPrefab;

        protected override void Configure(IContainerBuilder builder)
        {
            base.Configure(builder);

            builder.Register<IStateMachine, StateMachine>(Lifetime.Singleton);
            builder.Register<IStateFactory, StateFactory>(Lifetime.Singleton);
            builder.Register<IWebCamInitializer, WebCamInitializer>(Lifetime.Singleton);
            builder.Register<IImageSourceProvider, ImageSourceSwitch>(Lifetime.Singleton);

            builder.Register<MainMenuState>(Lifetime.Scoped);
            builder.Register<PrepareGameState>(Lifetime.Scoped);
            builder.Register<GameState>(Lifetime.Scoped);
            builder.Register<GameOverState>(Lifetime.Scoped);
            builder.Register<ExitGameState>(Lifetime.Scoped);
            builder.Register<SettingsState>(Lifetime.Scoped);


            builder.RegisterEntryPoint<Timer>(Lifetime.Scoped).As<ITimer>();
            builder.RegisterEntryPoint<PosePublisher>(Lifetime.Scoped).As<IPosePublisher>();
            builder.RegisterEntryPoint<PlayerRaisePublisher>(Lifetime.Scoped).As<IPlayerRaisePublisher>();


            builder.RegisterComponentInNewPrefab(typeof(IPoseLandmarkPublisher), publisherPrefab, Lifetime.Scoped);

            builder.RegisterComponentInHierarchy<GameController>().As<IGameController>().As<ILevelPublisher>();

            builder.RegisterComponentInHierarchy<ObjectSpawner>().As<IObjectSpawner>();
            builder.RegisterComponentInHierarchy<UIController>().As<ICanvas>();
            builder.RegisterComponentInHierarchy<FoodFactory>().As<IFoodFactory>();
            builder.RegisterComponentInHierarchy<PlayerFactory>().As<IPlayerFactory>();
            builder.RegisterComponentInHierarchy<CameraController>().As<ICameraController>();
            builder.RegisterComponentInHierarchy<ObjectCollectSetup>().As<IScorePublisher>();
            builder.RegisterComponentOnNewGameObject<CoroutineScope>(Lifetime.Scoped).As<ICoroutineScope>();

            builder.RegisterEntryPoint<GameEntryPoint>();
        }
    }
}