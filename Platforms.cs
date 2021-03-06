﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpDX;
using SharpDX.Toolkit;

namespace Project
{
    using SharpDX.Toolkit.Graphics;
    public class Platform : GameObject
    {
        public Effect effect;
        public Matrix World = Matrix.Identity;
        public Matrix WorldInverseTranspose;
        private LightManager lightManager;
        public VertexInputLayout inputLayout;
        public Buffer<VertexPositionNormalTexture> vertices;
        public string textureName;
        public Texture2D texture;

        Random rnd = new Random();
        // Height of each platform level
        public float[] Levels = new float[] { 0, 10.0f, 20.0f };

        // Store the information of the last platform created and next to be created
        static public int[,] last_platform = new int[,] { { -1, 0 }, { -1, 0 }, { -1, 0 }, { -1, 0 }, { -1, 0 } };
        static public int[,] next_platform = new int[,] { { 1, 0 }, { 1, 0 }, { 1, 0 }, { 1, 0 }, { 1, 0 } };

        // Curret and Next platforms were the player is standing
        static public Platform standing_platform;
        static public Platform next_standing_platform;

        // Platform information of the instance
        public int[,] platform;

        // Z coordinate to start creating the each new platform
        public static float z_position = 0;

        // Z coordinate that the platform instance start and ends
        public float z_position_start;
        public float z_position_end;

        // Dimensions of each tile
        public float tile_width = 20.0f;
        public float tile_depth = 100.0f;
        public float tile_separation = 20.0f;

        // Platform properties
        float base_offset = 2.0f;
        public float platform_base;
        public float platfom_midpoint;

        // Min number of extra tiles according to game difficulty
        int max_extra_tiles = 4;
        static int min_extra_tiles = -1;

        // Normals
        Vector3 right_top_front_normal;
        Vector3 right_top_back_normal;
        Vector3 right_bottom_front_normal;
        Vector3 right_bottom_back_normal;

        Vector3 left_top_front_normal;
        Vector3 left_top_back_normal;
        Vector3 left_bottom_back_normal;
        Vector3 left_bottom_front_normal;

        public Platform(ProjectGame game)
        {
            // Avoid null reference, it changes on the first update
            if (standing_platform == null)
            {
                standing_platform = this;
                next_standing_platform = this;
            }
            
            // Less tiles if the difficulty is higher
            min_extra_tiles = (int)(max_extra_tiles - game.difficulty);
            type = GameObjectType.Platform;

            //Load textures
            textureName = "Platform_Textures";
            texture = game.Content.Load<Texture2D>(textureName);

            // Store information of the platform and create information for the next one
            platform = next_platform;
            next_platform = create_platform(platform); 

            // Create the vertices based on the platform information
            VertexPositionNormalTexture[] platform_vertices = create_vertices(platform, last_platform, next_platform);
            last_platform = platform;

            // Set the platform Z position of the intance and uptade the new Z position for the next one
            z_position_start = z_position;
            z_position -= tile_depth;
            z_position_end = z_position;

            // Calculates midpoint and base
            platfom_midpoint = (platform.GetLength(0) * tile_width) - tile_width/2;
            platform_base = Levels[0] - base_offset;            

            //Load effects
            effect = game.Content.Load<Effect>("Phong");
            lightManager = new LightManager(game);
            lightManager.SetLighting(effect);

            // Pass the vertices data
            vertices = Buffer.Vertex.New(game.GraphicsDevice, platform_vertices);
            inputLayout = VertexInputLayout.FromBuffer(0, vertices);
            this.game = game;
        }

        // Creates the new information for a platform
        public int[,] create_platform(int[,] previous_platform)
        {
            // Assign initial values as an emty platform
            int[,] new_platform = new int[,] { { -1, 0 }, { -1, 0 }, { -1, 0 }, { -1, 0 }, { -1, 0 } };

            // Information of the platform tiles
            List<int> current_tiles = new List<int>();
            int conserve_tile;
            int new_tile;
            int extra_tiles;

            //Extract all the non empty tiles from the previous platform and add to the list 
            for (int i = 0; i <= previous_platform.GetLength(0) - 1; i++)
            {
                if (previous_platform[i, 0] >= 0)
                {
                    current_tiles.Add(i);
                }
            }

            // Make surre that at least one tile will continue, by selecting a random one
            // and give it random values for height and texture
            conserve_tile = current_tiles[rnd.Next(0, current_tiles.Count - 1)];
            new_platform[conserve_tile, 0] = rnd.Next(0, 3);
            new_platform[conserve_tile, 1] = rnd.Next(0, 3);

            //Generate extra tiles depending on the difficulty
            extra_tiles = rnd.Next(0, 2) + min_extra_tiles;

            // Assing values to the additional tiles as random 
            for (int i = 0; i < extra_tiles; i++)
            {
                new_tile = rnd.Next(0, 5);
                // If the tile is already in the list continue
                if (new_platform[new_tile, 0] >= 0)
                {
                    i--;
                }
                else
                {
                    new_platform[new_tile, 0] = rnd.Next(0, 3);
                    new_platform[new_tile, 1] = rnd.Next(0, 3);
                }
            }
            return new_platform;
        }

        // Creates the vertices for the a platform depending on the previous one
        // Only creates necessary vertices that will be render on camera at any time
        public VertexPositionNormalTexture[] create_vertices(int[,] platform, int[,] previous_platform, int[,] next_platform)
        {
            // Reference points for the faces
            float upper, lower;
            List<VertexPositionNormalTexture> platform_vetrices = new List<VertexPositionNormalTexture>();

            // Create vertices for each tile
            for (int x = 0; x < platform.GetLength(0); x++)
            {
                if (platform[x, 0] != -1)
                {
                    //Get the texture for the tile
                    Vector2[] texture_coords = getTexure(platform[x, 1]);
                    Vector2 texture_coord1 = texture_coords[0];
                    Vector2 texture_coord2 = texture_coords[1];
                    Vector2 texture_coord3 = texture_coords[2];
                    Vector2 texture_coord4 = texture_coords[3];

                    //Normal for the vertices
                    getNormals(x, previous_platform, next_platform);
                    
                    // Change start and end position in X
                    float start_x = (tile_width * x) + (tile_separation * x);
                    float end_x = (tile_width * (x + 1 )) + (tile_separation * x);

                    lower = platform_base;
                    upper = Levels[platform[x, 0]];

                    // Create Top Surface
                    platform_vetrices.Add(new VertexPositionNormalTexture(new Vector3(start_x, upper, z_position), left_top_front_normal, texture_coord1));
                    platform_vetrices.Add(new VertexPositionNormalTexture(new Vector3(start_x, upper, z_position - tile_depth), left_top_back_normal, texture_coord2));
                    platform_vetrices.Add(new VertexPositionNormalTexture(new Vector3(end_x, upper, z_position - tile_depth), right_top_back_normal, texture_coord3));

                    platform_vetrices.Add(new VertexPositionNormalTexture(new Vector3(start_x, upper, z_position), left_top_front_normal, texture_coord1));
                    platform_vetrices.Add(new VertexPositionNormalTexture(new Vector3(end_x, upper, z_position - tile_depth), right_top_back_normal, texture_coord3));
                    platform_vetrices.Add(new VertexPositionNormalTexture(new Vector3(end_x, upper, z_position), right_top_front_normal, texture_coord4));


                    // Create Left Surfaces
                    platform_vetrices.Add(new VertexPositionNormalTexture(new Vector3(start_x, lower, z_position), left_bottom_front_normal, texture_coord1));
                    platform_vetrices.Add(new VertexPositionNormalTexture(new Vector3(start_x, lower, z_position - tile_depth), left_bottom_back_normal, texture_coord2));
                    platform_vetrices.Add(new VertexPositionNormalTexture(new Vector3(start_x, upper, z_position - tile_depth), left_top_back_normal, texture_coord3));

                    platform_vetrices.Add(new VertexPositionNormalTexture(new Vector3(start_x, lower, z_position), left_bottom_front_normal, texture_coord1));
                    platform_vetrices.Add(new VertexPositionNormalTexture(new Vector3(start_x, upper, z_position - tile_depth), left_top_back_normal, texture_coord3));
                    platform_vetrices.Add(new VertexPositionNormalTexture(new Vector3(start_x, upper, z_position), left_top_front_normal, texture_coord4));

                    // Create Right Surfaces
                    platform_vetrices.Add(new VertexPositionNormalTexture(new Vector3(end_x, lower, z_position), right_bottom_front_normal, texture_coord1));
                    platform_vetrices.Add(new VertexPositionNormalTexture(new Vector3(end_x, upper, z_position), right_top_front_normal, texture_coord2));
                    platform_vetrices.Add(new VertexPositionNormalTexture(new Vector3(end_x, upper, z_position - tile_depth), right_top_back_normal, texture_coord3));

                    platform_vetrices.Add(new VertexPositionNormalTexture(new Vector3(end_x, lower, z_position), right_bottom_front_normal, texture_coord1));
                    platform_vetrices.Add(new VertexPositionNormalTexture(new Vector3(end_x, upper, z_position - tile_depth), right_top_back_normal, texture_coord3));
                    platform_vetrices.Add(new VertexPositionNormalTexture(new Vector3(end_x, lower, z_position - tile_depth), right_bottom_back_normal, texture_coord4));
                    
                    // Create Front Wall if there is no tile behind it
                    if (previous_platform[x, 0] < platform[x, 0])
                    {
                        upper = Levels[platform[x, 0]];
                        if (previous_platform[x, 0] == -1)
                        {
                            lower = platform_base;
                        }
                        else
                        {
                            lower = Levels[previous_platform[x, 0]];
                            left_bottom_front_normal = Vector3.Normalize(new Vector3(-1, 1, 1));
                            right_bottom_front_normal = Vector3.Normalize(new Vector3(1, 1, 1));
                        }

                        platform_vetrices.Add(new VertexPositionNormalTexture(new Vector3(start_x, lower, z_position), left_bottom_front_normal, texture_coord1));
                        platform_vetrices.Add(new VertexPositionNormalTexture(new Vector3(start_x, upper, z_position), left_top_front_normal, texture_coord2));
                        platform_vetrices.Add(new VertexPositionNormalTexture(new Vector3(end_x, upper, z_position), right_top_front_normal, texture_coord3));

                        platform_vetrices.Add(new VertexPositionNormalTexture(new Vector3(start_x, lower, z_position), left_bottom_front_normal, texture_coord1));
                        platform_vetrices.Add(new VertexPositionNormalTexture(new Vector3(end_x, upper, z_position), right_top_front_normal, texture_coord3));
                        platform_vetrices.Add(new VertexPositionNormalTexture(new Vector3(end_x, lower, z_position), right_bottom_front_normal, texture_coord4));

                    }
                    
                }
            }
            return platform_vetrices.ToArray();
        }

        // Get the texture coordinates according to the tile information
        Vector2[] getTexure(int texture_number)
        {
            Vector2 texture_1;
            Vector2 texture_2;
            Vector2 texture_3;
            Vector2 texture_4;

            if (texture_number == 0)
            {
                texture_1 = new Vector2(0, 0);
                texture_2 = new Vector2(0, 1);
                texture_3 = new Vector2(1 / 3.0f, 1);
                texture_4 = new Vector2(1 / 3.0f, 0);
            }
            else if (texture_number == 1)
            {
                texture_1 = new Vector2(1 / 3.0f, 0);
                texture_2 = new Vector2(1 / 3.0f, 1);
                texture_3 = new Vector2(2 / 3.0f, 1);
                texture_4 = new Vector2(2 / 3.0f, 0);
            }
            else
            {
                texture_1 = new Vector2(2 / 3.0f, 0);
                texture_2 = new Vector2(2 / 3.0f, 1);
                texture_3 = new Vector2(1, 1);
                texture_4 = new Vector2(1, 0);
            }

            return new Vector2[4] { texture_1, texture_2, texture_3, texture_4 };
        }

        // Change the value of the normals
        void getNormals(int index, int[,] previous_platform, int[,] next_platform)
        {

            if (previous_platform[index, 0] == -1)
            {
                left_bottom_front_normal = Vector3.Normalize(new Vector3(-1, -1, 1));
                left_top_front_normal = Vector3.Normalize(new Vector3(-1, 1, 1));
            }
            else
            {
                left_bottom_front_normal = Vector3.Normalize(new Vector3(-1, -1, 0));

                if (previous_platform[index, 0] < platform[index, 0])
                {
                    left_top_front_normal = Vector3.Normalize(new Vector3(-1, 1, 1));
                }
                else if (previous_platform[index, 0] == platform[index, 0])
                {
                    left_top_front_normal = Vector3.Normalize(new Vector3(-1, 1, 0));
                }
                else
                {
                    left_top_front_normal = Vector3.Normalize(new Vector3(-1, 1, -1));
                }
            }

            if (next_platform[index, 0] == -1)
            {
                left_bottom_back_normal = Vector3.Normalize(new Vector3(-1, -1, -1));
                left_top_back_normal = Vector3.Normalize(new Vector3(-1, 1, -1));
            }
            else
            {
                left_bottom_back_normal = Vector3.Normalize(new Vector3(-1, -1, 0));
                if (platform[index, 0] > next_platform[index, 0])
                {
                    left_top_back_normal = Vector3.Normalize(new Vector3(-1, 1, -1));
                }
                else if (platform[index, 0] == next_platform[index, 0])
                {
                    left_top_back_normal = Vector3.Normalize(new Vector3(-1, 1, 0));
                }
                else
                {
                    left_top_back_normal = Vector3.Normalize(new Vector3(-1, 1, 1));
                }
            }

            if (previous_platform[index, 0] == -1)
            {
                right_bottom_front_normal = Vector3.Normalize(new Vector3(1, -1, 1));
                right_top_front_normal = Vector3.Normalize(new Vector3(1, 1, 1));
            }
            else
            {
                right_bottom_front_normal = Vector3.Normalize(new Vector3(1, -1, 0));
                if (previous_platform[index, 0] < platform[index, 0])
                {
                    right_top_front_normal = Vector3.Normalize(new Vector3(1, 1, 1));
                }
                else if (previous_platform[index, 0] == platform[index, 0])
                {
                    right_top_front_normal = Vector3.Normalize(new Vector3(1, 1, 0));
                }
                else
                {
                    right_top_front_normal = Vector3.Normalize(new Vector3(1, 1, -1));
                }
            }

            if (next_platform[index, 0] == -1)
            {
                right_bottom_back_normal = Vector3.Normalize(new Vector3(1, -1, -1));
                right_top_back_normal = Vector3.Normalize(new Vector3(1, 1, -1));
            }
            else
            {
                right_bottom_back_normal = Vector3.Normalize(new Vector3(1, -1, 0));
                if (platform[index, 0] > next_platform[index, 0])
                {
                    right_top_back_normal = Vector3.Normalize(new Vector3(1, 1, -1));
                }
                else if (platform[index, 0] == next_platform[index, 0])
                {
                    right_top_back_normal = Vector3.Normalize(new Vector3(1, 1, 0));
                }
                else
                {
                    right_top_back_normal = Vector3.Normalize(new Vector3(1, 1, 1));
                }
            }
        }

        // Change the first platform position and its information 
        // The new position is set after the current last platform
        public void change_platform()
        {
            // Changes platform information
            platform = next_platform;
            next_platform = create_platform(platform);

            // Creates the new vertices and set this platform as the last platform
            VertexPositionNormalTexture[] platform_vertices = create_vertices(platform, last_platform, next_platform);
            vertices = Buffer.Vertex.New(game.GraphicsDevice, platform_vertices);
            last_platform = platform;

            // Updates Z positions
            z_position_start = z_position;
            z_position -= tile_depth;
            z_position_end = z_position;
        }

        public override void Draw(GameTime gameTime)
        {
            // Setup the effect parameters
            effect.Parameters["World"].SetValue(World);
            effect.Parameters["Projection"].SetValue(game.camera.Projection);
            effect.Parameters["View"].SetValue(game.camera.View);
            effect.Parameters["cameraPos"].SetValue(game.camera.Position);
            effect.Parameters["worldInvTrp"].SetValue(WorldInverseTranspose);
            effect.Parameters["ModelTexture"].SetResource(texture);

            // Change lights
            lightManager.SetLighting(effect);
            // Setup the vertices
            game.GraphicsDevice.SetVertexBuffer(vertices);
            game.GraphicsDevice.SetVertexInputLayout(inputLayout);

            // Apply the effect technique and draw the platform
            effect.CurrentTechnique.Passes[0].Apply();
            game.GraphicsDevice.Draw(PrimitiveType.TriangleList, vertices.ElementCount);
        }

        // Updates the platform that the player is standing on
        public void Update_player_platforms()
        {
            Vector3 player_position = game.player.pos;
            if ((player_position.Z <= z_position_start) && (player_position.Z > z_position_end))
            {
                standing_platform = this;
            }
            if ((player_position.Z + tile_depth >= z_position_start) && (player_position.Z + tile_depth < z_position_end))
            {

            }
        }

        public override void Update(GameTime gameTime)
        {
            // Change the platform if the camera passed it 
            if (game.camera.Position.Z < z_position_end)
            {
                change_platform();
            }

            Update_player_platforms();
            min_extra_tiles = (int)(max_extra_tiles - game.difficulty);

            WorldInverseTranspose = Matrix.Transpose(Matrix.Invert(World));

            //Update the parameters
            lightManager.Update();
            lightManager.SetLighting(effect);
            effect.Parameters["World"].SetValue(World);
            effect.Parameters["Projection"].SetValue(game.camera.Projection);
            effect.Parameters["View"].SetValue(game.camera.View);
            effect.Parameters["cameraPos"].SetValue(game.camera.Position);
            effect.Parameters["worldInvTrp"].SetValue(WorldInverseTranspose);
        }
    }
}
