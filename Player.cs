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
        //private float speed = 0.006f;
        private float projectileSpeed = 20;

        //player movement speed
        private float speed = 1;
        private float velocityX;
        float gravity = -0.005f;
        private float velocityY = 0.1f;
        bool onGround = true;
        private float terrheight = -4f;
        private int jump_count = 0;

        public Player(LabGame game)
        {
            this.game = game;
            type = GameObjectType.Player;
            myModel = game.assets.GetModel("player", CreatePlayerModel);
            pos = new SharpDX.Vector3(0, game.boundaryBottom + 0.5f, 0);
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
            float deltatime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (game.keyboardState.IsKeyDown(Keys.Space)) { fire(); }
            
            if (pos.Y < terrheight)
            {
                this.onGround = true;
                pos.Y = terrheight;
                velocityY = 0.1f;
            }


            //Debug.WriteLine(pos.Y);


            if (this.onGround == false)
            {
                pos.Y += velocityY;
                velocityY += gravity;
            }

            // TASK 1: Determine velocity based on accelerometer reading
            pos.X += (float)game.accelerometerReading.AccelerationX;

            // Keep within the boundaries.
            if (pos.X < game.boundaryLeft) { pos.X = game.boundaryLeft; }
            if (pos.X > game.boundaryRight) { pos.X = game.boundaryRight; }

            // move player forward
            pos.Z += speed * deltatime;

            basicEffect.World = Matrix.Translation(pos);

            // Set view of the player same as camera to view...
            basicEffect.View = game.camera.View;
        }

        // React to getting hit by an enemy bullet.
        public void Hit()
        {
            game.Exit();
        }

        public override void Tapped(GestureRecognizer sender, TappedEventArgs args)
        {
            //fire();
            //testStop();
            onGround = false;

        }

        public override void OnManipulationUpdated(GestureRecognizer sender, ManipulationUpdatedEventArgs args)
        {
            pos.X += (float)args.Delta.Translation.X / 100;
        }

        public void testStop()
        {
            speed = 0;
        }
    }
}
