using System.Diagnostics;
using System.Runtime.InteropServices;
using The_Legend_of_Zelda;
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
            int offset = id < 1920 ? 0 : 256;

            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    vram[((i + ((vram_id >> 5) << 3)) << 9) + j + ((vram_id & 31) << 3) + offset] = (byte)(texture_pixels[(i << 3) + j] + (ppu_plt[id] << 2));
                }
            }
        }
    }
}