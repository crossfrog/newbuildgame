using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

using MonoGame.Extended;

using SimplexNoise;

namespace newbuildgame
{
    public class TileChunk
    {
        public const int ChunkWidth = 16;
        public const int ChunkHeight = 64;

        public int[,] Tiles = new int[ChunkWidth, ChunkHeight];
        public int[,] LightValues = new int[ChunkWidth, ChunkHeight];
    }

    public class TileWorld
    {
        private readonly Dictionary<int, TileChunk> _chunks = new();
        private AutoTile _dirtAutoTile, _ironAutoTile;

        private int? _lastChunkIndex = null;
        private TileChunk _lastChunk;

        private class ChunkResult
        {
            public TileChunk Chunk;
            public int RelativeX;

            public ChunkResult(TileChunk chunk, int relativeX)
            {
                Chunk = chunk;
                RelativeX = relativeX;
            }
        }

        private ChunkResult GetLastChunkResult(int x, int chunkIndex)
        {
            var relativeX = x - chunkIndex * TileChunk.ChunkWidth;
            return new ChunkResult(_lastChunk, relativeX);
        }

        private ChunkResult GetChunkFor(int x, int y)
        {
            if (y < 0 || y >= TileChunk.ChunkHeight)
            {
                // Tile is not within vertical range
                return null;
            }

            // Calculate which chunk the tile is on
            var chunkIndex = (int)Math.Floor((float)x / TileChunk.ChunkWidth);

            // Optimization for checking the same chunk many times
            if (_lastChunkIndex == chunkIndex)
            {
                // Chunk was found last time
                if (_lastChunk == null)
                {
                    return null;
                }

                return GetLastChunkResult(x, chunkIndex);
            }

            // Find new chunk
            _lastChunkIndex = chunkIndex;

            if (_chunks.ContainsKey(chunkIndex))
            {
                _lastChunk = _chunks[chunkIndex];
                return GetLastChunkResult(x, chunkIndex);
            }
            else
            {
                // Chunk does not exist
                _lastChunk = null;
                return null;
            }
        }

        public int GetTile(int x, int y)
        {
            var chunkResult = GetChunkFor(x, y);

            if (chunkResult == null)
                return 0;
            else
                return chunkResult.Chunk.Tiles[chunkResult.RelativeX, y];
        }

        public int GetLightValue(int x, int y)
        {
            var chunkResult = GetChunkFor(x, y);

            if (chunkResult == null)
                return 16;
            else
                return chunkResult.Chunk.LightValues[chunkResult.RelativeX, y];
        }

        public void GenerateChunk(int chunkIndex)
        {
            if (_chunks.ContainsKey(chunkIndex))
            {
                // Chunk was already generated
                return;
            }

            // Create a new chunk
            TileChunk chunk = new();
            _chunks[chunkIndex] = chunk;

            int chunkStartX = chunkIndex * TileChunk.ChunkWidth;

            // Generate tiles
            for (int x = 0; x < TileChunk.ChunkWidth; x++)
            {
                for (int y = 0; y < TileChunk.ChunkHeight; y++)
                {
                    var noiseSample = Noise.CalcPixel2D(chunkStartX + x, y, 0.1f);

                    if (noiseSample < 60)
                    {
                        chunk.Tiles[x, y] = 2;
                    }
                    else if (noiseSample < 100)
                    {
                        chunk.Tiles[x, y] = 1;
                    }

                    chunk.LightValues[x, y] = x + y;
                }
            }
        }

        public void LoadContent(ContentManager content)
        {
            var dirtTexture = content.Load<Texture2D>("dirt");
            _dirtAutoTile = new(dirtTexture, 8, 8);

            var ironTexture = content.Load<Texture2D>("iron");
            _ironAutoTile = new(ironTexture, 8, 8);

            var random = new Random();
            Noise.Seed = random.Next();

            for (int i = 0; i < 10; i++)
            {
                GenerateChunk(i);
            }
        }

        public void Draw(SpriteBatch tileBatch, SpriteBatch tileCornerBatch, RectangleF cameraBounds)
        {
            int
                tileMinX = (int)Math.Floor(cameraBounds.Left) - 1,
                tileMaxX = (int)Math.Ceiling(cameraBounds.Right) + 1,
                tileMinY = (int)Math.Floor(cameraBounds.Top) - 1,
                tileMaxY = (int)Math.Ceiling(cameraBounds.Bottom) + 1;

            for (int x = tileMinX; x < tileMaxX; x++)
            {
                for (int y = tileMinY; y < tileMaxY; y++)
                {
                    var id = GetTile(x, y);

                    if (id != 0)
                    {
                        // Draw tile
                        switch (id)
                        {
                            case 1:
                                _dirtAutoTile.Draw(tileBatch, tileCornerBatch, this, x, y);
                                break;
                            case 2:
                                _ironAutoTile.Draw(tileBatch, tileCornerBatch, this, x, y);
                                break;
                        }
                    }
                }
            }
        }
    }
}
