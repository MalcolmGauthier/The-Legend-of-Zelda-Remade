using System.Diagnostics.CodeAnalysis;
using The_Legend_of_Zelda.Rendering;
using The_Legend_of_Zelda.Sprites;
using static The_Legend_of_Zelda.SaveLoad;
using static The_Legend_of_Zelda.Rendering.Screen;
using static The_Legend_of_Zelda.Gameplay.Program;

namespace The_Legend_of_Zelda.Gameplay
{
    public sealed class OverworldCode : GameplayCode
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

        public const byte DEFAULT_SPAWN_ROOM = 55;
        public const byte LEVEL_7_ENTRANCE_ANIM_DONE = 255;

        public byte return_screen = DEFAULT_SPAWN_ROOM;
        public byte level_7_entrance_timer { get; private set; } = LEVEL_7_ENTRANCE_ANIM_DONE;
        byte level_5_entrance_count = 0;
        byte lost_woods_count = 0;

        public int return_x, return_y;
        public int warp_animation_timer = 0;

        public bool black_square_stairs_flag = false;
        public bool black_square_stairs_return_flag = false;
        public bool stair_warp_flag = false;
        public bool fairy_animation_active = false;
        public bool raft_flag = false;

        public int recorder_destination = 0;

        // list of rooms with stairs in them
        public readonly byte[] stair_list = {
            11, 34, 26, 98, 28, 73, 29, 33, 35, 40, 52, 61, 66, 109, 70, 71, 72, 75, 77, 78, 81, 86, 91, 99, 104, 106, 107, 120, 121
        };
        // list of rooms with black squares in them
        public readonly byte[] screens_with_secrets_list = {
            1, 3, 5, 7, 13, 16, 18, 19, 20, 22, 29, 30, 35, 39, 40, 44, 45, 51, 70, 71, 72, 73, 75, 77, 81, 86, 91, 98, 99, 103, 104,
            106, 107, 109, 113, 118, 120, 121, 123, 124, 125
        };
        // (room, y); x is always 128. this is for recorder
        public readonly byte[] dungeon_location_list = {
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
        readonly int[] overworld_enemy_list =
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
        public readonly byte[] overworld_screens_side_entrance = {
            75, 77, 78, 82, 87, 88, 91, 92, 93, 97, 98, 104, 107, 108, 109, 110, 113, 114, 115, 120
        };
        // screens with zora (not all with water)
        readonly byte[] screens_with_zora =
        {
            10, 23, 24, 25, 26, 30, 38, 39, 40, 45, 46, 53, 54, 56, 62, 63, 68, 70, 71, 72, 79,
            84, 85, 86, 89, 90, 95, 101, 105, 106, 111, 117, 123, 124, 125, 126, 127
        };
        // screens with sea noise (not all with water)
        readonly byte[] screens_with_sea = { };
        // screens with ladder perms (not all with water)
        public readonly byte[] screens_with_ladder_permissions = {
            23, 24, 25, 39, 79, 95};

        public void Init()
        {
            Menu.draw_hud_objects = false;
            Menu.map_dot.shown = false;
            x_scroll = 0;
            y_scroll = 0;

            Textures.DrawHUDBG();
            Palettes.LoadPaletteGroup(PaletteID.BG_0, Palettes.PaletteGroups.GRAVEYARD_HUD1);
            Palettes.LoadPaletteGroup(PaletteID.BG_1, Palettes.PaletteGroups.HUD2);
            Palettes.LoadPaletteGroup(PaletteID.BG_2, Palettes.PaletteGroups.FOREST);
            Palettes.LoadPaletteGroup(PaletteID.BG_3, Palettes.PaletteGroups.MOUNTAIN);
            Palettes.LoadPaletteGroup(PaletteID.SP_0, Palettes.PaletteGroups.GREEN_LINK_HUDSPR1);
            if (red_ring)
                Palettes.LoadPalette(PaletteID.SP_0, 1, Color._16_RED_ORANGE);
            else if (blue_ring)
                Palettes.LoadPalette(PaletteID.SP_0, 1, Color._32_LIGHTER_INDIGO);
            Palettes.LoadPaletteGroup(PaletteID.SP_1, Palettes.PaletteGroups.HUDSPR_2);
            Palettes.LoadPaletteGroup(PaletteID.SP_2, Palettes.PaletteGroups.HUDSPR_3);
            Palettes.LoadPaletteGroup(PaletteID.SP_3, Palettes.PaletteGroups.OVERWORLD_DARK_ENEMIES);
            Textures.LoadNewRomData(Textures.ROMData.CHR_SURFACE);
            Menu.InitHUD();
            UpdateBGMusic(Sound.Songs.OVERWORLD, 5.1f);
            //Textures.LoadPPUPage(Textures.PPUDataGroup.OVERWORLD, current_screen, 0);
            Link.can_move = false;
            Link.Show(false);
            Link.self.dungeon_wall_mask = false;
            Link.counterpart.dungeon_wall_mask = false;
            Menu.hud_sword.shown = false;
            Menu.hud_B_item.shown = false;
            opening_animation_timer = 0;
            warp_animation_timer = 0;
            // detect initialization
            if (current_screen == 0)
                current_screen = DEFAULT_SPAWN_ROOM;
        }

        protected override void SpecificCode()
        {
            RaftAnimation();
            CheckForWarp();

            if (!Menu.menu_open)
            {
                if (current_screen == 128)
                {
                    // code that executes when link is in an underground room (room 128)
                    NPCCode.Tick();
                }
                else
                {
                    Scroll(raft_flag);
                }
            }

            Level7EntranceAnimation();
        }

        protected override bool SpecificScrollCode(bool scroll_finished)
        {
            if (scroll_finished)
            {
                SpawnEnemies();
                if (current_screen == 95 && !GetHeartContainerFlag(12))
                    new HeartContainerSprite(192, 144, 12);

                // check if scrolled onto water raft tile (which is a full water tile with special flag)
                MetaTile mt = meta_tiles[GetMetaTileIndexAtLocation(Link.x + 8, Link.y + 8)];
                if (mt.tile_index == MetatileType.WATER && mt.special)
                {
                    // force raft activation
                    new RaftSprite();
                    raft_flag = true;
                    Link.can_move = false;
                }
                return false;
            }

            if (Menu.tornado_out)
            {
                scroll_destination = RecorderDestination(false);
            }

            // special screen. going right, up or down loops, going left exits and going up 4 times in a row exits
            if (current_screen == 27)
            {
                const int NUM_OF_SCROLLS_TO_EXIT = 4;

                if (scroll_direction == Direction.UP)
                    level_5_entrance_count++;

                if (scroll_direction != Direction.LEFT && !(scroll_direction == Direction.UP && level_5_entrance_count >= NUM_OF_SCROLLS_TO_EXIT))
                    scroll_destination = 27;

                if (scroll_direction != Direction.UP || level_5_entrance_count >= NUM_OF_SCROLLS_TO_EXIT)
                    level_5_entrance_count = 0;
            }

            // special screen. going left, up or down loops, going right exits, and going up->left->down->left exits
            if (current_screen == 97)
            {
                Direction[] LOST_FOREST_CODE = { Direction.UP, Direction.LEFT, Direction.DOWN, Direction.LEFT };

                if (scroll_direction == LOST_FOREST_CODE[lost_woods_count])
                    lost_woods_count++;
                else
                    lost_woods_count = 0;

                if (scroll_direction != Direction.RIGHT && !(scroll_direction == Direction.LEFT && lost_woods_count == LOST_FOREST_CODE.Length))
                    scroll_destination = 97;

                if (lost_woods_count >= LOST_FOREST_CODE.Length)
                    lost_woods_count = 0;
            }

            return true;
        }

        // spawn ennemies depending on current screen. most enemies are stored in overworld_enemy_list as ints where each 4 bits is the code of an enemy.
        // example: 0000 0000 0101 0101 0101 0101 0110 0110 -> 0 0 5 5 5 5 6 6 -> 4 red Leevers and 2 blue Leevers
        public void SpawnEnemies()
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
            int right_shift_ammount = (NUM_ENS_PER_SCRREN - 1) * ENS_MEM_SIZE_BITS;
            int enemy_mask = (int)Math.Pow(2, ENS_MEM_SIZE_BITS) - 1 << right_shift_ammount; // (2^4)<<20 == 15728640 == 0xF00000 <- mask
            List<int> ignore_list = new();

            for (int i = 0; i < NUM_ENS_PER_SCRREN; i++)
            {
                int kq = IsInKillQueue(current_screen, ignore_list.ToArray());
                if (kq != -1)
                {
                    ignore_list.Add(kq);
                    right_shift_ammount -= ENS_MEM_SIZE_BITS;
                    enemy_mask >>= 4;
                    continue;
                }

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

                right_shift_ammount -= ENS_MEM_SIZE_BITS;
                enemy_mask >>= 4;
            }

            if (screens_with_zora.Contains(current_screen))
            {
                new Zora();
            }
        }

        // returns current recorder destination, and changes it if change_value is set
        public byte RecorderDestination(bool change_the_value)
        {
            for (byte i = 0; i < 8; i++)
            {
                if (change_the_value)
                {
                    if (Link.facing_direction == Direction.UP || Link.facing_direction == Direction.RIGHT)
                        recorder_destination++;
                    else
                        recorder_destination--;

                    recorder_destination = TrueMod(recorder_destination, 8);
                }

                if (GetTriforceFlag(i))
                {
                    return dungeon_location_list[recorder_destination * 2]; // returns room id of dungeon entrance
                }
            }

            return 0;
        }

        void CheckForWarp()
        {
            // when link has returned from r128 and is walking up
            if (black_square_stairs_return_flag)
            {
                LinkWalkAnimation(false);
                return;
            }

            // when link is walking down into r128
            if (black_square_stairs_flag)
            {
                LinkWalkAnimation(true);
                return;
            }

            // when link touches stairs. instant warp. r128 warp code handled in NPCCode.
            if (stair_warp_flag && current_screen != 128)
            {
                NPCCode.Init();
                Sound.PauseMusic();
                return;
            }
        }

        // code for animation that plays with warps on overworld
        public void LinkWalkAnimation(bool entering)
        {
            if (warp_animation_timer == 0)
            {
                ResetLinkPowerUps();
                Menu.can_open_menu = false;
                Link.can_move = false;
                Program.can_pause = false;
                Link.SetBGState(true);
                Link.current_action = entering ? LinkAction.WALKING_UP : LinkAction.WALKING_DOWN;
                UnloadSpritesRoomTransition();
                Sound.PauseMusic();
                //Sound.EndAllSFX(); //TODO
                Sound.PlaySFX(Sound.SoundEffects.STAIRS, true);
            }
            else if (warp_animation_timer <= 64)
            {
                if (Program.gTimer % 4 == 0)
                {
                    Link.SetPos(new_y: Link.y + (entering ? 1 : -1));
                }
                Link.animation_timer++;
            }
            else if (warp_animation_timer == 65)
            {
                if (entering)
                {
                    NPCCode.Init();
                    return;
                }

                int overshoot_protection = Link.y % 16;
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

            warp_animation_timer++;
        }

        void RaftAnimation()
        {
            if (!raft_flag)
                return;

            if (Link.facing_direction == Direction.UP)
                Link.SetPos(new_y: Link.y - 1);
            else
                Link.SetPos(new_y: Link.y + 1);

            Link.animation_timer++;
        }

        public void ActivateLevel7Animation() => level_7_entrance_timer = 0;
        void Level7EntranceAnimation()
        {
            if (current_screen != 66)
                return;

            switch (level_7_entrance_timer)
            {
                case 0:
                    Palettes.LoadPalette(PaletteID.BG_3, 3, Color._12_SMEI_DARK_BLUE);
                    break;
                case 1:
                    Palettes.LoadPalette(PaletteID.BG_3, 3, Color._11_SEMI_LIGHT_BLUE);
                    break;
                case 9:
                    Palettes.LoadPalette(PaletteID.BG_3, 3, Color._22_LIGHT_INDIGO);
                    break;
                case 17:
                    Palettes.LoadPalette(PaletteID.BG_3, 3, Color._21_LIGHT_BLUE);
                    break;
                case 25:
                    Palettes.LoadPalette(PaletteID.BG_3, 3, Color._31_LIGHTER_BLUE);
                    break;
                case 33:
                    Palettes.LoadPalette(PaletteID.BG_3, 3, Color._32_LIGHTER_INDIGO);
                    break;
                case 41:
                    Palettes.LoadPalette(PaletteID.BG_3, 3, Color._33_LIGHTER_PURPLE);
                    break;
                case 49:
                    Palettes.LoadPalette(PaletteID.BG_3, 3, Color._35_LIGHTER_RED);
                    break;
                case 57:
                    Palettes.LoadPalette(PaletteID.BG_3, 3, Color._34_LIGHTER_PINK);
                    break;
                case 65:
                    Palettes.LoadPalette(PaletteID.BG_3, 3, Color._36_LIGHTER_ORANGE);
                    break;
                case 73:
                    Palettes.LoadPalette(PaletteID.BG_3, 3, Color._37_BEIGE);
                    break;
                case 81:
                    int mt_index = 53;
                    // make it so you can walk on the sand by changing metatile index to ground
                    for (int i = 0; i < 4 * MetaTile.METATILES_PER_ROW; i += MetaTile.METATILES_PER_ROW)
                    {
                        for (int j = 0; j < 6; j++)
                        {
                            meta_tiles[i + j + mt_index].tile_index = MetatileType.GROUND;
                        }
                    }
                    // the actual entrance
                    meta_tiles[86].tile_index = MetatileType.STAIRS;
                    Textures.ppu[18 * 32 + 13] = 0x70;
                    Textures.ppu[18 * 32 + 14] = 0x72;
                    Textures.ppu[19 * 32 + 13] = 0x71;
                    Textures.ppu[19 * 32 + 14] = 0x73;
                    // TODO: play secret sfx
                    //level_7_entrance_timer = LEVEL_7_ENTRANCE_ANIM_DONE;
                    break;
            }

            // if link exits level 7 entrance with animation done, the animation plays in reverse before the scrolling finishes
            if (!ScrollingDone() && level_7_entrance_timer != LEVEL_7_ENTRANCE_ANIM_DONE)
            {
                if (level_7_entrance_timer == 0)
                {
                    Link.can_move = false;
                    level_7_entrance_timer = LEVEL_7_ENTRANCE_ANIM_DONE;
                    return;
                }

                //if (level_7_entrance_timer == LEVEL_7_ENTRANCE_ANIM_DONE)
                //{
                //    level_7_entrance_timer = 81;
                //}

                // force the scroll animation to not advance
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

        // sets the exact x and y coordinates that link will return to when exiting
        public void SetWarpReturnPosition(byte screen, bool stair_entrance)
        {
            Dictionary<byte, (int x, int y)> warpPositions = new()
            {
                { 11, (112, 128) },
                { 34, (112, 128) },
                { 26, (96, 128) },
                { 98, (96, 128) },
                { 28, (48, 112) },
                { 73, (48, 112) },
                { 29, (32, 112) },
                { 33, (80, 112) },
                { 35, (64, 96) },
                { 40, (224, 160) },
                { 52, (64, 112) },
                { 61, (144, 112) },
                { 66, (96, 96) },
                { 109, (96, 96) },
                { 70, (128, 176) },
                { 71, (176, 160) },
                { 72, (176, 112) },
                { 75, (192, 96) },
                { 77, (192, 144) },
                { 78, (112, 112) },
                { 81, (160, 160) },
                { 86, (176, 176) },
                { 91, (48, 160) },
                { 99, (112, 160) },
                { 104, (48, 144) },
                { 106, (208, 160) },
                { 107, (80, 160) },
                { 120, (80, 144) },
                { 121, (96, 112) }
            };

            if (warpPositions.TryGetValue(screen, out (int x, int y) position) && stair_entrance)
            {
                return_x = position.x;
                return_y = position.y;
            }
            else
            {
                return_x = Link.x;
                return_y = Link.y;
            }
        }
    }
}