using System.Collections.Generic;
using UnityEngine;
using PolarBond.Entities;
using PolarBond.Managers;
using PolarBond.Core;

namespace PolarBond.Logic
{
    public class MechanicalMovementSystem
    {
        private GridManager gridManager;
        private MagneticPhysicsEngine engine;

        public MechanicalMovementSystem(GridManager gridManager, MagneticPhysicsEngine engine)
        {
            this.gridManager = gridManager;
            this.engine = engine;
        }

        public bool TryMovePlayer(PlayerEntity player, Direction dir, List<MergedBlock> allBlocks)
        {
            if (engine.TryGetMovingEntities(player, dir, out HashSet<GridEntity> movingEntities))
            {
                Vector2Int offset = dir.ToVector2Int();
                
                foreach (var e in movingEntities)
                {
                    gridManager.RemoveEntity(e.Position);
                }
                foreach (var e in movingEntities)
                {
                    e.Position += offset;
                    gridManager.AddEntity(e);
                }
                
                return true;
            }
            return false;
        }
    }
}
