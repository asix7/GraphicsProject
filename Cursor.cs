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
    public class Cursor : GameObject
    {

        //Vector3 pos;
        /*
        public VertexInputLayout inputLayout;
        public Buffer<VertexPositionNormalTexture> vertices;
        public string textureName;
        public Texture2D texture;
        public int vertexStride;
        */

        private List<Enemy> enemyList;
        private Enemy target;
        private Vector3 targetPos;
        private int aimIndex;

        public Enemy Target
        {
            get { return target; }
        }
        public Cursor(ProjectGame game)
        {
            this.game = game;
            model = game.Content.Load<Model>("Simple_Ring");
            basicEffect = new BasicEffect(game.GraphicsDevice)
            {
                World = Matrix.Identity,
                View = game.camera.View,
                Projection = game.camera.Projection
            };
            BasicEffect.EnableDefaultLighting(model, true);
            aimIndex = 0;


        }
        public MyModel CreateCursorModel()
        {
            return game.assets.CreateCursor();
        }



        public override void Update(GameTime gametime)  
        {
            if (target == null || !game.gameObjects.Contains(target)) {
                SwitchEnemy();                
            }

            if (target != null)
            {
                pos = target.pos;
                pos.Y += 5;
            }
            basicEffect.World = Matrix.Scaling(5) * Matrix.Translation(pos);
            // Set view of the player same as camera to view...
            basicEffect.View = game.camera.View;
        }

        public void AddEnemy(Enemy enemy)
        {
            enemyList.Add(enemy);
        }

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
        
        public void StartAim()
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

            if (game.gameObjects.Contains(target))
            {
                model.Draw(game.GraphicsDevice, basicEffect.World, game.camera.View, game.camera.Projection);

            }
        }
    }
}
