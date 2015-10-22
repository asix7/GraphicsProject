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
        private Matrix rotation;
        private Vector3 targetPos;
        private BoundingSphere modelBound;
        private bool isTargetPlayer = false;

        float angleX;
        float angleY;
        // Constructor.
        public Projectile(ProjectGame game, Enemy shooter, Vector3 pos, float velocity, Vector3 targetPos, GameObjectType targetType)
        {
            this.game = game;
            this.shooter = shooter;
            this.pos = pos;
            this.velocity = velocity;
            //this.targetPos = targetPos;
            this.targetPos = targetPos;
            this.targetType = targetType;
            squareHitRadius = hitRadius * hitRadius;
            collisionRadius = squareHitRadius;
            isTargetPlayer = true;
            //GetParamsFromModel();

            model = game.Content.Load<Model>("STDARM");
            basicEffect = new BasicEffect(game.GraphicsDevice)
            {
                World = Matrix.Identity,
                View = game.camera.View,
                Projection = game.camera.Projection
            };
            BasicEffect.EnableDefaultLighting(model, true);

        }

        public Projectile(ProjectGame game, string model_name, Vector3 pos, float velocity, Enemy target, GameObjectType targetType)
        {
            this.game = game;
            this.pos = pos;
            this.velocity = velocity;
            this.target = target;
            this.targetType = targetType;
            squareHitRadius = hitRadius * hitRadius;
            collisionRadius = squareHitRadius;

            model = game.Content.Load<Model>(model_name);
            basicEffect = new BasicEffect(game.GraphicsDevice)
            {
                World = Matrix.Identity,
                View = game.camera.View,
                Projection = game.camera.Projection
            };
            BasicEffect.EnableDefaultLighting(model, true);

        }

        // TASK 3
        // Frame update method.
        public override void Update(GameTime gameTime)
        {
            float timeDelta = (float)gameTime.ElapsedGameTime.TotalSeconds;
            float time = (float)gameTime.TotalGameTime.TotalSeconds;
            modelBound = model.CalculateBounds();

            // Apply velocity to position.
            if (isTargetPlayer)
                pos += Vector3.Normalize(new Vector3(game.player.pos.X, targetPos.Y, targetPos.Z) - pos) * timeDelta * velocity;
            else
                pos += Vector3.Normalize(new Vector3(target.pos.X, target.pos.Y, target.pos.Z) - pos) * timeDelta * velocity;

            pointToTarget(game.player.pos, timeDelta);
            basicEffect.World = (Matrix.Translation(-modelBound.Center.X, -modelBound.Center.Y, -modelBound.Center.Z) * Matrix.Scaling(1) * rotation) * Matrix.Translation(pos);

            // remove the projectile if out of bound
            if (pos.Y < -10 || pos.Y > 110)
            {
                game.Remove(this);
            }

            if (!game.gameObjects.Contains(target))
            {
                game.Remove(this);
            }

                // Check if collided with the target type of object.
                //checkForCollisions();
                checkFor3DCollisions();
            //check3DCollision();
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

        // Check if the X and Y collided with the target type object
        private void check2DCollisions()
        {
            foreach (var obj in game.gameObjects)
            {
                Vector2 target2DPos = new Vector2(((GameObject)obj).pos.X, ((GameObject)obj).pos.Y);
                Vector2 self2DPos = new Vector2(pos.X, pos.Y);
                if (obj.type == targetType && (((target2DPos - self2DPos).LengthSquared() <=
                    Math.Pow(((GameObject)obj).myModel.collisionRadius + this.myModel.collisionRadius, 2))))
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

        public void TowardTarget(Vector3 target, float timeDelta)
        {
            //Vector3 target = game.player.pos;
            double angleY = Math.Atan((pos.Z - target.Z) / (pos.Y - target.Y));
            //double angleX = Math.Atan((pos.X - target.X) / (pos.Z - target.Z));
            //rotation = Matrix.RotationX((float)angleX)*Matrix.RotationY(-(float)angleY);
            //rotation = Matrix.RotationY((float)angleY);
            Vector3 direction = Vector3.Normalize(targetPos - pos);

            //direction = Vector3.TransformCoordinate(direction, rotation);
            pos += direction * 200 * timeDelta;
        }

        private void pointToTarget(Vector3 target, float timeDelta)
        {
            if (pos.Z < target.Z && pos.Y > target.Y)
            {
                angleY = (float)(Math.Atan((pos.Z - target.Z) / (pos.Y - target.Y)));
                angleX = (float)(Math.Atan((pos.X - target.X) / (pos.Z - target.Z)));
            }
            rotation = Matrix.RotationX(angleX) * Matrix.RotationY(angleY);
        }

        public void UpdateShooter()
        {
            // update the enemy shooter where it lands
            // could do collision detect with the platform
            if (pos.Y <= 0)
            {
                shooter.LastHitPosition = pos;
            }
        }
    }


}
