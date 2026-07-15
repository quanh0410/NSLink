using UnityEngine;
using PolarBond.Core;

namespace PolarBond.Entities
{
    public abstract class GridEntity
    {
        public System.Action<Vector2Int> OnPositionChanged;

        private Vector2Int _position;
        public Vector2Int Position 
        { 
            get => _position; 
            set 
            {
                if (_position != value)
                {
                    _position = value;
                    OnPositionChanged?.Invoke(_position);
                }
            }
        }
        
        public EntityType Type { get; protected set; }

        public GridEntity(Vector2Int position, EntityType type)
        {
            _position = position;
            Type = type;
        } 
    }
}
