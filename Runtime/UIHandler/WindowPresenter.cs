using UnityEngine;

namespace Agava.Wink
{
    public abstract class WindowPresenter : MonoBehaviour
    {
        public bool HasOpened { get; private set; } = false;

        public abstract void Enable();
        public abstract void Disable();

        protected void EnableCanvasGroup(CanvasGroup canvas)
        {
            canvas.alpha = 1;
            canvas.interactable = true;
            canvas.blocksRaycasts = true;
            HasOpened = true;
        }

        protected void DisableCanvasGroup(CanvasGroup canvas)
        {
            canvas.alpha = 0;
            canvas.interactable = false;
            canvas.blocksRaycasts = false;
            HasOpened = false;
        }
    }
}
