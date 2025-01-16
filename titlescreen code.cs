﻿using static The_Legend_of_Zelda.Program;
using static The_Legend_of_Zelda.Screen;
namespace The_Legend_of_Zelda
{
    internal static class TitlescreenCode
    {
        static byte intro_scroll_counter = 0;

        public static void Tick()
        {
            Triforce_shine();
            Fadeout();
            Intro();
            Sprites();
            Startgame();
        }

        static void Triforce_shine()
        {
            if (gTimer >= 510)
                return;

            if (gTimer % 56 == 6)
            {
                Palettes.LoadPalette(PaletteID.BG_1, 2, Color._37_BEIGE);
            }
            else if ((gTimer % 56 == 0) || (gTimer % 56 == 18))
            {
                Palettes.LoadPalette(PaletteID.BG_1, 2, Color._27_GOLD);
            }
            else if ((gTimer % 56 == 24) || (gTimer % 56 == 40))
            {
                Palettes.LoadPalette(PaletteID.BG_1, 2, Color._17_DARK_GOLD);
            }
            else if (gTimer % 56 == 30)
            {
                Palettes.LoadPalette(PaletteID.BG_1, 2, Color._07_BROWN);
            }
        }

        static void Fadeout()
        {
            if (gTimer == 1)
                Palettes.background_color = Color._36_LIGHTER_ORANGE;

            if (gTimer < 510 || gTimer > 745)
                return;

            switch (gTimer)
            {
                case 510:
                    Palettes.background_color = Color._39_LIGHTER_GREEN;
                    break;
                case 518:
                    Palettes.background_color = Color._31_LIGHTER_BLUE;
                    break;
                case 524:
                    Palettes.background_color = Color._3C_LIGHT_CYAN;
                    break;
                case 530:
                    Palettes.background_color = Color._3B_LIGHT_TURQUOISE;
                    Palettes.LoadPalette(PaletteID.BG_3, 1, Color._10_GRAY);
                    Palettes.LoadPalette(PaletteID.SP_3, 1, Color._10_GRAY);
                    break;
                case 534:
                    Palettes.background_color = Color._2C_CYAN;
                    break;
                case 537:
                    Palettes.background_color = Color._1C_DARK_CYAN;
                    break;
                case 539:
                    Palettes.background_color = Color._02_BLUE;
                    Palettes.LoadPalette(PaletteID.BG_1, 1, Color._06_RED);
                    Palettes.LoadPalette(PaletteID.BG_2, 1, Color._0A_GREEN);
                    Palettes.LoadPalette(PaletteID.BG_2, 2, Color._1A_SEMI_LIGHT_GREEN);
                    Palettes.LoadPalette(PaletteID.BG_2, 2, Color._18_OLIVE);
                    Palettes.LoadPalette(PaletteID.BG_3, 2, Color._2B_TURQUOISE);
                    Palettes.LoadPalette(PaletteID.BG_3, 3, Color._12_SMEI_DARK_BLUE);

                    Palettes.LoadPalette(PaletteID.SP_2, 1, Color._0A_GREEN);
                    Palettes.LoadPalette(PaletteID.SP_2, 2, Color._1A_SEMI_LIGHT_GREEN);
                    Palettes.LoadPalette(PaletteID.SP_2, 3, Color._18_OLIVE);
                    Palettes.LoadPalette(PaletteID.SP_3, 2, Color._2B_TURQUOISE);
                    Palettes.LoadPalette(PaletteID.SP_3, 3, Color._12_SMEI_DARK_BLUE);
                    break;
                case 541:
                    Palettes.background_color = Color._0C_AQUA;
                    Palettes.LoadPalette(PaletteID.BG_1, 1, Color._03_DARK_INDIGO);
                    Palettes.LoadPalette(PaletteID.BG_1, 2, Color._16_RED_ORANGE);
                    Palettes.LoadPalette(PaletteID.BG_2, 1, Color._01_DARK_BLUE);
                    Palettes.LoadPalette(PaletteID.BG_2, 2, Color._01_DARK_BLUE);
                    Palettes.LoadPalette(PaletteID.BG_2, 3, Color._08_DARK_YELLOW);
                    Palettes.LoadPalette(PaletteID.BG_3, 1, Color._00_DARK_GRAY);
                    Palettes.LoadPalette(PaletteID.BG_3, 2, Color._1B_EVERGREEN);
                    Palettes.LoadPalette(PaletteID.BG_3, 3, Color._02_BLUE);

                    Palettes.LoadPalette(PaletteID.SP_2, 1, Color._01_DARK_BLUE);
                    Palettes.LoadPalette(PaletteID.SP_2, 2, Color._01_DARK_BLUE);
                    Palettes.LoadPalette(PaletteID.SP_2, 3, Color._08_DARK_YELLOW);
                    Palettes.LoadPalette(PaletteID.SP_3, 1, Color._00_DARK_GRAY);
                    Palettes.LoadPalette(PaletteID.SP_3, 2, Color._1B_EVERGREEN);
                    Palettes.LoadPalette(PaletteID.SP_3, 3, Color._02_BLUE);
                    break;
                case 543:
                    Palettes.background_color = Color._0F_BLACK;
                    Palettes.LoadPalette(PaletteID.BG_0, 1, Color._0F_BLACK);
                    Palettes.LoadPalette(PaletteID.BG_0, 2, Color._0F_BLACK);
                    Palettes.LoadPalette(PaletteID.BG_0, 3, Color._00_DARK_GRAY);
                    Palettes.LoadPalette(PaletteID.BG_1, 1, Color._01_DARK_BLUE);
                    Palettes.LoadPalette(PaletteID.BG_1, 2, Color._11_SEMI_LIGHT_BLUE);
                    Palettes.LoadPalette(PaletteID.BG_2, 1, Color._0C_AQUA);
                    Palettes.LoadPalette(PaletteID.BG_2, 2, Color._01_DARK_BLUE);
                    Palettes.LoadPalette(PaletteID.BG_2, 3, Color._02_BLUE);
                    Palettes.LoadPalette(PaletteID.BG_3, 1, Color._00_DARK_GRAY);
                    Palettes.LoadPalette(PaletteID.BG_3, 2, Color._01_DARK_BLUE);
                    Palettes.LoadPalette(PaletteID.BG_3, 3, Color._0C_AQUA);

                    Palettes.LoadPalette(PaletteID.SP_2, 1, Color._0C_AQUA);
                    Palettes.LoadPalette(PaletteID.SP_2, 2, Color._01_DARK_BLUE);
                    Palettes.LoadPalette(PaletteID.SP_2, 3, Color._02_BLUE);
                    Palettes.LoadPalette(PaletteID.SP_3, 1, Color._00_DARK_GRAY);
                    Palettes.LoadPalette(PaletteID.SP_3, 2, Color._01_DARK_BLUE);
                    Palettes.LoadPalette(PaletteID.SP_3, 3, Color._0C_AQUA);
                    break;
                case 735:
                    Palettes.LoadPalette(PaletteID.BG_2, 1, Color._0F_BLACK);
                    Palettes.LoadPalette(PaletteID.BG_2, 2, Color._0C_AQUA);
                    Palettes.LoadPalette(PaletteID.BG_2, 3, Color._01_DARK_BLUE);
                    Palettes.LoadPalette(PaletteID.BG_3, 1, Color._01_DARK_BLUE);
                    Palettes.LoadPalette(PaletteID.BG_3, 2, Color._0C_AQUA);
                    Palettes.LoadPalette(PaletteID.BG_3, 3, Color._0F_BLACK);

                    Palettes.LoadPalette(PaletteID.SP_2, 1, Color._0F_BLACK);
                    Palettes.LoadPalette(PaletteID.SP_2, 2, Color._0C_AQUA);
                    Palettes.LoadPalette(PaletteID.SP_2, 3, Color._01_DARK_BLUE);
                    Palettes.LoadPalette(PaletteID.SP_3, 1, Color._01_DARK_BLUE);
                    Palettes.LoadPalette(PaletteID.SP_3, 2, Color._0C_AQUA);
                    Palettes.LoadPalette(PaletteID.SP_3, 3, Color._0F_BLACK);
                    break;
                case 741:
                    Palettes.LoadPalette(PaletteID.BG_0, 3, Color._0F_BLACK);
                    Palettes.LoadPalette(PaletteID.BG_1, 1, Color._0F_BLACK);
                    Palettes.LoadPalette(PaletteID.BG_1, 2, Color._01_DARK_BLUE);
                    Palettes.LoadPalette(PaletteID.BG_2, 2, Color._0F_BLACK);
                    Palettes.LoadPalette(PaletteID.BG_2, 3, Color._0C_AQUA);
                    Palettes.LoadPalette(PaletteID.BG_3, 1, Color._0C_AQUA);
                    Palettes.LoadPalette(PaletteID.BG_3, 2, Color._0F_BLACK);

                    Palettes.LoadPalette(PaletteID.SP_2, 2, Color._0F_BLACK);
                    Palettes.LoadPalette(PaletteID.SP_2, 3, Color._0C_AQUA);
                    Palettes.LoadPalette(PaletteID.SP_3, 1, Color._0C_AQUA);
                    Palettes.LoadPalette(PaletteID.SP_3, 2, Color._0F_BLACK);
                    break;
                case 745:
                    Palettes.LoadPalette(PaletteID.BG_1, 2, Color._0F_BLACK);
                    Palettes.LoadPalette(PaletteID.BG_2, 3, Color._0F_BLACK);
                    Palettes.LoadPalette(PaletteID.BG_3, 1, Color._0F_BLACK);

                    Palettes.LoadPalette(PaletteID.SP_2, 3, Color._0F_BLACK);
                    Palettes.LoadPalette(PaletteID.SP_3, 1, Color._0F_BLACK);
                    break;
            }
        }

        static void Intro()
        {
            if (gTimer < 745)
                return;

            switch (gTimer)
            {
                case 775:
                    Palettes.LoadPalette(PaletteID.BG_0, 1, Color._30_WHITE);
                    Palettes.LoadPalette(PaletteID.BG_1, 1, Color._21_LIGHT_BLUE);
                    Palettes.LoadPalette(PaletteID.BG_2, 1, Color._16_RED_ORANGE);
                    Palettes.LoadPalette(PaletteID.BG_3, 1, Color._29_LIGHT_GREEN);
                    Palettes.LoadPalette(PaletteID.BG_3, 2, Color._1A_SEMI_LIGHT_GREEN);
                    Palettes.LoadPalette(PaletteID.BG_3, 3, Color._09_DARK_GREEN);
                    Textures.LoadPPUPage(Textures.PPUDataGroup.OTHER, 1, 0);
                    Textures.LoadPPUPage(Textures.PPUDataGroup.OTHER, 2, 1);
                    break;
                case 1425:
                    Textures.LoadPPUPage(Textures.PPUDataGroup.OTHER, 2, 0);
                    break;
                case 1684:
                    y_scroll = 0;
                    break;
            }

            if (!Sound.IsMusicPlaying() && !mute_sound)
            {
                y_scroll = 0;
                Textures.LoadPPUPage(Textures.PPUDataGroup.OTHER, 0, 0);
                Palettes.LoadPaletteGroup(PaletteID.BG_0, Palettes.PaletteGroups.TITLESCREEN_1);
                Palettes.LoadPaletteGroup(PaletteID.BG_1, Palettes.PaletteGroups.TITLESCREEN_2);
                Palettes.LoadPaletteGroup(PaletteID.BG_2, Palettes.PaletteGroups.TITLESCREEN_3);
                Palettes.LoadPaletteGroup(PaletteID.BG_3, Palettes.PaletteGroups.TITLESCREEN_4);
                Palettes.LoadPaletteGroup(PaletteID.SP_1, Palettes.PaletteGroups.TITLESCREEN_2);
                Palettes.LoadPaletteGroup(PaletteID.SP_2, Palettes.PaletteGroups.TITLESCREEN_3);
                Palettes.LoadPaletteGroup(PaletteID.SP_3, Palettes.PaletteGroups.TITLESCREEN_4);
                Sound.PlaySong(Sound.Songs.SPLASH, false);
                sprites.Clear();
                intro_scroll_counter = 0;
                gTimer = 0;
                return;
            }

            if ((gTimer > 1684 && gTimer < 4500) && (gTimer % 2 == 0))
            {
                y_scroll++;
                if (gTimer % 16 == 0)
                {
                    Textures.ScrollPPU_V(Textures.PPUDataGroup.OTHER, 0x780 + 0x20 * intro_scroll_counter, intro_scroll_counter + 1);
                    intro_scroll_counter++;
                }
            }
            else if ((gTimer > 945 && gTimer < 1425) && (gTimer % 2 == 0))
            {
                y_scroll++;
                if (gTimer % 16 == 0)
                {
                    intro_scroll_counter++;
                }
            }
        }

        static void Sprites()
        {
            switch (gTimer)
            {
                case 1:
                    sprites.Add(new StaticSprite(0xca, 6, 40, 40));
                    sprites.Add(new StaticSprite(0xcc, 6, 48, 40));
                    sprites.Add(new StaticSprite(0xcc, 6, 200, 40, true));
                    sprites.Add(new StaticSprite(0xca, 6, 208, 40, true));
                    sprites.Add(new StaticSprite(0xca, 6, 40, 120, yflip: true));
                    sprites.Add(new StaticSprite(0xcc, 6, 48, 120, yflip: true));
                    sprites.Add(new StaticSprite(0xcc, 6, 200, 120, true, true));
                    sprites.Add(new StaticSprite(0xca, 6, 208, 120, true, true));

                    sprites.Add(new StaticSprite(0xce, 6, 116, 88));
                    sprites.Add(new StaticSprite(0xd0, 6, 124, 88));

                    sprites.Add(new StaticSprite(0xd2, 6, 87, 50));
                    sprites.Add(new StaticSprite(0xd2, 6, 204, 80));
                    sprites.Add(new StaticSprite(0xd2, 6, 123, 104));
                    sprites.Add(new StaticSprite(0xd2, 6, 80, 132));

                    sprites.Add(new StaticSprite(0xd4, 6, 95, 50));
                    sprites.Add(new StaticSprite(0xd4, 6, 36, 64));
                    sprites.Add(new StaticSprite(0xd4, 6, 100, 66));
                    sprites.Add(new StaticSprite(0xd4, 6, 144, 124));

                    sprites.Add(new StaticSprite(0xd6, 6, 80, 40));
                    sprites.Add(new StaticSprite(0xd6, 6, 160, 44));
                    sprites.Add(new StaticSprite(0xd6, 6, 44, 80));
                    sprites.Add(new StaticSprite(0xd6, 6, 188, 124));

                    sprites.Add(new StaticSprite(0xa0, 7, 112, 104));
                    sprites.Add(new StaticSprite(0xa0, 7, 120, 104));
                    sprites.Add(new StaticSprite(0xa0, 7, 128, 104));
                    sprites.Add(new StaticSprite(0xa0, 7, 136, 104));

                    sprites.Add(new WaterFallSprite(0xb2, 7, 80, 184));
                    sprites.Add(new WaterFallSprite(0xb4, 7, 88, 184));
                    sprites.Add(new WaterFallSprite(0xb6, 7, 96, 184));
                    sprites.Add(new WaterFallSprite(0xb8, 7, 104, 184));

                    sprites.Add(new WaterFallSprite(0xc2, 7, 80, 202));
                    sprites.Add(new WaterFallSprite(0xc4, 7, 88, 202));
                    sprites.Add(new WaterFallSprite(0xc6, 7, 96, 202));
                    sprites.Add(new WaterFallSprite(0xc8, 7, 104, 202));

                    sprites.Add(new WaterFallSprite(0xc2, 7, 80, 218));
                    sprites.Add(new WaterFallSprite(0xc4, 7, 88, 218));
                    sprites.Add(new WaterFallSprite(0xc6, 7, 96, 218));
                    sprites.Add(new WaterFallSprite(0xc8, 7, 104, 218));

                    sprites.Add(new FlickeringSprite(0xb0, 7, 104, 168, 8, 0xa8, second_palette_index: 7));
                    sprites.Add(new FlickeringSprite(0xae, 7, 96, 168, 8, 0xa6, second_palette_index: 7));
                    sprites.Add(new FlickeringSprite(0xac, 7, 88, 168, 8, 0xa4, second_palette_index: 7));
                    sprites.Add(new FlickeringSprite(0xaa, 7, 80, 168, 8, 0xa2, second_palette_index: 7));
                    break;
                case 745:
                    sprites.Clear();
                    break;
                case 945:
                    Palettes.LoadPaletteGroup(PaletteID.SP_0, Palettes.PaletteGroups.GREEN_LINK_HUDSPR1);
                    Palettes.LoadPalette(PaletteID.SP_0, 2, Color._37_BEIGE);
                    Palettes.LoadPaletteGroup(PaletteID.SP_1, Palettes.PaletteGroups.HUDSPR_2);
                    Palettes.LoadPaletteGroup(PaletteID.SP_2, Palettes.PaletteGroups.HUDSPR_3);
                    break;
                case 1780:
                    sprites.Add(new StaticHeartSprite(72, 834));
                    sprites.Add(new StaticSprite(0x68, 6, 172, 830));
                    sprites.Add(new StaticSprite(0x68, 6, 180, 830, xflip: true));
                    break;
                case 1908:
                    sprites.Add(new FlickeringSprite(0x50, 6, 72, 894, 4, 0x52));
                    sprites.Add(new StaticSprite(0x66, 6, 172, 894));
                    sprites.Add(new StaticSprite(0x66, 6, 180, 894, xflip: true));
                    break;
                case 2036:
                    sprites.Add(new StaticSprite(0x32, 5, 72, 958));
                    sprites.Add(new FlickeringSprite(0x32, 5, 176, 958, 8, 0x32, second_palette_index: 6));
                    break;
                case 2164:
                    sprites.Add(new StaticSprite(0x40, 5, 72, 1022));
                    sprites.Add(new StaticSprite(0x40, 6, 176, 1022));
                    break;
                case 2292:
                    sprites.Add(new StaticSprite(0x4c, 5, 72, 1086));
                    sprites.Add(new StaticSprite(0x22, 6, 176, 1086));
                    break;
                case 2420:
                    sprites.Add(new StaticSprite(0x20, 4, 72, 1150));
                    sprites.Add(new StaticSprite(0x20, 5, 176, 1150));
                    break;
                case 2548:
                    sprites.Add(new StaticSprite(0x48, 6, 72, 1214));
                    sprites.Add(new StaticSprite(0x56, 4, 176, 1214));
                    break;
                case 2676:
                    sprites.Add(new StaticSprite(0x36, 4, 72, 1278));
                    sprites.Add(new StaticSprite(0x36, 5, 176, 1278));
                    break;
                case 2804:
                    sprites.Add(new StaticSprite(0x34, 5, 72, 1342));
                    sprites.Add(new StaticSprite(0x2a, 4, 176, 1342));
                    break;
                case 2932:
                    sprites.Add(new StaticSprite(0x28, 4, 72, 1406));
                    sprites.Add(new StaticSprite(0x28, 5, 176, 1406));
                    break;
                case 3060:
                    sprites.Add(new StaticSprite(0x26, 5, 72, 1470));
                    sprites.Add(new StaticSprite(0x26, 6, 176, 1470));
                    break;
                case 3188:
                    sprites.Add(new StaticSprite(0x46, 5, 72, 1534));
                    sprites.Add(new StaticSprite(0x46, 6, 176, 1534));
                    break;
                case 3316:
                    sprites.Add(new StaticSprite(0x4e, 6, 72, 1598));
                    sprites.Add(new StaticSprite(0x24, 6, 176, 1598));
                    break;
                case 3444:
                    sprites.Add(new StaticSprite(0x6c, 4, 68, 1662));
                    sprites.Add(new StaticSprite(0x6c, 4, 76, 1662, xflip: true));
                    sprites.Add(new StaticSprite(0x76, 4, 172, 1662));
                    sprites.Add(new StaticSprite(0x76, 4, 180, 1662, xflip: true));
                    break;
                case 3572:
                    sprites.Add(new StaticSprite(0x4a, 5, 72, 1726));
                    sprites.Add(new StaticSprite(0x42, 6, 176, 1726));
                    break;
                case 3700:
                    sprites.Add(new StaticSprite(0x2e, 6, 72, 1790));
                    sprites.Add(new StaticSprite(0x2c, 6, 176, 1790));
                    break;
                case 3828:
                    sprites.Add(new StaticSprite(0x4c, 6, 72, 1854));
                    sprites.Add(new StaticSprite(0x6a, 6, 172, 1854));
                    sprites.Add(new StaticSprite(0x6a, 6, 180, 1854, xflip: true));
                    break;
                case 3956:
                    sprites.Add(new FlickeringSprite(0x6e, 5, 120, 1918, 8, 0x6e, second_palette_index: 6));
                    sprites.Add(new FlickeringSprite(0x6e, 5, 128, 1918, 8, 0x6e, true, second_palette_index: 6));
                    break;
                case 4084:
                    sprites.Add(new StaticSprite(0xe0, 4, 104, 2014));
                    sprites.Add(new StaticSprite(0xe2, 4, 112, 2014));
                    sprites.Add(new StaticSprite(0xec, 4, 120, 2014));
                    sprites.Add(new StaticSprite(0xee, 4, 128, 2014));
                    sprites.Add(new StaticSprite(0xf8, 4, 136, 2014));
                    sprites.Add(new StaticSprite(0xfa, 4, 144, 2014));

                    sprites.Add(new StaticSprite(0xe4, 4, 104, 2030));
                    sprites.Add(new StaticSprite(0xe6, 4, 112, 2030));
                    sprites.Add(new StaticSprite(0xf0, 4, 120, 2030));
                    sprites.Add(new StaticSprite(0xf2, 4, 128, 2030));
                    sprites.Add(new StaticSprite(0xfc, 4, 136, 2030));
                    sprites.Add(new StaticSprite(0xfe, 4, 144, 2030));

                    sprites.Add(new StaticSprite(0xe8, 4, 104, 2046));
                    sprites.Add(new StaticSprite(0xea, 4, 112, 2046));
                    sprites.Add(new StaticSprite(0xf4, 4, 120, 2046));
                    sprites.Add(new StaticSprite(0xf6, 4, 128, 2046));
                    sprites.Add(new StaticSprite(0xdc, 4, 136, 2046));
                    sprites.Add(new StaticSprite(0xde, 4, 144, 2046));

                    sprites.Add(new StaticSprite(0x78, 4, 120, 2062));
                    sprites.Add(new StaticSprite(0x78, 4, 128, 2062, xflip: true));
                    break;
            }

            if ((gTimer % 128 == 0) && (gTimer > 2430) && (gTimer < 4700))
            {
                sprites.RemoveRange(0, 2);
            }
        }

        static void Startgame()
        {
            if (Control.IsPressed(Buttons.START))
            {
                gamemode = Gamemode.FILESELECT;
                FileSelectCode.InitFileSelect();
            }
        }
    }
}