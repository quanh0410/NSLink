using UnityEngine;
using PolarBond.Core;

namespace PolarBond.Entities
{
    public abstract class GridEntity
    {
        private Vector2Int _position;
        public Vector2Int Position 
        { 
            get => _position; 
            set 
            {
                _position = value;
                if (VisualTransform != null)
                {
                    var view = VisualTransform.GetComponent<PolarBond.Views.EntityView>();
                    if (view != null) 
                    {
                        view.WakeUp();
                        UnityEngine.Debug.Log($"[GridEntity] Position changed to {value}. Waking up {VisualTransform.name}");
                    }
                }
                else
                {
                    UnityEngine.Debug.LogWarning($"[GridEntity] VisualTransform is NULL for Entity Type {Type} at {value}!");
                }
            }
        }
        
        public EntityType Type { get; protected set; }

        public GridEntity(Vector2Int position, EntityType type)
        {
            _position = position;
            Type = type;
        }

        // Potential visual linkage if tying logic to MonoBehaviour
        public Transform VisualTransform { get; set; } 
    }
}
