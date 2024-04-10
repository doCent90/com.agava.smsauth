using System;
using System.Collections;
using UnityEngine;

namespace Agava.Wink
{
    [Serializable]
    public class DemoTimer
    {
        private const string TimerKey = nameof(TimerKey);
        private const float Delay = 5f;

        [SerializeField] private int _defaultTimerSeconds = 1800;
        [SerializeField] private bool _test = false;

        private WinkAccessManager _winkAccessManager;
        private ICoroutine _coroutine;

        private Coroutine _current;
        private int _seconds;

        public event Action TimerExpired;

        public void Construct(WinkAccessManager winkAccessManager, int seconds, ICoroutine coroutine)
        {
            _winkAccessManager = winkAccessManager;
            _coroutine = coroutine;

            if (seconds <= 0 && _test == false)
                seconds = _defaultTimerSeconds;

            if (UnityEngine.PlayerPrefs.HasKey(TimerKey) == false)
            {
                if (_test)
                    _seconds = _defaultTimerSeconds;
                else
                    _seconds = seconds;
            }
            else
            {
                _seconds = UnityEngine.PlayerPrefs.GetInt(TimerKey);
            }

            if (_seconds <= 0)
                _seconds = seconds;

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
                    _seconds--;
                    UnityEngine.PlayerPrefs.SetInt(TimerKey, _seconds);

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
