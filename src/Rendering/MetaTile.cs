using The_Legend_of_Zelda.Gameplay;
using The_Legend_of_Zelda.Sprites;
using static The_Legend_of_Zelda.Gameplay.Program;

namespace The_Legend_of_Zelda.Rendering
{
    public enum MetatileType
    {
        ROCK,
        GROUND,
        ROCK_TOP,
        BLACK_SQUARE_WARP,
        ROCK_TR,
        ROCK_TL,
        ROCK_BR,
        ROCK_BL,
        ROCK_SNAIL,
        TREE,
        WATER,
        WATER_L,
        WATER_TL,
        WATER_T,
        WATER_TR,
        WATER_R,
        WATER_BR,
        WATER_B,
        WATER_BL,
        BLUE_STAIRS,
        DOCK,
        STAIRS,
        SAND,
        WATERFALL,
        WATERFALL_BOTTOM,
        TOMBSTONE,
        STUMP_TL,
        STUMP_BL,
        STUMP_BR,
        STUMP_TR,
        STUMP_FACE,
        RUINS_TL,
        RUINS_BL,
        RUINS_TR,
        RUINS_BR,
        RUINS_FACE_1_EYE,
        RUINS_FACE_2_EYES,
        STATUE,
        WATER_INNER_TR,
        WATER_INNER_TL,
        WATER_INNER_BL,
        WATER_INNER_BR,
        BOMBABLE_ROCK,
        BURNABLE_TREE,
        FAST_TRAVEL_STAIRCASE,
        WATER_RAFT,
        LADDER_TOP,
        LADDER_BOTTOM,
        LADDER_LEFT,
        LADDER_RIGHT,
        LADDER_TL,
        LADDER_TR,
        LADDER_EMPTY,
    }

    public enum DungeonMetatile
    {
        GROUND,
        WALL,
        WATER,
        LEFT_STATUE,
        RIGHT_STATUE,
        SAND,
        STAIRS,
        VOID,
        GRAY_BRICKS,
        GRAY_STAIRS,
        ROOM_TOP,
        VERT_DOOR_LEFT,
        VERT_DOOR_RIGHT,
        WALK_THROUGH_WALL,
        TOP_DOOR_OPEN_L,
        TOP_DOOR_OPEN_R,
    }

    public class MetaTile
    {
        public const int METATILES_PER_ROW = 16;

        // ppu indices of all overworld metatiles
        readonly byte[,] overworld_tileset_indexes = {
            {0xd8,0xda,0xd9,0xdb}, // rock
            {0x26,0x26,0x26,0x26}, // ground
            {0xce,0xd0,0xcf,0xd1}, // rock T
            {0x24,0x24,0x24,0x24}, // black hole
            {0xd0,0xd2,0xd1,0xd3}, // rock TR
            {0xcc,0xce,0xcd,0xcf}, // rock TL
            {0xdc,0xde,0xdd,0xdf}, // rock BR
            {0xd4,0xd6,0xd5,0xd7}, // rock BL
            {0xc8,0xca,0xc9,0xcb}, // rock snail
            {0xc4,0xc6,0xc5,0xc7}, // tree
            {0x90,0x90,0x95,0x95}, // water
            {0x8e,0x90,0x93,0x95}, // water R
            {0x8d,0x8f,0x8e,0x90}, // water TL
            {0x8f,0x8f,0x90,0x90}, // water T
            {0x8f,0x91,0x90,0x92}, // water TR
            {0x90,0x92,0x95,0x97}, // water L
            {0x95,0x97,0x96,0x98}, // water BR
            {0x95,0x95,0x96,0x96}, // water B
            {0x93,0x95,0x94,0x96}, // water BL
            {0x74,0x75,0x74,0x75}, // ladder
            {0x76,0x76,0x77,0x77}, // dock
            {0x70,0x72,0x71,0x73}, // stairs
            {0x84,0x86,0x85,0x87}, // sand
            {0x89,0x8b,0x8a,0x8c}, // waterfall
            {0x89,0x8b,0x88,0x88}, // waterfall bottom
            {0xbc,0xbe,0xbd,0xbf}, // tombstone
            {0xb0,0xb2,0xb1,0xb3}, // stump TL
            {0xac,0xae,0xad,0xaf}, // stump BL
            {0xb8,0xba,0xb9,0xbb}, // stump BR
            {0xaa,0xac,0xab,0xad}, // stump TR
            {0xb4,0xb6,0xb5,0xb7}, // stump face
            {0x9c,0x9e,0x9d,0x9f}, // ruins TL
            {0xa2,0xa4,0xa3,0xa5}, // ruins BL
            {0x9a,0x9c,0x9b,0x9d}, // ruins TR
            {0xa0,0xa2,0xa1,0xa3}, // ruins BR
            {0xe0,0xe2,0xe1,0xe3}, // ruins face 1 eye
            {0xa6,0xa8,0xa7,0xa9}, // ruins face 2 eyes
            {0xc0,0xc2,0xc1,0xc3}, // statue
            {0x7a,0x7c,0x7b,0x7d}, // water I TR
            {0x78,0x7a,0x79,0x7b}, // water I TL
            {0x7e,0x80,0x7f,0x81}, // water I BL
            {0x80,0x82,0x81,0x83}, // water I BR
        };
        public readonly byte[,] dungeon_tileset_indexes = {
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
        readonly byte[] dungeon_rooms_with_pushblock =
        {
            1, 13, 14, 26, 42, 48, 50, 51, 58, 74, 108, 133, 139, 140, 141, 143, 146, 151, 152, 161, 168, 184, 221, 233, 244, 252
        };

        public short id;
        public byte _tile_index;
        public bool special = false;

        public MetatileType tile_index
        {
            get => (MetatileType)_tile_index;
            set => _tile_index = (byte)value;
        }

        public DungeonMetatile tile_index_D
        {
            get => (DungeonMetatile)_tile_index;
            set => _tile_index = (byte)value;
        }

        public MetaTile(short id)
        {
            this.id = id;
        }

        public void SetPPUValues(byte tile_index)
        {
            // x and y of top left tile in tile grid
            int x = id % METATILES_PER_ROW * 2;
            int y = id / METATILES_PER_ROW * 2;

            //TODO: ????? magic numbers
            int y_offset = id < 352 ? 8 : 24;

            int index_in_ppu_arr = (y + y_offset) * Textures.PPU_WIDTH + x;

            this._tile_index = tile_index;
            special = false;

            if (gamemode == Gamemode.DUNGEON)
            {
                if (tile_index == 1 && Array.IndexOf(dungeon_rooms_with_pushblock, DC.scroll_destination) != -1)
                {
                    switch ((DungeonCode.RoomType)DC.room_list[DC.scroll_destination])
                    {
                        case DungeonCode.RoomType.SINGLE_PUSH_BLOCK:
                            if (id == 87)
                                special = true;
                            break;

                        case DungeonCode.RoomType.DOUBLE_PUSH_BLOCK:
                            if (id == 86)
                                special = true;
                            break;

                        case DungeonCode.RoomType.SIDEWAYS_U:
                            if (id == 92)
                                special = true;
                            break;

                        case DungeonCode.RoomType.CENTRAL_STAIRS:
                            if (id == 86)
                                special = true;
                            break;
                    }
                }

                Textures.ppu[index_in_ppu_arr] = dungeon_tileset_indexes[tile_index, 0];
                Textures.ppu[index_in_ppu_arr + 1] = dungeon_tileset_indexes[tile_index, 1];
                Textures.ppu[index_in_ppu_arr + Textures.PPU_WIDTH] = dungeon_tileset_indexes[tile_index, 2];
                Textures.ppu[index_in_ppu_arr + Textures.PPU_WIDTH + 1] = dungeon_tileset_indexes[tile_index, 3];
                return;
            }

            // screen to check for secrets in. usually the current one
            byte screen_to_apply_to = OC.current_screen;

            // if in cave, return screen
            if (OC.ScrollingDone() && OC.current_screen == 128)
            {
                screen_to_apply_to = OC.return_screen;
            }
            // if not scrolling up and not in first 2 screens, the scroll dest
            // this exception exists because scrolling up is weird and not like the other scrolls
            else if (OC.scroll_direction != Direction.UP || id < 176)
            {
                screen_to_apply_to = OC.scroll_destination;
            }

            //TODO: hardcode secrets in list, remove them from map file
            // this shit is ugly af
            special = false;
            if (tile_index > 41)
            {
                special = true;
                if (tile_index == 42)
                {
                    if (SaveLoad.GetOverworldSecretsFlag((byte)Array.IndexOf(OC.screens_with_secrets_list, screen_to_apply_to)))
                        tile_index = 3;
                    else
                        tile_index = 0;
                }
                else if (tile_index == 43)
                {
                    if (SaveLoad.GetOverworldSecretsFlag((byte)Array.IndexOf(OC.screens_with_secrets_list, screen_to_apply_to)))
                        tile_index = 0x15;
                    else
                        tile_index = 9;
                }
                else if (tile_index == 44)
                {
                    if (SaveLoad.GetOverworldSecretsFlag((byte)Array.IndexOf(OC.screens_with_secrets_list, screen_to_apply_to)))
                        tile_index = 0x15;
                    else
                    {
                        if (screen_to_apply_to == 29)
                            tile_index = 0;
                        else if (screen_to_apply_to == 73)
                            tile_index = 9;
                        else
                            tile_index = 1;
                    }
                }
                else if (tile_index == 45)
                {
                    tile_index = 0xa;
                }
                else if (tile_index == 46)
                {
                    special = true;
                    tile_index = 8;
                }
                else if (tile_index == 47)
                {
                    special = true;
                    tile_index = 0x19;
                }
            }

            this._tile_index = tile_index;
            Textures.ppu[index_in_ppu_arr] = overworld_tileset_indexes[tile_index, 0];
            Textures.ppu[index_in_ppu_arr + 1] = overworld_tileset_indexes[tile_index, 1];
            Textures.ppu[index_in_ppu_arr + Textures.PPU_WIDTH] = overworld_tileset_indexes[tile_index, 2];
            Textures.ppu[index_in_ppu_arr + Textures.PPU_WIDTH + 1] = overworld_tileset_indexes[tile_index, 3];

            return;
        }
    }
}