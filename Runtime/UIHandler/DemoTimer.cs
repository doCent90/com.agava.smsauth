using System;
using System.Collections;
using UnityEngine;

namespace Agava.Wink
{
    [Serializable]
    internal class DemoTimer
    {
        private const string TimerKey = nameof(TimerKey);
        private const float Delay = 5f;

        [Min(0)]
        [SerializeField] private int _defaultTimerSeconds = 1800;

        private WinkAccessManager _winkAccessManager;
        private IWinkSignInHandlerUI _winkSignInHandlerUI;
        private ICoroutine _coroutine;

        private Coroutine _current;
        private int _seconds;

        public event Action TimerExpired;

        public void Construct(WinkAccessManager winkAccessManager, int remoteCfgSeconds, IWinkSignInHandlerUI winkSignInHandlerUI, ICoroutine coroutine)
        {
            _winkSignInHandlerUI = winkSignInHandlerUI;
            _winkAccessManager = winkAccessManager;
            _coroutine = coroutine;

            if (remoteCfgSeconds <= 0)
                remoteCfgSeconds = _defaultTimerSeconds;

            if (UnityEngine.PlayerPrefs.HasKey(TimerKey))
                _seconds = UnityEngine.PlayerPrefs.GetInt(TimerKey);
            else
                _seconds = remoteCfgSeconds;

            _winkAccessManager.Successfully += Stop;
        }

        public void Dispose()
        {
            _winkAccessManager.Successfully -= Stop;
            UnityEngine.PlayerPrefs.SetInt(TimerKey, _seconds);
        }

        public void Start()
        {
            _current = _coroutine.StartCoroutine(Ticking());
            IEnumerator Ticking()
            {
                var tick = new WaitForSecondsRealtime(1);
                var waitBeforeStart = new WaitForSecondsRealtime(Delay);

                yield return waitBeforeStart;

                if (WinkAccessManager.Instance.HasAccess)
                    Stop();

                while (_seconds > 0)
                {
                    if (_winkSignInHandlerUI.IsAnyWindowEnabled == false)
                    {
                        _seconds--;
                        UnityEngine.PlayerPrefs.SetInt(TimerKey, _seconds);
                    }

                    yield return tick;
                }

                if(_seconds <= 0 && WinkAccessManager.Instance.HasAccess == false)
                    TimerExpired?.Invoke();
            }
        }

        public void Stop()
        {
            if (_current != null)
            {
                _coroutine.StopCoroutine(_current);
                _current = null;
            }
        }
    }
}
