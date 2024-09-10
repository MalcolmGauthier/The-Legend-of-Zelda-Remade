using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static SDL2.SDL;

namespace The_Legend_of_Zelda
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
        public void Render()
        {
            ChangeTexture();

            if (!shown)
                return;

            byte pixel_color, x_count = 0, y_count = 0;
            sbyte i, j, i_inc = 1, j_inc = 1, i_start = 0, j_start = -1;
            bool link_mask_flag = false;
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
                    pixel_color = pixels[(i << 3) + j];
                    if (pixel_color == 0) // check for transparency
                        continue;
                    int location = TrueMod(((y + (y_count - 1)) << 9) + (x_count - 1) + x, 245760);
                    if (link_mask_flag)
                        if ((y + (y_count - 1)) <= 80 || (y + (y_count - 1)) >= 224 || (x_count - 1) + x <= 16 || (x_count - 1) + x >= 240)
                            continue;
                    if (background)
                        if ((Textures.vram[location] & 3) != 0)
                            continue;
                    Textures.vram[location] = (byte)(pixel_color + (palette_index << 2));
                }
                x_count = 0;
                i += i_inc;
            }

            #region old way
            //if (xflip)
            //{
            //    if (yflip)
            //    {
            //        for (int i = 0; i < 16; i += 1)
            //        {
            //            for (int j = 0; j < 8; j += 1)
            //            {
            //                pixel_color = pixels[i * 8 + j];
            //                if (pixel_color == 0) // check for transparency
            //                    continue;
            //                int location = TrueMod(((y + (15 - i)) << 9) + (7 - j) + x, 245760);
            //                if (background)
            //                    if ((Textures.nametable[location] & 3) != 0)
            //                        continue;
            //                Textures.nametable[location] = (byte)(pixel_color + (palette_index << 2));
            //            }
            //        }
            //    }
            //    else
            //    {
            //        for (int i = 0; i < 16; i += 1)
            //        {
            //            for (int j = 0; j < 8; j += 1)
            //            {
            //                pixel_color = pixels[i * 8 + j];
            //                if (pixel_color == 0) // check for transparency
            //                    continue;
            //                int location = TrueMod(((y + i) << 9) + (7 - j) + x, 245760);
            //                if (background)
            //                    if ((Textures.nametable[location] & 3) != 0)
            //                        continue;
            //                Textures.nametable[location] = (byte)(pixel_color + (palette_index << 2));
            //            }
            //        }
            //    }
            //}
            //else
            //{
            //    if (yflip)
            //    {
            //        for (int i = 0; i < 16; i += 1)
            //        {
            //            for (int j = 0; j < 8; j += 1)
            //            {
            //                pixel_color = pixels[i * 8 + j];
            //                if (pixel_color == 0) // check for transparency
            //                    continue;
            //                int location = TrueMod(((y + (15 - i)) << 9) + j + x, 245760);
            //                if (background)
            //                    if ((Textures.nametable[location] & 3) != 0)
            //                        continue;
            //                Textures.nametable[location] = (byte)(pixel_color + (palette_index << 2));
            //            }
            //        }
            //    }
            //    else
            //    {
            //        for (int i = 0; i < 16; i += 1)
            //        {
            //            for (int j = 0; j < 8; j += 1)
            //            {
            //                pixel_color = pixels[i * 8 + j];
            //                if (pixel_color == 0) // check for transparency
            //                    continue;
            //                int location = TrueMod(((y + i) << 9) + j + x, 245760);
            //                if (background)
            //                    if ((Textures.nametable[location] & 3) != 0)
            //                        continue;
            //                Textures.nametable[location] = (byte)(pixel_color + (palette_index << 2));
            //            }
            //        }
            //    }
            //}
            #endregion
        }
        public void ChangeTexture()
        {
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