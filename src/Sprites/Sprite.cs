using The_Legend_of_Zelda.Gameplay;
using The_Legend_of_Zelda.Rendering;
using static The_Legend_of_Zelda.Gameplay.Program;
using static SDL2.SDL;

namespace The_Legend_of_Zelda.Sprites
{
    public abstract class Sprite
    {
        public int x = 0;
        public int y = 0;
        public byte tile_index;
        private byte current_tile_index;
        public byte palette_index;
        public byte[] pixels;
        public bool xflip, yflip;
        public bool shown = true;
        public bool background = false;
        public bool unload_during_transition = false;
        public bool use_chr_rom = false;

        public Sprite(byte tile_index, byte palette_index)
        {
            this.tile_index = tile_index;
            current_tile_index = tile_index;
            pixels = Textures.LoadSPRTexture(tile_index, use_chr_rom);
            this.palette_index = palette_index;
        }

        // freaky code. however, it's small and works.
        public void Render()
        {
            UpdateTexture();

            if (!shown)
                return;

            // sprites cannot use background palettes.
            if (palette_index < (byte)PaletteID.SP_0)
                palette_index += 4;

            byte pixel_color, x_count = 0, y_count = 0;
            int i, j, i_inc = 1, j_inc = 1, i_start = 0, j_start = -1;
            bool link_mask_flag = false;

            // order of pixel readings changes when flipped
            if (xflip)
            {
                j_start = 8;
                j_inc = -1;
            }
            if (yflip)
            {
                i_start = 15;
                i_inc = -1;
            }

            // mask for borders of dungeon
            if ((this == Link.self || this == Link.counterpart) && Program.gamemode == Program.Gamemode.DUNGEON)
            {
                link_mask_flag = true;
            }

            i = i_start;
            while (y_count++ < 16)
            {
                j = j_start;
                while (x_count++ < 8)
                {
                    j += j_inc;

                    pixel_color = pixels[i * 8 + j];
                    if (pixel_color == 0) // check for transparency
                        continue;

                    int location = TrueMod((y + (y_count - 1)) * Textures.VRAM_WIDTH + (x_count - 1) + x, Textures.vram.Length);

                    if (link_mask_flag)
                        if (y + (y_count - 1) <= 80 || y + (y_count - 1) >= 224 || x_count - 1 + x <= 16 || x_count - 1 + x >= 240)
                            continue;

                    // if on background, only draw pixel if the pixel is using palette index 0 (background)
                    if (background)
                        if ((Textures.vram[location] & 3) != 0)
                            continue;

                    Textures.vram[location] = (byte)(pixel_color + palette_index * 4);
                }
                x_count = 0;
                i += i_inc;
            }
        }

        public void UpdateTexture()
        {
            // prevent calling slow expensive function if the sprite is already fine
            if (tile_index == current_tile_index && !use_chr_rom)
                return;

            pixels = Textures.LoadSPRTexture(tile_index, use_chr_rom);
            current_tile_index = tile_index;
        }

        public abstract void Action();

        int TrueMod(int x, int m)
        {
            return (x % m + m) % m;
        }
    }
}