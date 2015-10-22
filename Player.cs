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
        public bool alive = true;
        private Matrix WorldInverseTranspose;

        // Create the cursor for the player target
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
        public bool fireOn = false;
        private float wait_timer = 0;
        private float max_terrheight;
        private float platform_base;
        public int platform_index;

        // Spaceship floats a little bit over the surface
        private float distance_from_floor = 2.0f;
        private float correction_distance = 2.0f;

        // Different types of score
        public int enemy_score = 0;
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

            WorldInverseTranspose = Matrix.Transpose(Matrix.Invert(World));

            collisionRadius = 5.0f;

            //Set initial position
            pos = initial_pos;
            update_points();

            cursor = new Cursor(game);
            game.gameObjects.Add(cursor);

            

            //spriteBatch = ToDisposeContent(new SpriteBatch(GraphicsDevice));
        }

        // Shoot a projectile.
        private void fire()
        {
            // Gett the target selected by the cursor and shots at it
            if (game.gameObjects.Contains(cursor.Target) && fireOn)
            {
                game.Add(new Projectile(game,
                "Medallion",
                front_point,
                projectileSpeed,
                cursor.Target,
                GameObjectType.Enemy
                ));
            }
        }

        // Frame update.
        public override void Update(GameTime gameTime)
        {
            if (alive)
            {
                // Set the max_speed according to the difficulty
                if (max_speed == -1)
                {
                    max_speed = base_speed + (additional_speed * game.difficulty);
                }
                float deltatime = (float)gameTime.ElapsedGameTime.TotalSeconds;

                // Lowers the timer to wait before shoting
                wait_timer -= deltatime;

                //Moves the player
                Player_movement(deltatime);
                WorldInverseTranspose = Matrix.Transpose(Matrix.Invert(World));

                // Updates the Score
                position_score = (Math.Abs(pos.Z))/10.0f;
                game.score = (int)((float)Math.Abs(position_score + enemy_score));

            }
        }

        // Control the player movement and collision
        void Player_movement(float deltatime)
        {       
            // Get the lowest points of the patforms
            platform_base = Platform.standing_platform.platform_base;
            max_terrheight = game.lower_bound;

            // Get the Heights of the platforms at each point 
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

                // Keyboard control
                if (game.keyboardState.IsKeyDown(Keys.Left))
                {
                    pos.X -= speed * 0.25f * deltatime;
                }

                if (game.keyboardState.IsKeyDown(Keys.Right))
                {
                    pos.X += speed * 0.25f * deltatime;
                }

                if (game.keyboardState.IsKeyPressed(Keys.C)){
                    Jump();
                }
                if (game.keyboardState.IsKeyPressed(Keys.X))
                {
                    Shoot();
                }

                //Update the positions in X and Z
                pos.Z -= speed * deltatime;
                pos.X += (float)game.accelerometerReading.AccelerationX * speed * deltatime;
                
                //Updte the collision points
                update_points();
                // Check if the player is out of bounds
                OutofBounds();

                World = Matrix.Translation(Vector3.Zero) * Matrix.RotationX(-(float) (Math.PI /2)) * Matrix.Translation(pos);
            }
        }

        // Update the point positions for collision
        void update_points()
        {
            front_point = pos + front_distance;
            left_front_point = pos + left_front_distance;
            right_front_point = pos + right_front_distance;
            left_back_point = pos + left_back_distance;
            right_back_point = pos + right_back_distance;
        }

        // Optain the current lecture of the platform height under a point
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
            
            //Check if the position is void
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

        // Give the player the correct bonus of each tile type
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

                // Enable shooting in grey platforms
                if (tile_type == 2)
                {
                    fireOn = true;
                }
                else
                {
                    fireOn = false;
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

        //Jump if is on ground
        public void Jump()
        {
            if (onGround)
            {
                velocityY = jump_velocity;
                onGround = false;
            }
        }

        //Shoot if is enable
        public void Shoot()
        {
            if (wait_timer < 0)
            {
                fire();
                wait_timer = 2;
            }            
        }

        public override void Draw(GameTime gametime)
        {
            model.Draw(game.GraphicsDevice, World, game.camera.View, game.camera.Projection);
        }
    }
}
