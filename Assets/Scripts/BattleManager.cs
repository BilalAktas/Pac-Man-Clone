using System.Collections.Generic;
using UnityEngine;

namespace Core
{
    public enum PowerPelletState
    {
        Off,
        On
    }
    
    public class BattleManager : MonoBehaviour
    {
        private PowerPelletState _powerPelletState;
        private void Start()
        {
            EventBus.Subscribe<OnCollectPowerPelletEvent>(OnCollectPowerPellet);
            EventBus.Subscribe<OnBattleStartedEvent>(OnBattleStarted);
        }

        private void OnDestroy()
        {
            EventBus.Unsubscribe<OnCollectPowerPelletEvent>(OnCollectPowerPellet);
            EventBus.Unsubscribe<OnBattleStartedEvent>(OnBattleStarted);
        }

        private void OnCollectPowerPellet(OnCollectPowerPelletEvent e)
        {
            _powerPelletState = PowerPelletState.On;
        }

        private void OnBattleStarted(OnBattleStartedEvent e)
        {
            EventBus.Raise(new OnBattleFinishedEvent()
            {
                Winner = _powerPelletState == PowerPelletState.On ? e.Pacman : e.Ghost,
                Loser =  _powerPelletState == PowerPelletState.On ? e.Ghost : e.Pacman,
                Node = e.Node
            });
        }
    }
}