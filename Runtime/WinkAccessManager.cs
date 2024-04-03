using System;
using System.Text;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using SmsAuthAPI.DTO;
using SmsAuthAPI.Utility;
using SmsAuthAPI.Program;

namespace Agava.Wink
{
    [DefaultExecutionOrder(100)]
    public class WinkAccessManager : MonoBehaviour, IWinkAccessManager
    {
        private const string UniqueId = nameof(UniqueId);
        private const string PhoneNumber = nameof(PhoneNumber);

        [SerializeField] private string _functionId;
        [SerializeField] private string _additiveId;

        private LoginData _data;
        private Action<bool> _winkSubscriptionAccessRequest;
        private string _uniqueId;

        public bool HasAccess { get; private set; } = false;
        public static IWinkAccessManager Instance {  get; private set; }

        public event Action<IReadOnlyList<string>> LimitReached;
        public event Action ResetLogin;
        public event Action Successfully;

        private void Awake()
        {
            Instance ??= this;            
            DontDestroyOnLoad(this);
        }

        private void Start()
        {
            if (SmsAuthApi.Initialized == false)
                SmsAuthApi.Initialize(_functionId);

            if (PlayerPrefs.HasKey(UniqueId) == false)
                _uniqueId = SystemInfo.deviceName + _additiveId;
            else
                _uniqueId = PlayerPrefs.GetString(UniqueId);

            if (PlayerPrefs.HasKey(TokenLifeHelper.Tokens))
                QuickAccess();
        }

        public async void Regist(string phoneNumber, Action<bool> otpCodeRequest, Action<bool> winkSubscriptionAccessRequest)
        {
            PlayerPrefs.SetString(PhoneNumber, phoneNumber);

            _winkSubscriptionAccessRequest = winkSubscriptionAccessRequest;
            _data = new()
            {
                phone = phoneNumber,
                otp_code = 0,
                device_id = _uniqueId,
            };

            Response response = await SmsAuthApi.Regist(phoneNumber);

            if (response.statusCode != (uint)YbdStatusCode.Success)
            {
                otpCodeRequest?.Invoke(false);
                Debug.LogError("Regist Error : " + response.statusCode);
            }
            else
            {
                otpCodeRequest?.Invoke(true);
            }
        }

        public async void Unlink(string deviceId)
        {
            Debug.Log(deviceId);

            var tokens = SaveLoadLocalDataService.Load<Tokens>(TokenLifeHelper.Tokens);
            var resopnse = await SmsAuthApi.Unlink(tokens.access, deviceId);

            if(resopnse.statusCode != (uint)YbdStatusCode.Success)
                Debug.LogError("Unlink fail: " + resopnse.statusCode);
            else
                ResetLogin?.Invoke();
        }

        public void SendOtpCode(uint enteredOtpCode)
        {
            _data.otp_code = enteredOtpCode;
            Login(_data);
        }

        internal void TestEnableSubsription() => OnSubscriptionExist();

        private async void Login(LoginData data)
        {
            var response = await SmsAuthApi.Login(data);

            if (response.statusCode == (uint)StatusCode.ValidationError)
            {
                Debug.LogError("ValidationError : " + response.statusCode);
                _winkSubscriptionAccessRequest?.Invoke(false);
            }
            else
            {
                string token;

                if (response.isBase64Encoded)
                {
                    byte[] bytes = Convert.FromBase64String(response.body);
                    token = Encoding.UTF8.GetString(bytes);
                }
                else
                {
                    token = response.body;
                }

                Tokens tokens = JsonConvert.DeserializeObject<Tokens>(token);
                SaveLoadLocalDataService.Save(tokens, TokenLifeHelper.Tokens);

                if (string.IsNullOrEmpty(tokens.refresh))
                {
                    OnLimitDevicesReached();
                    return;
                }

                RequestWinkDataBase();
            }
        }

        private async void QuickAccess()
        {
            var tokens = SaveLoadLocalDataService.Load<Tokens>(TokenLifeHelper.Tokens);

            if(tokens == null)
            {
                Debug.LogError("Tokens not exhist");
                ResetLogin?.Invoke();
                return;
            }

            string currentToken = string.Empty;

            if (TokenLifeHelper.IsTokenAlive(tokens.access))
            {
                currentToken = tokens.access;
            }
            else if (TokenLifeHelper.IsTokenAlive(tokens.refresh))
            {
                currentToken = await TokenLifeHelper.GetRefreshedToken(tokens.refresh);

                if(string.IsNullOrEmpty(currentToken))
                {
                    ResetLogin?.Invoke();
                    return;
                }
            }
            else
            {
                ResetLogin?.Invoke();
                SaveLoadLocalDataService.Delete(TokenLifeHelper.Tokens);
                return;
            }

            var response = await SmsAuthApi.SampleAuth(currentToken);

            if(response.statusCode != (uint)StatusCode.ValidationError)
            {
                OnSubscriptionExist();
            }
            else
            {
                Debug.LogError($"Quick access Validation Error: {response.body}-code: {response.statusCode}");
                ResetLogin?.Invoke();
            }
        }

        private async void OnLimitDevicesReached()
        {
            Tokens tokens = TokenLifeHelper.GetTokens();
            var response = await SmsAuthApi.GetDevices(tokens.access);

            if (response.statusCode != (uint)YbdStatusCode.Success)
            {
                Debug.Log("Error");
            }
            else
            {
                IReadOnlyList<string> devices = JsonConvert.DeserializeObject<List<string>>(response.body);
                LimitReached?.Invoke(devices);
            }
        }

        private void RequestWinkDataBase() //TODO: Make Wink request
        {
            _winkSubscriptionAccessRequest?.Invoke(true);
            OnSubscriptionExist(); 
        }

        private void OnSubscriptionExist()
        {
            HasAccess = true;
            Successfully?.Invoke();
            Debug.Log("Access succesfully");
        }
    }
}
