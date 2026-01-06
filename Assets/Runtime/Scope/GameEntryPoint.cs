using JetBrains.Annotations;
using Runtime.Machine;
using Runtime.Machine.States;
using SensorPack.Addons.Mediapipe.Solutions.Runners.PoseRunners;
using VContainer.Unity;

namespace Runtime.Scope
{
    [UsedImplicitly]
    public class GameEntryPoint : IStartable
    {
        private readonly IStateMachine _stateMachine;
        private readonly PoseSolution _poseSolution;

        public GameEntryPoint(IStateMachine stateMachine, PoseSolution poseSolution)
        {
            _poseSolution = poseSolution;
            _stateMachine = stateMachine;
        }

        public void Start()
        {
            _poseSolution.Config.NumPoses = 1;
            _stateMachine.ChangeState<MainMenuState>();
        }
    }
}