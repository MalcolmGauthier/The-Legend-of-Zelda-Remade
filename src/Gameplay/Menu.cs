using static The_Legend_of_Zelda.Rendering.Screen;
using static The_Legend_of_Zelda.SaveLoad;
using static The_Legend_of_Zelda.Gameplay.Program;
using The_Legend_of_Zelda.Sprites;
using The_Legend_of_Zelda.Rendering;

namespace The_Legend_of_Zelda.Gameplay
{
    internal static class Menu
    {
        public static bool can_open_menu = false;
        public static bool menu_open = false;
        public static bool boomerang_out = false, arrow_out = false, bait_out = false, magic_wave_out = false,
            tornado_out = false, bomb_out = false, sword_proj_out = false;
        public static bool blue_candle_limit_reached = false;
        public static bool draw_hud_objects = false;

        public static int selected_menu_index = 0;
        public static SpriteID current_B_item = 0;
        public static byte fire_out = 0;
        static byte rupie_count_display = 0;
        static byte menu_animation_timer = 250;

        public static StaticSprite map_dot = new StaticSprite(SpriteID.DOT, PaletteID.SP_0, 45, 52);
        public static StaticSprite hud_sword = new StaticSprite(SpriteID.SWORD, PaletteID.SP_0, 152, 32);
        public static StaticSprite hud_B_item = new StaticSprite(SpriteID.BOMB, PaletteID.SP_1, 128, 32);
        public static FlickeringSprite menu_selection_left = new FlickeringSprite(0x1e, 5, 128, 359, 8, 0x1e, second_palette_index: 6);
        public static FlickeringSprite menu_selection_right = new FlickeringSprite(0x1e, 5, 136, 359, 8, 0x1e, second_palette_index: 6, xflip: true);
        static StaticSprite[] menu_sprites = new StaticSprite[17];

        public static SpriteID[] menu_item_list = {
            SpriteID.BOOMERANG, SpriteID.BOMB, SpriteID.ARROW, SpriteID.CANDLE, SpriteID.RECORDER, SpriteID.BAIT, SpriteID.POTION, SpriteID.ROD
        };
        public static readonly ushort[,] hud_dungeon_maps = {
            {0b0000000000000000,
             0b0000101100010100,
             0b0010111111100000,
             0b0000011101000000},

            {0b0000001011010000,
             0b0000000011110000,
             0b0000000011110000,
             0b0000101111100000},

            {0b0000000000000000,
             0b0000101100010000,
             0b0011111111110000,
             0b0010001101000000},

            {0b0000111011110000,
             0b0000111101000000,
             0b0000110100000000,
             0b0000011110000000},

            {0b0000011011010000,
             0b0000111010110000,
             0b0000000111110000,
             0b0000101011110000},

            {0b0001111111110100,
             0b0011110100111000,
             0b0011000000000000,
             0b0011011100000000},

            {0b0001111011111000,
             0b0011111110000000,
             0b0011110101000000,
             0b0011111110101000},

            {0b0000000111010000,
             0b0001110111010000,
             0b0010111111010000,
             0b0000010111010000},

            {0b0111111111111101,
             0b1111101111101111,
             0b1111111111111111,
             0b0011101111101100},
        };
        public static readonly int[] map_offsets = {
            -8, 0,
            -2, 0,
            0, 0,
            -6, 0,
            -10, 0,
            1, 0,
            0, -8,
            -2, -8,
            -8, -8
        };

        public static void Tick()
        {
            if (menu_open)
            {
                if (Control.IsPressed(Buttons.START) && can_open_menu)
                {
                    menu_animation_timer = 100;
                    can_open_menu = false;
                }

                if (can_open_menu)
                {
                    if (Control.IsPressed(Buttons.LEFT))
                    {
                        AutoSwitchBItem(selected_menu_index, false);
                        MoveCursor();
                    }
                    else if (Control.IsPressed(Buttons.RIGHT))
                    {
                        AutoSwitchBItem(selected_menu_index, true);
                        MoveCursor();
                    }
                }

                can_pause = false;
            }
            else
            {
                if (Control.IsPressed(Buttons.START) && can_open_menu)
                {
                    menu_animation_timer = 0;
                    can_open_menu = false;
                    menu_open = true;
                }
            }

            MenuAnimation();
        }

        public static void InitHUD()
        {
            sprites.Remove(map_dot);
            sprites.Remove(hud_sword);
            sprites.Remove(hud_B_item);

            sprites.Add(map_dot);
            sprites.Add(hud_sword);
            sprites.Add(hud_B_item);

            rupie_count_display = rupy_count;

            Textures.DrawHUDBG();
        }

        public static void DrawHUD()
        {
            if (!draw_hud_objects)
            {
                hud_sword.shown = false;
                hud_B_item.shown = false;
                if (menu_open && can_open_menu)
                {
                    if (current_B_item == menu_item_list[0] && !(boomerang || magical_boomerang))
                        return;
                    hud_B_item.shown = true;
                    DisplayBItem();
                    goto skip_showing_items;
                }
                return;
            }

            if (magical_sword)
            {
                hud_sword.shown = true;
                hud_sword.tile_index = (byte)SpriteID.MAGIC_SWORD;
                hud_sword.palette_index = (byte)PaletteID.SP_2;
            }
            else if (white_sword)
            {
                hud_sword.shown = true;
                hud_sword.tile_index = (byte)SpriteID.SWORD;
                hud_sword.palette_index = (byte)PaletteID.SP_1;
            }
            else if (wooden_sword)
            {
                hud_sword.shown = true;
                hud_sword.tile_index = (byte)SpriteID.SWORD;
                hud_sword.palette_index = (byte)PaletteID.SP_0;
            }
            else
            {
                hud_sword.shown = false;
            }

            DisplayBItem();

            if (bomb_count > bomb_limit)
                bomb_count = bomb_limit;

            if (current_B_item == SpriteID.BOMB && bomb_count == 0)
            {
                AutoSwitchBItem(1);
            }
            else if (current_B_item == SpriteID.BAIT && !bait)
            {
                AutoSwitchBItem(5);
            }

            map_dot.shown = true;
            if (gamemode == Gamemode.OVERWORLD)
            {
                if (OC.current_screen != 128)
                {
                    map_dot.x = 17 + OC.current_screen % 16 * 4;
                    map_dot.y = 24 + OC.current_screen / 16 * 4;
                }
            }
            else
            {
                if (DC.room_list[DC.current_screen] is not (0x2a or 0x2b))
                {
                    map_dot.x = 10 + DC.current_screen % 16 * 8 + map_offsets[DC.current_dungeon * 2] * 8;
                    map_dot.y = 24 + DC.current_screen / 16 * 4 + map_offsets[DC.current_dungeon * 2 + 1] * 4;
                }
            }

        skip_showing_items:
            if (rupie_count_display != rupy_count && (gTimer & 1) == 0)
            {
                if (rupie_count_display > rupy_count)
                {
                    rupie_count_display--;
                }
                else if (rupie_count_display < rupy_count)
                {
                    rupie_count_display++;
                }
                Sound.PlaySFX(Sound.SoundEffects.ITEM_GET);
            }

            byte rupie_count = rupie_count_display;
            if (rupie_count >= 100)
                Textures.ppu[0x6c] = (byte)Math.Floor(rupie_count / 100.0);
            else
                Textures.ppu[0x6c] = 0x21;

            if (rupie_count >= 10)
            {
                Textures.ppu[0x6d] = (byte)(Math.Floor(rupie_count / 10.0) % 10);
                Textures.ppu[0x6e] = (byte)(rupie_count % 10);
            }
            else
            {
                Textures.ppu[0x6d] = (byte)(rupie_count % 10);
                Textures.ppu[0x6e] = 0x24;
            }

            if (magical_key)
            {
                // "xA "
                Textures.ppu[0xac] = 0x21;
                Textures.ppu[0xad] = 0xa;
                Textures.ppu[0xae] = 0x24;
            }
            else
            {
                byte keys = key_count;
                if (keys >= 100)
                    Textures.ppu[0xac] = (byte)Math.Floor(keys / 100.0);
                else
                    Textures.ppu[0xac] = 0x21;

                if (keys >= 10)
                {
                    Textures.ppu[0xad] = (byte)(Math.Floor(keys / 10.0) % 10);
                    Textures.ppu[0xae] = (byte)(keys % 10);
                }
                else
                {
                    Textures.ppu[0xad] = (byte)(keys % 10);
                    Textures.ppu[0xae] = 0x24;
                }
            }

            byte bombs = bomb_count;
            Textures.ppu[0xcc] = 0x21;
            if (bombs >= 10)
            {
                // supports number over 19 even tho highest bomb count possible is 16
                Textures.ppu[0xcd] = (byte)(bombs / 10 % 10);
                Textures.ppu[0xce] = (byte)(bombs % 10);
            }
            else
            {
                Textures.ppu[0xcd] = (byte)(bombs % 10);
                Textures.ppu[0xce] = 0x24;
            }

            for (int i = 0; i < nb_of_hearts; i++)
            {
                if (Link.hp >= i + 1)
                    Textures.ppu[0xd6 + i + (i >> 3) * -40] = 0xf2;
                else if (Link.hp >= i + 0.5f)
                    Textures.ppu[0xd6 + i + (i >> 3) * -40] = 0x65;
                else
                    Textures.ppu[0xd6 + i + (i >> 3) * -40] = 0x66;
            }

            //TODO: this shit
            void DisplayBItem()
            {
                if (current_B_item == 0)
                {
                    hud_B_item.shown = false;
                }
                else
                {
                    hud_B_item.shown = true;
                    hud_B_item.tile_index = (byte)current_B_item;
                    switch (current_B_item)
                    {
                        case SpriteID.CANDLE:
                            if (red_candle)
                                hud_B_item.palette_index = 6;
                            else
                                hud_B_item.palette_index = 5;
                            break;
                        case SpriteID.ARROW:
                            if (silver_arrow)
                                hud_B_item.palette_index = 5;
                            else
                                hud_B_item.palette_index = 4;
                            break;

                        case SpriteID.BOOMERANG:
                            if (magical_boomerang)
                                hud_B_item.palette_index = 5;
                            else
                                hud_B_item.palette_index = 4;
                            break;
                        case SpriteID.POTION:
                            if (red_potion)
                                hud_B_item.palette_index = 6;
                            else
                                hud_B_item.palette_index = 5;
                            break;
                        case SpriteID.BOMB or SpriteID.ROD or SpriteID.MAP:
                            hud_B_item.palette_index = 5;
                            break;
                        case SpriteID.BAIT or SpriteID.RECORDER:
                            hud_B_item.palette_index = 6;
                            break;
                    }
                }
            }
        }

        // adds in all of the other static sprites that exist inside the full screen menu
        static void InitMenuSprites()
        {
            sprites.Add(menu_selection_left);
            sprites.Add(menu_selection_right);

            if (raft)
            {
                menu_sprites[0] = new StaticSprite(SpriteID.RAFT, PaletteID.SP_0, 128, 31);
                menu_sprites[1] = new StaticSprite(SpriteID.RAFT, PaletteID.SP_0, 136, 31, xflip: true);
            }

            if (book_of_magic)
            {
                menu_sprites[2] = new StaticSprite(SpriteID.BOOK_OF_MAGIC, PaletteID.SP_2, 152, 31);
            }

            if (blue_ring || red_ring)
            {
                PaletteID make_ring_red = red_ring ? PaletteID.SP_2 : PaletteID.SP_1;

                menu_sprites[3] = new StaticSprite(SpriteID.RING, make_ring_red, 164, 31);
            }

            if (ladder)
            {
                menu_sprites[4] = new StaticSprite(SpriteID.LADDER, PaletteID.SP_0, 176, 31);
                menu_sprites[5] = new StaticSprite(SpriteID.LADDER, PaletteID.SP_0, 184, 31, xflip: true);
            }

            if (magical_key)
            {
                menu_sprites[6] = new StaticSprite(SpriteID.MAGICAL_KEY, PaletteID.SP_2, 196, 31);
            }

            if (power_bracelet)
            {
                menu_sprites[7] = new StaticSprite(SpriteID.POWER_BRACELET, PaletteID.SP_2, 208, 31);
            }

            if (boomerang || magical_boomerang)
            {
                PaletteID make_boom_blue = magical_boomerang ? PaletteID.SP_1 : PaletteID.SP_0;

                menu_sprites[8] = new StaticSprite(SpriteID.BOOMERANG, make_boom_blue, 132, 55);
            }

            if (bomb_count > 0)
            {
                menu_sprites[9] = new StaticSprite(SpriteID.BOMB, PaletteID.SP_1, 156, 55);
            }

            if (arrow || silver_arrow)
            {
                PaletteID make_arrow_silver = silver_arrow ? PaletteID.SP_1 : PaletteID.SP_0;

                menu_sprites[10] = new StaticSprite(SpriteID.ARROW, make_arrow_silver, 176, 55);
            }

            if (bow)
            {
                menu_sprites[11] = new StaticSprite(SpriteID.BOW, PaletteID.SP_0, 184, 55);
            }

            if (red_candle || blue_candle)
            {
                PaletteID make_candle_red = red_candle ? PaletteID.SP_2 : PaletteID.SP_1;

                menu_sprites[12] = new StaticSprite(SpriteID.CANDLE, make_candle_red, 204, 55);
            }

            if (recorder)
            {
                menu_sprites[13] = new StaticSprite(SpriteID.RECORDER, PaletteID.SP_2, 132, 71);
            }

            if (bait)
            {
                menu_sprites[14] = new StaticSprite(SpriteID.BAIT, PaletteID.SP_2, 156, 71);
            }

            if (red_potion || blue_potion || letter)
            {
                PaletteID make_red = red_potion ? PaletteID.SP_2 : PaletteID.SP_1;
                SpriteID is_potion = red_potion || blue_potion ? SpriteID.POTION : SpriteID.MAP;

                menu_sprites[15] = new StaticSprite(is_potion, make_red, 180, 71);
            }

            if (magical_rod)
            {
                menu_sprites[16] = new StaticSprite(SpriteID.ROD, PaletteID.SP_1, 204, 70);
            }

            for (int i = 0; i < menu_sprites.Length; i++)
            {
                if (menu_sprites[i] != null)
                {
                    menu_sprites[i].y += 304;
                    sprites.Add(menu_sprites[i]);
                }
            }
        }

        static void MenuAnimation()
        {
            if (menu_animation_timer == 0)
            {
                Textures.DrawMenu();
                Link.Show(false);
                Link.can_move = false;
                draw_hud_objects = false;
                y_scroll--;
                hud_B_item.x -= 60;
                hud_B_item.y += 328;
                if (gamemode == Gamemode.OVERWORLD)
                    DrawTriforce();
                else
                    DrawMap();
            }
            else if (menu_animation_timer == 58)
            {
                menu_animation_timer = 201;
                can_open_menu = true;
                InitMenuSprites();
                current_B_item = menu_item_list[selected_menu_index];
                MoveCursor();
            }
            else if (menu_animation_timer == 100)
            {
                can_open_menu = false;
                hud_B_item.x += 60;
                hud_B_item.y -= 328;
                hud_B_item.shown = false;
                y_scroll++;

                if (current_B_item == menu_item_list[0] && !(boomerang || magical_boomerang))
                    current_B_item = 0;
                RemoveMenu();
            }
            else if (menu_animation_timer == 158)
            {
                Link.Show(true);
                Link.can_move = true;
                can_open_menu = true;
                menu_open = false;
                draw_hud_objects = true;
                menu_animation_timer = 201;
                can_pause = true;
            }

            if (menu_animation_timer < 58)
                y_scroll -= 3;
            else if (menu_animation_timer >= 100 && menu_animation_timer < 158)
                y_scroll += 3;

            if (menu_animation_timer < 200)
                menu_animation_timer++;
        }

        // remove all menu sprites, used when menu closes
        static void RemoveMenu()
        {
            for (int i = 0; i < menu_sprites.Length; i++)
            {
                if (menu_sprites[i] != null)
                {
                    sprites.Remove(menu_sprites[i]);
                }
            }

            sprites.Remove(menu_selection_left);
            sprites.Remove(menu_selection_right);
        }

        public static void AutoSwitchBItem(int starting_index, bool add = false)
        {
            const int ITEM_MENU_LENGTH = 8;

            bool[] conditions = new bool[ITEM_MENU_LENGTH]
            {
                boomerang || magical_boomerang,
                bomb_count > 0,
                bow && (arrow || silver_arrow),
                red_candle || blue_candle,
                recorder,
                bait,
                red_potion || blue_potion || letter,
                magical_rod
            };
            SpriteID[] spriteIDs = new SpriteID[ITEM_MENU_LENGTH]
            {
                SpriteID.BOOMERANG,
                SpriteID.BOMB,
                SpriteID.ARROW,
                SpriteID.CANDLE,
                SpriteID.RECORDER,
                SpriteID.BAIT,
                SpriteID.POTION,
                SpriteID.ROD
            };

            current_B_item = 0;
            selected_menu_index = 0;

            for (int i = 0; i < ITEM_MENU_LENGTH; i++)
            {
                starting_index += add ? 1 : -1;
                // add 8 to prevent shitty modulo in C style languages
                starting_index = (starting_index + ITEM_MENU_LENGTH) % ITEM_MENU_LENGTH;

                if (conditions[starting_index])
                {
                    current_B_item = spriteIDs[starting_index];
                    selected_menu_index = starting_index;

                    if (current_B_item == SpriteID.POTION && !(blue_potion || red_potion))
                    {
                        current_B_item = SpriteID.MAP;
                    }

                    break;
                }
            }

            MoveCursor();
        }

        // moves the position of the cursor in the menu. this is called whenever the B item changes. even if done outside of full screen menu
        public static void MoveCursor()
        {
            menu_selection_left.x = 128 + selected_menu_index % 4 * 24;
            menu_selection_left.y = 359 + selected_menu_index / 4 * 16;
            menu_selection_right.x = 136 + selected_menu_index % 4 * 24;
            menu_selection_right.y = 359 + selected_menu_index / 4 * 16;

            SpriteID switch_to = menu_item_list[selected_menu_index];
            if (switch_to == SpriteID.POTION && !(blue_potion || red_potion))
                switch_to = SpriteID.MAP;

            current_B_item = switch_to;
        }

        // draw the big triforce on the menu screen in overworld
        static void DrawTriforce()
        {
            if (GetTriforceFlag(0))
            {
                Textures.ppu[0x6af] = 0xe7;
                Textures.ppu[0x6ce] = 0xe7;
                Textures.ppu[0x6cf] = 0xf5;
            }

            if (GetTriforceFlag(1))
            {
                Textures.ppu[0x6b0] = 0xe8;
                Textures.ppu[0x6d0] = 0xf5;
                Textures.ppu[0x6d1] = 0xe8;
            }

            if (GetTriforceFlag(2))
            {
                Textures.ppu[0x6ed] = 0xe7;
                Textures.ppu[0x70c] = 0xe7;
                Textures.ppu[0x70d] = 0xf5;
            }

            if (GetTriforceFlag(3))
            {
                Textures.ppu[0x6f2] = 0xe8;
                Textures.ppu[0x712] = 0xf5;
                Textures.ppu[0x713] = 0xe8;
            }

            bool piece_4 = GetTriforceFlag(4);
            bool piece_5 = GetTriforceFlag(5);
            if (piece_4 || piece_5)
            {
                if (piece_4 && piece_5)
                {
                    Textures.ppu[0x6ee] = 0xf5;
                    Textures.ppu[0x6ef] = 0xf5;
                    Textures.ppu[0x70e] = 0xf5;
                    Textures.ppu[0x70f] = 0xf5;
                }
                else if (piece_4)
                {
                    Textures.ppu[0x6ef] = 0xf5;
                    Textures.ppu[0x6ee] = 0xe5;
                    Textures.ppu[0x70f] = 0xe5;
                }
                else
                {
                    Textures.ppu[0x70e] = 0xf5;
                    Textures.ppu[0x6ee] = 0xe8;
                    Textures.ppu[0x70f] = 0xe8;
                }
            }

            bool piece_6 = GetTriforceFlag(6);
            bool piece_7 = GetTriforceFlag(7);
            if (piece_6 || piece_7)
            {
                if (piece_6 && piece_7)
                {
                    Textures.ppu[0x6f0] = 0xf5;
                    Textures.ppu[0x6f1] = 0xf5;
                    Textures.ppu[0x710] = 0xf5;
                    Textures.ppu[0x711] = 0xf5;
                }
                else if (piece_6)
                {
                    Textures.ppu[0x6f0] = 0xf5;
                    Textures.ppu[0x6f1] = 0xe6;
                    Textures.ppu[0x710] = 0xe6;
                }
                else
                {
                    Textures.ppu[0x711] = 0xf5;
                    Textures.ppu[0x6f1] = 0xe7;
                    Textures.ppu[0x710] = 0xe7;
                }
            }
        }

        // draw the big map on the menu screen in dungeon
        static void DrawMap()
        {
            // TODO: LOLOLOLOLOLOL gl lmao
        }

        public static void DrawHudMap()
        {
            // chr nametable address for each of the 4 dungeon hud map blocks
            byte[] tile_locations = { 0x24, 0xfb, 0x67, 0xff }; // none, bottom, top, both
            const int BIT_MASK = 0b11;
            const int MASK_BIT_LEN = 2;
            const int HUD_MAP_WIDTH = 8;
            const int HUD_MAP_HEIGHT = 4;
            const int HUD_MAP_OFFSET = 0x62;

            for (int i = 0; i < HUD_MAP_HEIGHT; i++)
            {
                int shift = (HUD_MAP_WIDTH - 1) * MASK_BIT_LEN;
                for (int j = 0; j < HUD_MAP_WIDTH; j++)
                {
                    shift -= MASK_BIT_LEN;
                    Textures.ppu[HUD_MAP_OFFSET + i * Textures.PPU_WIDTH + j] = tile_locations[
                        (hud_dungeon_maps[DC.current_dungeon, i] & BIT_MASK << shift) >> shift
                    ];
                }
            }
        }

        // returns number of triforce pieces gotten
        public static int GetTriforcePieceCount()
        {
            int count = 0;
            for (byte i = 0; i < 8; i++)
            {
                if (GetTriforceFlag(i))
                    count++;
            }
            return count;
        }
    }
}