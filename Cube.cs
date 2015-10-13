using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpDX;
using SharpDX.Toolkit;

namespace Lab4
{
    using SharpDX.Toolkit.Graphics;
    using SharpDX.Toolkit.Input;
    class Cube : GameObject
    {  
        public Cube(Lab4Game game)
        {
            vertices = Buffer.Vertex.New(
                game.GraphicsDevice,
                new[]
                    {
                        new VertexPositionColor(new Vector3(-1.0f, -1.0f, -1.0f), Color.Orange), // Front
                        new VertexPositionColor(new Vector3(-1.0f, 1.0f, -1.0f), Color.Orange),
                        new VertexPositionColor(new Vector3(1.0f, 1.0f, -1.0f), Color.Orange),
                        new VertexPositionColor(new Vector3(-1.0f, -1.0f, -1.0f), Color.Orange),
                        new VertexPositionColor(new Vector3(1.0f, 1.0f, -1.0f), Color.Orange),
                        new VertexPositionColor(new Vector3(1.0f, -1.0f, -1.0f), Color.Orange),
                        new VertexPositionColor(new Vector3(-1.0f, -1.0f, 1.0f), Color.Orange), // BACK
                        new VertexPositionColor(new Vector3(1.0f, 1.0f, 1.0f), Color.Orange),
                        new VertexPositionColor(new Vector3(-1.0f, 1.0f, 1.0f), Color.Orange),
                        new VertexPositionColor(new Vector3(-1.0f, -1.0f, 1.0f), Color.Orange),
                        new VertexPositionColor(new Vector3(1.0f, -1.0f, 1.0f), Color.Orange),
                        new VertexPositionColor(new Vector3(1.0f, 1.0f, 1.0f), Color.Orange),
                        new VertexPositionColor(new Vector3(-1.0f, 1.0f, -1.0f), Color.OrangeRed), // Top
                        new VertexPositionColor(new Vector3(-1.0f, 1.0f, 1.0f), Color.OrangeRed),
                        new VertexPositionColor(new Vector3(1.0f, 1.0f, 1.0f), Color.OrangeRed),
                        new VertexPositionColor(new Vector3(-1.0f, 1.0f, -1.0f), Color.OrangeRed),
                        new VertexPositionColor(new Vector3(1.0f, 1.0f, 1.0f), Color.OrangeRed),
                        new VertexPositionColor(new Vector3(1.0f, 1.0f, -1.0f), Color.OrangeRed),
                        new VertexPositionColor(new Vector3(-1.0f, -1.0f, -1.0f), Color.OrangeRed), // Bottom
                        new VertexPositionColor(new Vector3(1.0f, -1.0f, 1.0f), Color.OrangeRed),
                        new VertexPositionColor(new Vector3(-1.0f, -1.0f, 1.0f), Color.OrangeRed),
                        new VertexPositionColor(new Vector3(-1.0f, -1.0f, -1.0f), Color.OrangeRed),
                        new VertexPositionColor(new Vector3(1.0f, -1.0f, -1.0f), Color.OrangeRed),
                        new VertexPositionColor(new Vector3(1.0f, -1.0f, 1.0f), Color.OrangeRed),
                        new VertexPositionColor(new Vector3(-1.0f, -1.0f, -1.0f), Color.DarkOrange), // Left
                        new VertexPositionColor(new Vector3(-1.0f, -1.0f, 1.0f), Color.DarkOrange),
                        new VertexPositionColor(new Vector3(-1.0f, 1.0f, 1.0f), Color.DarkOrange),
                        new VertexPositionColor(new Vector3(-1.0f, -1.0f, -1.0f), Color.DarkOrange),
                        new VertexPositionColor(new Vector3(-1.0f, 1.0f, 1.0f), Color.DarkOrange),
                        new VertexPositionColor(new Vector3(-1.0f, 1.0f, -1.0f), Color.DarkOrange),
                        new VertexPositionColor(new Vector3(1.0f, -1.0f, -1.0f), Color.DarkOrange), // Right
                        new VertexPositionColor(new Vector3(1.0f, 1.0f, 1.0f), Color.DarkOrange),
                        new VertexPositionColor(new Vector3(1.0f, -1.0f, 1.0f), Color.DarkOrange),
                        new VertexPositionColor(new Vector3(1.0f, -1.0f, -1.0f), Color.DarkOrange),
                        new VertexPositionColor(new Vector3(1.0f, 1.0f, -1.0f), Color.DarkOrange),
                        new VertexPositionColor(new Vector3(1.0f, 1.0f, 1.0f), Color.DarkOrange),
                    });

            basicEffect = new BasicEffect(game.GraphicsDevice)
            {
                VertexColorEnabled = true,
                View = Matrix.LookAtLH(new Vector3(0, 0, -10), new Vector3(0, 0, 0), Vector3.UnitY),
                Projection = Matrix.PerspectiveFovLH((float)Math.PI / 4.0f, (float)game.GraphicsDevice.BackBuffer.Width / game.GraphicsDevice.BackBuffer.Height, 0.1f, 100.0f),
                World = Matrix.Identity
            };

            inputLayout = VertexInputLayout.FromBuffer(0, vertices);
            this.game = game;
        }

        public override void Update(GameTime gameTime)
        {
            // Rotate the cube.
            var time = (float)gameTime.TotalGameTime.TotalSeconds;
            basicEffect.World = Matrix.RotationX(time) * Matrix.RotationY(time * 2.0f) * Matrix.RotationZ(time * .7f) * Matrix.Translation(1,1,1);
            basicEffect.Projection = Matrix.PerspectiveFovLH((float)Math.PI / 4.0f, (float)game.GraphicsDevice.BackBuffer.Width / game.GraphicsDevice.BackBuffer.Height, 0.1f, 100.0f);
        }

        public override void Draw(GameTime gameTime)
        {
            // Setup the vertices
            game.GraphicsDevice.SetVertexBuffer(vertices);
            game.GraphicsDevice.SetVertexInputLayout(inputLayout);

            // Apply the basic effect technique and draw the rotating cube
            basicEffect.CurrentTechnique.Passes[0].Apply();
            game.GraphicsDevice.Draw(PrimitiveType.TriangleList, vertices.ElementCount);
        }
    }
}
