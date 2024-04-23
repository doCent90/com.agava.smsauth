using System;
using UnityEngine;
using UnityEngine.UI;

namespace Agava.Wink
{
    internal class SignInWindowPresenter : WindowPresenter
    {
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private Button _closeButton;

        private Action Closed;

        private void OnDestroy() => _closeButton.onClick.RemoveListener(Disable);

        private void Awake() => _closeButton.onClick.AddListener(Disable);

        public override void Enable() => EnableCanvasGroup(_canvasGroup);

        public void Enable(Action closeCallback)
        {            
            Enable();
            Closed = closeCallback;
        }

        public override void Disable()
        {
            DisableCanvasGroup(_canvasGroup);
            Closed?.Invoke();
        }
    }
}
