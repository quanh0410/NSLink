using System.Collections.Generic;
using UnityEngine;
using PolarBond.Core;
using PolarBond.Entities;

namespace PolarBond.Managers
{
    public class GridManager
    {
        private Dictionary<Vector2Int, GridEntity> grid;
        private Dictionary<Vector2Int, TargetType> targetTiles;
        private Dictionary<Vector2Int, SpecialTileType> specialTiles;

        public GridManager()
        {
            grid = new Dictionary<Vector2Int, GridEntity>();
            targetTiles = new Dictionary<Vector2Int, TargetType>();
            specialTiles = new Dictionary<Vector2Int, SpecialTileType>();
        }

        public void AddEntity(GridEntity entity)
        {
            if (!grid.ContainsKey(entity.Position))
            {
                grid[entity.Position] = entity;
            }
            else
            {
                Debug.LogWarning($"[GridManager] Thêm Entity thất bại! Ô {entity.Position} đã có vật thể {grid[entity.Position].Type}.");
            }
        }

        public void RemoveEntity(Vector2Int position)
        {
            if (grid.ContainsKey(position))
            {
                grid.Remove(position);
            }
        }

        public GridEntity GetEntityAt(Vector2Int position)
        {
            grid.TryGetValue(position, out GridEntity entity);
            return entity;
        }

        public bool IsCellEmpty(Vector2Int position)
        {
            return !grid.ContainsKey(position);
        }

        public void MoveEntity(Vector2Int from, Vector2Int to)
        {
            if (grid.TryGetValue(from, out GridEntity entity))
            {
                grid.Remove(from);
                entity.Position = to;
                grid[to] = entity;
            }
            else
            {
                Debug.LogWarning($"[GridManager] Lỗi di chuyển: Không tìm thấy vật thể nào ở tọa độ {from}");
            }
        }



        public void AddTargetTile(Vector2Int position, TargetType targetType)
        {
            targetTiles[position] = targetType;
        }

        public Dictionary<Vector2Int, TargetType> GetTargetTiles()
        {
            return targetTiles;
        }

        public void AddSpecialTile(Vector2Int position, SpecialTileType specialType)
        {
            specialTiles[position] = specialType;
        }

        public SpecialTileType GetSpecialTile(Vector2Int position)
        {
            return specialTiles.TryGetValue(position, out var type) ? type : SpecialTileType.None;
        }

        public IEnumerable<GridEntity> GetAllEntities()
        {
            return grid.Values;
        }
        
        public void ClearBoard()
        {
            grid.Clear();
            targetTiles.Clear();
            specialTiles.Clear();
        }
    }
}
