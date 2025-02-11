using static The_Legend_of_Zelda.Rendering.Screen;
using static The_Legend_of_Zelda.Control;
using static The_Legend_of_Zelda.SaveLoad;
using static The_Legend_of_Zelda.Gameplay.Program;
using System.Reflection;
using The_Legend_of_Zelda.Gameplay;
using The_Legend_of_Zelda.Rendering;

namespace The_Legend_of_Zelda.Sprites
{
    public enum Direction
    {
        UP,
        DOWN,
        LEFT,
        RIGHT
    }

    public static class DirectionFunctions
    {
        public static Direction Opposite(this Direction direction)
        {
            return (Direction)((int)direction ^ 1);
        }
    }

    public enum LinkAction
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

    public class LinkSprite : IBoomerangThrower
    {
        public int x, y;
        public int animation_timer = 0;
        int safe_x, safe_y;

        public float hp = 3;

        public int dungeon_wall_push_timer { get; private set; } = 0;
        public byte nb_of_ens_killed = 0;
        public byte nb_of_ens_killed_damageless = 0;
        public byte iframes_timer;
        public byte knockback_timer;
        byte longer_attack_anim = 0;

        public bool sword_out { get; private set; } = false;
        public bool wand_out { get; private set; } = false;
        public bool shown { get; private set; }
        public bool ladder_used = false;
        public bool can_move = false;
        public bool has_moved_after_warp_flag = true;
        public bool using_item = false;
        public bool full_heal_flag = false;
        public bool clock_flash = false;
        public bool can_use_sword = true;
        bool stair_speed = false;
        public bool boomerang_out { get => Menu.boomerang_out; set => Menu.boomerang_out = value; }
        public EightDirection boomerang_throw_dir { get => BoomerangSprite.FindLinkBoomerangDirection(); set => _ = value; }
        public (int x, int y) return_pos { get => (x, y); set => _ = value; }

        public Direction facing_direction;
        public Direction knockback_direction;
        public LinkAction current_action;

        public StaticSprite self = new StaticSprite(0x00, 4, 0, 0);
        public StaticSprite counterpart = new StaticSprite(0x00, 4, 8, 0);
        public StaticSprite sword_1 { get; private set; } = new StaticSprite(0x20, 4, 0, 0);
        public StaticSprite sword_2 { get; private set; } = new StaticSprite(0x20, 4, 0, 0);

        public void Init()
        {
            shown = false;

            if (!sprites.Contains(self))
                sprites.Add(self);
            if (!sprites.Contains(counterpart))
                sprites.Add(counterpart);

            current_action = LinkAction.WALKING_UP;
            facing_direction = Direction.UP;
            SetPos(120, 144);

            hp = 3;

            iframes_timer = 0;
            knockback_timer = 0;
            self.palette_index = 4;
            counterpart.palette_index = 4;
            animation_timer = 100;
            sword_out = false;
            wand_out = false;
            Tick();
        }

        public void Tick()
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
            hp = 3f;
            blue_candle = true;
            magical_key = true;
            recorder = true;
            arrow = true;
            bow = true;
            rupy_count = 255;
            // c# doesn't have c++'s friend keyword, so fuck you c#, et reflection and die :)
            // (this is only used for testing ofc)
            //FieldInfo? lol = typeof(SaveLoad).GetField("map_flags", BindingFlags.NonPublic | BindingFlags.Static);
            //lol?.SetValue(null, new ushort[] { 0xffff, 0, 0 });
            //lol = typeof(SaveLoad).GetField("compass_flags", BindingFlags.NonPublic | BindingFlags.Static);
            //lol?.SetValue(null, new ushort[] { 0xffff, 0, 0 });
            bomb_count = 8;
            ladder = true;
            raft = true;
            boomerang = true;
            power_bracelet = true;
            wooden_sword = true;
        }

        void CheckIfRecorderPlaying()
        {
            if (Sound.recorder_playing && !Sound.RecorderPlaying())
            {
                can_move = true;
                Menu.can_open_menu = true;
            }
        }

        void Move()
        {
            if (IsHeld(Buttons.UP) ^ IsHeld(Buttons.DOWN))
            {
                animation_timer++;
                if (IsHeld(Buttons.UP))
                {
                    current_action = LinkAction.WALKING_UP;
                    ChangePos(false, false);
                    if (facing_direction == Direction.LEFT || facing_direction == Direction.RIGHT)
                        GotoMod8();
                    facing_direction = Direction.UP;
                }
                else
                {
                    current_action = LinkAction.WALKING_DOWN;
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
                    current_action = LinkAction.WALKING_LEFT;
                    ChangePos(true, false);
                    if (facing_direction == Direction.UP || facing_direction == Direction.DOWN)
                        GotoMod8();
                    facing_direction = Direction.LEFT;
                }
                else
                {
                    current_action = LinkAction.WALKING_RIGHT;
                    ChangePos(true, true);
                    if (facing_direction == Direction.UP || facing_direction == Direction.DOWN)
                        GotoMod8();
                    facing_direction = Direction.RIGHT;
                }
            }
        }

        void Attack()
        {
            if (current_action == LinkAction.WALKING_LEFT)
                current_action = LinkAction.ATTACK_LEFT;
            else if (current_action == LinkAction.WALKING_UP)
                current_action = LinkAction.ATTACK_UP;
            else if (current_action == LinkAction.WALKING_DOWN)
                current_action = LinkAction.ATTACK_DOWN;
            else if (current_action == LinkAction.WALKING_RIGHT)
                current_action = LinkAction.ATTACK_RIGHT;
        }

        // returns true if link is attacking
        bool AButton()
        {
            if (IsPressed(Buttons.A) && Menu.hud_sword.shown && !using_item && can_use_sword)
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

        public void MeleeAttack(bool magic_wand)
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
                if (current_action == LinkAction.ATTACK_LEFT)
                {
                    sword_1.xflip = true;
                    sword_2.xflip = true;
                    sword_1.x = x - 3;
                    sword_1.y = y + 1;
                    sword_2.x = x - 11;
                    sword_2.y = y + 1;
                    sprites.Add(sword_2);
                }
                else if (current_action == LinkAction.ATTACK_UP)
                {
                    if (magic_wand)
                        sword_1.tile_index = 0x4a;
                    else
                        sword_1.tile_index = 0x20;
                    sword_1.yflip = false;
                    sword_1.x = x + 3;
                    sword_1.y = y - 12;
                }
                else if (current_action == LinkAction.ATTACK_RIGHT)
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
                sword_1.UpdateTexture(true);
            }
            else if (animation_timer == 13)
            {
                if (current_action == LinkAction.WALKING_LEFT)
                {
                    sword_2.x += 4;
                    sprites.Remove(sword_1);
                }
                else if (current_action == LinkAction.WALKING_UP)
                {
                    sword_1.y += 4;
                }
                else if (current_action == LinkAction.WALKING_RIGHT)
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
                if (current_action == LinkAction.WALKING_LEFT || current_action == LinkAction.ATTACK_LEFT)
                    sword_2.x += 4;
                else if (current_action == LinkAction.WALKING_UP || current_action == LinkAction.ATTACK_UP)
                    sword_1.y += 4;
                else if (current_action == LinkAction.WALKING_RIGHT || current_action == LinkAction.ATTACK_RIGHT)
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
        bool BButton()
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
                    if (!mute_sound && self.shown)
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
                        if (!Menu.tornado_out && gamemode == Gamemode.OVERWORLD &&
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
                    new BoomerangSprite(pos[0], pos[1], true, SaveLoad.magical_boomerang, this);
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
        int[] FindItemPos(bool double_wide)
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

        void ChangePos(bool is_x, bool is_positive)
        {
            int change;
            if (stair_speed)
            {
                change = 1;
                if (gTimer % 4 == 0)
                    change = 0;
            }
            else
            {
                change = gTimer % 2 + 1;
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

        // snap Link to the nearest multiple of 8 when he turns 90deg
        void GotoMod8()
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

        void FillHealth()
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
                if (gTimer % 22 == 0)
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

        // sets Link's position to a specific value.
        // !! bypasses wall collision checks by overriting safe_x and safe_y !!
        public void SetPos(int new_x = -1, int new_y = -1)
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
        public void Show(bool show)
        {
            shown = show;
            self.shown = show;
            counterpart.shown = show;
        }

        // method to set Link's background state
        public void SetBGState(bool bg_mod_activated)
        {
            self.background = bg_mod_activated;
            counterpart.background = bg_mod_activated;
        }

        void Animation()
        {
            if (gamemode == Gamemode.DEATH && DeathCode.death_timer > 120)
                return;

            switch (current_action)
            {
                case LinkAction.WALKING_DOWN:
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
                case LinkAction.WALKING_UP:
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
                case LinkAction.WALKING_LEFT:
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
                case LinkAction.WALKING_RIGHT:
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
                case LinkAction.ITEM_GET:
                    counterpart.tile_index = 0x08;
                    goto item_anim;
                case LinkAction.ITEM_HELD_UP:
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
                        current_action = LinkAction.WALKING_DOWN;
                        can_move = true;
                        goto case LinkAction.WALKING_DOWN;
                    }
                    break;
                case LinkAction.ATTACK_UP:
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
                        current_action = LinkAction.WALKING_UP;
                        if (!wand_out && !sword_out)
                            using_item = false;
                        goto case LinkAction.WALKING_UP;
                    }
                    break;
                case LinkAction.ATTACK_DOWN:
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
                        current_action = LinkAction.WALKING_DOWN;
                        if (!wand_out && !sword_out)
                            using_item = false;
                        goto case LinkAction.WALKING_DOWN;
                    }
                    break;
                case LinkAction.ATTACK_LEFT:
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
                        current_action = LinkAction.WALKING_LEFT;
                        if (!wand_out && !sword_out)
                            using_item = false;
                        goto case LinkAction.WALKING_LEFT;
                    }
                    break;
                case LinkAction.ATTACK_RIGHT:
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
                    else if (animation_timer == 12 + longer_attack_anim / 6)
                    {
                        current_action = LinkAction.WALKING_RIGHT;
                        if (!wand_out && !sword_out)
                            using_item = false;
                        goto case LinkAction.WALKING_RIGHT;
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
        bool Collision()
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
                (x % 8 != 0 || y % 16 != 0))
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
        // this also executes code for special tiles that do something when touched
        bool CheckCollision(int x_add, int y_add)
        {
            int metatile_index = GetMetaTileIndexAtLocation(self.x + x_add, self.y + y_add);

            if (metatile_index < 0)
            {
                return false;
            }

            if (gamemode == Gamemode.OVERWORLD)
            {
                // 2 exceptions to collision logic.

                // room 31 where you can walk through the wall to room 15
                if (OC.current_screen == 31 && (metatile_index == 8 || metatile_index == 24))
                {
                    // disable stair speed in case we're returning from screen above
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
                    case MetatileType.ROCK or MetatileType.ROCK_TOP or MetatileType.TREE or MetatileType.WATERFALL or MetatileType.STUMP_TL or 
                    MetatileType.STUMP_BL or MetatileType.STUMP_BR or MetatileType.STUMP_TR or MetatileType.STUMP_FACE or MetatileType.RUINS_TL or 
                    MetatileType.RUINS_BL or MetatileType.RUINS_TR or MetatileType.RUINS_BR or MetatileType.RUINS_FACE_1_EYE or MetatileType.RUINS_FACE_2_EYES:
                        return true;

                    case MetatileType.ROCK_SNAIL:
                        if (meta_tiles[metatile_index].special && power_bracelet)
                        {
                            new MovingTileSprite(OC.current_screen == 73 ? MovingTileSprite.MovingTile.GREEN_ROCK : MovingTileSprite.MovingTile.ROCK, metatile_index);
                        }
                        return true;

                    case MetatileType.TOMBSTONE:
                        // even though the original tile loses its special status upon being touched,
                        // it changes its metatile to a rock to prevent ghinis from being spawned from it during its moving animation
                        if (meta_tiles[metatile_index].special)
                        {
                            // intentional, you can only push the grave up or down in the original game. it's dumb
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

                    case MetatileType.STATUE:
                        int mtl_x2 = metatile_index % 16 * 16;
                        int mtl_y2 = metatile_index / 16 * 16 + 64;

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

                    // x_add and y_add can be negative, meaning the stupid fucking modulo operator won't fucking work
                    case MetatileType.ROCK_TR:
                        return (self.x + x_add & 15) < 8 || (self.y + y_add & 15) > 8;
                    case MetatileType.ROCK_TL:
                        return (self.x + x_add & 15) > 8 || (self.y + y_add & 15) > 8;
                    case MetatileType.ROCK_BR:
                        return (self.x + x_add & 15) < 8 || (self.y + y_add & 15) < 8;
                    case MetatileType.ROCK_BL:
                        return (self.x + x_add & 15) > 8 || (self.y + y_add & 15) < 8;

                    case MetatileType.BLACK_SQUARE_WARP:
                        if (self.x % 8 == 0 && self.y % 16 == 0 && has_moved_after_warp_flag)
                            OC.black_square_stairs_flag = true;
                        return (self.y + y_add & 15) < 8;

                    case MetatileType.WATERFALL_BOTTOM:
                        if (self.x % 16 == 0 && self.y % 16 == 0)
                        {
                            OC.stair_warp_flag = true;
                        }
                        return (self.y + y_add & 15) < 8;

                    case MetatileType.STAIRS:
                        if ((self.x + 1) % 16 < 3 && (self.y + 1) % 16 < 3)
                        {
                            // checks to make sure link is centered on the stairs
                            MetatileType mt_i = GetMetaTileTypeAtLocation(x + 8, y + 8);
                            if (mt_i != MetatileType.STAIRS)
                                return false;

                            OC.stair_warp_flag = true;
                        }
                        return false;

                    case MetatileType.BLUE_STAIRS:
                        stair_speed = true;
                        return false;

                    case MetatileType.DOCK:
                        if (!raft)
                            return false;

                        // verify which screen we're on because dock tile is also used elsewhere
                        if ((self.x + 1) % 16 < 3 && (self.y + 1) % 16 < 3
                            && GetMetaTileTypeAtLocation(self.x + 8, self.y + 8) == MetatileType.DOCK
                            && OC.current_screen is (63 or 85))
                        {
                            // center link on the dock if he arrives from the side via ladder
                            GotoMod8();

                            if (self.y < 80)
                                facing_direction = Direction.DOWN;
                            else
                                facing_direction = Direction.UP;

                            if (!OC.raft_flag)
                                new RaftSprite();

                            OC.raft_flag = true;
                            can_move = false;
                        }
                        return false;

                    // special tiles that ladder turns water into (half block)
                    case MetatileType.LADDER_TOP: // U
                        return (self.y + y_add) % 16 < 8;
                    case MetatileType.LADDER_BOTTOM: // D
                        return (self.y + y_add) % 16 > 8;
                    case MetatileType.LADDER_LEFT: // L
                        return (self.x + x_add) % 16 < 8;
                    case MetatileType.LADDER_RIGHT: // R
                        return (self.x + x_add) % 16 > 8;
                    case MetatileType.LADDER_TL: // TL
                        return (self.x + x_add) % 16 < 8 || (self.y + y_add) % 16 < 8;
                    case MetatileType.LADDER_TR: // TR
                        return (self.x + x_add) % 16 > 8 || (self.y + y_add) % 16 < 8;
                    case MetatileType.LADDER_EMPTY: // none
                        return false;

                    case MetatileType.WATER:
                        if (metatile_index > 16 && metatile_index < 32)
                        {
                            if (meta_tiles[metatile_index - 16].special)
                            {
                                SetPos(new_y: y - 1);
                                goto case MetatileType.DOCK;
                            }
                        }
                        goto case MetatileType.WATER_L;

                    case MetatileType.WATER_L:
                    case MetatileType.WATER_R or MetatileType.WATER_TL or MetatileType.WATER_T or MetatileType.WATER_TR or 
                    MetatileType.WATER_B or MetatileType.WATER_BL or MetatileType.WATER_BR:
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
            else if (gamemode == Gamemode.DUNGEON)
            {
                switch (meta_tiles[metatile_index].tile_index_D)
                {
                    case DungeonMetatile.GROUND or DungeonMetatile.SAND or DungeonMetatile.VOID or DungeonMetatile.GRAY_STAIRS:
                        // checking if dpad held makes it so if the first collision point is on ground but the second on a block,
                        // the push timer keeps going
                        if (!IsHeld(Buttons.UP) && !IsHeld(Buttons.DOWN) && !IsHeld(Buttons.LEFT) && !IsHeld(Buttons.RIGHT))
                            dungeon_wall_push_timer = 0;
                        return false;

                    case DungeonMetatile.WALL:
                        dungeon_wall_push_timer++;
                        if (meta_tiles[metatile_index].special && dungeon_wall_push_timer >= 8 && DC.nb_enemies_alive == 0)
                        {
                            new MovingTileSprite(MovingTileSprite.MovingTile.DUNGEON_BLOCK, metatile_index);
                        }
                        return true;
                    case DungeonMetatile.LEFT_STATUE or DungeonMetatile.RIGHT_STATUE or DungeonMetatile.GRAY_BRICKS:
                        return true;

                    case DungeonMetatile.WATER:
                        if (ladder && !ladder_used)
                        {
                            new LadderSprite(metatile_index);
                            return false;
                        }
                        return true;

                    case DungeonMetatile.STAIRS:
                        DC.warp_flag = true;
                        return false;

                    case DungeonMetatile.ROOM_TOP: // top of dungeon room
                        bool is_colliding = ((self.y + y_add) % 16) < 8;
                        if (is_colliding)
                            dungeon_wall_push_timer++;
                        else
                            dungeon_wall_push_timer = 0;
                        return is_colliding;
                    case DungeonMetatile.VERT_DOOR_LEFT: // left of up/down doors
                        return ((self.x + x_add) % 16) < 8;
                    case DungeonMetatile.VERT_DOOR_RIGHT: // right of up/down doors
                        return ((self.x + x_add) % 16) > 8;

                    case DungeonMetatile.WALK_THROUGH_WALL: // walk-through tile
                        // TODO: 8 frame timer before animation
                        return true;

                    case DungeonMetatile.TOP_DOOR_OPEN_L:
                        return (self.x + x_add) % 16 < 8 && (self.y + y_add) % 16 < 8;
                    case DungeonMetatile.TOP_DOOR_OPEN_R:
                        return (self.x + x_add) % 16 > 8 && (self.y + y_add) % 16 < 8;

                    // LADDER SHENANIGANS
                    // special tiles that ladder turns water into (half block)
                    case (DungeonMetatile)MetatileType.LADDER_TOP: // U
                        return (self.y + y_add) % 16 < 8;
                    case (DungeonMetatile)MetatileType.LADDER_BOTTOM: // D
                        return (self.y + y_add) % 16 > 8;
                    case (DungeonMetatile)MetatileType.LADDER_LEFT: // L
                        return (self.x + x_add) % 16 < 8;
                    case (DungeonMetatile)MetatileType.LADDER_RIGHT: // R
                        return (self.x + x_add) % 16 > 8;
                    case (DungeonMetatile)MetatileType.LADDER_TL: // TL
                        return (self.x + x_add) % 16 < 8 || (self.y + y_add) % 16 < 8;
                    case (DungeonMetatile)MetatileType.LADDER_TR: // TR
                        return (self.x + x_add) % 16 > 8 || (self.y + y_add) % 16 < 8;
                    case (DungeonMetatile)MetatileType.LADDER_EMPTY: // none
                        return false;
                }
            }

            return false;
        }

        public void TakeDamage(float damage)
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
            if ((int)knockback_direction < 2 && x % 8 == 0 ||
                (int)knockback_direction >= 2 && y % 8 == 0)
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
            DeathCode.died_in_dungeon = gamemode == Gamemode.DUNGEON;
            gamemode = Gamemode.DEATH;
        }

        // checks to se eif link should flash, does it if so
        void HitFlash()
        {
            if (iframes_timer <= 0 && !clock_flash)
                return;

            byte new_palette;
            if (iframes_timer == 1)
                new_palette = 4;
            else
                new_palette = (byte)(gTimer / 2 % 4 + 4);

            self.palette_index = new_palette;
            counterpart.palette_index = new_palette;
            iframes_timer--;
        }

        bool Knockback()
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
        public bool AddRupees(int change, bool perform_addition = true)
        {
            int new_val = rupy_count + change;

            int clamped_val = Math.Clamp(new_val, byte.MinValue, byte.MaxValue);

            if (perform_addition)
            {
                rupy_count = (byte)clamped_val;
            }

            return new_val == clamped_val;
        }

        // from IBoomerangThrower
        public void BoomerangRetreive()
        {
            if (Link.current_action == LinkAction.WALKING_LEFT)
                Link.current_action = LinkAction.ATTACK_LEFT;
            else if (Link.current_action == LinkAction.WALKING_UP)
                Link.current_action = LinkAction.ATTACK_UP;
            else if (Link.current_action == LinkAction.WALKING_DOWN)
                Link.current_action = LinkAction.ATTACK_DOWN;
            else if (Link.current_action == LinkAction.WALKING_RIGHT)
                Link.current_action = LinkAction.ATTACK_RIGHT;
            Link.using_item = true;
        }
        public void BoomerangThrow() => animation_timer = 0;
    }
}