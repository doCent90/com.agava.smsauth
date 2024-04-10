using UnityEngine;
using UnityEngine.UI;

namespace Agava.Wink
{
    internal class RedirectWindowPresenter : WindowPresenter
    {
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private Button _yesButton;
        [SerializeField] private Button _closeButton;
        [SerializeField] private string _url;

        private void OnDestroy() => _closeButton.onClick.RemoveAllListeners();

        private void Awake()
        {
            _closeButton.onClick.AddListener(Disable);
            _yesButton.onClick.AddListener(OnYesClicked);
        }

        public override void Enable() => EnableCanvasGroup(_canvasGroup);

        public override void Disable() => DisableCanvasGroup(_canvasGroup);

        private void OnYesClicked()
        {
            Application.OpenURL(_url);
            Disable();
        }
    }
}
