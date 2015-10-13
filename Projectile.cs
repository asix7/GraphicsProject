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
    // Projectile classed, used by both player and enemy.
    class Projectile : GameObject
    {
        private Vector3 vel;
        private GameObjectType targetType;
        private float hitRadius = 0.5f;
        private float squareHitRadius;

        // Constructor.
        public Projectile(LabGame game, MyModel myModel, Vector3 pos, Vector3 vel, GameObjectType targetType)
        {
            this.game = game;
            this.myModel = myModel;
            this.pos = pos;
            this.vel = vel;
            this.targetType = targetType;
            squareHitRadius = hitRadius * hitRadius;
            GetParamsFromModel();
        }

        // TASK 3
        // Frame update method.
        public override void Update(GameTime gameTime)
        {
            float timeDelta = (float)gameTime.ElapsedGameTime.TotalSeconds;
            float time = (float)gameTime.TotalGameTime.TotalSeconds;
            // Apply velocity to position.
            pos += vel * timeDelta;

            // Remove self if off screen.
            if (
                pos.X < game.boundaryLeft ||
                pos.X > game.boundaryRight ||
                pos.Y < game.boundaryBottom ||
                pos.Y > game.boundaryTop
                )
            {
                game.Remove(this);
            }

            // Set local transformation to be spinning according to time for fun.
            basicEffect.World = Matrix.RotationY(time) * Matrix.RotationZ(time * time) * Matrix.Translation(pos);

            // Check if collided with the target type of object.
            checkForCollisions();
        }

        // Check if collided with the target type of object.
        private void checkForCollisions()
        {
            foreach (var obj in game.gameObjects)
            {
                if (obj.type == targetType && ((((GameObject)obj).pos - pos).LengthSquared() <= 
                    Math.Pow(((GameObject)obj).myModel.collisionRadius + this.myModel.collisionRadius, 2)))
                {
                    // Cast to object class and call Hit method.
                    switch (obj.type)
                    {
                        case GameObjectType.Player:
                            ((Player)obj).Hit();
                            break;
                        case GameObjectType.Enemy:
                            ((Enemy)obj).Hit();
                            break;
                    }

                    // Destroy self.
                    game.Remove(this);
                }
            }
        }
    }
}
