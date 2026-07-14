using UnityEngine;
using PolarBond.Entities;

namespace PolarBond.Views
{
    public abstract class EntityView : MonoBehaviour
    {
        [Header("Animation Settings")]
        public float moveSpeed = 10f;
        
        [Header("Visuals")]
        public Vector3 visualOffset = new Vector3(0.5f, 0.5f, 0f);
        
        public GridEntity LogicEntity { get; protected set; }
        private Vector3 targetPosition;

        public virtual void Initialize(GridEntity entity)
        {
            LogicEntity = entity;
            if (entity != null)
            {
                entity.VisualTransform = this.transform;
                Debug.Log($"[EntityView] Initialized {gameObject.name} and set VisualTransform.");
            }
            // Snap to grid initially with offset
            transform.position = new Vector3(entity.Position.x, entity.Position.y, 0) + visualOffset;
            targetPosition = transform.position;
        }

        protected virtual void Update()
        {
            if (LogicEntity != null)
            {
                // Logic entity's position is the source of truth
                targetPosition = new Vector3(LogicEntity.Position.x, LogicEntity.Position.y, 0) + visualOffset;
                
                // Smoothly lerp towards the target position
                if (Vector3.Distance(transform.position, targetPosition) > 0.001f)
                {
                    transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
                }
                else
                {
                    if (transform.position != targetPosition)
                    {
                        transform.position = targetPosition;
                    }
                    this.enabled = false; // Ngủ đông khi đã đến đích
                }
            }
        }

        public void WakeUp()
        {
            this.enabled = true;
        }
        
        // Editor utility to help snap when designing levels
        private void OnValidate()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying && gameObject.scene.IsValid())
            {
                // Dùng delayCall để tránh cảnh báo của Unity khi sửa Transform trong OnValidate
                UnityEditor.EditorApplication.delayCall += () =>
                {
                    if (this == null || transform == null) return;

                    // Sử dụng localPosition để an toàn cho Prefab
                    transform.localPosition = new Vector3(
                        Mathf.Floor(transform.localPosition.x) + visualOffset.x,
                        Mathf.Floor(transform.localPosition.y) + visualOffset.y,
                        0
                    );
                };
            }
#endif
        }
    }
}
