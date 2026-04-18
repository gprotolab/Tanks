using UnityEngine;
using ANut.Core;

namespace Game.Battle
{
    public class PlayerTankHud : MonoBehaviour
    {
        [SerializeField] private ReloadBarView _reloadBar;

        public void Init()
        {
            // More components will be added later.

            if (_reloadBar != null)
                _reloadBar.Init();
            else
                Log.Warning("[PlayerTankHud] ReloadBarView is not assigned in the Inspector.");
        }
    }
}