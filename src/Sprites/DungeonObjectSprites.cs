using System.Runtime.InteropServices;
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
            WAIT3,
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

        protected override void ItemSpecificActions()
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

                    if (local_timer > 47)
                    {
                        DC.LoadPalette();
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

                        if (left_index_start + anim_row_index > right_index_start - anim_row_index)
                        {
                            local_timer = 0;
                            anim_state = CollectionAnimState.WAIT3;
                        }
                    }
                    break;

                case CollectionAnimState.WAIT3:
                    if (local_timer >= 128)
                    {
                        anim_state = CollectionAnimState.EXIT;
                    }
                    break;

                case CollectionAnimState.EXIT:
                    Screen.sprites.Remove(DC.compass_dot);
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
            tile_index = (byte)SpriteID.MAP;
            palette_index = 6;
        }
        protected override void ItemSpecificActions()
        {
            if (collected)
            {
                SaveLoad.SetMapFlag(DC.current_dungeon, true);
                Menu.DrawHudMap();
                Screen.sprites.Remove(this);
            }
        }
    }

    internal class CompassSprite : ItemDropSprite
    {
        public CompassSprite(int x, int y) : base(x, y, false)
        {
            tile_index = (byte)SpriteID.COMPASS;
            palette_index = 6;
        }
        protected override void ItemSpecificActions()
        {
            if (collected)
            {
                SaveLoad.SetCompassFlag(DC.current_dungeon, true);
                DC.compass_dot.shown = true;
                Screen.sprites.Remove(this);
            }
        }
    }

    internal class KeySprite : ItemDropSprite
    {
        public KeySprite(int x, int y) : base(x, y, false)
        {
            tile_index = (byte)SpriteID.KEY;
            palette_index = (byte)PaletteID.SP_2;
        }
        protected override void ItemSpecificActions()
        {
            if (collected)
            {
                SaveLoad.key_count++;
                Screen.sprites.Remove(this);
            }
        }
    }

    internal class BoomerangItemSprite : ItemDropSprite
    {
        bool magical_boomerang;
        public BoomerangItemSprite(int x, int y, bool magical) : base(x, y, false)
        {
            this.magical_boomerang = magical;
            tile_index = (byte)SpriteID.BOOMERANG;
            if (magical)
                palette_index = (byte)PaletteID.SP_1;
            else
                palette_index = (byte)PaletteID.SP_0;
        }

        protected override void ItemSpecificActions()
        {
            if (collected)
            {
                if (magical_boomerang)
                    SaveLoad.magical_boomerang = true;
                else
                    SaveLoad.boomerang = true;
                Screen.sprites.Remove(this);
            }
        }
    }

    internal class ImportantItemSprite : ItemDropSprite
    {
        Action collect_item;
        StaticSprite counterpart = new(1, 0, 0, 0);

        public ImportantItemSprite(Action collect_item, SpriteID sprite, PaletteID palette, bool double_wide) : base(132, 144, false)
        {
            this.collect_item = collect_item;
            tile_index = (byte)sprite;
            palette_index = (byte)palette;

            if (double_wide)
            {
                this.dbl_wide = true;
                counterpart.tile_index = tile_index;
                counterpart.palette_index = palette_index;
                counterpart.xflip = true;
                Screen.sprites.Add(counterpart);
            }
        }

        protected override void ItemSpecificActions()
        {
            if (collected)
            {
                collect_item();
            }
        }
    }

    internal class DamageableFireSprite : Enemy
    {
        public DamageableFireSprite() : base(AnimationMode.TWOFRAMES, (byte)SpriteID.FIRE_L, (byte)SpriteID.FIRE_L, false, false, 6, 0, 0, true)
        {
            HP = 1;
            unaffected_by_clock = true;
        }

        protected override void EnemySpecificActions()
        {
            return;
        }

        protected override void Animation()
        {
            if (Program.gTimer % 12 == 0)
            {
                tile_index = (byte)SpriteID.FIRE_R;
                xflip = true;
                counterpart.tile_index = (byte)SpriteID.FIRE_L;
                counterpart.xflip = true;
            }
            else if (Program.gTimer % 12 == 6)
            {
                tile_index = (byte)SpriteID.FIRE_L;
                xflip = false;
                counterpart.tile_index = (byte)SpriteID.FIRE_R;
                counterpart.xflip = false;
            }
        }
    }
}