using System;
using UnityEngine;
using UnityEngine.UI;

namespace Agava.Wink
{
    public class LockedObject : MonoBehaviour
    {
        [SerializeField] private Image _lockImage;
        [SerializeField] private Button _button;

        public bool IsLocked { get; private set; } = true;

        private void OnDestroy() 
            => WinkAccessManager.Instance.Successfully -= OnSuccessfully;

        private void Awake()
        {
            if (WinkAccessManager.Instance == null)
                throw new NullReferenceException(this.name);

            if (WinkAccessManager.Instance.HasAccess)
                SetLock(isLocked: false);
            else
                SetLock(isLocked: true);

            WinkAccessManager.Instance.Successfully += OnSuccessfully;
        }

        public void SetLock(bool isLocked)
        {
            if (isLocked)
            {
                _lockImage.gameObject.SetActive(true);
                _button.interactable = false;
            }
            else
            {
                _lockImage.gameObject.SetActive(false);
                _button.interactable = true;
            }

            IsLocked = isLocked;
        }

        private void OnSuccessfully() => SetLock(isLocked: false);
    }
}
