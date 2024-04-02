using System;
using UnityEngine;
using SmsAuthLibrary.DTO;
using SmsAuthLibrary.Program;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Linq;
using Newtonsoft.Json;
using Utility;

namespace Agava.Wink
{
    [DefaultExecutionOrder(100)]
    public class WinkAccessManager : MonoBehaviour, IWinkAccessManager
    {
        private const string UniqueId = nameof(UniqueId);
        private const string PhoneNumber = nameof(PhoneNumber);
        private const string Tokens = nameof(Tokens);

        [SerializeField] private string _functionId;

        private LoginData _data;
        private Action<bool> _winkSubscriptionAccessRequest;
        private string _uniqueId;

        public static IWinkAccessManager Instance {  get; private set; }
        public bool HasAccess { get; private set; } = false;

        public event Action OnRefreshFail;
        public event Action OnSuccessfully;

        private void Start()
        {
            DontDestroyOnLoad(this);
            Instance ??= this;

            if (SmsAuthApi.Initialized == false)
                SmsAuthApi.Initialize(_functionId);

            if (PlayerPrefs.HasKey(UniqueId) == false)
                _uniqueId = Guid.NewGuid().ToString();
            else
                _uniqueId = PlayerPrefs.GetString(UniqueId);

            if (PlayerPrefs.HasKey(Tokens))
                QuickAccess();
        }

        public async void Regist(string phoneNumber, Action<bool> otpCodeRequest, Action<bool> winkSubscriptionAccessRequest)
        {
            Debug.Log("Try sign in: " + phoneNumber);
            PlayerPrefs.SetString(PhoneNumber, phoneNumber);

            _winkSubscriptionAccessRequest = winkSubscriptionAccessRequest;
            _data = new()
            {
                phone = phoneNumber,
                otp_code = 0,
                device_id = _uniqueId,
            };

            Response response = await SmsAuthApi.Regist(phoneNumber);

            if (response.statusCode != (uint)YdbStatusCode.Success)
            {
                otpCodeRequest?.Invoke(false);
                Debug.LogError("Error : " + response.statusCode);
            }
            else
            {
                otpCodeRequest?.Invoke(true);
            }
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
                string token = response.body;
                Tokens tokens = JsonConvert.DeserializeObject<Tokens>(token);
                SaveLoadService.Save(tokens, Tokens);
                RequestWinkDataBase();
            }
        }

        private async void QuickAccess()
        {
            Debug.Log("Try quick access");
            var tokens = SaveLoadService.Load<Tokens>(Tokens);

            if(tokens == null)
            {
                Debug.Log("Tokens not exhist");
                OnRefreshFail?.Invoke();
                return;
            }

            var handler = new JwtSecurityTokenHandler();
            JwtSecurityToken accessToken = handler.ReadJwtToken(tokens.access);
            JwtSecurityToken refreshToken = handler.ReadJwtToken(tokens.refresh);

            var expiryTimeAccess = Convert.ToInt64(accessToken.Claims.First(claim => claim.Type == "exp").Value);
            var expiryTimeRefresh = Convert.ToInt64(refreshToken.Claims.First(claim => claim.Type == "exp").Value);

            DateTime expiryDateTimeAccess = DateTimeOffset.FromUnixTimeSeconds(expiryTimeAccess).UtcDateTime;
            DateTime expiryDateTimeRefresh = DateTimeOffset.FromUnixTimeSeconds(expiryTimeRefresh).UtcDateTime;

            Debug.Log("Life Time Access: " + expiryDateTimeAccess);
            Debug.Log("Life Time Refresh: " + expiryDateTimeRefresh);
            string currentToken = string.Empty;

            if (expiryDateTimeAccess > DateTime.UtcNow)
            {
                currentToken = tokens.access;
                Debug.Log("Data Time: " + DateTime.UtcNow);
                Debug.Log("Token access exhist");
            }
            else if (expiryDateTimeRefresh > DateTime.UtcNow)
            {
                Debug.Log("Try refresh access token");
                var refreshResponse = await SmsAuthApi.Refresh(tokens.refresh);

                if (refreshResponse.statusCode != (uint)StatusCode.ValidationError)
                {
                    byte[] bytes = Convert.FromBase64String(refreshResponse.body);
                    string json = Encoding.UTF8.GetString(bytes);
                    var tokensBack = JsonConvert.DeserializeObject<Tokens>(json);

                    currentToken = tokensBack.access;
                    SaveLoadService.Save(tokensBack, Tokens);
                    Debug.Log("Refresh access token successfuly");
                }
                else
                {
                    Debug.LogError($"Refresh Token Validation Error :{refreshResponse.statusCode}-{refreshResponse.body}");
                    OnRefreshFail?.Invoke();
                    return;
                }
            }
            else
            {
                Debug.Log("Quick access denied. Tokens lifetime has expired. Try regist again");
                OnRefreshFail?.Invoke();
                SaveLoadService.Delete(Tokens);
                return;
            }

            var response = await SmsAuthApi.SampleAuth(currentToken);

            if(response.statusCode != (uint)StatusCode.ValidationError)
            {
                Debug.Log("Quick access successfully");
                OnSubscriptionExist();
            }
            else
            {
                Debug.LogError($"Quick access Validation Error: {response.body}-code: {response.statusCode}");
                OnRefreshFail?.Invoke();
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
            OnSuccessfully?.Invoke();
            Debug.Log("Access succesfully");
        }
    }
}
