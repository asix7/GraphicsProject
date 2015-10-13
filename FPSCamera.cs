using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX.Windows;
using SharpDX;
using SharpDX.Toolkit;

namespace Project
{

    using SharpDX.Toolkit.Graphics;
    using SharpDX.Toolkit.Input;

    public class FPSCamera
    {
        public Matrix View;
        public Matrix Projection;

        // Attributes
        private Vector3 cameraPos;
        private Vector3 cameraTarget;
        private Vector3 cameraUp;
        public float cameraSpeed = 20f;

        private Vector3 cameraRot;
        public Vector3 viewVector;
        private Vector3 mouseRotationBuffer;
 

        private KeyboardManager keyboardManager;
        private KeyboardState keyboardState;
        private MouseManager mouseManager;
        private MouseState currentMouseState;
        private MouseState previousMouseState;

        private Vector3 minBound;
        private Vector3 maxBound;
        private bool bounded = false;
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

        public Vector3 Up
        {
            get { return cameraUp; }
        }
        // Ensures that all objects are being rendered from a consistent viewpoint
        public FPSCamera(LabGame game)
        {

            cameraPos = new Vector3(25, 20, 10);
            cameraTarget = new Vector3(0, 0, 1);
            cameraUp = Vector3.UnitY;

            View = Matrix.LookAtLH(cameraPos, cameraTarget, cameraUp);
            Projection = Matrix.PerspectiveFovLH((float)Math.PI / 4.0f, (float)game.GraphicsDevice.BackBuffer.Width / game.GraphicsDevice.BackBuffer.Height, 0.1f, 100.0f);

            keyboardManager = game.keyboardManager;
            mouseManager = game.mouseManager;

            previousMouseState = mouseManager.GetState();
        }

        // If the screen is resized, the projection matrix will change
        public void Update(LabGame game, GameTime gameTime)
        {
            keyboardState = game.keyboardManager.GetState();
            currentMouseState = mouseManager.GetState();

            Vector3 moveVector = Vector3.Zero;

            float time = (float)gameTime.TotalGameTime.TotalSeconds;
            float timechange = (float)gameTime.ElapsedGameTime.TotalSeconds;
            
            // Basic keyboard movement
            if (keyboardState.IsKeyDown(Keys.A)) { moveVector.X--; }
            if (keyboardState.IsKeyDown(Keys.D)) { moveVector.X++; }
            if (keyboardState.IsKeyDown(Keys.W)) { moveVector.Z++; }
            if (keyboardState.IsKeyDown(Keys.S)) { moveVector.Z--; }

            
            if (moveVector != Vector3.Zero)
            {
                // Normalize vector, no fast diagonally movement
                moveVector.Normalize();

                // smooth and speed
                moveVector *= timechange * cameraSpeed;

                MoveCamera(moveVector);
            }
            
            if (keyboardState.IsKeyDown(Keys.Q)) {
                mouseRotationBuffer.X += 2f* timechange;
                Rotation = new Vector3(0,
                       -MathUtil.Wrap(mouseRotationBuffer.X, MathUtil.DegreesToRadians(-180f), MathUtil.DegreesToRadians(180f)), 0);
            }

            if (keyboardState.IsKeyDown(Keys.E)) {
                mouseRotationBuffer.X -= 2f * timechange;
                Rotation = new Vector3(0,
                           -MathUtil.Wrap(mouseRotationBuffer.X, MathUtil.DegreesToRadians(-180f), MathUtil.DegreesToRadians(180f)), 0);
            }

            // Handle mouse movement
            if (currentMouseState != previousMouseState)
            {
                // Cache mouse location
                float deltaX = currentMouseState.X - 0.5f;
                float deltaY = currentMouseState.Y - 0.5f;


                mouseRotationBuffer.X -= 10f * deltaX * timechange;
                mouseRotationBuffer.Y -= 10f * deltaY * timechange;
                
                // Wrap how high our Y axis of mouse can move, prevent 360 degree movement
                if (mouseRotationBuffer.Y < MathUtil.DegreesToRadians(-75.0f))
                    mouseRotationBuffer.Y -= (mouseRotationBuffer.Y - MathUtil.DegreesToRadians(-75.0f));
                if (mouseRotationBuffer.Y > MathUtil.DegreesToRadians(75.0f))
                    mouseRotationBuffer.Y -= (mouseRotationBuffer.Y - MathUtil.DegreesToRadians(75.0f));
                    
                Rotation = new Vector3(-MathUtil.Clamp(mouseRotationBuffer.Y, MathUtil.DegreesToRadians(-75.0f), MathUtil.DegreesToRadians(75.0f)),
                   -MathUtil.Wrap(mouseRotationBuffer.X, MathUtil.DegreesToRadians(-180f), MathUtil.DegreesToRadians(180f)), 0);
                   
            }
            // set mouse position at middle of the window
            mouseManager.SetPosition(new Vector2(0.5f,0.5f));
            // cache previous mouse state
            previousMouseState = currentMouseState;

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

        // Simulates movement, can be use to test collider
        private Vector3 TestMove(Vector3 amount)
        {
            // Build a rotation matrix
            Matrix rotate = Matrix.RotationX(cameraRot.X) * Matrix.RotationY(cameraRot.Y) * Matrix.RotationZ(cameraRot.Z);

            // Create a movement vector
            Vector3 movement = new Vector3(amount.X, amount.Y, amount.Z);
            movement = Vector3.TransformCoordinate(movement, rotate);

            // check boundary
            if (bounded)
            {
                if ((cameraPos.X + movement.X) > maxBound.X || (cameraPos.Z + movement.Z) > maxBound.Z ||
                    (cameraPos.X - movement.X) < minBound.X || (cameraPos.Z - movement.Z) < minBound.Z)
                {
                    return cameraPos;
                }
            }
            
            return cameraPos + movement;
        }

        // Move the camera
        private void MoveCamera(Vector3 scale)
        {
            MoveTo(TestMove(scale), Rotation);
        }

        // Set boundary to the camera
        public void BoundCamera(Vector3 min, Vector3 max)
        {
            bounded = true;

            minBound = min;
            maxBound = max;
        }

    }
}