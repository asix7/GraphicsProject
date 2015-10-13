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
        private float projectileSpeed = 10;

        float fireTimer;
        float fireWaitMin = 2000;
        float fireWaitMax = 20000;
        float speed = 1f;

        public float Speed
        {
            get { return speed; }
            set { speed = value; }
        }

        public Enemy(LabGame game, Vector3 pos)
        {
            this.game = game;
            type = GameObjectType.Enemy;
            myModel = game.assets.GetModel("ship", CreateEnemyModel);
            this.pos = pos;
            setFireTimer();
            GetParamsFromModel();
        }

        private void setFireTimer()
        {
            fireTimer = fireWaitMin + (float)game.random.NextDouble() * (fireWaitMax - fireWaitMin);
        }

        public MyModel CreateEnemyModel()
        {
            return game.assets.CreateTexturedCube("enemy.png", 0.5f);
        }

        private MyModel CreateEnemyProjectileModel()
        {
            return game.assets.CreateTexturedCube("enemy projectile.png", new Vector3(0.2f, 0.2f, 0.4f));
        }

        public override void Update(GameTime gameTime)
        {
            // TASK 3: Fire projectile
            fireTimer -= gameTime.ElapsedGameTime.Milliseconds * game.difficulty;
            if (fireTimer < 0)
            {
                fire();
                setFireTimer();
            }

            // Set view of enemy
            basicEffect.View = game.camera.View;
            basicEffect.World = Matrix.Translation(pos);
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
