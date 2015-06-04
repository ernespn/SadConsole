﻿// Copyright (c) 2015 Andrew Varnerin
// Taken from https://github.com/BaconSoap/RexReader
// Namespace changed to match SadConsole

namespace SadConsole.Readers.REXPaint
{
    public class TileLayer {
        /// <summary>
        /// A 2D array of the tiles in this layer, of the form Tiles[y,x].
        /// Tiles[0,10] is the tile in the first row and the eleventh column.
        /// </summary>
        public Tile[,] Tiles { get; set; }

        public TileLayer(int width, int height) {
            //Going by y,x is low-hanging fruit on the tree of optimization
            Tiles = new Tile[height, width];
        }

        /// <summary>
        /// Converts a <see cref="TileLayer"/> to a <see cref="CellSurface"/>.
        /// </summary>
        /// <returns>The convereted surface.</returns>
        public CellSurface ToCellSurface()
        {
            var cells = new CellSurface(Tiles.GetUpperBound(0), Tiles.GetUpperBound(1));

            cells.Fill(Microsoft.Xna.Framework.Color.Black, Microsoft.Xna.Framework.Color.Transparent, 0, null);

            for (int y = 0; y < cells.Height; y++)
            {
                for (int x = 0; x < cells.Width; x++)
                {
                    var tile = Tiles[y, x];

                    if (tile.CharacterCode != 0)
                    {
                        var cell = cells[x, y];
                        cell.CharacterIndex = tile.CharacterCode;
                        cell.Foreground = new Microsoft.Xna.Framework.Color(tile.ForegroundRed, tile.ForegroundGreen, tile.ForegroundBlue);
                        cell.Background = new Microsoft.Xna.Framework.Color(tile.BackgroundRed, tile.BackgroundGreen, tile.BackgroundBlue);
                    }
                }
            }

            return cells;
        }
    }
}
