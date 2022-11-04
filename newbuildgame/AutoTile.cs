using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace newbuildgame
{
    public class AutoTile
    {
        public readonly Texture2D Texture;
        public readonly int TileWidth, TileHeight;

        public readonly Point TileSize;
        private readonly Vector2 _tileScale;

        private readonly Rectangle _centerRect;

        private readonly Rectangle _topRect;
        private readonly Rectangle _bottomRect;
        private readonly Rectangle _leftRect;
        private readonly Rectangle _rightRect;

        private readonly Rectangle _topLeftOuterRect;
        private readonly Rectangle _topRightOuterRect;
        private readonly Rectangle _bottomLeftOuterRect;
        private readonly Rectangle _bottomRightOuterRect;

        private readonly Rectangle _topLeftInnerRect;
        private readonly Rectangle _topRightInnerRect;
        private readonly Rectangle _bottomLeftInnerRect;
        private readonly Rectangle _bottomRightInnerRect;

        private readonly Vector2 _topOffset = new Vector2(0.0f, -0.5f);
        private readonly Vector2 _bottomOffset = new Vector2(0.0f, 0.5f);
        private readonly Vector2 _leftOffset = new Vector2(-0.5f, 0.0f);
        private readonly Vector2 _rightOffset = new Vector2(0.5f, 0.0f);

        private readonly Vector2 _topLeftOffset = new Vector2(-0.5f, -0.5f);
        private readonly Vector2 _topRightOffset = new Vector2(0.5f, -0.5f);
        private readonly Vector2 _bottomLeftOffset = new Vector2(-0.5f, 0.5f);
        private readonly Vector2 _bottomRightOffset = new Vector2(0.5f, 0.5f);

        private const int _topLeftShift = 0;
        private const int _topShift = 1;
        private const int _topRightShift = 2;
        private const int _leftShift = 3;
        private const int _rightShift = 4;
        private const int _bottomLeftShift = 5;
        private const int _bottomShift = 6;
        private const int _bottomRightShift = 7;

        private const int _topLeftMask = (1 << _topShift) | (1 << _leftShift);
        private const int _topRightMask = (1 << _topShift) | (1 << _rightShift);
        private const int _bottomLeftMask = (1 << _bottomShift) | (1 << _leftShift);
        private const int _bottomRightMask = (1 << _bottomShift) | (1 << _rightShift);

        public AutoTile(Texture2D texture, int tileWidth, int tileHeight)
        {
            Texture = texture;

            TileWidth = tileWidth;
            TileHeight = tileHeight;

            TileSize = new Point(tileWidth, tileHeight);
            _tileScale = new Vector2(1.0f / TileWidth, 1.0f / TileHeight);

            _centerRect = new Rectangle(
                TileSize * new Point(1, 1), TileSize);

            _topRect = new Rectangle(
                TileSize * new Point(1, 0), TileSize);

            _bottomRect = new Rectangle(
                TileSize * new Point(1, 2), TileSize);

            _leftRect = new Rectangle(
                TileSize * new Point(0, 1), TileSize);

            _rightRect = new Rectangle(
                TileSize * new Point(2, 1), TileSize);

            _topLeftOuterRect = new Rectangle(
                TileSize * new Point(2, 2), TileSize);

            _topRightOuterRect = new Rectangle(
                TileSize * new Point(0, 2), TileSize);

            _bottomLeftOuterRect = new Rectangle(
                TileSize * new Point(2, 0), TileSize);

            _bottomRightOuterRect = new Rectangle(
                TileSize * new Point(0, 0), TileSize);

            _topLeftInnerRect = new Rectangle(
                TileSize * new Point(3, 0), TileSize);

            _topRightInnerRect = new Rectangle(
                TileSize * new Point(4, 0), TileSize);

            _bottomLeftInnerRect = new Rectangle(
                TileSize * new Point(3, 1), TileSize);

            _bottomRightInnerRect = new Rectangle(
                TileSize * new Point(4, 1), TileSize);
        }

        private static int AutoTileTest(TileWorld tileWorld, int x, int y)
        {
            return tileWorld.GetTile(x, y) == 0 ? 0 : 1;
        }

        private void DrawTile(
            SpriteBatch spriteBatch,
            Vector2 tilePosition,
            Vector2 tileOffset,
            Rectangle tileRect,
            Color color)
        {
            spriteBatch.Draw(
                Texture,
                tilePosition + tileOffset,
                tileRect,
                color,
                0.0f,
                Vector2.Zero,
                _tileScale,
                SpriteEffects.None,
                0.0f);
        }

        private void DrawEdgeTile(
            SpriteBatch spriteBatch,
            Vector2 tilePosition,
            Vector2 tileOffset,
            Rectangle tileRect,
            Color color,
            int bitMask,
            int bitShift)
        {
            if ((bitMask & (1 << bitShift)) == 0)
            {
                DrawTile(spriteBatch, tilePosition, tileOffset, tileRect, color);
            }
        }

        private void DrawCornerTile(
            SpriteBatch spriteBatch,
            Vector2 tilePosition,
            Vector2 tileOffset,
            Rectangle outerRect,
            Rectangle innerRect,
            Color color,
            int bitMask,
            int bitShift,
            int cornerMask)
        {
            var bits = bitMask & cornerMask;

            if (bits == 0)
            {
                // Draw outer
                DrawTile(spriteBatch, tilePosition, tileOffset, outerRect, color);
            }
            else if ((bits | (bitMask & (1 << bitShift))) == cornerMask)
            {
                // Draw inner
                DrawTile(spriteBatch, tilePosition, tileOffset, innerRect, color);
            }
        }

        public void Draw(SpriteBatch tileBatch, SpriteBatch tileCornerBatch, TileWorld tileWorld, int x, int y)
        {
            int lightValue = tileWorld.GetLightValue(x, y);
            var lightness = 1.0f - (float)lightValue / 16;
            var color = new Color(lightness, lightness, lightness, 1.0f);

            color = Color.White;

            var tilePosition = new Vector2(x, y);

            // Draw center
            DrawTile(tileBatch, tilePosition, Vector2.Zero, _centerRect, color);

            // Create a bitMask based on neighboring tiles
            var bitMask = 0;
            
            bitMask |= AutoTileTest(tileWorld, x - 1, y - 1) << _topLeftShift;
            bitMask |= AutoTileTest(tileWorld, x, y - 1) << _topShift;
            bitMask |= AutoTileTest(tileWorld, x + 1, y - 1) << _topRightShift;

            bitMask |= AutoTileTest(tileWorld, x - 1, y) << _leftShift;
            bitMask |= AutoTileTest(tileWorld, x + 1, y) << _rightShift;

            bitMask |= AutoTileTest(tileWorld, x - 1, y + 1) << _bottomLeftShift;
            bitMask |= AutoTileTest(tileWorld, x, y + 1) << _bottomShift;
            bitMask |= AutoTileTest(tileWorld, x + 1, y + 1) << _bottomRightShift;

            // Draw top
            DrawEdgeTile(
                tileBatch,
                tilePosition,
                _topOffset,
                _topRect,
                color,
                bitMask,
                _topShift);

            // Draw bottom
            DrawEdgeTile(
                tileBatch,
                tilePosition,
                _bottomOffset,
                _bottomRect,
                color,
                bitMask,
                _bottomShift);

            // Draw left
            DrawEdgeTile(
                tileBatch,
                tilePosition,
                _leftOffset,
                _leftRect,
                color,
                bitMask,
                _leftShift);

            // Draw right
            DrawEdgeTile(
                tileBatch,
                tilePosition,
                _rightOffset,
                _rightRect,
                color,
                bitMask,
                _rightShift);

            // Draw top left
            DrawCornerTile(
                tileCornerBatch,
                tilePosition,
                _topLeftOffset, 
                _topLeftOuterRect,
                _topLeftInnerRect,
                color,
                bitMask,
                _topLeftShift,
                _topLeftMask);

            // Draw top right
            DrawCornerTile(
                tileCornerBatch,
                tilePosition,
                _topRightOffset,
                _topRightOuterRect,
                _topRightInnerRect,
                color,
                bitMask,
                _topRightShift,
                _topRightMask);

            // Draw bottom left
            DrawCornerTile(
                tileCornerBatch,
                tilePosition,
                _bottomLeftOffset,
                _bottomLeftOuterRect,
                _bottomLeftInnerRect,
                color,
                bitMask,
                _bottomLeftShift,
                _bottomLeftMask);

            // Draw bottom right
            DrawCornerTile(
                tileCornerBatch,
                tilePosition,
                _bottomRightOffset,
                _bottomRightOuterRect,
                _bottomRightInnerRect,
                color,
                bitMask,
                _bottomRightShift,
                _bottomRightMask);
        }
    }
}
