using System;
using R3;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Game.Idle
{
    public class IdleClickAreaView : MonoBehaviour, IPointerClickHandler
    {
        public Observable<Unit> OnClicked => _onClicked;

        private readonly Subject<Unit> _onClicked = new();

        public void OnPointerClick(PointerEventData eventData)
        {
            _onClicked.OnNext(Unit.Default);
        }

        private void OnDestroy()
        {
            _onClicked.Dispose();
        }
    }
}