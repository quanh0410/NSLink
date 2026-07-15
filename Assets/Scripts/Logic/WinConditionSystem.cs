using UnityEngine;
using PolarBond.Entities;
using PolarBond.Managers;
using PolarBond.Core;

namespace PolarBond.Logic
{
    public class WinConditionSystem
    {
        private GridManager gridManager;

        public WinConditionSystem(GridManager gridManager)
        {
            this.gridManager = gridManager;
        }

        public bool Check()
        {
            var targets = gridManager.GetTargetTiles();
            
#if UNITY_EDITOR
            Debug.Log($"[CheckWinCondition] Đang kiểm tra... Tổng số ô đích trên bản đồ: {targets.Count}");
#endif
            if (targets.Count == 0) return false;

            int satisfiedCount = 0;
            foreach (var kvp in targets)
            {
                Vector2Int pos = kvp.Key;
                TargetType targetType = kvp.Value;

                GridEntity entity = gridManager.GetEntityAt(pos);
                if (entity == null)
                {
#if UNITY_EDITOR
                    Debug.Log($"[CheckWinCondition] Ô đích tại {pos} ĐANG TRỐNG (Không có gì ở đây).");
#endif
                    return false;
                }
                
                if (entity.Type != EntityType.Magnet)
                {
#if UNITY_EDITOR
                    Debug.Log($"[CheckWinCondition] Ô đích tại {pos} bị chiếm bởi {entity.Type} (Không phải Nam Châm).");
#endif
                    return false;
                }

                MagnetEntity magnet = (MagnetEntity)entity;
                if (targetType == TargetType.NorthTarget && magnet.Polarity != MagneticPolarity.North)
                {
#if UNITY_EDITOR
                    Debug.Log($"[CheckWinCondition] Ô đích tại {pos} yêu cầu cực North (Đỏ), nhưng nam châm là {magnet.Polarity}.");
#endif
                    return false;
                }
                
                if (targetType == TargetType.SouthTarget && magnet.Polarity != MagneticPolarity.South)
                {
#if UNITY_EDITOR
                    Debug.Log($"[CheckWinCondition] Ô đích tại {pos} yêu cầu cực South (Xanh), nhưng nam châm là {magnet.Polarity}.");
#endif
                    return false;
                }
                
                satisfiedCount++;
            }

#if UNITY_EDITOR
            Debug.Log($"[CheckWinCondition] THÀNH CÔNG! Đã lấp đầy {satisfiedCount}/{targets.Count} ô đích.");
#endif
            return true;
        }
    }
}
