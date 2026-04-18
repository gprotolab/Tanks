using UnityEngine;

namespace Game.Battle
{
    public class TankExplosionEffect : MonoBehaviour
    {
        [SerializeField] private ParticleSystem _particles;

        public void Init()
        {
            Stop();
        }

        public void Play()
        {
            _particles.gameObject.SetActive(true);
            if (_particles != null)
                _particles.Play(withChildren: true);
        }

        public void Stop()
        {
            if (_particles != null)
            {
                _particles.Stop(withChildren: true, ParticleSystemStopBehavior.StopEmittingAndClear);
                _particles.gameObject.SetActive(false);
            }
        }
    }
}