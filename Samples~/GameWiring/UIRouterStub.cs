using KitforgeLabs.MobileUIKit.Routing;
using UnityEngine;

namespace KitforgeLabs.MobileUIKit.GameWiring
{
    public class UIRouterStub
    {
        private AppState _currentState = AppState.Loading;

        public AppState CurrentState => _currentState;

        public void TransitionTo(AppState next)
        {
            Debug.Log($"[UIRouterStub] {_currentState} -> {next}");
            _currentState = next;
        }
    }
}
