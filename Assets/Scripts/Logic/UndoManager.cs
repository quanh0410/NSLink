using System.Collections.Generic;
using UnityEngine;
using PolarBond.Core;
using PolarBond.Entities;
using PolarBond.Managers;

namespace PolarBond.Logic
{
    public class UndoManager
    {
        public event System.Action OnUndoStateChanged;

        private Stack<GameSnapshot> undoStack;
        private Stack<GameSnapshot> snapshotPool;
        private GridManager gridManager;

        // Caches
        private List<MagnetSnapshotData> currentMagnetsCache = new List<MagnetSnapshotData>();
        private List<MagnetEntity> pooledMagnetsCache = new List<MagnetEntity>();
        private Dictionary<int, MergedBlock> blockMapCache = new Dictionary<int, MergedBlock>();

        public UndoManager(GridManager gridManager)
        {
            this.gridManager = gridManager;
            undoStack = new Stack<GameSnapshot>();
            snapshotPool = new Stack<GameSnapshot>();
        }

        private GameSnapshot GetSnapshot()
        {
            if (snapshotPool.Count > 0)
            {
                return snapshotPool.Pop();
            }
            return new GameSnapshot();
        }

        private void ReturnSnapshot(GameSnapshot snapshot)
        {
            snapshot.Magnets.Clear();
            snapshotPool.Push(snapshot);
        }

        public void SaveState(PlayerEntity player, List<MergedBlock> allBlocks)
        {
            GameSnapshot snapshot = GetSnapshot();
            snapshot.PlayerPosition = player.Position;

            int blockIdCounter = 0;
            foreach (var block in allBlocks)
            {
                foreach (var magnet in block.Magnets)
                {
                    snapshot.Magnets.Add(new MagnetSnapshotData(
                        magnet.EntityId,
                        magnet.Position, 
                        magnet.Polarity, 
                        blockIdCounter
                    ));
                }
                blockIdCounter++;
            }

            undoStack.Push(snapshot);
            OnUndoStateChanged?.Invoke();
            Debug.Log($"[UndoManager] Saved state. Stack count is now {undoStack.Count}");
        }

        public bool CanUndo()
        {
            return undoStack.Count > 0;
        }

        public bool Undo(PlayerEntity player, List<MergedBlock> allBlocks)
        {
            if (undoStack.Count == 0) return false;

            // Extract current magnet state to compare
            currentMagnetsCache.Clear();
            List<MagnetSnapshotData> currentMagnets = currentMagnetsCache;
            int tempId = 0;
            foreach (var block in allBlocks)
            {
                foreach (var mag in block.Magnets)
                {
                    currentMagnets.Add(new MagnetSnapshotData(mag.EntityId, mag.Position, mag.Polarity, tempId));
                }
                tempId++;
            }

            GameSnapshot targetSnapshot = undoStack.Pop();

            if (targetSnapshot == null) return false;

            // Pool existing magnets
            pooledMagnetsCache.Clear();
            List<MagnetEntity> pooledMagnets = pooledMagnetsCache;
            foreach (var block in allBlocks)
            {
                pooledMagnets.AddRange(block.Magnets);
                foreach (var magnet in block.Magnets)
                {
                    gridManager.RemoveEntity(magnet.Position);
                }
            }
            allBlocks.Clear();

            // Reconstruct state
            gridManager.MoveEntity(player.Position, targetSnapshot.PlayerPosition);

            blockMapCache.Clear();
            Dictionary<int, MergedBlock> blockMap = blockMapCache;

            foreach (var magnetData in targetSnapshot.Magnets)
            {
                // Find a matching magnet from the pool
                MagnetEntity restoredMagnet = null;
                for (int i = 0; i < pooledMagnets.Count; i++)
                {
                    if (pooledMagnets[i].EntityId == magnetData.EntityId)
                    {
                        restoredMagnet = pooledMagnets[i];
                        pooledMagnets.RemoveAt(i);
                        break;
                    }
                }

                if (restoredMagnet != null)
                {
                    restoredMagnet.Position = magnetData.Position;
                    gridManager.AddEntity(restoredMagnet);

                    if (!blockMap.ContainsKey(magnetData.BlockId))
                    {
                        MergedBlock newBlock = MergedBlock.Get(restoredMagnet);
                        blockMap[magnetData.BlockId] = newBlock;
                        allBlocks.Add(newBlock);
                    }
                    else
                    {
                        blockMap[magnetData.BlockId].AddMagnet(restoredMagnet);
                    }
                }
            }

            ReturnSnapshot(targetSnapshot);

            OnUndoStateChanged?.Invoke();

            return true;
        }

        private bool AreMagnetsDifferent(List<MagnetSnapshotData> a, List<MagnetSnapshotData> b)
        {
            if (a.Count != b.Count) return true;

            foreach (var magA in a)
            {
                bool found = false;
                foreach (var magB in b)
                {
                    if (magB.EntityId == magA.EntityId && magB.Position == magA.Position && magB.Polarity == magA.Polarity)
                    {
                        found = true;
                        break;
                    }
                }
                if (!found) return true;
            }
            return false;
        }

        public void DropLastState()
        {
            if (undoStack.Count > 0)
            {
                ReturnSnapshot(undoStack.Pop());
                OnUndoStateChanged?.Invoke();
            }
        }

        public void Clear()
        {
            bool changed = undoStack.Count > 0;
            while (undoStack.Count > 0)
            {
                ReturnSnapshot(undoStack.Pop());
            }
            if (changed) OnUndoStateChanged?.Invoke();
        }
    }
}
