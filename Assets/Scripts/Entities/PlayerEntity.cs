using UnityEngine;
using PolarBond.Core;

namespace PolarBond.Entities
{
    public class PlayerEntity : GridEntity
    {
        public PlayerEntity(Vector2Int position) : base(position, EntityType.Player)
        {
        }
    }
}
