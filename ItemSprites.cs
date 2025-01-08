using Microsoft.VisualBasic;
using System.Data.Common;
using static The_Legend_of_Zelda.Enemy;

namespace The_Legend_of_Zelda
{
    public enum EightDirection
    {
        UP,
        DOWN,
        LEFT,// the ordering of the first 4 needs to be the same as Direction for FindBoomerangDirection() to be simpler
        RIGHT,
        UPLEFT,
        UPRIGHT,
        DOWNLEFT,
        DOWNRIGHT
    }

    internal abstract class FairySprite : FlickeringSprite
    {
        public FairySprite(int x, int y) : base(0x50, 6, x, y, 4, 0x52)
        {
            this.x = x;
            this.y = y;
        }
    }

    internal abstract class ProjectileSprite : Sprite
    {
        public bool is_from_link;
        public bool hit_target = false;
        public bool single_wide;
        public bool does_damage = true;
        public bool bonk = false;
        public byte animation_timer = 0;
        public float damage;

        public Direction direction;
        public StaticSprite counterpart = new StaticSprite(0, 0, 0, 0);

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

            counterpart.x = (short)(x + 8);
            counterpart.y = y;
        }

        void DoDamage()
        {
            byte width;
            if (single_wide)
                width = 8;
            else
                width = 16;

            if (is_from_link)
            {
                for (int i = 0; i < Screen.sprites.Count; i++)
                {
                    if (Screen.sprites[i].GetType().BaseType != typeof(Enemy))
                    {
                        continue;
                    }

                    Enemy enemy = (Enemy)Screen.sprites[i];

                    // if no collision or enemy is invincible, continue
                    if (!(x < enemy.x + 16 && 
                        x + width > enemy.x && 
                        y < enemy.y + 16 && 
                        y + 16 > enemy.y &&
                        !enemy.invincible))
                    {
                        continue;
                    }

                    if (this is BoomerangSprite &&
                        enemy.current_action != Enemy.ActionState.STUNNED)
                    {
                        enemy.unstunned_action = enemy.current_action;
                        enemy.current_action = Enemy.ActionState.STUNNED;
                    }
                    else
                    {
                        enemy.TakeDamage(damage);
                    }

                    hit_target = true;
                    break;
                }
            }

            // return if not colliding
            if (!(x < Link.x + 16 && 
                x + width > Link.x && 
                y < Link.y + 16 && 
                y + 16 > Link.y))
            {
                return;
            }

            // if link is... uh...
            if ((int)Link.facing_direction == ((int)direction ^ 1) && !((int)Link.current_action >= 4 && (int)Link.current_action <= 7))
            {
                // small rocks and arrows can be reflected by any shield, swords and beams need the magic shield
                if (this is RockProjectileSprite || this is ArrowSprite ||
                    ((this is SwordProjectileSprite || this is MagicBeamSprite) && SaveLoad.magical_shield[SaveLoad.current_save_file]))
                {
                    bonk = true;
                }
            }

            if (!bonk)
            {
                Link.knockback_direction = direction;
                Link.TakeDamage(damage);
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
    internal abstract class ItemDropSprite : Sprite
    {
        public bool collected = false;
        public int local_timer = 0;
        public bool dbl_wide = false;
        bool despawn;

        public ItemDropSprite(int x, int y, bool despawn = true) : base(0, 0)
        {
            this.x = x;
            this.y = y;
            this.despawn = despawn;
            unload_during_transition = true;
            Screen.sprites.Add(this);
        }

        public override void Action()
        {
            local_timer++;
            if (Dissapear() && despawn)
                Screen.sprites.Remove(this);
            if (CollidingWithLink())
                collected = true;

            ItemSpecificActions();
        }

        public abstract void ItemSpecificActions();

        public bool CollidingWithLink()
        {
            return x < Link.x + 16 && 
                x + (dbl_wide ? 16 : 8) > Link.x && 
                y < Link.y + 16 && 
                y + 16 > Link.y;
        }

        bool Dissapear()
        {
            return local_timer > 2000;
        }
    }

    internal class StaticHeartSprite : FlickeringSprite
    {
        public StaticHeartSprite(int x, int y) : base(0xf2, 6, x, y, 8, 0xf2, second_palette_index: 5)
        {
            use_chr_rom = true;
            ChangeTexture();
        }
    }

    internal class HeartItemSprite : ItemDropSprite
    {
        public HeartItemSprite(int x, int y) : base(x, y)
        {
            tile_index = 0xf2;
            use_chr_rom = true;
            ChangeTexture();
            palette_index = 5;
        }

        public override void ItemSpecificActions()
        {
            if (Program.gTimer % 8 == 0)
            {
                if (palette_index == 5)
                    palette_index = 6;
                else
                    palette_index = 5;
            }

            if (collected)
            {
                Link.hp += 1;
                Screen.sprites.Remove(this);
            }
        }
    }

    internal class RupySprite : ItemDropSprite
    {
        public short lTimer = 0;
        public bool five_rupies;

        public RupySprite(int x, int y, bool five_rupies, bool despawn = true) : base(x, y, despawn)
        {
            tile_index = 0x32;
            this.five_rupies = five_rupies;
            if (five_rupies)
                palette_index = 5;
            else
                palette_index = 6;
        }

        public override void ItemSpecificActions()
        {
            if (collected)
            {
                Link.AddRupees(five_rupies ? 5 : 1);
                Screen.sprites.Remove(this);
            }

            if (Program.gTimer % 8 == 0 && !five_rupies)
            {
                if (palette_index == 5)
                    palette_index = 6;
                else
                    palette_index = 5;
            }
        }
    }

    internal class FairyItemSprite : ItemDropSprite
    {
        int flying_timer = 0;
        int when_to_stop;
        EightDirection direction;

        public FairyItemSprite(int x, int y) : base(x, y)
        {
            tile_index = 0x50;
            palette_index = 6;
            Screen.sprites.Add(this);
        }

        public override void ItemSpecificActions()
        {
            if (local_timer % 8 == 0)
                tile_index = 0x50;
            else if (local_timer % 8 == 4)
                tile_index = 0x52;

            Fly();

            if (collected)
            {
                Link.hp += 3;
                //TODO: sfx fairy get sound
                Screen.sprites.Remove(this);
            }
        }

        void Fly()
        {
            if (flying_timer == 1)
            {
                direction = (EightDirection)Program.RNG.Next(8);
                when_to_stop = Program.RNG.Next(40, 90);
            }

            if (flying_timer > when_to_stop)
            {
                flying_timer = 0;
            }

            if (x <= 0 || x >= 240 || y <= 64 || y >= 224)
            {
                if (x <= 0)
                    direction = EightDirection.RIGHT;
                else if (x >= 240)
                    direction = EightDirection.LEFT;
                else if (y <= 64)
                    direction = EightDirection.DOWN;
                else
                    direction = EightDirection.UP;

                flying_timer = 2;
            }

            flying_timer++;
            Move8D();
        }

        void Move8D()
        {
            if (local_timer % 3 != 0)
                return;

            if (direction == EightDirection.UP || direction == EightDirection.UPLEFT ||
                direction == EightDirection.UPRIGHT)
            {
                y--;
            }
            else if (direction == EightDirection.DOWN || direction == EightDirection.DOWNLEFT ||
                direction == EightDirection.DOWNRIGHT)
            {
                y++;
            }

            if (direction == EightDirection.LEFT || direction == EightDirection.UPLEFT ||
                direction == EightDirection.DOWNLEFT)
            {
                x--;
            }
            else if (direction == EightDirection.RIGHT || direction == EightDirection.UPRIGHT ||
                direction == EightDirection.DOWNRIGHT)
            {
                x++;
            }
        }
    }

    //internal class TriforcePieceSprite : Sprite
    //{
    //    public TriforcePieceSprite(int x, int y, bool xflip = false) : base(0x6e, 5)
    //    {
    //        this.x = x;
    //        this.y = y;
    //        if (!xflip)
    //            Screen.sprites.Add((Sprite)new TriforcePieceSprite((short)(x + 8), y, true));
    //        this.xflip = xflip;
    //    }
    //    public override void Action()
    //    {
    //        if (Program.gTimer % 8 == 0)
    //        {
    //            if (palette_index == 5)
    //                palette_index = 6;
    //            else
    //                palette_index = 5;
    //        }
    //    }
    //}

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

            if (SaveLoad.magical_sword[SaveLoad.current_save_file])
                damage = 4;
            else if (SaveLoad.white_sword[SaveLoad.current_save_file])
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
                new_plt_index = (byte)((Program.gTimer & 3) + 4);
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
                    explosion_effects[i] = new StaticSprite(0x30, 4, x, (short)(y + 4));
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

            new_plt_index = (byte)((Program.gTimer % 4) + 4);
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
        public BombSprite(int x, int y) : base(true, true, 4)
        {
            direction = Link.facing_direction;
            tile_index = 0x34;
            palette_index = 5;
            does_damage = false;
            this.x = x;
            this.y = y;
            for (int i = 0; i < smoke.Length; i++)
            {
                smoke[i] = new StaticSprite(0x70, 5, x, y);
                if (i % 2 == 1)
                    smoke[i].xflip = true;
                smoke[i].x = (short)(x + (((i - 5) >> 1) * 8) + 4);
                smoke[i].y = (short)(y - 16 * Math.Abs((i >> 1) - 1) + 16);
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
                    smoke[i].x = (short)(x - smoke[i].x + x);
                    smoke[i].xflip = !smoke[i].xflip;
                }
            }

        }
        void UncoverHoles()
        {
            int metatile_index;
            for (sbyte i = -16; i < 17; i += 16)
            {
                for (sbyte j = -16; j < 17; j += 16)
                {
                    metatile_index = ((y + i) & 0xFFF0) + ((x + j) >> 4) - 64;

                    if (metatile_index < 0 || metatile_index > Screen.meta_tiles.Length)
                        continue;

                    if (Program.gamemode == Program.Gamemode.OVERWORLD)
                    {
                        if (Screen.meta_tiles[metatile_index].special)
                        {
                            SaveLoad.SetOverworldSecretsFlag(SaveLoad.current_save_file,
                                (byte)Array.IndexOf(OverworldCode.screens_with_secrets_list, OverworldCode.current_screen), true);
                            Screen.meta_tiles[metatile_index].tile_index = 3;
                            int ppu_index = 2 * metatile_index + 2 * (metatile_index & 0xFFFFFF0) + 256;
                            Textures.ppu[ppu_index] = 0x24;
                            Textures.ppu[ppu_index + 1] = 0x24;
                            Textures.ppu[ppu_index + 32] = 0x24;
                            Textures.ppu[ppu_index + 33] = 0x24;
                        }
                    }
                    else
                    {
                        if (MemoryExtensions.Contains(DungeonCode.door_metatiles, (byte)metatile_index)) // byte[].Contains marche pas
                        {
                            DungeonCode.door_statuses[Array.IndexOf(DungeonCode.door_metatiles, (byte)metatile_index)] = true;
                        }
                    }
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
            counterpart.tile_index = 0x5e;
            counterpart.palette_index = 6;
            counterpart.x = (short)(x + 8);
            counterpart.y = y;
            unload_during_transition = true;
            counterpart.unload_during_transition = true;
            Screen.sprites.Add(this);
            Screen.sprites.Add(counterpart);
        }
        public override void ProjSpecificActions()
        {
            is_from_link = !is_from_link; // fire damages link and ennemies, this makes it so it alternates between calculating dmg for link and enemies every frame

            if (animation_timer < 32 && animation_timer % 2 == 1)
            {
                Move(1);
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
                byte swap = counterpart.tile_index;
                counterpart.tile_index = tile_index;
                tile_index = swap;
                xflip = !xflip;
                counterpart.xflip = !counterpart.xflip;
            }
            counterpart.x = (short)(x + 8);
            counterpart.y = y;
            animation_timer++;
        }
        void BurnBushes()
        {
            int metatile_index;
            for (sbyte i = -8; i < 9; i += 8)
            {
                for (sbyte j = -8; j < 9; j += 8)
                {
                    metatile_index = ((y + i) & 0xFFF0) + ((x + j) >> 4) - 64;

                    if (metatile_index < 0 || metatile_index > Screen.meta_tiles.Length)
                        continue;

                    if (Screen.meta_tiles[metatile_index].special)
                    {
                        SaveLoad.SetOverworldSecretsFlag(SaveLoad.current_save_file,
                            (byte)Array.IndexOf(OverworldCode.screens_with_secrets_list, OverworldCode.current_screen), true);
                        Screen.meta_tiles[metatile_index].tile_index = 0x15;
                        int ppu_index = 2 * metatile_index + 2 * (metatile_index & 0xFFFFFF0) + 256;
                        Textures.ppu[ppu_index] = 0x70;
                        Textures.ppu[ppu_index + 1] = 0x72;
                        Textures.ppu[ppu_index + 32] = 0x71;
                        Textures.ppu[ppu_index + 33] = 0x73;
                    }
                }
            }
        }
    }
    internal class BaitSprite : Sprite
    {
        short existence_timer = 0;
        public BaitSprite(int x, int y) : base(0x22, 6)
        {
            this.x = x;
            this.y = y;
            unload_during_transition = true;
            Screen.sprites.Add(this);
        }
        public override void Action()
        {
            existence_timer++;
            if (existence_timer > 750)
            {
                Menu.bait_out = false;
                Screen.sprites.Remove(this);
                return;
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
                if (SaveLoad.book_of_magic[SaveLoad.current_save_file])
                {
                    ThrownFireSprite fire = new ThrownFireSprite(x, y, direction);
                    fire.animation_timer = 32;
                }
                Screen.sprites.Remove(counterpart);
                Screen.sprites.Remove(this);
                return;
            }

            byte distance = (byte)((Program.gTimer % 2) + 2);
            Move(distance);

            byte new_plt_index = (byte)((Program.gTimer % 4) + 4);
            palette_index = new_plt_index;
            counterpart.palette_index = new_plt_index;
        }
    }
    internal class TornadoSprite : Sprite, ISmokeSpawn
    {
        public int smoke_timer { get; set; } = 0;
        public bool smoke_stage { get; set; } = true;
        bool link_grabbed = false;
        bool second_arrival = true;
        StaticSprite counterpart = new StaticSprite(0x96, 5, 0, 0);
        public TornadoSprite(int x, int y) : base(0x94, 5)
        {
            this.x = x;
            this.y = y;
            //unload_during_transition = true;
            //counterpart.unload_during_transition = true;
            counterpart.y = y;
            counterpart.x = (short)(x + 8);
            shown = false;
            counterpart.shown = false;
            Screen.sprites.Add(this);
            Screen.sprites.Add(counterpart);
        }
        public override void Action()
        {
            if (Link.can_move)
            {
                if (smoke_stage)
                {
                    SetSmokeGraphic();
                }
                else
                {
                    x += 2;
                    counterpart.x += 2;
                    byte new_plt_index = (byte)((Program.gTimer & 3) + 4);
                    palette_index = new_plt_index;
                    counterpart.palette_index = new_plt_index;
                    byte swap = tile_index;
                    tile_index = counterpart.tile_index;
                    counterpart.tile_index = swap;
                    xflip = !xflip;
                    counterpart.xflip = !counterpart.xflip;
                    if (x > 240 && !link_grabbed)
                    {
                        Menu.tornado_out = false;
                        Screen.sprites.Remove(counterpart);
                        Screen.sprites.Remove(this);
                    }
                    else if (Math.Abs(x - Link.x) <= 2 && Math.Abs(y - Link.y) <= 2 && !link_grabbed)
                    {
                        Link.Show(false);
                        link_grabbed = true;
                        Link.SetPos(x, y);
                    }
                    else if (link_grabbed)
                    {
                        Link.SetPos(x, y);
                        if (x >= 112 && second_arrival)
                        {
                            Link.Show(true);
                            Menu.tornado_out = false;
                            Screen.sprites.Remove(counterpart);
                            Screen.sprites.Remove(this);
                            return;
                        }
                        else if (x > 240)
                        {
                            Link.can_move = false;
                            shown = false;
                            counterpart.shown = false;
                            smoke_stage = true;
                            smoke_timer = 0;
                            xflip = false;
                        }
                    }
                }
            }
        }
        public void SetSmokeGraphic()
        {
            if (smoke_timer == 0)
            {
                shown = true;
                counterpart.shown = true;
                tile_index = 0x70;
                counterpart.tile_index = 0x70;
                counterpart.xflip = true;
                second_arrival = !second_arrival;
                if (second_arrival)
                {
                    x = 0;
                    counterpart.x = 8;
                    short new_y = OverworldCode.dungeon_location_list[OverworldCode.recorder_destination * 2 + 1];
                    y = new_y;
                    counterpart.y = new_y;
                }
            }
            else if (smoke_timer == 1)
            {
                tile_index = 0x72;
                counterpart.tile_index = 0x72;
            }
            else if (smoke_timer == 8)
            {
                tile_index = 0x74;
                counterpart.tile_index = 0x74;
            }
            else if (smoke_timer >= 14)
            {
                tile_index = 0x94;
                counterpart.tile_index = 0x96;
                counterpart.xflip = false;
                smoke_stage = false;
            }

            smoke_timer++;
        }
    }

    internal class BoomerangSprite : ProjectileSprite
    {
        byte local_timer = 0;
        byte speed = 3;
        bool returning = false;
        short x_dist_from_link, y_dist_from_link;
        EightDirection m_direction;

        public BoomerangSprite(int x, int y, bool is_from_link) : base(is_from_link, true, 1)
        {
            this.x = x;
            this.y = y;
            if (SaveLoad.magical_boomerang[SaveLoad.current_save_file])
                palette_index = 5;
            unload_during_transition = true;
            m_direction = FindBoomerangDirection();
            Link.animation_timer = 0;
        }

        public override void ProjSpecificActions()
        {
            if (!(tile_index == 0x3c && !returning))
            {
                switch ((local_timer % 16) >> 1)
                {
                    case 0:
                        tile_index = 0x36;
                        if (!SaveLoad.magical_boomerang[SaveLoad.current_save_file])
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

            if (returning)
            {
                // https://www.desmos.com/calculator/esasntj0cm
                x_dist_from_link = (short)((x + 4) - (Link.x + 8));
                y_dist_from_link = (short)((y + 8) - (Link.y + 8));
                float angle = MathF.Atan(x_dist_from_link / (y_dist_from_link + 0.01f)); // +0.01f auto converts y_dist_from_link to float AND prevents div by 0 error
                float x_dist_to_move = MathF.Sin(angle) * speed;
                float y_dist_to_move = MathF.Cos(angle) * speed;
                if (y_dist_from_link >= 0)
                {
                    x_dist_to_move = -x_dist_to_move;
                    y_dist_to_move = -y_dist_to_move;
                }
                x_dist_to_move = MathF.Round(x_dist_to_move, MidpointRounding.AwayFromZero);
                y_dist_to_move = MathF.Round(y_dist_to_move, MidpointRounding.AwayFromZero);

                x += (short)x_dist_to_move;
                y += (short)y_dist_to_move;

                if (Math.Abs(x_dist_from_link) < 8 && Math.Abs(y_dist_from_link) < 8)
                {
                    Link.Attack();
                    Link.using_item = true;
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
                if (m_direction is (EightDirection.UP or EightDirection.LEFT or EightDirection.UPLEFT or EightDirection.DOWNLEFT))
                    x -= speed;
                else
                    x += speed;
            }
            if (m_direction is not (EightDirection.LEFT or EightDirection.RIGHT))
            {
                if (m_direction is (EightDirection.UP or EightDirection.DOWN))
                {
                    y += m_direction == EightDirection.UP ? -speed : speed;
                }
                else if (m_direction is (EightDirection.DOWNLEFT or EightDirection.DOWNRIGHT))
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
                Link.animation_timer += 48;
                return;
            }

            if (CheckIfEdgeHit() || hit_target)
            {
                tile_index = 0x3c;
                palette_index = 5;
            }

            if (palette_index == 5) // faster way of checking if is magical boomerang
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

        EightDirection FindBoomerangDirection()
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
            else if (SaveLoad.silver_arrow[SaveLoad.current_save_file])
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

    internal class LadderSprite : Sprite
    {
        int tile_being_used;
        bool half_off;
        byte og_tile, og_tile_2 = 0xff;

        StaticSprite counterpart = new StaticSprite(0x76, 4, 0, 0, xflip: true);

        public LadderSprite(int metatile_index) : base(0x76, 4)
        {
            x = (metatile_index % 16) * 16;
            y = (metatile_index >> 4) * 16 + 64;
            tile_being_used = metatile_index;

            og_tile = Screen.meta_tiles[tile_being_used].tile_index;
            byte tile_to_use = 0, tile_to_use_2 = 0;

            if (Link.facing_direction == Direction.UP)
            {
                CheckIfHalfOffX();

                if (half_off)
                {
                    if (IsTileWater(og_tile))
                        tile_to_use = 0x32;
                    og_tile_2 = Screen.meta_tiles[tile_being_used + 1].tile_index;
                    if (IsTileWater(og_tile_2))
                        tile_to_use_2 = 0x33;
                    if (!IsTileWater(Screen.meta_tiles[metatile_index - 16].tile_index))
                    {
                        tile_to_use = 0x34;
                        tile_to_use_2 = 0x34;
                    }
                }
                else
                {
                    tile_to_use = 0x2e;
                    if (!IsTileWater(Screen.meta_tiles[metatile_index - 16].tile_index))
                        tile_to_use = 0x34;
                }
            }
            else if (Link.facing_direction == Direction.DOWN)
            {
                CheckIfHalfOffX();

                if (half_off)
                {
                    if (IsTileWater(og_tile))
                        tile_to_use = 0x30;
                    og_tile_2 = Screen.meta_tiles[tile_being_used + 1].tile_index;
                    if (IsTileWater(og_tile_2))
                        tile_to_use_2 = 0x31;
                }
                else
                {
                    tile_to_use = 0x34;
                }
            }
            else
            {
                if ((Link.y + 4) % 16 >= 8)
                {
                    y -= 8;
                    half_off = true;
                }

                if (!IsTileWater(Screen.GetTileIndexAtLocation(x + 8, y - 16)))
                {
                    tile_to_use = 0x34;
                }
                else
                {
                    if (half_off)
                        tile_to_use = 0x2f;
                    else
                        tile_to_use = 0x2e;
                }
            }
            Screen.meta_tiles[tile_being_used].tile_index = tile_to_use;
            if (tile_to_use_2 != 0)
            {
                Screen.meta_tiles[tile_being_used + 1].tile_index = tile_to_use_2;
            }

            counterpart.x = x + 8;
            counterpart.y = y;
            Link.ladder_used = true;
            Screen.sprites.Add(this);
            Screen.sprites.Add(counterpart);

            void CheckIfHalfOffX()
            {
                if ((Link.x + 4) % 16 >= 8)
                {
                    x += 8;
                    half_off = true;
                }

                if (x == Link.x + 16)
                {
                    og_tile = Screen.meta_tiles[--tile_being_used].tile_index;
                    x -= 16;
                }
            }
        }

        public override void Action()
        {
            if (!IsTileWater(Screen.GetTileIndexAtLocation(Link.x, Link.y + 8)) && !IsTileWater(Screen.GetTileIndexAtLocation(Link.x + 15, Link.y + 8)) &&
                !IsTileWater(Screen.GetTileIndexAtLocation(Link.x, Link.y + 15)) && !IsTileWater(Screen.GetTileIndexAtLocation(Link.x + 15, Link.y + 15)))
            {
                //if (Link.x >= x - 16 && Link.x <= x + 16 && Link.y >= y - 16 && Link.y <= y + 16)
                //    return;
                Link.ladder_used = false;
                Screen.meta_tiles[tile_being_used].tile_index = og_tile;
                if (og_tile_2 != 0xff)
                {
                    Screen.meta_tiles[tile_being_used + 1].tile_index = og_tile_2;
                }
                //if (half_above)
                //    Screen.meta_tiles[tile_being_used - 16].tile_index = 0xa;
                Screen.sprites.Remove(counterpart);
                Screen.sprites.Remove(this);
                return;
            }
        }

        bool IsTileWater(int index)
        {
            if (Program.gamemode == Program.Gamemode.OVERWORLD)
            {
                if (index >= 0xa && index <= 0x12)
                    return true;
                else if (index == 0x17 || index == 0x18)
                    return true;
                else if (index >= 0x2e)
                    return true;
                else
                    return false;
            }
            else
            {
                return index == 2 || index >= 0x2e;
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

        public MagicOrbProjectileSprite(int x, int y) : base(false, true, 0.5f)
        {
            this.x = x;
            this.y = y;
            tile_index = 0x44;
            unload_during_transition = true;
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
            int x_dist_from_link = (x + 4) - (Link.x + 8);
            int y_dist_from_link = (y + 8) - (Link.y + 8);

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

    internal class HeartContainerSprite : ItemDropSprite
    {
        byte index;
        StaticSprite counterpart = new StaticSprite(0x68, 6, 0, 0, true);

        public HeartContainerSprite(int x, int y, byte heart_container_index) : base(x, y, false)
        {
            tile_index = 0x68;
            palette_index = 6;
            dbl_wide = true;
            index = heart_container_index;
            counterpart.x = x + 8;
            counterpart.y = y;
            counterpart.unload_during_transition = true;
            Screen.sprites.Add(counterpart);
        }

        public override void ItemSpecificActions()
        {
            if (collected)
            {
                SaveLoad.SetHeartContainerFlag(SaveLoad.current_save_file, index, true);
                SaveLoad.nb_of_hearts[SaveLoad.current_save_file]++;
                Screen.sprites.Remove(counterpart);
                Screen.sprites.Remove(this);
            }
        }
    }

    internal class ClockItemSprite : ItemDropSprite
    {
        StaticSprite counterpart = new StaticSprite(0x66, 6, 0, 0, true);

        public ClockItemSprite(int x, int y) : base(x, y)
        {
            tile_index = 0x66;
            palette_index = 6;
            dbl_wide = true;
            counterpart.x = x + 8;
            counterpart.y = y;
            counterpart.unload_during_transition = true;
            Screen.sprites.Add(counterpart);
        }

        public override void ItemSpecificActions()
        {
            if (collected)
            {
                Link.clock_flash = true;
                Screen.sprites.Remove(counterpart);
                Screen.sprites.Remove(this);
            }
        }
    }

    internal class BombItemSprite : ItemDropSprite
    {
        public BombItemSprite(int x, int y) : base(x, y)
        {
            tile_index = 0x34;
            palette_index = 5;
        }

        public override void ItemSpecificActions()
        {
            if (!collected)
                return;

            // TODO play bomb get sound
            SaveLoad.bomb_count[SaveLoad.current_save_file] += 4;
            if (SaveLoad.bomb_count[SaveLoad.current_save_file] > SaveLoad.bomb_limit[SaveLoad.current_save_file])
                SaveLoad.bomb_count[SaveLoad.current_save_file] = SaveLoad.bomb_limit[SaveLoad.current_save_file];
            Screen.sprites.Remove(this);
        }
    }
}