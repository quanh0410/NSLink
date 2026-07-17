using UnityEngine;
using PolarBond.Core;
using PolarBond.Entities;

namespace PolarBond.Views
{
    public class MagnetView : EntityView
    {
        [Header("Magnet Settings")]
        public MagneticPolarity Polarity;

        [Header("Visual Prefabs")]
        public GameObject northPrefab;
        public GameObject southPrefab;
        
        [Header("Sprites (Glow Effect)")]
        public Sprite northGlowSprite;
        public Sprite southGlowSprite;
        
        private GameObject currentVisualInstance;
        private SpriteRenderer cachedRenderer;
        private Sprite originalSprite;

        public override void Initialize(GridEntity entity)
        {
            base.Initialize(entity);
            if (entity is MagnetEntity magnetLogic)
            {
                magnetLogic.OnPolarityChanged += OnPolarityChangedHandler;
                magnetLogic.OnTargetStateChanged += OnTargetStateChangedHandler;
            }
            UpdatePolarityVisuals();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (LogicEntity is MagnetEntity magnetLogic)
            {
                magnetLogic.OnPolarityChanged -= OnPolarityChangedHandler;
                magnetLogic.OnTargetStateChanged -= OnTargetStateChangedHandler;
            }
        }

        private void OnPolarityChangedHandler(MagneticPolarity newPolarity)
        {
            UpdatePolarityVisuals();
        }

        private void OnTargetStateChangedHandler(bool isOnTarget)
        {
            UpdateGlowState(isOnTarget);
        }

        public void UpdatePolarityVisuals()
        {
            if (LogicEntity is MagnetEntity ml)
            {
                Polarity = ml.Polarity;
            }

            if (northPrefab != null && southPrefab != null)
            {
                if (currentVisualInstance != null)
                {
                    Destroy(currentVisualInstance);
                }
                else
                {
                    // Clean up any manually placed editor visuals to prevent overlapping duplicates
                    for (int i = transform.childCount - 1; i >= 0; i--)
                    {
                        Destroy(transform.GetChild(i).gameObject);
                    }
                }

                GameObject prefabToInstantiate = Polarity == MagneticPolarity.North ? northPrefab : southPrefab;
                currentVisualInstance = Instantiate(prefabToInstantiate, transform);
                currentVisualInstance.transform.localPosition = Vector3.zero;

                // Strip physics and logic components from the clone so it acts purely as a visual mesh
                foreach (var comp in currentVisualInstance.GetComponentsInChildren<Collider2D>()) Destroy(comp);
                foreach (var comp in currentVisualInstance.GetComponentsInChildren<Rigidbody2D>()) Destroy(comp);
                foreach (var comp in currentVisualInstance.GetComponentsInChildren<EntityView>()) Destroy(comp);

                // Cache sprite components for glow effect from the instantiated prefab
                cachedRenderer = currentVisualInstance.GetComponentInChildren<SpriteRenderer>();

                // Disable root SpriteRenderer to prevent it from overlapping and hiding the clone's visual
                SpriteRenderer rootSr = GetComponent<SpriteRenderer>();
                if (rootSr != null)
                {
                    rootSr.enabled = false;
                }
            }
            else
            {
                // Fallback: If not using prefabs, try to get SpriteRenderer on the root object
                cachedRenderer = GetComponent<SpriteRenderer>();
            }

            if (cachedRenderer != null)
            {
                originalSprite = cachedRenderer.sprite;
            }

            if (LogicEntity is MagnetEntity magnetEntityLogic)
            {
                UpdateGlowState(magnetEntityLogic.IsOnTarget);
            }
        }

        private void UpdateGlowState(bool isOnTarget)
        {
            if (cachedRenderer == null)
            {
                Debug.LogWarning($"[MagnetView] UpdateGlowState called but cachedRenderer is null on {gameObject.name}");
                return;
            }

            Debug.Log($"[MagnetView] UpdateGlowState on {gameObject.name} (Polarity: {Polarity}), isOnTarget: {isOnTarget}");

            if (isOnTarget)
            {
                if (Polarity == MagneticPolarity.North && northGlowSprite != null)
                {
                    Debug.Log($"[MagnetView] Setting North Glow Sprite on {gameObject.name}");
                    cachedRenderer.sprite = northGlowSprite;
                }
                else if (Polarity == MagneticPolarity.South && southGlowSprite != null)
                {
                    Debug.Log($"[MagnetView] Setting South Glow Sprite on {gameObject.name}");
                    cachedRenderer.sprite = southGlowSprite;
                }
                else
                {
                    Debug.LogWarning($"[MagnetView] isOnTarget is true but Glow Sprite is missing for {Polarity} on {gameObject.name}");
                }
            }
            else
            {
                if (originalSprite != null)
                {
                    cachedRenderer.sprite = originalSprite;
                }
            }
        }
    }
}
