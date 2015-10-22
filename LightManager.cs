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
    // Control the game lighting
    public class LightManager
    {
        const int MAX_LIGHTS = 4;
        private ProjectGame game;
        public struct PackedLight
        {
            public Vector4 lightPos;
            public Vector4 lightCol;
        }

        public Vector4 ambientCol;
        public PackedLight[] packedLights;

        public LightManager(ProjectGame game)
        {
            
            packedLights = new PackedLight[MAX_LIGHTS];
            // Set the ambient color equal to the Cosmic latte
            ambientCol = new Vector4(1, 0.9725f, 0.9059f, 1.0f);
            this.game = game;
        }

        // Create a Blue light if the player is on the ground, a yellow one otherwise
        public void SetPlayerLight()
        {
            Player player = game.player;
            packedLights[1].lightPos = new Vector4(player.pos.X, player.pos.Y, player.pos.Z - 10, 0f);
            //Blue
            if (player.OnGround)
            {
                packedLights[1].lightCol = new Vector4(0f, 0f, 1f, 1f);
                packedLights[1].lightCol *= 0.4f;
            }
            //Yellow
            else
            {
                packedLights[1].lightCol = new Vector4(1f, 0.7f, 0f, 1f);
                packedLights[1].lightCol *= 0.4f;
            }

        }

        // Creates a red light for every enemy onthe sky
        public void SetEnemiesLight()
        {
            int i = 2;
            // Get the position of the light according to the enemies
            foreach (var obj in game.gameObjects)
            {
                if (obj.type == GameObjectType.Enemy)
                {
                    packedLights[i].lightPos = new Vector4(obj.pos.X, obj.pos.Y, obj.pos.Z, 1f);
                    packedLights[i].lightCol = new Vector4(0.3f, 0f, 0f, 0.3f);
                    i++;
                }
            }
            // Turn the lights out if the Enemy dies
            if (i < MAX_LIGHTS)
            {
                packedLights[i].lightPos = new Vector4(0, 0, 0, 0f);
                packedLights[i].lightCol = new Vector4(0f, 0f, 0f, 0f);
            }
        }

        // Updates the parameters
        public void SetLighting(Effect effect)
        {
            effect.Parameters["lightAmbCol"].SetValue(ambientCol);
            effect.Parameters["lights"].SetValue(packedLights);
        }

        // Updates every light assigned
        public void Update()
        {
            // Turn on the general light 
            packedLights[0].lightPos = new Vector4(game.camera.Position.X, game.camera.Position.Y + 2000, game.camera.Position.Z + 2000, 1f);
            packedLights[0].lightCol = new Vector4(1f, 1f, 1f, 1f) * 0.5f;
            SetEnemiesLight();
            SetPlayerLight();
        }
    }
}
