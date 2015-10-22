using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpDX;

namespace Project
{
    // TASK 1: This class could represent a point light source
    class Light
    {
        public Vector4 lightPos;
        public Vector4 lightCol;
         
        public Light(Vector4 lightPos, Vector4 lightCol)
        {
            this.lightPos = lightPos;
            this.lightCol = lightCol;
        }
    }
}
