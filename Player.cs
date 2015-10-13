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
        private float projectileSpeed = 20;

        //player movement speed
        private float speed = 100.0f;
        float gravity = -30f;
        private float velocityY = 0;
        private float initial_jump_speedY = 30.0f;

        bool onGround = true;
        private float terrheight = 30f;

        public static float current_index;

        public Player(LabGame game)
        {
            this.game = game;
            type = GameObjectType.Player;
            myModel = game.assets.GetModel("player", CreatePlayerModel);
            pos = new Vector3(50, 30, 0);
            GetParamsFromModel();
        }

        public MyModel CreatePlayerModel()
        {
            return game.assets.CreateTexturedCube("player.png", 0.7f);
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
            OutofBounds();
            float deltatime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (game.keyboardState.IsKeyDown(Keys.Space)) { fire(); }
            
            terrheight = get_TerrainH();
            if (pos.Y < terrheight)
            {
                this.onGround = true;
                pos.Y = terrheight;
            }

            if (pos.Y > terrheight)
            {
                this.onGround = false;
            }

            if (!onGround)
            {
                pos.Y += velocityY * deltatime;
                velocityY += gravity * deltatime;
            }


            // TASK 1: Determine velocity based on accelerometer reading
            pos.X += (float)game.accelerometerReading.AccelerationX * speed * deltatime;

            // move player forward
            pos.Z += speed * deltatime;

            basicEffect.World = Matrix.Translation(pos);

            // Set view of the player same as camera to view...
            basicEffect.View = game.camera.View;
        }

        public float get_TerrainH()
        {
            Platforms standing_platform = Platforms.standing_platform;

            int index = (int)(pos.X / standing_platform.tile_width);
            current_index = index;
            
            float height;
            if ((index > standing_platform.platform.GetLength(0) - 1) || index < 0 || pos.X < 0)
            {
                height = game.lower_bound;
            }

            else
            {
                int level = standing_platform.platform[index, 0];
                if (level < 0)
                {
                    height = game.lower_bound;
                }
                else
                {
                    height = standing_platform.Levels[level];
                }
                
            }
            return height;
        }
        // React to getting hit by an enemy bullet.
        public void Hit()
        {
            game.Exit();
        }

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

        public override void OnManipulationUpdated(GestureRecognizer sender, ManipulationUpdatedEventArgs args)
        {
            pos.X += (float)args.Delta.Translation.X / 100;
        }

    }
}
