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
    
    public enum ModelType
    {
        Colored, Textured
    }
    public class MyModel
    {
        public Buffer vertices;
        public VertexInputLayout inputLayout;
        public int vertexStride;
        public ModelType modelType;
        public Texture2D Texture;
        public float collisionRadius;
        
        public MyModel(LabGame game, VertexPositionColor[] shapeArray, String textureName, float collisionRadius)
        {
            this.vertices = Buffer.Vertex.New(game.GraphicsDevice, shapeArray);
            this.inputLayout = VertexInputLayout.New<VertexPositionColor>(0);
            vertexStride = Utilities.SizeOf<VertexPositionColor>();
            modelType = ModelType.Colored;
            this.collisionRadius = collisionRadius;
        }

        public MyModel(LabGame game, VertexPositionTexture[] shapeArray, String textureName, float collisionRadius)
        {
            this.vertices = Buffer.Vertex.New(game.GraphicsDevice, shapeArray);
            this.inputLayout = VertexInputLayout.New<VertexPositionTexture>(0);
            vertexStride = Utilities.SizeOf<VertexPositionTexture>();
            modelType = ModelType.Textured;
            Texture = game.Content.Load<Texture2D>(textureName);
            this.collisionRadius = collisionRadius;
        }
    }
}
