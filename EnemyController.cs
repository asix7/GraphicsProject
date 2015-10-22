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
        private int number_of_enemies;
        private float rowSpacing = 50f;
        private float spawn_timer = 3;

        // Constructor.
        public EnemyController(ProjectGame game)
        {
            this.game = game;
        }

        // Create a grid of enemies for the current wave.
        private void createEnemies()
        {
            float y = 80;
            float x = 100;
            for (int i = 0; i < number_of_enemies; i++)
            {
                game.Add(new Enemy(game, new Vector3(x, y, 0)));
                y += rowSpacing;
            }
        }

        // Frame update method.
        public override void Update(GameTime gameTime)
        {
            float timechange = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // Create 1 or two Enemies depending on the difficulty
            number_of_enemies = (int)(1 + game.RandomFloat(0, game.difficulty));
            if (number_of_enemies > 2)
            {
                number_of_enemies = 2;
            }

            // Invoke next wave after 5 secs that current one has ended.
            if (allEnemiesAreDead() && spawn_timer <= 0)
            {
                createEnemies();
                spawn_timer = 5;
            }
            // Decrease the timer
            else if (allEnemiesAreDead() && spawn_timer > 0)
            {
                spawn_timer -= timechange;
            }
        }


        // Return whether all enemies are dead or not.
        private bool allEnemiesAreDead()
        {
            return game.Count(GameObjectType.Enemy) == 0;
        }
   }
}