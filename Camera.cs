﻿using System;
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
        public Vector3 cameraPos;
        private Vector3 cameraTarget;
        private Vector3 cameraUp;
        private float z_distance_from_player = 125.0f;
        private float z_distance_from_target = 1000.0f;
        private float y_camera_position = 50.0f;
        private float platform_midpoint = 50.0f;


        // Properties
        public Vector3 Position
        {
            get { return cameraPos; }
            set
            {
                cameraPos = value;
            }
        }


        // Set the intil values for the camera
        public Camera(ProjectGame game)
        {
            cameraPos = new Vector3(0, y_camera_position, 0);
            cameraTarget = new Vector3(-platform_midpoint, 0, 0);
            cameraUp = Vector3.UnitY;

            View = Matrix.LookAtRH(cameraPos, cameraTarget, cameraUp);
            Projection = Matrix.PerspectiveFovRH((float)Math.PI / 4.0f, (float)game.GraphicsDevice.BackBuffer.Width / game.GraphicsDevice.BackBuffer.Height, 0.1f, 1000.0f);
            this.game = game;
        }

        // If the screen is resized, the projection matrix will change
        public void Update(GameTime gameTime, Vector3 playerPos)
        {

            // Camera is updated according to the current player position
            cameraPos.Z = playerPos.Z + z_distance_from_player;
            cameraTarget.Z = playerPos.Z - z_distance_from_target;
            cameraPos.X = playerPos.X;

            // Change the projection and view
            Projection = Matrix.PerspectiveFovRH((float)Math.PI / 4.0f, (float)game.GraphicsDevice.BackBuffer.Width / game.GraphicsDevice.BackBuffer.Height, 0.1f, 2000.0f);
            View = Matrix.LookAtRH(cameraPos, cameraTarget, cameraUp);
        }

    }
}
