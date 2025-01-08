using static The_Legend_of_Zelda.OverworldCode;
namespace The_Legend_of_Zelda
{
    public class MetaTile
    {
        public const int METATILES_PER_ROW = 16;

        public short id;
        public byte tile_index;
        public bool special = false;

        public MetaTile(short id)
        {
            this.id = id;
        }

        public void SetPPUValues(byte tile_index)
        {
            // x and y of top left tile in tile grid
            int x = (id % METATILES_PER_ROW) * 2;
            int y = (id / METATILES_PER_ROW) * 2;

            //TODO: ????? magic numbers
            int y_offset = id < 352 ? 8 : 24;

            int index_in_ppu_arr = (y + y_offset) * Textures.PPU_WIDTH + x;

            if (Program.gamemode == Program.Gamemode.DUNGEON)
            {
                special = false;
                this.tile_index = tile_index;
                Textures.ppu[index_in_ppu_arr                         ] = DungeonCode.dungeon_tileset_indexes[tile_index, 0];
                Textures.ppu[index_in_ppu_arr + 1                     ] = DungeonCode.dungeon_tileset_indexes[tile_index, 1];
                Textures.ppu[index_in_ppu_arr + Textures.PPU_WIDTH    ] = DungeonCode.dungeon_tileset_indexes[tile_index, 2];
                Textures.ppu[index_in_ppu_arr + Textures.PPU_WIDTH + 1] = DungeonCode.dungeon_tileset_indexes[tile_index, 3];
                return;
            }

            // screen to check for secrets in. usually the current one
            byte screen_to_apply_to = current_screen;

            // if in cave, return screen
            if (scroll_animation_timer > 500 && current_screen == 128)
            {
                screen_to_apply_to = return_screen;
            }
            // if not scrolling up and not in first 2 screens, the scroll dest
            // this exception exists because scrolling up is weird and not like the other scrolls
            else if (scroll_direction != Direction.UP || id < 176)
            {
                screen_to_apply_to = scroll_destination;
            }

            //TODO: hardcode secrets in list, remove them from map file
            //TODO: load secrets in after scroll anim
            // this shit is ugly af
            special = false;
            if (tile_index > 41)
            {
                special = true;
                if (tile_index == 42)
                {
                    if (SaveLoad.GetOverworldSecretsFlag(SaveLoad.current_save_file, (byte)Array.IndexOf(screens_with_secrets_list, screen_to_apply_to)))
                        tile_index = 3;
                    else
                        tile_index = 0;
                }
                else if (tile_index == 43)
                {
                    if (SaveLoad.GetOverworldSecretsFlag(SaveLoad.current_save_file, (byte)Array.IndexOf(screens_with_secrets_list, screen_to_apply_to)))
                        tile_index = 0x15;
                    else
                        tile_index = 9;
                }
                else if (tile_index == 44)
                {
                    if (SaveLoad.GetOverworldSecretsFlag(SaveLoad.current_save_file, (byte)Array.IndexOf(screens_with_secrets_list, screen_to_apply_to)))
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

            this.tile_index = tile_index;
            Textures.ppu[index_in_ppu_arr]                          = overworld_tileset_indexes[tile_index, 0];
            Textures.ppu[index_in_ppu_arr + 1]                      = overworld_tileset_indexes[tile_index, 1];
            Textures.ppu[index_in_ppu_arr + Textures.PPU_WIDTH]     = overworld_tileset_indexes[tile_index, 2];
            Textures.ppu[index_in_ppu_arr + Textures.PPU_WIDTH + 1] = overworld_tileset_indexes[tile_index, 3];

            return;
        }
    }
}