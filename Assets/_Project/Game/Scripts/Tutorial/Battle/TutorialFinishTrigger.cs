using System;
using Game.Battle;
using R3;
using UnityEngine;

namespace Game.Tutorial
{
    public class TutorialFinishTrigger : MonoBehaviour
    {
        private readonly Subject<Unit> _onPlayerEntered = new();

        public Observable<Unit> OnPlayerEntered => _onPlayerEntered;

        private void OnTriggerEnter(Collider other)
        {
            var tank = other.GetComponentInParent<Tank>();
            if (tank == null || !tank.IsPlayer) return;

            _onPlayerEntered.OnNext(Unit.Default);
        }

        private void OnDestroy()
        {
            _onPlayerEntered.Dispose();
        }
    }
}