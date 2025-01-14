using static The_Legend_of_Zelda.Menu;
using static The_Legend_of_Zelda.Screen;
namespace The_Legend_of_Zelda
{
    public static class DungeonCode
    {
        public enum DoorType
        {
            NONE,
            OPEN,
            KEY,
            BOMBABLE,
            CLOSED_ENEMY,
            CLOSED_PUSH,
            CLOSED_TRIFORCE,
            CLOSED_ALWAYS,
            WALK_THROUGH,
        }

        enum DungeonEnemies
        {
            // common dungeon enemies
            NONE,
            GEL,
            BUBBLE,
            BUBBLE_BLUE,
            BUBBLE_RED,
            KEESE,
            NPC,
            RAZOR_TRAP,
            //TODO: proj statues (hardcoded)

            // dungeons 1, 2, 7
            STALFOS,
            GORIYA,
            GORIYA_HARDER,
            WALLMASTER,
            ROPE,
            ROPE_HARDER,

            // dungeons 3, 5, 8
            ZOL,
            GIBDO,
            DARKNUT,
            DARKNUT_HARDER,
            POLSVOICE,

            // dungeons 4, 6, 9
            // ZOL is also here
            VIRE,
            LIKELIKE,
            WIZROBE,
            WIZROBE_HARDER,
            LANMOLA,
            LANMOLA_HARDER,
        }

        enum Bosses
        {
            AQUAMENTUS,
            DODONGO,
            DODONGO_TRIPLE,
            MANHANDLA,
            GLEEOK,
            GLEEOK_TRIPLE,
            GLEEOK_QUADRUPLE,
            DIGDOGGER,
            DIGDOGGER_TRIPLE,
            GOHMA,
            GOHMA_HARDER,
            PATRA,
            MOLDORM,
            GANON,
        }

        public static byte current_dungeon;
        public static byte current_screen;
        public static byte nb_enemies_alive = 0;
        public static sbyte opening_animation_timer = 0;
        static byte scroll_destination;
        static byte link_walk_animation_timer = 0;

        public static int scroll_animation_timer = 1000;
        static int dark_room_animation_timer = 0;

        public static bool warp_flag = false;
        public static bool block_push_flag = false;
        static bool is_dark = false;

        public static DoorType[] door_types = new DoorType[4];
        static Direction scroll_direction;
        static FlickeringSprite compass_dot = new FlickeringSprite(0x3e, 5, 0, 0, 16, 0x3e, second_palette_index: 6);

        public static readonly byte[] room_list = {
            0x2A, 0x14, 0x14, 0x29, 0x0B, 0x0B, 0x0F, 0x2B, 0x29, 0x1A, 0x1B, 0x0B, 0x2A, 0x06, 0x0C, 0x2B,
            0x19, 0x1C, 0x16, 0x05, 0x18, 0x10, 0x08, 0x29, 0x18, 0x29, 0x14, 0x19, 0x0B, 0x29, 0x16, 0x29,
            0x08, 0x1E, 0x02, 0x29, 0x00, 0x1C, 0x02, 0x00, 0x0D, 0x17, 0x0C, 0x0D, 0x0E, 0x1D, 0x1E, 0x0D,
            0x14, 0x1C, 0x06, 0x14, 0x09, 0x0B, 0x0F, 0x00, 0x16, 0x09, 0x14, 0x09, 0x03, 0x0A, 0x0B, 0x09,
            0x1B, 0x00, 0x12, 0x13, 0x00, 0x0F, 0x04, 0x0E, 0x0D, 0x29, 0x06, 0x07, 0x04, 0x08, 0x17, 0x0D,
            0x0E, 0x02, 0x00, 0x00, 0x07, 0x03, 0x07, 0x05, 0x11, 0x05, 0x03, 0x04, 0x05, 0x1C, 0x00, 0x09,
            0x02, 0x10, 0x29, 0x11, 0x00, 0x05, 0x08, 0x03, 0x2A, 0x04, 0x15, 0x03, 0x0C, 0x04, 0x16, 0x29,
            0x07, 0x01, 0x05, 0x05, 0x01, 0x01, 0x00, 0x2A, 0x00, 0x01, 0x00, 0x01, 0x02, 0x2A, 0x01, 0x02,
            0x29, 0x14, 0x0E, 0x0F, 0x1F, 0x13, 0x00, 0x2A, 0x2A, 0x1C, 0x29, 0x0C, 0x0C, 0x14, 0x29, 0x0C,
            0x1A, 0x07, 0x0C, 0x00, 0x0E, 0x11, 0x0E, 0x0C, 0x13, 0x0F, 0x29, 0x00, 0x22, 0x1E, 0x14, 0x07,
            0x29, 0x0C, 0x0A, 0x0B, 0x0B, 0x29, 0x00, 0x2A, 0x0C, 0x0F, 0x02, 0x17, 0x09, 0x25, 0x05, 0x14,
            0x08, 0x0E, 0x0F, 0x29, 0x19, 0x29, 0x1F, 0x10, 0x14, 0x0F, 0x24, 0x21, 0x1E, 0x29, 0x00, 0x1F,
            0x29, 0x16, 0x2B, 0x1F, 0x22, 0x0F, 0x00, 0x2B, 0x1A, 0x00, 0x23, 0x29, 0x00, 0x00, 0x0F, 0x03,
            0x00, 0x04, 0x21, 0x29, 0x0E, 0x11, 0x0E, 0x16, 0x1E, 0x11, 0x10, 0x1F, 0x15, 0x0C, 0x21, 0x1E,
            0x00, 0x1F, 0x20, 0x03, 0x0E, 0x1C, 0x0F, 0x2B, 0x2B, 0x0C, 0x29, 0x10, 0x07, 0x16, 0x29, 0x2B,
            0x1F, 0x01, 0x02, 0x2A, 0x0C, 0x1F, 0x01, 0x1A, 0x2B, 0x10, 0x2A, 0x07, 0x0C, 0x2B, 0x01, 0x2B
        };
        public static readonly ushort[] dark_rooms = {
            0b0000000001100000,
            0b0010000000000000,
            0b0100010001000001,
            0b0100100010000001,
            0b0000000010000010,
            0b0000000010000001,
            0b0000000000100010,
            0b0010000000000000,
            0b0000000001000000,
            0b1000000000010100,
            0b0000000000001000,
            0b0000000000010000,
            0b0001000001100000,
            0b0010010110000001,
            0b0010000000000100,
            0b0000000100000000,
        };
        public static readonly byte[] connection_IDs = {
            #region code
            /*
                0 - none
                1 - open
                2 - key
                3 - bombable
                4 - blocked: needs push down/right
                5 - blocked: needs screen clear up/left
                6 - blocked: needs screen clear down/right
                7 - blocked: needs screen clear up & down
                8 - blocked: needs screen clear left & right
                9 - blocked: needs push up/left
                a - blocked: permanent up/left
                b - blocked: permanent down/right
                c - blocked: needs triforce
                d - walk-through wall
                e - walk-through wall up/left
                f - walk-through wall down/right

                IF ROOM NEEDS CLEAR, BLOCK DOORS THAT
                LEAD TO PUSH ROOMS AS CLEAR DOORS
                order = bottom, right for that room
            */
#endregion
            0x00, 0x23, 0x53, 0x60, 0x60, 0x06, 0x60, 0x00, 0x62, 0x31, 0x10, 0x60, 0x00, 0x02, 0x20, 0x00,
            0x75, 0x21, 0x01, 0x00, 0x20, 0x10, 0x73, 0x10, 0x53, 0x33, 0x09, 0x00, 0x60, 0x30, 0x03, 0x00,
            0x43, 0x60, 0x05, 0x40, 0x61, 0x00, 0x21, 0x30, 0x21, 0x20, 0x02, 0x20, 0x62, 0x01, 0x02, 0x10,
            0x50, 0x05, 0x00, 0x20, 0x00, 0x60, 0x11, 0x30, 0x12, 0x05, 0x00, 0x10, 0x00, 0x25, 0x00, 0x10,
            0x60, 0x11, 0x12, 0x12, 0x13, 0x70, 0x51, 0x30, 0x10, 0x04, 0x21, 0x32, 0x31, 0x00, 0x21, 0x10,
            0x10, 0x52, 0x01, 0x13, 0x08, 0x00, 0x12, 0x30, 0x01, 0x10, 0x05, 0x11, 0x00, 0xB8, 0x61, 0x00,
            0x10, 0x00, 0x20, 0x10, 0x06, 0x11, 0x12, 0x00, 0x00, 0x12, 0x00, 0x20, 0x03, 0x03, 0x12, 0x30,
            0x02, 0x11, 0x00, 0x01, 0x10, 0x11, 0x00, 0x00, 0x01, 0x10, 0x01, 0x11, 0x00, 0x00, 0x11, 0x00,
            0x33, 0x78, 0x00, 0x20, 0x63, 0x00, 0x70, 0x00, 0x00, 0xA2, 0x60, 0x03, 0x00, 0x03, 0x20, 0x30,
            0x52, 0x03, 0x03, 0x02, 0x00, 0x13, 0x25, 0x00, 0x30, 0x50, 0x22, 0x20, 0x12, 0x31, 0x10, 0x30,
            0x20, 0x03, 0x05, 0x00, 0x60, 0x03, 0x30, 0x00, 0x00, 0xA3, 0x02, 0x23, 0x10, 0x32, 0x23, 0x30,
            0x05, 0x63, 0x00, 0x13, 0x30, 0x60, 0x28, 0x00, 0x23, 0xA0, 0xC0, 0x03, 0x00, 0x00, 0x03, 0x10,
            0x20, 0x10, 0x00, 0x08, 0x00, 0x12, 0x60, 0x00, 0x20, 0xA0, 0x60, 0x30, 0x11, 0x31, 0x10, 0x10,
            0x11, 0x35, 0x30, 0x60, 0x01, 0x06, 0x32, 0x00, 0x08, 0x10, 0x00, 0x20, 0x13, 0x33, 0x60, 0x00,
            0x13, 0x11, 0x11, 0x01, 0x01, 0x00, 0x10, 0x00, 0x00, 0x11, 0x02, 0x20, 0x01, 0x06, 0x10, 0x00,
            0x00, 0x11, 0x00, 0x00, 0x06, 0x01, 0x11, 0x00, 0x00, 0x00, 0x00, 0x02, 0x00, 0x00, 0x10, 0x00
        };
        public static readonly byte[] starting_screens = {
            123, 117, 116, 121, 126, 113, 241, 246, 254 
        };
        public static readonly byte[] rooms_with_boses = {
            61, 54, 6, 69, 24, 27, 94, 44, 16, 20, 131, 132, 148, 162, 177, 178, 208, 228, 242, 245, 230, 197, 166, 150, 233, 218, 169, 175, 158, 202
        };
        public static readonly byte[] rooms_with_palette_3 = {
            9, 17, 18, 30, 33, 37, 40, 41, 43, 46, 47, 49, 52, 56, 57, 59, 63, 72, 78, 79, 80, 93, 95, 110, 113, 116, 117, 121, 123, 126, 
            132, 137, 144, 150, 157, 171, 172, 177, 182, 187, 188, 193, 195, 200, 202, 210, 214, 215, 216, 219, 222, 223, 225, 229, 237, 241, 245, 246, 247
        };
        public static readonly byte[] rooms_with_palette_1 = {
            54, 69, 131
        };
        public static readonly byte[] door_metatiles = {
            23, 151, 81, 94
        };
        public static readonly byte[,] dungeon_tileset_indexes = {
            {0x74,0x76,0x75,0x77}, // floor tile
            {0xb0,0xb2,0xb1,0xb3}, // block
            {0xf4,0xf4,0xf4,0xf4}, // water/lava
            {0x94,0x96,0x95,0x97}, // left statue
            {0xb4,0xb6,0xb5,0xb7}, // right statue
            {0x68,0x68,0x68,0x68}, // sand
            {0x70,0x72,0x71,0x73}, // stairs
            {0x24,0x24,0x24,0x24}, // black
            {0xfa,0xfa,0xfa,0xfa}, // bricks
            {0x6f,0x6f,0x6f,0x6f}  // gray stairs
        };
        public static readonly uint[] dungeon_enemy_list = {
            0x55550000, 0xbbccc000, 0x2288aa00, 0x60000000, 0x00000000, 0x00000000, 0xffff0001, 0x00005555, 0x60000000, 0x55555555, 0x70000000, 0x00000000, 0x55550000, 0xbbbbbb00, 0x00000000, 0x55550000,
            0xffff0005, 0xaa228800, 0xbbbbbb00, 0xbbcccaaa, 0xffff0009, 0xaa228800, 0x99999000, 0x60000000, 0xffff0003, 0x00000000, 0x99999000, 0xffff0004, 0x00000000, 0x60000000, 0x55555550, 0x60000000,
            0xbbccc000, 0xbbccc000, 0x55555555, 0x60000000, 0xbbcc7000, 0x22999000, 0xcccccccc, 0x11111000, 0x99999000, 0x11111000, 0x70000000, 0x99900000, 0xffff0007, 0xccccc000, 0x99999000, 0x9955cc00,
            0xbbcc2aaa, 0x99999000, 0xbbcc2aaa, 0x78800000, 0xbbccc000, 0x00000000, 0xffff000c, 0x75555000, 0x22999000, 0x99999000, 0x88aa2200, 0x88800000, 0x99999900, 0xffff0000, 0x00000000, 0xaaa00000,
            0x70000000, 0x22288555, 0x55555000, 0x88800000, 0x78800000, 0xffff0003, 0xccccc000, 0x6aaa0000, 0x88888000, 0x60000000, 0x11100000, 0x11111000, 0x99900000, 0xbbbbbbbb, 0x00000000, 0x99999000,
            0x55555555, 0xaaaaa000, 0x75555000, 0xaaa00000, 0xaaa00000, 0x55522288, 0x99999000, 0x11111000, 0x99999000, 0x55555555, 0x55555500, 0x88888000, 0x55555555, 0x88888000, 0xffff0002, 0x88888000,
            0x88888000, 0xaaaaaaaa, 0x60000000, 0x88888000, 0xcccccc00, 0xccccc000, 0xccc00000, 0x11111100, 0x55550000, 0x99900000, 0x99990000, 0x88800000, 0xbbbbb000, 0x99999000, 0x99900000, 0x60000000,
            0xbbbbb000, 0x00000000, 0xbbbbb000, 0x88888800, 0x00000000, 0x00000000, 0xccccc000, 0x55550000, 0x55555555, 0x00000000, 0x55500000, 0x00000000, 0x88888000, 0x55550000, 0x00000000, 0xccccc000,
            0x00000000, 0x999aaa00, 0xaaaaa000, 0xffff000c, 0xffff0002, 0xbbbbb222, 0xaaaaa000, 0x55550000, 0x55550000, 0x22288555, 0x60000000, 0x88aa2200, 0xbbcc7000, 0xbbccc000, 0x60000000, 0xbbccc222,
            0x22555aaa, 0xaaaaaa00, 0x999aaa00, 0xaaaaaa00, 0xffff0008, 0xccccc000, 0xffff000a, 0xaabbcc00, 0xbbccc222, 0xee000000, 0xdd000000, 0x88aa2200, 0xaaaa0000, 0xccc00000, 0xffff000b, 0xaaabbcc2,
            0x60000000, 0x999aaa00, 0xffff0000, 0x00000000, 0x00000000, 0x00000000, 0xffff0003, 0x55550000, 0xbbccc000, 0xffff000b, 0x22aa8800, 0x11111111, 0x99999000, 0xaa228800, 0x11111111, 0xffff000b,
            0x999aaa00, 0xffff0008, 0xffff000c, 0x60000000, 0xffff0006, 0x60000000, 0xbbbbbb00, 0xbcc22999, 0x7bbcc000, 0xaaabbcc2, 0x00000000, 0x11111111, 0x55555555, 0xbbccc000, 0x22288555, 0x99999900,
            0x60000000, 0x22555aaa, 0x55550000, 0xaaaaaa00, 0xcccccccc, 0xffff000a, 0x99922aab, 0x55550000, 0xbbcc0000, 0x7aaaa000, 0xffff000d, 0x60000000, 0xbbbbbb00, 0xbbcc2aaa, 0xcccccc00, 0x222bbcc0,
            0xffff0002, 0x999aaa00, 0x88888888, 0x60000000, 0xaaa00000, 0x55cc9900, 0xbbbbbb00, 0xcccccccc, 0x88888000, 0xaaaaaa00, 0xffff000b, 0xbbccaaa2, 0xbbccc000, 0xdd000000, 0x2288aa00, 0xbbccc000,
            0x75555000, 0xaaaaa000, 0x55555555, 0xaaaaaa00, 0xffff0007, 0x88888888, 0xffff0003, 0x55550000, 0x55550000, 0xffff000b, 0x55555555, 0x88888000, 0x2aaabbcc, 0x222bbccc, 0x60000000, 0x55550000,
            0xccccc000, 0x00000000, 0xffff000c, 0x55550000, 0x22aab999, 0xffff0003, 0x00000000, 0x22288555, 0x55550000, 0x00000000, 0x55550000, 0xbbccc222, 0xee000000, 0x55550000, 0x00000000, 0x55550000
        };
        public static readonly short[] bombable_connections = {
            3, 5, 18, 45, 49, 50, 51, 58, 61, 65, 78, 110, 137, 142, 150, 152, 167, 174, 217, 219, 256, 257, 
            265, 279, 283, 286, 291, 293, 299, 304, 314, 318, 323, 331, 332, 339, 343, 346, 349, 350, 355, 359, 
            360, 369, 375, 381, 406, 410, 418, 420, 428, 441, 442, 443, 449
        };
        public static readonly short[] key_door_connections = {
            2, 17, 27, 28, 34, 40, 76, 80, 82, 85, 86, 89, 93, 102, 113, 122, 133, 135, 148, 151, 163, 173, 205,
            211, 214, 221, 225, 262, 275, 284, 289, 295, 300, 308, 309, 310, 313, 320, 341, 342, 347, 348, 368, 
            384, 395, 400, 429, 438, 469, 470, 503
        };

        public static bool[] door_statuses = new bool[door_types.Length];

        public static void Init(byte dungeon)
        {
            Link.knockback_timer = 0;
            Link.iframes_timer = 0;
            Link.facing_direction = Direction.UP;

            Menu.InitHUD();
            // set number in hud "LEVEL-X"
            Textures.ppu[0x48] = (byte)(dungeon + 1);

            current_dungeon = dungeon;
            current_screen = starting_screens[current_dungeon];

            if (SaveLoad.GetMapFlag(current_dungeon))
            {
                Menu.DrawHudMap();
            }

            LoadPalette();
            Textures.LoadNewRomData(Textures.ROMData.CHR_DUNGEON);

            if (current_dungeon is 1 or 2 or 7)
                Textures.LoadNewRomData(Textures.ROMData.SPR_DUNGEON_127);
            else if (current_dungeon is 3 or 5 or 8)
                Textures.LoadNewRomData(Textures.ROMData.SPR_DUNGEON_358);
            else if (current_dungeon is 4 or 6 or 9)
                Textures.LoadNewRomData(Textures.ROMData.SPR_DUNGEON_469);

            if (current_dungeon is 1 or 2 or 5 or 7)
                Textures.LoadNewRomData(Textures.ROMData.SPR_DUNGEON_BOSS_1257);
            else if (current_dungeon is 3 or 4 or 6 or 8)
                Textures.LoadNewRomData(Textures.ROMData.SPR_DUNGEON_BOSS_3468);
            else if (current_dungeon == 9)
                Textures.LoadNewRomData(Textures.ROMData.SPR_DUNGEON_BOSS_9);

            for (int i = 0; i < meta_tiles.Length; i++)
            {
                meta_tiles[i].tile_index = 1;
            }

            LinkWalkAnimation();
            opening_animation_timer = 0;
            OverworldCode.black_square_stairs_flag = false;
            OverworldCode.stair_warp_flag = false;
            map_dot.shown = false;
        }

        public static void Tick()
        {
            if (opening_animation_timer <= 80)
            {
                OpeningAnimation();
                return;
            }

            DrawHUD();

            if (link_walk_animation_timer > 0)
                LinkWalkAnimation();

            Link.Tick();

            if (scroll_animation_timer > 500)
                DoorCode();

            CheckForWarp();
            Menu.Tick();

            if (menu_open)
                Program.can_pause = false;

            if (!menu_open)
            {
                if (dark_room_animation_timer == 0 && link_walk_animation_timer == 0)
                    Scroll();
                else
                    DarkeningAnimation();
            }
        }

        static void OpeningAnimation()
        {
            if (opening_animation_timer == 80)
            {
                Link.Show(true);
                can_open_menu = true;
                draw_hud_objects = true;
                opening_animation_timer++;
                Sound.PlaySong(Sound.Songs.DUNGEON);
                Program.can_pause = true;
                Menu.map_dot.shown = true;
                return;
            }

            can_open_menu = false;
            Link.can_move = false;
            Link.Show(false);
            draw_hud_objects = false;
            opening_animation_timer++;

            if (opening_animation_timer % 5 != 0)
                return;

            Textures.LoadPPUPage(Textures.PPUDataGroup.DUNGEON, current_screen, 0);
            int num_rows_to_erase = 16 - opening_animation_timer / 5;
            for (int i = 0; i < num_rows_to_erase; i++)
            {
                for (int j = 0; j < 22 * Textures.PPU_WIDTH; j += Textures.PPU_WIDTH)
                {
                    Textures.ppu[0x100 + j + i] = 0x24;
                    Textures.ppu[0x11f + j - i] = 0x24;
                }
            }
        }

        public static void LoadPalette(bool underground_room = false)
        {
            // the side-view rooms are gray, no matter the dungeon palette
            if (underground_room)
            {
                Palettes.LoadPaletteGroup(PaletteID.BG_2, Palettes.PaletteGroups.DUNGEON8_9);
                Palettes.LoadPaletteGroup(PaletteID.BG_3, Palettes.PaletteGroups.DUNGEON8_9);
                return;
            }

            Palettes.PaletteGroups chosen_palette;
            switch (current_dungeon + 1)
            {
                case 1:
                    chosen_palette = Palettes.PaletteGroups.DUNGEON1;
                    break;

                case 2:
                    chosen_palette = Palettes.PaletteGroups.DUNGEON2;
                    break;

                case 3:
                    chosen_palette = Palettes.PaletteGroups.DUNGEON3;
                    break;

                case 4 or 6:
                    chosen_palette = Palettes.PaletteGroups.DUNGEON4_6;
                    break;

                case 5 or 7:
                    chosen_palette = Palettes.PaletteGroups.DUNGEON5_7;
                    break;

                case 8 or 9:
                default:
                    chosen_palette = Palettes.PaletteGroups.DUNGEON8_9;
                    break;
            }
            Palettes.LoadPaletteGroup(PaletteID.BG_2, chosen_palette);
            Palettes.LoadPaletteGroup(PaletteID.BG_3, chosen_palette);
            Palettes.LoadPaletteGroup(PaletteID.SP_3, chosen_palette);

            // dungeons 2, 3, 5 and 9 have red water (lava)
            if ((current_dungeon + 1) is 2 or 3 or 5 or 9)
                Palettes.LoadPalette(3, 1, Color._16_RED_ORANGE);
            else
                Palettes.LoadPalette(3, 1, Color._12_SMEI_DARK_BLUE);
        }

        public static DoorType GetDoorType(byte room_id, Direction door_direction)
        {
            // door_types must be cleared before use
            if (door_types[(int)door_direction] != DoorType.NONE)
                return door_types[(int)door_direction];

            int connection_id = 2 * room_id;
            int connection_index;
            if (door_direction == Direction.LEFT)
                connection_id--;
            else if (door_direction == Direction.RIGHT)
                connection_id++;
            else if (door_direction == Direction.UP)
                connection_id -= 32;

            if (connection_id < 0 || connection_id >= connection_IDs.Length * 2)
                return DoorType.NONE;

            connection_index = connection_id;

            float connection_location = connection_id / 2f;
            byte bit_mask = 0xF0;
            if (connection_location % 1 != 0)
            {
                bit_mask >>= 4;
                connection_location -= 0.5f;
            }
            connection_id = connection_IDs[(int)connection_location] & bit_mask;
            if (bit_mask == 0xF0)
                connection_id >>= 4;

            if (connection_id >= 4 && connection_id <= 0xc && door_statuses[(int)door_direction])
                return DoorType.OPEN;

            switch (connection_id)
            {
                case 0:
                    return DoorType.NONE;
                case 1:
                    return DoorType.OPEN;
                case 2:
                    if (SaveLoad.GetOpenedKeyDoorsFlag((byte)Array.IndexOf(key_door_connections, (short)connection_index)))
                        return DoorType.OPEN;
                    else
                        return DoorType.KEY;
                case 3:
                    return DoorType.BOMBABLE;
                case 4:
                    if (door_direction == Direction.UP || door_direction == Direction.LEFT) // will likely have to flip conditions of a-b
                        return DoorType.CLOSED_PUSH;
                    else
                        return DoorType.OPEN;
                case 5:
                    if (door_direction == Direction.DOWN || door_direction == Direction.RIGHT)
                        return DoorType.CLOSED_ENEMY;
                    else
                        return DoorType.OPEN;
                case 6:
                    if (door_direction == Direction.UP || door_direction == Direction.LEFT)
                        return DoorType.CLOSED_ENEMY;
                    else
                        return DoorType.OPEN;
                case 7 or 8:
                    return DoorType.CLOSED_ENEMY;
                case 9:
                    if (door_direction == Direction.DOWN || door_direction == Direction.RIGHT)
                        return DoorType.CLOSED_PUSH;
                    else
                        return DoorType.OPEN;
                case 0xa:
                    if (door_direction == Direction.UP || door_direction == Direction.LEFT)
                        return DoorType.CLOSED_ALWAYS;
                    else
                        return DoorType.OPEN;
                case 0xb:
                    if (door_direction == Direction.DOWN || door_direction == Direction.RIGHT)
                        return DoorType.CLOSED_ALWAYS;
                    else
                        return DoorType.OPEN;
                case 0xc:
                    return DoorType.CLOSED_TRIFORCE;
                case 0xd:
                    return DoorType.WALK_THROUGH;
                case 0xe:
                    if (door_direction == Direction.UP || door_direction == Direction.LEFT)
                        return DoorType.WALK_THROUGH;
                    else
                        return DoorType.NONE;
                case 0xf:
                    if (door_direction == Direction.DOWN || door_direction == Direction.RIGHT)
                        return DoorType.WALK_THROUGH;
                    else
                        return DoorType.NONE;
                default:
                    return DoorType.NONE;
            }
        }

        // get connection ID of connection between two rooms, found with direction relative to room id
        public static int getConnectionID(byte room_id, Direction door_direction)
        {
            int connection_id = 2 * room_id;
            if (door_direction == Direction.LEFT)
                connection_id--;
            else if (door_direction == Direction.RIGHT)
                connection_id++;
            else if (door_direction == Direction.UP)
                connection_id -= 32;

            if (connection_id < 0 || connection_id >= connection_IDs.Length * 2)
                return 0;
            else
                return connection_id;
        }

        static void Scroll()
        {
            // no scrolling in the side view rooms!
            if (room_list[current_screen] >= 0x2a)
                return;

            if (scroll_animation_timer > 500)
            {
                if (Link.y < 64 && (Control.IsHeld(Buttons.UP)))
                {
                    scroll_destination = (byte)(current_screen - 16);
                    scroll_animation_timer = 0;
                    scroll_direction = Direction.UP;
                }
                else if (Link.y > 223 && (Control.IsHeld(Buttons.DOWN)))
                {
                    scroll_destination = (byte)(current_screen + 16);
                    scroll_animation_timer = 0;
                    scroll_direction = Direction.DOWN;
                }
                else if (Link.x < 1 && Control.IsHeld(Buttons.LEFT))
                {
                    scroll_destination = (byte)(current_screen - 1);
                    scroll_animation_timer = 0;
                    scroll_direction = Direction.LEFT;
                }
                else if (Link.x > 239 && (Control.IsHeld(Buttons.RIGHT) || tornado_out))
                {
                    scroll_destination = (byte)(current_screen + 1);
                    scroll_animation_timer = 0;
                    scroll_direction = Direction.RIGHT;
                }

                return;
            }

            if (scroll_animation_timer == 0)
            {
                Link.Show(false);
                can_open_menu = false;
                UnloadSpritesRoomTransition();
                ResetLinkPowerUps();

                if (room_list[current_screen] == 1 && scroll_direction == Direction.DOWN)
                {
                    Textures.LoadPPUPage(Textures.PPUDataGroup.OTHER, 1, 0);
                    Program.gamemode = Program.Gamemode.OVERWORLD;
                    OverworldCode.black_square_stairs_return_flag = true;
                    Link.Show(false);
                    Link.SetPos(-16, -16);
                    OverworldCode.Init();
                    OverworldCode.current_screen = OverworldCode.return_screen;
                    scroll_animation_timer = 1000;
                    return;
                }

                LinkWalkAnimation();
                if (GetRoomDarkness(scroll_destination) && !is_dark)
                {
                    is_dark = true;
                    dark_room_animation_timer = 22;
                }

                if (scroll_direction == Direction.DOWN)
                {
                    Textures.LoadPPUPage(Textures.PPUDataGroup.DUNGEON, scroll_destination, 1);
                }
                else if (scroll_direction == Direction.UP)
                {
                    Textures.LoadPPUPage(Textures.PPUDataGroup.DUNGEON, current_screen, 1);
                    Textures.LoadPPUPage(Textures.PPUDataGroup.DUNGEON, scroll_destination, 0);
                    y_scroll = 176;
                    Link.SetPos(new_y: 240);
                }
                else
                {
                    Textures.LoadPPUPage(Textures.PPUDataGroup.DUNGEON, scroll_destination, 2);
                }
                Link.can_move = false;
            }

            if (scroll_direction == Direction.UP || scroll_direction == Direction.DOWN)
            {
                if ((Program.gTimer % 4) == 0)
                {
                    if (scroll_direction == Direction.UP)
                    {
                        y_scroll -= 8;
                        if (Program.gTimer % 3 == 0)
                            Link.SetPos(new_y: Link.y - 2);
                    }
                    else
                    {
                        y_scroll += 8;
                        if (Program.gTimer % 3 == 0)
                            Link.SetPos(new_y: Link.y + 2);
                        if (Link.y < 65)
                            Link.SetPos(new_y: 65);
                    }
                }

                if (scroll_animation_timer == 88)
                {
                    EndScroll();
                }
            }
            else
            {
                if (scroll_direction == Direction.LEFT)
                {
                    x_scroll -= 2;
                    if (Program.gTimer % 4 == 0)
                        Link.SetPos(new_x: Link.x - 2);
                    if (Link.x > 239)
                        Link.SetPos(new_x: 239);
                }
                else
                {
                    x_scroll += 2;
                    if (Program.gTimer % 2 == 0)
                        Link.SetPos(new_x: Link.x + 1);
                    if (Link.x < 1)
                        Link.SetPos(new_x: 1);
                }

                if (scroll_animation_timer == 128)
                {
                    EndScroll();
                }
            }

            if ((int)scroll_direction < 2)
                Link.current_action = (Link.Action)scroll_direction + 2;
            else
                Link.current_action = (Link.Action)scroll_direction - 2;

            scroll_animation_timer++;
            Link.animation_timer++;

            void EndScroll()
            {
                Link.Show(true);
                x_scroll = 0;
                y_scroll = 0;
                Textures.LoadPPUPage(Textures.PPUDataGroup.DUNGEON, scroll_destination, 0);
                current_screen = scroll_destination;
                if (scroll_direction == Direction.UP)
                    Link.SetPos(new_y: 223);
                else if (scroll_direction == Direction.DOWN)
                    Link.SetPos(new_y: 65);
                else if (scroll_direction == Direction.LEFT)
                    Link.SetPos(new_x: 239);
                else
                    Link.SetPos(new_x: 1);
                scroll_animation_timer += 1000;
                can_open_menu = true;

                LinkWalkAnimation();
            }
        }

        // plays the room darkening animation, either forwards or backwards depending on dark room enter or exit
        static void DarkeningAnimation()
        {
            // often for this animation you're just lowering a palette color one level down, so this function takes a list of indices and lowers them by 16
            // this function also makes it so we don't have to care about what color the dungeon uses
            static void reducePalettes(byte[] to_reduce)
            {
                foreach (byte i in to_reduce)
                    if (Palettes.active_palette_list[i] > 15)
                        Palettes.active_palette_list[i] -= 16;
            }

            if (dark_room_animation_timer > 0)
                dark_room_animation_timer--;
            else if (dark_room_animation_timer < 0)
                dark_room_animation_timer++;

            byte[] list_21 = { 10, 11, 13, 14, 15 };
            byte[] list_11 = { 11, 13, 15 };
            switch (dark_room_animation_timer)
            {
                case 21:
                    reducePalettes(list_21);
                    break;

                case 11:
                    reducePalettes(list_11);
                    Palettes.LoadPalette(2, 1, Color._0F_BLACK);
                    break;

                case 1:
                    Palettes.LoadPalette(2, 2, Color._0F_BLACK);
                    Palettes.LoadPalette(3, 1, Color._0F_BLACK);
                    Palettes.LoadPalette(3, 2, Color._0F_BLACK);
                    Palettes.LoadPalette(3, 3, Color._0F_BLACK);
                    break;

                case -21:
                    LoadPalette();
                    reducePalettes(list_21);
                    reducePalettes(list_11);
                    Palettes.LoadPalette(2, 1, Color._0F_BLACK);
                    break;

                case -11:
                    LoadPalette();
                    reducePalettes(list_21);
                    break;

                case -1:
                    LoadPalette();
                    break;
            }
        }

        // spawn ennemies and or bosses in a simillar way to overworld
        static void SpawnEnemies()
        {
            uint enemies = dungeon_enemy_list[current_screen];
            // the first 16 bits of the enemy code being FFFF is a signal to load the boss of that room instead, whose id is the 16 next bits.
            // yes, this means a dungeon room can't have 8 of the enemy of ID 15, because 0xFFFFFFFF spawns ganon.
            // otherwise you can just insert the non-15 id enemy into one of the first 4 slots.
            if ((enemies & 0xFFFF0000) == 0xFFFF0000)
            {
                // if boss killed, it's gone
                if (SaveLoad.GetBossKillsFlag((byte)Array.IndexOf(rooms_with_boses, current_screen)))
                    return;

                switch ((Bosses)(enemies & 0xFFFF))
                {
                    case Bosses.AQUAMENTUS:
                        //new Aquamentus();
                        break;
                    case Bosses.DODONGO_TRIPLE:
                        //new Dodongo();
                        //new Dodongo();
                        //new Dodongo();
                    case Bosses.DODONGO:
                        //new Dodongo();
                        break;
                    case Bosses.MANHANDLA:
                        //new Manhandla();
                        break;
                    case Bosses.GLEEOK:
                        //new Gleeok(2);
                        break;
                    case Bosses.GLEEOK_TRIPLE:
                        //new Gleeok(3);
                        break;
                    case Bosses.GLEEOK_QUADRUPLE:
                        //new Gleeok(4);
                        break;
                    case Bosses.DIGDOGGER:
                        //new Digdogger(false);
                        break;
                    case Bosses.DIGDOGGER_TRIPLE:
                        //new Digdogger(true);
                        break;
                    case Bosses.GOHMA:
                        //new Gohma(false);
                        break;
                    case Bosses.GOHMA_HARDER:
                        //new Gohma(true);
                        break;
                    case Bosses.PATRA:
                        //new Patra();
                        break;
                    case Bosses.MOLDORM:
                        //new Moldorm();
                        break;
                    case Bosses.GANON:
                        //new Ganon();
                        break;
                }

                return;
            }

            uint enemy_selector = 0xF0000000;
            for (int i = 0; i < 8; i++)
            {
                uint enemy_id = (enemies & enemy_selector) >> ((7 - i) * 4);
                if (enemy_id < 8)
                {
                    switch ((DungeonEnemies)enemy_id)
                    {
                        case DungeonEnemies.NONE:
                            break;
                        case DungeonEnemies.GEL:
                            //new Gel();
                            break;
                        case DungeonEnemies.BUBBLE:
                            //new Bubble(Bubble.NORMAL);
                            break;
                        case DungeonEnemies.BUBBLE_BLUE:
                            //new Bubble(Bubble.BLUE);
                            break;
                        case DungeonEnemies.BUBBLE_RED:
                            //new Bubble(Bubble.RED);
                            break;
                        case DungeonEnemies.KEESE:
                            //new Keese();
                            break;
                        case DungeonEnemies.NPC:
                            // :/
                            break;
                        case DungeonEnemies.RAZOR_TRAP:
                            //new RazorTrap();
                            break;
                    }

                    continue;
                }

                if (current_dungeon is 1 or 2 or 7)
                {
                    switch (enemy_id)
                    {
                        case 0:
                            break;
                    }
                }
                else if (current_dungeon is 3 or 5 or 8)
                {
                    switch (enemy_id)
                    {
                        case 0:
                            break;
                    }
                }
                else
                {
                    switch (enemy_id)
                    {
                        case 0:
                            break;
                    }
                }

                enemy_selector >>= 4;
            }
        }

        static void SpawnItems()
        {
            if (room_list[current_screen] == 11 && !SaveLoad.GetTriforceFlag(current_dungeon))
            {
                new TriforcePieceSprite();
            }
            if (current_screen is 25 or 128 or 165 && !SaveLoad.GetDungeonVisitedRoomFlag(current_screen))
            {
                byte[] rupee_pile_x_positions = { 124, 116, 132, 100, 116, 132, 148, 116, 132, 124 };
                byte[] rupee_pile_y_positions = { 112, 128, 128, 144, 144, 144, 144, 160, 160, 176 };
                for (int i = 0; i < rupee_pile_x_positions.Length; i++)
                    new RupySprite(rupee_pile_x_positions[i], rupee_pile_y_positions[i], false, false);
            }
        }

        static void UnloadSpritesRoomTransition()
        {
            for (int i = 0; i < sprites.Count; i++)
            {
                if (sprites[i].unload_during_transition)
                {
                    sprites.RemoveAt(i);
                    i--;
                }
            }
        }

        static void ResetLinkPowerUps()
        {
            fire_out = 0;
            boomerang_out = false;
            arrow_out = false;
            magic_wave_out = false;
            bait_out = false;
            bomb_out = false;
            if (Link.shown)
                tornado_out = false;
            sword_proj_out = false;
            Link.clock_flash = false;
            Link.iframes_timer = 0;
            Link.self.palette_index = 4;
            Link.counterpart.palette_index = 4;
        }

        public static void DrawDoors(byte room, byte screen_index, bool redraw = false)
        {
            int screen_index_offset = screen_index * 0x3c0;

            byte[] door_data = {
                0xf6,0xf6,0xf6,0xf6,0x78,0x78,0x78,0x78,0x79,0x24,0x24,0x7b,0x7a,0x77,0x75,0x7c,
                0x7e,0x76,0x74,0x80,0x7f,0x24,0x24,0x81,0x7d,0x7d,0x7d,0x7d,0xf6,0xf6,0xf6,0xf6,
                0xf6,0x82,0x83,0x85,0xf6,0x82,0x24,0x76,0xf6,0x82,0x24,0x77,0xf6,0x82,0x84,0x86,
                0x88,0x8a,0x87,0xf6,0x74,0x24,0x87,0xf6,0x75,0x24,0x87,0xf6,0x89,0x8b,0x87,0xf6
            };
            short[] door_ppu_locations = { 0x10e, 0x34e, 0x220, 0x23c };
            byte[] ppu_location_differences = { 65, 1, 34, 32 };

            if (screen_index == 1)
                screen_index_offset -= 256;

            if (!redraw)
            {
                Array.Clear(door_types);
                Array.Clear(door_statuses);
                foreach (byte i in door_metatiles)
                    meta_tiles[i].special = false;
            }

            for (int i = 0; i < door_types.Length; i++)
            {
                if (!redraw)
                    door_types[i] = GetDoorType(room, (Direction)i);

                if (i == 0 && room - 16 >= 0) // makes it so entrances only have open door on bottom, and not top of room below
                    if (room_list[room - 16] == 1)
                        continue;

                if (door_types[i] == DoorType.NONE)
                {
                    continue;
                }
                
                if (door_types[i] is DoorType.BOMBABLE or DoorType.WALK_THROUGH)
                {
                    //meta_tiles[door_metatiles[i]].special = true;
                    if (door_types[i] == DoorType.BOMBABLE &&
                        SaveLoad.GetBombedHoleFlag((byte)Array.IndexOf(bombable_connections, (short)getConnectionID(room, (Direction)i))))
                    {
                        byte[] bomb_hole_texture_data = {
                            0x8c, 0x8d, 0x24, 0x24,
                            0x24, 0x24, 0x8e, 0x8f,
                            0x90, 0x24, 0x91, 0x24,
                            0x24, 0x92, 0x24, 0x93
                        };

                        int ppu_location2 = door_ppu_locations[i] + ppu_location_differences[i];
                        Textures.ppu[ppu_location2 + screen_index_offset] = bomb_hole_texture_data[i * 4];
                        Textures.ppu[ppu_location2 + 1 + screen_index_offset] = bomb_hole_texture_data[i * 4 + 1];
                        Textures.ppu[ppu_location2 + 32 + screen_index_offset] = bomb_hole_texture_data[i * 4 + 2];
                        Textures.ppu[ppu_location2 + 33 + screen_index_offset] = bomb_hole_texture_data[i * 4 + 3];
                        MakeDoorWalkable(i);
                    }
                    else if (door_types[i] == DoorType.WALK_THROUGH)
                    {
                        MakeDoorWalkable(i, true);
                    }

                    continue;
                }

                for (int height = 0; height < 4; height++)
                    for (int width = 0; width < 4; width++)
                        Textures.ppu[door_ppu_locations[i] + height * 32 + width + screen_index_offset] = door_data[height * 4 + width + i * 16];

                byte[] texture_locations = { 0x98, 0x9c, 0xa0, 0xa4, 0xa8, 0xa8, 0xac, 0xac };
                int ppu_location = door_ppu_locations[i] + ppu_location_differences[i];


                if (door_types[i] == DoorType.OPEN)
                {
                    MakeDoorWalkable(i);
                    continue;
                }
                else if (door_types[i] == DoorType.KEY)
                {
                    Textures.ppu[ppu_location + screen_index_offset] = texture_locations[i];
                    Textures.ppu[ppu_location + 1 + screen_index_offset] = (byte)(texture_locations[i] + 2);
                    Textures.ppu[ppu_location + 32 + screen_index_offset] = (byte)(texture_locations[i] + 1);
                    Textures.ppu[ppu_location + 33 + screen_index_offset] = (byte)(texture_locations[i] + 3);
                    MakeDoorWalkable(i, locked_key_door: true);
                }
                else
                {
                    Textures.ppu[ppu_location + screen_index_offset] = texture_locations[i + 4];
                    Textures.ppu[ppu_location + 1 + screen_index_offset] = (byte)(texture_locations[i + 4] + 2);
                    Textures.ppu[ppu_location + 32 + screen_index_offset] = (byte)(texture_locations[i + 4] + 1);
                    Textures.ppu[ppu_location + 33 + screen_index_offset] = (byte)(texture_locations[i + 4] + 3);
                }
            }

            void MakeDoorWalkable(int i, bool walk_through = false, bool locked_key_door = false)
            {
                int add = 0;
                for (int loop = 0; loop < 2; loop++)
                {
                    if (loop != 0)
                    {
                        if (i == 0)
                            add = -16;
                        else if (i == 1)
                            add = 16;
                        else if (i == 2)
                            add = -1;
                        else
                            add = 1;
                    }

                    if (walk_through)
                    {
                        meta_tiles[door_metatiles[i]].tile_index = 13;
                        return;
                    }
                    else if (locked_key_door)
                    {
                        meta_tiles[door_metatiles[i]].special = true;
                        meta_tiles[door_metatiles[i]].tile_index = 1;
                        return;
                    }

                    if (i < 2)
                    {
                        meta_tiles[door_metatiles[i] + add].tile_index = 11;
                        meta_tiles[door_metatiles[i] + 1 + add].tile_index = 12;
                    }
                    else
                    {
                        meta_tiles[door_metatiles[i] + add].tile_index = 10;
                    }
                }
            }
        }

        public static void LinkWalkAnimation()
        {
            const byte anim_len = 10;
            if (link_walk_animation_timer == 0)
                link_walk_animation_timer = anim_len;

            if (link_walk_animation_timer == anim_len)
            {
                if (is_dark && !GetRoomDarkness(current_screen))
                {
                    dark_room_animation_timer = -22;
                    is_dark = false;                   
                }
                Link.can_move = false;
                UnloadSpritesRoomTransition();
                if (dark_room_animation_timer != 0)
                {
                    DarkeningAnimation();
                    return;
                }
            }
            else if (link_walk_animation_timer == 1)
            {
                Link.can_move = true;
                SpawnEnemies();
                SpawnItems();
                SaveLoad.SetDungeonVisitedRoomFlag(current_screen, true);
            }

            Link.animation_timer++;
            int x_add = 0, y_add = 0;

            if (Link.facing_direction == Direction.UP)
                y_add = -1;
            else if (Link.facing_direction == Direction.DOWN)
                y_add = 1;
            else if (Link.facing_direction == Direction.LEFT)
                x_add = -1;
            else
                x_add = 1;

            Link.SetPos(Link.x + x_add, Link.y + y_add);
            if (Program.gTimer % 2 == 0)
                Link.SetPos(Link.x + x_add, Link.y + y_add);

            link_walk_animation_timer--;
        }

        static void DoorCode()
        {
            bool redraw = false;
            for (int i = 0; i < door_types.Length; i++)
            {
                switch (door_types[i])
                {
                    case DoorType.KEY:
                        break;
                    case DoorType.BOMBABLE:
                        if (door_statuses[i])
                        {
                            byte index = (byte)Array.IndexOf(bombable_connections, (short)getConnectionID(current_screen, (Direction)i));
                            if (!SaveLoad.GetBombedHoleFlag(index))
                            {
                                SaveLoad.SetBombedHoleFlag(index, true);
                                door_types[i] = DoorType.BOMBABLE;
                                redraw = true;
                            }
                        }
                        break;
                    case DoorType.CLOSED_ENEMY:
                        if (!door_statuses[i])
                        {
                            if (nb_enemies_alive <= 0)
                            {
                                door_statuses[i] = true;
                                redraw = true;
                            }
                        }
                        else
                        {
                            if (nb_enemies_alive > 0)
                            {
                                door_statuses[i] = false;
                                redraw = true;
                            }
                        }
                        break;
                    case DoorType.CLOSED_PUSH:
                        if (block_push_flag)
                        {
                            door_types[i] = DoorType.OPEN;
                            redraw = true;
                        }
                        break;
                    case DoorType.CLOSED_TRIFORCE:
                        if (SaveLoad.triforce_of_power)
                        {
                            door_types[i] = DoorType.OPEN;
                            redraw = true;
                        }
                        break;
                    case DoorType.WALK_THROUGH:
                        break;
                }
            }

            if (redraw)
                DrawDoors(current_screen, 0, true);
        }

        static void CheckForWarp()
        {
            if (warp_flag)
            {
                warp_flag = false;
                Link.current_action = Link.Action.WALKING_DOWN;
            }
        }

        static byte GetMetatileIndexFromGiftLocationID(byte id)
        {
            byte gift_metatile = 0;
            switch (current_dungeon)
            {
                case 0:
                    if (id == 0)
                        gift_metatile = 138;
                    else if (id == 1)
                        gift_metatile = 56;
                    else
                        gift_metatile = 92;
                    break;
                case 2:
                    if (id == 0)
                        gift_metatile = 88;
                    else
                        gift_metatile = 34;
                    break;
                case 3:
                    if (id == 0)
                        gift_metatile = 89;
                    else if (id == 1)
                        gift_metatile = 72;
                    else
                        gift_metatile = 141;
                    break;
                case 4:
                    if (id == 0)
                        gift_metatile = 104;
                    else
                        gift_metatile = 56;
                    break;
                default:
                    if (id == 0)
                        gift_metatile = 88;
                    else if (id == 1)
                        gift_metatile = 45;
                    else
                        gift_metatile = 130;
                    break;
            }
            return gift_metatile;
        }

        public static bool GetRoomDarkness(byte room)
        {
            int value = dark_rooms[room >> 4] & (1 << (15 - (room % 16)));
            return value > 0;
        }
    }
}