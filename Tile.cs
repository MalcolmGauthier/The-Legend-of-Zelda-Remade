using static SDL2.SDL;
using static The_Legend_of_Zelda.Textures;
namespace The_Legend_of_Zelda
{
    public unsafe class Tile
    {
        public int id;
        public byte tile_index = 0;

        public Tile(int id)
        {
            this.id = id;

            LoadTexture();
        }

        public void Render()
        {
            ChangeTexture();
            ChangePalette();
        }

        public void LoadTexture()
        {
            #region test code
            //texture_pixels = Textures.LoadBGTexture(0x71);
            //if (Math.Floor(id / 32.0) % 2 == 1)
            //    texture_pixels = Textures.LoadBGTexture(0x77);
            //if (id % 2 == 1)
            //    texture_pixels = Textures.LoadBGTexture(0x72);
            //if ((Math.Floor(id / 32.0) % 2 == 1) && (id % 2 == 1))
            //    texture_pixels = Textures.LoadBGTexture(0x78);
            #endregion

            Render();
        }

        public void ChangePalette()
        {
            SDL_SetPaletteColors(Screen.window_surface_palette, Palettes.GetPalette(), 0, 32);
        }

        public void ChangeTexture()
        {
            tile_index = ppu[id];

            byte[] texture_pixels = LoadBGTexture(tile_index);

            int vram_id = id % 1920;
            // if location on screen 3 or 4, add 1 screen to x pos
            int offset = id < 1920 ? 0 : 256;

            int tile_x_pos = (vram_id % 32) * 8;
            int tile_y_pos = (vram_id / 32) * 8;

            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    vram
                    [
                        ((i + tile_y_pos) * 512) +
                        tile_x_pos + j + offset

                    ] = (byte)(texture_pixels[(i * 8) + j] + (ppu_plt[id] * 4));
                }
            }
        }
    }
}