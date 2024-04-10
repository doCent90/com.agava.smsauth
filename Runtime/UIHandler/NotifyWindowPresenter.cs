using UnityEngine;
using UnityEngine.UI;

namespace Agava.Wink
{
    internal class NotifyWindowPresenter : WindowPresenter
    {
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private Button _closeButton;

        private void OnDestroy() => _closeButton.onClick.RemoveListener(Disable);

        private void Awake() => _closeButton.onClick.AddListener(Disable);

        public override void Enable() => EnableCanvasGroup(_canvasGroup);

        public override void Disable() => DisableCanvasGroup(_canvasGroup);
    }
}
