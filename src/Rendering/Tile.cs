using The_Legend_of_Zelda.Gameplay;
using static SDL2.SDL;
using static The_Legend_of_Zelda.Rendering.Textures;

namespace The_Legend_of_Zelda.Rendering
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

        // this retreives the pixel data for the tile's texture and sets it in the right vram location
        // we have to draw every single tile every single frame, because sprites can overwrite vram with their graphics, and those stay there.
        public void ChangeTexture()
        {
            tile_index = ppu[id];

            byte[] texture_pixels = LoadBGTexture(tile_index);

            // in the ppu, each screen in held sequentialy in data, screen 1's data comes only after screen 0, etc.
            // this is not the case for vram. vram places the screens how they appear in game, which means that screen 2 and 3
            // are located to the right of screen 0 and 1 respectively. so in vram, the data is the first row of pixels for
            // screen 0, followed by the first row of pixels for screen 2, then the first row of pixel for screen 0, etc.
            // this alternates until the 240th row of pixels for screen 2, which then comes the exact same thing, but for
            // screens 1 and 3.
            int vram_id = id % (ppu.Length / 2);
            // if location on screen 3 or 4, add 1 screen to x pos
            int offset = id < ppu.Length / 2 ? 0 : Program.NES_OUTPUT_WIDTH;

            int tile_x_pos = vram_id % 32 * 8 + offset;
            int tile_y_pos = vram_id / 32 * 8;

            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    // & operation on id ensures that palette acts just like nes, and affects 2x2 regions of tiles. in this case, the top left tile's palette is taken
                    vram[(i + tile_y_pos) * VRAM_WIDTH + tile_x_pos + j] = (byte)(texture_pixels[i * 8 + j] + ppu_plt[id & (~0b100001)] * 4);
                }
            }
        }
    }
}