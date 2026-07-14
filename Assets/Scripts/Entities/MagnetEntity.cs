using UnityEngine;
using PolarBond.Core;

namespace PolarBond.Entities
{
    public class MagnetEntity : GridEntity
    {
        public string EntityId { get; private set; }
        public MagneticPolarity Polarity { get; private set; }
        public MergedBlock CurrentBlock { get; set; }
        public bool WasOnReverseTile { get; set; }

        public MagnetEntity(Vector2Int position, MagneticPolarity polarity) 
            : base(position, EntityType.Magnet)
        {
            EntityId = System.Guid.NewGuid().ToString();
            Polarity = polarity;
            WasOnReverseTile = false;
        }

        public void ReversePolarity()
        {
            if (Polarity == MagneticPolarity.North)
                Polarity = MagneticPolarity.South;
            else if (Polarity == MagneticPolarity.South)
                Polarity = MagneticPolarity.North;

            if (VisualTransform != null)
            {
                var view = VisualTransform.GetComponent<PolarBond.Views.MagnetView>();
                if (view != null)
                {
                    view.UpdatePolarityVisuals();
                }
            }
        }
    }
}
