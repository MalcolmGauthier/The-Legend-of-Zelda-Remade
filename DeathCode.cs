namespace The_Legend_of_Zelda
{
    public static class DeathCode
    {
        public static int death_timer = 0;
        public static bool died_in_dungeon = false;
        static bool select_menu = false;
        static byte selected_option = 0;
        static byte flash_timer = 0;
        static StaticSprite selector = new StaticSprite(0xf2, 6, 80, 0);

        public static void Tick()
        {
            if (select_menu)
            {
                Selection();
            }
            else
            {
                DeathCutscene();
            }
        }

        static void DeathCutscene()
        {
            if (death_timer == 0)
            {
                Link.can_move = false;
                Link.current_action = Link.Action.WALKING_DOWN;
                Link.animation_timer += 6;
                selected_option = 0;
                for (int i = 0; i < Screen.sprites.Count; i++)
                {
                    Sprite spr = Screen.sprites[i];
                    if (spr.unload_during_transition || spr == Menu.map_dot)
                    {
                        Screen.sprites.Remove(spr);
                        i--;
                    }
                    //if (Screen.sprites[i].GetType().BaseType == typeof(Enemy))
                    //{
                    //    Enemy enemy_to_remove = (Enemy)Screen.sprites[i];
                    //    if (Screen.sprites.Remove(enemy_to_remove.counterpart))
                    //        i--;
                    //    Screen.sprites.Remove(enemy_to_remove);
                    //    i--;
                    //}
                }
            }
            else if (death_timer <= 32)
            {
                byte new_palette = (byte)(((death_timer >> 1) % 4) + 4);
                Link.self.palette_index = new_palette;
                Link.counterpart.palette_index = new_palette;
            }
            else if (death_timer == 60)
            {
                Palettes.LoadPalette(2, 1, 0x17);
                Palettes.LoadPalette(2, 2, 0x16);
                Palettes.LoadPalette(2, 3, 0x26);
                Palettes.LoadPalette(3, 1, 0x17);
                Palettes.LoadPalette(3, 2, 0x16);
                Palettes.LoadPalette(3, 3, 0x26);
            }
            else if (death_timer > 60 && death_timer <= 120)
            {
                if (death_timer % 5 == 0)
                {
                    if (Link.current_action == Link.Action.WALKING_DOWN)
                        Link.current_action = Link.Action.WALKING_RIGHT;
                    else if (Link.current_action == Link.Action.WALKING_RIGHT)
                        Link.current_action = Link.Action.WALKING_UP;
                    else if (Link.current_action == Link.Action.WALKING_UP)
                        Link.current_action = Link.Action.WALKING_LEFT;
                    else
                        Link.current_action = Link.Action.WALKING_DOWN;

                    Link.animation_timer += 6;
                }
            }
            else if (death_timer == 116)
            {
                Palettes.LoadPalette(2, 1, Color._06_RED);
                Palettes.LoadPalette(2, 2, 0x17);
                Palettes.LoadPalette(2, 3, 0x16);
                Palettes.LoadPalette(3, 1, Color._06_RED);
                Palettes.LoadPalette(3, 2, 0x17);
                Palettes.LoadPalette(3, 3, 0x16);
            }
            else if (death_timer == 125)
            {
                Palettes.LoadPalette(2, 1, 0x7);
                Palettes.LoadPalette(2, 2, 0x6);
                Palettes.LoadPalette(3, 1, 0x7);
                Palettes.LoadPalette(3, 2, 0x6);
            }
            else if (death_timer == 135)
            {
                Palettes.LoadPalette(2, 1, 0xf);
                Palettes.LoadPalette(2, 2, 0x7);
                Palettes.LoadPalette(2, 3, 0x6);
                Palettes.LoadPalette(3, 1, 0xf);
                Palettes.LoadPalette(3, 2, 0x7);
                Palettes.LoadPalette(3, 3, 0x6);
            }
            else if (death_timer == 145)
            {
                Palettes.LoadPalette(2, 2, 0xf);
                Palettes.LoadPalette(2, 3, 0xf);
                Palettes.LoadPalette(3, 2, 0xf);
                Palettes.LoadPalette(3, 3, 0xf);
            }
            else if (death_timer == 146)
            {
                Palettes.LoadPalette(4, 1, 0x10);
                Palettes.LoadPalette(4, 2, 0x30);
                Palettes.LoadPalette(4, 3, 0x0);
            }
            else if (death_timer == 170)
            {
                Link.self.tile_index = 0x62;
                Link.counterpart.tile_index = 0x62;
                Link.self.palette_index = 5;
                Link.counterpart.palette_index = 5;
                Link.counterpart.xflip = true;
                Link.self.ChangeTexture();
                Link.counterpart.ChangeTexture();
            }
            else if (death_timer == 180)
            {
                Link.self.tile_index = 0x64;
                Link.counterpart.tile_index = 0x64;
                Link.self.ChangeTexture();
                Link.counterpart.ChangeTexture();
            }
            else if (death_timer == 184)
            {
                Link.Show(false);
            }
            else if (death_timer == 230)
            {
                Textures.LoadPPUPage(Textures.PPUDataGroup.OVERWORLD, 128, 0);
                byte[] game_over_text = { 0x10, 0xa, 0x16, 0xe, 0x24, 0x18, 0x1f, 0xe, 0x1b };
                for (int i = 0; i < game_over_text.Length; i++)
                {
                    Textures.ppu[0x24c + i] = game_over_text[i];
                    Textures.ppu_plt[0x24c + i] = 0;
                }
            }
            else if (death_timer >= 330)
            {
                Screen.y_scroll = 8;
                Screen.sprites.Clear();
                Textures.LoadPPUPage(Textures.PPUDataGroup.OTHER, 12, 0);
                select_menu = true;
                selector.use_chr_rom = true;
                selector.ChangeTexture();
                Screen.sprites.Add(selector);
                Sound.PlaySong(Sound.Songs.DEATH, true);
            }

            Link.Tick();
            death_timer++;
        }

        static void Selection()
        {
            if (flash_timer > 0 || Control.IsPressed(Buttons.START))
            {
                Flash();
                return;
            }

            if (Control.IsPressed(Buttons.SELECT))
            {
                selected_option++;
                selected_option %= 3;
            }

            selector.y = 48 + 16 * selected_option;
        }

        static void Flash()
        {
            flash_timer++;

            if (flash_timer % 4 == 0)
            {
                byte new_palette;
                if (flash_timer % 8 == 0)
                    new_palette = 0;
                else
                    new_palette = 1;
                for (int i = 0; i < 8; i++)
                {
                    Textures.ppu_plt[0xcc + selected_option * 64 + i] = new_palette;
                }
            }

            if (flash_timer <= 64)
            {
                return;
            }

            Screen.sprites.Remove(selector);
            select_menu = false;
            flash_timer = 0;
            selector.y = 48;

            if (selected_option == 0) // continue
            {
                if (died_in_dungeon)
                {
                    Program.gamemode = Program.Gamemode.DUNGEON;
                    DungeonCode.Init(DungeonCode.current_dungeon);
                }
                else
                {
                    OverworldCode.current_screen = 119;
                    Program.gamemode = Program.Gamemode.OVERWORLD;
                    OverworldCode.Init();
                }

                return;
            }

            if (selected_option == 1)
            {
                SaveLoad.SaveFile(SaveLoad.current_save_file); // save
            }
            else
            {
                SaveLoad.LoadFile(SaveLoad.current_save_file); // retry
            }

            Program.gamemode = Program.Gamemode.FILESELECT;
            FileSelectCode.InitFileSelect();

            return;
        }
    }
}