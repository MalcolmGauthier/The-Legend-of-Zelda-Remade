using The_Legend_of_Zelda.Gameplay;
using The_Legend_of_Zelda.Sprites;
using static SDL2.SDL;
using static The_Legend_of_Zelda.Rendering.Textures;

namespace The_Legend_of_Zelda.Rendering
{
    public static class Screen
    {
        public static IntPtr render;
        public static IntPtr window_surface;
        public static IntPtr screen = IntPtr.Zero;
        public static IntPtr window_surface_palette;

        static SDL_Rect window_display;

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
            unsafe { window_surface_palette = ((SDL_PixelFormat*)((SDL_Surface*)window_surface)->format)->palette; }
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

            //TODO: you don't need to render everything.
            for (int i = 0; i < tiles.Length; i++)
            {
                tiles[i].Render();
            }
            // oldest sprites rendered last, meaning that they are on top and have priority.
            for (int i = sprites.Count - 1; i >= 0; i--)
            {
                sprites[i].Render();
            }

            byte HUD_start = 0;
            if ((Program.gamemode == Program.Gamemode.OVERWORLD || Program.gamemode == Program.Gamemode.DUNGEON) && !Menu.menu_open)
            {
                HUD_start = 64;
            }

            // Pointers? In my memory-safe language? It's more likely than you think!
            unsafe
            {
                // usually you'd use SDL's surface combining functions to add data onto eachother, but having a byte array
                // as vram is faster, and SDL's functions check for alot of stuff i don't care about, so using pointers was how
                // i was first able to achieve a stable 60.1fps
                byte* ref_pixel = (byte*)((SDL_Surface*)window_surface)->pixels;

                for (int i = 0; i < HUD_start; i++)
                {
                    for (int j = 0; j < Program.NES_OUTPUT_WIDTH; j++)
                    {
                        ref_pixel[i * Program.NES_OUTPUT_WIDTH + j] = vram[i * VRAM_WIDTH + j];
                    }
                }

                for (int i = HUD_start; i < Program.NES_OUTPUT_HEIGHT; i++)
                {
                    for (int j = 0; j < Program.NES_OUTPUT_WIDTH; j++)
                    {
                        ref_pixel[i * Program.NES_OUTPUT_WIDTH + j] =
                            vram[TrueMod(i + y_scroll, VRAM_HEIGHT) * VRAM_WIDTH + TrueMod(j + x_scroll, VRAM_WIDTH)];
                    }
                }
            }

            screen = SDL_CreateTextureFromSurface(render, window_surface);
            SDL_RenderCopy(render, screen, ref window_display, IntPtr.Zero);
            SDL_DestroyTexture(screen);
        }

        // fuck C# for keeping this bullshit from c/c++
        public static int TrueMod(int x, int m)
        {
            return (x % m + m) % m;
        }


        public static int GetMetaTileIndexAtLocation(int x, int y)
        {
            int index = (y & ~0xF) + x / 16 - 64;

            if (index < 0 || index > Screen.meta_tiles.Length)
                return -1;

            return index;
        }
        // gives tile type of meta tile at location, returns 0 if oob
        public static MetatileType GetMetaTileTypeAtLocation(int x, int y)
        {
            int metatile_index = (y & ~0xF) + (x >> 4) - 64;
            if (metatile_index < 0 || metatile_index > meta_tiles.Length)
                return 0;
            return meta_tiles[metatile_index].tile_index;
        }

        // debug
        public static void ScreenTest()
        {
            if (Program.gTimer % 2 == 0)
                Palettes.LoadPaletteGroup(0, Palettes.PaletteGroups.FOREST);
            else
                Palettes.LoadPaletteGroup(0, Palettes.PaletteGroups.BLACK);

            y_scroll = 8;
            Palettes.background_color = Color._00_DARK_GRAY;
            LoadPPUPage(PPUDataGroup.OTHER, OtherPPUPages.TITLE, 0);
            LoadPPUPage(PPUDataGroup.OTHER, OtherPPUPages.TITLE, 2);
            x_scroll++;
        }
    }
}