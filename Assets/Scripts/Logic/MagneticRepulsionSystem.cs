using System.Collections.Generic;
using UnityEngine;
using PolarBond.Entities;
using PolarBond.Managers;
using PolarBond.Core;

namespace PolarBond.Logic
{
    public class MagneticRepulsionSystem
    {
        private GridManager gridManager;
        private MagneticPhysicsEngine engine;

        // Caches local to Repulsion
        private Dictionary<MagnetEntity, Vector2Int> netForcesCache = new Dictionary<MagnetEntity, Vector2Int>();
        private List<GridEntity> entitiesCache = new List<GridEntity>();
        private List<(MagnetEntity mag, Direction dir)> repulsionsCache = new List<(MagnetEntity, Direction)>();
        private HashSet<GridEntity> alreadyMovedCache = new HashSet<GridEntity>();
        private HashSet<GridEntity> visitedCache = new HashSet<GridEntity>();
        private HashSet<GridEntity> emptyTentativeCache = new HashSet<GridEntity>();

        public MagneticRepulsionSystem(GridManager gridManager, MagneticPhysicsEngine engine)
        {
            this.gridManager = gridManager;
            this.engine = engine;
        }

        private int GetEffectiveMass(MagnetEntity mag)
        {
            List<MagnetEntity> magnets = mag.CurrentBlock != null ? mag.CurrentBlock.Magnets : new List<MagnetEntity> { mag };
            int mass = magnets.Count;
            
            foreach (var m in magnets)
            {
                Vector2Int[] adj = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
                foreach (var d in adj)
                {
                    GridEntity entity = gridManager.GetEntityAt(m.Position + d);
                    if (entity != null && entity.Type == EntityType.Player)
                    {
                        return mass + 3;
                    }
                }
            }
            return mass;
        }

        private void AddForce(Dictionary<MagnetEntity, Vector2Int> forces, MagnetEntity mag, Vector2Int force)
        {
            if (forces.ContainsKey(mag))
                forces[mag] += force;
            else
                forces[mag] = force;
        }

        public bool ApplyRepulsion(List<MergedBlock> allBlocks)
        {
            netForcesCache.Clear();
            Dictionary<MagnetEntity, Vector2Int> netForces = netForcesCache;
            
            entitiesCache.Clear();
            entitiesCache.AddRange(gridManager.GetAllEntities());
            var entities = entitiesCache;
            
            for (int i = 0; i < entities.Count; i++)
            {
                if (entities[i] is MagnetEntity m1)
                {
                    for (int j = i + 1; j < entities.Count; j++)
                    {
                        if (entities[j] is MagnetEntity m2)
                        {
                            if (m1.Polarity == m2.Polarity && engine.IsAdjacent(m1.Position, m2.Position))
                            {
                                int size1 = GetEffectiveMass(m1);
                                int size2 = GetEffectiveMass(m2);
                                
                                Vector2Int diff = m2.Position - m1.Position;
                                Vector2Int forceOnM2 = diff;
                                Vector2Int forceOnM1 = -diff;
                                
                                if (size1 < size2)
                                {
                                    visitedCache.Clear();
                                    emptyTentativeCache.Clear();
                                    if (engine.CanRepelledMagnetMove(m1, forceOnM1, size2, emptyTentativeCache, visitedCache))
                                        AddForce(netForces, m1, forceOnM1);
                                    else
                                        AddForce(netForces, m2, forceOnM2);
                                }
                                else if (size2 < size1)
                                {
                                    visitedCache.Clear();
                                    emptyTentativeCache.Clear();
                                    if (engine.CanRepelledMagnetMove(m2, forceOnM2, size1, emptyTentativeCache, visitedCache))
                                        AddForce(netForces, m2, forceOnM2);
                                    else
                                        AddForce(netForces, m1, forceOnM1);
                                }
                                else
                                {
                                    visitedCache.Clear();
                                    emptyTentativeCache.Clear();
                                    bool m1CanMove = engine.CanRepelledMagnetMove(m1, forceOnM1, size2, emptyTentativeCache, visitedCache);
                                    
                                    visitedCache.Clear();
                                    bool m2CanMove = engine.CanRepelledMagnetMove(m2, forceOnM2, size1, emptyTentativeCache, visitedCache);
                                    
                                    if (m1CanMove) AddForce(netForces, m1, forceOnM1);
                                    if (m2CanMove) AddForce(netForces, m2, forceOnM2);
                                }
                            }
                        }
                    }
                }
            }
            
            repulsionsCache.Clear();
            List<(MagnetEntity mag, Direction dir)> repulsions = repulsionsCache;
            foreach (var kvp in netForces)
            {
                if (kvp.Value == Vector2Int.zero) continue;
                
                if (kvp.Value.x != 0 && kvp.Value.y == 0)
                {
                    repulsions.Add((kvp.Key, kvp.Value.x > 0 ? Direction.Right : Direction.Left));
                }
                else if (kvp.Value.y != 0 && kvp.Value.x == 0)
                {
                    repulsions.Add((kvp.Key, kvp.Value.y > 0 ? Direction.Up : Direction.Down));
                }
            }
            
            bool anyMoved = false;
            alreadyMovedCache.Clear();
            HashSet<GridEntity> alreadyMoved = alreadyMovedCache;
            foreach (var rep in repulsions)
            {
                if (alreadyMoved.Contains(rep.mag)) continue;
                if (engine.TryGetMovingEntities(rep.mag, rep.dir, out HashSet<GridEntity> movingEntities))
                {
                    anyMoved = true;
                    Vector2Int offset = rep.dir.ToVector2Int();
                    foreach (var e in movingEntities) gridManager.RemoveEntity(e.Position);
                    foreach (var e in movingEntities) 
                    { 
                        e.Position += offset;
                        gridManager.AddEntity(e);
                        alreadyMoved.Add(e);
                    }
                }
            }

            return anyMoved;
        }
    }
}
