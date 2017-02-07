﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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

        public float BLOCK_SIZE = 10;
        public static int MAX_BLOCK_SIZE = 50;
        public static int MIN_BLOCK_SIZE = 3;
        public int MOVE_SPEED = 10;
        public static int SCREEN_WIDTH = 1800;
        public static int SCREEN_HEIGHT = 1000;

        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        GameTimeWrapper mainGameTime;
        World world;
        Circle circle;
        Wall wall;
        Wall cursorOutline;
        List<Wall> oldWalls;
        List<Wall> redoQueue;
        Grid grid;
        bool isHoldingLeft;
        bool isHoldingRight;
        Vector2 startingHeldMousePosition;
        KeyboardState keyboardState;
        KeyboardState previousKeyboardState;
        MouseState previousMouseState;
        bool isGridOn;
        Camera mainCamera;
        Camera minimapCamera;
        VirtualResolutionRenderer minimapVirtualResolutionRenderer;
        Wall minimapBackground;
        Wall hoveredOutline;
        bool isBlockSelected;
        bool isHoveringOverABlock;
        Wall hoveredBlock;
        Wall selectedBlock;
        Wall selectedOutline;

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
            isBlockSelected = false;
            isHoveringOverABlock = false;
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
            isGridOn = true;
            this.Window.Position = new Point(0, 0);
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

            grid = new Grid(graphics, SCREEN_WIDTH, SCREEN_HEIGHT, BLOCK_SIZE);

            mainGameTime = new GameTimeWrapper(MainUpdate, this, 1);
            world.AddGameState(MainGame, graphics);
            world.gameStates[MainGame].AddTime(mainGameTime);
            world.gameStates[MainGame].AddDraw(MainDraw);
            world.ActivateGameState(MainGame);

            hoveredBlock = new Wall(graphics);
            selectedBlock = new Wall(graphics);

            wall = new Wall(graphics);
            wall.sprite.DrawSize = new Size(BLOCK_SIZE, BLOCK_SIZE);
            wall.sprite.position = new Vector2(10, 10);
            minimapBackground = new Wall(graphics);
            minimapBackground.sprite.color = Color.Black;
            minimapBackground.sprite.DrawSize = new Size(99999, 99999);
            hoveredOutline = new Wall(graphics);
            selectedOutline = new Wall(graphics);
            cursorOutline = new Wall(graphics);
            cursorOutline.sprite.color = Color.Red;

            minimapVirtualResolutionRenderer = new VirtualResolutionRenderer(graphics, new Size(SCREEN_WIDTH, SCREEN_HEIGHT), new Size(SCREEN_WIDTH / 10, SCREEN_HEIGHT / 10));
            minimapVirtualResolutionRenderer.BackgroundColor = Color.Black;
            minimapCamera = new Camera(minimapVirtualResolutionRenderer, Camera.CameraFocus.TopLeft);
            minimapCamera.Zoom = .05f;
            world.AddCamera("minimap", minimapCamera);

            world.CurrentCameraName = "camera1";
            world.CurrentCamera.Focus = Camera.CameraFocus.Center;
            world.CurrentCamera.Zoom = 10f;
            //grid.Pan(SCREEN_WIDTH, SCREEN_HEIGHT, BLOCK_SIZE);
        }

        public void saveGame()
        {
            System.Windows.Forms.SaveFileDialog saveFileDialog = new System.Windows.Forms.SaveFileDialog();
            saveFileDialog.Filter = ".json file (*.json)|*.json";
            saveFileDialog.FilterIndex = 2;
            saveFileDialog.RestoreDirectory = true;
            saveFileDialog.Title = "Save your level";
            saveFileDialog.ShowDialog();

            if (saveFileDialog.FileName != "")
            {
                FileStream fs = (FileStream) saveFileDialog.OpenFile();
                List<SimpleRectangle> simpleRectangles = new List<SimpleRectangle>();

                int xOffset = (int)oldWalls[0].sprite.position.X;
                int yOffset = (int)oldWalls[0].sprite.position.Y;

                foreach (Wall oldWall in oldWalls)
                {
                    if (oldWall.sprite.position.X < xOffset)
                    {
                        xOffset = (int)oldWall.sprite.position.X;
                    }

                    if (oldWall.sprite.position.Y < yOffset)
                    {
                        yOffset = (int)oldWall.sprite.position.Y;
                    }
                }

                foreach (Wall oldWall in oldWalls)
                {
                    simpleRectangles.Add(SimpleRectangle.fromWall(oldWall, BLOCK_SIZE, xOffset, yOffset));
                }

                string json = JsonConvert.SerializeObject(simpleRectangles.ToArray(), Formatting.Indented);
                int epoch = (int)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds;
                byte[] jsonBytes = Encoding.ASCII.GetBytes(json);

                fs.Write(jsonBytes, 0, jsonBytes.Length);
                fs.Close();
            }
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

        public bool isHoveringOverBlock(Wall block)
        {
            MouseState mouseState = Mouse.GetState();
            Vector2 mousePosition = world.CurrentCamera.MouseToScreenCoords(Mouse.GetState().Position);
            return (block.sprite.rectangle.Contains(mousePosition.X, mousePosition.Y));
        }

        public void MainUpdate(GameTimeWrapper gameTime)
        {
            world.CurrentCameraName = "camera1";
            keyboardState = Keyboard.GetState();
            MouseState mouseState = Mouse.GetState();
            Vector2 mousePosition = world.CurrentCamera.MouseToScreenCoords(Mouse.GetState().Position);
            float mouseX = mousePosition.X;
            float mouseY = mousePosition.Y;

            if (mouseState.LeftButton == ButtonState.Pressed && previousMouseState.LeftButton == ButtonState.Released)
            {
                if (isHoveringOverABlock)
                {
                    if (hoveredBlock == selectedBlock)
                    {
                        selectedBlock = new Wall(graphics);
                    }
                    else
                    {
                        selectedBlock = hoveredBlock;
                        selectedOutline = new Wall(graphics);
                        selectedOutline.sprite.color = Color.DarkRed;
                        selectedOutline.sprite.position = new Vector2(hoveredOutline.sprite.position.X - 1, hoveredOutline.sprite.position.Y - 1);
                        selectedOutline.sprite.DrawSize = new Size(hoveredOutline.sprite.DrawSize.Width + 2, hoveredOutline.sprite.DrawSize.Height + 2);
                        selectedOutline.Update(gameTime);
                    }
                }
                else
                {
                    isHoldingLeft = true;
                    startingHeldMousePosition = new Vector2();
                    startingHeldMousePosition.X = mouseX - (mouseX % BLOCK_SIZE);
                    startingHeldMousePosition.Y = mouseY - (mouseY % BLOCK_SIZE);
                }
            }
            else if (previousMouseState.LeftButton == ButtonState.Pressed && mouseState.LeftButton != ButtonState.Released && isHoldingLeft && !isHoveringOverABlock)
            {
                float width = Math.Max(BLOCK_SIZE, Math.Abs(startingHeldMousePosition.X - mouseX) - (Math.Abs(startingHeldMousePosition.X - mouseX) % BLOCK_SIZE));
                float height = Math.Max(BLOCK_SIZE, Math.Abs(startingHeldMousePosition.Y - mouseY) - (Math.Abs(startingHeldMousePosition.Y - mouseY) % BLOCK_SIZE));

                int drawX = mouseX < startingHeldMousePosition.X
                    ? (int)(startingHeldMousePosition.X - width)
                    : (int)startingHeldMousePosition.X;

                if (mouseX < startingHeldMousePosition.X)
                {
                    width += BLOCK_SIZE;
                }

                int drawY = mouseY < startingHeldMousePosition.Y
                    ? (int)(startingHeldMousePosition.Y - height)
                    : (int)startingHeldMousePosition.Y;

                if (mouseY < startingHeldMousePosition.Y)
                {
                    height += BLOCK_SIZE;
                }

                //wall.sprite.position = new Vector2(drawX, drawY);
                wall.sprite.DrawSize = new Size(width, height);
                //wall.sprite.Update(gameTime);
            }
            else if (mouseState.LeftButton == ButtonState.Released && previousMouseState.LeftButton == ButtonState.Pressed)
            {
                isHoldingLeft = false;

                Wall oldWall = new Wall(graphics);
                oldWall.sprite.DrawSize = wall.sprite.DrawSize;
                oldWall.sprite.position = new Vector2(startingHeldMousePosition.X, startingHeldMousePosition.Y);
                oldWall.sprite.Update(gameTime);

                oldWalls.Add(oldWall);
                startingHeldMousePosition = Vector2.Zero;
                wall.sprite.DrawSize = new Size(BLOCK_SIZE, BLOCK_SIZE);

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
                world.CurrentCameraName = "camera1";
                world.CurrentCamera.Zoom = world.CurrentCamera.Zoom - 1f;
                //grid.resetGrid(world.CurrentCamera.Zoom);
                //BLOCK_SIZE = world.CurrentCamera.Zoom;
                //wall.sprite.DrawSize = new Size(BLOCK_SIZE, BLOCK_SIZE);
            }

            if (Mouse.GetState().ScrollWheelValue > previousMouseState.ScrollWheelValue && BLOCK_SIZE < MAX_BLOCK_SIZE && !isHoldingLeft)
            {          
                world.CurrentCameraName = "camera1";
                world.CurrentCamera.Zoom = world.CurrentCamera.Zoom + 1f;
                //grid.resetGrid(world.CurrentCamera.Zoom);
                //BLOCK_SIZE = world.CurrentCamera.Zoom;
                //wall.sprite.DrawSize = new Size(BLOCK_SIZE, BLOCK_SIZE);
            }

            if (keyboardState.IsKeyDown(Keys.Down) && !isHoldingLeft)
            {
                world.CurrentCameraName = "camera1";
                world.CurrentCamera.Pan = new Vector2(world.CurrentCamera.Pan.X, world.CurrentCamera.Pan.Y + 10);
                world.CurrentCameraName = "minimap";
                world.CurrentCamera.Pan = new Vector2(world.CurrentCamera.Pan.X, world.CurrentCamera.Pan.Y + 10);
                grid.Pan(0, 1);
            }   

            if (keyboardState.IsKeyDown(Keys.Up) && !isHoldingLeft)
            {
                world.CurrentCameraName = "camera1";
                world.CurrentCamera.Pan = new Vector2(world.CurrentCamera.Pan.X, world.CurrentCamera.Pan.Y - 10);
                world.CurrentCameraName = "minimap";
                world.CurrentCamera.Pan = new Vector2(world.CurrentCamera.Pan.X, world.CurrentCamera.Pan.Y - 10);
                grid.Pan(0, -1);
            }

            if (keyboardState.IsKeyDown(Keys.Left) && !isHoldingLeft)
            {
                world.CurrentCameraName = "camera1";
                world.CurrentCamera.Pan = new Vector2(world.CurrentCamera.Pan.X - 10, world.CurrentCamera.Pan.Y);
                world.CurrentCameraName = "minimap";
                world.CurrentCamera.Pan = new Vector2(world.CurrentCamera.Pan.X - 10, world.CurrentCamera.Pan.Y);
                grid.Pan(-1, 0);
            }

            if (keyboardState.IsKeyDown(Keys.Right) && !isHoldingLeft)
            {
                world.CurrentCameraName = "camera1";
                world.CurrentCamera.Pan = new Vector2(world.CurrentCamera.Pan.X + 10, world.CurrentCamera.Pan.Y);
                world.CurrentCameraName = "minimap";
                world.CurrentCamera.Pan = new Vector2(world.CurrentCamera.Pan.X + 10, world.CurrentCamera.Pan.Y);
                grid.Pan(1, 0);
            }

            if (keyboardState.IsKeyDown(Keys.G) && previousKeyboardState.IsKeyUp(Keys.G)) 
            {
                if (isGridOn)
                {
                    grid.Hide();
                }
                else
                {
                    grid.Show();
                }
            }

            circle.Update(gameTime);
            
            if (!isHoldingLeft)
            {
                wall.sprite.position = new Vector2(mouseX - (mouseX % BLOCK_SIZE), mouseY - (mouseY % BLOCK_SIZE));
            }
            wall.Update(gameTime);
            cursorOutline.sprite.color = Color.Red;
            cursorOutline.sprite.position = new Vector2(wall.sprite.position.X - 1, wall.sprite.position.Y - 1);
            cursorOutline.sprite.DrawSize = new Size(wall.sprite.DrawSize.Width + 2, wall.sprite.DrawSize.Height + 2);
            cursorOutline.Update(gameTime);

            hoveredOutline.sprite.DrawSize = new Size(0, 0);
            isHoveringOverABlock = false;
            hoveredBlock = new Wall(graphics);
            List<Wall> reversedWalls = oldWalls;
            reversedWalls.Reverse();
            foreach (Wall oldWall in reversedWalls)
            {
                if (isHoveringOverBlock(oldWall) && !isHoldingLeft)
                {
                    isHoveringOverABlock = true;
                    hoveredBlock = oldWall;
                    hoveredOutline.sprite.color = Color.Red;
                    hoveredOutline.sprite.position = new Vector2(oldWall.sprite.position.X - 1, oldWall.sprite.position.Y - 1);
                    hoveredOutline.sprite.DrawSize = new Size(oldWall.sprite.DrawSize.Width + 2, oldWall.sprite.DrawSize.Height + 2);                 
                }
            }

            hoveredOutline.Update(gameTime);

            previousKeyboardState = keyboardState;
            previousMouseState = mouseState;

            world.UpdateCurrentCamera(gameTime);
            world.CurrentCameraName = "minimap";
            world.UpdateCurrentCamera(gameTime);
            world.CurrentCameraName = "camera1";

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

            world.DrawWorld();

            base.Draw(gameTime);
        }

        public void MainDraw()
        {
            world.CurrentCameraName = "camera1";
            world.BeginDraw();
            world.Draw(grid.Draw);
            foreach (Wall oldWall in oldWalls)
            {
                world.Draw((spriteBatch) => { oldWall.Draw(spriteBatch, world, BLOCK_SIZE); });
            }
            world.Draw((spriteBatch) => { hoveredOutline.Draw(spriteBatch, world, BLOCK_SIZE); });
            world.Draw((spriteBatch) => { hoveredBlock.Draw(spriteBatch, world, BLOCK_SIZE); });
            world.Draw((spriteBatch) => { selectedOutline.Draw(spriteBatch, world, BLOCK_SIZE); });
            world.Draw((spriteBatch) => { selectedBlock.Draw(spriteBatch, world, BLOCK_SIZE); });
            world.Draw((spriteBatch) => { cursorOutline.Draw(spriteBatch, world, BLOCK_SIZE); });
            world.Draw((spriteBatch) => { wall.Draw(spriteBatch, world, BLOCK_SIZE); });
            world.EndDraw();
            DrawMinimap();
        }

        public void DrawMinimap()
        {
            world.CurrentCameraName = "minimap";
            world.BeginDraw();
            world.Draw((spriteBatch) => { minimapBackground.Draw(spriteBatch, world, BLOCK_SIZE); });
            foreach (Wall oldWall in oldWalls)
            {
                world.Draw((spriteBatch) => { oldWall.Draw(spriteBatch, world, BLOCK_SIZE); });
            }
            world.EndDraw();
        }
    }
}
