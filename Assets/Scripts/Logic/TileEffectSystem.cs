using System.Collections.Generic;
using PolarBond.Entities;
using PolarBond.Managers;
using PolarBond.Core;

namespace PolarBond.Logic
{
    public class TileEffectSystem
    {
        private GridManager gridManager;

        public TileEffectSystem(GridManager gridManager)
        {
            this.gridManager = gridManager;
        }

        public bool ProcessEffects(List<MergedBlock> allBlocks)
        {
            bool stateChanged = false;

            foreach (var block in allBlocks)
            {
                foreach (var magnet in block.Magnets)
                {
                    if (gridManager.GetSpecialTile(magnet.Position) == SpecialTileType.PolarityReverse)
                    {
                        if (!magnet.WasOnReverseTile)
                        {
                            magnet.ReversePolarity();
                            magnet.WasOnReverseTile = true;
                            stateChanged = true; // Trạng thái thay đổi, tiếp tục tính toán Hút/Đẩy
                        }
                    }
                    else
                    {
                        magnet.WasOnReverseTile = false;
                    }
                }
            }

            return stateChanged;
        }
    }
}
