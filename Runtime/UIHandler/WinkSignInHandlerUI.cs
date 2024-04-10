using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SmsAuthAPI.Program;
using TMPro;
using System;
using SmsAuthAPI.DTO;
using System.Threading.Tasks;
using System.Linq;

namespace Agava.Wink
{
    [DefaultExecutionOrder(-12)]
    internal class WinkSignInHandlerUI : MonoBehaviour, IWinkSignInHandlerUI, ICoroutine
    {
        private const int MinutesFactor = 60;
        [SerializeField] private WinkAccessManager _winkAccessManager;
        [SerializeField] private DemoTimer _demoTimer;
        [Header("UI Windows")]
        [SerializeField] private NotifyWindowPresenter _signInWindow;
        [SerializeField] private NotifyWindowPresenter _failWindow;
        [SerializeField] private NotifyWindowPresenter _wrongNumberWindow;
        [SerializeField] private NotifyWindowPresenter _proccesOnWindow;
        [SerializeField] private NotifyWindowPresenter _successfullyWindow;
        [SerializeField] private NotifyWindowPresenter _unlinkWindow;
        [SerializeField] private NotifyWindowPresenter _demoTimerExpiredWindow;
        [SerializeField] private RedirectWindowPresenter _redirectToWebsiteWindow;
        [SerializeField] private InputWindowPresenter _enterCodeWindow;
        [SerializeField] private List<WindowPresenter> _windows;
        [Header("UI Input")]
        [SerializeField] private TMP_InputField _codeInputField;
        [SerializeField] private TMP_InputField _numbersInputField;
        [Header("UI Buttons")]
        [SerializeField] private Button _signInButton;
        [SerializeField] private Button _openSignInButton;
        [SerializeField] private Button _testSignInButton;
        [SerializeField] private Button _unlinkButtonTemplate;
        [Header("Phone Number Check Settings")]
        [SerializeField] private int _maxNumberCount = 30;
        [SerializeField] private int _minNumberCount = 5;
        [SerializeField] private int _codeCount = 4;
        [SerializeField] private bool _additivePlusChar = false;
        [Header("Factory components")]
        [SerializeField] private Transform _containerButtons;

        private readonly List<Button> _devicesIdButtons = new();

        public bool IsAnyWindowEnabled => _windows.Any(window => window.HasOpened);
        public event Action WindowsClosed;

        private void OnDestroy()
        {
            _signInButton.onClick.RemoveAllListeners();
            _winkAccessManager.ResetLogin -= OpenSignWindow;
            _winkAccessManager.LimitReached -= OnLimitReached;
            _winkAccessManager.Successfully -= OnSuccessfully;
            _demoTimer.Dispose();
        }

        private async void Awake()
        {
            _signInButton.onClick.AddListener(OnSignInClicked);
#if UNITY_EDITOR || TEST
            _testSignInButton.onClick.AddListener(OnTestSignInClicked);
            _testSignInButton.gameObject.SetActive(true);
#else
            _testSignInButton.gameObject.SetActive(false);
#endif
            _openSignInButton.onClick.AddListener(OpenSignWindow);
            CloseAllWindows();

            _winkAccessManager.ResetLogin += OpenSignWindow;
            _winkAccessManager.LimitReached += OnLimitReached;
            _winkAccessManager.Successfully += OnSuccessfully;
            _demoTimer.TimerExpired += OnTimerExpired;

            await SetRemoteConfig();
        }

        public void OpenSignWindow() => _signInWindow.Enable();

        public void OpenWindow(WindowPresenter window) => window.Enable();
        public void CloseWindow(WindowPresenter window) => window.Disable();

        public void CloseAllWindows()
        {
            _windows.ForEach(window => window.Disable());
            WindowsClosed?.Invoke();
        }

        private async Task SetRemoteConfig()
        {
            await Task.Yield();

            var response = await SmsAuthApi.GetRemoteConfig("max-demo-minutes");

            if (response.statusCode == (uint)YbdStatusCode.Success)
            {
                int seconds;

                if (string.IsNullOrEmpty(response.body))
                    seconds = 0;
                else
                    seconds = Convert.ToInt32(response.body) * MinutesFactor;

                _demoTimer.Construct(_winkAccessManager, seconds, this, this);
                _demoTimer.Start();
                Debug.Log("Remote setted: " + response.body);
            }
            else
            {
                Debug.LogError("Fail to recieve remote config: " + response.statusCode);
            }
        }

#if UNITY_EDITOR || TEST
        private void OnTestSignInClicked()
        {
            _winkAccessManager.TestEnableSubsription();
            _testSignInButton.gameObject.SetActive(false);
        }
#endif

        private void OnSignInClicked()
        {
            string number = WinkAcceessHelper.GetNumber(_codeInputField.text, _numbersInputField.text,
                _minNumberCount, _maxNumberCount, _codeCount, _additivePlusChar);

            if (string.IsNullOrEmpty(number))
            {
                _wrongNumberWindow.Enable();
                return;
            }

            _proccesOnWindow.Enable();

            _winkAccessManager.Regist(phoneNumber: number,
            otpCodeRequest: (hasOtpCode) =>
            {
                if (hasOtpCode)
                {
                    _proccesOnWindow.Disable();
                    _enterCodeWindow.Enable(onInputDone: (code) =>
                    {
                        _proccesOnWindow.Enable();
                        _winkAccessManager.SendOtpCode(code);
                    });
                }
                else
                {
                    _proccesOnWindow.Disable();
                    _failWindow.Enable();
                }
            },
            winkSubscriptionAccessRequest: (hasAccess) =>
            {
                if (hasAccess)
                {
                    OnSignInDone();
                }
                else
                {
                    _failWindow.Enable();
                    _proccesOnWindow.Disable();
                    _redirectToWebsiteWindow.Enable();
                }
            });
        }

        private void OnSignInDone()
        {
            _successfullyWindow.Enable();
            _signInWindow.Disable();
            _proccesOnWindow.Disable();
            OnSuccessfully();
        }

        private void OnLimitReached(IReadOnlyList<string> devicesList)
        {
            CloseAllWindows();
            _enterCodeWindow.Clear();
            _unlinkWindow.Enable();

            foreach (string device in devicesList)
            {
                Button button = Instantiate(_unlinkButtonTemplate, _containerButtons);
                button.GetComponentInChildren<TMP_Text>().text = device;
                button.onClick.AddListener(()
                    => OnUnlinkClicked(button.GetComponentInChildren<TMP_Text>().text));
                _devicesIdButtons.Add(button);
            }
        }

        private void OnUnlinkClicked(string device)
        {
            foreach (Button button in _devicesIdButtons)
            {
                button.onClick.RemoveListener(()
                    => OnUnlinkClicked(button.GetComponentInChildren<TMP_Text>().text));
            }

            _devicesIdButtons.Clear();
            _winkAccessManager.Unlink(device);
            _unlinkWindow.Disable();
            _signInWindow.Enable();
        }

        private void OnSuccessfully()
        {
            _openSignInButton.gameObject.SetActive(false);
            _demoTimer.Stop();
            _demoTimerExpiredWindow.Disable();
        }

        private void OnTimerExpired() => _demoTimerExpiredWindow.Enable();
    }
}
