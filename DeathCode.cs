using static The_Legend_of_Zelda.Program;

namespace The_Legend_of_Zelda
{
    public static class DeathCode
    {
        enum DeathMenuSelection
        {
            CONTINUE,
            SAVE,
            RETRY
        }

        public static int death_timer = 0;
        public static bool died_in_dungeon = false;
        static bool select_menu = false;
        static DeathMenuSelection selected_option = 0;
        static byte flash_timer = 0;
        static StaticSprite selector = new StaticSprite(0xf2, 6, 80, 0);

        public static void Tick()
        {
            // death mode has two halves: the death animation and the option selection menu
            if (select_menu)
            {
                Selection();
            }
            else
            {
                DeathCutscene();
            }
        }

        // Link's death cutscene
        static void DeathCutscene()
        {
            // death animation part 1: force link to look down and stay still on the first frame, then make him flash for 32 frames
            if (death_timer == 0)
            {
                Link.can_move = false;
                Link.current_action = Link.Action.WALKING_DOWN;
                // forces animation to update next frame
                Link.animation_timer += 6;
                // sets flash timer for 32 frames
                Link.iframes_timer = 32;
                selected_option = DeathMenuSelection.CONTINUE;

                // no foreach, because foreach throws error when modifying list
                for (int i = 0; i < Screen.sprites.Count; i++)
                {
                    Sprite spr = Screen.sprites[i];
                    if (spr.unload_during_transition || spr == Menu.map_dot)
                    {
                        Screen.sprites.Remove(spr);
                        i--;
                    }
                }
            }

            // death animation part 2: background turns to red, link spins around for a second
            else if (death_timer == 60)
            {
                //TODO: does this cover the case of dying in the graveyard? (or anywhere with gray palette)
                Palettes.LoadPalette(PaletteID.BG_2, 1, Color._17_DARK_GOLD);
                Palettes.LoadPalette(PaletteID.BG_2, 2, Color._16_RED_ORANGE);
                Palettes.LoadPalette(PaletteID.BG_2, 3, Color._26_LIGHT_ORANGE);
                Palettes.LoadPalette(PaletteID.BG_3, 1, Color._17_DARK_GOLD);
                Palettes.LoadPalette(PaletteID.BG_3, 2, Color._16_RED_ORANGE);
                Palettes.LoadPalette(PaletteID.BG_3, 3, Color._26_LIGHT_ORANGE);
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

                    // forces link animation to update
                    Link.animation_timer += 6;
                }
            }

            // death animation part 3: background fades to black
            else if (death_timer == 116)
            {
                Palettes.LoadPalette(PaletteID.BG_2, 1, Color._06_RED);
                Palettes.LoadPalette(PaletteID.BG_2, 2, Color._17_DARK_GOLD);
                Palettes.LoadPalette(PaletteID.BG_2, 3, Color._16_RED_ORANGE);
                Palettes.LoadPalette(PaletteID.BG_3, 1, Color._06_RED);
                Palettes.LoadPalette(PaletteID.BG_3, 2, Color._17_DARK_GOLD);
                Palettes.LoadPalette(PaletteID.BG_3, 3, Color._16_RED_ORANGE);
            }
            else if (death_timer == 125)
            {
                Palettes.LoadPalette(PaletteID.BG_2, 1, Color._07_BROWN);
                Palettes.LoadPalette(PaletteID.BG_2, 2, Color._06_RED);
                Palettes.LoadPalette(PaletteID.BG_3, 1, Color._07_BROWN);
                Palettes.LoadPalette(PaletteID.BG_3, 2, Color._06_RED);
            }
            else if (death_timer == 135)
            {
                Palettes.LoadPalette(PaletteID.BG_2, 1, Color._0F_BLACK);
                Palettes.LoadPalette(PaletteID.BG_2, 2, Color._07_BROWN);
                Palettes.LoadPalette(PaletteID.BG_2, 3, Color._06_RED);
                Palettes.LoadPalette(PaletteID.BG_3, 1, Color._0F_BLACK);
                Palettes.LoadPalette(PaletteID.BG_3, 2, Color._07_BROWN);
                Palettes.LoadPalette(PaletteID.BG_3, 3, Color._06_RED);
            }
            else if (death_timer == 145)
            {
                Palettes.LoadPalette(PaletteID.BG_2, 2, Color._0F_BLACK);
                Palettes.LoadPalette(PaletteID.BG_2, 3, Color._0F_BLACK);
                Palettes.LoadPalette(PaletteID.BG_3, 2, Color._0F_BLACK);
                Palettes.LoadPalette(PaletteID.BG_3, 3, Color._0F_BLACK);
            }

            // death animation part 4: link goes gray then explodes into a spark
            else if (death_timer == 146)
            {
                Palettes.LoadPalette(PaletteID.SP_0, 1, Color._10_GRAY);
                Palettes.LoadPalette(PaletteID.SP_0, 2, Color._30_WHITE);
                Palettes.LoadPalette(PaletteID.SP_0, 3, Color._00_DARK_GRAY);
            }
            else if (death_timer == 170)
            {
                Link.self.tile_index = (byte)SpriteID.BIG_SPARK_1;
                Link.counterpart.tile_index = (byte)SpriteID.BIG_SPARK_1;
                Link.self.palette_index = (byte)PaletteID.SP_1;
                Link.counterpart.palette_index = (byte)PaletteID.SP_1;
                Link.counterpart.xflip = true;
            }
            else if (death_timer == 180)
            {
                Link.self.tile_index = (byte)SpriteID.BIG_SPARK_2;
                Link.counterpart.tile_index = (byte)SpriteID.BIG_SPARK_2;
            }
            else if (death_timer == 184)
            {
                Link.Show(false);
            }

            // death animation part 5: gama ovar?!
            else if (death_timer == 230)
            {
                // load room 128 as background because it allows us to have a pitch black background and white text at the same time
                Textures.LoadPPUPage(Textures.PPUDataGroup.OVERWORLD, 128, 0);
                byte[] game_over_text = { 0x10, 0xa, 0x16, 0xe, 0x24, 0x18, 0x1f, 0xe, 0x1b };
                for (int i = 0; i < game_over_text.Length; i++)
                {
                    Textures.ppu[0x24c + i] = game_over_text[i];
                    Textures.ppu_plt[0x24c + i] = (byte)PaletteID.BG_0;
                }
            }
            else if (death_timer >= 330)
            {
                Screen.y_scroll = 8;
                Screen.sprites.Clear();
                Screen.sprites.Add(selector);
                Textures.LoadPPUPage(Textures.PPUDataGroup.OTHER, Textures.OtherPPUPages.GAME_OVER, 0);
                Sound.PlaySong(Sound.Songs.DEATH, true);
                select_menu = true;
                selector.use_chr_rom = true;
                return;
            }

            // link needs his logic to go forward for his animations and flashing to update
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
                if (selected_option > DeathMenuSelection.RETRY)
                    selected_option = DeathMenuSelection.CONTINUE;
            }

            selector.y = 48 + 16 * (int)selected_option;
        }

        // when you select an option, the text flashes for a bit before doing something
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
                    Textures.ppu_plt[0xcc + (int)selected_option * 64 + i] = new_palette;
                }
            }

            if (flash_timer <= 64)
            {
                return;
            }

            // code below executes once flashing is done
            Screen.sprites.Remove(selector);
            select_menu = false;
            flash_timer = 0;
            selector.y = 48;

            if (selected_option == DeathMenuSelection.CONTINUE)
            {
                if (died_in_dungeon)
                {
                    Program.gamemode = Program.Gamemode.DUNGEON;
                    DC.Init(DC.current_dungeon);
                }
                else
                {
                    OC.current_screen = OverworldCode.DEFAULT_SPAWN_ROOM;
                    Program.gamemode = Program.Gamemode.OVERWORLD;
                    Link.Init();
                    OC.Init();
                }

                return;
            }

            if (selected_option == DeathMenuSelection.SAVE)
            {
                SaveLoad.SaveFile(SaveLoad.current_save_file);
            }
            else if (selected_option == DeathMenuSelection.RETRY)
            {
                SaveLoad.LoadFile(SaveLoad.current_save_file);
            }

            Program.gamemode = Program.Gamemode.FILESELECT;
            FileSelectCode.InitFileSelect();

            return;
        }
    }
}