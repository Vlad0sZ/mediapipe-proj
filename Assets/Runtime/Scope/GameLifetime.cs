using Runtime.Game;
using Runtime.Game.Controllers;
using Runtime.Game.Embient;
using Runtime.Game.Factories;
using Runtime.Game.Interfaces;
using Runtime.Game.Publishers;
using Runtime.Game.Stats;
using Runtime.Game.Timers;
using Runtime.Game.TrainingCustoms;
using Runtime.Game.Types;
using Runtime.Game.UI;
using Runtime.Infrastructure;
using Runtime.Infrastructure.Interfaces;
using Runtime.Infrastructure.Video;
using Runtime.Machine;
using Runtime.Machine.States;
using Runtime.UI;
using Runtime.UI.Interfaces;
using SensorPack.Addons.Mediapipe.Solutions.Runners.PoseRunners;
using SensorPack.KinectCore.Runtime;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Runtime.Scope
{
    public class GameLifetime : LifetimeScope
    {
        [SerializeField] private PoseSolution poseSolutionPrefab;
        [SerializeField] private KinectManager kinectManagerPrefab;

        protected override void Configure(IContainerBuilder builder)
        {
            base.Configure(builder);

            builder.Register<IStateMachine, StateMachine>(Lifetime.Singleton);
            builder.Register<IStateFactory, StateFactory>(Lifetime.Singleton);
            builder.Register<IGameControlFactory, GameControlFactory>(Lifetime.Singleton);
            builder.Register<IRecordsStorage, RecordsStorage>(Lifetime.Singleton);
            builder.Register<IWebCamInitializer, WebCamInitializer>(Lifetime.Singleton);
            builder.Register<IImageSourceProvider, ImageSourceSwitch>(Lifetime.Singleton);
            builder.Register<IFoodGroupProvider, FoodGroupProvider>(Lifetime.Singleton)
                .As<IFoodPayload>();

            builder.Register<IPauseController, PauseController>(Lifetime.Scoped);
            builder.Register<ILevelController, LevelController>(Lifetime.Scoped);

            builder.Register<MainMenuState>(Lifetime.Scoped);
            builder.Register<SettingsState>(Lifetime.Scoped);
            builder.Register<GameModeState>(Lifetime.Scoped);
            builder.Register<PrepareGameState>(Lifetime.Scoped);
            builder.Register<GameState>(Lifetime.Scoped);
            builder.Register<GameOverState>(Lifetime.Scoped);
            builder.Register<ExitGameState>(Lifetime.Scoped);
            builder.Register<RecordsState>(Lifetime.Scoped);

            builder.Register<IGameControl, ClassicGameControl>(Lifetime.Scoped).Keyed(GameMode.Classic);
            builder.Register<IGameControl, EndlessGameControl>(Lifetime.Scoped).Keyed(GameMode.Endless);
            builder.Register<IGameControl, TrainingGameControl>(Lifetime.Scoped).Keyed(GameMode.Training);

            builder.Register<ITrainingObjectSpawner, TrainingObjectSpawner>(Lifetime.Scoped);

            builder.RegisterEntryPoint<Timer>(Lifetime.Scoped).As<ITimer>();
            builder.RegisterEntryPoint<PosePublisher>(Lifetime.Scoped).As<IPosePublisher>();
            builder.RegisterEntryPoint<PlayerRaisePublisher>(Lifetime.Scoped).As<IPlayerRaisePublisher>();


            builder.RegisterComponentInNewPrefab(typeof(PoseSolution), poseSolutionPrefab, Lifetime.Singleton);
            builder.RegisterComponentInNewPrefab(typeof(KinectManager), kinectManagerPrefab, Lifetime.Singleton);
            builder.RegisterComponentInHierarchy<GameModeSettingsController>()
                .As<IGameModeSettings>()
                .As<ILevelSetup>();

            builder.RegisterComponentInHierarchy<ObjectSpawner>().As<IObjectSpawner>();
            builder.RegisterComponentInHierarchy<ObjectChainSetup>().As<ISpawnerSetup>();
            builder.RegisterComponentInHierarchy<UIController>().As<ICanvas>();
            builder.RegisterComponentInHierarchy<PlayerFactory>().As<IPlayerFactory>();
            builder.RegisterComponentInHierarchy<CameraController>().As<ICameraController>();
            builder.RegisterComponentInHierarchy<ObjectCollectSetup>().As<IScorePublisher>();
            builder.RegisterComponentInHierarchy<PrepareTaskUI>().As<IGameModePayload>();
            builder.RegisterComponentInHierarchy<TrainingUI>().As<ITrainingUI>();

            builder.RegisterComponentOnNewGameObject<CoroutineScope>(Lifetime.Scoped).As<ICoroutineScope>();

            builder.RegisterEntryPoint<GameEntryPoint>();
        }
    }
}