﻿using static The_Legend_of_Zelda.Screen;
using static The_Legend_of_Zelda.Control;
using static The_Legend_of_Zelda.SaveLoad;
using static The_Legend_of_Zelda.Program;
using System.Reflection;

namespace The_Legend_of_Zelda
{
    public enum Direction
    {
        UP,
        DOWN,
        LEFT,
        RIGHT
    }
    
    public static class Link
    {
        public static int x, y;
        public static int animation_timer = 0;
        static int safe_x, safe_y;

        public static float hp = 3;

        public static byte nb_of_ens_killed = 0;
        public static byte nb_of_ens_killed_damageless = 0;
        public static byte iframes_timer;
        public static byte knockback_timer;
        static byte longer_attack_anim = 0;
        static byte dungeon_wall_push_timer = 0;

        public static bool ladder_used = false;
        public static bool can_move = false;
        public static bool has_moved_after_warp_flag = true;
        public static bool sword_out = false;
        public static bool wand_out = false;
        public static bool using_item = false;
        public static bool shown;
        public static bool full_heal_flag = false;
        public static bool clock_flash = false;
        static bool stair_speed = false;

        public static Direction facing_direction;
        public static Direction knockback_direction;
        public static Action current_action;

        public static StaticSprite self = new StaticSprite(0x00, 4, 0, 0);
        public static StaticSprite counterpart = new StaticSprite(0x00, 4, 8, 0);
        static StaticSprite sword_1 = new StaticSprite(0x20, 4, 0, 0);
        static StaticSprite sword_2 = new StaticSprite(0x20, 4, 0, 0);

        public enum Action
        {
            WALKING_LEFT,
            WALKING_RIGHT,
            WALKING_UP,
            WALKING_DOWN,
            ATTACK_LEFT,
            ATTACK_RIGHT,
            ATTACK_UP,
            ATTACK_DOWN,
            ITEM_GET,
            ITEM_HELD_UP,
        }

        public static void Init()
        {
            shown = false;

            if (!sprites.Contains(self))
                sprites.Add(self);
            if (!sprites.Contains(counterpart))
                sprites.Add(counterpart);

            current_action = Action.WALKING_UP;
            facing_direction = Direction.UP;
            SetPos(120, 144);

            hp = 3;

            iframes_timer = 0;
            knockback_timer = 0;
            self.palette_index = 4;
            counterpart.palette_index = 4;
            sword_out = false;
            wand_out = false;
            Tick();
        }

        public static void Tick()
        {
            CheckIfRecorderPlaying();

            if (full_heal_flag)
            {
                FillHealth();
            }
            else if (can_move)
            {
                HitFlash();

                if (Knockback())
                    return;

                if (AButton())
                {
                    Animation();
                    return;
                }

                if (BButton())
                {
                    Animation();
                    return;
                }

                Move();

                if (shown)
                    Collision();

                BButton();
            }
            
            Animation();

            hp = Math.Clamp(hp, 0, nb_of_hearts);
            
            counterpart.x = self.x + 8;
            counterpart.y = self.y;
            x = self.x;
            y = self.y;
            
            // debug section
            //hp = 3f;
            blue_candle = true;
            magical_key = true;
            recorder = true;
            // c# doesn't have c++'s friend keyword, so fuck you c#, et reflection and die :)
            // (this is only used for testing ofc)
            FieldInfo? lol = typeof(SaveLoad).GetField("map_flags", BindingFlags.NonPublic | BindingFlags.Static);
            lol?.SetValue(null, new ushort[]{ 0xffff, 0, 0});
            bomb_count = 8;
            ladder = true;
            raft = true;
            boomerang = true;
        }

        static void CheckIfRecorderPlaying()
        {
            if (Sound.recorder_playing && !Sound.RecorderPlaying())
            {
                can_move = true;
                Menu.can_open_menu = true;
            }
        }

        static void Move()
        {
            if (IsHeld(Buttons.UP) ^ IsHeld(Buttons.DOWN))
            {
                animation_timer++;
                if (IsHeld(Buttons.UP))
                {
                    current_action = Action.WALKING_UP;
                    ChangePos(false, false);
                    if (facing_direction == Direction.LEFT || facing_direction == Direction.RIGHT)
                        GotoMod8();
                    facing_direction = Direction.UP;
                }
                else
                {
                    current_action = Action.WALKING_DOWN;
                    ChangePos(false, true);
                    if (facing_direction == Direction.LEFT || facing_direction == Direction.RIGHT)
                        GotoMod8();
                    facing_direction = Direction.DOWN;
                }
            }
            else if (IsHeld(Buttons.LEFT) ^ IsHeld(Buttons.RIGHT))
            {
                animation_timer++;
                if (IsHeld(Buttons.LEFT))
                {
                    current_action = Action.WALKING_LEFT;
                    ChangePos(true, false);
                    if (facing_direction == Direction.UP || facing_direction == Direction.DOWN)
                        GotoMod8();
                    facing_direction = Direction.LEFT;
                }
                else
                {
                    current_action = Action.WALKING_RIGHT;
                    ChangePos(true, true);
                    if (facing_direction == Direction.UP || facing_direction == Direction.DOWN)
                        GotoMod8();
                    facing_direction = Direction.RIGHT;
                }
            }
        }

        static void Attack()
        {
            if (current_action == Action.WALKING_LEFT)
                current_action = Action.ATTACK_LEFT;
            else if (current_action == Action.WALKING_UP)
                current_action = Action.ATTACK_UP;
            else if (current_action == Action.WALKING_DOWN)
                current_action = Action.ATTACK_DOWN;
            else if (current_action == Action.WALKING_RIGHT)
                current_action = Action.ATTACK_RIGHT;
        }

        // returns true if link is attacking
        static bool AButton()
        {
            if (IsPressed(Buttons.A) && Menu.hud_sword.shown && !using_item)
            {
                Attack();

                if (!sword_out)
                {
                    Sound.PlaySFX(Sound.SoundEffects.SWORD);
                }

                sword_out = true;
                return true;
            }

            if (sword_out || wand_out)
            {
                MeleeAttack(wand_out);
                return sword_out;
            }

            return false;
        }

        public static void MeleeAttack(bool magic_wand)
        {
            if (animation_timer == 5)
            {
                if (magic_wand)
                {
                    sword_1.tile_index = 0x8a;
                    sword_2.tile_index = 0x8c;
                    sword_1.palette_index = 5;
                    sword_2.palette_index = 5;
                }
                else
                {
                    sword_1.tile_index = 0x82;
                    sword_2.tile_index = 0x84;
                    sword_1.palette_index = Menu.hud_sword.palette_index;
                    sword_2.palette_index = Menu.hud_sword.palette_index;
                }

                sword_1.yflip = false;
                if (current_action == Action.ATTACK_LEFT)
                {
                    sword_1.xflip = true;
                    sword_2.xflip = true;
                    sword_1.x = x - 3;
                    sword_1.y = y + 1;
                    sword_2.x = x - 11;
                    sword_2.y = y + 1;
                    sprites.Add(sword_2);
                }
                else if (current_action == Action.ATTACK_UP)
                {
                    if (magic_wand)
                        sword_1.tile_index = 0x4a;
                    else
                        sword_1.tile_index = 0x20;
                    sword_1.yflip = false;
                    sword_1.x = x + 3;
                    sword_1.y = y - 12;
                }
                else if (current_action == Action.ATTACK_RIGHT)
                {
                    sword_1.xflip = false;
                    sword_2.xflip = false;
                    sword_1.x = x + 11;
                    sword_1.y = y + 1;
                    sword_2.x = x + 19;
                    sword_2.y = y + 1;
                    sprites.Add(sword_2);
                }
                else
                {
                    if (magic_wand)
                        sword_1.tile_index = 0x4a;
                    else
                        sword_1.tile_index = 0x20;
                    sword_1.yflip = true;
                    sword_1.x = x + 5;
                    sword_1.y = y + 11;
                }
                sprites.Add(sword_1);
            }
            else if (animation_timer == 13)
            {
                if (current_action == Action.WALKING_LEFT)
                {
                    sword_2.x += 4;
                    sprites.Remove(sword_1);
                }
                else if (current_action == Action.WALKING_UP)
                {
                    sword_1.y += 4;
                }
                else if (current_action == Action.WALKING_RIGHT)
                {
                    sword_2.x -= 4;
                    sprites.Remove(sword_1);
                }
                else
                {
                    sword_1.y -= 4;
                }

                if (nb_of_hearts - hp < 0.5f && !Menu.sword_proj_out && !magic_wand && !Menu.magic_wave_out)
                {
                    Menu.sword_proj_out = true;
                    int[] pos = FindItemPos(false);
                    new SwordProjectileSprite(pos[0], pos[1], facing_direction, true);
                }
            }
            else if (animation_timer == 14)
            {
                if (current_action == Action.WALKING_LEFT || current_action == Action.ATTACK_LEFT)
                    sword_2.x += 4;
                else if (current_action == Action.WALKING_UP || current_action == Action.ATTACK_UP)
                    sword_1.y += 4;
                else if (current_action == Action.WALKING_RIGHT || current_action == Action.ATTACK_RIGHT)
                    sword_2.x -= 4;
                else
                    sword_1.y -= 4;

                if (magic_wand && !Menu.magic_wave_out && !Menu.sword_proj_out)
                {
                    Menu.magic_wave_out = true;
                    int[] pos = FindItemPos(true);
                    new MagicBeamSprite(pos[0], pos[1], facing_direction, true);
                }
            }
            else if (animation_timer == 15)
            {
                animation_timer += 6;
                sword_out = false;
                wand_out = false;
                using_item = false;
                sprites.Remove(sword_1);
                sprites.Remove(sword_2);
            }

            if (!magic_wand)
                animation_timer++;
        }

        // returns true if in animation for smthn related to the B button
        static bool BButton()
        {
            if (using_item)
            {
                animation_timer++;
                return true;
            }

            if (!IsPressed(Buttons.B))
            {
                return false;
            }

            int[] pos;

            switch (Menu.current_B_item)
            {
                case 0:
                    return false;

                case SpriteID.BAIT:
                    if (Menu.bait_out)
                    {
                        return false;
                    }

                    Attack();
                    Menu.bait_out = true;
                    pos = FindItemPos(false);
                    new BaitSprite(pos[0], pos[1]);
                    break;

                case SpriteID.RECORDER:
                    if (!Program.mute_sound && self.shown)
                    {
                        Sound.PlaySFX(Sound.SoundEffects.RECORDER);
                        can_move = false;
                    }

                    Menu.can_open_menu = false;

                    if (self.shown)
                    {
                        OC.RecorderDestination(true);
                    }

                    // IF LEVEL 7 ENTRANCE
                    if (OC.current_screen == 66)
                    {
                        if (OC.level_7_entrance_timer == OverworldCode.LEVEL_7_ENTRANCE_ANIM_DONE)
                            OC.ActivateLevel7Animation();
                    }
                    else
                    {
                        if (!Menu.tornado_out && Program.gamemode == Program.Gamemode.OVERWORLD &&
                            OC.current_screen != 128 && Menu.GetTriforcePieceCount() != 0)
                        {
                            new TornadoSprite(0, y);
                            Menu.tornado_out = true;
                        }
                    }
                    break;

                case SpriteID.CANDLE:
                    if (blue_candle)
                    {
                        if (Menu.blue_candle_limit_reached)
                        {
                            return true;
                        }

                        Menu.blue_candle_limit_reached = true;
                    }

                    if (Menu.fire_out >= 2)
                    {
                        return false;
                    }

                    Menu.fire_out++;
                    Attack();
                    pos = FindItemPos(true);
                    new ThrownFireSprite(pos[0], pos[1], facing_direction);
                    break;

                case SpriteID.ARROW:
                    if (Menu.arrow_out || rupy_count <= 0)
                    {
                        return false;
                    }

                    rupy_count--;
                    Menu.arrow_out = true;
                    Attack();
                    pos = FindItemPos(facing_direction == Direction.LEFT || facing_direction == Direction.RIGHT);
                    new ArrowSprite(pos[0], pos[1], facing_direction, true);
                    break;

                case SpriteID.BOMB:
                    if (Menu.bomb_out)
                    {
                        return false;
                    }

                    Attack();
                    pos = FindItemPos(false);
                    new BombSprite(pos[0], pos[1]);
                    bomb_count--;
                    Menu.bomb_out = true;
                    break;

                case SpriteID.BOOMERANG:
                    if (Menu.boomerang_out)
                    {
                        return false;
                    }

                    Attack();
                    pos = FindItemPos(false);
                    new BoomerangSprite(pos[0], pos[1], true);
                    Menu.boomerang_out = true;
                    break;

                case SpriteID.POTION:
                    if (red_potion)
                    {
                        Menu.hud_B_item.palette_index = 5;
                        red_potion = false;
                    }
                    else
                    {
                        Menu.hud_B_item.tile_index = (byte)SpriteID.MAP; // letter
                        blue_potion = false;
                    }

                    full_heal_flag = true;
                    break;

                case SpriteID.ROD:
                    if (sword_out || wand_out)
                    {
                        return false;
                    }

                    wand_out = true;
                    Attack();
                    break;
            }

            if (Menu.current_B_item != 0 && Menu.current_B_item != SpriteID.RECORDER)
                using_item = true;

            return true;
        }

        // find location relative to link for where item spawns
        static int[] FindItemPos(bool double_wide)
        {
            int[] pos = new int[2];

            int dbl_spr_mod_1 = 0;
            int dbl_spr_mod_2 = 0;
            int dbl_spr_mod_3 = 0;

            if (double_wide)
            {
                dbl_spr_mod_1 = 1;
                dbl_spr_mod_2 = 4;
                dbl_spr_mod_3 = 5;
            }

            if (facing_direction == Direction.UP)
                pos[1] = y - 18 + dbl_spr_mod_1;
            else if (facing_direction == Direction.DOWN)
                pos[1] = y + 14;
            else
                pos[1] = y - 2;

            if (facing_direction == Direction.LEFT)
                pos[0] = x - 12 - dbl_spr_mod_3;
            else if (facing_direction == Direction.RIGHT)
                pos[0] = x + 20 - dbl_spr_mod_2;
            else
                pos[0] = x + 4 - dbl_spr_mod_2;

            return new int[2] {
                pos[0], pos[1]
            };
        }

        static void ChangePos(bool is_x, bool is_positive)
        {
            int change;
            if (stair_speed)
            {
                change = 1;
                if ((Program.gTimer % 4) == 0)
                    change = 0;
            }
            else
            {
                change = (Program.gTimer % 2) + 1;
            }

            if (!is_positive)
            {
                change *= -1;
            }

            if (is_x)
                self.x += change;
            else
                self.y += change;
        }

        // snap Link to the nearest multiple of 8
        static void GotoMod8()
        {
            // why the fuck does this work
            if (facing_direction == Direction.LEFT || facing_direction == Direction.RIGHT)
            {
                int diff_from_mod8 = self.x % 8;
                if (diff_from_mod8 >= 4)
                {
                    self.x += 8 - diff_from_mod8;
                }
                else
                {
                    self.x -= diff_from_mod8;
                }
            }
            else
            {
                int diff_from_mod8 = self.y % 8;
                if (diff_from_mod8 >= 4)
                {
                    self.y += 8 - diff_from_mod8;
                }
                else
                {
                    self.y -= diff_from_mod8;
                }
            }
        }

        static void FillHealth()
        {
            Menu.can_open_menu = false;
            can_move = false;
            if (!OC.fairy_animation_active)
            {
                if (Sound.IsMusicPlaying())
                    Sound.PauseMusic();
            }

            if (hp < nb_of_hearts)
            {
                if (Program.gTimer % 22 == 0)
                    hp += 0.5f;

                Sound.PlaySFX(Sound.SoundEffects.HEART, true);
            }

            if (hp == nb_of_hearts && !OC.fairy_animation_active)
            {
                Menu.can_open_menu = true;
                full_heal_flag = false;
                can_move = true;
                Sound.PauseMusic(true);
            }
        }

        // sets Link'S position to a specific value.
        // !! bypasses wall collision checks by overriting safe_x and safe_y !!
        public static void SetPos(int new_x = -1, int new_y = -1)
        {
            if (new_x != -1)
            {
                self.x = new_x;
                safe_x = new_x;
                x = new_x;
            }
            if (new_y != -1)
            {
                self.y = new_y;
                safe_y = new_y;
                y = new_y;
            }

            counterpart.x = self.x + 8;
            counterpart.y = self.y;
        }

        // method to show or hide link
        public static void Show(bool show)
        {
            shown = show;
            self.shown = show;
            counterpart.shown = show;
        }

        // method to set Link's background state
        public static void SetBGState(bool bg_mod_activated)
        {
            self.background = bg_mod_activated;
            counterpart.background = bg_mod_activated;
        }

        static void Animation()
        {
            if (Program.gamemode == Program.Gamemode.DEATH && DeathCode.death_timer > 120)
                return;

            switch (current_action)
            {
                case Action.WALKING_DOWN:
                    if (animation_timer % 12 < 6)
                    {
                        if (magical_shield)
                        {
                            self.tile_index = 0x60;
                        }
                        else
                        {
                            self.tile_index = 0x58;
                        }
                        counterpart.tile_index = 0xa;
                        self.xflip = false;
                        counterpart.xflip = false;
                    }
                    else
                    {
                        if (magical_shield)
                        {
                            self.tile_index = 0x60;
                        }
                        else
                        {
                            self.tile_index = 0x5a;
                        }
                        counterpart.tile_index = 0x8;
                        self.xflip = false;
                        counterpart.xflip = true;
                    }
                    break;
                case Action.WALKING_UP:
                    if (animation_timer % 12 < 6)
                    {
                        self.tile_index = 0xc;
                        counterpart.tile_index = 0xe;
                        self.xflip = false;
                        counterpart.xflip = false;
                    }
                    else
                    {
                        self.tile_index = 0xe;
                        counterpart.tile_index = 0xc;
                        self.xflip = true;
                        counterpart.xflip = true;
                    }
                    break;
                case Action.WALKING_LEFT:
                    if (animation_timer % 12 < 6)
                    {
                        if (magical_shield)
                        {
                            self.tile_index = 0x80;
                        }
                        else
                        {
                            self.tile_index = 0x2;
                        }
                        counterpart.tile_index = 0x0;
                        self.xflip = true;
                        counterpart.xflip = true;
                    }
                    else
                    {
                        if (magical_shield)
                        {
                            self.tile_index = 0x54;
                        }
                        else
                        {
                            self.tile_index = 0x6;
                        }
                        counterpart.tile_index = 0x4;
                        self.xflip = true;
                        counterpart.xflip = true;
                    }
                    break;
                case Action.WALKING_RIGHT:
                    if (animation_timer % 12 < 6)
                    {
                        if (magical_shield)
                        {
                            counterpart.tile_index = 0x80;
                        }
                        else
                        {
                            counterpart.tile_index = 0x2;
                        }
                        self.tile_index = 0x0;
                        self.xflip = false;
                        counterpart.xflip = false;
                    }
                    else
                    {
                        if (magical_shield)
                        {
                            counterpart.tile_index = 0x54;
                        }
                        else
                        {
                            counterpart.tile_index = 0x6;
                        }
                        self.tile_index = 0x4;
                        self.xflip = false;
                        counterpart.xflip = false;
                    }
                    break;
                case Action.ITEM_GET:
                    counterpart.tile_index = 0x08;
                    goto item_anim;
                case Action.ITEM_HELD_UP:
                    counterpart.tile_index = 0x78;
                    item_anim:
                    self.tile_index = 0x78;
                    self.xflip = false;
                    counterpart.xflip = true;
                    can_move = false;
                    animation_timer++;
                    if (animation_timer > 128)
                        animation_timer = 0;
                    if (animation_timer == 128)
                    {
                        facing_direction = Direction.DOWN;
                        current_action = Action.WALKING_DOWN;
                        can_move = true;
                        goto case Action.WALKING_DOWN;
                    }
                    break;
                case Action.ATTACK_UP:
                    if (animation_timer > 20)
                        animation_timer = 0;
                    if (animation_timer == 0)
                    {
                        longer_attack_anim = AttackAnim();
                        self.xflip = false;
                        counterpart.xflip = false;
                        self.tile_index = 0x18;
                        counterpart.tile_index = 0x1a;
                    }
                    else if (animation_timer == 6 + longer_attack_anim)
                    {
                        counterpart.tile_index = 0xe;
                        self.tile_index = 0xc;
                    }
                    else if (animation_timer == 12 + longer_attack_anim / 6)
                    {
                        current_action = Action.WALKING_UP;
                        if (!wand_out && !sword_out)
                            using_item = false;
                        goto case Action.WALKING_UP;
                    }
                    break;
                case Action.ATTACK_DOWN:
                    if (animation_timer > 20)
                        animation_timer = 0;
                    if (animation_timer == 0)
                    {
                        longer_attack_anim = AttackAnim();
                        counterpart.xflip = false;
                        self.tile_index = 0x14;
                        counterpart.tile_index = 0x16;
                    }
                    else if (animation_timer == 6 + longer_attack_anim)
                    {
                        counterpart.tile_index = 0xa;
                        if (magical_shield)
                            self.tile_index = 0x60;
                        else
                            self.tile_index = 0x58;
                    }
                    else if (animation_timer == 12 + longer_attack_anim / 6)
                    {
                        current_action = Action.WALKING_DOWN;
                        if (!wand_out && !sword_out)
                            using_item = false;
                        goto case Action.WALKING_DOWN;
                    }
                    break;
                case Action.ATTACK_LEFT:
                    if (animation_timer > 20)
                        animation_timer = 0;
                    if (animation_timer == 0)
                    {
                        longer_attack_anim = AttackAnim();
                        self.tile_index = 0x12;
                        counterpart.tile_index = 0x10;
                        self.xflip = true;
                        counterpart.xflip = true;
                    }
                    else if (animation_timer == 6 + longer_attack_anim)
                    {
                        counterpart.tile_index = 0x0;
                        if (magical_shield)
                            self.tile_index = 0x54;
                        else
                            self.tile_index = 0x2;
                    }
                    else if (animation_timer == 12 + longer_attack_anim / 6)
                    {
                        current_action = Action.WALKING_LEFT;
                        if (!wand_out && !sword_out)
                            using_item = false;
                        goto case Action.WALKING_LEFT;
                    }
                    break;
                case Action.ATTACK_RIGHT:
                    if (animation_timer > 20)
                        animation_timer = 0;
                    if (animation_timer == 0)
                    {
                        longer_attack_anim = AttackAnim();
                        self.tile_index = 0x10;
                        counterpart.tile_index = 0x12;
                    }
                    else if (animation_timer == 6 + longer_attack_anim)
                    {
                        self.tile_index = 0x0;
                        if (magical_shield)
                            counterpart.tile_index = 0x54;
                        else
                            counterpart.tile_index = 0x2;
                    }
                    else if(animation_timer == 12 + longer_attack_anim / 6)
                    {
                        current_action = Action.WALKING_RIGHT;
                        if (!wand_out && !sword_out)
                            using_item = false;
                        goto case Action.WALKING_RIGHT;
                    }
                    break;
                default:
                    self.tile_index = 0x8;
                    counterpart.tile_index = 0xa;
                    self.xflip = false;
                    counterpart.xflip = false;
                    break;
            }

            // equivalent to "(sword_out || wand_out) ? 6 : 0"
            //TODO: worth removing? local functions suck
            byte AttackAnim()
            {
                byte make_anim_longer = 0;

                if (sword_out || wand_out)
                    make_anim_longer = 6;

                return make_anim_longer;
            }
        }

        // the top half of link doesn't have collision. thus collision is only checked for the bottom 16x8 pixel box of link's sprites
        // returns true if link collided
        static bool Collision()
        {
            bool collision = false;

            if (knockback_timer <= 0)
            {
                // performs less collision checks, but doesn't work when moving backwards from enemy knockback
                switch (facing_direction)
                {
                    case Direction.UP:
                        if (CheckCollision(0, 8) || CheckCollision(15, 8))
                            collision = true;
                        break;
                    case Direction.DOWN:
                        if (CheckCollision(0, 15) || CheckCollision(15, 15))
                            collision = true;
                        break;
                    case Direction.LEFT:
                        if (CheckCollision(0, 8) || CheckCollision(0, 15))
                            collision = true;
                        break;
                    case Direction.RIGHT:
                        if (CheckCollision(15, 8) || CheckCollision(15, 15))
                            collision = true;
                        break;
                }
            }
            else
            {
                if (CheckCollision(0, 8) || CheckCollision(15, 8) || CheckCollision(0, 15) || CheckCollision(15, 15))
                    collision = true;
            }

            // link always spawns lined up on the metatile grid when warping. being off the grid means he moved after a warp.
            // this is useful because you can't enter a warp until you've moved after a warp transition
            if (!has_moved_after_warp_flag && 
                ((x % 16) != 0 || (y % 16) != 0))
            {
                has_moved_after_warp_flag = true;
            }

            // safe_x safe_y system. classic!
            if (collision)
            {
                self.x = safe_x;
                self.y = safe_y;
            }
            else
            {
                safe_x = self.x;
                safe_y = self.y;
            }

            return collision;
        }

        // checks if the tile at position Link.x + x_add, Link.y + y_add is solid. returns true if collision found
        static bool CheckCollision(int x_add, int y_add) // create duplicate for dungeon
        {
            int metatile_index = ((self.y + y_add) & (~0xF)) + ((self.x + x_add) / 16) - 64;

            if (metatile_index < 0 || metatile_index > meta_tiles.Length)
            {
                return false;
            }

            if (Program.gamemode == Program.Gamemode.OVERWORLD)
            {
                // 2 exceptions to collision logic.

                // room 31 where you can walk through the wall to room 15
                if (OC.current_screen == 31 && (metatile_index == 8 || metatile_index == 24))
                {
                    stair_speed = false;
                    return false;
                }
                // you can get stuck in the wall if you enter this room the wrong way, so we overwrite the tiles here to be empty
                // (room below dungeon 2 entrance)
                else if (OC.current_screen == 76 && (metatile_index == 79 || metatile_index == 111))
                {
                    return false;
                }

                switch (meta_tiles[metatile_index].tile_index)
                {
                    case 0x00 or 0x02 or 0x09 or 0x17 or 0x1a or 0x1b or 0x1c or 0x1d or 0x1e or 0x1f or 0x20 or 0x21 or 0x22 or 0x23 or 0x24:
                        return true;

                    case 0x08:
                        if (meta_tiles[metatile_index].special && power_bracelet)
                        {
                            new MovingTileSprite(OC.current_screen == 73 ? MovingTileSprite.MovingTile.GREEN_ROCK : MovingTileSprite.MovingTile.ROCK, metatile_index);
                        }
                        return true;

                    case 0x19:
                        if (meta_tiles[metatile_index].special)
                        {
                            if (facing_direction is (Direction.UP or Direction.DOWN) &&
                                self.x % 16 == 0)
                            {
                                new MovingTileSprite(MovingTileSprite.MovingTile.TOMBSTONE, metatile_index);
                            }

                            return true;
                        }

                        byte ghini_count = 0;
                        int mtl_x = metatile_index % 16 * 16;
                        int mtl_y = (metatile_index >> 4) * 16 + 64;
                        //TODO: ???
                        if (mtl_x == 144 && mtl_y == 144 && OC.current_screen == 33)
                            return true;

                        foreach (Sprite spr in sprites)
                        {
                            if (spr is Ghini)
                            {
                                ghini_count++;
                                // do not spawn ghini if ghini would be at exact location of existing ghini (likely because it's in its spawning animation)
                                // or if ghini count is at 11 (maximum)
                                if (spr.x == mtl_x && spr.y == mtl_y || ghini_count >= 11)
                                    return true;
                            }
                        }

                        new Ghini(false, mtl_x, mtl_y);
                        return true;

                    case 0x25:
                        int mtl_x2 = (metatile_index % 16) * 16;
                        int mtl_y2 = (metatile_index / 16) * 16 + 64;

                        foreach (Sprite spr in sprites)
                        {
                            if (spr is Armos)
                            {
                                // do not spawn armos if armos already exists at that location (prevents turning 1 statue into several)
                                if (spr.x == mtl_x2 && spr.y == mtl_y2)
                                    return true;
                            }
                        }
                        new Armos(metatile_index, mtl_x2, mtl_y2);
                        return true;

                    case 0x04: // BL
                        return ((self.x + x_add & 15) < 8 || (self.y + y_add & 15) > 8);
                    case 0x05: // BR
                        return ((self.x + x_add & 15) > 8 || (self.y + y_add & 15) > 8);
                    case 0x06: // TL
                        return ((self.x + x_add & 15) < 8 || (self.y + y_add & 15) < 8);
                    case 0x07: // TR
                        return ((self.x + x_add & 15) > 8 || (self.y + y_add & 15) < 8);

                    case 0x03:
                        if ((self.x % 16) == 0 && (self.y % 16) == 0 && has_moved_after_warp_flag)
                            OC.black_square_stairs_flag = true;
                        return ((self.y + y_add & 15) < 8);

                    case 0x18:
                        if ((self.x % 16) == 0 && (self.y % 16) == 0)
                        {
                            OC.stair_warp_flag = true;
                        }
                        return ((self.y + y_add & 15) < 8);

                    case 0x15:
                        if (((self.x + 1) % 16) < 3 && ((self.y + 1) % 16) < 3)
                        {
                            int mt_i = GetTileIndexAtLocation(x + 8, y + 8);
                            //TODO: ??
                            if (mt_i != 0x15)
                                return false;

                            OC.stair_warp_flag = true;
                        }
                        return false;

                    case 0x13:
                        stair_speed = true;
                        return false;

                    case 0x14:
                        // TODO: why check for which screen??
                        if (raft && (OC.current_screen == 63 || OC.current_screen == 85))
                        {
                            if (((self.x + 1) % 16) < 3 && ((self.y + 1) % 16) < 3)
                            {
                                if (self.y < 80)
                                    facing_direction = Direction.DOWN;
                                else
                                    facing_direction = Direction.UP;

                                if (!OC.raft_flag)
                                    new RaftSprite();

                                OC.raft_flag = true;
                                can_move = false;
                            }
                        }
                        return false;

                    // special tiles that ladder turns water into (half block)
                    case 0x2e: // U
                        return (((self.y + y_add) % 16) < 8);
                    case 0x2f: // D
                        return (((self.y + y_add) % 16) > 8);
                    case 0x30: // L
                        return (((self.x + x_add) % 16) < 8);
                    case 0x31: // R
                        return (((self.x + x_add) % 16) > 8);
                    case 0x32: // TL
                        return (((self.x + x_add) % 16) < 8 || ((self.y + y_add) % 16) < 8);
                    case 0x33: // TR
                        return (((self.x + x_add) % 16) > 8 || ((self.y + y_add) % 16) < 8);
                    case 0x34: // none
                        return false;

                    case 0x0a:
                        if (metatile_index > 16 && metatile_index < 32)
                        {
                            if (meta_tiles[metatile_index - 16].special)
                            {
                                SetPos(new_y: y - 1);
                                goto case 0x14;
                            }
                        }
                        goto case 0x0b;

                    case 0x0b:
                    case 0x0c or 0x0d or 0x0e or 0x0f or 0x10 or 0x11 or 0x12:
                        // only activate ladder if you have it, it's not being used, scrolling is done and you,re not near the edge of the screen
                        if (ladder && !ladder_used && OC.ScrollingDone() &&
                            y >= 66 && y <= 222 && x >= 2 && x <= 238)
                        {
                            new LadderSprite(metatile_index);
                            return false;
                        }
                        return true;

                    default:
                        stair_speed = false;
                        return false;
                }
            }
            else if (Program.gamemode == Program.Gamemode.DUNGEON)
            {
                switch (meta_tiles[metatile_index].tile_index)
                {
                    // ???
                    //TODO: enum + wtf is this first one
                    case 0 or 5 or 7 or 9:
                        if (!IsHeld(Buttons.UP) && !IsHeld(Buttons.DOWN) && !IsHeld(Buttons.LEFT) && !IsHeld(Buttons.RIGHT))
                            dungeon_wall_push_timer = 0;
                        return false;
                    case 1 or 3 or 4 or 8:
                        KeyCode();
                        return true;
                    case 2:
                        if (ladder && !ladder_used)
                        {
                            new LadderSprite(metatile_index);
                            return false;
                        }
                        return true;
                    case 6:
                        DC.warp_flag = true;
                        return false;

                    case 10: // top of dungeon room
                        return ((self.y + y_add & 15) < 8 && self.x != 120);
                    case 11: // left of up/down doors
                        return ((self.x + x_add & 15) < 8);
                    case 12: // right of up/down doors
                        return ((self.x + x_add & 15) > 8);

                    case 13: // walk-through tile
                        // TODO: 8 frame timer before animation
                        return true;

                    // LADDER SHENANIGANS
                    // special tiles that ladder turns water into (half block)
                    case 0x2e: // U
                        return (((self.y + y_add) % 16) < 8);
                    case 0x2f: // D
                        return (((self.y + y_add) % 16) > 8);
                    case 0x30: // L
                        return (((self.x + x_add) % 16) < 8);
                    case 0x31: // R
                        return (((self.x + x_add) % 16) > 8);
                    case 0x32: // TL
                        return (((self.x + x_add) % 16) < 8 || ((self.y + y_add) % 16) < 8);
                    case 0x33: // TR
                        return (((self.x + x_add) % 16) > 8 || ((self.y + y_add) % 16) < 8);
                    case 0x34: // none
                        return false;
                }
            }

            return false;

            //TODO: move this up
            void KeyCode()
            {
                if (!meta_tiles[metatile_index].special)
                {
                    return;
                }

                dungeon_wall_push_timer++;
                if (dungeon_wall_push_timer < 8)
                {
                    return;
                }

                if (!magical_key)
                {
                    if (key_count <= 0)
                    {
                        return;
                    }

                    key_count--;
                }

                // jesus christ...
                // SET OPENED KEY DOOR FLAG TO TRUE.
                // WHICH KEY DOOR? WELL ITS ID IS GOING TO BE ITS INDEX IN THIS LIST.
                // WHICH VALUE IN THE LIST HOWEVER? WELL ITS VALUE IS ITS CONNECTION_ID.
                // WHAT IS ITS CONNECTION_ID? IT'S CALCULATED WITH THE CURRENT SCREEN AND THE DIRECTION OF THE DOOR.
                // WHAT DIRECTION IS THIS SPECIFIC DOOR POINTING IN? THE INDEX OF THE METATILE IN THIS LIST, CASTED TO A DIRECTION.
                //TODO: find a way to cut this bullshit
                SetOpenedKeyDoorsFlag(
                    (byte)Array.IndexOf(
                        DC.key_door_connections,
                        (short)DC.getConnectionID(
                            DC.current_screen,
                            (Direction)Array.IndexOf(
                                DC.door_metatiles,
                                (byte)metatile_index
                            )
                        )
                    ),
                true
                );

                DC.door_types[Array.IndexOf(DC.door_metatiles, (byte)metatile_index)] = DungeonCode.DoorType.OPEN;
                DC.DrawDoors(DC.current_screen, 0, true);
                // TODO: play door opening sfx
            }
        }

        public static void TakeDamage(float damage)
        {
            if (!can_move || iframes_timer > 0)
                return;

            if (red_ring)
                damage /= 4;
            else if (blue_ring)
                damage /= 2;

            hp -= damage;
            iframes_timer = 48;

            // if aligned on grid
            if (((int)knockback_direction < 2 && x % 8 == 0) ||
                ((int)knockback_direction >= 2 && y % 8 == 0))
            {
                knockback_timer = 8;
            }

            Sound.PlaySFX(Sound.SoundEffects.HURT);
            nb_of_ens_killed_damageless = 0;

            if (hp > 0)
            {
                return;
            }

            // death init
            hp = 0;
            death_count++;
            Sound.PauseMusic();
            // update menu to 0 hearts right now because it won't be updated in death animation
            Menu.DrawHUD();
            sprites.Remove(sword_1);
            sprites.Remove(sword_2);
            DeathCode.death_timer = 0;
            DeathCode.died_in_dungeon = (Program.gamemode == Program.Gamemode.DUNGEON);
            Program.gamemode = Program.Gamemode.DEATH;
        }

        // checks to se eif link should flash, does it if so
        static void HitFlash()
        {
            if (iframes_timer <= 0 && !clock_flash)
                return;

            byte new_palette;
            if (iframes_timer == 1)
                new_palette = 4;
            else
                new_palette = (byte)((Program.gTimer / 2) % 4 + 4);

            self.palette_index = new_palette;
            counterpart.palette_index = new_palette;
            iframes_timer--;
        }

        static bool Knockback()
        {
            if (knockback_timer <= 0)
                return false;

            // setpos overwrites safex and safey, so we have to manually remember link's last non-wall pos
            int backup_x = x, backup_y = y;
            knockback_timer--;
            int new_x = 0, new_y = 0;

            if (knockback_direction == Direction.UP)
                new_y = -4;
            else if (knockback_direction == Direction.DOWN)
                new_y = 4;
            else if (knockback_direction == Direction.LEFT)
                new_x = -4;
            else
                new_x = 4;

            sword_1.x += new_x;
            sword_2.x += new_x;
            sword_1.y += new_y;
            sword_2.y += new_y;

            new_x += x;
            new_y += y;
            SetPos(new_x, new_y);

            if (Collision() || y < 64 || y > 224 || x < 0 || x > 240)
            {
                knockback_timer = 0;
                SetPos(backup_x, backup_y);
            }

            return true;
        }

        // returns false if underflow or overflow, true if not
        // "perform_addition" performs the change. no matter what the rupy count is.set it to false to just check if link has enough
        public static bool AddRupees(int change, bool perform_addition = true)
        {
            int new_val = rupy_count + change;

            int clamped_val = Math.Clamp(new_val, byte.MinValue, byte.MaxValue);

            if (perform_addition)
            {
                rupy_count = (byte)clamped_val;
            }

            return new_val == clamped_val;
        }
    }
}