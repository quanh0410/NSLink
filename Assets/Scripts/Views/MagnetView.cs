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
        private GameObject currentVisualInstance;

        public override void Initialize(GridEntity entity)
        {
            base.Initialize(entity);
            if (entity is MagnetEntity magnetLogic)
            {
                magnetLogic.OnPolarityChanged += OnPolarityChangedHandler;
            }
            UpdatePolarityVisuals();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (LogicEntity is MagnetEntity magnetLogic)
            {
                magnetLogic.OnPolarityChanged -= OnPolarityChangedHandler;
            }
        }

        private void OnPolarityChangedHandler(MagneticPolarity newPolarity)
        {
            UpdatePolarityVisuals();
        }

        public void UpdatePolarityVisuals()
        {
            if (LogicEntity is MagnetEntity magnetLogic)
            {
                Polarity = magnetLogic.Polarity;
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
            }
        }
    }
}
