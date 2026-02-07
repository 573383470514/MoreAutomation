using System;
using System.Collections.Generic;

namespace MoreAutomation.Automation.FSM
{
    public class StateMachine
    {
        private StateBase? _currentState;
        public StateBase? CurrentState => _currentState;

        public void TransitionTo(StateBase newState)
        {
            if (newState == null) throw new ArgumentNullException(nameof(newState));

            _currentState?.OnExit();
            _currentState = newState;
            _currentState.OnEnter();
        }

        public void Update() => _currentState?.OnUpdate();
    }
}