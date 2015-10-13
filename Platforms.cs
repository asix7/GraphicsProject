using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpDX;
using SharpDX.Toolkit;

namespace Project
{
    using SharpDX.Toolkit.Graphics;
    public class Platforms : GameObject
    {
        public Effect effect;
        public Matrix World = Matrix.Identity;
        public Matrix WorldInverseTranspose;
        public VertexInputLayout inputLayout;
        public Buffer<VertexPositionNormalTexture> vertices;
        public string textureName;
        public Texture2D texture;

        Random rnd = new Random();
        public float[] Levels = new float[] { 0, 10.0f, 20.0f };
        static public int[,] current_platform = new int[,] { { -1, 0 }, { -1, 0 }, { 0, 0 }, { -1, 0 }, { -1, 0 } };
        static public Platforms standing_platform;

        public int[,] platform;
        // Z coordinate to start creating the each new platform
        static float z_position = 0;
        // Z coordinate that the platform instance ends
        public float z_position_start;
        public float z_position_end;

        public float tile_width = 20.0f;
        public float tile_depth = 100.0f;

        float base_offset = 2.0f;

        public Platforms(LabGame game) 
        {
            if (standing_platform == null)
            {
                standing_platform = this;
            }

            textureName = "Water.jpg";
            texture = game.Content.Load<Texture2D>(textureName);

            int[,] previous_platform = current_platform;
            platform = create_platform(previous_platform);


            VertexPositionNormalTexture[] platform_vertices = create_vertices(platform, previous_platform);
            current_platform = platform;

            z_position_start = z_position;
            z_position += tile_depth;
            z_position_end = z_position; 

            vertices = Buffer.Vertex.New(game.GraphicsDevice, platform_vertices);
            effect = game.Content.Load<Effect>("Water");
            inputLayout = VertexInputLayout.FromBuffer(0, vertices);
            this.game = game;
        }

        public void change_platform()
        {
            int[,] previous_platform = current_platform;
            platform = create_platform(previous_platform);

            VertexPositionNormalTexture[] platform_vertices = create_vertices(platform, previous_platform);
            current_platform = platform;

            z_position_start = z_position;
            z_position += tile_depth;
            z_position_end = z_position; 

            vertices = Buffer.Vertex.New(game.GraphicsDevice, platform_vertices);
        }

        public int[,] create_platform(int[,] platform)
        {
            int[,] new_platform = new int[,] { { -1, 0 }, { -1, 0 }, { -1, 0 }, { -1, 0 }, { -1, 0 } };
            List<int> current_tiles = new List<int>();
            int conserve;
            int extra_tiles;
            int new_tile;

            for (int i = 0; i <= platform.GetLength(0) - 1; i++)
            {
                if (platform[i, 0] >= 0)
                {
                    current_tiles.Add(i);
                }
            }

            // Make surre that at least one tile will continue
            conserve = current_tiles[rnd.Next(0, current_tiles.Count - 1)];
            new_platform[conserve, 0] = rnd.Next(0, 3);
            new_platform[conserve, 1] = rnd.Next(0, 3);

            //Generate at least one more tiles, 2 in total
            extra_tiles = rnd.Next(0, 3) + 2;

            for (int i = 0; i < extra_tiles; i++)
            {
                new_tile = rnd.Next(0, 5);
                // If the tile is already in the dictionary continue
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

        public override void Draw(GameTime gameTime)
        {
            // Setup the effect parameters
            effect.Parameters["World"].SetValue(World);
            effect.Parameters["Projection"].SetValue(game.camera.Projection);
            effect.Parameters["View"].SetValue(game.camera.View);
            effect.Parameters["cameraPos"].SetValue(game.camera.cameraPos);
            effect.Parameters["worldInvTrp"].SetValue(WorldInverseTranspose);
            effect.Parameters["ModelTexture"].SetResource(texture);

            // Setup the vertices
            game.GraphicsDevice.SetVertexBuffer(vertices);
            game.GraphicsDevice.SetVertexInputLayout(inputLayout);

            // Apply the basic effect technique and draw the rotating cube
            effect.CurrentTechnique.Passes[0].Apply();            
            game.GraphicsDevice.Draw(PrimitiveType.TriangleList, vertices.ElementCount);
        }

        public void Standing_Platform(Vector3 player_position)
        {
            if ((player_position.Z >= z_position_start) && (player_position.Z < z_position_end))
            {
                standing_platform = this;
            }
        }
        
        public override void Update(GameTime gameTime)
        {
            if (game.camera.cameraPos.Z > z_position_end)
            {
                change_platform();
            }

            effect.Parameters["World"].SetValue(World);
            effect.Parameters["Projection"].SetValue(game.camera.Projection);
            effect.Parameters["View"].SetValue(game.camera.View);
            effect.Parameters["cameraPos"].SetValue(game.camera.cameraPos);
            effect.Parameters["worldInvTrp"].SetValue(WorldInverseTranspose);
            effect.Parameters["ModelTexture"].SetResource(texture);

            var time = (float)gameTime.TotalGameTime.TotalSeconds;
            WorldInverseTranspose = Matrix.Transpose(Matrix.Invert(World));
        }
    }
}
