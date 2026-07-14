using System.Collections.Generic;
using UnityEngine;
using PolarBond.Core;

namespace PolarBond.Logic
{
    [System.Serializable]
    public struct MagnetSnapshotData
    {
        public string EntityId;
        public Vector2Int Position;
        public MagneticPolarity Polarity;
        public int BlockId;

        public MagnetSnapshotData(string entityId, Vector2Int position, MagneticPolarity polarity, int blockId)
        {
            EntityId = entityId;
            Position = position;
            Polarity = polarity;
            BlockId = blockId;
        }
    }

    [System.Serializable]
    public class GameSnapshot
    {
        public Vector2Int PlayerPosition;
        public List<MagnetSnapshotData> Magnets;

        public GameSnapshot()
        {
            Magnets = new List<MagnetSnapshotData>();
        }
    }
}
