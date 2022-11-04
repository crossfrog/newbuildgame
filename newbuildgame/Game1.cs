using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using MonoGame.Extended;
using MonoGame.Extended.ViewportAdapters;

namespace newbuildgame
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch, _tileBatch, _tileCornerBatch;

        private TileWorld _tileWorld = new();

        private BoxingViewportAdapter _viewportAdapter;
        private OrthographicCamera _camera;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);

            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        private void WindowSizeChanged(object sender, EventArgs e)
        {
            // Fill in empty space with black
            GraphicsDevice.Clear(Color.Black);
        }

        protected override void Initialize()
        {
            // Create initial window size
            const int initialWindowWidth = 640;
            const int initialWindowHeight = 360;

            const int initialScale = 2;

            Window.AllowUserResizing = true;

            _graphics.PreferredBackBufferWidth = initialWindowWidth * initialScale;
            _graphics.PreferredBackBufferHeight = initialWindowHeight * initialScale;
            _graphics.ApplyChanges();

            // Link window size change
            Window.ClientSizeChanged += WindowSizeChanged;

            // Create viewport
            _viewportAdapter = new BoxingViewportAdapter(
                Window,
                GraphicsDevice,
                initialWindowWidth,
                initialWindowHeight);

            _camera = new(_viewportAdapter);
            _camera.Origin = Vector2.Zero;
            _camera.Zoom = 16;

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _tileBatch = new SpriteBatch(GraphicsDevice);
            _tileCornerBatch = new SpriteBatch(GraphicsDevice);

            _tileWorld.LoadContent(Content);
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            _camera.Position += new Vector2(0.01f, 0.0f);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            // Fill background
            _spriteBatch.Begin(transformMatrix: _viewportAdapter.GetScaleMatrix());
            _spriteBatch.FillRectangle(_viewportAdapter.BoundingRectangle, Color.DarkSlateBlue);
            _spriteBatch.End();

            // Draw sprites
            _tileBatch.Begin(
                SpriteSortMode.Deferred,
                BlendState.AlphaBlend,
                SamplerState.PointClamp,
                DepthStencilState.Default,
                null,
                null,
                _camera.GetViewMatrix());

            _tileCornerBatch.Begin(
                SpriteSortMode.Deferred,
                BlendState.AlphaBlend,
                SamplerState.PointClamp,
                DepthStencilState.Default,
                null,
                null,
                _camera.GetViewMatrix());

            _tileWorld.Draw(_tileBatch, _tileCornerBatch, _camera.BoundingRectangle);

            _tileBatch.End();
            _tileCornerBatch.End();

            base.Draw(gameTime);
        }
    }
}