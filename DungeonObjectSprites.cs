namespace The_Legend_of_Zelda
{
    internal class TriforcePieceSprite : ItemDropSprite
    {
        StaticSprite counterpart = new StaticSprite(0x6e, 6, 128, 144);
        int triforce_anim_timer = 0;
        public TriforcePieceSprite() : base(120, 144, false)
        {
            tile_index = 0x6e;
            palette_index = 6;
            counterpart.xflip = true;
            counterpart.unload_during_transition = true;
            Screen.sprites.Add(counterpart);
        }
        public override void ItemSpecificActions()
        {
            if (Program.gTimer % 12 == 0)
            {
                palette_index = 5;
                counterpart.palette_index = 5;
            }
            else if (Program.gTimer % 12 == 6)
            {
                palette_index = 6;
                counterpart.palette_index = 6;
            }

            if (collected)
            {
                Link.animation_timer = 0;
                if (triforce_anim_timer == 0)
                {
                    Link.can_move = false;
                    Link.current_action = Link.Action.ITEM_HELD_UP;
                    Link.SetPos(120, 150);
                    y = Link.y - 16;
                    counterpart.y = Link.y - 16;
                    Sound.PauseMusic();
                }
                else if (triforce_anim_timer >= 50 && triforce_anim_timer <= 96)
                {
                    if (triforce_anim_timer % 6 == 3)
                    {
                        byte[] indexes = { 9, 10, 11, 13, 14, 15 };
                        foreach (byte i in indexes)
                            Palettes.active_palette_list[i] = 0x30;
                    }
                    else if (triforce_anim_timer % 6 == 0)
                    {
                        DungeonCode.LoadPalette();
                    }
                }
                else if (triforce_anim_timer == 97)
                {
                    if (Link.hp < SaveLoad.nb_of_hearts[SaveLoad.current_save_file])
                    {
                        Link.full_heal_flag = true;
                        triforce_anim_timer--;
                    }
                }
                else if (triforce_anim_timer == 241)
                {
                    DungeonCode.opening_animation_timer -= 2;
                    triforce_anim_timer--;
                    Link.Show(true);
                    if (DungeonCode.opening_animation_timer <= -2)
                    {
                        DungeonCode.opening_animation_timer = 81;
                        triforce_anim_timer++;
                    }
                }
                else if (triforce_anim_timer > 370)
                {
                    OverworldCode.opening_animation_timer = 0;
                    OverworldCode.black_square_stairs_return_flag = true;
                    OverworldCode.current_screen = OverworldCode.return_screen;
                    SaveLoad.SetTriforceFlag(SaveLoad.current_save_file, DungeonCode.current_dungeon, true);
                    Program.gamemode = Program.Gamemode.OVERWORLD;
                    OverworldCode.Init();
                    Link.SetBGState(true);
                    Screen.sprites.Remove(counterpart);
                    Screen.sprites.Remove(this);
                    return;
                }
                triforce_anim_timer++;
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
                SaveLoad.SetMapFlag(SaveLoad.current_save_file, DungeonCode.current_dungeon, true);
                Menu.DrawHudMap();
                Screen.sprites.Remove(this);
            }
        }
    }
    internal class CompassSprite
    {

    }
}