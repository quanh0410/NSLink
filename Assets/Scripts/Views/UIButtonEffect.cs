using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace PolarBond.Views
{
    public class UIButtonEffect : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
    {
        [SerializeField] private float pressScale = 0.9f;
        [SerializeField] private float transitionDuration = 0.15f;
        
        private Vector3 originalScale;
        private Color originalColor = Color.white;
        
        private Coroutine scaleCoroutine;
        private Image targetImage;
        private Color tintColor = new Color(0.85f, 0.85f, 0.85f, 1f);

        private void Awake()
        {
            originalScale = transform.localScale;
            targetImage = GetComponent<Image>();
            if (targetImage != null)
            {
                originalColor = targetImage.color;
            }
        }
        
        private void OnDisable()
        {
            if (scaleCoroutine != null)
            {
                UITweener.StopTween(scaleCoroutine);
                scaleCoroutine = null;
            }
            transform.localScale = originalScale;
            if (targetImage != null) targetImage.color = originalColor;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (scaleCoroutine != null) UITweener.StopTween(scaleCoroutine);
            scaleCoroutine = UITweener.ScaleTo(transform, originalScale * pressScale, transitionDuration);
            
            if (targetImage != null) targetImage.color = originalColor * tintColor;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            ResetState();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            ResetState();
        }

        private void ResetState()
        {
            if (scaleCoroutine != null) UITweener.StopTween(scaleCoroutine);
            scaleCoroutine = UITweener.ScaleTo(transform, originalScale, transitionDuration);
            
            if (targetImage != null) targetImage.color = originalColor;
        }
    }
}
