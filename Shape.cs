using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpDX;
using SharpDX.Toolkit;

namespace Project
{
    using SharpDX.Toolkit.Graphics;

    abstract public class Shape : GameObject
    {
        //public BasicEffect basicEffect;
        public VertexInputLayout inputLayoutx;
		public Buffer<VertexPositionColor> vertices;
        //public ProjectGame game;

        //public abstract void Update(GameTime gametime);
        //public abstract void Draw(GameTime gametime);
    }
}
