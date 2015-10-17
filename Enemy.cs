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
        private float projectileSpeed = 10;
        private float distance_from_screen = 120.0f;
        float fireTimer;
        float fireWaitMin = 2000;
        float fireWaitMax = 20000;
        float travelTimer;
        float travelDirX;
        float speed = 1f;

        public Enemy(ProjectGame game, Vector3 pos)
        {
            this.game = game;
            type = GameObjectType.Player;
            enemy_model = game.Content.Load<Model>("Enemy");
            basicEffect = new BasicEffect(game.GraphicsDevice)
            {
                View = game.camera.View,
                Projection = game.camera.Projection,
                World = Matrix.Identity,
            };
            BasicEffect.EnableDefaultLighting(enemy_model, true);
            type = GameObjectType.Enemy;
            this.pos = pos;
            this.pos.Z = game.camera.Position.Z + distance_from_screen;

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

            // TASK 3: Fire projectile
            fireTimer -= gameTime.ElapsedGameTime.Milliseconds * game.difficulty;
            if (fireTimer < 0)
            {
                setFireTimer();
            }
            pos.Z = game.camera.Position.Z + distance_from_screen;

            // randomise the enemy travel direction in X
            travelTimer -= gameTime.ElapsedGameTime.Milliseconds;
            if (travelTimer < 0) {

                // refresh the timer
                travelTimer = 5000;

                // set the travel direction
                travelDirX = game.RandomFloat(-1f, 1f);
                //TravelPosition = new Vector2(game.RandomFloat(game.boundaryLeft, game.boundaryRight), pos.Y);
            }

            // reverse the direction if enemy out of bound
            if (pos.X > 1000 || pos.X < -1000)
                travelDirX *= -1;
            // move enemy
            pos.X += speed * travelDirX * timechange;

            // Set view of enemy
            basicEffect.View = game.camera.View;
            basicEffect.World = Matrix.Translation(Vector3.Zero) * Matrix.Scaling(5) * Matrix.RotationY((float)(gameTime.TotalGameTime.TotalSeconds * 0.7)) * Matrix.RotationY((float)-Math.PI) * Matrix.Translation(pos);

        }

        public override void Draw(GameTime gametime)
        {
            enemy_model.Draw(game.GraphicsDevice, basicEffect.World, game.camera.View, game.camera.Projection);
        }

        private void fire()
        {
            game.Add(new Projectile(game,
                game.assets.GetModel("enemy projectile", CreateEnemyProjectileModel),
                pos,
                new Vector3(0, -projectileSpeed, 0),
                GameObjectType.Player
            ));
        }

        public void Hit()
        {
            game.score += 10;
            game.Remove(this);
        }

    }
}
