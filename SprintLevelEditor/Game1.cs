using System;
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

        private enum EditorState
        {
            SHAPE_START,
            SHAPE_DRAWING,
            SHAPE_SELECTED
        }

        public float BLOCK_SIZE = 10;
        public static int MAX_BLOCK_SIZE = 50;
        public static int MIN_BLOCK_SIZE = 3;
        public int MOVE_SPEED = 10;
        public static int SCREEN_WIDTH = 1800;
        public static int SCREEN_HEIGHT = 1000;

        EditorState editorState;
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        GameTimeWrapper mainGameTime;
        World world;
        Circle circle;
        Wall wall;
        List<Wall> oldWalls;
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
<<<<<<< Updated upstream
            redoQueue = new List<Wall>();
            grid = new List<Line>();
=======
            isBlockSelected = false;
            isHoveringOverABlock = false;
            editorState = EditorState.SHAPE_START;
>>>>>>> Stashed changes
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

            mainGameTime = new GameTimeWrapper(MainUpdate, this, 1);
            world.AddGameState(MainGame, graphics);
            world.gameStates[MainGame].AddTime(mainGameTime);
            world.gameStates[MainGame].AddDraw(MainDraw);
            world.ActivateGameState(MainGame);

            wall = new Wall(graphics);
            wall.sprite.DrawSize = new Size(BLOCK_SIZE, BLOCK_SIZE);
            minimapBackground = new Wall(graphics);
            minimapBackground.sprite.color = Color.Black;
            minimapBackground.sprite.DrawSize = new Size(99999, 99999);

            mainCamera = new Camera(world.virtualResolutionRenderer, Camera.CameraFocus.Center);
            world.AddCamera("mainCamera", mainCamera);

            minimapVirtualResolutionRenderer = new VirtualResolutionRenderer(graphics, new Size(SCREEN_WIDTH, SCREEN_HEIGHT), new Size(SCREEN_WIDTH / 10, SCREEN_HEIGHT / 10));
            minimapVirtualResolutionRenderer.BackgroundColor = Color.Black;
            minimapCamera = new Camera(minimapVirtualResolutionRenderer, Camera.CameraFocus.TopLeft);
            minimapCamera.Zoom = .05f;
            world.AddCamera("minimap", minimapCamera);

            world.CurrentCameraName = "mainCamera";

            makeGrid();
        }

        public void makeGrid()
        {
            grid = new List<Line>();
            // grid
            foreach (int i in Enumerable.Range(1, SCREEN_WIDTH / BLOCK_SIZE))
            {
                Vector2 start = new Vector2(i * BLOCK_SIZE, 0);
                Vector2 end = new Vector2(i * BLOCK_SIZE, SCREEN_HEIGHT);
                Line line = new Line(graphics, Line.Type.Point, start, end, 1);
                grid.Add(line);
            }

            foreach (int j in Enumerable.Range(1, SCREEN_HEIGHT / BLOCK_SIZE))
            {
                Vector2 start = new Vector2(0, j * BLOCK_SIZE);
                Vector2 end = new Vector2(SCREEN_WIDTH, j * BLOCK_SIZE);
                Line line = new Line(graphics, Line.Type.Point, start, end, 1);
                grid.Add(line);
            }
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

        public void startFresh()
        {
            oldWalls = new List<Wall>();
        }

        public void MainUpdate(GameTimeWrapper gameTime)
        {
            world.UpdateCurrentCamera(gameTime);
            world.CurrentCameraName = "minimap";
            world.UpdateCurrentCamera(gameTime);
            world.CurrentCameraName = "mainCamera";
            keyboardState = Keyboard.GetState();
            MouseState mouseState = Mouse.GetState();
            int mouseX = mouseState.Position.X;
            int mouseY = mouseState.Position.Y;

<<<<<<< Updated upstream
            if (Mouse.GetState().LeftButton == ButtonState.Pressed && previousMouseState.LeftButton == ButtonState.Released)
            {
                isHoldingLeft = true;
                startingHeldMousePosition = Mouse.GetState().Position.ToVector2();
                startingHeldMousePosition.X = startingHeldMousePosition.X - (startingHeldMousePosition.X % BLOCK_SIZE);
                startingHeldMousePosition.Y = startingHeldMousePosition.Y - (startingHeldMousePosition.Y % BLOCK_SIZE);
            }
            else if (previousMouseState.LeftButton == ButtonState.Pressed && mouseState.LeftButton != ButtonState.Released && isHoldingLeft)
=======
            if (editorState == EditorState.SHAPE_SELECTED)
            {
                wall.sprite.position = new Vector2(mouseX - (mouseX % BLOCK_SIZE), mouseY - (mouseY % BLOCK_SIZE));
                selectedOutline.sprite.position = new Vector2(wall.sprite.position.X - 2, wall.sprite.position.Y - 2);
                selectedOutline.Update(gameTime);
            }

            if (mouseState.LeftButton == ButtonState.Pressed && previousMouseState.LeftButton == ButtonState.Released)
            {
                if (isHoveringOverABlock)
                {
                    if (hoveredBlock == selectedBlock && editorState == EditorState.SHAPE_SELECTED)
                    {
                        editorState = EditorState.SHAPE_START;
                        selectedBlock = new Wall(graphics);
                        selectedOutline.sprite.DrawSize = new Size();
                    }
                    else
                    {
                        editorState = EditorState.SHAPE_SELECTED;
                        wall.sprite.position = hoveredBlock.sprite.position;
                        wall.sprite.DrawSize = hoveredBlock.sprite.DrawSize;
                        selectedOutline = new Wall(graphics);
                        selectedOutline.sprite.color = Color.DarkRed;
                        selectedOutline.sprite.position = new Vector2(hoveredOutline.sprite.position.X - 1, hoveredOutline.sprite.position.Y - 1);
                        selectedOutline.sprite.DrawSize = new Size(hoveredOutline.sprite.DrawSize.Width + 2, hoveredOutline.sprite.DrawSize.Height + 2);
                        Vector2 newCursorPosition = world.CurrentCamera.MouseToScreenCoords(new Point((int) hoveredBlock.sprite.position.X, (int) hoveredBlock.sprite.position.Y));
                        Mouse.SetPosition((int) hoveredBlock.sprite.position.X, (int) hoveredBlock.sprite.position.Y);
                        cursorOutline.sprite.DrawSize = new Size();
                        oldWalls.Remove(hoveredBlock);
                    }

                    selectedOutline.Update(gameTime);
                }
                else if (editorState == EditorState.SHAPE_SELECTED)
                {
                    editorState = EditorState.SHAPE_START;
                    oldWalls.Add(wall);
                    Wall newWall = new Wall(graphics);
                    newWall.sprite.position = wall.sprite.position;
                    newWall.sprite.DrawSize = new Size(BLOCK_SIZE, BLOCK_SIZE);
                    wall = newWall;
                    selectedOutline.sprite.DrawSize = new Size();
                }
                else
                {
                    editorState = EditorState.SHAPE_DRAWING;
                    startingHeldMousePosition = new Vector2();
                    startingHeldMousePosition.X = mouseX - (mouseX % BLOCK_SIZE);
                    startingHeldMousePosition.Y = mouseY - (mouseY % BLOCK_SIZE);
                }
            }
            else if (previousMouseState.LeftButton == ButtonState.Pressed && mouseState.LeftButton != ButtonState.Released && !isHoveringOverABlock && editorState == EditorState.SHAPE_DRAWING)
>>>>>>> Stashed changes
            {
                float width = Math.Max(BLOCK_SIZE, Math.Abs(startingHeldMousePosition.X - Mouse.GetState().Position.X) - (Math.Abs(startingHeldMousePosition.X - Mouse.GetState().Position.X) % BLOCK_SIZE));
                float height = Math.Max(BLOCK_SIZE, Math.Abs(startingHeldMousePosition.Y - Mouse.GetState().Position.Y) - (Math.Abs(startingHeldMousePosition.Y - Mouse.GetState().Position.Y) % BLOCK_SIZE));

                int drawX = Mouse.GetState().Position.X < startingHeldMousePosition.X
                    ? (int)(startingHeldMousePosition.X - width)
                    : (int)startingHeldMousePosition.X;

                if (Mouse.GetState().Position.X < startingHeldMousePosition.X)
                {
                    width += BLOCK_SIZE;
                }

                int drawY = Mouse.GetState().Position.Y < startingHeldMousePosition.Y
                    ? (int)(startingHeldMousePosition.Y - height)
                    : (int)startingHeldMousePosition.Y;

                if (Mouse.GetState().Position.Y < startingHeldMousePosition.Y)
                {
                    height += BLOCK_SIZE;
                }

                wall.sprite.position = new Vector2(drawX, drawY);
                wall.sprite.DrawSize = new Size(width, height);
                wall.sprite.Update(gameTime);
            }
<<<<<<< Updated upstream
            else if (Mouse.GetState().LeftButton == ButtonState.Released && previousMouseState.LeftButton == ButtonState.Pressed)
=======
            else if (mouseState.LeftButton == ButtonState.Released && previousMouseState.LeftButton == ButtonState.Pressed && editorState == EditorState.SHAPE_DRAWING)
>>>>>>> Stashed changes
            {
                editorState = EditorState.SHAPE_START;

                Wall oldWall = new Wall(graphics);
                oldWall.sprite.DrawSize = wall.sprite.DrawSize;
                oldWall.sprite.position = new Vector2(wall.sprite.position.X, wall.sprite.position.Y);
                oldWall.sprite.Update(gameTime);

                oldWalls.Add(oldWall);
                startingHeldMousePosition = Vector2.Zero;
                wall.sprite.DrawSize = new Size(BLOCK_SIZE, BLOCK_SIZE);
<<<<<<< Updated upstream

                redoQueue = new List<Wall>();
            }
            else if (!isHoldingLeft)
            {
                wall.sprite.position = new Vector2(mouseX - mouseX % BLOCK_SIZE, mouseY - mouseY % BLOCK_SIZE);
            }

            if (keyboardState.IsKeyDown(Keys.LeftControl) && keyboardState.IsKeyDown(Keys.S) && previousKeyboardState.IsKeyUp(Keys.S))
            {
                saveGame();
=======
>>>>>>> Stashed changes
            }

            if (mouseState.RightButton == ButtonState.Pressed)
            {
                if (isHoveringOverABlock)
                {
                    oldWalls.Remove(hoveredBlock);
                }
            }

            if (keyboardState.IsKeyDown(Keys.LeftControl) && keyboardState.IsKeyDown(Keys.S) && previousKeyboardState.IsKeyUp(Keys.S))
            {
                saveGame();
            }

            if (keyboardState.IsKeyDown(Keys.LeftControl) && keyboardState.IsKeyDown(Keys.N) && previousKeyboardState.IsKeyUp(Keys.N))
            {
                startFresh();
            }

<<<<<<< Updated upstream
            if (Mouse.GetState().ScrollWheelValue < previousMouseState.ScrollWheelValue && BLOCK_SIZE > MIN_BLOCK_SIZE && !isHoldingLeft)
            {
                List<Wall> scaledOldWalls = new List<Wall>();
                foreach (Wall oldWall in oldWalls)
                {
                    Wall scaledOldWall = new Wall(graphics);
                    scaledOldWall.sprite.position = new Vector2((oldWall.sprite.position.X / BLOCK_SIZE) * (BLOCK_SIZE - 1),
                        (oldWall.sprite.position.Y / BLOCK_SIZE) * (BLOCK_SIZE - 1));
                    scaledOldWall.sprite.DrawSize = new Size((oldWall.sprite.DrawSize.Width / BLOCK_SIZE) * (BLOCK_SIZE - 1),
                        (oldWall.sprite.DrawSize.Height / BLOCK_SIZE) * (BLOCK_SIZE - 1));
                    scaledOldWall.sprite.Update(gameTime);
                    scaledOldWalls.Add(scaledOldWall);
                }
                oldWalls = scaledOldWalls;

                List<Wall> scaledRedoQueue = new List<Wall>();
                foreach (Wall oldWall in redoQueue)
                {
                    Wall scaledOldWall = new Wall(graphics);
                    scaledOldWall.sprite.position = new Vector2((oldWall.sprite.position.X / BLOCK_SIZE) * (BLOCK_SIZE - 1),
                        (oldWall.sprite.position.Y / BLOCK_SIZE) * (BLOCK_SIZE - 1));
                    scaledOldWall.sprite.DrawSize = new Size((oldWall.sprite.DrawSize.Width / BLOCK_SIZE) * (BLOCK_SIZE - 1),
                        (oldWall.sprite.DrawSize.Height / BLOCK_SIZE) * (BLOCK_SIZE - 1));
                    scaledOldWall.sprite.Update(gameTime);
                    scaledRedoQueue.Add(scaledOldWall);
                }
                redoQueue = scaledRedoQueue;

                BLOCK_SIZE--;
                MOVE_SPEED = BLOCK_SIZE;
                wall.sprite.DrawSize = new Size(BLOCK_SIZE, BLOCK_SIZE);
                if (isGridOn)
                {
                    makeGrid();
                }
            }

            if (Mouse.GetState().ScrollWheelValue > previousMouseState.ScrollWheelValue && BLOCK_SIZE < MAX_BLOCK_SIZE && !isHoldingLeft)
            {
                List<Wall> scaledOldWalls = new List<Wall>();
                foreach (Wall oldWall in oldWalls)
                {
                    Wall scaledOldWall = new Wall(graphics);
                    scaledOldWall.sprite.position = new Vector2((oldWall.sprite.position.X / BLOCK_SIZE) * (BLOCK_SIZE + 1),
                        (oldWall.sprite.position.Y / BLOCK_SIZE) * (BLOCK_SIZE + 1));
                    scaledOldWall.sprite.DrawSize = new Size((oldWall.sprite.DrawSize.Width / BLOCK_SIZE) * (BLOCK_SIZE + 1),
                        (oldWall.sprite.DrawSize.Height / BLOCK_SIZE) * (BLOCK_SIZE + 1));
                    scaledOldWall.sprite.Update(gameTime);
                    scaledOldWalls.Add(scaledOldWall);
                }
                oldWalls = scaledOldWalls;

                List<Wall> scaledRedoQueue = new List<Wall>();
                foreach (Wall oldWall in redoQueue)
                {
                    Wall scaledOldWall = new Wall(graphics);
                    scaledOldWall.sprite.position = new Vector2((oldWall.sprite.position.X / BLOCK_SIZE) * (BLOCK_SIZE + 1),
                        (oldWall.sprite.position.Y / BLOCK_SIZE) * (BLOCK_SIZE + 1));
                    scaledOldWall.sprite.DrawSize = new Size((oldWall.sprite.DrawSize.Width / BLOCK_SIZE) * (BLOCK_SIZE + 1),
                        (oldWall.sprite.DrawSize.Height / BLOCK_SIZE) * (BLOCK_SIZE + 1));
                    scaledOldWall.sprite.Update(gameTime);
                    scaledRedoQueue.Add(scaledOldWall);
                }
                redoQueue = scaledRedoQueue;

                BLOCK_SIZE++;
                MOVE_SPEED = BLOCK_SIZE;
                wall.sprite.DrawSize = new Size(BLOCK_SIZE, BLOCK_SIZE);
                if (isGridOn)
                {
                    makeGrid();
                }
=======
            if (Mouse.GetState().ScrollWheelValue < previousMouseState.ScrollWheelValue && world.CurrentCamera.Zoom > MIN_BLOCK_SIZE && editorState != EditorState.SHAPE_DRAWING)
            {   
                world.CurrentCameraName = "camera1";
                world.CurrentCamera.Zoom = world.CurrentCamera.Zoom - 1f;
            }

            if (Mouse.GetState().ScrollWheelValue > previousMouseState.ScrollWheelValue && world.CurrentCamera.Zoom < MAX_BLOCK_SIZE && editorState != EditorState.SHAPE_DRAWING)
            {          
                world.CurrentCameraName = "camera1";
                world.CurrentCamera.Zoom = world.CurrentCamera.Zoom + 1f;
>>>>>>> Stashed changes
            }

            if (keyboardState.IsKeyDown(Keys.Down) && editorState != EditorState.SHAPE_DRAWING)
            {
                List<Wall> scaledOldWalls = new List<Wall>();
                foreach (Wall oldWall in oldWalls)
                {
                    Wall scaledOldWall = new Wall(graphics);
                    scaledOldWall.sprite.position = new Vector2(oldWall.sprite.position.X, oldWall.sprite.position.Y - MOVE_SPEED);
                    scaledOldWall.sprite.DrawSize = oldWall.sprite.DrawSize;
                    scaledOldWall.sprite.Update(gameTime);
                    scaledOldWalls.Add(scaledOldWall);
                }
                oldWalls = scaledOldWalls;

                List<Wall> scaledRedoQueue = new List<Wall>();
                foreach (Wall oldWall in redoQueue)
                {
                    Wall scaledOldWall = new Wall(graphics);
                    scaledOldWall.sprite.position = new Vector2(oldWall.sprite.position.X, oldWall.sprite.position.Y - MOVE_SPEED);
                    scaledOldWall.sprite.DrawSize = oldWall.sprite.DrawSize;
                    scaledOldWall.sprite.Update(gameTime);
                    scaledRedoQueue.Add(scaledOldWall);
                }
                redoQueue = scaledRedoQueue;
            }

            if (keyboardState.IsKeyDown(Keys.Up) && editorState != EditorState.SHAPE_DRAWING)
            {
                List<Wall> scaledOldWalls = new List<Wall>();
                foreach (Wall oldWall in oldWalls)
                {
                    Wall scaledOldWall = new Wall(graphics);
                    scaledOldWall.sprite.position = new Vector2(oldWall.sprite.position.X, oldWall.sprite.position.Y + MOVE_SPEED);
                    scaledOldWall.sprite.DrawSize = oldWall.sprite.DrawSize;
                    scaledOldWall.sprite.Update(gameTime);
                    scaledOldWalls.Add(scaledOldWall);
                }
                oldWalls = scaledOldWalls;

                List<Wall> scaledRedoQueue = new List<Wall>();
                foreach (Wall oldWall in redoQueue)
                {
                    Wall scaledOldWall = new Wall(graphics);
                    scaledOldWall.sprite.position = new Vector2(oldWall.sprite.position.X, oldWall.sprite.position.Y + MOVE_SPEED);
                    scaledOldWall.sprite.DrawSize = oldWall.sprite.DrawSize;
                    scaledOldWall.sprite.Update(gameTime);
                    scaledRedoQueue.Add(scaledOldWall);
                }
                redoQueue = scaledRedoQueue;
            }

            if (keyboardState.IsKeyDown(Keys.Left) && editorState != EditorState.SHAPE_DRAWING)
            {
                List<Wall> scaledOldWalls = new List<Wall>();
                foreach (Wall oldWall in oldWalls)
                {
                    Wall scaledOldWall = new Wall(graphics);
                    scaledOldWall.sprite.position = new Vector2(oldWall.sprite.position.X + MOVE_SPEED, oldWall.sprite.position.Y);
                    scaledOldWall.sprite.DrawSize = oldWall.sprite.DrawSize;
                    scaledOldWall.sprite.Update(gameTime);
                    scaledOldWalls.Add(scaledOldWall);
                }
                oldWalls = scaledOldWalls;

                List<Wall> scaledRedoQueue = new List<Wall>();
                foreach (Wall oldWall in redoQueue)
                {
                    Wall scaledOldWall = new Wall(graphics);
                    scaledOldWall.sprite.position = new Vector2(oldWall.sprite.position.X + MOVE_SPEED, oldWall.sprite.position.Y);
                    scaledOldWall.sprite.DrawSize = oldWall.sprite.DrawSize;
                    scaledOldWall.sprite.Update(gameTime);
                    scaledRedoQueue.Add(scaledOldWall);
                }
                redoQueue = scaledRedoQueue;
            }

            if (keyboardState.IsKeyDown(Keys.Right) && editorState != EditorState.SHAPE_DRAWING)
            {
                List<Wall> scaledOldWalls = new List<Wall>();
                foreach (Wall oldWall in oldWalls)
                {
                    Wall scaledOldWall = new Wall(graphics);
                    scaledOldWall.sprite.position = new Vector2(oldWall.sprite.position.X - MOVE_SPEED, oldWall.sprite.position.Y);
                    scaledOldWall.sprite.DrawSize = oldWall.sprite.DrawSize;
                    scaledOldWall.sprite.Update(gameTime);
                    scaledOldWalls.Add(scaledOldWall);
                }
                oldWalls = scaledOldWalls;

                List<Wall> scaledRedoQueue = new List<Wall>();
                foreach (Wall oldWall in redoQueue)
                {
                    Wall scaledOldWall = new Wall(graphics);
                    scaledOldWall.sprite.position = new Vector2(oldWall.sprite.position.X - MOVE_SPEED, oldWall.sprite.position.Y);
                    scaledOldWall.sprite.DrawSize = oldWall.sprite.DrawSize;
                    scaledOldWall.sprite.Update(gameTime);
                    scaledRedoQueue.Add(scaledOldWall);
                }
                redoQueue = scaledRedoQueue;
            }

            if (keyboardState.IsKeyDown(Keys.G) && previousKeyboardState.IsKeyUp(Keys.G)) 
            {
                if (isGridOn)
                {
                    grid = new List<Line>();
                    isGridOn = false;
                }
                else
                {
                    makeGrid();
                    isGridOn = true;
                }
            }

            circle.Update(gameTime);
<<<<<<< Updated upstream
            wall.Update(gameTime);
=======
            
            if (editorState == EditorState.SHAPE_START)
            {
                wall.sprite.position = new Vector2(mouseX - (mouseX % BLOCK_SIZE), mouseY - (mouseY % BLOCK_SIZE));
            }
            if (editorState != EditorState.SHAPE_SELECTED)
            {
                cursorOutline.sprite.color = Color.Red;
                cursorOutline.sprite.position = new Vector2(wall.sprite.position.X - 1, wall.sprite.position.Y - 1);
                cursorOutline.sprite.DrawSize = new Size(wall.sprite.DrawSize.Width + 2, wall.sprite.DrawSize.Height + 2);
            }
            wall.Update(gameTime);
            cursorOutline.Update(gameTime);

            hoveredOutline.sprite.DrawSize = new Size(0, 0);
            isHoveringOverABlock = false;
            hoveredBlock = new Wall(graphics);
            List<Wall> reversedWalls = oldWalls;
            reversedWalls.Reverse();
            foreach (Wall oldWall in reversedWalls)
            {
                if (isHoveringOverBlock(oldWall) && editorState == EditorState.SHAPE_START)
                {
                    isHoveringOverABlock = true;
                    hoveredBlock = oldWall;
                    hoveredOutline.sprite.color = Color.DarkRed;
                    hoveredOutline.sprite.position = new Vector2(oldWall.sprite.position.X - 1, oldWall.sprite.position.Y - 1);
                    hoveredOutline.sprite.DrawSize = new Size(oldWall.sprite.DrawSize.Width + 2, oldWall.sprite.DrawSize.Height + 2);                 
                }
            }

            hoveredOutline.Update(gameTime);
>>>>>>> Stashed changes

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

            world.DrawWorld();

            base.Draw(gameTime);
        }

        public void MainDraw()
        {
            world.CurrentCameraName = "mainCamera";
            world.BeginDraw();
            foreach (Line line in grid)
            {
                world.Draw(line.Draw);
            }
            foreach (Wall oldWall in oldWalls)
            {
                world.Draw(oldWall.Draw);
            }
            world.Draw(wall.Draw);
            world.EndDraw();
            world.CurrentCameraName = "minimap";
            world.BeginDraw();
            world.Draw(minimapBackground.Draw);
            /*foreach (Line line in grid)
            {
                world.Draw(line.Draw);
            }*/
            foreach (Wall oldWall in oldWalls)
            {
                world.Draw(oldWall.Draw);
            }
            world.EndDraw();
        }
    }
}
