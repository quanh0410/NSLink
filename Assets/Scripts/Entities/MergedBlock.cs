using System.Collections.Generic;
using UnityEngine;
using PolarBond.Core;

namespace PolarBond.Entities
{
    public class MergedBlock
    {
        public List<MagnetEntity> Magnets { get; private set; }
        
        public int Size => Magnets.Count;

        public MergedBlock()
        {
            Magnets = new List<MagnetEntity>();
        }

        private static Stack<MergedBlock> pool = new Stack<MergedBlock>();

        public static MergedBlock Get()
        {
            if (pool.Count > 0)
            {
                var block = pool.Pop();
                block.Magnets.Clear();
                return block;
            }
            return new MergedBlock();
        }

        public static MergedBlock Get(MagnetEntity singleMagnet)
        {
            var block = Get();
            block.AddMagnet(singleMagnet);
            return block;
        }

        public static void ReturnToPool(MergedBlock block)
        {
            block.Magnets.Clear();
            pool.Push(block);
        }

        // Deprecated: Use Get(singleMagnet) instead to utilize pooling
        public MergedBlock(MagnetEntity singleMagnet)
        {
            Magnets = new List<MagnetEntity> { singleMagnet };
            singleMagnet.CurrentBlock = this;
        }

        public void AddMagnet(MagnetEntity magnet)
        {
            if (!Magnets.Contains(magnet))
            {
                Magnets.Add(magnet);
                magnet.CurrentBlock = this;
            }
        }

        public void MergeWith(MergedBlock otherBlock)
        {
            if (otherBlock == this) return;

            foreach (var magnet in otherBlock.Magnets)
            {
                AddMagnet(magnet);
            }
            otherBlock.Magnets.Clear();
        }

        public void RemoveMagnet(MagnetEntity magnet)
        {
            if (Magnets.Contains(magnet))
            {
                Magnets.Remove(magnet);
                magnet.CurrentBlock = null;
            }
        }

        // Checks if moving this block in a direction would intersect with a given position
        public bool ContainsPosition(Vector2Int pos)
        {
            foreach (var magnet in Magnets)
            {
                if (magnet.Position == pos) return true;
            }
            return false;
        }

        public void Move(Vector2Int offset)
        {
            foreach (var magnet in Magnets)
            {
                magnet.Position += offset;
            }
        }
    }
}
