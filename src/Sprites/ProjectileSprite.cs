using The_Legend_of_Zelda.Gameplay;
using The_Legend_of_Zelda.Rendering;
using static The_Legend_of_Zelda.Gameplay.Program;

namespace The_Legend_of_Zelda.Sprites
{
    public abstract class ProjectileSprite : Sprite
    {
        public bool is_from_link;
        public bool hit_target = false;
        public bool single_wide;
        public bool does_damage = true;
        public bool bonk = false;
        public byte animation_timer = 0;
        public float damage;

        public Direction direction;
        public StaticSprite counterpart = new StaticSprite(SpriteID.BLANK, 0, 0, 0);

        public ProjectileSprite(bool is_from_link, bool single_wide, float damage) : base(0, 0)
        {
            this.single_wide = single_wide;
            this.is_from_link = is_from_link;
            this.damage = damage;
            Screen.sprites.Add(this);
            if (!single_wide)
                Screen.sprites.Add(counterpart);
            unload_during_transition = true;
            counterpart.unload_during_transition = true;
        }

        public override void Action()
        {
            if (does_damage)
                DoDamage();

            if (bonk)
                Bonk();
            else
                ProjSpecificActions();
        }

        public abstract void ProjSpecificActions();

        //TODO: less in dungeons?
        public bool CheckIfEdgeHit()
        {
            return x >= 232 || x <= 8 || y <= 64 || y >= 216;
        }

        public void Move(byte speed = 3)
        {
            if (direction == Direction.UP)
                y -= speed;
            else if (direction == Direction.DOWN)
                y += speed;
            else if (direction == Direction.LEFT)
                x -= speed;
            else
                x += speed;

            counterpart.x = x + 8;
            counterpart.y = y;
        }

        void DoDamage()
        {
            if (hit_target)
                return;

            byte width;
            if (single_wide)
                width = 8;
            else
                width = 16;

            if (is_from_link)
            {
                for (int i = 0; i < Screen.sprites.Count; i++)
                {
                    if (Screen.sprites[i] is not Enemy enemy)
                    {
                        continue;
                    }

                    // if no collision or enemy is invincible, continue
                    if (!(x < enemy.x + 16 &&
                        x + width > enemy.x &&
                        y < enemy.y + 16 &&
                        y + 16 > enemy.y &&
                        !enemy.invincible))
                    {
                        continue;
                    }

                    enemy.hit_cause = this.GetType();

                    //TODO: some enemies are vincible to the boomerang!
                    //TODO: some enemies don't get stunned by the boomerang!
                    //TODO: put all this bs in virtual func
                    if (this is BoomerangSprite)
                    {
                        if (enemy.current_action != Enemy.ActionState.STUNNED)
                        {
                            enemy.unstunned_action = enemy.current_action;
                            enemy.current_action = Enemy.ActionState.STUNNED;
                        }
                    }

                    if (!enemy.OnProjectileHit())
                    {
                        continue;
                    }

                    enemy.TakeDamage(damage);
                    hit_target = true;
                    break;
                }

                return;
            }

            // return if not colliding
            if (!(x < Program.Link.x + 16 &&
                x + width > Program.Link.x &&
                y < Program.Link.y + 16 &&
                y + 16 > Program.Link.y))
            {
                return;
            }

            // if link is facing the projectile
            if (Program.Link.facing_direction == direction.Opposite() && !(Program.Link.current_action >= LinkAction.ATTACK_LEFT && Program.Link.current_action <= LinkAction.ATTACK_DOWN))
            {
                // small rocks and arrows can be reflected by any shield, swords and beams need the magic shield
                if (this is RockProjectileSprite || this is ArrowSprite ||
                    (this is SwordProjectileSprite || this is MagicBeamSprite) && SaveLoad.magical_shield)
                {
                    bonk = true;
                }
            }

            if (!bonk)
            {
                Program.Link.knockback_direction = direction;
                Program.Link.TakeDamage(damage);
                hit_target = true;
                return;
            }

            animation_timer = 0;
        }

        // bonk animation
        void Bonk()
        {
            if (direction == Direction.UP)
            {
                x++;
                y += 2;
            }
            else if (direction == Direction.DOWN)
            {
                x++;
                y -= 2;
            }
            else if (direction == Direction.LEFT)
            {
                y--;
                x += 2;
            }
            else
            {
                y--;
                x -= 2;
            }
            counterpart.x = x + 8;
            counterpart.y = y;

            if (animation_timer == 16)
            {
                Screen.sprites.Remove(counterpart);
                Screen.sprites.Remove(this);
            }
            animation_timer++;
        }
    }

    internal class SwordProjectileSprite : ProjectileSprite
    {
        bool explosion_flag = false;
        StaticSprite[] explosion_effects = new StaticSprite[4];

        public SwordProjectileSprite(int x, int y, Direction direction, bool is_from_link) : base(is_from_link, true, 1)
        {
            tile_index = 0x20;
            counterpart.tile_index = 0x84;
            palette_index = 4;
            counterpart.palette_index = 4;
            this.x = x;
            this.y = y;
            this.direction = direction;
            unload_during_transition = true;
            counterpart.unload_during_transition = true;

            if (!is_from_link)
                damage = 2;

            if (SaveLoad.magical_sword)
                damage = 4;
            else if (SaveLoad.white_sword)
                damage = 2;

            if (direction == Direction.DOWN)
            {
                yflip = true;
            }
            else if (direction == Direction.LEFT)
            {
                this.y += 3;
                tile_index = 0x84;
                counterpart.tile_index = 0x82;
                xflip = true;
                counterpart.xflip = true;
                single_wide = false;
            }
            else if (direction == Direction.RIGHT)
            {
                this.y += 3;
                tile_index = 0x82;
                counterpart.tile_index = 0x84;
                single_wide = false;
            }

            if (!single_wide)
                Screen.sprites.Add(counterpart);
        }

        public override void ProjSpecificActions()
        {
            if (hit_target)
                explosion_flag = true;

            byte new_plt_index;

            if (!(CheckIfEdgeHit() || explosion_flag))
            {
                new_plt_index = (byte)(gTimer % 4 + 4);
                palette_index = new_plt_index;
                counterpart.palette_index = new_plt_index;
                Move();

                return;
            }

            explosion_flag = true;
            if (animation_timer == 0)
            {
                does_damage = false;
                Screen.sprites.Remove(counterpart);

                if (!is_from_link)
                {
                    Screen.sprites.Remove(this);
                    return;
                }

                shown = false;
                for (int i = 0; i < explosion_effects.Length; i++)
                {
                    explosion_effects[i] = new StaticSprite(0x30, 4, x, y + 4);
                    if (i % 2 == 0)
                        explosion_effects[i].xflip = true;
                    if (i > 1)
                        explosion_effects[i].yflip = true;
                    explosion_effects[i].unload_during_transition = true;
                    Screen.sprites.Add(explosion_effects[i]);
                }
            }
            else if (animation_timer == 23)
            {
                for (int i = 0; i < explosion_effects.Length; i++)
                {
                    Screen.sprites.Remove(explosion_effects[i]);
                }
                Menu.sword_proj_out = false;
                Screen.sprites.Remove(this);
                return;
            }

            new_plt_index = (byte)(gTimer % 4 + 4);
            for (int i = 0; i < explosion_effects.Length; i++)
            {
                explosion_effects[i].palette_index = new_plt_index;
                if (i % 2 == 0)
                    explosion_effects[i].x++;
                else
                    explosion_effects[i].x--;

                if (i > 1)
                    explosion_effects[i].y++;
                else
                    explosion_effects[i].y--;
            }

            animation_timer++;
        }
    }

    internal class BombSprite : ProjectileSprite
    {
        byte explosion_timer = 86;
        StaticSprite[] smoke = new StaticSprite[8];
        static readonly int[,] smoke_init_pos = new int[8, 2]
        {
            { -20, 0},
            { -12, 0},
            { -12, 16},
            { -4, 16},
            { -4, 0},
            { 4, 0},
            { 4, -16},
            { 12, -16}
        };

        public BombSprite(int x, int y) : base(true, true, 4)
        {
            // important for darknuts!
            direction = Link.facing_direction;
            tile_index = 0x34;
            palette_index = 5;
            does_damage = false;
            this.x = x;
            this.y = y;
            for (int i = 0; i < smoke.Length; i++)
            {
                smoke[i] = new StaticSprite(0x70, 5, x, y);
                smoke[i].xflip = i % 2 == 1;
                smoke[i].x = x + smoke_init_pos[i, 0];
                smoke[i].y = y + smoke_init_pos[i, 1];
                smoke[i].unload_during_transition = true;
            }
            Sound.PlaySFX(Sound.SoundEffects.BOMB_PLACE);
        }

        public override void ProjSpecificActions()
        {
            explosion_timer--;
            if (explosion_timer == 37)
            {
                for (int i = 0; i < smoke.Length; i++)
                {
                    Screen.sprites.Add(smoke[i]);
                }
                shown = false;
                Menu.bomb_out = false;
                HurtEnemies();
                UncoverHoles();
                Sound.PlaySFX(Sound.SoundEffects.EXPLOSION);
                return;
            }
            else if (explosion_timer == 35 || explosion_timer == 30)
            {
                Palettes.grayscale_mode = true;
            }
            else if (explosion_timer == 31 || explosion_timer == 26)
            {
                Palettes.grayscale_mode = false;
            }
            else if (explosion_timer == 13)
            {
                for (int i = 0; i < smoke.Length; i++)
                {
                    smoke[i].tile_index = 0x72;
                }
            }
            else if (explosion_timer == 0)
            {
                for (int i = 0; i < smoke.Length; i++)
                {
                    Screen.sprites.Remove(smoke[i]);
                }
                Screen.sprites.Remove(this);
            }
            if (explosion_timer < 37)
            {
                for (int i = 0; i < smoke.Length; i++)
                {
                    smoke[i].x = x - smoke[i].x + x;
                    smoke[i].xflip = !smoke[i].xflip;
                }
            }

        }

        void UncoverHoles()
        {
            int metatile_index;
            for (int i = -16; i < 17; i += 16)
            {
                for (int j = -16; j < 17; j += 16)
                {
                    metatile_index = Screen.GetMetaTileIndexAtLocation(x + j, y + i);

                    if (metatile_index < 0)
                        continue;

                    if (gamemode == Gamemode.OVERWORLD)
                    {
                        if (!Screen.meta_tiles[metatile_index].special)
                            continue;

                        SaveLoad.SetOverworldSecretsFlag((byte)Array.IndexOf(OC.screens_with_secrets_list, OC.current_screen), true);
                        Screen.meta_tiles[metatile_index].tile_index = MetatileType.BLACK_SQUARE_WARP;
                        int ppu_index = 2 * metatile_index + 2 * (metatile_index & (~0xF)) + 256;
                        Textures.ppu[ppu_index] = 0x24;
                        Textures.ppu[ppu_index + 1] = 0x24;
                        Textures.ppu[ppu_index + 32] = 0x24;
                        Textures.ppu[ppu_index + 33] = 0x24;
                    }
                    else
                    {
                        byte[] door_metatiles = { 23, 151, 81, 94 };
                        if (door_metatiles.Contains((byte)metatile_index))
                        {
                            DC.door_statuses[Array.IndexOf(door_metatiles, (byte)metatile_index)] = true;
                        }
                    }
                }
            }
        }

        void HurtEnemies()
        {
            for (int i = 0; i < Screen.sprites.Count; i++)
            {
                if (Screen.sprites[i] is not Enemy e)
                    continue;

                if (e.y < y + 24 &&
                    e.y > y - 24 &&
                    e.x < x + 24 &&
                    e.x > x - 24)
                {
                    if (e.TakeDamage(4))
                        i--;
                }
            }
        }
    }

    internal class ThrownFireSprite : ProjectileSprite
    {
        public ThrownFireSprite(int x, int y, Direction direction) : base(true, false, 0.5f)
        {
            this.x = x;
            this.y = y;
            this.direction = direction;
            tile_index = 0x5c;
            palette_index = 6;
            counterpart.tile_index = 0x5e;
            counterpart.palette_index = 6;
            counterpart.x = x + 8;
            counterpart.y = y;
            unload_during_transition = true;
            counterpart.unload_during_transition = true;
            Screen.sprites.Add(this);
            Screen.sprites.Add(counterpart);
        }

        public override void ProjSpecificActions()
        {
            // fire damages link and ennemies, this makes it so it alternates between calculating dmg for link and enemies every frame
            // fire does not damage link for the first 8 frames to prevent damaging him as he throws it. it only happened when he threw it down,
            // so someone could try to fix it later but this works too
            if (animation_timer > 8)
                is_from_link = !is_from_link;

            if (animation_timer < 32 && animation_timer % 2 == 1)
            {
                Move(1);
            }
            else if (animation_timer == 33)
            {
                if (gamemode == Gamemode.DUNGEON && DC.is_dark)
                {
                    DC.DarkeningAnimationDisable();
                }
            }
            else if (animation_timer == 94)
            {
                BurnBushes();
                if (Menu.fire_out > 0)
                    Menu.fire_out--;
                Screen.sprites.Remove(counterpart);
                Screen.sprites.Remove(this);
                return;
            }

            if (animation_timer % 4 == 0)
            {
                (tile_index, counterpart.tile_index) = (counterpart.tile_index, tile_index);
                xflip = !xflip;
                counterpart.xflip = !counterpart.xflip;
            }
            counterpart.x = x + 8;
            counterpart.y = y;
            animation_timer++;
        }

        void BurnBushes()
        {
            int metatile_index;

            if (gamemode == Gamemode.DUNGEON)
                return;

            for (int i = -8; i < 9; i += 8)
            {
                for (int j = -8; j < 9; j += 8)
                {
                    metatile_index = (y + i & 0xFFF0) + (x + j >> 4) - 64;

                    if (metatile_index < 0 || metatile_index > Screen.meta_tiles.Length)
                        continue;

                    if (!Screen.meta_tiles[metatile_index].special)
                        continue;

                    SaveLoad.SetOverworldSecretsFlag((byte)Array.IndexOf(OC.screens_with_secrets_list, OC.current_screen), true);
                    Screen.meta_tiles[metatile_index].tile_index = MetatileType.STAIRS;
                    int ppu_index = 2 * metatile_index + 2 * (metatile_index & 0xFFFFFF0) + 256;
                    Textures.ppu[ppu_index] = 0x70;
                    Textures.ppu[ppu_index + 1] = 0x72;
                    Textures.ppu[ppu_index + 32] = 0x71;
                    Textures.ppu[ppu_index + 33] = 0x73;
                }
            }
        }
    }

    internal class MagicBeamSprite : ProjectileSprite
    {
        bool attack_or_fire_flag = false;
        public MagicBeamSprite(int x, int y, Direction direction, bool is_from_link) : base(is_from_link, false, 2)
        {
            tile_index = 0x7a;
            counterpart.tile_index = 0x7a;
            palette_index = 4;
            counterpart.palette_index = 4;
            this.x = x;
            this.y = y;
            this.direction = direction;
            unload_during_transition = true;
            counterpart.unload_during_transition = true;
            if (direction == Direction.DOWN)
            {
                yflip = true;
                counterpart.yflip = true;
                counterpart.xflip = true;
            }
            else if (direction == Direction.LEFT)
            {
                tile_index = 0x7e;
                counterpart.tile_index = 0x7c;
                xflip = true;
                counterpart.xflip = true;
            }
            else if (direction == Direction.RIGHT)
            {
                tile_index = 0x7c;
                counterpart.tile_index = 0x7e;
            }
            else
            {
                counterpart.xflip = true;
            }
        }

        public override void ProjSpecificActions()
        {
            if (CheckIfEdgeHit() || attack_or_fire_flag)
            {
                Menu.magic_wave_out = false;
                attack_or_fire_flag = true;
                if (SaveLoad.book_of_magic)
                {
                    ThrownFireSprite fire = new ThrownFireSprite(x, y, direction);
                    fire.animation_timer = 32;
                }
                Screen.sprites.Remove(counterpart);
                Screen.sprites.Remove(this);
                return;
            }

            byte distance = (byte)(gTimer % 2 + 2);
            Move(distance);

            byte new_plt_index = (byte)(gTimer % 4 + 4);
            palette_index = new_plt_index;
            counterpart.palette_index = new_plt_index;
        }
    }

    public interface IBoomerangThrower
    {
        public bool boomerang_out { get; set; }
        public EightDirection boomerang_throw_dir { get; set; }
        public (int x, int y) return_pos { get; set; }

        public abstract void BoomerangThrow();
        public abstract void BoomerangRetreive();
    }
    internal class BoomerangSprite : ProjectileSprite
    {
        byte local_timer = 0;
        byte speed = 3;
        bool returning = false;
        int x_dist_from_parent, y_dist_from_parent;
        int return_x, return_y;
        EightDirection m_direction;
        IBoomerangThrower parent;

        public BoomerangSprite(int x, int y, bool is_from_link, bool blue, IBoomerangThrower parent) : base(is_from_link, true, 0)
        {
            this.x = x;
            this.y = y;
            // just in case thrower dies next frame. if that happens without this, boomerang would go to 0,0
            return_x = this.x;
            return_y = this.y;
            this.parent = parent;
            if (blue)
                palette_index = 5;
            unload_during_transition = true;
            parent.boomerang_out = true;
            // this is important! boomerang effects happen in damage function, and even if damage==0, the boomerang "does damage"
            does_damage = true;
            m_direction = parent.boomerang_throw_dir;
            parent.BoomerangThrow();
        }

        public override void ProjSpecificActions()
        {
            if (!(tile_index == 0x3c && !returning))
            {
                switch ((local_timer % 16) / 2)
                {
                    case 0:
                        tile_index = 0x36;
                        if (!SaveLoad.magical_boomerang)
                            palette_index = 4;
                        xflip = false;
                        yflip = false;
                        break;
                    case 1:
                        tile_index = 0x38;
                        break;
                    case 2:
                        tile_index = 0x3a;
                        break;
                    case 3:
                        tile_index = 0x38;
                        xflip = true;
                        break;
                    case 4:
                        tile_index = 0x36;
                        break;
                    case 5:
                        tile_index = 0x38;
                        yflip = true;
                        break;
                    case 6:
                        tile_index = 0x3a;
                        xflip = false;
                        yflip = true;
                        break;
                    case 7:
                        tile_index = 0x38;
                        yflip = true;
                        break;
                }
            }
            if (local_timer % 14 == 0)
            {
                Sound.PlaySFX(Sound.SoundEffects.BOOMERANG);
            }
            local_timer++;

            return_x = parent.return_pos.x;
            return_y = parent.return_pos.y;
            //if (parent is LinkSprite l)
            //{
            //    return_x = l.x;
            //    return_y = l.y;
            //}
            //// only update return pos if parent still exists
            //if (parent is Sprite s && Screen.sprites.Contains(s))
            //{
            //    return_x = s.x;
            //    return_y = s.y;
            //}

            //TODO: what about goriya? booreangs are not always thrown by link
            if (returning)
            {
                // https://www.desmos.com/calculator/esasntj0cm
                x_dist_from_parent = x + 4 - (return_x + 8);
                y_dist_from_parent = y + 8 - (return_y + 8);
                // +0.01f auto converts y_dist_from_link to float AND prevents div by 0 error
                float angle = MathF.Atan(x_dist_from_parent / (y_dist_from_parent + 0.01f));
                float x_dist_to_move = MathF.Sin(angle) * speed;
                float y_dist_to_move = MathF.Cos(angle) * speed;
                if (y_dist_from_parent >= 0)
                {
                    x_dist_to_move = -x_dist_to_move;
                    y_dist_to_move = -y_dist_to_move;
                }
                x_dist_to_move = MathF.Round(x_dist_to_move, MidpointRounding.AwayFromZero);
                y_dist_to_move = MathF.Round(y_dist_to_move, MidpointRounding.AwayFromZero);

                x += (int)x_dist_to_move;
                y += (int)y_dist_to_move;

                if (Math.Abs(x_dist_from_parent) < 8 && Math.Abs(y_dist_from_parent) < 8)
                {
                    parent.BoomerangRetreive();
                    parent.boomerang_out = false;
                    Menu.boomerang_out = false;
                    Screen.sprites.Remove(this);
                }

                if (local_timer == 16)
                {
                    speed = 3;
                }

                return;
            }

            if (m_direction is not (EightDirection.UP or EightDirection.DOWN))
            {
                if (m_direction is EightDirection.UP or EightDirection.LEFT or EightDirection.UPLEFT or EightDirection.DOWNLEFT)
                    x -= speed;
                else
                    x += speed;
            }
            if (m_direction is not (EightDirection.LEFT or EightDirection.RIGHT))
            {
                if (m_direction is EightDirection.UP or EightDirection.DOWN)
                {
                    y += m_direction == EightDirection.UP ? -speed : speed;
                }
                else if (m_direction is EightDirection.DOWNLEFT or EightDirection.DOWNRIGHT)
                {
                    y += speed;
                }
                else
                {
                    y -= speed;
                }
            }

            if (tile_index == 0x3c)
            {
                returning = true;
                local_timer = 0;
                //Link.animation_timer += 48;//
                return;
            }

            if (CheckIfEdgeHit() || hit_target)
            {
                tile_index = 0x3c;
                palette_index = 5;
            }

            if (SaveLoad.magical_boomerang)
                return;

            if (local_timer == 16)
            {
                speed = 1;
            }
            else if (local_timer > 34)
            {
                returning = true;
                local_timer = 0;
            }
        }

        public static EightDirection FindLinkBoomerangDirection()
        {
            if (Control.IsHeld(Buttons.UP) && !Control.IsHeld(Buttons.DOWN))
            {
                if (Control.IsHeld(Buttons.LEFT) && !Control.IsHeld(Buttons.RIGHT))
                    return EightDirection.UPLEFT;
                else if (Control.IsHeld(Buttons.RIGHT) && !Control.IsHeld(Buttons.LEFT))
                    return EightDirection.UPRIGHT;
                else
                    return EightDirection.UP;
            }
            else if (Control.IsHeld(Buttons.DOWN) && !Control.IsHeld(Buttons.UP))
            {
                if (Control.IsHeld(Buttons.LEFT) && !Control.IsHeld(Buttons.RIGHT))
                    return EightDirection.DOWNLEFT;
                else if (Control.IsHeld(Buttons.RIGHT) && !Control.IsHeld(Buttons.LEFT))
                    return EightDirection.DOWNRIGHT;
                else
                    return EightDirection.DOWN;
            }
            else
            {
                return (EightDirection)Link.facing_direction;
            }
        }
    }

    internal class ArrowSprite : ProjectileSprite
    {
        public ArrowSprite(int x, int y, Direction direction, bool is_from_link) : base(is_from_link, true, 2)
        {
            tile_index = 0x28;
            palette_index = 4;
            counterpart.tile_index = 0x88;
            counterpart.palette_index = 4;
            this.x = x;
            this.y = y;
            this.direction = direction;
            unload_during_transition = true;
            counterpart.unload_during_transition = true;

            if (!is_from_link)
            {
                palette_index = 6;
                counterpart.palette_index = 6;
            }
            else if (SaveLoad.silver_arrow)
            {
                palette_index = 5;
                counterpart.palette_index = 5;
                damage = 4;
            }

            if (direction == Direction.DOWN)
            {
                yflip = true;
            }
            else if (direction == Direction.LEFT)
            {
                tile_index = 0x88;
                counterpart.tile_index = 0x86;
                xflip = true;
                counterpart.xflip = true;
                single_wide = false;
            }
            else if (direction == Direction.RIGHT)
            {
                tile_index = 0x86;
                counterpart.tile_index = 0x88;
                single_wide = false;
            }

            if (!single_wide)
            {
                Screen.sprites.Add(counterpart);
            }
        }

        public override void ProjSpecificActions()
        {
            if (tile_index == 0x3c)
            {
                animation_timer++;
                if (animation_timer == 3)
                {
                    Menu.arrow_out = false;
                    Screen.sprites.Remove(this);
                }

                return;
            }

            Move();

            if (CheckIfEdgeHit())
            {
                palette_index = 5;
                tile_index = 0x3c;
                Screen.sprites.Remove(counterpart);
            }
        }
    }

    internal class RockProjectileSprite : ProjectileSprite
    {
        public RockProjectileSprite(int x, int y, Direction direction) : base(false, true, 0.5f)
        {
            this.x = x;
            this.y = y;
            this.direction = direction;
            unload_during_transition = true;
            tile_index = 0x9e;
            palette_index = 4;
        }

        public override void ProjSpecificActions()
        {
            Move();

            if (hit_target || CheckIfEdgeHit())
            {
                Screen.sprites.Remove(this);
            }
        }
    }

    internal class MagicOrbProjectileSprite : ProjectileSprite
    {
        float x_speed, y_speed;
        float true_x, true_y;

        public MagicOrbProjectileSprite(int x, int y, bool no_charge = false) : base(false, true, 0.5f)
        {
            this.x = x;
            this.y = y;
            tile_index = 0x44;
            unload_during_transition = true;
            if (no_charge)
                animation_timer = 15;
        }

        public override void ProjSpecificActions()
        {
            palette_index = (byte)(animation_timer % 4 + 4);
            animation_timer++;
            if (hit_target || CheckIfEdgeHit() && animation_timer > 21)
                Screen.sprites.Remove(this);

            if (animation_timer < 16)
            {
                return;
            }

            if (animation_timer > 16)
            {
                true_x = x + x_speed;
                true_y = y + y_speed;
                x = (int)MathF.Round(true_x);
                y = (int)MathF.Round(true_y);
                return;
            }

            // if animation_timer == 16
            // copied from boomerang code
            int x_dist_from_link = x + 4 - (Link.x + 8);
            int y_dist_from_link = y + 8 - (Link.y + 8);

            float angle = MathF.Atan(x_dist_from_link / (y_dist_from_link + 0.01f)); // +0.01f auto converts y_dist_from_link to float AND prevents div by 0 error

            float x_dist_to_move = MathF.Sin(angle) * 1.5f;
            float y_dist_to_move = MathF.Cos(angle) * 1.5f;

            if (y_dist_from_link >= 0)
            {
                x_dist_to_move = -x_dist_to_move;
                y_dist_to_move = -y_dist_to_move;
            }

            x_dist_to_move = MathF.Round(x_dist_to_move, MidpointRounding.AwayFromZero);
            y_dist_to_move = MathF.Round(y_dist_to_move, MidpointRounding.AwayFromZero);

            x_speed = x_dist_to_move;
            y_speed = y_dist_to_move;

            if (MathF.Abs(x_speed) > MathF.Abs(y_speed)) // setting direction is only so that knockback works properly
            {
                if (x_speed > 0)
                    direction = Direction.RIGHT;
                else
                    direction = Direction.LEFT;
            }
            else
            {
                if (y_speed > 0)
                    direction = Direction.DOWN;
                else
                    direction = Direction.UP;
            }
        }
    }
}