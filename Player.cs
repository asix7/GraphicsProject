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
    public class Player : GameObject
    {
        private Model player_model;
        public bool alive = true;
        private Effect effect;
        private Matrix WorldInverseTranspose;

        public Cursor cursor;

        //Player movement properties
        private float speed = 0.0f;
        private float base_speed = 100.0f;
        private float additional_speed = 20.0f;
        private float bonus_speed = 20.0f;
        public float max_speed = -1;
        private float acceleration = 20.0f;

        // Projectile properties
        private float projectileSpeed = 400f;

        // Player jump properties
        private float velocityY = 0;
        private float jump_velocity;
        private float base_jump_velocity = 80.0f;
        private float bonus_jump_velocity = 15.0f;

        //Standing Platfom information
        bool onGround = true;
        private float max_terrheight;
        private float platform_base;
        public int platform_index;

        // Spaceship floats a little bit over the surface
        private float distance_from_floor = 2.0f;
        private float correction_distance = 2.0f;

        //
        private int enemy_score = 0;
        private float position_score = 0;

        // Points used for collisions
        Vector3 front_point;
        Vector3 left_front_point;
        Vector3 right_front_point;
        Vector3 left_back_point;
        Vector3 right_back_point;
        Vector3 front_distance = new Vector3(0, 0, -5);
        Vector3 left_front_distance = new Vector3(-1, 0, -3);
        Vector3 right_front_distance = new Vector3(1, 0, -3);
        Vector3 left_back_distance = new Vector3(-2, 0, 0);
        Vector3 right_back_distance = new Vector3(2, 0, 0);

        public bool OnGround
        {
            get { return onGround; }
        }

        private Matrix World;

        public Player(ProjectGame game,Vector3 initial_pos)
        {
            this.game = game;
            alive = true;
            World = Matrix.Identity;
            type = GameObjectType.Player;
            model = game.Content.Load<Model>("Spaceship");
            basicEffect = new BasicEffect(game.GraphicsDevice);
            BasicEffect.EnableDefaultLighting(model, true);

            //effect = game.Content.Load<Effect>("Phong");

            WorldInverseTranspose = Matrix.Transpose(Matrix.Invert(World));

            collisionRadius = 5.0f;

            //Set initial position
            pos = initial_pos;
            update_points();

            //effect = game.Content.Load<Effect>("Gouraud");
            //game.plat

            cursor = new Cursor(game);
            game.gameObjects.Add(cursor);
        }

        // Method to create projectile texture to give to newly created projectiles.
        private MyModel CreatePlayerProjectileModel()
        {
            return game.assets.CreateTexturedCube("player projectile.png", new Vector3(0.3f, 0.2f, 0.25f));
        }

        // Shoot a projectile.
        private void fire()
        {
            if (game.gameObjects.Contains(cursor.Target))
            {
                game.Add(new Projectile(game,
                "bitcoin",
                pos,
                projectileSpeed,
                //new Vector3(pos.X, 100, pos.Z-500),
                cursor.Target,
                GameObjectType.Enemy
                ));
            }
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
               
                float deltatime = (float)gameTime.ElapsedGameTime.TotalSeconds;
                if (game.keyboardState.IsKeyDown(Keys.U)) { fire(); }
                Player_movement(deltatime);
                WorldInverseTranspose = Matrix.Transpose(Matrix.Invert(World));
                position_score += pos.Z - position_score;
                game.score = (int)((float)Math.Abs(position_score + enemy_score));

            }
        }

        void Player_movement(float deltatime)
        {       
            // Get the lowest points of the patforms
            platform_base = Platform.standing_platform.platform_base;
            max_terrheight = game.lower_bound;

            // Get the Heights of the pltforms at each point 
            float terrheight = get_platform_height(pos) - correction_distance;
            float front_point_terrheight = get_platform_height(front_point) - correction_distance;
            float left_front_point_terrheight = get_platform_height(left_front_point) - correction_distance;
            float right_front_point_terrheight = get_platform_height(right_front_point) - correction_distance;
            float left_back_point_terrheight = get_platform_height(left_back_point) - correction_distance;
            float right_back_point_terrheight = get_platform_height(right_back_point) - correction_distance;

            // Check if the player collide with the platform, kill it if it does
            if ((pos.Y > platform_base) &&  
                ((pos.Y < terrheight) || (front_point.Y < front_point_terrheight) || 
                (left_front_point.Y < left_front_point_terrheight) || (right_front_point.Y < right_front_point_terrheight) ||
                (left_back_point.Y < left_back_point_terrheight) || (right_back_point.Y < right_back_point_terrheight)))
            {
                alive = false;
            }
            else
            {
                //Detect when the player touches the ground
                if ((pos.Y < max_terrheight + distance_from_floor) && (pos.Y > platform_base))
                {
                    this.onGround = true;
                    pos.Y = max_terrheight + distance_from_floor;
                }

                // Turn gravity on
                if (pos.Y > max_terrheight + distance_from_floor)
                {
                    this.onGround = false;
                }

                // If the player is not on the ground it falls
                // If the player velocity is > 0 it jumps first
                if (!onGround)
                {
                    pos.Y += velocityY * deltatime;
                    velocityY += game.gravity * deltatime;
                }

                // Determine the X position based on accelerometer reading
                

                //Apply pltforms bonus
                if (onGround)
                {
                    platform_bonus(Platform.standing_platform, platform_index);
                }

                // Move player forward depending of its speed
                if (speed < max_speed)
                {
                    speed += acceleration * deltatime;
                }
                else if (speed > max_speed)
                {
                    speed -= acceleration * deltatime;
                }

                if (game.keyboardState.IsKeyDown(Keys.A))
                {
                    pos.X -= speed * 0.5f * deltatime;
                }

                if (game.keyboardState.IsKeyDown(Keys.D))
                {
                    pos.X += speed * 0.5f * deltatime;
                }
                if (game.keyboardState.IsKeyDown(Keys.Space)){
                    if (onGround)
                    {
                       velocityY = jump_velocity;
                       onGround = false;
                    }
                }

                if (!game.keyboardState.IsKeyDown(Keys.P))
                {
                    pos.Z -= speed * deltatime;
                    pos.X += (float)game.accelerometerReading.AccelerationX * speed * deltatime;
                }

                update_points();
                OutofBounds();
                World = Matrix.Translation(Vector3.Zero) * Matrix.RotationX(-(float) (Math.PI /2)) * Matrix.Translation(pos);
            }
        }

        // Update the point positions
        void update_points()
        {
            front_point = pos + front_distance;
            left_front_point = pos + left_front_distance;
            right_front_point = pos + right_front_distance;
            left_back_point = pos + left_back_distance;
            right_back_point = pos + right_back_distance;
        }

        // Optain the current lecture of the platform height under the player
        public float get_platform_height(Vector3 point_position)
        {
            float height;
            Platform target_platform = Platform.standing_platform;

            // Check if the point is in the next platform
            if (point_position.Z < target_platform.z_position_end)
            {
                target_platform = Platform.next_standing_platform;
            }

            // Get the correct index of the tile to fetch
            int index = (int)(point_position.X / target_platform.tile_width);

            // Detect if the current point is outside of the platform
            if ((index > (target_platform.platform.GetLength(0) - 1) * 2) || index < 0 || point_position.X < 0)
            {
                height = game.lower_bound;
            }

            else if (index % 2 == 1)
            {
                height = game.lower_bound;
            }

            else
            {
                int level = target_platform.platform[index/2, 0];
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
            
            if (height >= max_terrheight)
            {
                if (height == max_terrheight)
                {
                    platform_index = index/2;                    
                }
                max_terrheight = height;                
            }

            return height;
        }

        void platform_bonus(Platform target_platform, int index)
        {
            // Get the correct index of the tile to fetch
            if ((index < target_platform.platform.GetLength(0)) && index >= 0)
            {
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
        public void Jump()
        {
            if (onGround)
            {
                velocityY = jump_velocity;
                onGround = false;
            }
        }

        public void Shoot()
        {
            fire();
        }

        /*
        public override void Tapped(GestureRecognizer sender, TappedEventArgs args)
        {
            if (onGround)
            {
                velocityY = jump_velocity;
                onGround = false;
            }
        }*/

        public override void Draw(GameTime gametime)
        {
            model.Draw(game.GraphicsDevice, World, game.camera.View, game.camera.Projection);
        }

        public override void OnManipulationUpdated(GestureRecognizer sender, ManipulationUpdatedEventArgs args)
        {
            pos.X += (float)args.Delta.Translation.X / 100;
        }

    }
}
