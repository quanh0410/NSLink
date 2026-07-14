using UnityEngine;
using UnityEngine.EventSystems;

namespace PolarBond.Views
{
    public class UIButtonEffect : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        [SerializeField] private float pressScale = 0.9f;
        [SerializeField] private float transitionSpeed = 10f;
        
        private Vector3 originalScale;
        private Vector3 targetScale;

        private void Awake()
        {
            originalScale = transform.localScale;
            targetScale = originalScale;
            this.enabled = false; // Ngủ đông khi không có thay đổi
        }

        private void Update()
        {
            if (Vector3.Distance(transform.localScale, targetScale) > 0.001f)
            {
                transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * transitionSpeed);
            }
            else
            {
                if (transform.localScale != targetScale)
                {
                    transform.localScale = targetScale;
                }
                this.enabled = false; // Đã đạt mục tiêu, tắt Update
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            targetScale = originalScale * pressScale;
            this.enabled = true; // Bật Update để chạy hiệu ứng
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            targetScale = originalScale;
            this.enabled = true; // Bật Update để chạy hiệu ứng
        }
    }
}
