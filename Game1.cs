using System;
using System.Collections.Generic;
using System.IO;
using GLX;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Newtonsoft.Json;

namespace SprintLevelEditor
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Game1 : Game
    {
        const string MainGame = "game1";

        int BLOCK_SIZE = 10;

        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        GameTimeWrapper mainGameTime;
        World world;
        Circle circle;
        Wall wall;
        List<Wall> oldWalls;
        bool isHoldingLeft;
        bool isHoldingRight;
        Vector2 startingHeldMousePosition;
        KeyboardState keyboardState;
        KeyboardState previousKeyboardState;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            isHoldingLeft = false;
            isHoldingRight = false;
            startingHeldMousePosition = Vector2.Zero;
            oldWalls = new List<Wall>();
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            previousKeyboardState = Keyboard.GetState();
            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            world = new World(graphics);
            circle = new Circle(Content.Load<Texture2D>("circle"));

            mainGameTime = new GameTimeWrapper(MainUpdate, this, 1);
            world.AddGameState(MainGame, graphics);
            world.gameStates[MainGame].AddTime(mainGameTime);
            world.gameStates[MainGame].AddDraw(MainDraw);
            world.ActivateGameState(MainGame);

            wall = new Wall(graphics);
            wall.sprite.drawRect = new Rectangle(0, 0, BLOCK_SIZE, BLOCK_SIZE);

            // TODO: use this.Content to load your game content here
        }

        public void saveGame()
        {
            List<SimpleRectangle> simpleRectangles = new List<SimpleRectangle>();

            foreach (Wall oldWall in oldWalls)
            {
                simpleRectangles.Add(SimpleRectangle.fromWall(oldWall));
            }

            string json = JsonConvert.SerializeObject(simpleRectangles.ToArray(), Formatting.Indented);
            int epoch = (int)((DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds);
            Console.WriteLine(Path.Combine(Environment.CurrentDirectory, "level-" + epoch + ".json"));
            File.WriteAllText(Path.Combine(Environment.CurrentDirectory, "level-" + epoch + ".json"), json);
        }

        public void MainUpdate(GameTimeWrapper gameTime)
        {
            world.gameStates[MainGame].UpdateCurrentCamera(gameTime);
            keyboardState = Keyboard.GetState();

            if (Mouse.GetState().LeftButton == ButtonState.Pressed && startingHeldMousePosition == Vector2.Zero)
            {
                isHoldingLeft = true;
                startingHeldMousePosition = Mouse.GetState().Position.ToVector2();
            }
            else if (Mouse.GetState().LeftButton == ButtonState.Released && startingHeldMousePosition != Vector2.Zero)
            {
                isHoldingLeft = false;

                Wall oldWall = new Wall(graphics);
                oldWall.sprite.drawRect = new Rectangle(wall.sprite.drawRect.X, wall.sprite.drawRect.Y, wall.sprite.drawRect.Width, wall.sprite.drawRect.Height);
                oldWall.sprite.position = new Vector2(wall.sprite.position.X, wall.sprite.position.Y);

                oldWalls.Add(oldWall);
                startingHeldMousePosition = Vector2.Zero;
                wall.sprite.drawRect = new Rectangle(0, 0, BLOCK_SIZE, BLOCK_SIZE);
            }

            if (keyboardState.IsKeyDown(Keys.S) && previousKeyboardState.IsKeyUp(Keys.S))
            {
                saveGame();
            }

            circle.Update(gameTime);
            wall.Update(gameTime);

            previousKeyboardState = keyboardState;

            base.Update(gameTime);
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // TODO: Add your update logic here
            world.Update(gameTime);
            base.Update(gameTime);
        }
       
        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // TODO: Add your drawing code here
            MouseState mouseState = Mouse.GetState();
            int mouseX = mouseState.Position.X;
            int mouseY = mouseState.Position.Y;

            circle.position = new Vector2(mouseX, mouseY);
            
            if (!isHoldingLeft)
            {
                wall.sprite.position = new Vector2(mouseX - (mouseX % BLOCK_SIZE), mouseY - (mouseY % BLOCK_SIZE));
            }
            else
            {
                float width = Math.Max(BLOCK_SIZE, Math.Abs(startingHeldMousePosition.X - Mouse.GetState().Position.X) - (Math.Abs(startingHeldMousePosition.X - Mouse.GetState().Position.X) % BLOCK_SIZE));
                float height = Math.Max(BLOCK_SIZE, Math.Abs(startingHeldMousePosition.Y - Mouse.GetState().Position.Y) - (Math.Abs(startingHeldMousePosition.Y - Mouse.GetState().Position.Y) % BLOCK_SIZE));

                int drawX = Mouse.GetState().Position.X < startingHeldMousePosition.X
                    ? (int) (startingHeldMousePosition.X - width)
                    : (int) startingHeldMousePosition.X;

                int drawY = Mouse.GetState().Position.Y < startingHeldMousePosition.Y
                    ? (int)(startingHeldMousePosition.Y - height)
                    : (int)startingHeldMousePosition.Y;

                wall.sprite.drawRect = new Rectangle(drawX, drawY, (int) width, (int) height);
                wall.sprite.position = new Vector2((startingHeldMousePosition.X + Mouse.GetState().Position.X) / 2, (startingHeldMousePosition.Y + Mouse.GetState().Position.Y) / 2);
            }

            world.DrawWorld();

            base.Draw(gameTime);
        }

        public void MainDraw()
        {
            world.BeginDraw();
            world.Draw(circle.Draw);
            world.Draw(wall.Draw);
            foreach (Wall oldWall in oldWalls)
            {
                world.Draw(oldWall.Draw);
            }
            world.EndDraw();
        }
    }
}
