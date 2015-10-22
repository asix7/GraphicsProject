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
    // Indicator of the Enemy Target
    public class Cursor : GameObject
    {

        private Enemy target;
        private float offset_y = 30;
        private float scaling = 5;

        public Enemy Target
        {
            get { return target; }
        }

        public Cursor(ProjectGame game)
        {
            this.game = game;
            model = game.Content.Load<Model>("Cursor");
            basicEffect = new BasicEffect(game.GraphicsDevice)
            {
                World = Matrix.Identity,
                View = game.camera.View,
                Projection = game.camera.Projection
            };
            BasicEffect.EnableDefaultLighting(model, true);
        }

        public override void Update(GameTime gametime)  
        {
            // Switch to a new Enemy if the previous has died
            if (target == null || !game.gameObjects.Contains(target)) {
                SwitchEnemy();                
            }

            // Change the position above the Enemy
            if (target != null)
            {
                pos = target.pos;
                pos.Y += offset_y;
            }
            basicEffect.World = Matrix.Scaling(scaling) * Matrix.Translation(pos);
        }

        // Find a new target for the player
        public void SwitchEnemy()
        {
            foreach (var obj in game.gameObjects)
            {
                if (obj.type == GameObjectType.Enemy)
                {
                    target = (Enemy)obj;
                    break;
                }
            }
        }
        
        public override void Draw(GameTime gametime)
        {
            // Draw it only if there is a target
            if (game.gameObjects.Contains(target))
            {
                model.Draw(game.GraphicsDevice, basicEffect.World, game.camera.View, game.camera.Projection);

            }
        }
    }
}
