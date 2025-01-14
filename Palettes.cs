using static SDL2.SDL;
namespace The_Legend_of_Zelda
{
    public enum Color : byte
    {
        _00_DARK_GRAY,
        _01_DARK_BLUE,
        _02_BLUE,
        _03_DARK_INDIGO,
        _04_PURPLE,
        _05_MAGENTA,
        _06_RED,
        _07_BROWN,
        _08_DARK_YELLOW,
        _09_DARK_GREEN,
        _0A_GREEN,
        _0B_FOREST_GREEN,
        _0C_AQUA,
        _0D_NULL_DO_NOT_USE,
        _0E_BLACK,
        _0F_BLACK,

        _10_GRAY,
        _11_SEMI_LIGHT_BLUE,
        _12_SMEI_DARK_BLUE,
        _13_INDIGO,
        _14_SEMI_DARK_PURPLE,
        _15_ROSE,
        _16_RED_ORANGE,
        _17_DARK_GOLD,
        _18_OLIVE,
        _19_SEMI_DARK_GREEN,
        _1A_SEMI_LIGHT_GREEN,
        _1B_EVERGREEN,
        _1C_DARK_CYAN,
        _1D_BLACK,
        _1E_BLACK,
        _1F_BLACK,

        _20_WHITE,
        _21_LIGHT_BLUE,
        _22_LIGHT_INDIGO,
        _23_LIGHT_PURPLE,
        _24_PINK,
        _25_LIGHT_RED,
        _26_LIGHT_ORANGE,
        _27_GOLD,
        _28_LIGHT_OLIVE,
        _29_LIGHT_GREEN,
        _2A_LIME,
        _2B_TURQUOISE,
        _2C_CYAN,
        _2D_DARKER_GRAY,
        _2E_BLACK,
        _2F_BLACK,

        _30_WHITE,
        _31_LIGHTER_BLUE,
        _32_LIGHTER_INDIGO,
        _33_LIGHTER_PURPLE,
        _34_LIGHTER_PINK,
        _35_LIGHTER_RED,
        _36_LIGHTER_ORANGE,
        _37_BEIGE,
        _38_LIGHT_YELLOW,
        _39_LIGHTER_GREEN,
        _3A_LIGHT_LIME,
        _3B_LIGHT_TURQUOISE,
        _3C_LIGHT_CYAN,
        _3D_LIGHT_GRAY,
        _3E_BLACK,
        _3F_BLACK
    }

    public enum PaletteID
    {
        BG_0,
        BG_1,
        BG_2,
        BG_3,
        SP_0,
        SP_1,
        SP_2,
        SP_3
    }

    public static class Palettes
    {
        const int COLORS_IN_PALETTE = 4;
        const int NUM_PALETTES = 8;
        const int NUM_COLORS = 0x40;

        public enum PaletteGroups
        {
            BLACK,
            FOREST,
            MOUNTAIN,
            GRAVEYARD_HUD1,
            HUD2,
            TITLESCREEN_1,
            TITLESCREEN_2,
            TITLESCREEN_3,
            TITLESCREEN_4,
            GREEN_LINK_HUDSPR1,
            HUDSPR_2,
            HUDSPR_3,
            OVERWORLD_CAVE,
            OVERWORLD_DARK_ENEMIES,
            DUNGEON1,
            DUNGEON2,
            DUNGEON3,
            DUNGEON4_6,
            DUNGEON5_7,
            DUNGEON8_9,
        }

        public static byte[] active_palette_list = new byte[COLORS_IN_PALETTE * NUM_PALETTES];

        public static Color background_color = Color._1D_BLACK;
        static Color old_bg_color = Color._1D_BLACK;

        static byte[,] palette_list = new byte[20, 3]
        {
            {0x0f,0x0f,0x0f}, // black
            {0x1a,0x37,0x12}, // forest
            {0x17,0x37,0x12}, // mountain
            {0x30,0x00,0x12}, // graveyard & hud 1
            {0x16,0x27,0x36}, // hud 2
            {0x0f,0x00,0x10}, // titlescreen rocks
            {0x17,0x37,0x0f}, // titlescreen title
            {0x08,0x1a,0x28}, // titlescreen leaves
            {0x30,0x3b,0x22}, // titlescreen sword & water
            {0x29,0x27,0x17}, // green link & hud link sprites
            {0x02,0x22,0x30}, // hud blue sprites
            {0x16,0x27,0x30}, // hud red sprites
            {0x07,0x0f,0x17}, // overowlrd cave
            {0x0f,0x1c,0x16}, // overworld dark enemies
            {0x0c,0x1c,0x2c}, // dungeon 1
            {0x02,0x12,0x22}, // dungeon 2
            {0x0b,0x1b,0x2b}, // dungeon 3
            {0x08,0x18,0x28}, // dungeon 4 & 6
            {0x0a,0x1a,0x2a}, // dungeon 5 & 7
            {0x00,0x10,0x20}, // dungeon 8 & 9
        };

        // full NES color list - https://imgur.com/a/sGzojmV
        public static SDL_Color[] color_list = new SDL_Color[NUM_COLORS]
        {
            new SDL_Color(){r=84, g=84, b=84, a=255},
            new SDL_Color(){r=0, g=30, b=116, a=255},
            new SDL_Color(){r=8, g=16, b=144, a=255},
            new SDL_Color(){r=48, g=0, b=136, a=255},
            new SDL_Color(){r=68, g=0, b=100, a=255},
            new SDL_Color(){r=92, g=0, b=48, a=255},
            new SDL_Color(){r=84, g=4, b=0, a=255},
            new SDL_Color(){r=60, g=24, b=0, a=255},
            new SDL_Color(){r=32, g=42, b=0, a=255},
            new SDL_Color(){r=8, g=58, b=0, a=255},
            new SDL_Color(){r=0, g=64, b=0, a=255},
            new SDL_Color(){r=0, g=60, b=0, a=255},
            new SDL_Color(){r=0, g=50, b=60, a=255},
            new SDL_Color(){r=0, g=0, b=0, a=0},
            new SDL_Color(){r=0, g=0, b=0, a=255},
            new SDL_Color(){r=0, g=0, b=0, a=255},

            new SDL_Color(){r=152, g=150, b=152, a=255},
            new SDL_Color(){r=8, g=76, b=196, a=255},
            new SDL_Color(){r=48, g=50, b=236, a=255},
            new SDL_Color(){r=92, g=30, b=228, a=255},
            new SDL_Color(){r=136, g=20, b=176, a=255},
            new SDL_Color(){r=160, g=20, b=100, a=255},
            new SDL_Color(){r=152, g=34, b=32, a=255},
            new SDL_Color(){r=120, g=60, b=0, a=255},
            new SDL_Color(){r=84, g=90, b=0, a=255},
            new SDL_Color(){r=40, g=114, b=0, a=255},
            new SDL_Color(){r=8, g=124, b=0, a=255},
            new SDL_Color(){r=0, g=118, b=40, a=255},
            new SDL_Color(){r=0, g=102, b=120, a=255},
            new SDL_Color(){r=0, g=0, b=0, a=255},
            new SDL_Color(){r=0, g=0, b=0, a=255},
            new SDL_Color(){r=0, g=0, b=0, a=255},

            new SDL_Color(){r=236, g=238, b=236, a=255},
            new SDL_Color(){r=76, g=154, b=236, a=255},
            new SDL_Color(){r=120, g=124, b=236, a=255},
            new SDL_Color(){r=176, g=98, b=236, a=255},
            new SDL_Color(){r=228, g=84, b=236, a=255},
            new SDL_Color(){r=236, g=88, b=180, a=255},
            new SDL_Color(){r=236, g=106, b=100, a=255},
            new SDL_Color(){r=212, g=136, b=32, a=255},
            new SDL_Color(){r=160, g=170, b=0, a=255},
            new SDL_Color(){r=116, g=196, b=0, a=255},
            new SDL_Color(){r=76, g=208, b=32, a=255},
            new SDL_Color(){r=56, g=204, b=108, a=255},
            new SDL_Color(){r=56, g=180, b=204, a=255},
            new SDL_Color(){r=60, g=60, b=60, a=255},
            new SDL_Color(){r=0, g=0, b=0, a=255},
            new SDL_Color(){r=0, g=0, b=0, a=255},

            new SDL_Color(){r=236, g=238, b=236, a=255},
            new SDL_Color(){r=168, g=204, b=236, a=255},
            new SDL_Color(){r=188, g=188, b=236, a=255},
            new SDL_Color(){r=212, g=178, b=236, a=255},
            new SDL_Color(){r=236, g=174, b=236, a=255},
            new SDL_Color(){r=236, g=174, b=212, a=255},
            new SDL_Color(){r=236, g=180, b=176, a=255},
            new SDL_Color(){r=228, g=196, b=144, a=255},
            new SDL_Color(){r=204, g=210, b=120, a=255},
            new SDL_Color(){r=180, g=222, b=120, a=255},
            new SDL_Color(){r=168, g=226, b=144, a=255},
            new SDL_Color(){r=152, g=226, b=144, a=255},
            new SDL_Color(){r=160, g=214, b=228, a=255},
            new SDL_Color(){r=160, g=162, b=160, a=255},
            new SDL_Color(){r=0, g=0, b=0, a=255},
            new SDL_Color(){r=0, g=0, b=0, a=255}
        };

        public static bool grayscale_mode = false;

        public static void Init()
        {
            LoadPaletteGroup(PaletteID.BG_0, PaletteGroups.TITLESCREEN_1);
            LoadPaletteGroup(PaletteID.BG_1, PaletteGroups.TITLESCREEN_2);
            LoadPaletteGroup(PaletteID.BG_2, PaletteGroups.TITLESCREEN_3);
            LoadPaletteGroup(PaletteID.BG_3, PaletteGroups.TITLESCREEN_4);
            LoadPaletteGroup(PaletteID.SP_0, PaletteGroups.TITLESCREEN_1);
            LoadPaletteGroup(PaletteID.SP_1, PaletteGroups.TITLESCREEN_2);
            LoadPaletteGroup(PaletteID.SP_2, PaletteGroups.TITLESCREEN_3);
            LoadPaletteGroup(PaletteID.SP_3, PaletteGroups.TITLESCREEN_4);
        }
        public static void LoadPaletteGroup(PaletteID active_plt_index, PaletteGroups palette_group)
        {
            active_palette_list[(int)active_plt_index * COLORS_IN_PALETTE] = (byte)background_color;
            for (int i = 1; i < COLORS_IN_PALETTE; i++)
            {
                active_palette_list[(int)active_plt_index * COLORS_IN_PALETTE + i] = palette_list[(byte)palette_group, i - 1];
            }
        }

        public static void LoadPalette(byte active_plt_index, byte active_plt_color, Color color_index)
        {
            active_palette_list[active_plt_index * COLORS_IN_PALETTE + active_plt_color] = (byte)color_index;
        }

        public static SDL_Color[] GetPalette()
        {
            SDL_Color[] return_value = new SDL_Color[COLORS_IN_PALETTE * NUM_PALETTES];

            byte color_mask = 0xFF;
            if (grayscale_mode)
            {
                color_mask = 0xF0;
            }

            for (int i = 0; i < COLORS_IN_PALETTE * NUM_PALETTES; i++)
            {
                return_value[i] = color_list[active_palette_list[i] & color_mask];
            }

            return return_value;
        }

        public static void CheckForBGColorChange()
        {
            if (old_bg_color != background_color)
            {
                old_bg_color = background_color;
                for (int i = 0; i < active_palette_list.Length / COLORS_IN_PALETTE; i++)
                    active_palette_list[i * COLORS_IN_PALETTE] = (byte)background_color;
            }
        }
    }
}