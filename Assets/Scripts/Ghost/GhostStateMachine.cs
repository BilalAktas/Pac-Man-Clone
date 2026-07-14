using System.Collections.Generic;

namespace Core
{
    public enum GhostStateEnum
    {
        Patrol,
        Chase,
        Frightened,
        Die
    }

    public class GhostStateMachine
    {
        private readonly Dictionary<GhostStateEnum, GhostState> _states = new();
        private GhostState _currentState;

        public GhostStateEnum CurrentStateEnum { get; private set; }

        public void RegisterState(GhostStateEnum stateEnum, GhostState state)
        {
            _states[stateEnum] = state;
        }
        
        public void Start(GhostStateEnum startState)
        {
            CurrentStateEnum = startState;
            _currentState = _states[startState];
            _currentState.Enter();
        }

        public void ChangeState(GhostStateEnum newStateEnum)
        {
            if (newStateEnum == CurrentStateEnum) return;

            if (!_states.TryGetValue(newStateEnum, out var newState))
            {
                UnityEngine.Debug.LogWarning($"GhostStateMachine: '{newStateEnum}' doesnt exist.");
                return;
            }

            _currentState?.Exit();

            CurrentStateEnum = newStateEnum;
            _currentState = newState;
            _currentState.Enter();
        }

        public void Update()
        {
            _currentState?.Update();
        }

        public GhostState GetState(GhostStateEnum stateEnum)
        {
            return _states.TryGetValue(stateEnum, out var state) ? state : null;
        }
    }
}