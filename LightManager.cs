using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpDX;
using SharpDX.Toolkit;

namespace Project
{
    using SharpDX.Toolkit.Graphics;
    using SharpDX.Toolkit.Input;
    // This class is optional.  It just makes it a bit easier to see what's been added, and to organise lighting separately from other code.
    class LightManager
    {
        const int MAX_LIGHTS = 3;
        private ProjectGame game;
        public struct PackedLight
        {
            public Vector4 lightPos;
            public Vector4 lightCol;
        }

        public Vector4 ambientCol;
        // TASKS 3 & 6: Note that an array of PackedLights has been used here, rather than the Light Class defined before.
        // This is not required, but somewhat more efficient as a single array can be passed to the shader rather than individual Lights.
        // It also makes it easier to extend the number of lights (as in Task 6)
        public PackedLight[] packedLights;
        public LightManager(ProjectGame game)
        {
            packedLights = new PackedLight[MAX_LIGHTS];
            ambientCol = new Vector4(0.4f, 0.4f, 0.4f, 1.0f);
            // TASK 3: Initialise lights

            packedLights[1].lightPos = new Vector4(game.camera.Position.X, game.camera.Position.Y, game.camera.Position.Z, 1f);
            packedLights[1].lightCol = new Vector4(1f, 1f, 1f, 1f);
            


            this.game = game;
        }

        public void SetLighting(Effect effect)
        {
            // TASK 2: Pass parameters to shader
            effect.Parameters["lightAmbCol"].SetValue(ambientCol);
            effect.Parameters["lights"].SetValue(packedLights);
        }

        // TASK 5: Keyboard control
        // This could be alternatively be accomplished by passing an intensity value to the shader, however this is mathematically idential and requires no extra data transfer 
        public void Update()
        {
            packedLights[1].lightPos = new Vector4(game.camera.Position.X - 100, game.camera.Position.Y, game.camera.Position.Z + 100, 1f);
            if (game.keyboardState.IsKeyDown(Keys.Up))
            {
                for (int i = 0; i < MAX_LIGHTS; i++)
                {
                    packedLights[i].lightCol = packedLights[i].lightCol * 1.05f;
                }
            }

            if (game.keyboardState.IsKeyDown(Keys.Down))
            {
                for (int i = 0; i < MAX_LIGHTS; i++)
                {
                    packedLights[i].lightCol = packedLights[i].lightCol * 0.95f;
                }
            }
        }
    }
}
