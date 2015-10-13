using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using SharpDX.Toolkit;
namespace Project
{
    public class Camera
    {
        public Matrix View;
        public Matrix Projection;
        public Game game;

        // Attributes
        private Vector3 thirdPersonRef;
        private Vector3 cameraPos;
        private Vector3 cameraTarget;
        private Vector3 cameraUp;
        public float cameraSpeed = 1f;

        private Vector3 cameraRot;
        public Vector3 viewVector;

        // Properties
        public Vector3 Position
        {
            get { return cameraPos; }
            set
            {
                cameraPos = value;
                UpdateTarget();
            }
        }
        public Vector3 Rotation
        {
            get { return cameraRot; }
            set
            {
                cameraRot = value;
                UpdateTarget();
            }
        }

        public Vector3 Target
        {
            get { return cameraTarget; }
        }
        public Vector3 Up
        {
            get { return cameraUp; }
        }
        // Ensures that all objects are being rendered from a consistent viewpoint
        public Camera(LabGame game)
        {
            thirdPersonRef = new Vector3(0, 0, -15);
            cameraPos = new Vector3(0, 0, -15);
            cameraTarget = new Vector3(0, 0, 0);
            cameraUp = Vector3.UnitY;

            View = Matrix.LookAtLH(cameraPos, cameraTarget, cameraUp);
            Projection = Matrix.PerspectiveFovLH((float)Math.PI / 4.0f, (float)game.GraphicsDevice.BackBuffer.Width / game.GraphicsDevice.BackBuffer.Height, 0.1f, 100.0f);
            this.game = game;
        }

        // If the screen is resized, the projection matrix will change
        public void Update(GameTime gameTime, Vector3 playerPos)
        {
            Vector3 moveVector = Vector3.Zero;

            float time = (float)gameTime.TotalGameTime.TotalSeconds;
            float timechange = (float)gameTime.ElapsedGameTime.TotalSeconds;

            //Position = playerPos;


            // The camera Z is moving along with player;
            moveVector.Z = (thirdPersonRef.Z + playerPos.Z);
            //MoveCamera(moveVector);
            Position = moveVector;


            Projection = Matrix.PerspectiveFovLH((float)Math.PI / 4.0f, (float)game.GraphicsDevice.BackBuffer.Width / game.GraphicsDevice.BackBuffer.Height, 0.1f, 100.0f);
            View = Matrix.LookAtLH(cameraPos, cameraTarget, cameraUp);
            viewVector = cameraTarget - cameraPos;


        }

        //Set camera's position and rotation
        private void MoveTo(Vector3 pos, Vector3 rot)
        {

            Position = pos;
            Rotation = rot;
        }

        // Update target vector
        private void UpdateTarget()
        {
            // Build a rotation matrix
            Matrix rotationMatrix = Matrix.RotationX(cameraRot.X) * Matrix.RotationY(cameraRot.Y);

            // Build target offset vector
            Vector3 targetOffset = Vector3.TransformCoordinate(Vector3.UnitZ, rotationMatrix);

            // Update our camera's  look at vector
            cameraTarget = cameraPos + targetOffset;

            cameraUp = Vector3.TransformCoordinate(Vector3.UnitY, rotationMatrix);
        }

        // Update camera position
        private void UpdateCameraPosition(Vector3 playerPos)
        {
            // Build a rotation matrix
            Matrix rotationMatrix = Matrix.RotationX(cameraRot.X) * Matrix.RotationY(cameraRot.Y);

            // Build transform reference
            Vector3 transformRef = Vector3.TransformCoordinate(thirdPersonRef, rotationMatrix);

            Position = transformRef + playerPos;
        }

        // Simulates movement, can be use to test collider
        private Vector3 TestMove(Vector3 amount)
        {
            // Build a rotation matrix
            Matrix rotate = Matrix.RotationX(cameraRot.X) * Matrix.RotationY(cameraRot.Y) * Matrix.RotationZ(cameraRot.Z);

            // Create a movement vector
            Vector3 movement = new Vector3(amount.X, amount.Y, amount.Z);
            movement = Vector3.TransformCoordinate(movement, rotate);


            return cameraPos + movement;
        }

        // Move the camera
        private void MoveCamera(Vector3 scale)
        {
            MoveTo(TestMove(scale), Rotation);
        }


    }
}
