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
        private float velocity;
        private GameObjectType targetType;
        private float hitRadius = 0.5f;
        private float squareHitRadius;

        private Enemy shooter;
        private Enemy target;
        //Projectile rotation
        private Matrix rotation;
        float angleX;
        float angleY;
        private Vector3 targetPos;
        private BoundingSphere modelBound;
        private bool isTargetPlayer = false;
        private float scaling;


        // Constructor with the enemy as the shooter
        public Projectile(ProjectGame game, Enemy shooter, Vector3 pos, float velocity, Vector3 targetPos, GameObjectType targetType)
        {
            this.game = game;
            this.shooter = shooter;
            this.pos = pos;
            this.velocity = velocity;
            this.targetPos = targetPos;
            this.targetType = targetType;            
            squareHitRadius = hitRadius * hitRadius;
            collisionRadius = squareHitRadius;
            isTargetPlayer = true;
            scaling = 8;

            model = game.Content.Load<Model>("Sphere");
            basicEffect = new BasicEffect(game.GraphicsDevice)
            {
                World = Matrix.Identity,
                View = game.camera.View,
                Projection = game.camera.Projection
            };
            BasicEffect.EnableDefaultLighting(model, true);

        }

        // Constructor for the player as the shooter
        public Projectile(ProjectGame game, string model_name, Vector3 pos, float velocity, Enemy target, GameObjectType targetType)
        {
            this.game = game;
            this.pos = pos;
            this.velocity = velocity;
            this.target = target;
            this.targetType = targetType;
            hitRadius = 5;
            squareHitRadius = hitRadius * hitRadius;
            collisionRadius = squareHitRadius;
            scaling = 1.5f;

            model = game.Content.Load<Model>(model_name);
            basicEffect = new BasicEffect(game.GraphicsDevice)
            {
                World = Matrix.Identity,
                View = game.camera.View,
                Projection = game.camera.Projection
            };
            BasicEffect.EnableDefaultLighting(model, true);

        }

        // Frame update method.
        public override void Update(GameTime gameTime)
        {
            float timeDelta = (float)gameTime.ElapsedGameTime.TotalSeconds;
            float time = (float)gameTime.TotalGameTime.TotalSeconds;
            modelBound = model.CalculateBounds();

            // Apply velocity to position.
            if (isTargetPlayer){
                pos += Vector3.Normalize(new Vector3(game.player.pos.X, targetPos.Y, targetPos.Z) - pos) * timeDelta * velocity;
                pointToTarget(game.player.pos, timeDelta);
            }
            else
            {
                pos += Vector3.Normalize(new Vector3(target.pos.X, target.pos.Y, target.pos.Z) - pos) * timeDelta * velocity;
                pointToTarget(target.pos, timeDelta);
            }

            // remove the projectile if out of bound
            if (pos.Y < -10 )
            {
                game.Remove(this);
            }

            //If the enemy is dead delete the projectile
            if (!game.gameObjects.Contains(target) && !isTargetPlayer)
            {
                game.Remove(this);
            }

            // Check if collided with the target type of object.
            checkFor3DCollisions();

            basicEffect.World = (Matrix.Translation(-modelBound.Center.X, -modelBound.Center.Y, -modelBound.Center.Z) * Matrix.Scaling(scaling) * rotation) * Matrix.Translation(pos);

        }

        // Check if collided with the target type of object.
        private void checkFor3DCollisions()
        {
            foreach (var obj in game.gameObjects)
            {
                if (obj.type == targetType && ((((GameObject)obj).pos - pos).LengthSquared() <=
                    Math.Pow(((GameObject)obj).collisionRadius + this.collisionRadius, 2)))
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

        public override void Draw(GameTime gametime)
        {
            model.Draw(game.GraphicsDevice, basicEffect.World, game.camera.View, game.camera.Projection);
        }

        // Rotate according to the target position
        private void pointToTarget(Vector3 target, float timeDelta)
        {
            if (pos.Z < target.Z && pos.Y > target.Y)
            {
                angleY = (float)(Math.Atan((pos.Z - target.Z) / (pos.Y - target.Y)));
                angleX = (float)(Math.Atan((pos.X - target.X) / (pos.Z - target.Z)));
            }
            rotation = Matrix.RotationX(angleX) * Matrix.RotationY(angleY);
        }

        //Updates the last place where the projectile hit to take into account in the next calculation
        public void UpdateShooter()
        {
            // update the enemy shooter where it lands
            if (pos.Y <= 0)
            {
                shooter.LastHitPosition = pos;
            }
        }
    }


}
