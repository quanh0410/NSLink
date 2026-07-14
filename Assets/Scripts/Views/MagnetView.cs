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

                GameObject prefabToInstantiate = Polarity == MagneticPolarity.North ? northPrefab : southPrefab;
                currentVisualInstance = Instantiate(prefabToInstantiate, transform);
                currentVisualInstance.transform.localPosition = Vector3.zero;
            }
        }
    }
}
