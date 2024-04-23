using System;
using System.Collections.Generic;
using UnityEngine;
using SmsAuthAPI.DTO;
using SmsAuthAPI.Program;

namespace Agava.Wink
{
    /// <summary>
    ///     Auth process logic.
    /// </summary>
    public class WinkAccessManager : MonoBehaviour, IWinkAccessManager
    {
        private const string UniqueId = nameof(UniqueId);

        [SerializeField] private string _functionId;
        [SerializeField] private string _additiveId;

        private RequestHandler _requestHandler;
        private LoginData _data;
        private Action<bool> _winkSubscriptionAccessRequest;
        private string _uniqueId;

        public bool HasAccess { get; private set; } = false;
        public static WinkAccessManager Instance { get; private set; }

        public event Action<IReadOnlyList<string>> LimitReached;
        public event Action ResetLogin;
        public event Action Successfully;

        public void Construct()
        {
            if (Instance == null)
                Instance = this;

            _requestHandler = new();
            DontDestroyOnLoad(this);
      
            if (SmsAuthApi.Initialized == false)
                SmsAuthApi.Initialize(_functionId);

            if (UnityEngine.PlayerPrefs.HasKey(UniqueId) == false)
                _uniqueId = SystemInfo.deviceName + Application.identifier + _additiveId;
            else
                _uniqueId = UnityEngine.PlayerPrefs.GetString(UniqueId);

            if (UnityEngine.PlayerPrefs.HasKey(TokenLifeHelper.Tokens))
                QuickAccess();
        }

        public void SendOtpCode(uint enteredOtpCode)
        {
            _data.otp_code = enteredOtpCode;
            Login(_data);
        }

        public async void Regist(string phoneNumber, Action<bool> otpCodeRequest, Action<bool> winkSubscriptionAccessRequest)
        {
            _winkSubscriptionAccessRequest = winkSubscriptionAccessRequest;
            _data = await _requestHandler.Regist(phoneNumber, _uniqueId, otpCodeRequest);
        }

        public void Unlink(string deviceId) => _requestHandler.Unlink(deviceId, ResetLogin);

#if UNITY_EDITOR || TEST
        public void TestEnableSubsription()
        {
            HasAccess = true;
            Successfully?.Invoke();
            Debug.Log("Test Access succesfully. No cloud saves");
        }
#endif

        private void Login(LoginData data) 
            => _requestHandler.Login(data, LimitReached, _winkSubscriptionAccessRequest, OnSubscriptionExist);

        private void QuickAccess() => _requestHandler.QuickAccess(OnSubscriptionExist, ResetLogin);

        private void OnSubscriptionExist()
        {
            HasAccess = true;
            Successfully?.Invoke();
            Debug.Log("Access succesfully");
        }
    }
}
