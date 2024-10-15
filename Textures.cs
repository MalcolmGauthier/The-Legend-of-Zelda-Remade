using static SDL2.SDL;
namespace The_Legend_of_Zelda
{
    public enum Text : byte
    {
        _0, _1, _2, _3, _4, _5, _6, _7, _8, _9,
        A, B, C, D, E, F, G, H, I, J, K, L, M, N, O, P, Q, R, S, T, U, V, W, X, Y, Z,
        COMMA = 0x28,
        EXCLAM,
        APOSTR,
        ANDPERC,
        DOT,
        QUOTE,
        QUESTN,
        DASH,
    }

    public enum SpriteID : byte
    {
        LINK_SIDE_L1 = 0,
        LINK_SIDE_R1 = 2,
        LINK_SIDE_L2 = 4,
        LINK_SIDE_R2 = 6,
        LINK_DOWN_L = 8,
        LINK_DOWN_R = 10,
        LINK_UP_L = 12,
        LINK_UP_R = 14,
        LINK_SIDE_ATK_L = 16,
        LINK_SIDE_ATK_R = 18,
        LINK_DOWN_ATK_L = 20,
        LINK_DOWN_ATK_R = 22,
        LINK_UP_ATK_L = 24,
        LINK_UP_ATK_R = 26,
        BLANK = 28,
        SELECTOR = 30,
        SWORD = 32,
        BAIT = 34,
        RECORDER = 36,
        CANDLE = 38,
        ARROW = 40,
        BOW = 42,
        MAGICAL_KEY = 44,
        KEY = 46,
        EXPLOSION_PARTICLE = 48,
        RUPEE = 50,
        BOMB = 52,
        BOOMERANG = 54,
        BOOMERANG_45 = 56,
        BOOMERANG_90 = 58,
        SPARK = 60,
        DOT = 62,
        POTION = 64,
        BOOK_OF_MAGIC = 66,
        FIREBALL = 68,
        RING = 70,
        MAGIC_SWORD = 72,
        ROD = 74,
        MAP = 76,
        POWER_BRACELET = 78,
        FAIRY_1 = 80,
        FAIRY_2 = 82,
        LINK_SIDE_R2_SHIELD = 84,
        MAGICAL_SHIELD = 86,
        LINK_DOWN_SHIELD_1 = 88,
        LINK_DOWN_SHIELD_2 = 90,
        FIRE_L = 92,
        FIRE_R = 94,
        LINK_DOWN_MAG_SHIELD = 96,
        BIG_SPARK_1 = 98,
        BIG_SPARK_2 = 100,
        CLOCK = 102,
        HEART_CONTAINER = 104,
        COMPASS = 106,
        RAFT = 108,
        TRIFORCE = 110,
        SMOKE_1 = 112,
        SMOKE_2 = 114,
        SMOKE_3 = 116,
        LADDER = 118,
        LINK_ITEM_GET = 120,
        MAGIC_BEAM_UP = 122,
        MAGIC_BEAM_SIDE_1 = 124,
        MAGIC_BEAM_SIDE_2 = 126,
        LINK_SIDE_R1_SHIELD = 128,
        SWORD_SIDE_1 = 130,
        SWORD_SIDE_2 = 132,
        ARROW_SIDE_1 = 134,
        ARROW_SIDE_2 = 136,
        ROD_SIDE_1 = 138,
        ROD_SIDE_2 = 140,
    }

    public static class Textures
    {
        public const int PPU_WIDTH = 32;
        public const int PPU_HEIGHT = 30;
        public const int PPU_SCREENS = 4;
        public const int SCREEN_TILES = PPU_WIDTH * PPU_HEIGHT;
        public const int PIXELS_PER_TILE = 64;
        public const int BYTES_PER_CHR_TILE = 16;
        public const int CHR_TILESET_SIZE = 256;

        public static byte[] chr_bg = new byte[CHR_TILESET_SIZE * BYTES_PER_CHR_TILE]; // 4096
        public static byte[] chr_sp = new byte[CHR_TILESET_SIZE * BYTES_PER_CHR_TILE]; // 4096
        public static byte[] vram = new byte[SCREEN_TILES * PPU_SCREENS * PIXELS_PER_TILE]; // 245000
        public static byte[] ppu = new byte[SCREEN_TILES * PPU_SCREENS]; // 3840
        public static byte[] ppu_plt = new byte[SCREEN_TILES * PPU_SCREENS]; // 3840

        public enum PPUDataGroup
        {
            OVERWORLD,
            DUNGEON,
            OTHER
        }

        public enum ROMData
        {
            CHR_SURFACE, // BG tiles + enemies for overworld
            CHR_DUNGEON, // BG tiles + common enemies for dungeons
            SPR_DUNGEON_127,
            SPR_DUNGEON_358,
            SPR_DUNGEON_469,
            SPR_DUNGEON_BOSS_1257,
            SPR_DUNGEON_BOSS_3468,
            SPR_DUNGEON_BOSS_9
        }

        public static void Init()
        {
            using (Stream stream = File.OpenRead(@"Data\CHR_FULL.bin"))
            {
                BinaryReader reader = new(stream);
                chr_bg = reader.ReadBytes(chr_bg.Length);
            }
            using (Stream stream = File.OpenRead(@"Data\SPR_FULL.bin"))
            {
                BinaryReader reader = new(stream);
                chr_sp = reader.ReadBytes(chr_sp.Length);
            }

            using (Stream stream = File.OpenRead(@"Data\TILES_OTHER.bin"))
            {
                BinaryReader reader = new(stream);
                ppu = reader.ReadBytes(ppu.Length);
            }
            using (Stream stream = File.OpenRead(@"Data\PLT_OTHER.bin"))
            {
                BinaryReader reader = new(stream);
                byte a;
                for (int i = 0; i < ppu_plt.Length / 4; i++)
                {
                    a = reader.ReadByte();
                    ppu_plt[i * 4    ] = (byte)((a & 0b11000000) >> 6);
                    ppu_plt[i * 4 + 1] = (byte)((a & 0b00110000) >> 4);
                    ppu_plt[i * 4 + 2] = (byte)((a & 0b00001100) >> 2);
                    ppu_plt[i * 4 + 3] = (byte) (a & 0b00000011);
                }
            }
        }

        public static byte[] LoadBGTexture(byte index)
        {
            byte[] texture = new byte[PIXELS_PER_TILE];
            int real_index = index * BYTES_PER_CHR_TILE;

            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    texture[i * 8 + (7 - j)] = (byte)(
                        ((chr_bg[real_index +     i] & (1 << j)) >> j) | // gets j-th bit of byte real_index+i
                        ((chr_bg[real_index + 8 + i] & (1 << j)) >> j) * 2 // gets j-th bit of byte real_index+i+8, and multiplies by 2
                    ); // stores resulting byte in texture, top to bottom, right to left
                }
            }

            return texture;
        }

        public static byte[] LoadSPRTexture(byte index, bool use_chr_rom = false)
        {
            byte[] texture = new byte[PIXELS_PER_TILE * 2];

            if (use_chr_rom)
            {
                byte[] top_tile = LoadBGTexture(index);
                byte[] bottom_tile = LoadBGTexture((byte)(index + 1));
                texture = top_tile.Concat(bottom_tile).ToArray();

                return texture;
            }

            int real_index = index * BYTES_PER_CHR_TILE;
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    texture[i * 8 + (7 - j)] = (byte)(
                        ((chr_sp[real_index +     i] & (1 << j)) >> j) | // gets j-th bit of byte real_index+i
                        ((chr_sp[real_index + 8 + i] & (1 << j)) >> j) * 2 // gets j-th bit of byte real_index+i+8, and multiplies by 2
                    ); // stores resulting byte in 1st half of texture, top to bottom, right to left
                }
            }
            real_index += BYTES_PER_CHR_TILE;
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    texture[i * 8 + (7 - j) + PIXELS_PER_TILE] = (byte)(
                        ((chr_sp[real_index +     i] & (1 << j)) >> j) | // gets j-th bit of byte real_index+i
                        ((chr_sp[real_index + 8 + i] & (1 << j)) >> j) * 2 // gets j-th bit of byte real_index+i+8, and multiplies by 2
                    ); // stores resulting byte in 2nd half of texture, top to bottom, right to left
                }
            }

            return texture;
        }

        public static void LoadPPUPage(PPUDataGroup group, byte page, byte screen_index)
        {
            switch (group)
            {
                case PPUDataGroup.OVERWORLD:
                    Menu.blue_candle_limit_reached = false;
                    using (Stream stream = File.OpenRead(@"Data\TILES_OVERWORLD.bin"))
                    {
                        BinaryReader reader = new BinaryReader(stream);
                        stream.Seek(page * 0xb0, SeekOrigin.Begin);
                        for (int i = screen_index * 176; i < screen_index * 176 + 176; i++)
                        {
                            Screen.meta_tiles[i].SetPPUValues(reader.ReadByte());
                        }
                    }
                    using (Stream stream = File.OpenRead(@"Data\PLT_OVERWORLD.bin"))
                    {
                        BinaryReader reader = new BinaryReader(stream);
                        stream.Seek(6 * 8 * 22 + page, SeekOrigin.Begin);
                        stream.Seek((8 * 22) * reader.ReadByte(), SeekOrigin.Begin);
                        byte a;
                        int start, lim;
                        if ((screen_index % 2) == 1)
                        {
                            start = 0;
                            lim = 176;
                        }
                        else
                        {
                            start = 64;
                            lim = 240;
                        }

                        for (int i = start; i < lim; i++)
                        {
                            a = reader.ReadByte();
                            ppu_plt[i * 4 + 0 + screen_index * 960] = (byte)((a & 0b11000000) >> 6);
                            ppu_plt[i * 4 + 1 + screen_index * 960] = (byte)((a & 0b00110000) >> 4);
                            ppu_plt[i * 4 + 2 + screen_index * 960] = (byte)((a & 0b00001100) >> 2);
                            ppu_plt[i * 4 + 3 + screen_index * 960] = (byte)(a & 0b00000011);
                        }
                    }
                    break;

                case PPUDataGroup.DUNGEON:
                    using (Stream stream = File.OpenRead(@"Data\TILES_DUNGEON.bin"))
                    {
                        BinaryReader reader = new BinaryReader(stream);
                        if (DungeonCode.room_list[page] < 0x2a)
                        {
                            int screen_1_exception = 0;
                            if (screen_index == 1)
                                screen_1_exception = -256;
                            int lim = ppu.Length / 4 + screen_index * 960 + screen_1_exception;

                            for (int i = 256 + screen_index * 960 + screen_1_exception; i < lim; i++)
                            {
                                ppu[i] = reader.ReadByte();
                                ppu_plt[i] = 2;
                            }

                            for (int i = 256 + 128 + screen_1_exception + screen_index * 960; i < 832 + screen_1_exception + screen_index * 960; i++)
                            {
                                byte palette = 2;
                                if (i % 32 >= 4 && i % 32 <= 0x1b)
                                {
                                    if (DungeonCode.rooms_with_palette_3.Contains(page) || DungeonCode.GetRoomDarkness(page))
                                        palette = 3;
                                    else if (DungeonCode.rooms_with_palette_1.Contains(page))
                                        palette = 1;
                                }
                                ppu_plt[i] = palette;
                            }
                            stream.Seek(0x420 + (12 * 7) * DungeonCode.room_list[page], SeekOrigin.Begin);
                            for (int i = 2; i < 9; i++)
                            {
                                for (int j = 2 + screen_index * 176; j < 14 + screen_index * 176; j++)
                                {
                                    Screen.meta_tiles[i * 16 + j].SetPPUValues(reader.ReadByte());
                                    if (i == 2 && Screen.meta_tiles[i * 16 + j].tile_index is 0 or 5 or 7)
                                        Screen.meta_tiles[i * 16 + j].tile_index = 10;
                                }
                            }
                            byte[] metatiles_to_reset = { 80, 81, 94, 95, 7, 8, 23, 24, 151, 152, 167, 168 };
                            foreach (byte i in metatiles_to_reset)
                                Screen.meta_tiles[i].tile_index = 1;
                            DungeonCode.DrawDoors(page, screen_index);
                        }
                        else
                        {
                            stream.Seek(0x2c0 + (DungeonCode.room_list[page] - 0x2a) * 176, SeekOrigin.Begin);
                            for (int i = screen_index * 176; i < screen_index * 176 + 176; i++)
                            {
                                Screen.meta_tiles[i].SetPPUValues(reader.ReadByte());
                            }
                            int lim = ppu.Length / 4 + screen_index * 960;
                            for (int i = 256 + screen_index * 960; i < lim; i++)
                            {
                                ppu_plt[i] = 2;
                            }
                        }
                    }
                    break;

                case PPUDataGroup.OTHER:
                    using (Stream stream = File.OpenRead(@"Data\TILES_OTHER.bin"))
                    {
                        BinaryReader reader = new BinaryReader(stream);
                        stream.Seek(page * 0x3c0, SeekOrigin.Begin);
                        for (int i = 0; i < ppu.Length / 4; i++)
                        {
                            ppu[i + screen_index * 960] = reader.ReadByte();
                        }
                    }
                    using (Stream stream = File.OpenRead(@"Data\PLT_OTHER.bin"))
                    {
                        BinaryReader reader = new BinaryReader(stream);
                        stream.Seek(page * 0xf0, SeekOrigin.Begin);
                        byte a;
                        for (int i = 0; i < ppu_plt.Length / 16; i++)
                        {
                            a = reader.ReadByte();
                            ppu_plt[i * 4 + screen_index * 960] = (byte)((a & 0b11000000) >> 6);
                            ppu_plt[i * 4 + 1 + screen_index * 960] = (byte)((a & 0b00110000) >> 4);
                            ppu_plt[i * 4 + 2 + screen_index * 960] = (byte)((a & 0b00001100) >> 2);
                            ppu_plt[i * 4 + 3 + screen_index * 960] = (byte)(a & 0b00000011);
                        }
                    }
                    break;
            }
        }

        //TODO: remove
        public static void ScrollPPU_V(PPUDataGroup group, int row_index, int ppu_index)
        {
            switch (group)
            {
                case PPUDataGroup.OVERWORLD:
                case PPUDataGroup.DUNGEON:
                    break;
                case PPUDataGroup.OTHER:
                    using (Stream stream = File.OpenRead(@"Data\TILES_OTHER.bin"))
                    {
                        BinaryReader reader = new BinaryReader(stream);
                        stream.Seek(row_index, SeekOrigin.Begin);
                        for (int i = 0; i < 32; i++)
                        {
                            ppu[i + (ppu_index % 60) * 32] = reader.ReadByte();
                        }
                    }
                    using (Stream stream = File.OpenRead(@"Data\PLT_OTHER.bin"))
                    {
                        BinaryReader reader = new BinaryReader(stream);
                        stream.Seek(row_index / 4, SeekOrigin.Begin);
                        byte a;
                        for (int i = 0; i < 8; i++)
                        {
                            a = reader.ReadByte();
                            ppu_plt[i * 4 +     32 * (ppu_index % 60)] = (byte)((a & 0b11000000) >> 6);
                            ppu_plt[i * 4 + 1 + 32 * (ppu_index % 60)] = (byte)((a & 0b00110000) >> 4);
                            ppu_plt[i * 4 + 2 + 32 * (ppu_index % 60)] = (byte)((a & 0b00001100) >> 2);
                            ppu_plt[i * 4 + 3 + 32 * (ppu_index % 60)] = (byte) (a & 0b00000011);
                        }
                    }
                    break;
            }
        }

        public static void DrawHUDBG()
        {
            using (Stream stream = File.OpenRead(@"Data\TILES_OTHER.bin"))
            {
                BinaryReader reader = new BinaryReader(stream);
                stream.Seek(11 * 0x3c0, SeekOrigin.Begin);
                for (int i = 0; i < 256; i++)
                {
                    ppu[i] = reader.ReadByte();
                }
            }

            if (Program.gamemode == Program.Gamemode.DUNGEON)
            {
                // writes "LEVEL-" above the map
                byte[] level_text = { (byte)Text.L, (byte)Text.E, (byte)Text.V, (byte)Text.E, (byte)Text.L, (byte)Text.DASH }; // LEVEL-
                for (int i = 0; i < level_text.Length; i++)
                {
                    ppu[0x42 + i] = level_text[i];
                }

                // clears the area on screen where the map is with empty tiles
                for (int i = 0; i < 4; i++)
                    for (int j = 0; j < 8; j++)
                        ppu[0x62 + i * PPU_WIDTH + j] = 0x24;
            }

            using (Stream stream = File.OpenRead(@"Data\PLT_OTHER.bin"))
            {
                BinaryReader reader = new BinaryReader(stream);
                stream.Seek(11 * 0xf0, SeekOrigin.Begin);
                byte a;
                for (int i = 0; i < 64; i++)
                {
                    a = reader.ReadByte();
                    ppu_plt[i * 4    ] = (byte)((a & 0b11000000) >> 6);
                    ppu_plt[i * 4 + 1] = (byte)((a & 0b00110000) >> 4);
                    ppu_plt[i * 4 + 2] = (byte)((a & 0b00001100) >> 2);
                    ppu_plt[i * 4 + 3] = (byte) (a & 0b00000011);
                }
            }
        }

        public static void DrawMenu()
        {
            int page_index = Program.gamemode == Program.Gamemode.OVERWORLD ? 11 : 12;

            using (Stream stream = File.OpenRead(@"Data\TILES_OTHER.bin"))
            {
                BinaryReader reader = new BinaryReader(stream);
                stream.Seek(page_index * 0x3c0 + 256 + 32, SeekOrigin.Begin);
                for (int i = 256 + 960; i < 1920; i++)
                {
                    ppu[i] = reader.ReadByte();
                }
            }

            using (Stream stream = File.OpenRead(@"Data\PLT_OTHER.bin"))
            {
                BinaryReader reader = new BinaryReader(stream);
                stream.Seek(11 * 0xf0 + 64, SeekOrigin.Begin);
                byte a;
                for (int i = 64 + 240; i < 480; i++)
                {
                    a = reader.ReadByte();
                    ppu_plt[i * 4    ] = (byte)((a & 0b11000000) >> 6);
                    ppu_plt[i * 4 + 1] = (byte)((a & 0b00110000) >> 4);
                    ppu_plt[i * 4 + 2] = (byte)((a & 0b00001100) >> 2);
                    ppu_plt[i * 4 + 3] = (byte) (a & 0b00000011);
                }
            }
        }

        public static void LoadNewRomData(ROMData data)
        {
            switch (data)
            {
                case ROMData.CHR_SURFACE:
                    using (Stream stream = File.OpenRead(@"Data\CHR_OVERWORLD.bin"))
                    {
                        BinaryReader reader = new BinaryReader(stream);
                        for (int i = 0; i < stream.Length; i++)
                        {
                            chr_bg[i + 0x700] = reader.ReadByte();
                        }
                    }
                    using (Stream stream = File.OpenRead(@"Data\SPR_OVERWORLD.bin"))
                    {
                        BinaryReader reader = new BinaryReader(stream);
                        for (int i = 0; i < stream.Length; i++)
                        {
                            chr_sp[i + 0x8e0] = reader.ReadByte();
                        }
                    }
                    break;
                case ROMData.CHR_DUNGEON:
                    using (Stream stream = File.OpenRead(@"Data\CHR_DUNGEON.bin"))
                    {
                        BinaryReader reader = new BinaryReader(stream);
                        for (int i = 0; i < stream.Length; i++)
                        {
                            chr_bg[i + 0x700] = reader.ReadByte();
                        }
                    }
                    break;
            }
        }
    }
}