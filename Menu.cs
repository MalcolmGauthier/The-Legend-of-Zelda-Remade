using static The_Legend_of_Zelda.Screen;
using static The_Legend_of_Zelda.SaveLoad;
using static The_Legend_of_Zelda.OverworldCode;
namespace The_Legend_of_Zelda
{
    internal static class Menu
    {
        public static bool can_open_menu = false;
        public static bool menu_open = false;
        public static bool boomerang_out = false, arrow_out = false, bait_out = false, magic_wave_out = false, 
            tornado_out = false, bomb_out = false, sword_proj_out = false;
        public static bool blue_candle_limit_reached = false;
        public static bool draw_hud_objects = false;

        public static sbyte selected_menu_index = 0;
        public static SpriteID current_B_item = 0;
        public static byte fire_out = 0;
        static byte rupie_count_display = 0;
        static byte menu_animation_timer = 250;

        public static StaticSprite map_dot = new StaticSprite(0x3e, 4, 45, 52);
        public static StaticSprite hud_sword = new StaticSprite(0x20, 4, 152, 32);
        public static StaticSprite hud_B_item = new StaticSprite(0x34, 5, 128, 32);
        public static FlickeringSprite menu_selection_left = new FlickeringSprite(0x1e, 5, 128, 359, 8, 0x1e, second_palette_index: 6);
        public static FlickeringSprite menu_selection_right = new FlickeringSprite(0x1e, 5, 136, 359, 8, 0x1e, second_palette_index: 6, xflip: true);
        static StaticSprite[] menu_sprites = new StaticSprite[17];

        public static SpriteID[] menu_item_list = {
            0x36, 0x34, 0x28, 0x26, 0x24, 0x22, 0x40, 0x4a
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
        public static readonly sbyte[] map_offsets = {
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

            if (menu_animation_timer == 0)
            {
                Textures.DrawMenu();
                Link.Show(false);
                Link.can_move = false;
                draw_hud_objects = false;
                y_scroll--;
                hud_B_item.x -= 60;
                hud_B_item.y += 328;
                if (Program.gamemode == Program.Gamemode.OVERWORLD)
                    DrawTriforce();
                else
                    DrawMap();
            }
            else if (menu_animation_timer == 58)
            {
                menu_animation_timer = 201;
                can_open_menu = true;
                InitMenu();
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
                if (current_B_item == 0x36 && !sprites.Contains(menu_sprites[8]))
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
                Program.can_pause = true;
            }

            if (menu_animation_timer < 58)
                y_scroll -= 3;
            else if (menu_animation_timer >= 100 && menu_animation_timer < 158)
                y_scroll += 3;

            if (menu_animation_timer < 200)
                menu_animation_timer++;
        }

        public static void InitHUD()
        {
            if (sprites.Contains(map_dot))
                sprites.Remove(map_dot);
            if (sprites.Contains(hud_sword))
                sprites.Remove(hud_sword);
            if (sprites.Contains(hud_B_item))
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
                    if (current_B_item == 0x36 && !sprites.Contains(menu_sprites[8]))
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
                hud_sword.tile_index = 0x48;
                hud_sword.palette_index = 6;
            }
            else if (white_sword)
            {
                hud_sword.shown = true;
                hud_sword.tile_index = 0x20;
                hud_sword.palette_index = 5;
            }
            else if (wooden_sword)
            {
                hud_sword.shown = true;
                hud_sword.tile_index = 0x20;
                hud_sword.palette_index = 4;
            }
            else
                hud_sword.shown = false;

            DisplayBItem();

            if (bomb_count > bomb_limit)
                bomb_count = bomb_limit;

            if (current_B_item == 0x34 && bomb_count == 0)
            {
                AutoSwitchBItem(1);
            }
            else if (current_B_item == 0x22 && !bait)
            {
                AutoSwitchBItem(5);
            }

            if (Program.gamemode == Program.Gamemode.OVERWORLD)
            {
                if (current_screen != 128)
                {
                    map_dot.x = 17 + ((current_screen & 15) << 2);
                    map_dot.y = 24 + ((current_screen >> 4) << 2);
                }
            }
            else
            {
                if (DungeonCode.room_list[DungeonCode.current_screen] is not (0x2a or 0x2b))
                {
                    map_dot.x = 18 + ((DungeonCode.current_screen & 15) << 3) + map_offsets[DungeonCode.current_dungeon * 2] * 8;
                    map_dot.y = 24 + ((DungeonCode.current_screen >> 4) << 2) + map_offsets[DungeonCode.current_dungeon * 2 + 1] * 4;
                }
            }

            skip_showing_items:
            if (rupie_count_display != rupy_count && (Program.gTimer & 1) == 0)
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
                Textures.ppu[0xcd] = (byte)(Math.Floor(bombs / 10.0) % 10);
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
                    Textures.ppu[0xd6 + i + ((i >> 3) * -40)] = 0xf2;
                else if (Link.hp >= i + 0.5f)
                    Textures.ppu[0xd6 + i + ((i >> 3) * -40)] = 0x65;
                else
                    Textures.ppu[0xd6 + i + ((i >> 3) * -40)] = 0x66;
            }
            void DisplayBItem()
            {
                if (current_B_item == 0)
                {
                    hud_B_item.shown = false;
                }
                else
                {
                    hud_B_item.shown = true;
                    hud_B_item.tile_index = current_B_item;
                    switch (current_B_item)
                    {
                        case 0x26:
                            if (red_candle)
                                hud_B_item.palette_index = 6;
                            else
                                hud_B_item.palette_index = 5;
                            break;
                        case 0x28:
                            if (silver_arrow)
                                hud_B_item.palette_index = 5;
                            else
                                hud_B_item.palette_index = 4;
                            break;

                        case 0x36:
                            if (magical_boomerang)
                                hud_B_item.palette_index = 5;
                            else
                                hud_B_item.palette_index = 4;
                            break;
                        case 0x40:
                            if (red_potion)
                                hud_B_item.palette_index = 6;
                            else
                                hud_B_item.palette_index = 5;
                            break;
                        case 0x34 or 0x4a or 0x4c:
                            hud_B_item.palette_index = 5;
                            break;
                        case 0x22 or 0x24:
                            hud_B_item.palette_index = 6;
                            break;
                    }
                }
            }
        }

        static void InitMenu()
        {
            sprites.Add(menu_selection_left);
            sprites.Add(menu_selection_right);

            if (raft[current_save_file])
            {
                menu_sprites[0] = new StaticSprite((byte)SpriteID.RAFT, (byte)PaletteID.SP_0, 128, 31);
                menu_sprites[1] = new StaticSprite((byte)SpriteID.RAFT, (byte)PaletteID.SP_0, 136, 31, xflip: true);
            }

            if (book_of_magic[current_save_file])
            {
                menu_sprites[2] = new StaticSprite((byte)SpriteID.BOOK_OF_MAGIC, (byte)PaletteID.SP_2, 152, 31);
            }

            if (blue_ring[current_save_file] || red_ring[current_save_file])
            {
                PaletteID make_ring_red = PaletteID.SP_1;
                if (red_ring[current_save_file])
                {
                    make_ring_red = PaletteID.SP_2;
                }

                menu_sprites[3] = new StaticSprite((byte)SpriteID.RING, (byte)make_ring_red, 164, 31);
            }

            if (ladder[current_save_file])
            {
                menu_sprites[4] = new StaticSprite((byte)SpriteID.LADDER, (byte)PaletteID.SP_0, 176, 31);
                menu_sprites[5] = new StaticSprite((byte)SpriteID.LADDER, (byte)PaletteID.SP_0, 184, 31, xflip: true);
            }

            if (magical_key[current_save_file])
            {
                menu_sprites[6] = new StaticSprite((byte)SpriteID.MAGICAL_KEY, (byte)PaletteID.SP_2, 196, 31);
            }

            if (power_bracelet[current_save_file])
            {
                menu_sprites[7] = new StaticSprite((byte)SpriteID.POWER_BRACELET, (byte)PaletteID.SP_2, 208, 31);
            }

            if (boomerang[current_save_file] || magical_boomerang[current_save_file])
            {
                PaletteID make_boom_blue = PaletteID.SP_0;
                if (magical_boomerang[current_save_file])
                {
                    make_boom_blue = PaletteID.SP_1;
                }

                menu_sprites[8] = new StaticSprite((byte)SpriteID.BOOMERANG, (byte)make_boom_blue, 132, 55);
            }

            if (bomb_count[current_save_file] > 0)
            {
                menu_sprites[9] = new StaticSprite((byte)SpriteID.BOMB, (byte)PaletteID.SP_1, 156, 55);
            }

            if (arrow[current_save_file] || silver_arrow[current_save_file])
            {
                PaletteID make_arrow_silver = PaletteID.SP_0;
                if (silver_arrow[current_save_file])
                {
                    make_arrow_silver = PaletteID.SP_1;
                }

                menu_sprites[10] = new StaticSprite((byte)SpriteID.ARROW, (byte)make_arrow_silver, 176, 55);
            }

            if (bow[current_save_file])
            {
                menu_sprites[11] = new StaticSprite((byte)SpriteID.BOW, (byte)PaletteID.SP_0, 184, 55);
            }

            if (red_candle[current_save_file] || blue_candle[current_save_file])
            {
                PaletteID make_candle_red = PaletteID.SP_1;
                if (red_candle[current_save_file])
                {
                    make_candle_red = PaletteID.SP_2;
                }

                menu_sprites[12] = new StaticSprite((byte)SpriteID.CANDLE, (byte)make_candle_red, 204, 55);
            }

            if (recorder[current_save_file])
            {
                menu_sprites[13] = new StaticSprite((byte)SpriteID.RECORDER, (byte)PaletteID.SP_2, 132, 71);
            }

            if (bait[current_save_file])
            {
                menu_sprites[14] = new StaticSprite((byte)SpriteID.BAIT, (byte)PaletteID.SP_2, 156, 71);
            }

            if (red_potion[current_save_file] || blue_potion[current_save_file] || letter[current_save_file])
            {
                byte make_red = (byte)PaletteID.SP_1;
                SpriteID is_potion = SpriteID.MAP;
                if (red_potion[current_save_file] || blue_potion[current_save_file])
                {
                    if (red_potion[current_save_file])
                    {
                        make_red = (byte)PaletteID.SP_2;
                    }

                    is_potion = SpriteID.POTION;
                }

                menu_sprites[15] = new StaticSprite((byte)is_potion, make_red, 180, 71);
            }

            if (magical_rod[current_save_file])
            {
                menu_sprites[16] = new StaticSprite(0x4a, (byte)PaletteID.SP_1, 204, 70);
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

        public static void MoveCursor()
        {
            sbyte index = (sbyte)Array.IndexOf(menu_item_list, current_B_item);
            if (index < 0)
            {
                if (current_B_item == SpriteID.MAP)
                    index = 6;
                else
                    index = 0;
            }
            selected_menu_index = index;

            menu_selection_left.x = 128 + (selected_menu_index & 3) * 24;
            menu_selection_left.y = 359 + (selected_menu_index >> 2) * 16;
            menu_selection_right.x = 136 + (selected_menu_index & 3) * 24;
            menu_selection_right.y = 359 + (selected_menu_index >> 2) * 16;

            SpriteID switch_to = menu_item_list[selected_menu_index];
            if (switch_to == SpriteID.POTION && !blue_potion[current_save_file] && !red_potion[current_save_file])
                switch_to = SpriteID.MAP;

            current_B_item = switch_to;
        }

        // draw the big triforce on the menu screen in overworld
        static void DrawTriforce()
        {
            if (GetTriforceFlag(current_save_file, 0))
            {
                Textures.ppu[0x6af] = 0xe7;
                Textures.ppu[0x6ce] = 0xe7;
                Textures.ppu[0x6cf] = 0xf5;
            }

            if (GetTriforceFlag(current_save_file, 1))
            {
                Textures.ppu[0x6b0] = 0xe8;
                Textures.ppu[0x6d0] = 0xf5;
                Textures.ppu[0x6d1] = 0xe8;
            }

            if (GetTriforceFlag(current_save_file, 2))
            {
                Textures.ppu[0x6ed] = 0xe7;
                Textures.ppu[0x70c] = 0xe7;
                Textures.ppu[0x70d] = 0xf5;
            }

            if (GetTriforceFlag(current_save_file, 3))
            {
                Textures.ppu[0x6f2] = 0xe8;
                Textures.ppu[0x712] = 0xf5;
                Textures.ppu[0x713] = 0xe8;
            }

            bool piece_4 = GetTriforceFlag(current_save_file, 4);
            bool piece_5 = GetTriforceFlag(current_save_file, 5);
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

            bool piece_6 = GetTriforceFlag(current_save_file, 6);
            bool piece_7 = GetTriforceFlag(current_save_file, 7);
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

            int shift = (HUD_MAP_WIDTH - 1) * MASK_BIT_LEN;
            for (int i = 0; i < HUD_MAP_HEIGHT; i++)
            {
                for (int j = 0; j < HUD_MAP_WIDTH; j++)
                {
                    shift -= MASK_BIT_LEN;
                    Textures.ppu[HUD_MAP_OFFSET + i * Textures.PPU_WIDTH + j] = tile_locations[
                        (hud_dungeon_maps[DungeonCode.current_dungeon, i] & (BIT_MASK << shift)) >> shift
                    ];
                }
            }
        }
    }
}