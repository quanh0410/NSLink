using System.Collections.Generic;
using UnityEngine;
using PolarBond.Entities;
using PolarBond.Managers;

namespace PolarBond.Logic
{
    public class MagneticAttractionSystem
    {
        private GridManager gridManager;

        public MagneticAttractionSystem(GridManager gridManager)
        {
            this.gridManager = gridManager;
        }

        public void ProcessAttraction(List<MergedBlock> allBlocks)
        {
            int originalBlockCount = allBlocks.Count;

            foreach (var block in allBlocks)
            {
                MergedBlock.ReturnToPool(block);
            }
            allBlocks.Clear();
            foreach (var entity in gridManager.GetAllEntities())
            {
                if (entity is MagnetEntity mag)
                {
                    allBlocks.Add(MergedBlock.Get(mag));
                }
            }
            
            bool mergedSomething = true;
            while (mergedSomething)
            {
                mergedSomething = false;
                for (int i = 0; i < allBlocks.Count; i++)
                {
                    for (int j = i + 1; j < allBlocks.Count; j++)
                    {
                        if (CheckAndMerge(allBlocks[i], allBlocks[j]))
                        {
                            MergedBlock.ReturnToPool(allBlocks[j]);
                            allBlocks.RemoveAt(j);
                            mergedSomething = true;
                            break;
                        }
                    }
                    if (mergedSomething) break;
                }
            }

            if (allBlocks.Count < originalBlockCount)
            {
                if (AudioManager.Instance != null) AudioManager.Instance.PlayLinkSound();
            }
        }

        private bool CheckAndMerge(MergedBlock b1, MergedBlock b2)
        {
            foreach (var m1 in b1.Magnets)
            {
                foreach (var m2 in b2.Magnets)
                {
                    if (IsAdjacent(m1.Position, m2.Position) && m1.Polarity != m2.Polarity)
                    {
                        b1.MergeWith(b2);
                        return true;
                    }
                }
            }
            return false;
        }

        private bool IsAdjacent(Vector2Int p1, Vector2Int p2)
        {
            int dx = Mathf.Abs(p1.x - p2.x);
            int dy = Mathf.Abs(p1.y - p2.y);
            return (dx == 1 && dy == 0) || (dx == 0 && dy == 1);
        }
    }
}
