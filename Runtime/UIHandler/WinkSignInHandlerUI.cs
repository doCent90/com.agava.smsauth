using System;
using System.Collections;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SmsAuthAPI.Program;
using SmsAuthAPI.DTO;
using TMPro;

namespace Agava.Wink
{
    /// <summary>
    ///     Handler UI. Input data and view auth process.
    /// </summary>
    public class WinkSignInHandlerUI : MonoBehaviour, IWinkSignInHandlerUI, ICoroutine
    {
        private const int MinutesFactor = 60;
        private const string RemoteName = "max-demo-minutes";

        [SerializeField] private DemoTimer _demoTimer;
        [SerializeField] private NotifyWindowHandler _notifyWindowHandler;
        [Header("UI Input")]
        [SerializeField] private TMP_InputField _codeInputField;
        [SerializeField] private TMP_InputField _numbersInputField;
        [Header("UI Buttons")]
        [SerializeField] private Button _signInButton;
        [SerializeField] private Button _openSignInButton;
        [SerializeField] private Button _unlinkButtonTemplate;
        [Header("UI Test Buttons")]
        [SerializeField] private Button _testSignInButton;
        [SerializeField] private Button _testDeleteButton;
        [Header("Phone Number Check Settings")]
        [SerializeField] private int _maxNumberCount = 30;
        [SerializeField] private int _minNumberCount = 5;
        [SerializeField] private int _codeCount = 4;
        [SerializeField] private bool _additivePlusChar = false;
        [Header("Factory components")]
        [SerializeField] private Transform _containerButtons;

        private WinkAccessManager _winkAccessManager;
        private readonly List<Button> _devicesIdButtons = new();

        public static WinkSignInHandlerUI Instance { get; private set; }

        public bool IsAnyWindowEnabled => _notifyWindowHandler.IsAnyWindowEnabled;
        public event Action AllWindowsClosed;
        public event Action SignInWindowClosed;

        public void Dispose()
        {
            _signInButton.onClick.RemoveAllListeners();

            if (_winkAccessManager == null) return;

            _winkAccessManager.ResetLogin -= OpenSignWindow;
            _winkAccessManager.LimitReached -= OnLimitReached;
            _winkAccessManager.Successfully -= OnSuccessfully;
            _demoTimer.Dispose();
        }

        public IEnumerator Construct(WinkAccessManager winkAccessManager)
        {
            if (Instance == null)
                Instance = this;

#if UNITY_EDITOR || TEST
            _testSignInButton.onClick.AddListener(OnTestSignInClicked);
            _testDeleteButton.onClick.AddListener(OnTestDeleteClicked);
            _testSignInButton.gameObject.SetActive(true);
            _testDeleteButton.gameObject.SetActive(true);
#else
            _testDeleteButton.gameObject.SetActive(false);
            _testSignInButton.gameObject.SetActive(false);
#endif
            _winkAccessManager = winkAccessManager;
            _signInButton.onClick.AddListener(OnSignInClicked);
            _openSignInButton.onClick.AddListener(OpenSignWindow);
            CloseAllWindows();

            _winkAccessManager.ResetLogin += OpenSignWindow;
            _winkAccessManager.LimitReached += OnLimitReached;
            _winkAccessManager.Successfully += OnSuccessfully;
            _demoTimer.TimerExpired += OnTimerExpired;

            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                _notifyWindowHandler.OpenWindow(WindowType.NoEnternet);
                yield return new WaitWhile(() => Application.internetReachability == NetworkReachability.NotReachable);
            }
            else
            {
                _notifyWindowHandler.CloseWindow(WindowType.NoEnternet);
            }

            SetRemoteConfig();
        }

        public void OpenSignWindow() => _notifyWindowHandler.OpenSignInWindow(() => SignInWindowClosed?.Invoke());
        public void OpenWindow(WindowType type) => _notifyWindowHandler.OpenWindow(type);
        public void CloseWindow(WindowType type) => _notifyWindowHandler.CloseWindow(type);

        public void CloseAllWindows()
        {
            _notifyWindowHandler.CloseAllWindows();
            AllWindowsClosed?.Invoke();
        }

        private async void SetRemoteConfig()
        {
            await Task.Yield();

            var response = await SmsAuthApi.GetRemoteConfig(RemoteName);

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

        private async void OnTestDeleteClicked()
        {
            if (WinkAccessManager.Instance.HasAccess == false)
                throw new System.Exception("Wink not authorizated!");

            await SmsAuthAPI.Utility.PlayerPrefs.Load();
            SmsAuthAPI.Utility.PlayerPrefs.DeleteAll();
            SmsAuthAPI.Utility.PlayerPrefs.Save();
        }
#endif

        private void OnSignInClicked()
        {
            string number = WinkAcceessHelper.GetNumber(_codeInputField.text, _numbersInputField.text,
                _minNumberCount, _maxNumberCount, _codeCount, _additivePlusChar);

            if (string.IsNullOrEmpty(number))
            {
                _notifyWindowHandler.OpenWindow(WindowType.WrongNumber);
                return;
            }

            _notifyWindowHandler.OpenWindow(WindowType.ProccessOn);

            _winkAccessManager.Regist(phoneNumber: number,
            otpCodeRequest: (hasOtpCode) =>
            {
                if (hasOtpCode)
                {
                    _notifyWindowHandler.CloseWindow(WindowType.ProccessOn);

                    _notifyWindowHandler.OpenInputWindow(onInputDone: (code) =>
                    {
                        _notifyWindowHandler.OpenWindow(WindowType.ProccessOn);
                        _winkAccessManager.SendOtpCode(code);
                    });
                }
                else
                {
                    _notifyWindowHandler.CloseWindow(WindowType.ProccessOn);
                    _notifyWindowHandler.OpenWindow(WindowType.Fail);
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
                    _notifyWindowHandler.OpenWindow(WindowType.Fail);
                    _notifyWindowHandler.CloseWindow(WindowType.ProccessOn);
                    _notifyWindowHandler.OpenWindow(WindowType.Redirect);
                }
            });
        }

        private void OnSignInDone()
        {
            _notifyWindowHandler.OpenWindow(WindowType.Successfully);
            _notifyWindowHandler.CloseWindow(WindowType.SignIn);
            _notifyWindowHandler.CloseWindow(WindowType.ProccessOn);
            OnSuccessfully();
        }

        private void OnLimitReached(IReadOnlyList<string> devicesList)
        {
            CloseAllWindows();
            _notifyWindowHandler.OnLimitReached();

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
            _notifyWindowHandler.CloseWindow(WindowType.Unlink);
            _notifyWindowHandler.OpenSignInWindow();
        }

        private void OnSuccessfully()
        {
            _openSignInButton.gameObject.SetActive(false);
            _demoTimer.Stop();
            _notifyWindowHandler.CloseWindow(WindowType.DemoTimerExpired);
        }

        private void OnTimerExpired() => _notifyWindowHandler.OpenWindow(WindowType.DemoTimerExpired);
    }
}
