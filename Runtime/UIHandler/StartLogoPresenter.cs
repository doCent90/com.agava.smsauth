using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Agava.Wink
{
    /// <summary>
    ///     Show logo on start app.
    /// </summary>
    internal class StartLogoPresenter : MonoBehaviour
    {
        [field: SerializeField] public float LogoDuration { get; private set; } = 3f;

        [SerializeField] private CanvasGroup _logoGroup;
        [SerializeField] private CanvasGroup _bootGroup;
        [SerializeField] private Image _enLogo;
        [SerializeField] private Image _ruLogo;
        [SerializeField] private Image _loading;
        [SerializeField] private bool _enable = true;

        private void Update()
        {
            if (SmsAuthAPI.Utility.PlayerPrefs.s_Loaded == false)
                _loading.transform.localEulerAngles += new Vector3(0, 0, 2f);
        }

        internal void Construct()
        {
            _enLogo.gameObject.SetActive(false);
            _ruLogo.gameObject.SetActive(false);
        }

        /// <summary>
        ///     Show localized logo.
        /// </summary>
        internal void ShowLogo()
        {
            if (_enable == false) return;

            if (Application.systemLanguage == SystemLanguage.Russian)
                _ruLogo.gameObject.SetActive(true);
            else
                _enLogo.gameObject.SetActive(true);
        }

        /// <summary>
        ///     Slow shotdown logo.
        /// </summary>
        internal IEnumerator HidingLogo()
        {
            while (_logoGroup.alpha > 0.1 && _enable)
            {
                _logoGroup.alpha -= 0.1f;
                yield return new WaitForFixedUpdate();
            }

            _enLogo.gameObject.SetActive(false);
            _ruLogo.gameObject.SetActive(false);
        }

        /// <summary>
        ///     Hide background and loading view.
        /// </summary>
        internal void CloseBootView()
        {
            _bootGroup.alpha = 0;
        }
    }
}