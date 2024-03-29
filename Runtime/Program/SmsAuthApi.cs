using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SmsAuthLibrary.DTO;

namespace SmsAuthLibrary.Program
{
    public static class SmsAuthApi
    {
        private static YandexFunction _function;

        public static void Initialize(string functionId)
        {
            if (Initialized)
                throw new InvalidOperationException(nameof(SmsAuthApi) + " has already been initialized");

            _function = new YandexFunction(functionId ?? throw new ArgumentNullException(nameof(functionId)));
        }

        public static bool Initialized => _function != null;

        public async static Task<Response> Login(LoginData loginData)
        {
            EnsureInitialize();

            var request = new Request()
            {
                method = "LOGIN",
                body = JsonConvert.SerializeObject(loginData),
                access_token = "",
            };

            return await _function.Post(request);
        }

        public async static Task<Response> Regist(LoginData loginData)
        {
            EnsureInitialize();

            var request = new Request()
            {
                method = "REGISTRATION",
                body = JsonConvert.SerializeObject(loginData),
                access_token = "",
            };

            return await _function.Post(request);
        }

        public async static Task<Response> Refresh(LoginData loginData)
        {
            EnsureInitialize();

            var request = new Request()
            {
                method = "REFRESH",
                body = JsonConvert.SerializeObject(loginData),
                access_token = "",
            };

            return await _function.Post(request);
        }

        public async static Task<Response> Unlink(LoginData loginData)
        {
            EnsureInitialize();

            var request = new Request()
            {
                method = "UNLINK",
                body = JsonConvert.SerializeObject(loginData),
                access_token = "",
            };

            return await _function.Post(request);
        }

        public async static Task<Response> SampleAuth(LoginData loginData)
        {
            EnsureInitialize();

            var request = new Request()
            {
                method = "SAMPLE_AUTH",
                body = JsonConvert.SerializeObject(loginData),
                access_token = "",
            };

            return await _function.Post(request);
        }

        private static void EnsureInitialize()
        {
            if (Initialized == false)
                throw new InvalidOperationException(nameof(SmsAuthApi) + " is not initialized");
        }
    }
}
