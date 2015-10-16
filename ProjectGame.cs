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

        public List<GameObject> gameObjects;
        public List<Platform> platforms_list;
        private Stack<GameObject> addedGameObjects;
        private Stack<GameObject> removedGameObjects;


        private Player player;
        public int score;


        // Use this to represent difficulty
        public float difficulty;

        // Represents the camera's position and orientation
        public Camera camera;

        // Graphics assets
        public Assets assets;

        // Random number generator
        public Random random;

        // Lower boundary used to end the game
        public float lower_bound = -60.0f;

        // Game gravity
        public float gravity = -100.0f;

        // Player initial position
        public float init_pos;

        public bool started = false;
        /// <summary>
        /// Initializes a new instance of the <see cref="ProjectGame" /> class.
        /// </summary>
        public ProjectGame(MainPage mainPage)
        {
            // Creates a graphics manager. This is mandatory.
            graphicsDeviceManager = new GraphicsDeviceManager(this);
            graphicsDeviceManager.PreferredGraphicsProfile = new FeatureLevel[] { FeatureLevel.Level_10_0, };


            // Setup the relative directory to the executable directory
            // for loading contents with the ContentManager
            Content.RootDirectory = "Content";

            // Create the keyboard manager
            keyboardManager = new KeyboardManager(this);
            assets = new Assets(this);
            random = new Random();
            input = new GameInput();

            // Initialise event handling.
            input.gestureRecognizer.Tapped += Tapped;
            input.gestureRecognizer.ManipulationStarted += OnManipulationStarted;
            input.gestureRecognizer.ManipulationUpdated += OnManipulationUpdated;
            input.gestureRecognizer.ManipulationCompleted += OnManipulationCompleted;

            this.mainPage = mainPage;

            score = 0;
            difficulty = 1;
        }

        protected override void LoadContent()
        {
            // Initialise game object containers.
            gameObjects = new List<GameObject>();
            platforms_list = new List<Platform>();
            addedGameObjects = new Stack<GameObject>();
            removedGameObjects = new Stack<GameObject>();

            // Create game objects.


            gameObjects.Add(new EnemyController(this));
            // Create the first platform and get the height
            Platform first_platform = new Platform(this);
            int level = first_platform.platform[2, 0];
            init_pos = first_platform.Levels[level];

            player = new Player(this);
            gameObjects.Add(player);
            //Number of platforms to render on screen at any time
            platforms_list.Add(first_platform);
            platforms_list.Add(new Platform(this));
            platforms_list.Add(new Platform(this));
            platforms_list.Add(new Platform(this));
            platforms_list.Add(new Platform(this));
            platforms_list.Add(new Platform(this));

            base.LoadContent();
        }

        protected override void Initialize()
        {
            Window.Title = "Project 2";

            camera = new Camera(this);

            base.Initialize();
        }

        protected override void Update(GameTime gameTime)
        {

            if (started)
            {
                if (!player.alive)
                {
                    this.Exit();
                }
                keyboardState = keyboardManager.GetState();
                flushAddedAndRemovedGameObjects();

                // pass gameTime to let camera move along
                camera.Update(gameTime, player.pos);

                accelerometerReading = input.accelerometer.GetCurrentReading();

                for (int i = 0; i < gameObjects.Count; i++)
                {
                    gameObjects[i].Update(gameTime);
                }

                for (int i = 0; i < platforms_list.Count; i++)
                {
                    platforms_list[i].Update(gameTime);
                    platforms_list[i].Update_player_platforms(player.pos);

                }
                // Update score board on the game page
                //mainPage.UpdateScore(score);

                // Update camera and player position for testing
                mainPage.DisplayCameraPlayerPos(camera.Position, camera.Target, player.pos, difficulty, Platform.standing_platform.z_position_end);

                if (keyboardState.IsKeyDown(Keys.Escape))
                {
                    this.Exit();
                    this.Dispose();
                    App.Current.Exit();
                }
                // Handle base.Update
            }
            base.Update(gameTime);

        }

        protected override void Draw(GameTime gameTime)
        {
            if (started)
            {
                // Clears the screen with the Color.CornflowerBlue
                GraphicsDevice.Clear(Color.CornflowerBlue);
                for (int i = 0; i < gameObjects.Count; i++)
                {
                    gameObjects[i].Draw(gameTime);
                }
                for (int i = 0; i < platforms_list.Count; i++)
                {
                    platforms_list[i].Draw(gameTime);
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

    }
}
