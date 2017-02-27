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

        private enum SelectedShape
        {
            RECTANGLE,
            TRIANGLE,
            START_POINT,
            END_POINT
        }

        public float BLOCK_SIZE = 10;
        public static int MAX_BLOCK_SIZE = 50;
        public static int MIN_BLOCK_SIZE = 3;
        public int MOVE_SPEED = 10;
        public static int SCREEN_WIDTH = 1800;
        public static int SCREEN_HEIGHT = 1000;

        bool shouldIgnoreOneLeftClick;
        EditorState editorState;
        SelectedShape selectedShape;
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        GameTimeWrapper mainGameTime;
        World world;
        Circle circle;
        Wall wall;
        Wall cursorOutline;
        List<Wall> oldWalls;
        List<Triangle> oldTriangles;
        Triangle currentTriangle;
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
        Camera UICamera;
        VirtualResolutionRenderer minimapVirtualResolutionRenderer;
        VirtualResolutionRenderer UIVirtualResolutionRenderer;
        Wall minimapBackground;
        Wall hoveredOutline;
        bool isBlockSelected;
        bool isHoveringOverABlock;
        Wall hoveredBlock;
        Wall selectedBlock;
        Wall selectedOutline;
        Menu menu;
        Marker startPoint;
        Marker endPoint;
        bool justClickedButton;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferWidth = SCREEN_WIDTH;
            graphics.PreferredBackBufferHeight = SCREEN_HEIGHT;
            graphics.PreferMultiSampling = true;
            Content.RootDirectory = "Content";
            isHoldingLeft = false;
            isHoldingRight = false;
            startingHeldMousePosition = Vector2.Zero;
            oldWalls = new List<Wall>();
            isBlockSelected = false;
            isHoveringOverABlock = false;
            editorState = EditorState.SHAPE_START;
            selectedShape = SelectedShape.RECTANGLE;
            shouldIgnoreOneLeftClick = false;
            justClickedButton = false;
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

        public void selectRectangle()
        {
            selectedShape = SelectedShape.RECTANGLE;
            startPoint.Hide();
            endPoint.Hide();
            justClickedButton = true;
        }

        public void selectTriangle()
        {
            selectedShape = SelectedShape.TRIANGLE;
            startPoint.Hide();
            endPoint.Hide();
            currentTriangle.DrawSize = new Size(10, 10);
            justClickedButton = true;
        }

        public void selectStartPoint()
        {
            selectedShape = SelectedShape.START_POINT;
            startPoint.Show();
            endPoint.Hide();
            justClickedButton = true;
        }

        public void selectEndPoint()
        {
            selectedShape = SelectedShape.END_POINT;
            startPoint.Hide();
            endPoint.Show();
            justClickedButton = true;
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

            MenuButton rectangleButton = new MenuButton(Content.Load<Texture2D>("button-rectangle"), Content.Load<Texture2D>("button-rectangle-hover"), Content.Load<Texture2D>("button-rectangle-selected"), selectRectangle);
            rectangleButton.position = new Vector2(405, 20);
            MenuButton triangleButton = new MenuButton(Content.Load<Texture2D>("button-triangle"), Content.Load<Texture2D>("button-triangle-hover"), Content.Load<Texture2D>("button-triangle-selected"), selectTriangle);
            triangleButton.position = new Vector2(505, 20);
            MenuButton startButton = new MenuButton(Content.Load<Texture2D>("button-start"), selectStartPoint);
            startButton.position = new Vector2(605, 20);
            MenuButton endButton = new MenuButton(Content.Load<Texture2D>("button-end"), selectEndPoint);
            endButton.position = new Vector2(705, 20);
            List<MenuButton> buttons = new List<MenuButton> { rectangleButton, triangleButton, startButton, endButton };

            menu = new Menu(buttons);

            startPoint = new Marker(Content.Load<Texture2D>("start"));
            endPoint = new Marker(Content.Load<Texture2D>("end"));

            grid = new Grid(graphics, SCREEN_WIDTH, SCREEN_HEIGHT, BLOCK_SIZE);

            mainGameTime = new GameTimeWrapper(MainUpdate, this, 1);
            world.AddGameState(MainGame, graphics);
            world.gameStates[MainGame].AddTime(mainGameTime);
            world.gameStates[MainGame].AddDraw(MainDraw);
            world.ActivateGameState(MainGame);

            hoveredBlock = new Wall(graphics);
            selectedBlock = new Wall(graphics);

            currentTriangle = new Triangle(Content.Load<Texture2D>("triangle"));
            currentTriangle.DrawSize = new Size(10, 10);
            oldTriangles = new List<Triangle>();

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
            minimapCamera.Zoom = .5f;
            world.AddCamera("minimap", minimapCamera);

            world.CurrentCameraName = "camera1";
            world.CurrentCamera.Focus = Camera.CameraFocus.Center;
            world.CurrentCamera.Zoom = 10f;

            UIVirtualResolutionRenderer = new VirtualResolutionRenderer(graphics, new Size(SCREEN_WIDTH, SCREEN_HEIGHT), new Size(SCREEN_WIDTH, SCREEN_HEIGHT));
            UIVirtualResolutionRenderer.BackgroundColor = Color.Transparent;
            UICamera = new Camera(UIVirtualResolutionRenderer, Camera.CameraFocus.Center);
            world.AddCamera("UI", UICamera);

            world.CurrentCameraName = "camera1";

        }

        public void loadGame()
        {
            System.Windows.Forms.OpenFileDialog openFileDialog = new System.Windows.Forms.OpenFileDialog();
            openFileDialog.Filter = ".json file (*.json)|*.json";
            openFileDialog.FilterIndex = 2;
            openFileDialog.RestoreDirectory = true;
            openFileDialog.Title = "Open a level";
            openFileDialog.ShowDialog();

            if (openFileDialog.FileName != "")
            {
                FileStream fs = (FileStream)openFileDialog.OpenFile();
                long levelSize = fs.Length;
                Byte[] levelDataRaw = new Byte[levelSize];
                fs.Read(levelDataRaw, 0, (int) levelSize);
                String levelDataString = levelDataRaw.ToString();
                List<SimpleRectangle> levelData = JsonConvert.DeserializeObject<List<SimpleRectangle>>(System.Text.Encoding.Default.GetString(levelDataRaw));

                this.oldWalls = new List<Wall>();

                foreach (SimpleRectangle rectangle in levelData)
                {
                    this.oldWalls.Add(rectangle.toWall(graphics, BLOCK_SIZE));
                }
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
                GameSave save = new GameSave(oldWalls, startPoint, endPoint, BLOCK_SIZE);

                string json = JsonConvert.SerializeObject(save, Formatting.Indented);
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
            bool isHoveringOverAnyButton = menu.isHoveringOverAnyButton();

            if (editorState == EditorState.SHAPE_SELECTED)
            {
                wall.sprite.position = new Vector2(mouseX - (mouseX % BLOCK_SIZE), mouseY - (mouseY % BLOCK_SIZE));
                selectedOutline.sprite.position = new Vector2(wall.sprite.position.X - 2, wall.sprite.position.Y - 2);
                selectedOutline.Update(gameTime);
            }

            if (shouldIgnoreOneLeftClick)
            {
                shouldIgnoreOneLeftClick = false;
            }
            else if (mouseState.LeftButton == ButtonState.Pressed && previousMouseState.LeftButton == ButtonState.Released)
            {
                if (isHoveringOverAnyButton)
                {
                    menu.clickHoveredButton();
                }
                else if (isHoveringOverABlock)
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
                    if (selectedShape == SelectedShape.RECTANGLE)
                    {
                        editorState = EditorState.SHAPE_DRAWING;
                        startingHeldMousePosition = new Vector2();
                        startingHeldMousePosition.X = mouseX - (mouseX % BLOCK_SIZE);
                        startingHeldMousePosition.Y = mouseY - (mouseY % BLOCK_SIZE);
                    }
                    else if (selectedShape == SelectedShape.TRIANGLE)
                    {
                        editorState = EditorState.SHAPE_DRAWING;
                        startingHeldMousePosition = new Vector2();
                        startingHeldMousePosition.X = mouseX - (mouseX % BLOCK_SIZE);
                        startingHeldMousePosition.Y = mouseY - (mouseY % BLOCK_SIZE);
                    }
                }
            }
            else if (previousMouseState.LeftButton == ButtonState.Pressed && mouseState.LeftButton != ButtonState.Released && !isHoveringOverABlock && editorState == EditorState.SHAPE_DRAWING)
            {
                if (selectedShape == SelectedShape.RECTANGLE)
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

                    wall.sprite.DrawSize = new Size(width, height);
                    wall.sprite.position = new Vector2(drawX, drawY);
                }
                else if (selectedShape == SelectedShape.TRIANGLE)
                {
                    float width = Math.Max(BLOCK_SIZE * 2, Math.Abs(startingHeldMousePosition.X - mouseX) - (Math.Abs(startingHeldMousePosition.X - mouseX) % (BLOCK_SIZE*2)));
                    float height = Math.Max(BLOCK_SIZE * 2, Math.Abs(startingHeldMousePosition.Y - mouseY) - (Math.Abs(startingHeldMousePosition.Y - mouseY) % (BLOCK_SIZE*2)));

                    int drawX = (int)startingHeldMousePosition.X;

                    if (mouseX < startingHeldMousePosition.X)
                    {
                        //width += BLOCK_SIZE;
                        currentTriangle.horizontalFlip = false;
                    } else
                    {
                        currentTriangle.horizontalFlip = true;
                    }

                    int drawY = (int)startingHeldMousePosition.Y;

                    if (mouseY < startingHeldMousePosition.Y)
                    {
                        //height += BLOCK_SIZE;
                        currentTriangle.verticalFlip = false;
                    }
                    else
                    {
                        currentTriangle.verticalFlip = true;
                    }

                    currentTriangle.DrawSize = new Size(width, height);
                    wall.sprite.position = new Vector2(drawX, drawY);
                }
            }
            else if (mouseState.LeftButton == ButtonState.Released && previousMouseState.LeftButton == ButtonState.Pressed && (editorState == EditorState.SHAPE_DRAWING || selectedShape == SelectedShape.START_POINT || selectedShape == SelectedShape.END_POINT || selectedShape == SelectedShape.TRIANGLE))
            {
                if (justClickedButton)
                {
                    justClickedButton = false;
                }
                else
                {
                    editorState = EditorState.SHAPE_START;

                    if (selectedShape == SelectedShape.RECTANGLE)
                    {
                        Wall oldWall = new Wall(graphics);
                        oldWall.sprite.DrawSize = wall.sprite.DrawSize;
                        oldWall.sprite.position = new Vector2(wall.sprite.position.X, wall.sprite.position.Y);
                        oldWall.sprite.Update(gameTime);

                        oldWalls.Add(oldWall);
                        startingHeldMousePosition = Vector2.Zero;
                        wall.sprite.DrawSize = new Size(BLOCK_SIZE, BLOCK_SIZE);
                    }
                    else if (selectedShape == SelectedShape.START_POINT)
                    {
                        startPoint.Place();
                        selectedShape = SelectedShape.RECTANGLE;
                    }
                    else if (selectedShape == SelectedShape.END_POINT)
                    {
                        endPoint.Place();
                        selectedShape = SelectedShape.RECTANGLE;
                    }
                    else if (selectedShape == SelectedShape.TRIANGLE)
                    {
                        Triangle oldTriangle = new Triangle(Content.Load<Texture2D>("triangle"));
                        oldTriangle.DrawSize = currentTriangle.DrawSize;
                        oldTriangle.position = new Vector2(currentTriangle.position.X, currentTriangle.position.Y);
                        oldTriangle.verticalFlip = currentTriangle.verticalFlip;
                        oldTriangle.horizontalFlip = currentTriangle.horizontalFlip;
                        oldTriangle.scale = currentTriangle.scale;
                        oldTriangle.Update(gameTime);

                        oldTriangles.Add(oldTriangle);
                        startingHeldMousePosition = Vector2.Zero;
                        currentTriangle.DrawSize = new Size(BLOCK_SIZE, BLOCK_SIZE);
                    }
                }
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

            if (keyboardState.IsKeyDown(Keys.LeftControl) && keyboardState.IsKeyDown(Keys.O) && previousKeyboardState.IsKeyUp(Keys.O))
            {
                loadGame();
            }

            if (keyboardState.IsKeyDown(Keys.LeftControl) && keyboardState.IsKeyDown(Keys.N) && previousKeyboardState.IsKeyUp(Keys.N))
            {
                startFresh();
            }

            if (Mouse.GetState().ScrollWheelValue < previousMouseState.ScrollWheelValue && world.CurrentCamera.Zoom > MIN_BLOCK_SIZE && editorState != EditorState.SHAPE_DRAWING)
            {   
                world.CurrentCameraName = "camera1";
                world.CurrentCamera.Zoom = world.CurrentCamera.Zoom - 1f;
            }

            if (Mouse.GetState().ScrollWheelValue > previousMouseState.ScrollWheelValue && world.CurrentCamera.Zoom < MAX_BLOCK_SIZE && editorState != EditorState.SHAPE_DRAWING)
            {          
                world.CurrentCameraName = "camera1";
                world.CurrentCamera.Zoom = Math.Min(15f, world.CurrentCamera.Zoom + 1f);
            }

            if (keyboardState.IsKeyDown(Keys.Down) && editorState != EditorState.SHAPE_DRAWING)
            {
                world.CurrentCameraName = "camera1";
                world.CurrentCamera.Pan = new Vector2(world.CurrentCamera.Pan.X, world.CurrentCamera.Pan.Y + (2f * (16f - world.CurrentCamera.Zoom)));
            }   

            if (keyboardState.IsKeyDown(Keys.Up) && editorState != EditorState.SHAPE_DRAWING)
            {
                world.CurrentCameraName = "camera1";
                world.CurrentCamera.Pan = new Vector2(world.CurrentCamera.Pan.X, world.CurrentCamera.Pan.Y - (2f * (16f - world.CurrentCamera.Zoom)));
            }

            if (keyboardState.IsKeyDown(Keys.Left) && editorState != EditorState.SHAPE_DRAWING)
            {
                world.CurrentCameraName = "camera1";
                world.CurrentCamera.Pan = new Vector2(world.CurrentCamera.Pan.X - (2f * (16f - world.CurrentCamera.Zoom)), world.CurrentCamera.Pan.Y);
            }

            if (keyboardState.IsKeyDown(Keys.Right) && editorState != EditorState.SHAPE_DRAWING)
            {
                world.CurrentCameraName = "camera1";
                world.CurrentCamera.Pan = new Vector2(world.CurrentCamera.Pan.X + (2f * (16f - world.CurrentCamera.Zoom)), world.CurrentCamera.Pan.Y);
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
            
            if (editorState == EditorState.SHAPE_START)
            {
                wall.sprite.position = new Vector2(mouseX - (mouseX % BLOCK_SIZE), mouseY - (mouseY % BLOCK_SIZE));
            }

            if (selectedShape == SelectedShape.TRIANGLE)
            {
                currentTriangle.position = wall.sprite.position;
            }

            if (editorState != EditorState.SHAPE_SELECTED)
            {
                cursorOutline.sprite.color = Color.Red;
                cursorOutline.sprite.position = new Vector2(wall.sprite.position.X - 1, wall.sprite.position.Y - 1);
                cursorOutline.sprite.DrawSize = new Size(wall.sprite.DrawSize.Width + 2, wall.sprite.DrawSize.Height + 2);
            }
            wall.Update(gameTime);
            cursorOutline.Update(gameTime);
            currentTriangle.Update(gameTime);

            hoveredOutline.sprite.DrawSize = new Size(0, 0);
            isHoveringOverABlock = false;
            hoveredBlock = new Wall(graphics);
            List<Wall> reversedWalls = oldWalls;
            reversedWalls.Reverse();
            foreach (Wall oldWall in reversedWalls)
            {
                if (isHoveringOverBlock(oldWall) && editorState == EditorState.SHAPE_START && selectedShape == SelectedShape.RECTANGLE)
                {
                    isHoveringOverABlock = true;
                    hoveredBlock = oldWall;
                    hoveredOutline.sprite.color = Color.DarkRed;
                    hoveredOutline.sprite.position = new Vector2(oldWall.sprite.position.X - 1, oldWall.sprite.position.Y - 1);
                    hoveredOutline.sprite.DrawSize = new Size(oldWall.sprite.DrawSize.Width + 2, oldWall.sprite.DrawSize.Height + 2);                 
                }
            }

            if (!startPoint.isPlaced)
            {
                startPoint.position = wall.sprite.position;
            }

            if (!endPoint.isPlaced)
            {
                endPoint.position = wall.sprite.position;
            }

            hoveredOutline.Update(gameTime);

            previousKeyboardState = keyboardState;
            previousMouseState = mouseState;

            world.UpdateCurrentCamera(gameTime);
            world.CurrentCameraName = "minimap";
            world.UpdateCurrentCamera(gameTime);
            world.CurrentCameraName = "camera1";

            menu.Update(gameTime);

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

            if (!this.IsActive)
            {
                shouldIgnoreOneLeftClick = true;
                return;
            }

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
            if (!this.IsActive)
            {
                return;
            }

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
            foreach (Triangle triangle in oldTriangles)
            {
                world.Draw(triangle.Draw);
            }
            world.Draw((spriteBatch) => { hoveredOutline.Draw(spriteBatch, world, BLOCK_SIZE); });
            world.Draw((spriteBatch) => { hoveredBlock.Draw(spriteBatch, world, BLOCK_SIZE); });
            world.Draw((spriteBatch) => { selectedOutline.Draw(spriteBatch, world, BLOCK_SIZE); });
            world.Draw((spriteBatch) => { selectedBlock.Draw(spriteBatch, world, BLOCK_SIZE); });
            if (selectedShape == SelectedShape.RECTANGLE)
            {
                world.Draw((spriteBatch) => { cursorOutline.Draw(spriteBatch, world, BLOCK_SIZE); });
                world.Draw((spriteBatch) => { wall.Draw(spriteBatch, world, BLOCK_SIZE); });
            }
            else if (selectedShape == SelectedShape.TRIANGLE)
            {
                world.Draw(currentTriangle.Draw);
            }
            world.Draw(startPoint.Draw);
            world.Draw(endPoint.Draw);
            
            world.EndDraw();
            DrawMinimap();
            DrawUI();
        }

        public void DrawUI()
        {
            world.CurrentCameraName = "UI";
            world.BeginDraw();
            world.Draw((spriteBatch) => { menu.Draw(spriteBatch, world); });
            world.EndDraw();
        }

        public void DrawMinimap()
        {
            world.CurrentCameraName = "minimap";
            world.BeginDraw(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, new RasterizerState(), null, world.CurrentCamera.Transform);
            world.Draw((spriteBatch) => { minimapBackground.Draw(spriteBatch, world, BLOCK_SIZE); });
            foreach (Wall oldWall in oldWalls)
            {
                world.Draw((spriteBatch) => { oldWall.Draw(spriteBatch, world, BLOCK_SIZE); });
            }
            world.EndDraw();
        }
    }
}
