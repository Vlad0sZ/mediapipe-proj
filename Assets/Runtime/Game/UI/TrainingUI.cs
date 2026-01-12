using Runtime.Game.Factories;
using Runtime.Game.Interfaces;
using Runtime.Game.TrainingCustoms;
using Runtime.UI.Interfaces;
using UnityEngine;
using VContainer;

namespace Runtime.Game.UI
{
    public class TrainingUI : MonoBehaviour, ITrainingUI
    {
        [SerializeField] private GameObject playerTargetUi;
        [SerializeField] private GameObject itemPositiveTargetUi;
        [SerializeField] private GameObject itemNegativeTargetUi;
        [SerializeField] private GameObject scoreTargetUi;
        [SerializeField] private GameObject timerTargetUi;
        [SerializeField] private GameObject continueTargetUi;

        [Inject] private IPlayerFactory PlayerFactory { get; set; }

        public void ShowRaiseHandsHint()
        {
            var player = PlayerFactory.GetPlayer();
            var playerTransform = player.transform;
            ConfigureAttach(playerTargetUi, playerTransform, new Vector3(0, 0.5f, 0));
            ConfigureVisibility(playerTargetUi, true);
        }

        public void HideRaiseHandsHint()
        {
            ConfigureDetach(playerTargetUi);
            ConfigureVisibility(playerTargetUi, false);
        }

        public void ShowCollectHintOnItem(ICollectableItem item, bool isPositive)
        {
            var target = isPositive ? itemPositiveTargetUi : itemNegativeTargetUi;
            ConfigureAttach(target, item.gameObject.transform);
            ConfigureVisibility(target, true);
        }

        public void HideCollectHint(bool isPositive)
        {
            var target = isPositive ? itemPositiveTargetUi : itemNegativeTargetUi;
            ConfigureDetach(target);
            ConfigureVisibility(target, false);
        }

        public void ShowScoreHint()
        {
            ConfigureVisibility(scoreTargetUi, true);
        }

        public void HideScoreHint()
        {
            ConfigureVisibility(scoreTargetUi, false);
        }

        public void ShowTimerHint()
        {
            ConfigureVisibility(timerTargetUi, true);
        }

        public void ShowFinishHint()
        {
            ConfigureVisibility(continueTargetUi, true);
        }

        public void HideHints()
        {
            ConfigureVisibility(continueTargetUi, false);
            ConfigureVisibility(timerTargetUi, false);
            ConfigureVisibility(scoreTargetUi, false);
        }

        private void ConfigureAttach(GameObject obj, Transform target, Vector3 offset = default)
        {
            if (obj.TryGetComponent<FollowWorldTargetUI>(out var follow) == false)
                return;

            follow.AttachTo(target, offset);
        }

        private void ConfigureDetach(GameObject obj)
        {
            if (obj.TryGetComponent<FollowWorldTargetUI>(out var follow) == false)
                return;


            follow.Detach();
        }

        private void ConfigureVisibility(GameObject obj, bool isVisible)
        {
            if (obj.TryGetComponent<IScreen>(out var screen))
            {
                if (isVisible)
                    screen.Show();
                else
                    screen.Hide();
            }
            else
            {
                obj.SetActive(isVisible);
            }
        }
    }
}