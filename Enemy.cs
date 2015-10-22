using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX.Toolkit;
using SharpDX;
namespace Project
{
    // Enemy class
    // Basically just shoots randomly, see EnemyController for enemy movement.
    using SharpDX.Toolkit.Graphics;
    class Enemy : GameObject
    {
        Model enemy_model;
        private float projectileSpeed = 500;
        private float distance_from_screen = -1500.0f;
        float fireTimer;
        float pointZ;
        float fireWaitMin = 200;
        float fireWaitMax = 2000;
        float travelTimer = 5000;
        float travelDirX;
        float speed = 300f;
        private bool locking = false;
        Vector3 lastHitPos;

        // landing position of the last projectile
        public Vector3 LastHitPosition
        {
            get { return lastHitPos; }
            set { lastHitPos = value; }
        }

        public Enemy(ProjectGame game, Vector3 pos)
        {
            this.game = game;
            type = GameObjectType.Player;
            model = game.Content.Load<Model>("Enemy");
            basicEffect = new BasicEffect(game.GraphicsDevice)
            {
                View = game.camera.View,
                Projection = game.camera.Projection,
                World = Matrix.Identity,
            };
            BasicEffect.EnableDefaultLighting(model, true);
            type = GameObjectType.Enemy;
            this.pos = pos;
            //this.pos.Z = game.camera.Position.Z + distance_from_screen;

            this.pos.Z = game.player.pos.Z;
            travelTimer = 0;
            setFireTimer();

        }

        private void setFireTimer()
        {
            fireTimer = fireWaitMin + (float)game.random.NextDouble() * (fireWaitMax - fireWaitMin);
        }

        private MyModel CreateEnemyProjectileModel()
        {
            return game.assets.CreateTexturedCube("enemy projectile.png", new Vector3(0.2f, 0.2f, 0.4f));
        }

        public override void Update(GameTime gameTime)
        {
            float timechange = (float)gameTime.ElapsedGameTime.TotalSeconds;

            //random pointZ
            pointZ = game.RandomFloat(pos.Z + 400, pos.Z + 500);

            // TASK 3: Fire projectile
            fireTimer -= gameTime.ElapsedGameTime.Milliseconds * game.difficulty;
            if (fireTimer < 0)
            {
                fire();
                setFireTimer();
            }
            //pos.Z = game.camera.Position.Z + distance_from_screen;
            pos.Z = game.player.pos.Z - 500;

            // randomise the enemy travel direction in X
            travelTimer -= gameTime.ElapsedGameTime.Milliseconds;
            if (travelTimer < 0)
            {
                //locking = true;

                // refresh the timer
                travelTimer = 5000;

                // set the travel direction
                travelDirX = game.RandomFloat(-1f, 1f);

                /*
                if (travelDirX < 0.3 && travelDirX > -0.3) {
                    locking = true;
                }*/
            }

            SimpleFlying(timechange);


            // Set view of enemy
            basicEffect.View = game.camera.View;
            basicEffect.World = Matrix.Scaling(10) * Matrix.RotationX((float)(Math.PI / 4.0f)) * Matrix.Translation(pos);

        }

        public override void Draw(GameTime gametime)
        {
            model.Draw(game.GraphicsDevice, basicEffect.World, game.camera.View, game.camera.Projection);
        }

        private void fire()
        {
            game.Add(new Projectile(game,
                this,
                pos,
                projectileSpeed,
                new Vector3(game.player.pos.X, -10, pointZ),
                GameObjectType.Player
            ));
        }

        private void PredictTargetZ()
        {

        }

        private void LockTarget(float timechange)
        {
            if (Math.Abs(game.player.pos.X - pos.X) <= 2)
            {
                pos.X = game.player.pos.X;
                Landing(timechange);
            }
            else if (pos.X < game.player.pos.X)
            {
                pos.X += (speed / 2) * timechange;
            }
            else if (pos.X > game.player.pos.X)
            {
                pos.X -= (speed / 2) * timechange;
            }

        }

        private void Landing(float timechange)
        {
            if (Math.Abs(game.player.pos.Y - pos.Y) - 22 <= 2)
            {
                pos.Y = game.player.pos.Y + 22;
            }
            else
            {
                pos.Y -= speed * 0.4f * timechange;
            }
        }

        private void SimpleFlying(float timechange)
        {
            // reverse the direction if enemy out of bound
            if (pos.X > 100 || pos.X < 0)
                travelDirX *= -1;
            // move enemy
            pos.X += speed * travelDirX * timechange;
        }

        public void Hit()
        {
            game.score += 10;
            game.Remove(this);
        }

    }
}
