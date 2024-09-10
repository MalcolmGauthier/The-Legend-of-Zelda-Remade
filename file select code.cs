using static The_Legend_of_Zelda.Screen;
using static The_Legend_of_Zelda.SaveLoad;
namespace The_Legend_of_Zelda
{
    internal static class FileSelectCode
    {
        public static FileSelectMode mode = FileSelectMode.FILESELECT;
        static byte selected_option = 0;
        static sbyte selected_character = 0;
        static byte selected_name_letter = 0;
        static readonly byte[] selection_heart_positions = new byte[5] { 92, 116, 140, 168, 184 };
        static readonly byte[] register_name_text = new byte[18] { 0x1b, 0xe, 0x10, 0x12, 0x1c, 0x1d, 0xe, 0x1b, 0x24, 0x22, 0x18, 0x1e, 0x1b, 0x24, 0x17, 0xa, 0x18, 0xe };
        static readonly byte[] elimination_mode_text = new byte[17] { 0xe, 0x15, 0x12, 0x16, 0x12, 0x17, 0xa, 0x1d, 0x12, 0x18, 0x17, 0x24, 0x24, 0x16, 0x18, 0xd, 0xe };
        static byte[,] file_new_names = new byte[3, 8]
        {
            {0x24,0x24,0x24,0x24,0x24,0x24,0x24,0x24},
            {0x24,0x24,0x24,0x24,0x24,0x24,0x24,0x24},
            {0x24,0x24,0x24,0x24,0x24,0x24,0x24,0x24}
        };
        public enum FileSelectMode
        {
            FILESELECT,
            REGISTERYOURNAME,
            ELIMINATIONMODE
        }
        public static void InitFileSelect()
        {
            Palettes.background_color = 0x0f;
            y_scroll = 0;
            sprites.Clear();
            Sound.PauseMusic();
            Palettes.LoadPaletteGroup(Palettes.PaletteID.BG_0, Palettes.PaletteGroups.GRAVEYARD_HUD1);
            Palettes.LoadPaletteGroup(Palettes.PaletteID.BG_1, Palettes.PaletteGroups.HUD2);
            Palettes.LoadPaletteGroup(Palettes.PaletteID.SP_0, Palettes.PaletteGroups.GREEN_LINK_HUDSPR1);
            Palettes.LoadPaletteGroup(Palettes.PaletteID.SP_1, Palettes.PaletteGroups.GREEN_LINK_HUDSPR1);
            Palettes.LoadPaletteGroup(Palettes.PaletteID.SP_2, Palettes.PaletteGroups.GREEN_LINK_HUDSPR1);
            Palettes.LoadPalette(7, 1, 0x15);
            Palettes.LoadPalette(2, 0, 0x15);
            Palettes.LoadPalette(2, 1, 0x30);
            Palettes.LoadPalette(3, 0, 0x0f);
            Palettes.LoadPalette(3, 1, 0x15);
            Palettes.LoadPalette(3, 2, 0x27);
            Palettes.LoadPalette(3, 3, 0x30);
            Textures.LoadPPUPage(Textures.PPUDataGroup.OTHER, 9, 0);
            for (byte i = 0; i < 3; i++)
            {
                LoadFile(i);
                DrawFileInfo(i);
            }
            Palettes.LoadPalette(7, 1, 0x15);
            sprites.Add(new StaticSprite(0x1c, 7, 40, 92));
            for (int i = 0; i < 3; i++)
            {
                if (second_quest[i])
                {
                    sprites.Add(new StaticSprite(0x20, 3, 60, (short)(86 + i * 24)));
                }
            }
            sprites.Add(new StaticSprite(0x08, 4, 48, 88));
            sprites.Add(new StaticSprite(0x0a, 4, 56, 88));
            sprites.Add(new StaticSprite(0x08, 5, 48, 112));
            sprites.Add(new StaticSprite(0x0a, 5, 56, 112));
            sprites.Add(new StaticSprite(0x08, 6, 48, 136));
            sprites.Add(new StaticSprite(0x0a, 6, 56, 136));
            selected_option = 0;
            SkipOverEmptyFiles();
        }
        static void InitRegisterName()
        {
            Palettes.LoadPalette(7, 1, 0x15);
            Textures.LoadPPUPage(Textures.PPUDataGroup.OTHER, 10, 0);
            for (int i = 0; i < register_name_text.Length; i++)
            {
                Textures.ppu[0x68 + i] = register_name_text[i];
                if (i < 8)
                    Textures.ppu[0x1ea + i] = register_name_text[i];
            }
            for (int i = 1; i < 7; i++)
            {
                sprites[i].x = (short)(80 + (((i - 1) % 2) * 8));
                sprites[i].y = (short)(48 + 24 * (int)Math.Floor((i - 1) / 2.0));
            }
            for (int i = 0; i < 3; i++)
            {
                if (!save_file_exists[i])
                    continue;
                for (int j = 0; j < 8; j++)
                {
                    Textures.ppu[0xce + i * 0x60 + j] = file_name[i, j];
                }
            }
            sprites[0].x = 67;
            sprites[0].y = 48;
            selected_option = 0;
            selected_character = 0;
            SkipOverEmptyFiles(true);
            if (selected_option != 3)
            {
                if (file_new_names[selected_option, 7] != 0x24)
                {
                    selected_name_letter = 0;
                }
                else
                {
                    for (int i = 0; i < 7; i++)
                    {
                        selected_name_letter--;
                        if (selected_name_letter == 255)
                        {
                            selected_name_letter = 0;
                            break;
                        }
                        if (file_new_names[selected_option, selected_name_letter] != 0x24)
                        {
                            selected_name_letter++;
                            break;
                        }
                    }
                }
            }
        }
        static void DrawFileInfo(byte file_index)
        {
            if (!save_file_exists[file_index])
                return;

            if (red_ring[file_index])
                Palettes.LoadPalette((byte)(4 + file_index), 1, 0x16);
            else if (blue_ring[file_index])
                Palettes.LoadPalette((byte)(4 + file_index), 1, 0x32);

            for (int i = 0; i < 8; i++)
            {
                Textures.ppu[0x169 + file_index * 0x60 + i] = file_name[file_index, i];
            }

            if (death_count[file_index] >= 100)
                Textures.ppu[0x189 + file_index * 0x60] = (byte)Math.Floor(death_count[file_index] / 100.0);
            if (death_count[file_index] >= 10)
                Textures.ppu[0x18a + file_index * 0x60] = (byte)(Math.Floor(death_count[file_index] / 10.0) % 10);
            Textures.ppu[0x18b + file_index * 0x60] = (byte)(death_count[file_index] % 10);

            if (nb_of_hearts[file_index] > 8)
            {
                for (int i = 0; i < nb_of_hearts[file_index] - 8; i++)
                {
                    Textures.ppu[0x172 + file_index * 0x60 + i] = 0xf2;
                }
            }
            for (int i = 0; i < nb_of_hearts[file_index]; i++)
            {
                Textures.ppu[0x192 + file_index * 0x60 + i] = 0xf2;
                if (i == 7)
                    break;
            }
        }
        public static void Tick()
        {
            switch (mode)
            {
                case FileSelectMode.FILESELECT:
                    FileSelect();
                    break;
                case FileSelectMode.REGISTERYOURNAME:
                    RegistrationMode();
                    break;
                case FileSelectMode.ELIMINATIONMODE:
                    EliminationMode();
                    break;
            }
        }
        static void FileSelect()
        {
            if (Control.IsPressed(Control.Buttons.START))
            {
                if (selected_option < 3)
                {
                    Textures.LoadPPUPage(Textures.PPUDataGroup.OTHER, 1, 0);
                    sprites.Clear();
                    Program.gamemode = Program.Gamemode.OVERWORLD;
                    current_save_file = selected_option;
                    Link.Init();
                    OverworldCode.Init();
                    return;
                }
                else if (selected_option == 3)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        if (second_quest[i])
                        {
                            sprites.RemoveAt(1);
                        }
                    }
                    InitRegisterName();
                    mode = FileSelectMode.REGISTERYOURNAME;
                    return;
                }
                else
                {
                    for (int i = 0; i < 3; i++)
                    {
                        if (second_quest[i])
                        {
                            sprites.RemoveAt(1);
                        }
                    }
                    Textures.LoadPPUPage(Textures.PPUDataGroup.OTHER, 10, 0);
                    Palettes.LoadPalette(7, 1, 0x30);
                    for (int i = 1; i < 7; i++)
                    {
                        sprites[i].x = (short)(80 + (((i - 1) % 2) * 8));
                        sprites[i].y = (short)(48 + 24 * (int)Math.Floor((i - 1) / 2.0));
                    }
                    sprites[0].x = 67;
                    sprites[0].y = 48;
                    selected_option = 0;
                    for (int i = 0; i < elimination_mode_text.Length; i++)
                    {
                        Textures.ppu[0x68 + i] = elimination_mode_text[i];
                        if (i < 11)
                            Textures.ppu[0x1ea + i] = elimination_mode_text[i];
                    }
                    for (int i = 0; i < 3; i++)
                    {
                        if (!save_file_exists[i])
                            continue;
                        for (int j = 0; j < 8; j++)
                        {
                            Textures.ppu[0xce + i * 0x60 + j] = file_name[i, j];
                        }
                    }
                    mode = FileSelectMode.ELIMINATIONMODE;
                    return;
                }
            }

            if (Control.IsPressed(Control.Buttons.SELECT))
            {
                selected_option++;
                if (selected_option == 5)
                    selected_option = 0;
                SkipOverEmptyFiles();
                Sound.PlaySFX(Sound.SoundEffects.RUPEE, true);
            }
            sprites[0].y = selection_heart_positions[selected_option];
        }
        static void EliminationMode()
        {
            if (Control.IsPressed(Control.Buttons.START))
            {
                if (selected_option < 3)
                {
                    save_file_exists[selected_option] = false;
                    for (int i = 0; i < 8; i++)
                    {
                        Textures.ppu[0xce + selected_option * 0x60 + i] = 0x24;
                        file_name[selected_option, i] = 0x24;
                        file_new_names[selected_option, i] = 0x24;
                    }
                    save_file_exists[selected_option] = false;
                    second_quest[selected_option] = false;
                    DeleteData(selected_option);
                    Sound.PlaySFX(Sound.SoundEffects.HURT);
                }
                else
                {
                    mode = FileSelectMode.REGISTERYOURNAME;
                    InitRegisterName();
                    return;
                }
            }
            if (Control.IsPressed(Control.Buttons.SELECT))
            {
                selected_option++;
                if (selected_option == 4)
                    selected_option = 0;
                Sound.PlaySFX(Sound.SoundEffects.RUPEE, true);
            }
            sprites[0].y = (short)(48 + selected_option * 24);
        }
        static void RegistrationMode()
        {
            if (Control.IsPressed(Control.Buttons.START) && (selected_option == 3))
            {
                for (byte i = 0; i < 3; i++)
                {
                    if (!save_file_exists[i])
                    {
                        for (int j = 0; j < 8; j++)
                        {
                            if (file_new_names[i, j] != 0x24)
                            {
                                CreateFile(i);
                                SaveFile(i);
                                break;
                            }
                        }
                    }
                }
                Textures.ppu_plt[0x226 + (selected_character % 11) * 2 + ((int)Math.Floor(selected_character / 11.0) * 0x40)] = 0;
                Textures.ppu_plt[0xce + selected_option * 0x60 + selected_name_letter] = 0;
                InitFileSelect();
                mode = FileSelectMode.FILESELECT;
            }
            if (Control.IsPressed(Control.Buttons.SELECT))
            {
                Textures.ppu_plt[0xce + selected_option * 0x60 + selected_name_letter] = 0;
                selected_option++;
                if (selected_option == 4)
                    selected_option = 0;
                selected_name_letter = 7;
                if (selected_option != 3)
                {
                    if (file_new_names[selected_option, 7] != 0x24)
                    {
                        selected_name_letter = 0;
                    }
                    else
                    {
                        for (int i = 0; i < 7; i++)
                        {
                            selected_name_letter--;
                            if (file_new_names[selected_option, selected_name_letter] != 0x24)
                            {
                                selected_name_letter++;
                                break;
                            }
                        }
                    }
                }
                SkipOverEmptyFiles(true);
                Sound.PlaySFX(Sound.SoundEffects.RUPEE, true);
            }
            sprites[0].y = (short)(48 + selected_option * 24);
            if (Program.gTimer % 16 == 0)
            {
                if (selected_option != 3)
                    Textures.ppu_plt[0xce + selected_option * 0x60 + selected_name_letter] = 2;
                Textures.ppu_plt[0x226 + (selected_character % 11) * 2 + ((int)Math.Floor(selected_character / 11.0) * 0x40)] = 2;
            }
            else if (Program.gTimer % 16 == 8)
            {
                if (selected_option != 3)
                    Textures.ppu_plt[0xce + selected_option * 0x60 + selected_name_letter] = 0;
                Textures.ppu_plt[0x226 + (selected_character % 11) * 2 + ((int)Math.Floor(selected_character / 11.0) * 0x40)] = 0;
            }

            if (Control.IsPressed(Control.Buttons.UP) || Control.IsPressed(Control.Buttons.DOWN) || Control.IsPressed(Control.Buttons.LEFT) || Control.IsPressed(Control.Buttons.RIGHT))
            {
                Textures.ppu_plt[0x226 + (selected_character % 11) * 2 + ((int)Math.Floor(selected_character / 11.0) * 0x40)] = 0;
                if (Control.IsPressed(Control.Buttons.UP))
                {
                    selected_character -= 11;
                    if (selected_character < 0)
                        selected_character += 44;
                }
                if (Control.IsPressed(Control.Buttons.DOWN))
                {
                    selected_character += 11;
                    if (selected_character > 43)
                        selected_character -= 44;
                }
                if (Control.IsPressed(Control.Buttons.LEFT))
                {
                    selected_character--;
                    if (selected_character < 0)
                        selected_character = 43;
                }
                if (Control.IsPressed(Control.Buttons.RIGHT))
                {
                    selected_character++;
                    if (selected_character > 43)
                        selected_character = 0;
                }
                Textures.ppu_plt[0x226 + (selected_character % 11) * 2 + ((int)Math.Floor(selected_character / 11.0) * 0x40)] = 2;
                Sound.PlaySFX(Sound.SoundEffects.RUPEE, true);
            }

            if (Control.IsPressed(Control.Buttons.A))
            {
                if (selected_option != 3)
                {
                    Textures.ppu_plt[0xce + selected_option * 0x60 + selected_name_letter] = 0;
                    byte selected_letter = Textures.ppu[0x226 + (selected_character % 11) * 2 + ((int)Math.Floor(selected_character / 11.0) * 0x40)];
                    Textures.ppu[0xce + selected_option * 0x60 + selected_name_letter] = selected_letter;
                    file_new_names[selected_option, selected_name_letter] = selected_letter;
                    selected_name_letter++;
                    if (selected_name_letter > 7)
                        selected_name_letter = 0;
                }
            }

            if (Control.IsPressed(Control.Buttons.B))
            {
                if (selected_option != 3)
                {
                    Textures.ppu_plt[0xce + selected_option * 0x60 + selected_name_letter] = 0;
                    selected_name_letter++;
                    if (selected_name_letter > 7)
                        selected_name_letter = 0;
                }
            }
        }
        static void SkipOverEmptyFiles(bool opposite = false)
        {
            if (opposite)
            {
                if ((selected_option == 0) && (save_file_exists[0]))
                    selected_option++;
                if ((selected_option == 1) && (save_file_exists[1]))
                    selected_option++;
                if ((selected_option == 2) && (save_file_exists[2]))
                    selected_option++;
            }
            else
            {
                if ((selected_option == 0) && (!save_file_exists[0]))
                    selected_option++;
                if ((selected_option == 1) && (!save_file_exists[1]))
                    selected_option++;
                if ((selected_option == 2) && (!save_file_exists[2]))
                    selected_option++;
            }
        }
        static void CreateFile(byte file_index)
        {
            save_file_exists[file_index] = true;
            for (int i = 0; i < 8; i++)
            {
                file_name[file_index, i] = file_new_names[file_index, i];
            }

            byte[] second_quest_test = new byte[8] { 0x23, 0xe, 0x15, 0xd, 0xa, 0x24, 0x24, 0x24 };
            for (int i = 0; i < 8; i++)
            {
                if (file_name[file_index, i] == second_quest_test[i])
                {
                    if (i == 7)
                    {
                        second_quest[file_index] = true;
                    }
                }
                else
                {
                    second_quest[file_index] = false;
                    break;
                }
            }

            arrow[file_index] = false;
            bait[file_index] = false;
            blue_candle[file_index] = false;
            blue_potion[file_index] = false;
            blue_ring[file_index] = false;
            bomb_count[file_index] = 0;
            bomb_limit[file_index] = 8;
            book_of_magic[file_index] = false;
            boomerang[file_index] = false;
            bow[file_index] = false;
            death_count[file_index] = 0;
            key_count[file_index] = 0;
            ladder[file_index] = false;
            letter[file_index] = false;
            magical_boomerang[file_index] = false;
            magical_key[file_index] = false;
            magical_rod[file_index] = false;
            magical_sword[file_index] = false;
            nb_of_hearts[file_index] = 3;
            power_bracelet[file_index] = false;
            raft[file_index] = false;
            recorder[file_index] = false;
            red_candle[file_index] = false;
            red_potion[file_index] = false;
            red_ring[file_index] = false;
            silver_arrow[file_index] = false;
            triforce_of_power[file_index] = false;
            triforce_pieces[file_index] = 0;
            white_sword[file_index] = false;
            wooden_sword[file_index] = false;
            bombed_holes_flags[file_index] = 0;
            boss_kills_flags[file_index] = 0;
            compass_flags[file_index] = 0;
            gift_flags[file_index] = 0;
            map_flags[file_index] = 0;
            opened_key_doors_flags[file_index] = 0;
        }
    }
}