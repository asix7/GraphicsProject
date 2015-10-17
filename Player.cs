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
        public bool alive = true;
        
        //player movement speed
        private float speed = 0.0f;
        private float base_speed = 100.0f;
        private float additional_speed = 20.0f;
        private float bonus_speed = 20.0f;
        private float max_speed = -1;
        private float acceleration = 20.0f;

        private float projectileSpeed = 20;

        // velocity that affects player position in Y
        private float velocityY = 0;

        // player jump
        private float jump_velocity;
        private float base_jump_velocity = 80.0f;
        private float bonus_jump_velocity = 15.0f;

        //Standing Platfom information
        bool onGround = true;
        private float terrheight;
        private float platform_base;

        // Spaceship floats a little bit over the surface
        private float distance_from_floor = 2.0f;
        private float correction_distance = 2.0f;

        // Points used for collisions
        private float front_point;
        private float back_point;
        private float bottom_point;
        private float right_back_point;
        private float left_back_point;
        private float left_front_point;
        private float right_front_point;

        public static float current_index = 0;
        private Matrix World;

        public Player(ProjectGame game)
        {
            this.game = game;
            alive = true;
            type = GameObjectType.Player;
            player_model = game.Content.Load<Model>("Spaceship");
            basicEffect = new BasicEffect(game.GraphicsDevice);
            World = Matrix.Identity;
            BasicEffect.EnableDefaultLighting(player_model, true);
            //basicEffect.TextureEnabled = true;
            //basicEffect.Alpha = 0;
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
            if (max_speed == -1)
            {
                max_speed = base_speed + (additional_speed * game.difficulty);
            }
            if (alive)
            {
                OutofBounds();
                float deltatime = (float)gameTime.ElapsedGameTime.TotalSeconds;

                if (game.keyboardState.IsKeyDown(Keys.Space)) { fire(); }

                Player_movement(deltatime);

            }
        }

        void Player_movement(float deltatime)
        {
            terrheight = get_platform_height(Platform.standing_platform);
            platform_base = Platform.standing_platform.platform_base;

            if ((pos.Y < terrheight - correction_distance) && (pos.Y > platform_base))
            {
                alive = false;
            }
            else
            {

                //Detect when the player touches the ground
                if ((pos.Y < terrheight + distance_from_floor) && (pos.Y > platform_base))
                {
                    this.onGround = true;
                    pos.Y = terrheight + distance_from_floor;
                }

                // Falls from platform
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
                if (onGround)
                {
                    platform_bonus(Platform.standing_platform);
                }
                
                if (speed < max_speed)
                {
                    speed += acceleration * deltatime;
                }
                else if (speed > max_speed)
                {
                    speed -= acceleration * deltatime;
                }                

                pos.Z += speed * deltatime;

                World = Matrix.Translation(Vector3.Zero) * Matrix.RotationY((float)-Math.PI) * Matrix.Translation(pos);
            }
        }

        void platform_bonus(Platform target_platform)
        {
            // Get the correct index of the tile to fetch
            int index = (int)(pos.X / target_platform.tile_width);
            int tile_type = target_platform.platform[index, 1];

            // Go faster in Blue plaforms
            if (tile_type == 0)
            {
                max_speed = base_speed + (additional_speed * game.difficulty) + bonus_speed;
            } 
            else
            {
                max_speed = base_speed + (additional_speed * game.difficulty);
            }

            // Jump higher in red platforms
            if (tile_type == 1)
            {
                jump_velocity = base_jump_velocity + bonus_jump_velocity;
            }
            else 
            {
                jump_velocity = base_jump_velocity;
            }

            // Other bonus in grey platforms
            if (tile_type == 2)
            {
                // BONUS NUMBER 3
            }
            else
            {
                // NORMAL STATUS
            }
            
        }


        // Optain the current lecture of the platform height under the player
        public float get_platform_height(Platform target_platform)
        {
            float height;
            
            // Get the correct index of the tile to fetch
            int index = (int)(pos.X / target_platform.tile_width);

            // Detect if the current position is outside of the platform
            if ((index > target_platform.platform.GetLength(0) - 1) || index < 0 || pos.X < 0)
            {
                height = game.lower_bound;
            }

            else
            {
                int level = target_platform.platform[index, 0];
                // Detect if the current tile is empty
                if (level < 0)
                {
                    height = game.lower_bound;
                }
                // Get the correct height of the tile
                else
                {
                    height = target_platform.Levels[level];
                }

            }
            return height;
        }

        // React to getting hit by an enemy bullet.
        public void Hit()
        {
            alive = false;
        }

        // End the game if the player falls out of bounds
        public void OutofBounds()
        {
            if (pos.Y - distance_from_floor <= game.lower_bound)
            {
                alive = false;
            }
        }

        public override void Tapped(GestureRecognizer sender, TappedEventArgs args)
        {
            if (onGround)
            {
                velocityY = jump_velocity;
                onGround = false;
            }
        }
        public override void Draw(GameTime gametime)
        {
            player_model.Draw(game.GraphicsDevice, World, game.camera.View, game.camera.Projection);
        }

        public override void OnManipulationUpdated(GestureRecognizer sender, ManipulationUpdatedEventArgs args)
        {
            pos.X += (float)args.Delta.Translation.X / 100;
        }

    }
}
