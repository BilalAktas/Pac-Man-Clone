using System;
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
        private float _pelletTimer;
        private const float PELLET_DURATION = 15f;
        
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
            _pelletTimer = 0;
            _powerPelletState = PowerPelletState.On;
        }

        private void Update()
        {
            if (_powerPelletState == PowerPelletState.Off) return;
            
            _pelletTimer += Time.deltaTime;
            if (_pelletTimer >= PELLET_DURATION)
            {
                _powerPelletState = PowerPelletState.Off;
                EventBus.Raise(new OnPowerPelletEffectFinishedEvent());
            }
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