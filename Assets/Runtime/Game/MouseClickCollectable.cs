using System;
using Runtime.Game.Interfaces;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Runtime.Game
{
    [RequireComponent(typeof(ICollectableItem))]
    public class MouseClickCollectable : MonoBehaviour, IPointerClickHandler
    {
        private ICollectableItem _collectableItem;
        private void Awake() => _collectableItem = GetComponent<ICollectableItem>();

        public void OnPointerClick(PointerEventData eventData)
        {
            UnityEngine.Debug.Log($"collect {_collectableItem.gameObject}");
            _collectableItem.Collect();
        }
    }
}