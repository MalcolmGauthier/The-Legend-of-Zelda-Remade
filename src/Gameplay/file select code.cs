using System.Runtime.CompilerServices;
using The_Legend_of_Zelda.Rendering;
using The_Legend_of_Zelda.Sprites;
using static The_Legend_of_Zelda.Rendering.Screen;
using static The_Legend_of_Zelda.Gameplay.Program;
namespace The_Legend_of_Zelda.Gameplay
{
    internal static class FileSelectCode
    {
        private enum FileSelectMode
        {
            FILESELECT,
            REGISTERYOURNAME,
            ELIMINATIONMODE
        }

        // order is critical. do not rearrange
        private enum Selection
        {
            FILE_1,
            FILE_2,
            FILE_3,
            REGISTER_OR_END,
            ELIMINATE
        }

        const byte EMPTY_LETTER = 0x24;
        const byte NAME_LENGTH = 8;

        static FileSelectMode mode = FileSelectMode.FILESELECT;
        static Selection selected_option = 0;
        static int selected_character = 0;
        static byte selected_name_letter = 0;
        static StaticSprite cursor = new(SpriteID.BLANK, PaletteID.SP_3, 40, 92);
        static StaticSprite[] link_icons = new StaticSprite[6];
        static StaticSprite[] quest_2_swords = new StaticSprite[3];

        static readonly byte[] selection_heart_positions = new byte[5] { 92, 116, 140, 168, 184 };
        static readonly byte[] register_name_text = new byte[18]
            { 0x1b, 0xe, 0x10, 0x12, 0x1c, 0x1d, 0xe, 0x1b, 0x24, 0x22, 0x18, 0x1e, 0x1b, 0x24, 0x17, 0xa, 0x18, 0xe }; // "REGISTER YOUR NAME"
        static readonly byte[] register_end_text = new byte[15]
            { 0x1b, 0xe, 0x10, 0x12, 0x1c, 0x1d, 0xe, 0x1b, 0x24, 0x24, 0x24, 0x24, 0xe, 0x17, 0xd }; // "REGISTER    END"
        static readonly byte[] elimination_mode_text = new byte[17]
            { 0xe, 0x15, 0x12, 0x16, 0x12, 0x17, 0xa, 0x1d, 0x12, 0x18, 0x17, 0x24, 0x24, 0x16, 0x18, 0xd, 0xe }; // "ELIMINATION  MODE"
        static readonly byte[] elimination_end_text = new byte[15]
            { 0xe, 0x15, 0x12, 0x16, 0x12, 0x17, 0xa, 0x1d, 0x12, 0x18, 0x17, 0x24, 0xe, 0x17, 0xd }; // "ELIMINATION END"
        static byte[,] file_new_names = new byte[3, NAME_LENGTH]
        {
            {0x24,0x24,0x24,0x24,0x24,0x24,0x24,0x24},
            {0x24,0x24,0x24,0x24,0x24,0x24,0x24,0x24},
            {0x24,0x24,0x24,0x24,0x24,0x24,0x24,0x24}
        };

        public static void InitFileSelect()
        {
            Palettes.background_color = Color._0F_BLACK;
            y_scroll = 0;
            sprites.Clear();
            Sound.PauseMusic();

            Palettes.LoadPaletteGroup(PaletteID.BG_0, Palettes.PaletteGroups.GRAVEYARD_HUD1);
            Palettes.LoadPaletteGroup(PaletteID.BG_1, Palettes.PaletteGroups.HUD2);
            // each save file icon uses a seperate palette to indicate its ring status, default is green
            Palettes.LoadPaletteGroup(PaletteID.SP_0, Palettes.PaletteGroups.GREEN_LINK_HUDSPR1);
            Palettes.LoadPaletteGroup(PaletteID.SP_1, Palettes.PaletteGroups.GREEN_LINK_HUDSPR1);
            Palettes.LoadPaletteGroup(PaletteID.SP_2, Palettes.PaletteGroups.GREEN_LINK_HUDSPR1);
            Palettes.LoadPalette(PaletteID.SP_3, 1, Color._15_ROSE);
            Palettes.LoadPalette(PaletteID.BG_2, 0, Color._15_ROSE);
            Palettes.LoadPalette(PaletteID.BG_2, 1, Color._30_WHITE);
            Palettes.LoadPalette(PaletteID.BG_3, 0, Color._0F_BLACK);
            Palettes.LoadPalette(PaletteID.BG_3, 1, Color._15_ROSE);
            Palettes.LoadPalette(PaletteID.BG_3, 2, Color._27_GOLD);
            Palettes.LoadPalette(PaletteID.BG_3, 3, Color._30_WHITE);

            Textures.LoadPPUPage(Textures.PPUDataGroup.OTHER, Textures.OtherPPUPages.FILE_SELECT, 0);
            for (byte i = 0; i < 3; i++)
            {
                SaveLoad.LoadFile(i);
                DrawFileInfo(i);
                if (SaveLoad.second_quest[i])
                {
                    sprites.Add(quest_2_swords[i] = new StaticSprite(SpriteID.SWORD, PaletteID.BG_3, 60, 86 + i * 24));
                    // in the real game, the swords aren't on background layer but are instead just lower priority than link icons,
                    // but i don't want two loops in this already ugly function, so this does the same thing. + there's no background to worry about
                    quest_2_swords[i].background = true;
                }
            }

            sprites.Add(cursor = new StaticSprite(SpriteID.BLANK, PaletteID.SP_3, 40, 92));

            for (int i = 0; i < link_icons.Length; i++)
            {
                SpriteID sprite = i % 2 == 0 ? SpriteID.LINK_DOWN_L : SpriteID.LINK_DOWN_R;
                PaletteID palette = (PaletteID)(i / 2 + 4);
                int x = i % 2 == 0 ? 48 : 56;
                int y = i / 2 * 24 + 88;

                sprites.Add(link_icons[i] = new StaticSprite(sprite, palette, x, y));
            }

            selected_option = 0;
            SkipOverFiles();
        }

        // initialization code for when switching to name registration mode
        static void InitRegisterName()
        {
            // color needs to be set back to pink when returning from elimination mode
            Palettes.LoadPalette(PaletteID.SP_3, 1, Color._15_ROSE);
            Textures.LoadPPUPage(Textures.PPUDataGroup.OTHER, Textures.OtherPPUPages.REGISTER_NAME, 0);

            // write text to screen
            for (int i = 0; i < register_name_text.Length; i++)
            {
                Textures.ppu[0x68 + i] = register_name_text[i];
            }
            for (int i = 0; i < register_end_text.Length; i++)
            {
                Textures.ppu[0x1ea + i] = register_end_text[i];
            }

            // move link icons
            for (int i = 0; i < link_icons.Length; i++)
            {
                link_icons[i].x = 80 + i % 2 * 8;
                link_icons[i].y = 48 + 24 * (i / 2);
            }

            // write file names
            for (int i = 0; i < 3; i++)
            {
                if (!SaveLoad.save_file_exists[i])
                    continue;

                for (int j = 0; j < NAME_LENGTH; j++)
                {
                    Textures.ppu[0xce + i * 0x60 + j] = SaveLoad.file_name[i, j];
                }
            }

            cursor.x = 67;
            cursor.y = 48;
            selected_option = Selection.FILE_1;
            selected_character = 0;
            SkipOverFiles(true);

            selected_name_letter = 0;

            // calling skipoverfiles(true) when all files exist will set cursor to first menu option
            // if every file exists, return
            // why would selected_name_letter ever need to be non 0 on registration init? even if we're not selecting a file, it doesn't matter.
            // make sure that removing this code is fine.

            if (selected_option == Selection.REGISTER_OR_END)
                return;

            // ??? when would this ever be true? if we're here, it's because selected option is on an empty file, and yet this checks for a full name
            if (file_new_names[(byte)selected_option, 7] != EMPTY_LETTER)
            {
                selected_name_letter = 0;
                return;
            }

            for (int i = 0; i < 7; i++)
            {
                selected_name_letter--;
                if (selected_name_letter == 255)
                {
                    selected_name_letter = 0;
                    break;
                }
                if (file_new_names[(byte)selected_option, selected_name_letter] != EMPTY_LETTER)
                {
                    selected_name_letter++;
                    break;
                }
            }
        }

        // draw the info for the files on loading file select mode
        static void DrawFileInfo(byte file_index)
        {
            if (!SaveLoad.save_file_exists[file_index])
                return;

            // retreive gameplay info from a specific file (this is the only time we need to do this)
            (bool blue_ring, bool red_ring, byte nb_of_hearts, byte death_count) file_info = SaveLoad.GetBasicFileInfo(file_index);

            // link icon color (ring status)
            if (file_info.red_ring)
                Palettes.LoadPalette(PaletteID.SP_0 + file_index, 1, Color._16_RED_ORANGE);
            else if (file_info.blue_ring)
                Palettes.LoadPalette(PaletteID.SP_0 + file_index, 1, Color._32_LIGHTER_INDIGO);

            // file name
            for (int i = 0; i < NAME_LENGTH; i++)
            {
                Textures.ppu[0x169 + file_index * 0x60 + i] = SaveLoad.file_name[file_index, i];
            }

            // death count
            if (file_info.death_count >= 100)
                Textures.ppu[0x189 + file_index * 0x60] = (byte)Math.Floor(file_info.death_count / 100.0);
            if (file_info.death_count >= 10)
                Textures.ppu[0x18a + file_index * 0x60] = (byte)(Math.Floor(file_info.death_count / 10.0) % 10);
            Textures.ppu[0x18b + file_index * 0x60] = (byte)(file_info.death_count % 10);

            // max hearts
            for (int i = 0; i < file_info.nb_of_hearts; i++)
            {
                int ppu_index = i >= 8 ? 0x172 - 8 : 0x192;
                Textures.ppu[0x192 + file_index * 0x60 + i] = 0xf2;
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
            if (Control.IsPressed(Buttons.START))
            {
                if (selected_option <= Selection.FILE_3)
                {
                    Textures.LoadPPUPage(Textures.PPUDataGroup.OTHER, Textures.OtherPPUPages.EMPTY, 0);
                    sprites.Clear();
                    Program.gamemode = Program.Gamemode.OVERWORLD;
                    SaveLoad.current_save_file = (byte)selected_option;
                    Link.Init();
                    Program.OC.Init();
                    return;
                }

                // this happens for either regis mode or elim mode, so this happens before checking which was chosen
                foreach (StaticSprite s in quest_2_swords)
                    sprites.Remove(s);

                if (selected_option == Selection.REGISTER_OR_END)
                {
                    InitRegisterName();
                    mode = FileSelectMode.REGISTERYOURNAME;
                    return;
                }

                // elimination mode chosen
                Textures.LoadPPUPage(Textures.PPUDataGroup.OTHER, Textures.OtherPPUPages.REGISTER_NAME, 0);
                Palettes.LoadPalette(PaletteID.SP_3, 1, Color._30_WHITE);
                for (int i = 0; i < link_icons.Length; i++)
                {
                    link_icons[i].x = 80 + i % 2 * 8;
                    link_icons[i].y = 48 + 24 * (i / 2);
                }

                cursor.x = 67;
                cursor.y = 48;
                selected_option = Selection.FILE_1;

                // write elimination mode text
                for (int i = 0; i < elimination_mode_text.Length; i++)
                {
                    Textures.ppu[0x68 + i] = elimination_mode_text[i];
                }
                for (int i = 0; i < elimination_end_text.Length; i++)
                {
                    Textures.ppu[0x1ea + i] = elimination_end_text[i];
                }

                // write file names to screen
                for (int i = 0; i < 3; i++)
                {
                    if (!SaveLoad.save_file_exists[i])
                        continue;

                    for (int j = 0; j < NAME_LENGTH; j++)
                    {
                        Textures.ppu[0xce + i * 0x60 + j] = SaveLoad.file_name[i, j];
                    }
                }

                mode = FileSelectMode.ELIMINATIONMODE;
                return;
            }

            // select last, meaning pressing start and select at same time means start takes priority.
            // to change this, move this if statement to the top
            if (Control.IsPressed(Buttons.SELECT))
            {
                // this might not be legal c#, check back later
                selected_option++;
                if (selected_option > Selection.ELIMINATE)
                    selected_option = Selection.FILE_1;
                SkipOverFiles();
                Sound.PlaySFX(Sound.SoundEffects.RUPEE, true);
            }

            cursor.y = selection_heart_positions[(byte)selected_option];
        }

        static void EliminationMode()
        {
            if (Control.IsPressed(Buttons.START))
            {
                if (selected_option == Selection.REGISTER_OR_END)
                {
                    mode = FileSelectMode.REGISTERYOURNAME;
                    InitRegisterName();
                    return;
                }

                // delete save file
                SaveLoad.save_file_exists[(byte)selected_option] = false;
                for (int i = 0; i < NAME_LENGTH; i++)
                {
                    Textures.ppu[0xce + (byte)selected_option * 0x60 + i] = EMPTY_LETTER;
                    SaveLoad.file_name[(byte)selected_option, i] = EMPTY_LETTER;
                    file_new_names[(byte)selected_option, i] = EMPTY_LETTER;
                }
                SaveLoad.DeleteData((byte)selected_option);
                Sound.PlaySFX(Sound.SoundEffects.HURT);
            }

            if (Control.IsPressed(Buttons.SELECT))
            {
                selected_option++;
                if (selected_option == Selection.ELIMINATE)
                    selected_option = Selection.FILE_1;

                Sound.PlaySFX(Sound.SoundEffects.RUPEE, true);
            }

            cursor.y = 48 + (byte)selected_option * 24;
        }

        static void RegistrationMode()
        {
            // end registration mode
            if (Control.IsPressed(Buttons.START) && selected_option == Selection.REGISTER_OR_END)
            {
                // for each file that did not exist (and thus had no name) but now DOES have a name, create it
                for (byte i = 0; i < 3; i++)
                {
                    if (SaveLoad.save_file_exists[i])
                        continue;

                    for (int j = 0; j < NAME_LENGTH; j++)
                    {
                        if (file_new_names[i, j] != EMPTY_LETTER)
                        {
                            CreateFile(i);
                            break;
                        }
                    }
                }

                // set blinking areas to black
                SetRegistrationChrBackground(selected_character, 0);
                Textures.ppu_plt[0xce + (byte)selected_option * 0x60 + selected_name_letter] = 0;
                InitFileSelect();
                mode = FileSelectMode.FILESELECT;
                return;
            }

            // move cursor
            if (Control.IsPressed(Buttons.SELECT))
            {
                Textures.ppu_plt[0xce + (byte)selected_option * 0x60 + selected_name_letter] = 0;
                selected_option++;
                if (selected_option > Selection.REGISTER_OR_END)
                    selected_option = Selection.FILE_1;

                SkipOverFiles(true);
                Sound.PlaySFX(Sound.SoundEffects.RUPEE, true);

                // code below only applies to making the cursor go onto one of the files
                if (selected_option == Selection.REGISTER_OR_END)
                    return;

                // this algorithm sets the cursor to be 1 after the last non-blank character in the name, but sets the cursor to 0 if the last character is non-blank.
                // this checks letter by letter going from last to first, and when it finds a non-blank letter, it increments the counter by one and does mod 8.
                // the mod 8 makes it so that a full name will select the first character, and setting the value to -1 beforehand makes it so that if the name
                // is completely empty, the loop won't do anything and the increment will overflow the value to 0, which is where we want it to be.
                selected_name_letter = byte.MaxValue;
                for (int i = NAME_LENGTH - 1; i >= 0; i--)
                {
                    if (file_new_names[(int)selected_option, i] == EMPTY_LETTER)
                        continue;

                    selected_name_letter = (byte)i;
                    break;
                }
                selected_name_letter++;
                selected_name_letter %= NAME_LENGTH;

                return;
            }

            cursor.y = 48 + (byte)selected_option * 24;

            if (Program.gTimer % 8 == 0)
            {
                // cast needed to prevent error. why tf does c# throw an error about implicitly casting 0 or 2 to a byte??
                byte new_plt = Program.gTimer % 16 == 0 ? (byte)2 : (byte)0;

                if (selected_option != Selection.REGISTER_OR_END)
                    Textures.ppu_plt[0xce + (byte)selected_option * 0x60 + selected_name_letter] = new_plt;

                SetRegistrationChrBackground(selected_character, new_plt);
            }

            // move character selector
            if (Control.IsPressed(Buttons.UP) || Control.IsPressed(Buttons.DOWN) || Control.IsPressed(Buttons.LEFT) || Control.IsPressed(Buttons.RIGHT))
            {
                SetRegistrationChrBackground(selected_character, 0);
                if (Control.IsPressed(Buttons.UP))
                {
                    selected_character -= 11;
                    if (selected_character < 0)
                        selected_character += 44;
                }
                if (Control.IsPressed(Buttons.DOWN))
                {
                    selected_character += 11;
                    if (selected_character > 43)
                        selected_character -= 44;
                }
                if (Control.IsPressed(Buttons.LEFT))
                {
                    selected_character--;
                    if (selected_character < 0)
                        selected_character = 43;
                }
                if (Control.IsPressed(Buttons.RIGHT))
                {
                    selected_character++;
                    if (selected_character > 43)
                        selected_character = 0;
                }
                SetRegistrationChrBackground(selected_character, 2);
                Sound.PlaySFX(Sound.SoundEffects.RUPEE, true);
            }

            if ((Control.IsPressed(Buttons.A) || Control.IsPressed(Buttons.B)) && selected_option != Selection.REGISTER_OR_END)
            {
                Textures.ppu_plt[0xce + (byte)selected_option * 0x60 + selected_name_letter] = 0;

                if (Control.IsPressed(Buttons.A))
                {
                    byte selected_letter = SetRegistrationChrBackground(selected_character, -1);
                    Textures.ppu[0xce + (byte)selected_option * 0x60 + selected_name_letter] = selected_letter;
                    file_new_names[(byte)selected_option, selected_name_letter] = selected_letter;
                }

                selected_name_letter++;
                selected_name_letter %= NAME_LENGTH;
            }
        }

        // this function moves the cursor (selected_option) to the next file that exists. (or that doesn't exist, if the condition is set to true)
        // if none of the files meet this condition, the cursor goes to the menu below the files
        static void SkipOverFiles(bool skip_over_existing_files = false)
        {
            for (Selection i = 0; i <= Selection.FILE_3; i++)
            {
                if (selected_option == i && SaveLoad.save_file_exists[(int)i] == skip_over_existing_files)
                {
                    selected_option++;
                    continue;
                }

                break;
            }
        }

        static void CreateFile(byte file_index)
        {
            SaveLoad.DeleteData(file_index);

            SaveLoad.save_file_exists[file_index] = true;
            for (int i = 0; i < NAME_LENGTH; i++)
            {
                SaveLoad.file_name[file_index, i] = file_new_names[file_index, i];
            }

            // if file name is exactly "ZELDA   ", then second quest is activated
            byte[] second_quest_test = new byte[8] { 0x23, 0xe, 0x15, 0xd, 0xa, 0x24, 0x24, 0x24 }; //"ZELDA   "
            byte index = 0;
            foreach (byte b in second_quest_test)
            {
                if (SaveLoad.file_name[file_index, index] != b)
                    break;

                index++;
                if (index == second_quest_test.Length)
                    SaveLoad.second_quest[file_index] = true;
            }

            SaveLoad.SaveFile(file_index);
        }

        // sets character's background to certain palette. returns character at index, so set palette to invalid value to not change palette.
        static byte SetRegistrationChrBackground(int selected_character, int palette)
        {
            int index = 0x226 + selected_character % 11 * 2 + selected_character / 11 * 0x40;
            if (palette >= 0 && palette <= 7)
                Textures.ppu_plt[index] = (byte)palette;
            return Textures.ppu[index];
        }
    }
}