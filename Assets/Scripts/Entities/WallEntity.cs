using UnityEngine;
using PolarBond.Core;

namespace PolarBond.Entities
{
    public class WallEntity : GridEntity
    {
        public WallEntity(Vector2Int position) : base(position, EntityType.Wall)
        {
        }
    }
}
