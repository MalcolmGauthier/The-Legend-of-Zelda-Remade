using static The_Legend_of_Zelda.SaveLoad;
using static The_Legend_of_Zelda.Screen;

namespace The_Legend_of_Zelda
{
    internal static class OverworldCode
    {
        public enum OverworldEnemies
        {
            NONE,
            OCTOROK,
            OCTOROK_HARDER,
            TEKTITE,
            TEKTITE_HARDER,
            LEEVER,
            LEEVER_HARDER,
            MOBLIN,
            MOBLIN_HARDER,
            LYNEL,
            LYNEL_HARDER,
            ZORA,
            ARMOS, // spawned manually
            GHINI,
            ROCK,
            PEAHAT
        }

        const byte DEFAULT_SPAWN_ROOM = 119;
        public const byte LEVEL_7_ENTRANCE_ANIM_DONE = 255;
        const int SCROLL_ANIMATION_DONE = 500;
        const byte OPENING_ANIMATION_DONE = 255;

        public static byte current_screen = DEFAULT_SPAWN_ROOM;
        public static byte return_screen = DEFAULT_SPAWN_ROOM;
        public static byte level_7_entrance_timer = LEVEL_7_ENTRANCE_ANIM_DONE;
        static byte scroll_destination;
        static byte level_5_entrance_count = 0;
        static byte lost_woods_count = 0;
        static byte opening_animation_timer = 0;

        public static int return_x, return_y;
        public static int scroll_animation_timer = SCROLL_ANIMATION_DONE;
        public static int warp_animation_timer = 0;

        public static bool black_square_stairs_flag = false;
        public static bool black_square_stairs_return_flag = false;
        public static bool stair_warp_flag = false;
        public static bool fairy_animation_active = false;
        public static bool raft_flag = false;

        public static sbyte recorder_destination = 0;

        public static Direction scroll_direction;

        // list of rooms with stairs in them
        public static readonly byte[] stair_list = {
            11, 34, 26, 98, 28, 73, 29, 33, 35, 40, 52, 61, 66, 109, 70, 71, 72, 75, 77, 78, 81, 86, 91, 99, 104, 106, 107, 120, 121
        };
        // list of rooms with black squares in them
        public static readonly byte[] screens_with_secrets_list = {
            1, 3, 5, 7, 13, 16, 18, 19, 20, 22, 29, 30, 35, 39, 40, 44, 45, 51, 70, 71, 72, 73, 75, 77, 81, 86, 91, 98, 99, 103, 104,
            106, 107, 109, 113, 118, 120, 121, 123, 124, 125
        };
        // ppu indices of all overworld metatiles
        public static readonly byte[,] overworld_tileset_indexes = {
            {0xd8,0xda,0xd9,0xdb}, // rock
            {0x26,0x26,0x26,0x26}, // ground
            {0xce,0xd0,0xcf,0xd1}, // rock T
            {0x24,0x24,0x24,0x24}, // black hole
            {0xd0,0xd2,0xd1,0xd3}, // rock TR
            {0xcc,0xce,0xcd,0xcf}, // rock TL
            {0xdc,0xde,0xdd,0xdf}, // rock BR
            {0xd4,0xd6,0xd5,0xd7}, // rock BL
            {0xc8,0xca,0xc9,0xcb}, // rock snail
            {0xc4,0xc6,0xc5,0xc7}, // tree
            {0x90,0x90,0x95,0x95}, // water
            {0x8e,0x90,0x93,0x95}, // water R
            {0x8d,0x8f,0x8e,0x90}, // water TL
            {0x8f,0x8f,0x90,0x90}, // water T
            {0x8f,0x91,0x90,0x92}, // water TR
            {0x90,0x92,0x95,0x97}, // water L
            {0x95,0x97,0x96,0x98}, // water BR
            {0x95,0x95,0x96,0x96}, // water B
            {0x93,0x95,0x94,0x96}, // water BL
            {0x74,0x75,0x74,0x75}, // ladder
            {0x76,0x76,0x77,0x77}, // dock
            {0x70,0x72,0x71,0x73}, // stairs
            {0x84,0x86,0x85,0x87}, // sand
            {0x89,0x8b,0x8a,0x8c}, // waterfall
            {0x89,0x8b,0x88,0x88}, // waterfall bottom
            {0xbc,0xbe,0xbd,0xbf}, // tombstone
            {0xb0,0xb2,0xb1,0xb3}, // stump TL
            {0xac,0xae,0xad,0xaf}, // stump BL
            {0xb8,0xba,0xb9,0xbb}, // stump BR
            {0xaa,0xac,0xab,0xad}, // stump TR
            {0xb4,0xb6,0xb5,0xb7}, // stump face
            {0x9c,0x9e,0x9d,0x9f}, // ruins TL
            {0xa2,0xa4,0xa3,0xa5}, // ruins BL
            {0x9a,0x9c,0x9b,0x9d}, // ruins TR
            {0xa0,0xa2,0xa1,0xa3}, // ruins BR
            {0xe0,0xe2,0xe1,0xe3}, // ruins face 1 eye
            {0xa6,0xa8,0xa7,0xa9}, // ruins face 2 eyes
            {0xc0,0xc2,0xc1,0xc3}, // statue
            {0x7a,0x7c,0x7b,0x7d}, // water I TR
            {0x78,0x7a,0x79,0x7b}, // water I TL
            {0x7e,0x80,0x7f,0x81}, // water I BL
            {0x80,0x82,0x81,0x83}, // water I BR
        };
        // (room, y); x is always 128
        public static readonly byte[] dungeon_location_list = {
            55, 144, //1
            60, 176, //2
            116, 144,//3
            69, 144, //4
            11, 176, //5
            34, 144, //6
            66, 176, //7
            109, 96, //8
        };
        // list of all ennemies for each room, refer to enemy spawning function to learn to decipher this
        public static readonly int[] overworld_enemy_list =
        {
            0x000000, 0x999900, 0x999900, 0xeee000, 0xaaaaaa, 0xaa9956, 0xaa99ff, 0x900000, 0xeee000, 0x000000, 0xa00000, 0x500000, 0x333333, 0x333333, 0x000000, 0x000000,
            0xaaaa00, 0xaa99ff, 0xaaaaaa, 0xaa9900, 0x999900, 0xaa99ff, 0xeee000, 0xeee000, 0xeee000, 0xeee000, 0x333333, 0x000000, 0x000000, 0xffffff, 0x333333, 0xffffff,
            0x00000d, 0x00000d, 0x900000, 0x999900, 0x000000, 0xffff00, 0xffffff, 0xffffff, 0xffffff, 0x555500, 0x666666, 0x665fff, 0x333300, 0x111220, 0x222200, 0x000000,
            0x00000d, 0x00000d, 0xff99aa, 0x000000, 0x666600, 0x000000, 0x000000, 0x100000, 0x111112, 0x000000, 0x566fff, 0x666600, 0x200000, 0x888800, 0x111220, 0x200000,
            0x00000d, 0x00000d, 0x700000, 0x000000, 0x111100, 0xf00000, 0x000000, 0x000000, 0x555500, 0x111112, 0x444444, 0x777777, 0x111220, 0x888800, 0x888800, 0x111220,
            0x999990, 0x888880, 0x778800, 0x778880, 0x111100, 0x100000, 0x111100, 0x111110, 0x111000, 0xffff00, 0x111100, 0x778800, 0x888800, 0x888800, 0x888880, 0x111220,
            0x99aaff, 0x888880, 0x888880, 0x777822, 0x111220, 0x111100, 0x111100, 0x111100, 0x111100, 0x111100, 0x111100, 0x227778, 0x777700, 0x777700, 0x777822, 0x111120,
            0xffff00, 0x888880, 0x888700, 0x888800, 0x300000, 0xfff566, 0x333300, 0x000000, 0x111100, 0x444440, 0x444400, 0x555555, 0x555555, 0x222200, 0x111100, 0x200000
        };
        // screens where enemies spawn on the side instead of in the middle
        public static readonly byte[] overworld_screens_side_entrance = {
            75, 77, 78, 82, 87, 88, 91, 92, 93, 97, 98, 104, 107, 108, 109, 110, 113, 114, 115, 120
        };

        public static void Init()
        {
            Menu.draw_hud_objects = false;
            x_scroll = 0;
            y_scroll = 0;
            Menu.map_dot.shown = false;
            
            Textures.DrawHUDBG();
            Palettes.LoadPaletteGroup(PaletteID.BG_0, Palettes.PaletteGroups.GRAVEYARD_HUD1);
            Palettes.LoadPaletteGroup(PaletteID.BG_1, Palettes.PaletteGroups.HUD2);
            Palettes.LoadPaletteGroup(PaletteID.BG_2, Palettes.PaletteGroups.FOREST);
            Palettes.LoadPaletteGroup(PaletteID.BG_3, Palettes.PaletteGroups.MOUNTAIN);
            Palettes.LoadPaletteGroup(PaletteID.SP_0, Palettes.PaletteGroups.GREEN_LINK_HUDSPR1);
            if (red_ring)
                Palettes.LoadPalette(4, 1, Color._16_RED_ORANGE);
            else if (blue_ring)
                Palettes.LoadPalette(4, 1, Color._32_LIGHTER_INDIGO);
            Palettes.LoadPaletteGroup(PaletteID.SP_1, Palettes.PaletteGroups.HUDSPR_2);
            Palettes.LoadPaletteGroup(PaletteID.SP_2, Palettes.PaletteGroups.HUDSPR_3);
            Palettes.LoadPaletteGroup(PaletteID.SP_3, Palettes.PaletteGroups.OVERWORLD_DARK_ENEMIES);
            Textures.LoadNewRomData(Textures.ROMData.CHR_SURFACE);
            Menu.InitHUD();
            //Textures.LoadPPUPage(Textures.PPUDataGroup.OVERWORLD, current_screen, 0);
            Link.can_move = false;
            Link.Show(false);
            Menu.hud_sword.shown = false;
            Menu.hud_B_item.shown = false;
            opening_animation_timer = 0;
            warp_animation_timer = 0;
        }

        public static void Tick()
        {
            if (opening_animation_timer < OPENING_ANIMATION_DONE)
            {
                OpeningAnimation();
                return;
            }

            Menu.DrawHUD();
            Link.Tick();
            RaftAnimation();
            CheckForWarp();
            Menu.Tick();

            if (Menu.menu_open)
            {
                Program.can_pause = false;
            }
            else
            {
                if (current_screen == 128)
                {
                    Room128Code();
                }
                else
                {
                    Scroll();
                }
            }

            Level7EntranceAnimation();

            if (!Sound.IsMusicPlaying())
            {
                Sound.PlaySong(Sound.Songs.OVERWORLD, false);
                Sound.JumpTo(5.1f);
            }
        }

        static void OpeningAnimation()
        {
            if (opening_animation_timer == OPENING_ANIMATION_DONE)
            {
                Link.Show(true);
                Link.can_move = true;
                Menu.can_open_menu = true;
                Menu.draw_hud_objects = true;
                opening_animation_timer++;
                Sound.PlaySong(Sound.Songs.OVERWORLD, false);
                Program.can_pause = true;
                Menu.map_dot.shown = true;
                Menu.map_dot.x = 45;
                Menu.map_dot.y = 52;
                return;
            }

            Menu.can_open_menu = false;
            Link.can_move = false;
            Link.Show(false);
            Menu.draw_hud_objects = false;
            opening_animation_timer++;

            if (opening_animation_timer % 5 != 0)
            {
                return;
            }

            int num_rows_to_erase = 16 - opening_animation_timer / 5;
            if (num_rows_to_erase <= 0)
            {
                opening_animation_timer = OPENING_ANIMATION_DONE - 1;
                return;
            }

            Textures.LoadPPUPage(Textures.PPUDataGroup.OVERWORLD, current_screen, 0);
            int left_index_start = Textures.PPU_WIDTH * 5;
            int right_index_start = Textures.PPU_WIDTH * 6 - 1;

            for (int i = 0; i < num_rows_to_erase; i++)
            {
                for (int j = 0; j < 22; j++)
                {
                    Textures.ppu[left_index_start + j * Textures.PPU_WIDTH + i] = 0x24;
                    Textures.ppu[right_index_start + j * Textures.PPU_WIDTH - i] = 0x24;
                }
            }
        }

        static void Scroll()
        {
            if (scroll_animation_timer > 500)
            {
                if (Link.y < 64 && (Control.IsHeld(Buttons.UP) || raft_flag))
                {
                    scroll_destination = (byte)(current_screen - 16);
                    scroll_animation_timer = 0;
                    scroll_direction = Direction.UP;
                }
                else if (Link.y > 223 && (Control.IsHeld(Buttons.DOWN) || raft_flag))
                {
                    scroll_destination = (byte)(current_screen + 16);
                    scroll_animation_timer = 0;
                    scroll_direction = Direction.DOWN;
                }
                else if (Link.x < 1 && Control.IsHeld(Buttons.LEFT))
                {
                    scroll_destination = (byte)(current_screen - 1);
                    scroll_animation_timer = 0;
                    scroll_direction = Direction.LEFT;
                }
                else if (Link.x > 239 && (Control.IsHeld(Buttons.RIGHT) || Menu.tornado_out))
                {
                    scroll_destination = (byte)(current_screen + 1);
                    scroll_animation_timer = 0;
                    scroll_direction = Direction.RIGHT;
                }

                return;
            }

            if (scroll_animation_timer == 0)
            {
                if (Menu.tornado_out)
                {
                    scroll_destination = ChangeRecorderDestination(false);
                    scroll_direction = Direction.RIGHT;
                }
                Menu.can_open_menu = false;
                UnloadSpritesRoomTransition();
                ResetLinkPowerUps();

                if (current_screen == 27)
                {
                    if (scroll_direction == Direction.UP)
                    {
                        if (level_5_entrance_count == 3)
                        {
                            level_5_entrance_count = 0;
                            // todo: play secret discovered jingle
                        }
                        else
                        {
                            level_5_entrance_count++;
                            scroll_destination = 27;
                        }
                    }
                    else if (scroll_direction != Direction.LEFT)
                    {
                        scroll_destination = 27;
                        level_5_entrance_count = 0;
                    }
                    else
                    {
                        level_5_entrance_count = 0;
                    }
                }

                if (current_screen == 97)
                {
                    if (scroll_direction != Direction.RIGHT)
                    {
                        switch (lost_woods_count)
                        {
                            case 0:
                                if (scroll_direction == Direction.UP)
                                    lost_woods_count++;
                                else
                                    lost_woods_count = 0;
                                scroll_destination = 97;
                                break;
                            case 1:
                                if (scroll_direction == Direction.LEFT)
                                    lost_woods_count++;
                                else
                                    lost_woods_count = 0;
                                scroll_destination = 97;
                                break;
                            case 2:
                                if (scroll_direction == Direction.DOWN)
                                    lost_woods_count++;
                                else
                                    lost_woods_count = 0;
                                scroll_destination = 97;
                                break;
                            case 3:
                                if (scroll_direction == Direction.LEFT)
                                    scroll_destination = 96;
                                else
                                    scroll_destination = 97;
                                lost_woods_count = 0;
                                break;
                        }
                    }
                    else
                    {
                        lost_woods_count = 0;
                    }
                }

                if (scroll_direction == Direction.DOWN)
                {
                    Textures.LoadPPUPage(Textures.PPUDataGroup.OVERWORLD, scroll_destination, 1);
                }
                else if (scroll_direction == Direction.UP)
                {
                    Textures.LoadPPUPage(Textures.PPUDataGroup.OVERWORLD, current_screen, 1);
                    Textures.LoadPPUPage(Textures.PPUDataGroup.OVERWORLD, scroll_destination, 0);
                    y_scroll = 176;
                    Link.SetPos(new_y: 240);
                }
                else
                {
                    Textures.LoadPPUPage(Textures.PPUDataGroup.OVERWORLD, scroll_destination, 2);
                }
                Link.can_move = false;
            }

            if (scroll_direction == Direction.UP || scroll_direction == Direction.DOWN)
            {
                if ((Program.gTimer & 1) == 0)
                {
                    if (scroll_direction == Direction.UP)
                    {
                        y_scroll -= 7;
                        if (Program.gTimer % 3 == 0)
                            Link.SetPos(new_y: Link.y - 2);
                    }
                    else
                    {
                        y_scroll += 7;
                        if (Program.gTimer % 3 == 0)
                            Link.SetPos(new_y: Link.y + 2);
                        if (Link.y < 65)
                            Link.SetPos(new_y: 65);
                    }
                }

                if (scroll_animation_timer == 50)
                {
                    EndScroll();
                }
            }
            else
            {
                if (scroll_direction == Direction.LEFT)
                {
                    x_scroll -= 4;
                    if (Program.gTimer % 8 == 0)
                        Link.SetPos(new_x: Link.x - 2);
                    if (Link.x > 239)
                        Link.SetPos(new_x: 239);
                }
                else
                {
                    x_scroll += 4;
                    if (Program.gTimer % 4 == 0)
                        Link.SetPos(new_x: Link.x + 1);
                    if (Link.x < 1)
                        Link.SetPos(new_x: 1);
                }

                if (scroll_animation_timer == 64)
                {
                    EndScroll();
                }
            }

            if ((int)scroll_direction < 2)
                Link.current_action = (Link.Action)scroll_direction + 2;
            else
                Link.current_action = (Link.Action)scroll_direction - 2;

            scroll_animation_timer++;
            Link.animation_timer++;

            void EndScroll()
            {
                x_scroll = 0;
                y_scroll = 0;
                Textures.LoadPPUPage(Textures.PPUDataGroup.OVERWORLD, scroll_destination, 0);
                current_screen = scroll_destination;
                Link.can_move = true;
                Menu.blue_candle_limit_reached = false;
                if (scroll_direction == Direction.UP)
                    Link.SetPos(new_y: 223);
                else if (scroll_direction == Direction.DOWN)
                    Link.SetPos(new_y: 65);
                else if (scroll_direction == Direction.LEFT)
                    Link.SetPos(new_x: 239);
                else
                    Link.SetPos(new_x: 1);
                scroll_animation_timer += 1000;
                Menu.can_open_menu = true;

                SpawnEnemies();
                if (current_screen == 95 && !GetHeartContainerFlag(12))
                    new HeartContainerSprite(192, 144, 12);
            }
        }

        // spawn ennemies depending on current screen. most enemies are stored in overworld_enemy_list as ints where each 4 bits is the code of an enemy.
        // example: 0000 0000 0101 0101 0101 0101 0110 0110 -> 0 0 5 5 5 5 6 6 -> 4 red Leevers and 2 blue Leevers
        static void SpawnEnemies()
        {
            const int NUM_ENS_PER_SCRREN = 6;
            const int ENS_MEM_SIZE_BITS = 4;

            // screens with healing faires. they have nothing else.
            if (current_screen == 57 || current_screen == 67)
            {
                new OverworldFairySprite();
                return;
            }

            int enemies = overworld_enemy_list[current_screen];
            int enemy_mask = (int)Math.Pow(Math.Pow(2, ENS_MEM_SIZE_BITS), NUM_ENS_PER_SCRREN); // (2^4)^6 == 15728640 == 0xF00000 <- mask
            int right_shift_ammount = (NUM_ENS_PER_SCRREN - 1) * ENS_MEM_SIZE_BITS;

            for (int i = 0; i < NUM_ENS_PER_SCRREN; i++)
            {
                right_shift_ammount -= ENS_MEM_SIZE_BITS;

                switch ((OverworldEnemies)((enemies & enemy_mask) >> right_shift_ammount))
                {
                    case OverworldEnemies.NONE:
                        break;

                    case OverworldEnemies.OCTOROK:
                        new Octorok(false);
                        break;

                    case OverworldEnemies.OCTOROK_HARDER:
                        new Octorok(true);
                        break;

                    case OverworldEnemies.MOBLIN:
                        new Moblin(false);
                        break;

                    case OverworldEnemies.MOBLIN_HARDER:
                        new Moblin(true);
                        break;

                    case OverworldEnemies.LYNEL:
                        new Lynel(false);
                        break;

                    case OverworldEnemies.LYNEL_HARDER:
                        new Lynel(true);
                        break;

                    case OverworldEnemies.LEEVER:
                        new Leever(false);
                        break;

                    case OverworldEnemies.LEEVER_HARDER:
                        new Leever(true);
                        break;

                    case OverworldEnemies.ROCK:
                        new Rock();
                        break;

                    case OverworldEnemies.TEKTITE:
                        new Tektite(false);
                        break;

                    case OverworldEnemies.TEKTITE_HARDER:
                        new Tektite(true);
                        break;

                    case OverworldEnemies.PEAHAT:
                        new Peahat();
                        break;

                    case OverworldEnemies.GHINI:
                        new Ghini(true);
                        break;
                }

                enemy_mask >>= 4;
            }

            //TODO: find a better way of checking for water. like, a readonly list
            if ((meta_tiles[1].tile_index == 0xa || meta_tiles[14].tile_index == 0xa || meta_tiles[161].tile_index == 0xa || meta_tiles[174].tile_index == 0xa ||
                meta_tiles[69].tile_index == 0xa || meta_tiles[70].tile_index == 0xc || meta_tiles[64].tile_index == 0x11 || meta_tiles[166].tile_index == 0xa)
                && current_screen != 14)
            {
                new Zora();
            }
        }

        // returns current recorder destination, and changes it if change_value is set
        public static byte ChangeRecorderDestination(bool change_the_value)
        {
            for (byte i = 0; i < 8; i++)
            {
                if (change_the_value)
                {
                    if (Link.facing_direction == Direction.DOWN || Link.facing_direction == Direction.RIGHT)
                        recorder_destination++;
                    else
                        recorder_destination--;

                    recorder_destination %= 8;
                }

                if (GetTriforceFlag(i))
                {
                    return dungeon_location_list[recorder_destination * 2]; // returns y coord of dungeon entrance
                }
            }

            return 0;
        }

        static void CheckForWarp()
        {
            if (black_square_stairs_return_flag)
            {
                BlackSquareLinkAnimation(false);
                return;
            }
            
            if (black_square_stairs_flag)
            {
                BlackSquareLinkAnimation(true);
                return;
            }

            if (!stair_warp_flag)
                return;

            if (current_screen != 128)
            {
                warp_animation_timer = 65;
                BlackSquareLinkAnimation(true);
                Link.current_action = Link.Action.WALKING_UP;
                Sound.PauseMusic();

            }

            if (Link.y < 200)
            {
                switch (return_screen)
                {
                    case 29:
                        if (Link.x < 100)
                            return_screen = 35;
                        else if (Link.x > 150)
                            return_screen = 121;
                        else
                            return_screen = 73;
                        break;
                    case 35:
                        if (Link.x < 100)
                            return_screen = 73;
                        else if (Link.x > 150)
                            return_screen = 29;
                        else
                            return_screen = 121;
                        break;
                    case 73:
                        if (Link.x < 100)
                            return_screen = 121;
                        else if (Link.x > 150)
                            return_screen = 35;
                        else
                            return_screen = 29;
                        break;
                    case 121:
                        if (Link.x < 100)
                            return_screen = 29;
                        else if (Link.x > 150)
                            return_screen = 73;
                        else
                            return_screen = 35;
                        break;
                }
            }
            BlackSquareLinkAnimation(false, true);
            warp_animation_timer = 0;
            stair_warp_flag = false;
            Link.SetBGState(false);
            Link.can_move = true;
        }

        // code that executes when link is in an underground room (room 128)
        static void Room128Code()
        {
            //TODO: make this only happen once on load
            if (Sound.IsMusicPlaying())
                Sound.PauseMusic();

            // most logic for this room is found in seperate class, because there's alot
            WarpCode.Tick();

            // check if link is leaving
            if (Link.y >= 224)
            {
                if (stair_list.Contains(return_screen))
                {
                    BlackSquareLinkAnimation(false, true);
                    warp_animation_timer = 0;
                    stair_warp_flag = false;
                    Link.SetBGState(false);
                    Link.can_move = true;
                }
                else
                {
                    black_square_stairs_return_flag = true;
                }
            }

            if (!Menu.menu_open)
                Menu.can_open_menu = Link.can_move;
        }

        // unloads all sprites that should unload when screen changes
        static void UnloadSpritesRoomTransition()
        {
            foreach (Sprite s in sprites)
            {
                if (s.unload_during_transition)
                    sprites.Remove(s);
            }
        }

        // code for animation that plays with black square on overworld
        public static void BlackSquareLinkAnimation(bool entering, bool immediate_exit = false)
        {
            if (warp_animation_timer == 0)
            {
                ResetLinkPowerUps();
                Menu.can_open_menu = false;
                Link.can_move = false;
                Program.can_pause = false;
                Link.SetBGState(true);
                if (entering)
                {
                    Link.current_action = Link.Action.WALKING_UP;
                    UnloadSpritesRoomTransition();
                    Sound.PauseMusic();
                    Sound.PlaySFX(Sound.SoundEffects.STAIRS, true);
                }
                else
                {
                    Link.current_action = Link.Action.WALKING_DOWN;
                    if (WarpCode.warp_info == WarpCode.WarpType.TAKE_ANY_ROAD)
                        WarpCode.SetWarpReturnPosition();
                    Link.SetPos(return_x, return_y);
                    Palettes.LoadPaletteGroup(PaletteID.BG_3, Palettes.PaletteGroups.MOUNTAIN);
                    Palettes.LoadPalette(2, 1, Color._1A_SEMI_LIGHT_GREEN);
                    Palettes.LoadPalette(2, 2, Color._37_BEIGE);
                    Textures.LoadPPUPage(Textures.PPUDataGroup.OVERWORLD, return_screen, 0);
                    current_screen = return_screen; // keep this after ppu page load or else bombable/burnable tiles fuck up their display
                    Link.SetBGState(true);
                    UnloadSpritesRoomTransition();
                    WarpCode.fire_appeared = false;

                    // if link exits the underground immediately, skip leaving animation, go directly to anim_timer = 72 next frame
                    if (immediate_exit)
                    {
                        warp_animation_timer = 71;
                        SpawnEnemies();
                        Program.can_pause = true;
                        Sound.PlaySong(Sound.Songs.OVERWORLD, false);
                    }
                    else
                    {
                        Sound.PlaySFX(Sound.SoundEffects.STAIRS, true);
                    }
                }
            }
            else if (warp_animation_timer == 65)
            {
                if (entering)
                {
                    UnloadSpritesRoomTransition();
                    Palettes.LoadPaletteGroup(PaletteID.BG_3, Palettes.PaletteGroups.OVERWORLD_CAVE);
                    Palettes.LoadPalette(2, 1, Color._30_WHITE);
                    Palettes.LoadPalette(2, 2, Color._0F_BLACK);
                    WarpCode.SetWarpReturnPosition();
                    if (WarpCode.screen_warp_info[current_screen] != 0xe)
                    {
                        Textures.LoadPPUPage(Textures.PPUDataGroup.OVERWORLD, 128, 0);
                        sprites.Add(new UndergroundFireSprite(0x5c, 5, 72, 128));
                        sprites.Add(new UndergroundFireSprite(0x5c, 5, 168, 128));
                        Link.SetPos(112, 223);
                    }
                    else
                    {
                        Textures.LoadPPUPage(Textures.PPUDataGroup.OTHER, 1, 0);
                        return_x = Link.x;
                        return_y = Link.y;
                        Link.SetPos(120, 223);
                    }
                    return_screen = current_screen;
                    current_screen = 128;
                    Link.SetBGState(false);
                    WarpCode.Init(true);
                }
                else
                {
                    int overshoot_protection = Link.y & 15;
                    if (overshoot_protection >= 8)
                        Link.SetPos(new_y: Link.y + (16 - overshoot_protection));
                    else
                        Link.SetPos(new_y: Link.y - overshoot_protection);
                    black_square_stairs_return_flag = false;
                    Link.SetBGState(false);
                    Link.can_move = true;
                    Link.has_moved_after_warp_flag = false;
                    warp_animation_timer = 0;
                    Program.can_pause = true;
                    Menu.can_open_menu = true;
                    Sound.PlaySong(Sound.Songs.OVERWORLD, false);
                    SpawnEnemies();
                    return;
                }
            }
            else if (warp_animation_timer == 72)
            {
                black_square_stairs_flag = false;
                warp_animation_timer = 0;
                Link.can_move = false;
                Program.can_pause = true;
                return;
            }

            if (Program.gTimer % 4 == 0 && warp_animation_timer <= 64)
                Link.SetPos(new_y: Link.y + (entering ? 1 : -1));

            if (warp_animation_timer > 64)
                Link.SetPos(new_y: Link.y - 1 - (Program.gTimer % 2));

            Link.animation_timer++;
            warp_animation_timer++;
        }

        static void RaftAnimation()
        {
            if (!raft_flag)
                return;

            if (Link.facing_direction == Direction.UP)
                Link.SetPos(new_y: Link.y - 1);
            else
                Link.SetPos(new_y: Link.y + 1);

            Link.animation_timer++;
        }

        static void Level7EntranceAnimation()
        {
            if (level_7_entrance_timer == 255)
                return;

            switch (level_7_entrance_timer)
            {
                case 0:
                    Palettes.LoadPalette(3, 3, Color._12_SMEI_DARK_BLUE);
                    break;
                case 1:
                    Palettes.LoadPalette(3, 3, Color._11_SEMI_LIGHT_BLUE);
                    break;
                case 9:
                    Palettes.LoadPalette(3, 3, Color._22_LIGHT_INDIGO);
                    break;
                case 17:
                    Palettes.LoadPalette(3, 3, Color._21_LIGHT_BLUE);
                    break;
                case 25:
                    Palettes.LoadPalette(3, 3, Color._31_LIGHTER_BLUE);
                    break;
                case 33:
                    Palettes.LoadPalette(3, 3, Color._32_LIGHTER_INDIGO);
                    break;
                case 41:
                    Palettes.LoadPalette(3, 3, Color._33_LIGHTER_PURPLE);
                    break;
                case 49:
                    Palettes.LoadPalette(3, 3, Color._35_LIGHTER_RED);
                    break;
                case 57:
                    Palettes.LoadPalette(3, 3, Color._34_LIGHTER_PINK);
                    break;
                case 65:
                    Palettes.LoadPalette(3, 3, Color._36_LIGHTER_ORANGE);
                    break;
                case 73:
                    Palettes.LoadPalette(3, 3, Color._37_BEIGE);
                    break;
                case 81:
                    int mt_index = 53;
                    // make it so you can walk on the sand by changing metatile index to ground
                    for (int i = 0; i < 4 * MetaTile.METATILES_PER_ROW; i += MetaTile.METATILES_PER_ROW)
                    {
                        for (int j = 0; j < 6; j++)
                        {
                            meta_tiles[i + j + mt_index].tile_index = 1;
                        }
                    }
                    meta_tiles[86].tile_index = 0x15;
                    Textures.ppu[18 * 32 + 13] = 0x70;
                    Textures.ppu[18 * 32 + 14] = 0x72;
                    Textures.ppu[19 * 32 + 13] = 0x71;
                    Textures.ppu[19 * 32 + 14] = 0x73;
                    // TODO: play secret sfx
                    break;
            }

            if (scroll_animation_timer < 500 && level_7_entrance_timer != 255)
            {
                if (level_7_entrance_timer == 0)
                {
                    Link.can_move = false;
                    level_7_entrance_timer = 255;
                    return;
                }

                scroll_animation_timer = 0;
                y_scroll = 0;
                Link.SetPos(new_y: 224);
                Link.animation_timer = 0;
                level_7_entrance_timer--;
            }
            else if (level_7_entrance_timer <= 82)
            {
                level_7_entrance_timer++;
            }
        }

        // reset all of the variables keeping track of which items link has thrown onto the field
        // and other stuff that needs to go away on a screen transition
        static void ResetLinkPowerUps()
        {
            Menu.fire_out = 0;
            Menu.boomerang_out = false;
            Menu.arrow_out = false;
            Menu.magic_wave_out = false;
            Menu.bait_out = false;
            Menu.bomb_out = false;
            // if link is in the tornado, we don't want to destroy the tornado
            if (Link.shown) Menu.tornado_out = false;
            Menu.sword_proj_out = false;
            Link.clock_flash = false;
            Link.iframes_timer = 0;
            Link.self.palette_index = 4;
            Link.counterpart.palette_index = 4;
        }
    }
}