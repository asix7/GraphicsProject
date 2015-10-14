using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using SharpDX.Toolkit;
using Windows.UI.Input;
using Windows.UI.Core;

namespace Project
{
    using SharpDX.Toolkit.Graphics;
    using SharpDX.Toolkit.Input;
    // Player class.
    class Player : GameObject
    {
        private Model player_model;
        private float projectileSpeed = 20;

        //player movement speed
        private float speed = 100.0f;
        private float base_speed = 100.0f;
        private float additional_speed = 20.0f;
        private float velocityY = 0;
        private float initial_jump_speedY = 80.0f;

        bool onGround = true;
        private float terrheight;

        // Spaceship floats a little bit over the surface
        private float distance_from_floor = 2.0f;

        // Points used for collisions
        private float front_point;
        private float back_point;
        private float bottom_point;
        private float right_back_point;
        private float left_back_point;
        private float left_front_point;
        private float right_front_point;

        public static float current_index = 0;

        public Player(ProjectGame game)
        {
            this.game = game;
            type = GameObjectType.Player;
            player_model = game.Content.Load<Model>("Spaceship");
            basicEffect = new BasicEffect(game.GraphicsDevice)
            {
                View = game.camera.View,
                Projection = game.camera.Projection,
                World = Matrix.Identity,
            };
            BasicEffect.EnableDefaultLighting(player_model, true);
            pos = new Vector3(50, game.init_pos, 0);  

        }

        // Method to create projectile texture to give to newly created projectiles.
        private MyModel CreatePlayerProjectileModel()
        {
            return game.assets.CreateTexturedCube("player projectile.png", new Vector3(0.3f, 0.2f, 0.25f));
        }

        // Shoot a projectile.
        private void fire()
        {
            game.Add(new Projectile(game,
                game.assets.GetModel("player projectile", CreatePlayerProjectileModel),
                pos,
                new Vector3(0, projectileSpeed, 0),
                GameObjectType.Enemy
            ));
        }

        // Frame update.
        public override void Update(GameTime gameTime)
        {
            speed = base_speed + (additional_speed * game.difficulty);
            OutofBounds();
            float deltatime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (game.keyboardState.IsKeyDown(Keys.Space)) { fire(); }
            
            Player_movement(deltatime);

            // Set view of the player same as camera to view...
            basicEffect.View = game.camera.View;
        }

        public void Player_movement(float deltatime)
        {
            terrheight = get_platform_height(Platform.standing_platform);

            if (pos.Y < terrheight)
            {
                game.Exit();
            }

            //Detect when the player touches the ground
            if (pos.Y < terrheight + distance_from_floor)
            {
                this.onGround = true;
                pos.Y = terrheight + distance_from_floor;
            }

            if (pos.Y > terrheight + distance_from_floor)
            {
                this.onGround = false;
            }

            // If the player is not on the ground it falls
            if (!onGround)
            {
                pos.Y += velocityY * deltatime;
                velocityY += game.gravity * deltatime;
            }

            // Determine the X position based on accelerometer reading
            pos.X += (float)game.accelerometerReading.AccelerationX * speed * deltatime;

            // move player forward depending of its speed
            pos.Z += speed * deltatime;

            basicEffect.World = Matrix.Translation(pos);
        }


        // Optain the current lecture of the platform height under the player
        public float get_platform_height(Platform target_platform)
        {
            float height;
            Platform platform = target_platform;

            // Get the correct index of the tile to fetch
            int index = (int)(pos.X / platform.tile_width);
            
            // Detect if the current position is outside of the platform
            if ((index > platform.platform.GetLength(0) - 1) || index < 0 || pos.X < 0)
            {
                height = game.lower_bound;
            }

            else
            {
                int level = platform.platform[index, 0];
                // Detect if the current tile is empty
                if (level < 0)
                {
                    height = game.lower_bound;
                }
                // Get the correct height of the tile
                else
                {
                    height = platform.Levels[level];
                }
                
            }
            return height;
        }

        // React to getting hit by an enemy bullet.
        public void Hit()
        {
            game.Exit();
        }

        // End the game if the player falls out of bounds
        public void OutofBounds()
        {
            if (pos.Y < game.lower_bound)
            {
                game.Exit();
            }            
        }

        public override void Tapped(GestureRecognizer sender, TappedEventArgs args)
        {
            if (onGround)
            {
                velocityY = initial_jump_speedY;
                onGround = false;
            }
        }
        public override void Draw(GameTime gametime)
        {
            player_model.Draw(game.GraphicsDevice, basicEffect.World ,game.camera.View, game.camera.Projection);
        }

        public override void OnManipulationUpdated(GestureRecognizer sender, ManipulationUpdatedEventArgs args)
        {
            pos.X += (float)args.Delta.Translation.X / 100;
        }

    }
}
