using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using PolarBond.Core;
using PolarBond.Entities;
using PolarBond.Logic;
using PolarBond.Views;

namespace PolarBond.Managers
{
    [Serializable]
    public struct TargetTileMapping
    {
        public TileBase tile;
        public TargetType targetType;
    }

    [Serializable]
    public struct SpecialTileMapping
    {
        public TileBase tile;
        public SpecialTileType specialType;
    }

    public class TilemapLevelBuilder : MonoBehaviour
    {
        [Header("System References")]
        public GameLoopController GameController;

        [Header("Tile Mappings")]
        public List<TargetTileMapping> targetMappings;
        public List<SpecialTileMapping> specialMappings;

        [Header("Prefabs to Spawn (Optional visual representation)")]
        public GameObject wallPrefab;
        public GameObject targetPrefabUniversal;
        public GameObject targetPrefabNorth;
        public GameObject targetPrefabSouth;
        public GameObject reverseTilePrefab;

        public void BuildLevel(Transform levelRoot)
        {
            if (GameController == null)
            {
                Debug.LogError("[TilemapLevelBuilder] GameController chưa được gán trên Inspector!");
                return;
            }

            // 1. Scan Dynamic Entities within the level prefab
            PlayerView playerView = levelRoot.GetComponentInChildren<PlayerView>();
            MagnetView[] magnetViews = levelRoot.GetComponentsInChildren<MagnetView>();

            PlayerEntity playerLogic = null;
            if (playerView != null)
            {
                playerLogic = new PlayerEntity(GetGridPos(playerView.transform.position));
                playerView.Initialize(playerLogic);
            }

            List<MagnetEntity> magnetsLogic = new List<MagnetEntity>();
            foreach (var mv in magnetViews)
            {
                MagnetEntity mLogic = new MagnetEntity(GetGridPos(mv.transform.position), mv.Polarity);
                mv.Initialize(mLogic);
                magnetsLogic.Add(mLogic);
            }

            // 2. Scan Static Tilemaps within the level prefab
            List<WallEntity> wallsLogic = new List<WallEntity>();
            Dictionary<Vector2Int, TargetType> scannedTargets = new Dictionary<Vector2Int, TargetType>();

            TargetView[] targetViews = levelRoot.GetComponentsInChildren<TargetView>();
            foreach (var tv in targetViews)
            {
                scannedTargets[GetGridPos(tv.transform.position)] = tv.Type;
            }

            // Find Tilemaps
            Tilemap wallsTilemap = null;
            Tilemap targetsTilemap = null;
            Tilemap specialsTilemap = null;
            
            Transform wallsTransform = levelRoot.Find("Walls") ?? levelRoot.Find("Tilemaps/Walls") ?? levelRoot.Find("Grid/Walls");
            if (wallsTransform != null) wallsTilemap = wallsTransform.GetComponent<Tilemap>();
            
            Transform targetsTransform = levelRoot.Find("Targets") ?? levelRoot.Find("Tilemaps/Targets") ?? levelRoot.Find("Grid/Targets");
            if (targetsTransform != null) targetsTilemap = targetsTransform.GetComponent<Tilemap>();

            Transform specialsTransform = levelRoot.Find("SpecialTiles") ?? levelRoot.Find("Tilemaps/SpecialTiles") ?? levelRoot.Find("Grid/SpecialTiles");
            if (specialsTransform != null) specialsTilemap = specialsTransform.GetComponent<Tilemap>();

            // Fallback: just search all child tilemaps if names don't match, picking by name heuristic
            if (wallsTilemap == null || targetsTilemap == null || specialsTilemap == null)
            {
                Tilemap[] allTilemaps = levelRoot.GetComponentsInChildren<Tilemap>();
                foreach(var tm in allTilemaps)
                {
                    if (wallsTilemap == null && tm.gameObject.name.ToLower().Contains("wall")) wallsTilemap = tm;
                    if (targetsTilemap == null && tm.gameObject.name.ToLower().Contains("target")) targetsTilemap = tm;
                    if (specialsTilemap == null && tm.gameObject.name.ToLower().Contains("special")) specialsTilemap = tm;
                }
            }

            // Scan Walls
            if (wallsTilemap != null)
            {
                BoundsInt bounds = wallsTilemap.cellBounds;
                foreach (var pos in bounds.allPositionsWithin)
                {
                    TileBase tile = wallsTilemap.GetTile(pos);
                    if (tile != null)
                    {
                        Vector2Int logicPos = new Vector2Int(pos.x, pos.y);
                        wallsLogic.Add(new WallEntity(logicPos));
                        
                        if (wallPrefab != null)
                        {
                            Instantiate(wallPrefab, wallsTilemap.CellToWorld(pos) + wallsTilemap.tileAnchor, Quaternion.identity, levelRoot);
                            wallsTilemap.SetTile(pos, null);
                        }
                    }
                }
            }

            // Scan Targets
            if (targetsTilemap != null)
            {
                BoundsInt bounds = targetsTilemap.cellBounds;
                foreach (var pos in bounds.allPositionsWithin)
                {
                    TileBase tile = targetsTilemap.GetTile(pos);
                    if (tile != null)
                    {
                        TargetType type = TargetType.Universal;
                        foreach (var mapping in targetMappings)
                        {
                            if (mapping.tile == tile)
                            {
                                type = mapping.targetType;
                                break;
                            }
                        }

                        Vector2Int logicPos = new Vector2Int(pos.x, pos.y);
                        scannedTargets[logicPos] = type;
                        
                        GameObject prefabToSpawn = type == TargetType.NorthTarget ? targetPrefabNorth : 
                                                   type == TargetType.SouthTarget ? targetPrefabSouth : targetPrefabUniversal;
                                                   
                        if (prefabToSpawn != null)
                        {
                            Instantiate(prefabToSpawn, targetsTilemap.CellToWorld(pos) + targetsTilemap.tileAnchor, Quaternion.identity, levelRoot);
                            targetsTilemap.SetTile(pos, null);
                        }
                    }
                }
            }

            // Scan Special Tiles
            Dictionary<Vector2Int, SpecialTileType> scannedSpecials = new Dictionary<Vector2Int, SpecialTileType>();
            if (specialsTilemap != null)
            {
                BoundsInt bounds = specialsTilemap.cellBounds;
                foreach (var pos in bounds.allPositionsWithin)
                {
                    TileBase tile = specialsTilemap.GetTile(pos);
                    if (tile != null)
                    {
                        SpecialTileType type = SpecialTileType.None;
                        if (specialMappings != null)
                        {
                            foreach (var mapping in specialMappings)
                            {
                                if (mapping.tile == tile)
                                {
                                    type = mapping.specialType;
                                    break;
                                }
                            }
                        }

                        Vector2Int logicPos = new Vector2Int(pos.x, pos.y);
                        if (type != SpecialTileType.None)
                        {
                            scannedSpecials[logicPos] = type;
                            
                            if (type == SpecialTileType.PolarityReverse && reverseTilePrefab != null)
                            {
                                Instantiate(reverseTilePrefab, specialsTilemap.CellToWorld(pos) + specialsTilemap.tileAnchor, Quaternion.identity, levelRoot);
                                specialsTilemap.SetTile(pos, null);
                            }
                        }
                    }
                }
            }

            // 3. Initialize Core Logic
            // Update Camera Fit
            CameraFitter fitter = Camera.main != null ? Camera.main.GetComponent<CameraFitter>() : null;
            if (fitter == null && Camera.main != null)
            {
                fitter = Camera.main.gameObject.AddComponent<CameraFitter>();
            }
            if (fitter != null)
            {
                UnityEngine.Tilemaps.Tilemap[] tilemaps = levelRoot.GetComponentsInChildren<UnityEngine.Tilemaps.Tilemap>(true);
                fitter.FitCameraToGrid(tilemaps);
            }
            
            GameController.InitializeLevel(playerLogic, magnetsLogic, wallsLogic);
            
            // Add targets
            foreach (var kvp in scannedTargets)
            {
                GameController.AddTargetTile(kvp.Key, kvp.Value);
            }

            // Add special tiles
            foreach (var kvp in scannedSpecials)
            {
                GameController.AddSpecialTile(kvp.Key, kvp.Value);
            }
            
            // Reset win state since we loaded a new level
            GameController.ResetWinState();
        }

        private Vector2Int GetGridPos(Vector3 position)
        {
            return new Vector2Int(Mathf.FloorToInt(position.x), Mathf.FloorToInt(position.y));
        }
    }
}
