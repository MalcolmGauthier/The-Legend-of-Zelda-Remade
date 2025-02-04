using static The_Legend_of_Zelda.Rendering.Screen;
using static The_Legend_of_Zelda.Gameplay.Program;
using The_Legend_of_Zelda.Sprites;
using The_Legend_of_Zelda.Rendering;

namespace The_Legend_of_Zelda.Gameplay
{
    public sealed class DungeonCode : GameplayCode
    {
        public enum RoomType
        {
            EMPTY,
            ENTRANCE,
            DOUBLE_2X3,
            SINGLE_2X3,
            FOUR_BLOCKS_CLOSE,
            FOUR_BLOCKS_FAR,
            SINGLE_PUSH_BLOCK,
            FIVE_2X1,
            GRID,
            WATER_MAZE,
            AQUAMENTUS_ARENA,
            TRIFORCE,
            CENTRAL_STAIRS,
            WATER_POKEBALL,
            ARENA,
            SAND,
            RIGHT_STAIRS,
            DIAGONALS,
            HORIZONTAL_BARRIER,
            SIDEWAYS_U,
            DOUBLE_PUSH_BLOCK,
            MAZE,
            HORIZONTAL_WATER_BAR,
            WATER_MOAT,
            GOHMA_ARENA,
            GLEEOK_ARENA,
            WATER_ISLAND,
            SPIKE_TRAP,
            VERTICAL_WATER_BAR,
            TRIPLE_HORIZONTAL_BAR,
            WATER_RING,
            STATUE_DUO,
            BOX,
            DOUBLE_HORIZONTAL_WATER_BAR,
            SPIRAL,
            GANON_ARENA,
            ZELDA,
            VERTICAL_BARRIER,
            VERTICAL_BARS, //2q only
            FORCE_TURN, //2q only
            DIAGONAL_WATER_BARRIER, //2q only
            VOID,
            SIDESCROLL_ITEM,
            SIDESCROLL_SHORTCUT,
        }

        public enum DoorType
        {
            NONE,
            OPEN,
            KEY,
            BOMBABLE,
            CLOSED_PUSH_DR,
            CLOSED_ENEMY_UL,
            CLOSED_ENEMY_DR,
            CLOSED_ENEMY_UD,
            CLOSED_ENEMY_LR,
            CLOSED_PUSH_UL,
            CLOSED_ALWAYS_UL,
            CLOSED_ALWAYS_DR,
            CLOSED_TRIFORCE,
            WALK_THROUGH,
            WALK_THROUGH_UL,
            WALK_THROUGH_DR
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
            BLADE_TRAP,
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
            LANMOLA,
            LANMOLA_HARDER,
            GANON,
        }

        const int NUMBER_OF_DOORS = 4;
        const int DARKENING_EFFECT_ACTIVATE = 22;
        const int DARKENING_EFFECT_REMOVE = -22;
        // if you don't care about which kind of closed door it is, use this
        public const DoorType DOOR_GENERIC_CLOSED = DoorType.CLOSED_ENEMY_UL;

        public byte current_dungeon { get; private set; }
        byte link_walk_animation_timer = 0;

        public int nb_enemies_alive = 0;
        int dark_room_animation_timer = 0;
        int frames_since_link_can_walk = 0;

        public bool warp_flag = false;
        public bool block_push_flag = false;
        public bool is_dark { get; private set; } = false;

        public DoorType[] doors = new DoorType[NUMBER_OF_DOORS];
        public bool[] door_statuses = new bool[NUMBER_OF_DOORS];
        public FlickeringSprite compass_dot = new FlickeringSprite(0x3e, 5, 0, 0, 16, 0x3e, second_palette_index: 6);

        public readonly byte[] room_list = {
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
        public readonly ushort[] dark_rooms = {
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
        //TODO: change data for bosses: lanmola
        public readonly uint[] dungeon_enemy_list = {
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
            0x60000000, 0x22555aaa, 0x55550000, 0xaaaaaa00, 0xcccccccc, 0xffff000a, 0x99922aab, 0x55550000, 0xbbcc0000, 0x7aaaa000, 0xffff000f, 0x60000000, 0xbbbbbb00, 0xbbcc2aaa, 0xcccccc00, 0x222bbcc0,
            0xffff0002, 0x999aaa00, 0x88888888, 0x60000000, 0xaaa00000, 0x55cc9900, 0xbbbbbb00, 0xcccccccc, 0x88888000, 0xaaaaaa00, 0xffff000b, 0xbbccaaa2, 0xbbccc000, 0xdd000000, 0x2288aa00, 0xbbccc000,
            0x75555000, 0xaaaaa000, 0x55555555, 0xaaaaaa00, 0xffff0007, 0x88888888, 0xffff0003, 0x55550000, 0x55550000, 0xffff000b, 0x55555555, 0x88888000, 0x2aaabbcc, 0x222bbccc, 0x60000000, 0x55550000,
            0xccccc000, 0x00000000, 0xffff000c, 0x55550000, 0x22aab999, 0xffff0003, 0x00000000, 0x22288555, 0x55550000, 0x00000000, 0x55550000, 0xbbccc222, 0xee000000, 0x55550000, 0x00000000, 0x55550000
        };
        readonly (byte x, byte y)[] compass_coords =
        // x,y coordinates within the minimap of the dungeon on where the compass dot should be
        // where 1unit = 1 blue square over (which is twice the distance horizontally)
        // (0,0) is the blue square (theoretically) located just under the L of LEVEL, and (x+,y+) is going down right from there
        {
            (6, 3),
            (3, 0),
            (5, 3),
            (5, 0),
            (2, 1),
            (5, 0),
            (4, 2),
            (2, 2),
            (2, 3)
        };
        // for save file flags
        public readonly byte[] rooms_with_boses = {
            61, 54, 6, 69, 24, 27, 94, 44, 16, 20, 131, 132, 148, 162, 177, 178, 208, 228, 242, 245, 230, 197, 166, 150, 233, 218, 169, 175, 158, 202
        };
        public readonly short[] bombable_connections = {
            3, 5, 18, 45, 49, 50, 51, 58, 61, 65, 78, 110, 137, 142, 150, 152, 167, 174, 217, 219, 256, 257,
            265, 279, 283, 286, 291, 293, 299, 304, 314, 318, 323, 331, 332, 339, 343, 346, 349, 350, 355, 359,
            360, 369, 375, 381, 406, 410, 418, 420, 428, 441, 442, 443, 449
        };
        public readonly short[] key_door_connections = {
            2, 17, 27, 28, 34, 40, 76, 80, 82, 85, 86, 89, 93, 102, 113, 122, 133, 135, 148, 151, 163, 173, 205,
            211, 214, 221, 225, 262, 275, 284, 289, 295, 300, 308, 309, 310, 313, 320, 341, 342, 347, 348, 368,
            384, 395, 400, 429, 438, 469, 470, 503
        };
        // for Textures.LoadPPUPage
        public readonly byte[] rooms_with_palette_3 = {
            9, 17, 18, 30, 33, 37, 40, 41, 43, 46, 47, 49, 52, 56, 57, 59, 63, 72, 78, 79, 80, 93, 95, 110, 113, 116, 117, 121, 123, 126,
            132, 137, 144, 150, 157, 171, 172, 177, 182, 187, 188, 193, 195, 200, 202, 210, 214, 215, 216, 219, 222, 223, 225, 229, 237, 241, 245, 246, 247
        };
        public readonly byte[] rooms_with_palette_1 = {
            54, 69, 131
        };
        // doors...
        public readonly byte[] connection_IDs = {
#region documentation
            //  0 - none
            //  1 - open
            //  2 - key
            //  3 - bombable
            //  4 - blocked: needs push down/right
            //  5 - blocked: needs screen clear up/left
            //  6 - blocked: needs screen clear down/right
            //  7 - blocked: needs screen clear up & down
            //  8 - blocked: needs screen clear left & right
            //  9 - blocked: needs push up/left
            //  a - blocked: permanent up/left
            //  b - blocked: permanent down/right
            //  c - blocked: needs triforce
            //  d - walk-through wall
            //  e - one way walk-through wall up/left
            //  f - one way walk-through wall down/right
            //
            //  IF ROOM NEEDS CLEAR, BLOCK DOORS THAT
            //  LEAD TO PUSH ROOMS AS CLEAR DOORS
            //  order = bottom, right for that room
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

        // dungeon is 0 indexed. 0 is level 1, etc.
        public void Init(byte dungeon)
        {
            ReadOnlySpan<byte> starting_screens = new byte[]{
                123, 117, 116, 121, 126, 113, 241, 246, 254
            };

            // reset link
            Link.SetPos(120, 223);
            Link.self.palette_index = (byte)PaletteID.SP_0;
            Link.counterpart.palette_index = (byte)PaletteID.SP_0;
            Link.current_action = LinkAction.WALKING_UP;
            DC.UnloadSpritesRoomTransition();
            Link.knockback_timer = 0;
            Link.iframes_timer = 0;
            Link.facing_direction = Direction.UP;
            Link.self.dungeon_wall_mask = true;
            Link.counterpart.dungeon_wall_mask = true;
            x_scroll = 0;
            y_scroll = 0;

            Menu.InitHUD();
            // set number in hud "LEVEL-X"
            Textures.ppu[0x48] = (byte)(dungeon + 1);
            // THE COMPASS DOT DOES NOT SHOW UP UNLESS IT'S MODIFIED AFTER CONSTRUCTION???? WHY
            //compass_dot = new FlickeringSprite(0x3e, 5, 0, 0, 16, 0x3e, second_palette_index: 6);
            compass_dot.UpdateTexture(true);
            compass_dot.x = 10 + compass_coords[dungeon].x * 8;
            compass_dot.y = 24 + compass_coords[dungeon].y * 4;
            Menu.map_dot.shown = false;

            current_dungeon = dungeon;
            current_screen = starting_screens[current_dungeon];

            if (SaveLoad.GetMapFlag(current_dungeon))
            {
                Menu.DrawHudMap();
            }

            LoadPalette();
            // i once had an enemy knock me into a dungeon staircase and made me keep my flash state, so i added this
            Palettes.LoadPaletteGroup(PaletteID.SP_0, Palettes.PaletteGroups.GREEN_LINK_HUDSPR1);
            if (SaveLoad.red_ring)
                Palettes.LoadPalette(PaletteID.SP_0, 1, Color._16_RED_ORANGE);
            else if (SaveLoad.blue_ring)
                Palettes.LoadPalette(PaletteID.SP_0, 1, Color._32_LIGHTER_INDIGO);
            // ¯\_(ツ)_/¯
            if (current_dungeon == 2)
                Palettes.LoadPalette(PaletteID.SP_0, 2, Color._37_BEIGE);

            Textures.LoadNewRomData(Textures.ROMData.CHR_DUNGEON);
            UpdateBGMusic(current_dungeon == 8 ? Sound.Songs.DEATH_MOUNTAIN : Sound.Songs.DUNGEON, 0);

            if (current_dungeon is 0 or 1 or 6)
                Textures.LoadNewRomData(Textures.ROMData.SPR_DUNGEON_127);
            else if (current_dungeon is 2 or 4 or 7)
                Textures.LoadNewRomData(Textures.ROMData.SPR_DUNGEON_358);
            else if (current_dungeon is 3 or 5 or 8)
                Textures.LoadNewRomData(Textures.ROMData.SPR_DUNGEON_469);

            //if (current_dungeon is 0 or 1 or 4 or 6)
            //    Textures.LoadNewRomData(Textures.ROMData.SPR_DUNGEON_BOSS_1257);
            //else if (current_dungeon is 2 or 3 or 5 or 7)
            //    Textures.LoadNewRomData(Textures.ROMData.SPR_DUNGEON_BOSS_3468);
            //else if (current_dungeon == 8)
            //    Textures.LoadNewRomData(Textures.ROMData.SPR_DUNGEON_BOSS_9);

            for (int i = 0; i < meta_tiles.Length; i++)
            {
                meta_tiles[i].tile_index_D = DungeonMetatile.WALL;
            }

            LinkWalkAnimation();
            opening_animation_timer = 0;
            OC.black_square_stairs_flag = false;
            OC.stair_warp_flag = false;
        }

        protected override void SpecificCode()
        {
            if (link_walk_animation_timer > 0)
                LinkWalkAnimation();

            if (ScrollingDone() && link_walk_animation_timer == 0)
                DoorCode();

            CheckForWarp();

            if (!Menu.menu_open)
            {
                if (dark_room_animation_timer == 0 && link_walk_animation_timer == 0)
                    Scroll(false);
                else
                    DarkeningAnimation();
            }
        }

        public void LoadPalette(bool underground_room = false)
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

            // dungeons 3, 5, 6 and 9 have red water (lava?)
            if (current_dungeon is 2 or 4 or 5 or 8)
                Palettes.LoadPalette(PaletteID.BG_3, 1, Color._16_RED_ORANGE);
            else
                Palettes.LoadPalette(PaletteID.BG_3, 1, Color._12_SMEI_DARK_BLUE);
        }

        protected override bool SpecificScrollCode(bool scroll_finished)
        {
            if (scroll_finished)
            {
                LinkWalkAnimation();
                nb_enemies_alive = 0;
                // static variables that need to be reset upon unload
                WizrobeOrange.ResetData();
                return false;
            }

            // no scrolling in the side view rooms!
            if (room_list[current_screen] >= 0x2a)
            {
                return false;
            }

            // check if dungeon entrance. load overworld and return if going down
            if (room_list[current_screen] == 1 && scroll_direction == Direction.DOWN)
            {
                Textures.LoadPPUPage(Textures.PPUDataGroup.OTHER, Textures.OtherPPUPages.EMPTY, 0);
                gamemode = Gamemode.OVERWORLD;
                Link.Show(false);
                Link.SetPos(-16, -16);
                sprites.Remove(compass_dot);
                OC.Init();
                OC.black_square_stairs_return_flag = true;
                OC.current_screen = OC.return_screen;
                return false;
            }

            block_push_flag = false;
            LinkWalkAnimation();
            if (GetRoomDarkness(scroll_destination) && !is_dark)
            {
                DarkeningAnimationActivate();
            }

            return true;
        }


        // plays the room darkening animation, either forwards or backwards depending on dark room enter or exit
        void DarkeningAnimation()
        {
            // often for this animation you're just lowering a palette color one level down, so this function takes a list of indices and lowers them by 16
            // this function also makes it so we don't have to care about what color the dungeon uses
            void reducePalettes(byte[] to_reduce)
            {
                foreach (byte i in to_reduce)
                    if (Palettes.active_palette_list[i] > 15)
                        Palettes.active_palette_list[i] -= 16;
            }

            if (dark_room_animation_timer == 0)
                return;

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
                    Palettes.LoadPalette(PaletteID.BG_2, 1, Color._0F_BLACK);
                    break;

                case 1:
                    Palettes.LoadPalette(PaletteID.BG_2, 2, Color._0F_BLACK);
                    Palettes.LoadPalette(PaletteID.BG_3, 1, Color._0F_BLACK);
                    Palettes.LoadPalette(PaletteID.BG_3, 2, Color._0F_BLACK);
                    Palettes.LoadPalette(PaletteID.BG_3, 3, Color._0F_BLACK);
                    break;

                case 0:
                    return;

                case -21:
                    LoadPalette();
                    reducePalettes(list_21);
                    reducePalettes(list_11);
                    Palettes.LoadPalette(PaletteID.BG_2, 1, Color._0F_BLACK);
                    Link.can_move = false;
                    break;

                case -11:
                    LoadPalette();
                    reducePalettes(list_21);
                    break;

                case -1:
                    LoadPalette();
                    if (Menu.fire_out > 0)
                        Link.can_move = true;
                    return;
            }
        }
        public void DarkeningAnimationActivate()
        {
            dark_room_animation_timer = DARKENING_EFFECT_ACTIVATE;
            is_dark = true;
        }
        public void DarkeningAnimationDisable()
        {
            dark_room_animation_timer = DARKENING_EFFECT_REMOVE;
            is_dark = false;
        }

        public bool GetRoomDarkness(byte room)
        {
            int value = dark_rooms[room >> 4] & 1 << 15 - room % 16;
            return value != 0;
        }


        // spawn ennemies and or bosses in a simillar way to overworld
        void SpawnEnemies()
        {
            if (!ScrollingDone())
                return;

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
                        break;
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
            List<int> ignore_list = new();
            for (int i = 0; i < 8; i++)
            {
                uint enemy_id = (enemies & enemy_selector) >> (7 - i) * 4;

                if ((DungeonEnemies)enemy_id is not (DungeonEnemies.BUBBLE or DungeonEnemies.BUBBLE_BLUE or DungeonEnemies.BUBBLE_RED
                    or DungeonEnemies.BLADE_TRAP or DungeonEnemies.NPC))
                {
                    int kq = IsInKillQueue(current_screen, ignore_list.ToArray());
                    if (kq != -1)
                    {
                        ignore_list.Add(kq);
                        enemy_selector >>= 4;
                        continue;
                    }
                }

                if (enemy_id != (uint)DungeonEnemies.NONE)
                {
                    nb_enemies_alive++;
                }

                if (enemy_id < 8)
                {
                    switch ((DungeonEnemies)enemy_id)
                    {
                        case DungeonEnemies.NONE:
                            break;
                        case DungeonEnemies.GEL:
                            new Gel(null);
                            break;
                        case DungeonEnemies.BUBBLE:
                            new Bubble(Bubble.BubbleType.NORMAL);
                            nb_enemies_alive--;
                            break;
                        case DungeonEnemies.BUBBLE_BLUE:
                            new Bubble(Bubble.BubbleType.BLUE);
                            nb_enemies_alive--;
                            break;
                        case DungeonEnemies.BUBBLE_RED:
                            new Bubble(Bubble.BubbleType.RED);
                            nb_enemies_alive--;
                            break;
                        case DungeonEnemies.KEESE:
                            new Keese(false);
                            break;
                        case DungeonEnemies.NPC:
                            //TODO
                            new CaveNPC(WarpCode.NPC.OLD_MAN);
                            new UndergroundFireSprite(72, 128);
                            new UndergroundFireSprite(168, 128);
                            nb_enemies_alive--;
                            break;
                        case DungeonEnemies.BLADE_TRAP:
                            BladeTrapManager.CreateTraps();
                            nb_enemies_alive--;
                            break;
                    }

                    enemy_selector >>= 4;
                    continue;
                }

                if (current_dungeon is 0 or 1 or 6)
                {
                    switch ((DungeonEnemies)enemy_id)
                    {
                        case DungeonEnemies.STALFOS:
                            new Stalfos();
                            break;
                        case DungeonEnemies.GORIYA:
                            new Goriya(false);
                            break;
                        case DungeonEnemies.GORIYA_HARDER:
                            new Goriya(true);
                            break;
                        case DungeonEnemies.WALLMASTER:
                            new Wallmaster();
                            break;
                        case DungeonEnemies.ROPE:
                            new Rope(false);
                            break;
                        case DungeonEnemies.ROPE_HARDER:
                            new Rope(true);
                            break;
                    }
                }
                else if (current_dungeon is 2 or 4 or 7)
                {
                    switch ((DungeonEnemies)enemy_id + 6)
                    {
                        case DungeonEnemies.ZOL:
                            new Zol();
                            break;
                        case DungeonEnemies.GIBDO:
                            new Gibdo();
                            break;
                        case DungeonEnemies.DARKNUT:
                            new Darknut(false);
                            break;
                        case DungeonEnemies.DARKNUT_HARDER:
                            new Darknut(true);
                            break;
                        case DungeonEnemies.POLSVOICE:
                            new PolsVoice();
                            break;
                    }
                }
                else
                {
                    switch ((DungeonEnemies)enemy_id + 10)
                    {
                        // woops, the enemy table uses the wrong value for zols in 469 dungeons. oh well
                        case DungeonEnemies.ZOL or (DungeonEnemies)18:
                            new Zol();
                            break;
                        case DungeonEnemies.VIRE:
                            new Vire();
                            break;
                        case DungeonEnemies.LIKELIKE:
                            new LikeLike();
                            break;
                        case DungeonEnemies.WIZROBE:
                            new WizrobeOrange();
                            break;
                        case DungeonEnemies.WIZROBE_HARDER:
                            new WizrobeBlue();
                            break;
                    }
                }

                enemy_selector >>= 4;
            }

            if (room_list[current_screen] is ((byte)RoomType.STATUE_DUO or (byte)RoomType.ARENA))
            {
                new Statues();
            }
        }

        void SpawnItems()
        {
            if (room_list[current_screen] == 11 && ScrollingDone() && !SaveLoad.GetTriforceFlag(current_dungeon))
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

        // animation of link walking on his own when entering or exiting a room
        public void LinkWalkAnimation()
        {
            const byte ANIM_LEN = 11;
            if (link_walk_animation_timer == 0)
            {
                //TODO: extend animation if entering bomb door
                link_walk_animation_timer = ANIM_LEN;
                Direction link_opposite_dir = Link.facing_direction.Opposite();
                if ((IsClosedType(doors[(int)link_opposite_dir], link_opposite_dir) ||
                    GetDoorType(current_screen, link_opposite_dir) == DoorType.BOMBABLE) && ScrollingDone())
                {
                    link_walk_animation_timer = ANIM_LEN * 2;
                }
            }

            if (link_walk_animation_timer == ANIM_LEN)
            {
                if (is_dark && !GetRoomDarkness(current_screen))
                {
                    DarkeningAnimationDisable();
                }
                UnloadSpritesRoomTransition();
                if (dark_room_animation_timer != 0)
                {
                    DarkeningAnimation();
                    return;
                }
            }
            else if (link_walk_animation_timer == 1)
            {
                Link.can_move = ScrollingDone();
                Menu.can_open_menu = ScrollingDone();
                SpawnEnemies();
                SpawnItems();
                SaveLoad.SetDungeonVisitedRoomFlag(current_screen, true);
                link_walk_animation_timer = 0;
                frames_since_link_can_walk = 0;
                // when link exits through a closed door type, it initializes as open, but then closes the moment link can move.
                // even if the condition to open is already meant. if that's the case, it'll open next frame.
                Direction exit_door = Link.facing_direction.Opposite();
                if (IsClosedType(GetDoorType(current_screen, exit_door), exit_door) && ScrollingDone())
                {
                    DrawDoor(exit_door, DOOR_GENERIC_CLOSED);
                    //TODO: door opening noise
                }
                return;
            }

            Link.can_move = false;
            Menu.can_open_menu = false;
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

            if (gTimer % 2 == 0)
            {
                x_add *= 2;
                y_add *= 2;
            }
            Link.SetPos(Link.x + x_add, Link.y + y_add);

            link_walk_animation_timer--;
        }


        // code that runs every frame after the screen has been fully loaded
        void DoorCode()
        {
            frames_since_link_can_walk++;
            for (int i = 0; i < doors.Length; i++)
            {
                switch (doors[i])
                {
                    case DoorType.KEY:
                        if (Link.dungeon_wall_push_timer >= 8 && Link.facing_direction == (Direction)i 
                            && GetMetaTileIndexAtLocation(Link.x + 8, Link.y + 8) is (151 or 152 or 82 or 93 or 39 or 40)
                            && (Math.Abs(Link.x - 120) < 4 || Math.Abs(Link.y - 144) < 4))
                        {
                            // shit code. bleh. stupid ass key door detection code.
                            // need to do this otherwise you can remotely open doors
                            if ((Direction)i == Direction.UP && Link.y > 120 ||
                                (Direction)i == Direction.DOWN && Link.y < 180 ||
                                (Direction)i == Direction.LEFT && Link.x > 64 ||
                                (Direction)i == Direction.RIGHT && Link.x < 192)
                                break;

                            if (SaveLoad.key_count == 0 && !SaveLoad.magical_key)
                                break;

                            if (SaveLoad.key_count > 0)
                                SaveLoad.key_count--;

                            // set correct key flag by getting the index of the door's connectionID in key_door_connections
                            SaveLoad.SetOpenedKeyDoorsFlag
                            (
                                (byte)Array.IndexOf
                                (
                                    key_door_connections,
                                    (short)DC.getConnectionID(current_screen, (Direction)i)
                                ),
                                true
                            );

                            DrawDoor((Direction)i, DoorType.OPEN);
                            // TODO: play door opening sfx
                        }
                        break;

                    case DoorType.BOMBABLE:
                        // bomb sprite sets flag to true if it hit a bombable door metatile
                        if (door_statuses[i])
                        {
                            byte index = (byte)Array.IndexOf(bombable_connections, (short)getConnectionID(current_screen, (Direction)i));
                            if (!SaveLoad.GetBombedHoleFlag(index))
                            {
                                SaveLoad.SetBombedHoleFlag(index, true);
                                DrawDoor((Direction)i, DoorType.BOMBABLE);
                            }
                        }
                        break;

                    case DoorType.CLOSED_ENEMY_DR or DoorType.CLOSED_ENEMY_UL or DoorType.CLOSED_ENEMY_UD or DoorType.CLOSED_ENEMY_LR:
                        // don't check the door from the first 16 frames so that it can close before
                        // opening when the condition of the room is met when entering it
                        if (door_statuses[i] || frames_since_link_can_walk <= 16)
                            break;

                        if (nb_enemies_alive <= 0)
                        {
                            door_statuses[i] = true;
                            DrawDoor((Direction)i, DoorType.OPEN);
                        }
                        break;
                    case DoorType.CLOSED_PUSH_DR or DoorType.CLOSED_PUSH_UL:
                        if (block_push_flag && !door_statuses[i])
                        {
                            door_statuses[i] = true;
                            DrawDoor((Direction)i, DoorType.OPEN);
                        }
                        break;
                    case DoorType.CLOSED_TRIFORCE:
                        if (SaveLoad.triforce_of_power && !(door_statuses[i] || frames_since_link_can_walk <= 16))
                        {
                            door_statuses[i] = true;
                            DrawDoor((Direction)i, DoorType.OPEN);
                        }
                        break;
                    case DoorType.WALK_THROUGH:
                        break;
                }
            }
        }

        // this sets the door's entry in doors[] to its correct type, by finding and using its connectionID.
        // however, it only does this if the entry is marked as DoorType.NONE.
        // otherwise, it just returns the current door type of the given door.
        public DoorType GetDoorType(byte room_id, Direction door_direction)
        {
            // door_types must be cleared before use
            if (doors[(int)door_direction] != DoorType.NONE)
                return doors[(int)door_direction];

            // now we must retreive the data from the crunched up connectionID table. there are 256 bytes in the table, one for each room,
            // but each entry in the table is 4 bits, where the upper 4 bits are the connection type between that room and the one below,
            // and the lower 4 bits are for the connection between that room and the one to the right. if we want to know the upper or left
            // connection, we need to check the entry for the room on the other side of those connections. connection_id will be an index into
            // this table, but will treat each nibble as if it were its on entry, and thus pretends the table is 512 entries.
            int connection_id = 2 * room_id;
            if (door_direction == Direction.LEFT)
                connection_id--;
            else if (door_direction == Direction.RIGHT)
                connection_id++;
            else if (door_direction == Direction.UP)
                connection_id -= 32;

            // if entry indexes into out of bounds, there is no door.
            if (connection_id < 0 || connection_id >= connection_IDs.Length * 2)
                return DoorType.NONE;

            // store the connection index for later
            int connection_index = connection_id;

            // now that we have our entry in the hypothetical 512 byte table, we need to retreive data from the real 256 byte table.
            // the first step is to determine wether we want the upper nibble or lower nibble. the lower nibble will always be for
            // left-right connections, and thus we can use the right bitmask to retreive the data.
            byte bit_mask = 0xF0;
            if (door_direction is (Direction.LEFT or Direction.RIGHT))
            {
                bit_mask >>= 4;
            }
            connection_id = connection_IDs[connection_id / 2] & bit_mask;
            if (bit_mask == 0xF0)
                connection_id >>= 4;

            DoorType connection_type = (DoorType)connection_id;

            if (connection_type == DoorType.KEY)
            {
                if (SaveLoad.GetOpenedKeyDoorsFlag((byte)Array.IndexOf(key_door_connections, (short)connection_index)))
                    connection_type = DoorType.OPEN;
            }

            doors[(int)door_direction] = connection_type;
            return connection_type;
        }

        // returns true if input "type" is supposed to be a closed door type
        public bool IsClosedType(DoorType type, Direction door_direction)
        {
            if (door_direction is (Direction.DOWN or Direction.RIGHT))
            {
                return type is (DoorType.CLOSED_TRIFORCE or DoorType.CLOSED_PUSH_UL or DoorType.CLOSED_ENEMY_UL
                or DoorType.CLOSED_ENEMY_UD or DoorType.CLOSED_ENEMY_LR or DoorType.CLOSED_ALWAYS_UL);
            }
            else
            {
                return type is (DoorType.CLOSED_TRIFORCE or DoorType.CLOSED_PUSH_DR or DoorType.CLOSED_ENEMY_DR
                or DoorType.CLOSED_ENEMY_UD or DoorType.CLOSED_ENEMY_LR or DoorType.CLOSED_ALWAYS_DR);
            }
        }

        // code that gets called after PPU page drawn to a screen. sets the doors to what they should appear like
        // during scrolling/opening/warp
        public void InitDoors(byte room, byte screen_index)
        {
            int screen_index_offset = screen_index * Textures.SCREEN_TILES;

            // texture data for adding an open door to a wall on any side.
            // index into this with [direction, y, x]
            byte[,,] door_data = {
                {// up
                    { 0xf6,0xf6,0xf6,0xf6 },
                    { 0x78,0x78,0x78,0x78 },
                    { 0x79,0x24,0x24,0x7b },
                    { 0x7a,0x77,0x75,0x7c }
                },
                {// down
                    { 0x7e,0x76,0x74,0x80 },
                    { 0x7f,0x24,0x24,0x81 },
                    { 0x7d,0x7d,0x7d,0x7d },
                    { 0xf6,0xf6,0xf6,0xf6 }
                },
                {// left
                    { 0xf6,0x82,0x83,0x85 },
                    { 0xf6,0x82,0x24,0x76 },
                    { 0xf6,0x82,0x24,0x77 },
                    { 0xf6,0x82,0x84,0x86 }
                },
                {// right
                    { 0x88,0x8a,0x87,0xf6 },
                    { 0x74,0x24,0x87,0xf6 },
                    { 0x75,0x24,0x87,0xf6 },
                    { 0x89,0x8b,0x87,0xf6 }
                }
            };
            int[] door_ppu_locations = { 0x10e, 0x34e, 0x220, 0x23c };
            int[] door_center_ppu_locations = { 0x14f, 0x34f, 0x242, 0x25c};

            // init door metatiles to wall. |left|  |right| |-----up---|  |----down--------|
            foreach (byte i in new byte[] { 80, 81, 94, 95, 7, 8, 23, 24, 151, 152, 167, 168 })
                Screen.meta_tiles[i].tile_index_D = DungeonMetatile.WALL;

            if (screen_index == 1)
                screen_index_offset -= 256;

            Array.Clear(doors);

            for (int i = 0; i < doors.Length; i++)
            {
                // makes it so entrances only have open door on bottom, and not top of room below
                // this checks for every room, so an additional check to prevent OOB indexing when checking the type of room above
                if ((Direction)i == Direction.UP && room - 16 >= 0)
                    if (room_list[room - 16] == (int)RoomType.ENTRANCE)
                        continue;

                doors[i] = GetDoorType(room, (Direction)i);

                if (doors[i] == DoorType.NONE)
                {
                    continue;
                }

                // DoorType.Open is here to leech off of the code to make the 
                if (doors[i] is DoorType.BOMBABLE)
                {
                    if (SaveLoad.GetBombedHoleFlag((byte)Array.IndexOf(bombable_connections, (short)getConnectionID(room, (Direction)i))))
                    {
                        byte[,] bomb_hole_texture_data = {
                            { 0x8c, 0x8d, 0x24, 0x24 },// up
                            { 0x24, 0x24, 0x8e, 0x8f },// down
                            { 0x90, 0x24, 0x91, 0x24 },// left
                            { 0x24, 0x92, 0x24, 0x93 }// right
                        };

                        Textures.ppu[door_center_ppu_locations[i] + screen_index_offset] = bomb_hole_texture_data[i, 0];
                        Textures.ppu[door_center_ppu_locations[i] + screen_index_offset + 1] = bomb_hole_texture_data[i, 1];
                        Textures.ppu[door_center_ppu_locations[i] + screen_index_offset + Textures.PPU_WIDTH] = bomb_hole_texture_data[i, 2];
                        Textures.ppu[door_center_ppu_locations[i] + screen_index_offset + Textures.PPU_WIDTH + 1] = bomb_hole_texture_data[i, 3];
                        MakeDoorWalkable((Direction)i);
                    }

                    continue;
                }

                // draw an open door onto the wall. doesn't matter if it's supposed to be open, the middle will be replaced later
                for (int height = 0; height < 4; height++)
                    for (int width = 0; width < 4; width++)
                        Textures.ppu[door_ppu_locations[i] + height * Textures.PPU_WIDTH + width + screen_index_offset] = door_data[i, height, width];

                // these are not the full textures, but indexes to the top left corner of the 2x2 texture.
                // index with [type, direction]
                byte[,] texture_locations = {
                    { 0x98, 0x9c, 0xa0, 0xa4 },// key door
                    { 0xa8, 0xa8, 0xac, 0xac }// locked door
                };

                if (doors[i] == DoorType.OPEN)
                {
                    MakeDoorWalkable((Direction)i);
                    continue;
                }
                // a door with doortype key will always be locked. when unlocked, the doortype is set to open, because an unlocked key door
                // is perfectly identical to an open door.
                else if (doors[i] == DoorType.KEY)
                {
                    Textures.ppu[door_center_ppu_locations[i] + screen_index_offset] = texture_locations[0, i];
                    Textures.ppu[door_center_ppu_locations[i] + screen_index_offset + 1] = (byte)(texture_locations[0, i] + 2);
                    Textures.ppu[door_center_ppu_locations[i] + screen_index_offset + Textures.PPU_WIDTH] = (byte)(texture_locations[0, i] + 1);
                    Textures.ppu[door_center_ppu_locations[i] + screen_index_offset + Textures.PPU_WIDTH + 1] = (byte)(texture_locations[0, i] + 3);
                }
                // remaining cases are for locked doors, but if link is coming through one form the other side,
                // then they only lock after the walking animation. on initialization, they appear open.
                else if (((Direction)i).Opposite() != scroll_direction && IsClosedType(doors[i], (Direction)i))
                {
                    Textures.ppu[door_center_ppu_locations[i] + screen_index_offset] = texture_locations[1, i];
                    Textures.ppu[door_center_ppu_locations[i] + screen_index_offset + 1] = (byte)(texture_locations[1, i] + 2);
                    Textures.ppu[door_center_ppu_locations[i] + screen_index_offset + Textures.PPU_WIDTH] = (byte)(texture_locations[1, i] + 1);
                    Textures.ppu[door_center_ppu_locations[i] + screen_index_offset + Textures.PPU_WIDTH + 1] = (byte)(texture_locations[1, i] + 3);
                }
                // there was a glitch where walking through a pushblock door would make the other side not walkable even if it was open. this is the fix
                else if (!IsClosedType(doors[i], (Direction)i))
                {
                    MakeDoorWalkable((Direction)i);
                }
            }

            // do not place before loop! check above comment.
            Array.Clear(door_statuses);
        }

        // replaces door metatiles with ones where link can walk through.
        // setting make_wall to true does the opposite, and makes the door unwalkable
        // setting walk_through to true will instead replace them with walk-through tiles
        void MakeDoorWalkable(Direction dir, bool make_wall = false, bool walk_through = false)
        {
            byte[] door_metatiles = {
                7, 151, 80, 94
            };

            if (walk_through)
            {
                meta_tiles[door_metatiles[(int)dir]].tile_index_D = DungeonMetatile.WALK_THROUGH_WALL;
                return;
            }

            if (make_wall)
            {
                switch (dir)
                {
                    case Direction.UP:
                    case Direction.DOWN:
                        meta_tiles[door_metatiles[(int)dir]].tile_index_D = DungeonMetatile.WALL;
                        meta_tiles[door_metatiles[(int)dir] + 1].tile_index_D = DungeonMetatile.WALL;
                        meta_tiles[door_metatiles[(int)dir] + 16].tile_index_D = DungeonMetatile.WALL;
                        meta_tiles[door_metatiles[(int)dir] + 16 + 1].tile_index_D = DungeonMetatile.WALL;
                        if (dir == Direction.UP)
                        {
                            meta_tiles[door_metatiles[(int)dir] + 16 * 2].tile_index_D = DungeonMetatile.ROOM_TOP;
                            meta_tiles[door_metatiles[(int)dir] + 16 * 2 + 1].tile_index_D = DungeonMetatile.ROOM_TOP;
                        }
                        break;
                    case Direction.LEFT:
                    case Direction.RIGHT:
                        meta_tiles[door_metatiles[(int)dir]].tile_index_D = DungeonMetatile.WALL;
                        meta_tiles[door_metatiles[(int)dir] + 1].tile_index_D = DungeonMetatile.WALL;
                        break;
                }
                return;
            }

            switch (dir)
            {
                case Direction.UP:
                case Direction.DOWN:
                    meta_tiles[door_metatiles[(int)dir]].tile_index_D = DungeonMetatile.VERT_DOOR_LEFT;
                    meta_tiles[door_metatiles[(int)dir] + 1].tile_index_D = DungeonMetatile.VERT_DOOR_RIGHT;
                    meta_tiles[door_metatiles[(int)dir] + 16].tile_index_D = DungeonMetatile.VERT_DOOR_LEFT;
                    meta_tiles[door_metatiles[(int)dir] + 16 + 1].tile_index_D = DungeonMetatile.VERT_DOOR_RIGHT;
                    // eliminate the "room_top" tiles below top door. however, special tiles need to be made that act like
                    // corner tiles... JUST for this edge case.
                    if (dir == Direction.UP)
                    {
                        meta_tiles[door_metatiles[(int)dir] + 16 * 2].tile_index_D = DungeonMetatile.TOP_DOOR_OPEN_L;
                        meta_tiles[door_metatiles[(int)dir] + 16 * 2 + 1].tile_index_D = DungeonMetatile.TOP_DOOR_OPEN_R;
                    }
                    break;
                case Direction.LEFT:
                case Direction.RIGHT:
                    meta_tiles[door_metatiles[(int)dir]].tile_index_D = DungeonMetatile.ROOM_TOP;
                    meta_tiles[door_metatiles[(int)dir] + 1].tile_index_D = DungeonMetatile.ROOM_TOP;
                    break;
            }
        }

        // THIS SHOULD NOT BE CALLED DURING SCROLLING OR INIT!
        // it does not keep track of which screen it needs to draw to, it only draws to screen 0.
        void DrawDoor(Direction door, DoorType new_type)
        {
            // copied from InitDoors()
            byte[,,] door_data = {
                {// up
                    { 0xf6,0xf6,0xf6,0xf6 },
                    { 0x78,0x78,0x78,0x78 },
                    { 0x79,0x24,0x24,0x7b },
                    { 0x7a,0x77,0x75,0x7c }
                },
                {// down
                    { 0x7e,0x76,0x74,0x80 },
                    { 0x7f,0x24,0x24,0x81 },
                    { 0x7d,0x7d,0x7d,0x7d },
                    { 0xf6,0xf6,0xf6,0xf6 }
                },
                {// left
                    { 0xf6,0x82,0x83,0x85 },
                    { 0xf6,0x82,0x24,0x76 },
                    { 0xf6,0x82,0x24,0x77 },
                    { 0xf6,0x82,0x84,0x86 }
                },
                {// right
                    { 0x88,0x8a,0x87,0xf6 },
                    { 0x74,0x24,0x87,0xf6 },
                    { 0x75,0x24,0x87,0xf6 },
                    { 0x89,0x8b,0x87,0xf6 }
                }
            };
            int[] door_ppu_locations = { 0x10e, 0x34e, 0x220, 0x23c };
            int[] door_center_ppu_locations = { 0x14f, 0x34f, 0x242, 0x25c };
            byte[] locked_door_tile_indexes = {
                0xa8, 0xa8, 0xac, 0xac
            };
            byte[,] bomb_hole_texture_data = {
                { 0x8c, 0x8d, 0x24, 0x24 },// up
                { 0x24, 0x24, 0x8e, 0x8f },// down
                { 0x90, 0x24, 0x91, 0x24 },// left
                { 0x24, 0x92, 0x24, 0x93 }// right
            };

            switch (new_type)
            {
                case DoorType.OPEN:
                    // code copied from InitDoors(). it draws the whole thing instead of the 2x2 area that needs to be modified, but fuckin whatever.
                    //TODO: reduce this to the 2x2 needed
                    for (int height = 0; height < 4; height++)
                        for (int width = 0; width < 4; width++)
                            Textures.ppu[door_ppu_locations[(int)door] + height * Textures.PPU_WIDTH + width] = door_data[(int)door, height, width];
                    MakeDoorWalkable(door);
                    //TODO: door open noise
                    break;

                case DoorType.BOMBABLE:
                    Textures.ppu[door_center_ppu_locations[(int)door]] = bomb_hole_texture_data[(int)door, 0];
                    Textures.ppu[door_center_ppu_locations[(int)door] + 1] = bomb_hole_texture_data[(int)door, 1];
                    Textures.ppu[door_center_ppu_locations[(int)door] + Textures.PPU_WIDTH] = bomb_hole_texture_data[(int)door, 2];
                    Textures.ppu[door_center_ppu_locations[(int)door] + Textures.PPU_WIDTH + 1] = bomb_hole_texture_data[(int)door, 3];
                    MakeDoorWalkable(door);
                    break;

                case DOOR_GENERIC_CLOSED:
                    Textures.ppu[door_center_ppu_locations[(int)door]] = locked_door_tile_indexes[(int)door];
                    Textures.ppu[door_center_ppu_locations[(int)door] + 1] = (byte)(locked_door_tile_indexes[(int)door] + 2);
                    Textures.ppu[door_center_ppu_locations[(int)door] + Textures.PPU_WIDTH] = (byte)(locked_door_tile_indexes[(int)door] + 1);
                    Textures.ppu[door_center_ppu_locations[(int)door] + Textures.PPU_WIDTH + 1] = (byte)(locked_door_tile_indexes[(int)door] + 3);
                    //TODO: door close noise
                    break;
            }
        }


        // get connection ID of connection between two rooms, found with direction relative to room id
        public int getConnectionID(byte room_id, Direction door_direction)
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


        void CheckForWarp()
        {
            if (warp_flag)
            {
                warp_flag = false;
                Link.current_action = LinkAction.WALKING_DOWN;
            }
        }

        byte GetMetatileIndexFromGiftLocationID(byte id)
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
    }
}