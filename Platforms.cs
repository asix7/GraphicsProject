using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpDX;
using SharpDX.Toolkit;

namespace Project
{
    using SharpDX.Toolkit.Graphics;
    public class Platform
    {
        public Effect effect;
        public ProjectGame game;
        public Matrix World = Matrix.Identity;
        public Matrix WorldInverseTranspose;
        public VertexInputLayout inputLayout;
        public Buffer<VertexPositionNormalTexture> vertices;
        public string textureName;
        public Texture2D texture;

        Random rnd = new Random();
        // Height of each platform level
        public float[] Levels = new float[] { 0, 10.0f, 20.0f };
        // Last platform created
        static public int[,] last_platform = new int[,] { { -1, 0 }, { -1, 0 }, { 0, 0 }, { -1, 0 }, { -1, 0 } };
        // Curret platform were the player is standing and the next one
        static public Platform standing_platform;
        static public Platform next_platform;
                        
        // Platform information of the instance
        public int[,] platform;

        // Z coordinate to start creating the each new platform
        static float z_position = 0;
        // Z coordinate that the platform instance start and ends
        public float z_position_start;
        public float z_position_end;

        // Dimensions of each tile
        public float tile_width = 20.0f;
        public float tile_depth = 100.0f;

        // Off set from the lower level to render walls
        float base_offset = 2.0f;

        // Min number of extra tiles according to game difficulty
        int max_extra_tiles = 4;
        static int min_extra_tiles = -1;
        
        public Platform(ProjectGame game) 
        {
            // Avoid null reference, it changes on the first update
            if (standing_platform == null)
            {
                standing_platform = this;
            }

            min_extra_tiles = (int)(max_extra_tiles - game.difficulty);

            //Load textures
            textureName = "Platform_textures.jpg";
            texture = game.Content.Load<Texture2D>(textureName);

            // create new platform information according to the last
            int[,] previous_platform = last_platform;
            platform = create_platform(previous_platform);

            // Create the vertices based on the platform information
            VertexPositionNormalTexture[] platform_vertices = create_vertices(platform, previous_platform);
            last_platform = platform;
            
            // Set the platform Z position of the intance and uptade the new Z position for the next one
            z_position_start = z_position;
            z_position += tile_depth;
            z_position_end = z_position;

            vertices = Buffer.Vertex.New(game.GraphicsDevice, platform_vertices);
            effect = game.Content.Load<Effect>("Platform");
            inputLayout = VertexInputLayout.FromBuffer(0, vertices);
            this.game = game;
        }

        // Change the first platform position and its information 
        // The new position is set after the current last platform
        public void change_platform()
        {
            // Changes platform information
            int[,] previous_platform = last_platform;
            platform = create_platform(previous_platform);

            // Creates the new vertices and set this platform as the last platform
            VertexPositionNormalTexture[] platform_vertices = create_vertices(platform, previous_platform);
            vertices = Buffer.Vertex.New(game.GraphicsDevice, platform_vertices);
            last_platform = platform;

            // Updates Z positions
            z_position_start = z_position;
            z_position += tile_depth;
            z_position_end = z_position;             
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
        public VertexPositionNormalTexture[] create_vertices(int[,] platform, int[,] previous_platform)
        {

            float upper, lower;
            float platform_base = Levels[0] - base_offset;
            List<VertexPositionNormalTexture> platform_vetrices = new List<VertexPositionNormalTexture>();

            //Normal for the vertices

            Vector3 right_top_front_normal = Vector3.Normalize(new Vector3(1, 1, -1));
            Vector3 right_top_back_normal = Vector3.Normalize(new Vector3(1, 1, 1));
            Vector3 right_bottom_front_normal = Vector3.Normalize(new Vector3(1, -1, -1));
            Vector3 right_bottom_back_normal = Vector3.Normalize(new Vector3(1, -1, 1));


            Vector3 left_top_front_normal = Vector3.Normalize(new Vector3(-1, 1, -1));
            Vector3 left_top_back_normal = Vector3.Normalize(new Vector3(-1, 1, 1));
            Vector3 left_bottom_back_normal = Vector3.Normalize(new Vector3(-1, -1, -1));
            Vector3 left_bottom_front_normal = Vector3.Normalize(new Vector3(-1, -1, -1));

            for (int x = 0; x < platform.GetLength(0); x++)
            {
                if (platform[x, 0] != -1)
                {
                    lower = platform_base;
                    upper = Levels[platform[x, 0]];

                    // Create Top Surface
                    platform_vetrices.Add(new VertexPositionNormalTexture(new Vector3(tile_width * x, upper, z_position), left_top_front_normal, new Vector2(0, 0)));
                    platform_vetrices.Add(new VertexPositionNormalTexture(new Vector3(tile_width * x, upper, z_position + tile_depth), left_top_back_normal, new Vector2(0, 1)));
                    platform_vetrices.Add(new VertexPositionNormalTexture(new Vector3(tile_width * (x + 1), upper, z_position + tile_depth), right_top_back_normal, new Vector2(1, 1)));

                    platform_vetrices.Add(new VertexPositionNormalTexture(new Vector3(tile_width * x, upper, z_position), left_top_front_normal, new Vector2(0, 0)));
                    platform_vetrices.Add(new VertexPositionNormalTexture(new Vector3(tile_width * (x + 1), upper, z_position + tile_depth), right_top_back_normal, new Vector2(0, 1)));
                    platform_vetrices.Add(new VertexPositionNormalTexture(new Vector3(tile_width * (x + 1), upper, z_position), right_top_front_normal, new Vector2(1, 1)));

                    // Create Left Wall if there are no adjacent tiles
                    if (x == 0 || platform[x - 1, 0] < 0)
                    {
                        platform_vetrices.Add(new VertexPositionNormalTexture(new Vector3(tile_width * x, lower, z_position), left_bottom_front_normal, new Vector2(0, 0)));
                        platform_vetrices.Add(new VertexPositionNormalTexture(new Vector3(tile_width * x, lower, z_position + tile_depth), left_bottom_back_normal, new Vector2(0, 1)));
                        platform_vetrices.Add(new VertexPositionNormalTexture(new Vector3(tile_width * x, upper, z_position + tile_depth), left_top_back_normal, new Vector2(1, 1)));

                        platform_vetrices.Add(new VertexPositionNormalTexture(new Vector3(tile_width * x, lower, z_position), left_bottom_front_normal, new Vector2(0, 0)));
                        platform_vetrices.Add(new VertexPositionNormalTexture(new Vector3(tile_width * x, upper, z_position + tile_depth), left_top_back_normal, new Vector2(0, 1)));
                        platform_vetrices.Add(new VertexPositionNormalTexture(new Vector3(tile_width * x, upper, z_position), left_top_front_normal, new Vector2(1, 1)));
                    }

                    // Create Right Wall if there are no adjacent tiles
                    if (x == platform.GetLength(0) - 1 || platform[x + 1, 0] < 0)
                    {
                        platform_vetrices.Add(new VertexPositionNormalTexture(new Vector3(tile_width * (x + 1), lower, z_position), right_bottom_front_normal, new Vector2(0, 0)));
                        platform_vetrices.Add(new VertexPositionNormalTexture(new Vector3(tile_width * (x + 1), upper, z_position), right_top_front_normal, new Vector2(0, 1)));
                        platform_vetrices.Add(new VertexPositionNormalTexture(new Vector3(tile_width * (x + 1), upper, z_position + tile_depth), right_top_back_normal, new Vector2(1, 1)));

                        platform_vetrices.Add(new VertexPositionNormalTexture(new Vector3(tile_width * (x + 1), lower, z_position), right_bottom_front_normal, new Vector2(0, 0)));
                        platform_vetrices.Add(new VertexPositionNormalTexture(new Vector3(tile_width * (x + 1), upper, z_position + tile_depth), right_top_back_normal, new Vector2(0, 1)));
                        platform_vetrices.Add(new VertexPositionNormalTexture(new Vector3(tile_width * (x + 1), lower, z_position + tile_depth), right_bottom_back_normal, new Vector2(1, 1)));
                    }

                    // Compare and create wall between adjacent tiles 
                    else
                    {
                        // Right
                        if (platform[x, 0] > platform[x + 1, 0])
                        {
                            lower = Levels[platform[x + 1, 0]];
                            upper = Levels[platform[x, 0]];
                            

                            platform_vetrices.Add(new VertexPositionNormalTexture(new Vector3(tile_width * (x + 1), lower, z_position), right_bottom_front_normal, new Vector2(0, 0)));
                            platform_vetrices.Add(new VertexPositionNormalTexture(new Vector3(tile_width * (x + 1), upper, z_position), right_top_front_normal, new Vector2(0, 1)));
                            platform_vetrices.Add(new VertexPositionNormalTexture(new Vector3(tile_width * (x + 1), upper, z_position + tile_depth), right_top_back_normal, new Vector2(1, 1)));

                            platform_vetrices.Add(new VertexPositionNormalTexture(new Vector3(tile_width * (x + 1), lower, z_position), right_bottom_front_normal, new Vector2(0, 0)));
                            platform_vetrices.Add(new VertexPositionNormalTexture(new Vector3(tile_width * (x + 1), upper, z_position + tile_depth), right_top_back_normal, new Vector2(0, 1)));
                            platform_vetrices.Add(new VertexPositionNormalTexture(new Vector3(tile_width * (x + 1), lower, z_position + tile_depth), right_bottom_back_normal, new Vector2(1, 1)));

                        }
                        else if (platform[x, 0] < platform[x + 1, 0])
                        // Left
                        {
                            lower = Levels[platform[x, 0]];
                            upper = Levels[platform[x + 1, 0]];

                            platform_vetrices.Add(new VertexPositionNormalTexture(new Vector3(tile_width * (x + 1), lower, z_position), left_bottom_front_normal, new Vector2(0, 0)));
                            platform_vetrices.Add(new VertexPositionNormalTexture(new Vector3(tile_width * (x + 1), upper, z_position + tile_depth), left_top_back_normal, new Vector2(1, 1)));
                            platform_vetrices.Add(new VertexPositionNormalTexture(new Vector3(tile_width * (x + 1), upper, z_position), left_top_front_normal, new Vector2(0, 1)));
                            

                            platform_vetrices.Add(new VertexPositionNormalTexture(new Vector3(tile_width * (x + 1), lower, z_position), left_bottom_front_normal, new Vector2(0, 0)));
                            platform_vetrices.Add(new VertexPositionNormalTexture(new Vector3(tile_width * (x + 1), lower, z_position + tile_depth), left_bottom_back_normal, new Vector2(0, 1)));
                            platform_vetrices.Add(new VertexPositionNormalTexture(new Vector3(tile_width * (x + 1), upper, z_position + tile_depth), left_top_back_normal, new Vector2(1, 1)));
                        }
                    }

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
                        }
                        

                        platform_vetrices.Add(new VertexPositionNormalTexture(new Vector3(tile_width * x, lower, z_position), left_bottom_front_normal, new Vector2(0, 0)));
                        platform_vetrices.Add(new VertexPositionNormalTexture(new Vector3(tile_width * x, upper, z_position), left_top_front_normal, new Vector2(0, 1)));
                        platform_vetrices.Add(new VertexPositionNormalTexture(new Vector3(tile_width * (x + 1), upper, z_position), right_top_front_normal, new Vector2(1, 1)));

                        platform_vetrices.Add(new VertexPositionNormalTexture(new Vector3(tile_width * x, lower, z_position), left_bottom_front_normal, new Vector2(0, 0)));
                        platform_vetrices.Add(new VertexPositionNormalTexture(new Vector3(tile_width * (x + 1), upper, z_position), right_top_front_normal, new Vector2(0, 1)));
                        platform_vetrices.Add(new VertexPositionNormalTexture(new Vector3(tile_width * (x + 1), lower, z_position), right_bottom_front_normal, new Vector2(1, 1)));

                    }
                }
            }            
            return platform_vetrices.ToArray();
        }

        public void Draw(GameTime gameTime)
        {
            // Setup the effect parameters
            effect.Parameters["World"].SetValue(World);
            effect.Parameters["Projection"].SetValue(game.camera.Projection);
            effect.Parameters["View"].SetValue(game.camera.View);
            effect.Parameters["cameraPos"].SetValue(game.camera.Position);
            effect.Parameters["worldInvTrp"].SetValue(WorldInverseTranspose);
            effect.Parameters["ModelTexture"].SetResource(texture);

            // Setup the vertices
            game.GraphicsDevice.SetVertexBuffer(vertices);
            game.GraphicsDevice.SetVertexInputLayout(inputLayout);

            // Apply the effect technique and draw the platform
            effect.CurrentTechnique.Passes[0].Apply();            
            game.GraphicsDevice.Draw(PrimitiveType.TriangleList, vertices.ElementCount);
        }

        // Updates the platform that the player is standing on
        public void Update_player_platforms(Vector3 player_position)
        {
            if ((player_position.Z >= z_position_start) && (player_position.Z < z_position_end))
            {
                standing_platform = this;
            }
            if ((player_position.Z + tile_depth >= z_position_start) && (player_position.Z + tile_depth < z_position_end))
            {
                next_platform = this;
            }
        }
        
        public void Update(GameTime gameTime)
        {
            // Change the platform if the camera passed it 
            if (game.camera.Position.Z > z_position_end)
            {
                change_platform();
            }
            min_extra_tiles = (int)(max_extra_tiles - game.difficulty);

            WorldInverseTranspose = Matrix.Transpose(Matrix.Invert(World));

            //Update the parameters
            effect.Parameters["World"].SetValue(World);
            effect.Parameters["Projection"].SetValue(game.camera.Projection);
            effect.Parameters["View"].SetValue(game.camera.View);
            effect.Parameters["cameraPos"].SetValue(game.camera.Position);
            effect.Parameters["worldInvTrp"].SetValue(WorldInverseTranspose);

            
        }
    }
}
