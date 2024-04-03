using System;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Agava.Wink
{
    public class InputWindowPresenter : WindowPresenter
    {
        [SerializeField] private NotifyWindowPresenter _failWindow;
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private Button _sendButton;
        [SerializeField] private TMP_InputField _inputField;

        private Action<uint> _onInputDone;

        private void OnDestroy() => _sendButton.onClick.RemoveListener(OnSendCodeClicked);

        private void Awake() => _sendButton.onClick.AddListener(OnSendCodeClicked);

        public void Enable(Action<uint> onInputDone)
        {
            _onInputDone = onInputDone;
            EnableCanvasGroup(_canvasGroup);
        }

        public override void Enable() { }

        public override void Disable() => DisableCanvasGroup(_canvasGroup);

        public void OnSendCodeClicked()
        {
            if (string.IsNullOrEmpty(_inputField.text))
                return;

            bool isCorrectCode = uint.TryParse(_inputField.text, System.Globalization.NumberStyles.Any, CultureInfo.InvariantCulture, out uint resultCode);

            if (isCorrectCode == false)
            {
                _failWindow.Enable();
                return;
            }

            _onInputDone?.Invoke(resultCode);
            Disable();
        }

        internal void Clear() => _inputField.text = string.Empty;
    }
}
