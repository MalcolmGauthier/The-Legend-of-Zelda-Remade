using static The_Legend_of_Zelda.OverworldCode;
namespace The_Legend_of_Zelda
{
    public class MetaTile
    {
        public short id;
        public byte tile_index;
        public bool special = false;

        public MetaTile(short id)
        {
            this.id = id;
        }

        public void SetPPUValues(byte tile_index)
        {
            int x = (id % 16) * 2;
            int y = (id >> 4) * 2;
            int location;

            int y_offset = 24;
            if (id < 352)
            {
                y_offset = 8;
            }

            location = (y + y_offset) * 32 + x;

            if (Program.gamemode == Program.Gamemode.DUNGEON)
            {
                special = false;
                this.tile_index = tile_index;
                Textures.ppu[location] = DungeonCode.dungeon_tileset_indexes[tile_index, 0];
                Textures.ppu[location + 1] = DungeonCode.dungeon_tileset_indexes[tile_index, 1];
                Textures.ppu[location + 32] = DungeonCode.dungeon_tileset_indexes[tile_index, 2];
                Textures.ppu[location + 33] = DungeonCode.dungeon_tileset_indexes[tile_index, 3];
                return;
            }

            //TODO: figure out why this bullshit exists
            byte screen_to_apply_to;
            if (scroll_animation_timer > 500)
            {
                if (current_screen == 128)
                    screen_to_apply_to = return_screen;
                else
                    screen_to_apply_to = current_screen;
            }
            else
            {
                if (scroll_direction == Direction.UP)
                {
                    if (id < 176)
                        screen_to_apply_to = scroll_destination;
                    else
                        screen_to_apply_to = current_screen;
                }
                else
                {
                    screen_to_apply_to = scroll_destination;
                }
            }

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
            Textures.ppu[location] = overworld_tileset_indexes[tile_index, 0];
            Textures.ppu[location + 1] = overworld_tileset_indexes[tile_index, 1];
            Textures.ppu[location + 32] = overworld_tileset_indexes[tile_index, 2];
            Textures.ppu[location + 33] = overworld_tileset_indexes[tile_index, 3];

            return;

        }
    }
}