using System;
using System.Collections;
using UnityEngine;

namespace Agava.Wink
{
    [Serializable]
    public class DemoTimer
    {
        [SerializeField] private int _defaultTimerSeconds = 1800;

        private const string TimerKey = nameof(TimerKey);

        private WinkAccessManager _winkAccessManager;
        private ICoroutine _coroutine;

        private Coroutine _current;
        private int _seconds;

        public event Action TimerExpired;

        public void Construct(WinkAccessManager winkAccessManager, int seconds, ICoroutine coroutine)
        {
            _winkAccessManager = winkAccessManager;
            _coroutine = coroutine;

            if (seconds <= 0)
                seconds = _defaultTimerSeconds;

            if (UnityEngine.PlayerPrefs.HasKey(TimerKey) == false)
                _seconds = seconds;
            else
                _seconds = UnityEngine.PlayerPrefs.GetInt(TimerKey);

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
            _coroutine.StartCoroutine(Ticking());
            IEnumerator Ticking()
            {
                var tick = new WaitForSecondsRealtime(1);
                var waitBeforeStart = new WaitForSecondsRealtime(10);

                yield return waitBeforeStart;

                while (_seconds > 0)
                {
                    _seconds--;
                    UnityEngine.PlayerPrefs.SetInt(TimerKey, _seconds);

                    yield return tick;
                }

                if(_seconds <= 0)
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
