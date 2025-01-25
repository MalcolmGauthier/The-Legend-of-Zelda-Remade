using The_Legend_of_Zelda.Gameplay;
using static The_Legend_of_Zelda.Gameplay.Program;

namespace The_Legend_of_Zelda.Rendering
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
        LINK_DOWN_R = 0xa,
        LINK_UP_L = 0xc,
        LINK_UP_R = 0xe,
        LINK_SIDE_ATK_L = 0x10,
        LINK_SIDE_ATK_R = 0x12,
        LINK_DOWN_ATK_L = 0x14,
        LINK_DOWN_ATK_R = 0x16,
        LINK_UP_ATK_L = 0x18,
        LINK_UP_ATK_R = 0x1a,
        BLANK = 0x1c,
        SELECTOR = 0x1e,
        SWORD = 0x20,
        BAIT = 0x22,
        RECORDER = 0x24,
        CANDLE = 0x26,
        ARROW = 0x28,
        BOW = 0x2a,
        MAGICAL_KEY = 0x2c,
        KEY = 0x2e,
        EXPLOSION_PARTICLE = 0x30,
        RUPEE = 0x32,
        BOMB = 0x34,
        BOOMERANG = 0x36,
        BOOMERANG_45 = 0x38,
        BOOMERANG_90 = 0x3a,
        SPARK = 0x3c,
        DOT = 0x3e,
        POTION = 0x40,
        BOOK_OF_MAGIC = 0x42,
        FIREBALL = 0x44,
        RING = 0x46,
        MAGIC_SWORD = 0x48,
        ROD = 0x4a,
        MAP = 0x4c,
        POWER_BRACELET = 0x4e,
        FAIRY_1 = 0x50,
        FAIRY_2 = 0x52,
        LINK_SIDE_R2_SHIELD = 0x54,
        MAGICAL_SHIELD = 0x56,
        LINK_DOWN_SHIELD_1 = 0x58,
        LINK_DOWN_SHIELD_2 = 0x5a,
        FIRE_L = 0x5c,
        FIRE_R = 0x5e,
        LINK_DOWN_MAG_SHIELD = 0x60,
        BIG_SPARK_1 = 0x62,
        BIG_SPARK_2 = 0x64,
        CLOCK = 0x66,
        HEART_CONTAINER = 0x68,
        COMPASS = 0x6a,
        RAFT = 0x6c,
        TRIFORCE = 0x6e,
        SMOKE_1 = 0x70,
        SMOKE_2 = 0x72,
        SMOKE_3 = 0x74,
        LADDER = 0x76,
        LINK_ITEM_GET = 0x78,
        MAGIC_BEAM_UP = 0x7a,
        MAGIC_BEAM_SIDE_1 = 0x7c,
        MAGIC_BEAM_SIDE_2 = 0x7e,
        LINK_SIDE_R1_SHIELD = 0x80,
        SWORD_SIDE_1 = 0x82,
        SWORD_SIDE_2 = 0x84,
        ARROW_SIDE_1 = 0x86,
        ARROW_SIDE_2 = 0x88,
        ROD_SIDE_1 = 0x8a,
        ROD_SIDE_2 = 0x8c,
    }

    public static class Textures
    {
        public enum PPUDataGroup
        {
            OVERWORLD,
            DUNGEON,
            OTHER
        }

        public enum OtherPPUPages
        {
            TITLE,
            EMPTY,
            STORY,
            INTRO_1,
            INTRO_2,
            INTRO_3,
            INTRO_4,
            INTRO_5,
            INTRO_6,
            FILE_SELECT,
            REGISTER_NAME,
            OVERWORLD_MENU,
            DUNGEON_MENU,
            GAME_OVER
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
            // a tile bank for the splash screen exists, but because the only way to load the splash screen is to 
            // start the game, it does not need to be an option for loading, as it will just be the default starting value
        }

        public const int TILE_SIZE = 8;
        public const int PPU_WIDTH = NES_OUTPUT_WIDTH / TILE_SIZE;
        public const int PPU_HEIGHT = NES_OUTPUT_HEIGHT / TILE_SIZE;
        // the 4 screens worth of image data allow for scrolling both vertically and horizontally.
        // in the game, they are layed out like this:
        // +-----+-----+
        // |  0  |  2  |
        // +-----+-----+
        // |  1  |  3  |
        // +-----+-----+
        // 
        // the 4th screen (screen 3) is never used, so theoretically we could save space
        // by removing the last quarter of the tiles, metatiles, but not vram.
        // for more info on vram layout, check the comment inside Tile.ChangeTexture()
        public const int PPU_SCREENS = 4;
        public const int SCREEN_TILES = PPU_WIDTH * PPU_HEIGHT; // 960
        public const int VRAM_WIDTH = PPU_WIDTH * TILE_SIZE * 2; // 512
        public const int VRAM_HEIGHT = PPU_HEIGHT * TILE_SIZE * 2; // 480
        public const int PIXELS_PER_TILE = TILE_SIZE * TILE_SIZE; // 64
        public const int BYTES_PER_CHR_TILE = 16;
        public const int CHR_TILESET_SIZE = 256;
        public const int HUD_HEIGHT = 64;
        public const int HUD_TILE_COUNT = (HUD_HEIGHT / TILE_SIZE) * PPU_WIDTH; // 256

        public static byte[] chr_bg = new byte[CHR_TILESET_SIZE * BYTES_PER_CHR_TILE]; // 4096
        public static byte[] chr_sp = new byte[CHR_TILESET_SIZE * BYTES_PER_CHR_TILE]; // 4096
        public static byte[] vram = new byte[SCREEN_TILES * PPU_SCREENS * PIXELS_PER_TILE]; // 245000
        public static byte[] ppu = new byte[SCREEN_TILES * PPU_SCREENS]; // 3840
        public static byte[] ppu_plt = new byte[SCREEN_TILES * PPU_SCREENS]; // 3840

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
                    ppu_plt[i * 4] = (byte)((a & 0b11000000) >> 6);
                    ppu_plt[i * 4 + 1] = (byte)((a & 0b00110000) >> 4);
                    ppu_plt[i * 4 + 2] = (byte)((a & 0b00001100) >> 2);
                    ppu_plt[i * 4 + 3] = (byte)(a & 0b00000011);
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
                        (chr_bg[real_index + i] & 1 << j) >> j | // gets j-th bit of byte real_index+i
                        ((chr_bg[real_index + 8 + i] & 1 << j) >> j) * 2 // gets j-th bit of byte real_index+i+8, and multiplies by 2
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
                        (chr_sp[real_index + i] & 1 << j) >> j | // gets j-th bit of byte real_index+i
                        ((chr_sp[real_index + 8 + i] & 1 << j) >> j) * 2 // gets j-th bit of byte real_index+i+8, and multiplies by 2
                    ); // stores resulting byte in 1st half of texture, top to bottom, right to left
                }
            }
            real_index += BYTES_PER_CHR_TILE;
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    texture[i * 8 + (7 - j) + PIXELS_PER_TILE] = (byte)(
                        (chr_sp[real_index + i] & 1 << j) >> j | // gets j-th bit of byte real_index+i
                        ((chr_sp[real_index + 8 + i] & 1 << j) >> j) * 2 // gets j-th bit of byte real_index+i+8, and multiplies by 2
                    ); // stores resulting byte in 2nd half of texture, top to bottom, right to left
                }
            }

            return texture;
        }

        public static void LoadPPUPage(PPUDataGroup group, OtherPPUPages page, byte screen_index) => LoadPPUPage(group, (byte)page, screen_index);
        public static void LoadPPUPage(PPUDataGroup group, byte page, byte screen_index)
        {
            int ppu_start_index = screen_index * SCREEN_TILES;

            switch (group)
            {
                case PPUDataGroup.OVERWORLD:
                    using (Stream stream = File.OpenRead(@"Data\TILES_OVERWORLD.bin"))
                    {
                        BinaryReader reader = new BinaryReader(stream);
                        stream.Seek(page * 176, SeekOrigin.Begin);
                        for (int i = screen_index * 176; i < screen_index * 176 + 176; i++)
                        {
                            Screen.meta_tiles[i].SetPPUValues(reader.ReadByte());
                        }
                    }
                    using (Stream stream = File.OpenRead(@"Data\PLT_OVERWORLD.bin"))
                    {
                        BinaryReader reader = new BinaryReader(stream);
                        stream.Seek(6 * 8 * 22 + page, SeekOrigin.Begin);
                        stream.Seek(8 * 22 * reader.ReadByte(), SeekOrigin.Begin);
                        byte a;
                        int start = HUD_HEIGHT;
                        int lim = NES_OUTPUT_HEIGHT;
                        // when scrolling vertically in overworld or dungeon, screen 1 is used in a special way where the world is rendered at the
                        // top where the HUD would be in the other screens. thus, we need to update the top of the screen, and not under the HUD.
                        // screen 3 is never used in the entire game, so we don't care about that one.
                        if (screen_index == 1)
                        {
                            start -= HUD_HEIGHT;
                            lim -= HUD_HEIGHT;
                        }

                        for (int i = start; i < lim; i++)
                        {
                            a = reader.ReadByte();
                            ppu_plt[i * 4 + 0 + ppu_start_index] = (byte)((a & 0b11000000) >> 6);
                            ppu_plt[i * 4 + 1 + ppu_start_index] = (byte)((a & 0b00110000) >> 4);
                            ppu_plt[i * 4 + 2 + ppu_start_index] = (byte)((a & 0b00001100) >> 2);
                            ppu_plt[i * 4 + 3 + ppu_start_index] = (byte)(a & 0b00000011);
                        }
                    }
                    break;

                case PPUDataGroup.DUNGEON:
                    using (Stream stream = File.OpenRead(@"Data\TILES_DUNGEON.bin"))
                    {
                        BinaryReader reader = new BinaryReader(stream);

                        // IF NORMAL ROOM
                        if (DC.room_list[page] is not (0x2a or 0x2b))
                        {
                            int screen_1_exception = 0;
                            if (screen_index == 1)
                                screen_1_exception = -((HUD_HEIGHT / TILE_SIZE) * PPU_WIDTH);
                            int lim = SCREEN_TILES + ppu_start_index + screen_1_exception;

                            // draw the walls around the room, this writes 0's to the middle, but they will be overwritten
                            for (int i = 256 + ppu_start_index + screen_1_exception; i < lim; i++)
                            {
                                ppu[i] = reader.ReadByte();
                                ppu_plt[i] = 2;
                            }

                            // set palette of entire room
                            byte inner_palette = 2;
                            // rooms with water or darkness require palette 3
                            if (DC.rooms_with_palette_3.Contains(page) || DC.GetRoomDarkness(page))
                                inner_palette = 3;
                            // rooms with sand require palette 1
                            else if (DC.rooms_with_palette_1.Contains(page))
                                inner_palette = 1;
                            for (int i = 256 + 128 + screen_1_exception + ppu_start_index; i < 832 + screen_1_exception + ppu_start_index; i++)
                            {
                                byte palette = 2;
                                if (i % 32 >= 4 && i % 32 <= 27)
                                {
                                    palette = inner_palette;
                                }
                                ppu_plt[i] = palette;
                            }

                            // find and draw inside of room, depending on its room type
                            stream.Seek(0x420 + 12 * 7 * DC.room_list[page], SeekOrigin.Begin);
                            for (int i = 2; i < 9; i++)
                            {
                                for (int j = 2 + screen_index * 176; j < 14 + screen_index * 176; j++)
                                {
                                    Screen.meta_tiles[i * 16 + j].SetPPUValues(reader.ReadByte());
                                    // top row of dungeon room doesn't allow link be along the wall.
                                    if (i == 2 && Screen.meta_tiles[i * 16 + j].tile_index_D is (DungeonMetatile.GROUND or DungeonMetatile.SAND or DungeonMetatile.VOID))
                                        Screen.meta_tiles[i * 16 + j].tile_index_D = DungeonMetatile.ROOM_TOP;
                                }
                            }

                            DC.InitDoors(page, screen_index);
                        }
                        // IF SIDE VIEW
                        else
                        {
                            stream.Seek(0x2c0 + (DC.room_list[page] - (byte)DungeonCode.RoomType.SIDESCROLL_ITEM) * 176, SeekOrigin.Begin);
                            for (int i = screen_index * 176; i < screen_index * 176 + 176; i++)
                            {
                                Screen.meta_tiles[i].SetPPUValues(reader.ReadByte());
                            }
                            int lim = SCREEN_TILES + ppu_start_index;
                            for (int i = 256 + ppu_start_index; i < lim; i++)
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
                        for (int i = 0; i < SCREEN_TILES; i++)
                        {
                            ppu[i + screen_index * SCREEN_TILES] = reader.ReadByte();
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
                            ppu_plt[i * 4 + 0 + screen_index * SCREEN_TILES] = (byte)((a & 0b11000000) >> 6);
                            ppu_plt[i * 4 + 1 + screen_index * SCREEN_TILES] = (byte)((a & 0b00110000) >> 4);
                            ppu_plt[i * 4 + 2 + screen_index * SCREEN_TILES] = (byte)((a & 0b00001100) >> 2);
                            ppu_plt[i * 4 + 3 + screen_index * SCREEN_TILES] = (byte)((a & 0b00000011) >> 0);
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

            if (gamemode == Gamemode.DUNGEON)
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
                    ppu_plt[i * 4] = (byte)((a & 0b11000000) >> 6);
                    ppu_plt[i * 4 + 1] = (byte)((a & 0b00110000) >> 4);
                    ppu_plt[i * 4 + 2] = (byte)((a & 0b00001100) >> 2);
                    ppu_plt[i * 4 + 3] = (byte)(a & 0b00000011);
                }
            }
        }

        public static void DrawMenu()
        {
            int page_index = gamemode == Gamemode.OVERWORLD ? (int)OtherPPUPages.OVERWORLD_MENU : (int)OtherPPUPages.DUNGEON_MENU;

            using (Stream stream = File.OpenRead(@"Data\TILES_OTHER.bin"))
            {
                BinaryReader reader = new BinaryReader(stream);
                stream.Seek(page_index * 0x3c0 + 256 + 32, SeekOrigin.Begin);
                for (int i = 256 + SCREEN_TILES; i < 1920; i++)
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
                    ppu_plt[i * 4] = (byte)((a & 0b11000000) >> 6);
                    ppu_plt[i * 4 + 1] = (byte)((a & 0b00110000) >> 4);
                    ppu_plt[i * 4 + 2] = (byte)((a & 0b00001100) >> 2);
                    ppu_plt[i * 4 + 3] = (byte)(a & 0b00000011);
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
                case ROMData.SPR_DUNGEON_127:
                    break;
                case ROMData.SPR_DUNGEON_358:
                    break;
                case ROMData.SPR_DUNGEON_469:
                    break;
                case ROMData.SPR_DUNGEON_BOSS_1257:
                    break;
                case ROMData.SPR_DUNGEON_BOSS_3468:
                    break;
                case ROMData.SPR_DUNGEON_BOSS_9:
                    break;
            }
        }
    }
}