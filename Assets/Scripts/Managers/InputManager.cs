using UnityEngine;
using PolarBond.Core;
using PolarBond.Logic;

namespace PolarBond.Managers
{
    public class InputManager : MonoBehaviour
    {
        [SerializeField] private GameLoopController gameController;

        private void Update()
        {
            if (gameController == null) return;

#if ENABLE_INPUT_SYSTEM
            var keyboard = UnityEngine.InputSystem.Keyboard.current;
            if (keyboard == null) return;

            if (keyboard.wKey.wasPressedThisFrame || keyboard.upArrowKey.wasPressedThisFrame)
            {
                gameController.ProcessTurn(Direction.Up);
            }
            else if (keyboard.sKey.wasPressedThisFrame || keyboard.downArrowKey.wasPressedThisFrame)
            {
                gameController.ProcessTurn(Direction.Down);
            }
            else if (keyboard.aKey.wasPressedThisFrame || keyboard.leftArrowKey.wasPressedThisFrame)
            {
                gameController.ProcessTurn(Direction.Left);
            }
            else if (keyboard.dKey.wasPressedThisFrame || keyboard.rightArrowKey.wasPressedThisFrame)
            {
                gameController.ProcessTurn(Direction.Right);
            }
            else if (keyboard.zKey.wasPressedThisFrame || keyboard.uKey.wasPressedThisFrame)
            {
                gameController.UndoTurn();
            }
#else
            // Fallback for old input system just in case
            if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
            {
                gameController.ProcessTurn(Direction.Up);
            }
            else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
            {
                gameController.ProcessTurn(Direction.Down);
            }
            else if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
            {
                gameController.ProcessTurn(Direction.Left);
            }
            else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
            {
                gameController.ProcessTurn(Direction.Right);
            }
            else if (Input.GetKeyDown(KeyCode.Z) || Input.GetKeyDown(KeyCode.U))
            {
                gameController.UndoTurn();
            }
#endif
        }
    }
}
