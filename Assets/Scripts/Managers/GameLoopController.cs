using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using PolarBond.Core;
using PolarBond.Entities;
using PolarBond.Managers;
using PolarBond.Views;

namespace PolarBond.Logic
{
    public class GameLoopController : MonoBehaviour
    {
        private GridManager gridManager;
        private MagneticPhysicsEngine physicsEngine;
        private UndoManager undoManager;
        private WinConditionSystem winConditionSystem;
        private TileEffectSystem tileEffectSystem;

        private PlayerEntity player;
        private List<MergedBlock> allBlocks;

        public event System.Action OnUndoStateChanged;

        private void Awake()
        {
            gridManager = new GridManager();
            physicsEngine = new MagneticPhysicsEngine(gridManager);
            undoManager = new UndoManager(gridManager);
            winConditionSystem = new WinConditionSystem(gridManager);
            tileEffectSystem = new TileEffectSystem(gridManager);
            undoManager.OnUndoStateChanged += () => OnUndoStateChanged?.Invoke();
            allBlocks = new List<MergedBlock>();
        }

        public void InitializeLevel(PlayerEntity startPlayer, List<MagnetEntity> startMagnets, List<WallEntity> startWalls)
        {
            gridManager.ClearBoard();
            allBlocks.Clear();
            undoManager.Clear();

            player = startPlayer;
            gridManager.AddEntity(player);

            foreach (var w in startWalls) gridManager.AddEntity(w);

            foreach (var m in startMagnets)
            {
                gridManager.AddEntity(m);
                allBlocks.Add(MergedBlock.Get(m));
            }

            // Initial attraction check
            physicsEngine.ProcessAttraction(allBlocks);
        }

        public void AddTargetTile(Vector2Int position, TargetType type)
        {
            gridManager.AddTargetTile(position, type);
        }

        public void AddSpecialTile(Vector2Int position, SpecialTileType type)
        {
            gridManager.AddSpecialTile(position, type);
        }

        public void ProcessTurn(Direction playerInput)
        {
#if UNITY_EDITOR
            Debug.Log($"[GameLoopController] Processing turn for input: {playerInput}");
#endif
            // Snapshot state before move
            undoManager.SaveState(player, allBlocks);

            // Step 1: Mechanical Movement
            bool playerMoved = physicsEngine.TryMovePlayer(player, playerInput, allBlocks);
#if UNITY_EDITOR
            Debug.Log($"[GameLoopController] Player Moved: {playerMoved}. Player Pos: {player.Position}");
#endif
            
            if (!playerMoved)
            {
                GridEntity entityAhead = gridManager.GetEntityAt(player.Position + playerInput.ToVector2Int());
                if (entityAhead != null && entityAhead.Type == EntityType.Magnet)
                {
                    if (AudioManager.Instance != null) AudioManager.Instance.PlayErrorSound();
                }

                // If player didn't move, we just drop the saved state instead of performing a full Undo.
                // This fixes the bug where views get disconnected.
                undoManager.DropLastState(); 
                return;
            }
            else
            {
                if (AudioManager.Instance != null) AudioManager.Instance.PlayMoveSound();
            }
            // --- VÒNG LẶP ĐỘNG (PHYSICS RESOLVE LOOP) ---
            int iterations = 0;
            const int MAX_ITERATIONS = 10;
            bool stateChanged = true;

            while (stateChanged)
            {
                stateChanged = false;

                // Pha 1: Hút (Attraction) - Gom cụm trước
                physicsEngine.ProcessAttraction(allBlocks);

                // Pha 2 & 3: Đẩy (Repulsion) & Tách khối (Splitting/Collision)
                bool repulsed = physicsEngine.ApplyRepulsion(allBlocks);
                
                if (repulsed)
                {
                    stateChanged = true;
                    iterations++;

                    // Chống Vòng lặp vô hạn (Infinite Loop Prevention)
                    if (iterations >= MAX_ITERATIONS)
                    {
                        Debug.LogWarning("[GameLoopController] Infinite loop detected! Undoing step to prevent crash.");
                        undoManager.Undo(player, allBlocks);
                        return; // Hủy bước đi
                    }
                }

                // Pha 4: Kiểm tra Reverse Polarity (Đảo Cực)
                if (tileEffectSystem.ProcessEffects(allBlocks))
                {
                    stateChanged = true; // Trạng thái thay đổi, tiếp tục tính toán Hút/Đẩy
                }
            }

            // Pha 5: Cập nhật trạng thái mục tiêu (để hiển thị hình ảnh phát sáng)
            EvaluateMagnetTargets();

            // Step 6: Check Win Condition
            if (!isLevelComplete && winConditionSystem.Check())
            {
                isLevelComplete = true;
                Debug.Log("Level Complete! Loading next level...");
                StartCoroutine(LoadNextLevelRoutine());
            }
        }

        private bool isLevelComplete = false;
        
        public void ResetWinState()
        {
            isLevelComplete = false;
        }

        private IEnumerator LoadNextLevelRoutine()
        {
            // Wait slightly for player to realize they won
            yield return new WaitForSeconds(0.5f);

            if (WinScreenManager.Instance != null)
            {
                WinScreenManager.Instance.ShowWinScreen();
            }
            else
            {
                Debug.LogWarning("[GameLoopController] WinScreenManager not found! Proceeding directly to next level.");
                if (LevelManager.Instance != null)
                {
                    LevelManager.Instance.CompleteLevel();
                }
            }
        }

        public bool CanUndo()
        {
            return undoManager != null && undoManager.CanUndo();
        }

        public void UndoTurn()
        {
            Debug.Log("[GameLoopController] UndoTurn called!");
            bool success = undoManager.Undo(player, allBlocks);
            Debug.Log($"[GameLoopController] Undo result: {success}");
            
            if (success)
            {
                EvaluateMagnetTargets();
            }
        }

        public void EvaluateMagnetTargets()
        {
            var targets = gridManager.GetTargetTiles();
            Debug.Log($"[EvaluateMagnetTargets] Bắt đầu quét. Tổng số target: {targets.Count}, số lượng block: {allBlocks.Count}");
            foreach (var block in allBlocks)
            {
                foreach (var magnet in block.Magnets)
                {
                    bool isOnTarget = false;
                    if (targets.TryGetValue(magnet.Position, out TargetType tType))
                    {
                        if (tType == TargetType.NorthTarget && magnet.Polarity == MagneticPolarity.North)
                        {
                            isOnTarget = true;
                        }
                        else if (tType == TargetType.SouthTarget && magnet.Polarity == MagneticPolarity.South)
                        {
                            isOnTarget = true;
                        }
                    }
                    Debug.Log($"[EvaluateMagnetTargets] Magnet tại {magnet.Position} đang ở trên đích? {isOnTarget}");
                    magnet.SetTargetState(isOnTarget);
                }
            }
        }

    }
}
