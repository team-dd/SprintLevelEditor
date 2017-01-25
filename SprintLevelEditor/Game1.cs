﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

        public int BLOCK_SIZE = 10;
        public static int MAX_BLOCK_SIZE = 50;
        public static int MIN_BLOCK_SIZE = 3;
        public int MOVE_SPEED = 10;
        public static int SCREEN_WIDTH = 800;
        public static int SCREEN_HEIGHT = 800;

        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        GameTimeWrapper mainGameTime;
        World world;
        Circle circle;
        Wall wall;
        List<Wall> oldWalls;
        List<Wall> redoQueue;
        List<Line> grid;
        bool isHoldingLeft;
        bool isHoldingRight;
        Vector2 startingHeldMousePosition;
        KeyboardState keyboardState;
        KeyboardState previousKeyboardState;
        MouseState previousMouseState;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferWidth = SCREEN_WIDTH;
            graphics.PreferredBackBufferHeight = SCREEN_HEIGHT;
            Content.RootDirectory = "Content";
            isHoldingLeft = false;
            isHoldingRight = false;
            startingHeldMousePosition = Vector2.Zero;
            oldWalls = new List<Wall>();
            redoQueue = new List<Wall>();
            grid = new List<Line>();
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
            previousMouseState = Mouse.GetState();
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
            world.virtualResolutionRenderer.VirtualResolution = new Vector2(SCREEN_WIDTH, SCREEN_HEIGHT);
            circle = new Circle(Content.Load<Texture2D>("circle"));

            mainGameTime = new GameTimeWrapper(MainUpdate, this, 1);
            world.AddGameState(MainGame, graphics);
            world.gameStates[MainGame].AddTime(mainGameTime);
            world.gameStates[MainGame].AddDraw(MainDraw);
            world.ActivateGameState(MainGame);

            wall = new Wall(graphics);
            wall.sprite.drawRect = new Rectangle(0, 0, BLOCK_SIZE, BLOCK_SIZE);

            makeGrid();
        }

        public void makeGrid()
        {
            grid = new List<Line>();
            // grid
            foreach (int i in Enumerable.Range(1, SCREEN_HEIGHT / BLOCK_SIZE))
            {
                Vector2 start = new Vector2(i * BLOCK_SIZE, 0);
                Vector2 end = new Vector2(i * BLOCK_SIZE, SCREEN_HEIGHT);
                Line line = new Line(graphics, Line.Type.Point, start, end, 1);
                grid.Add(line);
            }

            foreach (int j in Enumerable.Range(1, SCREEN_WIDTH / BLOCK_SIZE))
            {
                Vector2 start = new Vector2(0, j * BLOCK_SIZE);
                Vector2 end = new Vector2(SCREEN_WIDTH, j * BLOCK_SIZE);
                Line line = new Line(graphics, Line.Type.Point, start, end, 1);
                grid.Add(line);
            }
        }

        public void saveGame()
        {
            List<SimpleRectangle> simpleRectangles = new List<SimpleRectangle>();

            foreach (Wall oldWall in oldWalls)
            {
                simpleRectangles.Add(SimpleRectangle.fromWall(oldWall));
            }

            string json = JsonConvert.SerializeObject(simpleRectangles.ToArray(), Formatting.Indented);
            int epoch = (int) (DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds;
            File.WriteAllText(Path.Combine(Environment.CurrentDirectory, "level-" + epoch + ".json"), json);
        }

        public void undo()
        {
            if (oldWalls.Count == 0)
            {
                return;
            }

            Wall redoWall = oldWalls[oldWalls.Count - 1];
            redoQueue.Add(redoWall);
            oldWalls.RemoveAt(oldWalls.Count - 1);
        }

        public void redo()
        {
            if (redoQueue.Count == 0)
            {
                return;
            }

            Wall redoWall = redoQueue[redoQueue.Count - 1];
            oldWalls.Add(redoWall);
            redoQueue.RemoveAt(redoQueue.Count - 1);
        }

        public void startFresh()
        {
            oldWalls = new List<Wall>();
            redoQueue = new List<Wall>();
        }

        public void MainUpdate(GameTimeWrapper gameTime)
        {
            world.gameStates[MainGame].UpdateCurrentCamera(gameTime);
            keyboardState = Keyboard.GetState();

            if (Mouse.GetState().LeftButton == ButtonState.Pressed && previousMouseState.LeftButton == ButtonState.Released)
            {
                isHoldingLeft = true;
                startingHeldMousePosition = Mouse.GetState().Position.ToVector2();
                startingHeldMousePosition.X = startingHeldMousePosition.X - (startingHeldMousePosition.X % BLOCK_SIZE);
                startingHeldMousePosition.Y = startingHeldMousePosition.Y - (startingHeldMousePosition.Y % BLOCK_SIZE);
            }
            else if (Mouse.GetState().LeftButton == ButtonState.Released && previousMouseState.LeftButton == ButtonState.Pressed)
            {
                isHoldingLeft = false;

                Wall oldWall = new Wall(graphics);
                oldWall.sprite.drawRect = new Rectangle(wall.sprite.drawRect.X, wall.sprite.drawRect.Y, wall.sprite.drawRect.Width, wall.sprite.drawRect.Height);
                oldWall.sprite.position = new Vector2(wall.sprite.position.X, wall.sprite.position.Y);

                oldWalls.Add(oldWall);
                startingHeldMousePosition = Vector2.Zero;
                wall.sprite.drawRect = new Rectangle(0, 0, BLOCK_SIZE, BLOCK_SIZE);

                redoQueue = new List<Wall>();
            }

            if (keyboardState.IsKeyDown(Keys.LeftControl) && keyboardState.IsKeyDown(Keys.S) && previousKeyboardState.IsKeyUp(Keys.S))
            {
                saveGame();
            }

            if (keyboardState.IsKeyDown(Keys.LeftControl) && keyboardState.IsKeyDown(Keys.Z) && previousKeyboardState.IsKeyUp(Keys.Z))
            {
                undo();
            }

            if (keyboardState.IsKeyDown(Keys.LeftControl) && keyboardState.IsKeyDown(Keys.Y) && previousKeyboardState.IsKeyUp(Keys.Y))
            {
                redo();
            }

            if (keyboardState.IsKeyDown(Keys.LeftControl) && keyboardState.IsKeyDown(Keys.N) && previousKeyboardState.IsKeyUp(Keys.N))
            {
                startFresh();
            }

            if (Mouse.GetState().ScrollWheelValue < previousMouseState.ScrollWheelValue && BLOCK_SIZE > MIN_BLOCK_SIZE && !isHoldingLeft)
            {
                List<Wall> scaledOldWalls = new List<Wall>();
                foreach (Wall oldWall in oldWalls)
                {
                    Wall scaledOldWall = new Wall(graphics);
                    Rectangle rectangle = oldWall.sprite.drawRect;
                    scaledOldWall.sprite.drawRect = new Rectangle((rectangle.X / BLOCK_SIZE) * (BLOCK_SIZE - 1), (rectangle.Y / BLOCK_SIZE) * (BLOCK_SIZE - 1), (rectangle.Width / BLOCK_SIZE) * (BLOCK_SIZE - 1), (rectangle.Height / BLOCK_SIZE) * (BLOCK_SIZE - 1));
                    scaledOldWalls.Add(scaledOldWall);
                }
                oldWalls = scaledOldWalls;

                List<Wall> scaledRedoQueue = new List<Wall>();
                foreach (Wall oldWall in redoQueue)
                {
                    Wall scaledOldWall = new Wall(graphics);
                    Rectangle rectangle = oldWall.sprite.drawRect;
                    scaledOldWall.sprite.drawRect = new Rectangle((rectangle.X / BLOCK_SIZE) * (BLOCK_SIZE - 1), (rectangle.Y / BLOCK_SIZE) * (BLOCK_SIZE - 1), (rectangle.Width / BLOCK_SIZE) * (BLOCK_SIZE - 1), (rectangle.Height / BLOCK_SIZE) * (BLOCK_SIZE - 1));
                    scaledRedoQueue.Add(scaledOldWall);
                }
                redoQueue = scaledRedoQueue;

                BLOCK_SIZE--;
                MOVE_SPEED = BLOCK_SIZE;
                wall.sprite.drawRect = new Rectangle(0, 0, BLOCK_SIZE, BLOCK_SIZE);
                makeGrid();
            }

            if (Mouse.GetState().ScrollWheelValue > previousMouseState.ScrollWheelValue && BLOCK_SIZE < MAX_BLOCK_SIZE && !isHoldingLeft)
            {
                List<Wall> scaledOldWalls = new List<Wall>();
                foreach (Wall oldWall in oldWalls)
                {
                    Wall scaledOldWall = new Wall(graphics);
                    Rectangle rectangle = oldWall.sprite.drawRect;
                    scaledOldWall.sprite.drawRect = new Rectangle((rectangle.X / BLOCK_SIZE) * (BLOCK_SIZE + 1), (rectangle.Y / BLOCK_SIZE) * (BLOCK_SIZE + 1), (rectangle.Width / BLOCK_SIZE) * (BLOCK_SIZE + 1), (rectangle.Height / BLOCK_SIZE) * (BLOCK_SIZE + 1));
                    scaledOldWalls.Add(scaledOldWall);
                }
                oldWalls = scaledOldWalls;

                List<Wall> scaledRedoQueue = new List<Wall>();
                foreach (Wall oldWall in redoQueue)
                {
                    Wall scaledOldWall = new Wall(graphics);
                    Rectangle rectangle = oldWall.sprite.drawRect;
                    scaledOldWall.sprite.drawRect = new Rectangle((rectangle.X / BLOCK_SIZE) * (BLOCK_SIZE + 1), (rectangle.Y / BLOCK_SIZE) * (BLOCK_SIZE + 1), (rectangle.Width / BLOCK_SIZE) * (BLOCK_SIZE + 1), (rectangle.Height / BLOCK_SIZE) * (BLOCK_SIZE + 1));
                    scaledRedoQueue.Add(scaledOldWall);
                }
                redoQueue = scaledRedoQueue;

                BLOCK_SIZE++;
                MOVE_SPEED = BLOCK_SIZE;
                wall.sprite.drawRect = new Rectangle(0, 0, BLOCK_SIZE, BLOCK_SIZE);
                makeGrid();
            }

            if (keyboardState.IsKeyDown(Keys.Down) && !isHoldingLeft)
            {
                List<Wall> scaledOldWalls = new List<Wall>();
                foreach (Wall oldWall in oldWalls)
                {
                    Wall scaledOldWall = new Wall(graphics);
                    Rectangle rectangle = oldWall.sprite.drawRect;
                    scaledOldWall.sprite.drawRect = new Rectangle(rectangle.X, rectangle.Y - MOVE_SPEED, rectangle.Width, rectangle.Height);
                    scaledOldWalls.Add(scaledOldWall);
                }
                oldWalls = scaledOldWalls;

                List<Wall> scaledRedoQueue = new List<Wall>();
                foreach (Wall oldWall in redoQueue)
                {
                    Wall scaledOldWall = new Wall(graphics);
                    Rectangle rectangle = oldWall.sprite.drawRect;
                    scaledOldWall.sprite.drawRect = new Rectangle(rectangle.X, rectangle.Y - MOVE_SPEED, rectangle.Width, rectangle.Height);
                    scaledRedoQueue.Add(scaledOldWall);
                }
                redoQueue = scaledRedoQueue;
            }

            if (keyboardState.IsKeyDown(Keys.Up) && !isHoldingLeft)
            {
                List<Wall> scaledOldWalls = new List<Wall>();
                foreach (Wall oldWall in oldWalls)
                {
                    Wall scaledOldWall = new Wall(graphics);
                    Rectangle rectangle = oldWall.sprite.drawRect;
                    scaledOldWall.sprite.drawRect = new Rectangle(rectangle.X, rectangle.Y + MOVE_SPEED, rectangle.Width, rectangle.Height);
                    scaledOldWalls.Add(scaledOldWall);
                }
                oldWalls = scaledOldWalls;

                List<Wall> scaledRedoQueue = new List<Wall>();
                foreach (Wall oldWall in redoQueue)
                {
                    Wall scaledOldWall = new Wall(graphics);
                    Rectangle rectangle = oldWall.sprite.drawRect;
                    scaledOldWall.sprite.drawRect = new Rectangle(rectangle.X, rectangle.Y + MOVE_SPEED, rectangle.Width, rectangle.Height);
                    scaledRedoQueue.Add(scaledOldWall);
                }
                redoQueue = scaledRedoQueue;
            }

            if (keyboardState.IsKeyDown(Keys.Left) && !isHoldingLeft)
            {
                List<Wall> scaledOldWalls = new List<Wall>();
                foreach (Wall oldWall in oldWalls)
                {
                    Wall scaledOldWall = new Wall(graphics);
                    Rectangle rectangle = oldWall.sprite.drawRect;
                    scaledOldWall.sprite.drawRect = new Rectangle(rectangle.X + MOVE_SPEED, rectangle.Y, rectangle.Width, rectangle.Height);
                    scaledOldWalls.Add(scaledOldWall);
                }
                oldWalls = scaledOldWalls;

                List<Wall> scaledRedoQueue = new List<Wall>();
                foreach (Wall oldWall in redoQueue)
                {
                    Wall scaledOldWall = new Wall(graphics);
                    Rectangle rectangle = oldWall.sprite.drawRect;
                    scaledOldWall.sprite.drawRect = new Rectangle(rectangle.X + MOVE_SPEED, rectangle.Y, rectangle.Width, rectangle.Height);
                    scaledRedoQueue.Add(scaledOldWall);
                }
                redoQueue = scaledRedoQueue;
            }

            if (keyboardState.IsKeyDown(Keys.Right) && !isHoldingLeft)
            {
                List<Wall> scaledOldWalls = new List<Wall>();
                foreach (Wall oldWall in oldWalls)
                {
                    Wall scaledOldWall = new Wall(graphics);
                    Rectangle rectangle = oldWall.sprite.drawRect;
                    scaledOldWall.sprite.drawRect = new Rectangle(rectangle.X - MOVE_SPEED, rectangle.Y, rectangle.Width, rectangle.Height);
                    scaledOldWalls.Add(scaledOldWall);
                }
                oldWalls = scaledOldWalls;

                List<Wall> scaledRedoQueue = new List<Wall>();
                foreach (Wall oldWall in redoQueue)
                {
                    Wall scaledOldWall = new Wall(graphics);
                    Rectangle rectangle = oldWall.sprite.drawRect;
                    scaledOldWall.sprite.drawRect = new Rectangle(rectangle.X - MOVE_SPEED, rectangle.Y, rectangle.Width, rectangle.Height);
                    scaledRedoQueue.Add(scaledOldWall);
                }
                redoQueue = scaledRedoQueue;
            }

            circle.Update(gameTime);
            wall.Update(gameTime);

            previousKeyboardState = keyboardState;
            previousMouseState = Mouse.GetState();

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
            
            if (!isHoldingLeft)
            {
                wall.sprite.position = new Vector2(mouseX - mouseX%BLOCK_SIZE, mouseY - mouseY%BLOCK_SIZE);
            }
            else
            {
                circle.position = new Vector2(-100, -100);
                float width = Math.Max(BLOCK_SIZE, Math.Abs(startingHeldMousePosition.X - Mouse.GetState().Position.X) - (Math.Abs(startingHeldMousePosition.X - Mouse.GetState().Position.X) % BLOCK_SIZE));
                float height = Math.Max(BLOCK_SIZE, Math.Abs(startingHeldMousePosition.Y - Mouse.GetState().Position.Y) - (Math.Abs(startingHeldMousePosition.Y - Mouse.GetState().Position.Y) % BLOCK_SIZE));

                int drawX = Mouse.GetState().Position.X < startingHeldMousePosition.X
                    ? (int) (startingHeldMousePosition.X - width)
                    : (int) startingHeldMousePosition.X;

                if (Mouse.GetState().Position.X < startingHeldMousePosition.X)
                {
                    width += BLOCK_SIZE;
                }

                int drawY = Mouse.GetState().Position.Y < startingHeldMousePosition.Y
                    ? (int) (startingHeldMousePosition.Y - height)
                    : (int) startingHeldMousePosition.Y;

                if (Mouse.GetState().Position.Y < startingHeldMousePosition.Y)
                {
                    height += BLOCK_SIZE;
                }

                wall.sprite.drawRect = new Rectangle(drawX, drawY, (int) width, (int) height);
                wall.sprite.position = new Vector2((startingHeldMousePosition.X + Mouse.GetState().Position.X) / 2, (startingHeldMousePosition.Y + Mouse.GetState().Position.Y) / 2);
            }

            world.DrawWorld();

            base.Draw(gameTime);
        }

        public void MainDraw()
        {
            world.BeginDraw();
            foreach (Line line in grid)
            {
                world.Draw(line.Draw);
            }
            foreach (Wall oldWall in oldWalls)
            {
                world.Draw(oldWall.Draw);
            }
            //world.Draw(circle.Draw);
            world.Draw(wall.Draw);
            world.EndDraw();
        }
    }
}