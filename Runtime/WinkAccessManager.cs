using SmsAuthLibrary.Program;
using System;
using System.Collections;
using UnityEngine;

namespace Agava.Wink
{
    [DefaultExecutionOrder(100)]
    public class WinkAccessManager : MonoBehaviour, IWinkAccessManager
    {
        [SerializeField] private string _functionId;

        private uint _checkCode = 0;

        public bool HasAccess { get; private set; } = false;
        public static IWinkAccessManager Instance {  get; private set; }
        public static readonly string UnlockKey = nameof(UnlockKey);

        public event Action OnSuccessfully;

        private void Start()
        {
            if(SmsAuthApi.Initialized == false)
                SmsAuthApi.Initialize(_functionId);

            DontDestroyOnLoad(this);

            if (PlayerPrefs.HasKey(UnlockKey) == false)
                HasAccess = false;
            else
                HasAccess = true;

            Instance ??= this;
        }

        internal async void SignIn(string phoneNumber, Action checkCodeRequested, Action<bool> winkSubscriptionAccessRequest)
        {
            Debug.Log("Sign in: " + phoneNumber);

            //var requets = await HttpClientSmtp.Get($"https://bsms.tele2.ru/api/" +
                //$"?operation={SMTPRequestType.send}" +
                //$"&login={login}" + //login=(логин рассылки/подключения)
                //$"&password={password}" + //password=(пароль рассылки/подлючения)
                //$"&msisdn={phoneNumber}" + //msisdn=(номер абонента – 11 цифр)
                //$"&shortcode={shortcode}" + //shortcode=(разрешѐнное имя отправителя – не более 11 символов в кодировке ASCII, за исключением символов: \x00-\x1F, «[»,«\»,«]»,«^», «`», «{»,«|», «}», «~») shortcode=(разрешѐнное имя отправителя – не более 11 символов в кодировке ASCII, за исключением символов: \x00-\x1F, «[»,«\»,«]»,«^», «`», «{»,«|», «}», «~»)
                //$"&text={code}"); //generated verify code

            StartCoroutine(WaitEntering());
            IEnumerator WaitEntering()
            {
                //TODO: Make Web Request to ADB. Send phone number.
                //request code { }

                //TODO: Reiceve message check code in to UI.
                checkCodeRequested?.Invoke();
                yield return new WaitWhile(() => _checkCode <= 0);

                //TODO: Send check code to DB and compare.
                Debug.Log("Check code send: " + _checkCode);

                //TODO: Reiceve call back compare result true/false.
                bool isCheckCodeCorrect = _checkCode > 0; // true for test

                if (isCheckCodeCorrect)
                {
                    //TODO: Make Web Request to Wink. Send phone number. Reiceve call back access true/false.
                    winkSubscriptionAccessRequest?.Invoke(true);
                    OnSubscriptionExist();
                }
                else
                {
                    winkSubscriptionAccessRequest?.Invoke(false);
                }
            }
        }

        internal void SetCheckCode(uint code) => _checkCode = code;

        internal void TestEnableSubsription() => OnSubscriptionExist();

        private void OnSubscriptionExist()
        {
            HasAccess = true;

            if (PlayerPrefs.HasKey(UnlockKey) == false)
                PlayerPrefs.SetString(UnlockKey, "Unlocked");

            OnSuccessfully?.Invoke();
        }
    }
}
