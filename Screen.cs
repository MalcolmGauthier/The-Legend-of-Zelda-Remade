using static SDL2.SDL;
using static The_Legend_of_Zelda.Textures;
using System.Reflection;

namespace The_Legend_of_Zelda
{
    public static unsafe class Screen
    {
        public static IntPtr render;
        public static IntPtr window_surface;
        public static IntPtr screen = IntPtr.Zero;
        public static IntPtr window_surface_palette;

        static SDL_Rect window_display;
        public static SDL_Rect screen_display;

        public static Tile[] tiles = new Tile[ppu.Length];
        public static MetaTile[] meta_tiles = new MetaTile[704];
        public static List<Sprite> sprites = new List<Sprite>();

        public static int x_scroll;
        public static int y_scroll;

        public static void Init()
        {
            render = SDL_CreateRenderer(Program.window, -1, SDL_RendererFlags.SDL_RENDERER_SOFTWARE);
            SDL_SetRenderDrawBlendMode(render, SDL_BlendMode.SDL_BLENDMODE_BLEND);
            window_surface = SDL_CreateRGBSurface(0, 256, 240, 8, 0, 0, 0, 0);
            window_display = new SDL_Rect() { x = 0, y = 0, w = 256, h = 240 };
            screen_display = new SDL_Rect() { x = 0, y = 0, w = 256, h = 240 };
            window_surface_palette = ((SDL_PixelFormat*)((SDL_Surface*)window_surface)->format)->palette;
            for (int i = 0; i < tiles.Length; i++)
            {
                tiles[i] = new Tile(i);
            }
            for (short i = 0; i < meta_tiles.Length; i++)
            {
                meta_tiles[i] = new MetaTile(i);
            }
        }

        public static void Render()
        {
            Palettes.CheckForBGColorChange();

            for (int i = 0; i < tiles.Length; i++)
            {
                tiles[i].Render();
                //if (scrolling)
                //    scroll_tiles[i].Render();
            }
            for (int i = sprites.Count - 1; i >= 0; i--)
            {
                sprites[i].Render();
            }

            //Scroll();

            byte HUD_start;
            if (Program.gamemode == Program.Gamemode.OVERWORLD || Program.gamemode == Program.Gamemode.DUNGEON)
            {
                if (Menu.menu_open)
                    HUD_start = 0;
                else
                    HUD_start = 64;
            }
            else
            {
                HUD_start = 0;
            }

            // POINTERS? IN MMMMYYYYYYY MEMORY-SAFE LANGUAGE? UNACCEPTABLE!!!!!!!!
            byte* ref_pixel = (byte*)((SDL_Surface*)window_surface)->pixels;

            if (HUD_start == 64)
            {
                for (int i = 0; i < 64; i++)
                {
                    for (int j = 0; j < 256; j++)
                    {
                        ref_pixel[(i << 8) + j] = vram[(i << 9) + j];
                    }
                }
            }

            for (int i = HUD_start; i < 240; i++)
            {
                for (int j = 0; j < 256; j++)
                {
                    ref_pixel[(i << 8) + j] = vram[(TrueMod(i + y_scroll, 480) << 9) + ((j + x_scroll) & 511)];
                }
            }

            #region commented out
            //if (scrolling)
            //{
            //    screen = SDL_CreateTextureFromSurface(render, scroll_surface);
            //    scroll_display = new SDL_Rect() { w = 256, h = 240 };
            //    if (scroll_direction == Direction.VERTICAL)
            //    {
            //        scroll_display.x = 0;
            //        scroll_display.y = screen_display.y + 240;
            //    }
            //    else
            //    {
            //        scroll_display.x = screen_display.x + 256;
            //        scroll_display.y = 0;
            //    }
            //    SDL_RenderCopy(render, screen, ref window_display, ref scroll_display);
            //    SDL_DestroyTexture(screen);
            //}
            //else
            //{
            //    screen_display.y = 0;
            //}
            #endregion

            screen = SDL_CreateTextureFromSurface(render, window_surface);
            SDL_RenderCopy(render, screen, ref window_display, ref screen_display);
            SDL_DestroyTexture(screen);
        }

        static int TrueMod(int x, int m)
        {
            return (x % m + m) % m;
        }

        public static byte GetTileIndexAtLocation(int x, int y)
        { // gives tile id of meta tile at location, returns 0 if oob
            int metatile_index = (y & 0xFFF0) + (x >> 4) - 64;
            if (metatile_index < 0 || metatile_index > meta_tiles.Length)
                return 0;
            return meta_tiles[metatile_index].tile_index;
        }

        public static void ScreenTest()
        {
            if (Program.gTimer % 2 == 0)
                Palettes.LoadPaletteGroup(0, Palettes.PaletteGroups.FOREST);
            else
                Palettes.LoadPaletteGroup(0, Palettes.PaletteGroups.BLACK);

            LoadPPUPage(PPUDataGroup.OTHER, 0, 0);
            LoadPPUPage(PPUDataGroup.OTHER, 2, 2);
            x_scroll++;
        }
    }
}