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
    using SharpDX.Toolkit.Graphics;
    public class Enemy : GameObject
    {
        private float projectileSpeed = 500;
        private float distance_from_screen = -500.0f;
        float fireTimer;
        float pointZ;
        float fireWaitMin = 1000;
        float fireWaitMax = 2000;
        float travelTimer = 5000;
        float travelDirX;
        float speed = 200f;
        Vector3 lastHitPos;

        // Landing position of the last projectile
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

            // Put the Enemy away from the camera
            this.pos = pos;
            this.pos.Z = game.camera.Position.Z + distance_from_screen;

            // Send first direction to the player
            travelTimer = 0;
            setFireTimer();

        }

        // Timer before changing movement direction
        private void setFireTimer()
        {
            fireTimer = fireWaitMin + (float)game.random.NextDouble() * (fireWaitMax - fireWaitMin);
        }

        public override void Update(GameTime gameTime)
        {
            float timechange = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // Calculates the attack speed according to the difficulty
            fireWaitMin = (3 - game.difficulty) * 500;
            fireWaitMax = game.difficulty * 500;

            //Enemy first shoot at a random point near the player position
            pointZ = game.RandomFloat(game.player.pos.Z - 100, game.player.pos.Z + 100);

            // Fire projectile
            fireTimer -= gameTime.ElapsedGameTime.Milliseconds;
            if (fireTimer < 0)
            {
                fire();
                setFireTimer();
            }
            pos.Z = game.camera.Position.Z + distance_from_screen;

            // Randomise the enemy travel direction in X
            travelTimer -= gameTime.ElapsedGameTime.Milliseconds;
            if (travelTimer < 0)
            {
                // refresh the timer
                travelTimer = 5000;

                // set the travel direction
                travelDirX = game.RandomFloat(-1f, 1f);
            }

            SimpleFlying(timechange);

            // Set view of enemy
            basicEffect.View = game.camera.View;
            basicEffect.World = Matrix.Scaling(10) * Matrix.RotationX(-(float)(Math.PI / 4.0f)) * Matrix.Translation(pos);

        }

        public override void Draw(GameTime gametime)
        {
            model.Draw(game.GraphicsDevice, basicEffect.World, game.camera.View, game.camera.Projection);
        }

        //Fire a proyectile at a position near the player
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

        // Move in the X direction according to the random generated direction
        private void SimpleFlying(float timechange)
        {
            // reverse the direction if enemy out of bound
            if (pos.X < -20)
            {
                travelDirX = game.RandomFloat(0, 1);
            }
            else if (pos.X > 220)
            {
                travelDirX = game.RandomFloat(-1, 0);
            }

            // move enemy
            pos.X += speed * travelDirX * timechange;
        }

        // Kill the Enemy and add points to the score
        public void Hit()
        {
            game.player.enemy_score += 100 * (int)game.difficulty;
            game.Remove(this);
        }

    }
}
