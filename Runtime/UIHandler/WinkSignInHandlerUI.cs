using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;
using SmsAuthAPI.Program;
using TMPro;
using System;

namespace Agava.Wink
{
    [DefaultExecutionOrder(-12)]
    public class WinkSignInHandlerUI : MonoBehaviour, ICoroutine
    {
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

        private void OnDestroy()
        {
            _signInButton.onClick.RemoveAllListeners();
            _winkAccessManager.ResetLogin -= OpenSignWindow;
            _winkAccessManager.LimitReached -= OnLimitReached;
            _winkAccessManager.Successfully -= OnSuccessfully;
            _demoTimer.Dispose();
        }

        private void Awake()
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

            //var response = await SmsAuthApi.GetDemoTimer();
            //int seconds = Convert.ToInt32(response.body);

            _demoTimer.Construct(_winkAccessManager, seconds: 10, this);
            _demoTimer.Start();
        }

        private void CloseAllWindows() => _windows.ForEach(window => window.Disable());

#if UNITY_EDITOR || TEST
        private void OnTestSignInClicked()
        {
            _winkAccessManager.TestEnableSubsription();
            _testSignInButton.gameObject.SetActive(false);
        }
#endif

        private void OpenSignWindow() => _signInWindow.Enable();

        private void OnSignInClicked()
        {
            var number = GetNumber();

            if (string.IsNullOrEmpty(number))
                return;

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

        private string GetNumber()
        {
            bool isCorrectCode = uint.TryParse(_codeInputField.text, System.Globalization.NumberStyles.Any, CultureInfo.InvariantCulture, out uint resultCode);
            bool isCorrectNumber = ulong.TryParse(_numbersInputField.text, System.Globalization.NumberStyles.Any, CultureInfo.InvariantCulture, out ulong resultNumber);

            int countCode = resultCode.ToString().Length;
            int countNumber = resultNumber.ToString().Length;

            if (isCorrectNumber == false || isCorrectCode == false
                || string.IsNullOrEmpty(_numbersInputField.text)
                || (countNumber < _minNumberCount || countNumber > _maxNumberCount)
                || (countCode > _codeCount || countCode <= 0 || resultCode == 0))
            {
                _wrongNumberWindow.Enable();
                return null;
            }

            string plus = _additivePlusChar == true ? "+" : "";
            string number = $"{plus}{resultCode}{resultNumber}";
            return number;
        }
    }
}
