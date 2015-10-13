using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using SharpDX.Toolkit;

namespace Project
{
    using SharpDX.Toolkit.Graphics;

    
    // Enemy Controller class.
    class EnemyController : GameObject
    {
        // Spacing and counts.
        private int rows = 0;
        private int enemiesPerRow = 8;
        private float rowSpacing = 0.6f;
        private float colSpacing = 0.6f;

        // Timing and movement.
        private float stepSize = 1f;
        private float stepWait = 0;
        private float stepTimer = 0;
        private bool stepRight;

        // Constructor.
        public EnemyController(LabGame game)
        {
            this.game = game;
            nextWave();
        }

        // Set up the next wave.
        private void nextWave()
        {
            rows += 1;
            stepWait = 1000f / (1 + rows / 3f);
            createEnemies();
            stepRight = true;
        }

        // Create a grid of enemies for the current wave.
        private void createEnemies()
        {
            float y = game.boundaryTop;
            for (int row = 0; row < rows; row++)
            {
                float x = game.boundaryLeft;
                for (int col = 0; col < enemiesPerRow; col++)
                {
                    game.Add(new Enemy(game, new Vector3(x, y, 0)));
                    x += colSpacing;
                }
                y -= rowSpacing;
            }
        }

        // Frame update method.
        public override void Update(GameTime gameTime)
        {
            float timechange = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // Move the enemies along with the player and camera
            foreach (var obj in game.gameObjects)
            {
                if (obj.type == GameObjectType.Enemy)
                {
                    Enemy enemy = (Enemy)obj;
                    enemy.pos.Z += enemy.Speed * timechange;
                }
            }


            // Move the enemies a step once the step timer has run out and reset step timer.
            // TASK 3: Moves according to game difficulty.  N.B.  This is a simple way of achieving this, you may prefer to impletement
            // SetDifficulty methods in a manner similar to Tapped if you were to do more complicated things with the difficulty
            stepTimer -= gameTime.ElapsedGameTime.Milliseconds * game.difficulty;
            if (stepTimer <= 0)
            {
                //step();
                stepTimer = stepWait;
            }

            // Invoke next wave once current one has ended.
            if (allEnemiesAreDead())
            {
                nextWave();
            }
        }


        // Return whether all enemies are dead or not.
        private bool allEnemiesAreDead()
        {
            return game.Count(GameObjectType.Enemy) == 0;
        }

        // Move all the enemies, changing directions and stepping down when the edge of the screen is reached.
        private void step()
        {
            bool stepDownNeeded = false;
            foreach (var obj in game.gameObjects)
            {
                if (obj.type == GameObjectType.Enemy)
                {
                    Enemy enemy = (Enemy)obj;
                    if (stepRight)
                    {
                        enemy.pos.X += stepSize;
                        if (enemy.pos.X > game.boundaryRight) { stepDownNeeded = true; }
                    }
                    else
                    {
                        enemy.pos.X -= stepSize;
                        if (enemy.pos.X < game.boundaryLeft) { stepDownNeeded = true; }
                    }
                }
            }

            if (stepDownNeeded)
            {
                stepRight = !stepRight;
                stepDown();
            }
        }

        // Step all enemies down one.
        private void stepDown()
        {
            foreach (var obj in game.gameObjects)
            {
                if (obj.type == GameObjectType.Enemy)
                {
                    Enemy enemy = (Enemy)obj;
                    enemy.pos.Y -= stepSize;
                    if (enemy.pos.Y < game.boundaryBottom) { gameOver(); }
                }
            }
        }

        // Method for when the game ends.
        private void gameOver()
        {
            game.Exit();
        }

    }
}