// Copyright (c) 2010-2013 SharpDX - Alexandre Mutel
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.


using SharpDX;
using SharpDX.Toolkit;
using System;
using System.Collections.Generic;
using Windows.UI.Input;
using Windows.UI.Core;
using Windows.Devices.Sensors;

using SharpDX.Direct3D;
using SharpDX.Direct3D11;


namespace Project
{
    // Use this namespace here in case we need to use Direct3D11 namespace as well, as this
    // namespace will override the Direct3D11.
    using SharpDX.Toolkit.Graphics;
    using SharpDX.Toolkit.Input;

    public class ProjectGame : Game
    {
        private GraphicsDeviceManager graphicsDeviceManager;
        public KeyboardManager keyboardManager;
        public KeyboardState keyboardState;
        public MouseManager mouseManager;
        public MouseState mouseState;
        public AccelerometerReading accelerometerReading;
        public GameInput input;
        public MainPage mainPage;
        public SpriteBatch spriteBatch;

        public LightManager lightManager;

        public List<GameObject> gameObjects;
        private Stack<GameObject> addedGameObjects;
        private Stack<GameObject> removedGameObjects;


        public Player player;
        public int score;


        // Use this to represent difficulty
        public float difficulty;

        // Represents the camera's position and orientation
        public Camera camera;

        // Random number generator
        public Random random;

        // Lower boundary used to end the game
        public float lower_bound = -60.0f;

        // Game gravity
        public float gravity = -100.0f;

        // Player initial position
        public Vector3 init_pos;

        public bool started = false;
        /// <summary>
        /// Initializes a new instance of the <see cref="ProjectGame" /> class.
        /// </summary>
        public ProjectGame(MainPage mainPage)
        {
            // Creates a graphics manager. This is mandatory.
            graphicsDeviceManager = new GraphicsDeviceManager(this);
            //graphicsDeviceManager.PreferredGraphicsProfile = new FeatureLevel[] { FeatureLevel.Level_10_0, };
            graphicsDeviceManager.PreferredGraphicsProfile = new FeatureLevel[] { FeatureLevel.Level_10_1, };

            // Setup the relative directory to the executable directory
            // for loading contents with the ContentManager
            Content.RootDirectory = "Content";

            // Create the keyboard manager
            keyboardManager = new KeyboardManager(this);
            random = new Random();
            input = new GameInput();

            // Initialise event handling.
            input.gestureRecognizer.Tapped += Tapped;
            input.gestureRecognizer.ManipulationStarted += OnManipulationStarted;
            input.gestureRecognizer.ManipulationUpdated += OnManipulationUpdated;
            input.gestureRecognizer.ManipulationCompleted += OnManipulationCompleted;

            this.mainPage = mainPage;
            difficulty = 1;
            score = 0;
        }

        protected override void OnExiting(object sender, EventArgs args)
        {
            this.UnloadContent();
            this.Dispose();
        }

        protected override void LoadContent()
        {
            SetGameObjects();

            base.LoadContent();
        }

        void SetGameObjects()
        {
            // Initialise game object containers.
            gameObjects = new List<GameObject>();
            addedGameObjects = new Stack<GameObject>();
            removedGameObjects = new Stack<GameObject>();

            lightManager = new LightManager(this);

            // Create the first platform and get the height
            Platform first_platform = new Platform(this);
            int level = first_platform.platform[2, 0];
            init_pos = new Vector3(first_platform.platfom_midpoint, first_platform.Levels[level], 0);

            //Number of platforms to render on screen at any time
            gameObjects.Add(first_platform);
            gameObjects.Add(new Platform(this));
            gameObjects.Add(new Platform(this));
            gameObjects.Add(new Platform(this));
            gameObjects.Add(new Platform(this));
            gameObjects.Add(new Platform(this));
            gameObjects.Add(new Platform(this));
            gameObjects.Add(new Platform(this));

            // Create the player
            player = new Player(this, init_pos);
            gameObjects.Add(player);

            // Create game objects.
            gameObjects.Add(new EnemyController(this));

            spriteBatch = ToDisposeContent(new SpriteBatch(GraphicsDevice));
        }

        protected override void Initialize()
        {
            Window.Title = "Space Glider";

            camera = new Camera(this);

            base.Initialize();
        }

        protected override void Update(GameTime gameTime)
        {

            if (started)
            {
                if (!player.alive)
                {
                    Platform.z_position = 0;
                    Platform.last_platform = new int[,] { { -1, 0 }, { -1, 0 }, { -1, 0 }, { -1, 0 }, { -1, 0 } };
                    Platform.next_platform = new int[,] { { 1, 0 }, { 1, 0 }, { 1, 0 }, { 1, 0 }, { 1, 0 } };

                    Platform.standing_platform = null;
                    Platform.next_standing_platform = null;
                    SetGameObjects();
                    started = false;
                    mainPage.EndGame(score);
                }
                else
                {
                    lightManager.Update();
                    keyboardState = keyboardManager.GetState();
                    flushAddedAndRemovedGameObjects();

                    // pass gameTime to let camera move along
                    camera.Update(gameTime, player.pos);

                    accelerometerReading = input.accelerometer.GetCurrentReading();

                    for (int i = 0; i < gameObjects.Count; i++)
                    {
                        gameObjects[i].Update(gameTime);
                    }

                    // Update score board on the game page
                    mainPage.UpdateScore(score);

                    // Update camera and player position for testing
                    mainPage.DisplayCameraPlayerPos(camera.Position, camera.cameraPos, player.pos, player.max_speed);
                    mainPage.UpdateShootButton(player.fireOn);

                    if (keyboardState.IsKeyDown(Keys.Escape))
                    {
                        this.Exit();
                        this.Dispose();
                        App.Current.Exit();
                    }
                    // Handle base.Update
                }
            }
            base.Update(gameTime);

        }

        protected override void Draw(GameTime gameTime)
        {
            if (started)
            {
                // Clears the screen with the Color.CornflowerBlue
                GraphicsDevice.Clear(Color.Black);
                for (int i = 0; i < gameObjects.Count; i++)
                {
                    gameObjects[i].Draw(gameTime);
                }

            }
            // Handle base.Draw
            base.Draw(gameTime);
        }
        // Count the number of game objects for a certain type.
        public int Count(GameObjectType type)
        {
            int count = 0;
            foreach (var obj in gameObjects)
            {
                if (obj.type == type) { count++; }
            }
            return count;
        }

        // Add a new game object.
        public void Add(GameObject obj)
        {
            if (!gameObjects.Contains(obj) && !addedGameObjects.Contains(obj))
            {
                addedGameObjects.Push(obj);
            }
        }

        // Remove a game object.
        public void Remove(GameObject obj)
        {
            if (gameObjects.Contains(obj) && !removedGameObjects.Contains(obj))
            {
                removedGameObjects.Push(obj);
            }
        }

        // Process the buffers of game objects that need to be added/removed.
        private void flushAddedAndRemovedGameObjects()
        {
            while (addedGameObjects.Count > 0) { gameObjects.Add(addedGameObjects.Pop()); }
            while (removedGameObjects.Count > 0) { gameObjects.Remove(removedGameObjects.Pop()); }
        }

        public void OnManipulationStarted(GestureRecognizer sender, ManipulationStartedEventArgs args)
        {
            // Pass Manipulation events to the game objects.
        }

        public void Tapped(GestureRecognizer sender, TappedEventArgs args)
        {
            // Pass Manipulation events to the game objects.
            foreach (var obj in gameObjects)
            {
                obj.Tapped(sender, args);
            }
        }

        public void OnManipulationUpdated(GestureRecognizer sender, ManipulationUpdatedEventArgs args)
        {

        }

        public void OnManipulationCompleted(GestureRecognizer sender, ManipulationCompletedEventArgs args)
        {
        }

        public float RandomFloat(float min, float max)
        {
            return ((float)random.NextDouble() * (max - min)) + min;
        }
    }
}
