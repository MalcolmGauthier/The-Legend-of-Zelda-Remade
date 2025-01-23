using The_Legend_of_Zelda.Gameplay;
using The_Legend_of_Zelda.Rendering;
using static The_Legend_of_Zelda.Gameplay.Program;

namespace The_Legend_of_Zelda.Sprites
{
    internal class TriforcePieceSprite : ItemDropSprite
    {
        enum CollectionAnimState
        {
            INIT,
            WAIT,
            FLASH,
            HEAL,
            WAIT2,
            ERASE_BG,
            EXIT
        }

        StaticSprite counterpart = new StaticSprite(0x6e, 6, 128, 144);
        CollectionAnimState anim_state;
        int anim_row_index = 0;

        public TriforcePieceSprite() : base(120, 144, false)
        {
            tile_index = 0x6e;
            palette_index = 6;
            counterpart.xflip = true;
            counterpart.unload_during_transition = true;
            anim_state = CollectionAnimState.INIT;
            Screen.sprites.Add(counterpart);
        }

        public override void ItemSpecificActions()
        {
            if (gTimer % 12 == 0)
            {
                palette_index = 5;
                counterpart.palette_index = 5;
            }
            else if (gTimer % 12 == 6)
            {
                palette_index = 6;
                counterpart.palette_index = 6;
            }

            if (!collected)
                return;

            // freeze link's animation
            Link.animation_timer = 0;

            switch (anim_state)
            {
                case CollectionAnimState.INIT:
                    Link.can_move = false;
                    Link.current_action = LinkAction.ITEM_HELD_UP;
                    Link.SetPos(120, 150);
                    y = Link.y - 16;
                    counterpart.y = Link.y - 16;
                    Sound.PauseMusic();
                    anim_state = CollectionAnimState.WAIT;
                    local_timer = 0;
                    break;

                case CollectionAnimState.WAIT:
                    if (local_timer >= 50)
                    {
                        anim_state = CollectionAnimState.FLASH;
                        local_timer = 0;
                    }
                    break;

                case CollectionAnimState.FLASH:
                    if (local_timer % 6 == 3)
                    {
                        byte[] indexes = { 9, 10, 11, 13, 14, 15 };
                        foreach (byte i in indexes)
                            Palettes.active_palette_list[i] = (byte)Color._30_WHITE;
                    }
                    else if (local_timer % 6 == 0)
                    {
                        DC.LoadPalette();
                    }

                    if (local_timer > 46)
                    {
                        anim_state = CollectionAnimState.HEAL;
                    }
                    break;

                case CollectionAnimState.HEAL:
                    if (Link.hp == SaveLoad.nb_of_hearts)
                    {
                        anim_state = CollectionAnimState.WAIT2;
                        local_timer = 0;
                    }
                    Link.full_heal_flag = true;
                    break;

                case CollectionAnimState.WAIT2:
                    if (local_timer >= 144)
                    {
                        anim_state = CollectionAnimState.ERASE_BG;
                    }
                    break;

                case CollectionAnimState.ERASE_BG:
                    if (local_timer % 5 == 0)
                    {
                        const int left_index_start = Textures.PPU_WIDTH * 8;
                        const int right_index_start = Textures.PPU_WIDTH * 9 - 1;

                        for (int j = 0; j < 22; j++)
                        {
                            Textures.ppu[left_index_start + j * Textures.PPU_WIDTH + anim_row_index] = 0x24;
                            Textures.ppu[right_index_start + j * Textures.PPU_WIDTH - anim_row_index] = 0x24;
                        }

                        anim_row_index++;

                        if (left_index_start + anim_row_index > right_index_start + anim_row_index)
                        {
                            anim_state = CollectionAnimState.EXIT;
                        }
                    }
                    break;

                case CollectionAnimState.EXIT:
                    OC.black_square_stairs_return_flag = true;
                    OC.current_screen = OC.return_screen;
                    SaveLoad.SetTriforceFlag(DC.current_dungeon, true);
                    gamemode = Gamemode.OVERWORLD;
                    OC.Init();
                    Link.SetBGState(true);
                    Screen.sprites.Remove(counterpart);
                    Screen.sprites.Remove(this);
                    break;
            }
        }
    }

    internal class MapSprite : ItemDropSprite
    {
        public MapSprite(int x, int y) : base(x, y, false)
        {
            tile_index = 0x4c;
            palette_index = 6;
        }
        public override void ItemSpecificActions()
        {
            if (collected)
            {
                SaveLoad.SetMapFlag(DC.current_dungeon, true);
                Menu.DrawHudMap();
                Screen.sprites.Remove(this);
            }
        }
    }

    internal class CompassSprite
    {

    }
}