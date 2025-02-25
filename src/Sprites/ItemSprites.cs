using The_Legend_of_Zelda.Gameplay;
using The_Legend_of_Zelda.Rendering;
using static The_Legend_of_Zelda.Gameplay.Program;

namespace The_Legend_of_Zelda.Sprites
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


    public abstract class ItemDropSprite : Sprite
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
            if (CollidingWithLink() || CollidingWithSword() || CollidingWithBoomerang())
                collected = true;

            ItemSpecificActions();
        }

        protected abstract void ItemSpecificActions();

        bool CollidingWithLink()
        {
            return x < Link.x + 16 &&
                x + (dbl_wide ? 16 : 8) > Link.x &&
                y < Link.y + 16 &&
                y + 16 > Link.y;
        }

        bool CollidingWithSword()
        {
            if (!Link.sword_out)
                return false;

            bool second_sword_spr_col = false;

            if (Link.facing_direction is (Direction.LEFT or Direction.RIGHT))
            {
                second_sword_spr_col = x < Link.sword_2.x + 8 &&
                x + (dbl_wide ? 16 : 8) > Link.sword_2.x &&
                y < Link.sword_2.y + 12 &&
                y + 12 > Link.sword_2.y;
            }

            return x < Link.sword_1.x + 8 &&
                x + (dbl_wide ? 16 : 8) > Link.sword_1.x &&
                y < Link.sword_1.y + 12 &&
                y + 20 > Link.sword_1.y || second_sword_spr_col;
        }

        bool CollidingWithBoomerang()
        {
            if (!Menu.boomerang_out)
                return false;

            foreach (Sprite s in Screen.sprites)
            {
                if (s is not BoomerangSprite b)
                    continue;

                return x < b.x + 8 &&
                x + (dbl_wide ? 16 : 8) > b.x &&
                y < b.y + 12 &&
                y + 20 > b.y;
            }

            return false;
        }

        bool Dissapear()
        {
            return local_timer > 2000;
        }
    }

    internal class DungeonGiftSprite : Sprite
    {
        ItemDropSprite child;
        Enemy? parent = null;
        bool child_active = true;

        public DungeonGiftSprite(ItemDropSprite child, bool on_room_clear, bool attached_to_enemy) : base(0, 0)
        {
            shown = false;
            this.child = child;
            unload_during_transition = true;
            Screen.sprites.Add(this);

            if (on_room_clear && !attached_to_enemy)
            {
                Screen.sprites.Remove(child);
                child_active = false;
            }

            if (attached_to_enemy)
            {
                foreach (Sprite s in Screen.sprites)
                {
                    if (s is not Enemy e)
                        continue;

                    parent = e;
                    break;
                }
            }
        }

        public override void Action()
        {
            if (child_active)
            {
                if (child.collected)
                {
                    SaveLoad.SetDungeonGiftFlag(Program.DC.current_screen, true);
                    Screen.sprites.Remove(this);
                }

                if (parent is not null)
                {
                    child.x = parent.x + 4;
                    child.y = parent.y;
                }

                return;
            }

            if (Program.DC.nb_enemies_alive == 0)
            {
                Screen.sprites.Add(child);
                child_active = true;
            }
        }
    }


    internal class StaticHeartSprite : FlickeringSprite
    {
        public StaticHeartSprite(int x, int y) : base(0xf2, 6, x, y, 8, 0xf2, second_palette_index: 5)
        {
            use_chr_rom = true;
            UpdateTexture();
        }
    }

    internal class HeartItemSprite : ItemDropSprite
    {
        public HeartItemSprite(int x, int y) : base(x, y)
        {
            tile_index = 0xf2;
            use_chr_rom = true;
            UpdateTexture();
            palette_index = 5;
        }

        protected override void ItemSpecificActions()
        {
            if (gTimer % 8 == 0)
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

        protected override void ItemSpecificActions()
        {
            if (collected)
            {
                Link.AddRupees(five_rupies ? 5 : 1);
                Screen.sprites.Remove(this);
            }

            if (gTimer % 8 == 0 && !five_rupies)
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

        protected override void ItemSpecificActions()
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
                direction = (EightDirection)RNG.Next(8);
                when_to_stop = RNG.Next(40, 90);
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

    internal class TornadoSprite : Sprite
    {
        int smoke_timer = 0;
        bool smoke_stage = true;

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
            counterpart.x = x + 8;
            shown = false;
            counterpart.shown = false;
            Screen.sprites.Add(this);
            Screen.sprites.Add(counterpart);
        }

        public override void Action()
        {
            if (!Link.can_move)
                return;

            if (smoke_stage)
            {
                SetSmokeGraphic();
                return;
            }

            x += 2;
            counterpart.x += 2;

            byte new_plt_index = (byte)(gTimer % 4 + 4);
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

        void SetSmokeGraphic()
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
                    short new_y = OC.dungeon_location_list[OC.recorder_destination * 2 + 1];
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

    internal class LadderSprite : Sprite
    {
        const int INVALID_TILE = 0xff;

        int tile_being_used;
        bool half_off;
        MetatileType og_tile, og_tile_2 = (MetatileType)INVALID_TILE;

        StaticSprite counterpart = new StaticSprite(0x76, 4, 0, 0, xflip: true);

        public LadderSprite(int metatile_index) : base(0x76, 4)
        {
            x = metatile_index % 16 * 16;
            y = (metatile_index >> 4) * 16 + 64;
            tile_being_used = metatile_index;

            og_tile = Screen.meta_tiles[tile_being_used].tile_index;
            MetatileType tile_to_use = 0, tile_to_use_2 = 0;

            if (Link.facing_direction == Direction.UP)
            {
                CheckIfHalfOffX();

                if (half_off)
                {
                    if (IsTileWater(og_tile))
                        tile_to_use = MetatileType.LADDER_TL;
                    og_tile_2 = Screen.meta_tiles[tile_being_used + 1].tile_index;
                    if (IsTileWater(og_tile_2))
                        tile_to_use_2 = MetatileType.LADDER_TR;
                    if (!IsTileWater(Screen.meta_tiles[metatile_index - 16].tile_index))
                    {
                        tile_to_use = MetatileType.LADDER_EMPTY;
                        tile_to_use_2 = MetatileType.LADDER_EMPTY;
                    }
                }
                else
                {
                    tile_to_use = MetatileType.LADDER_TOP;
                    if (!IsTileWater(Screen.meta_tiles[metatile_index - 16].tile_index))
                        tile_to_use = MetatileType.LADDER_EMPTY;
                }
            }
            else if (Link.facing_direction == Direction.DOWN)
            {
                CheckIfHalfOffX();

                if (half_off)
                {
                    if (IsTileWater(og_tile))
                        tile_to_use = MetatileType.LADDER_LEFT;
                    og_tile_2 = Screen.meta_tiles[tile_being_used + 1].tile_index;
                    if (IsTileWater(og_tile_2))
                        tile_to_use_2 = MetatileType.LADDER_RIGHT;
                }
                else
                {
                    tile_to_use = MetatileType.LADDER_EMPTY;
                }
            }
            else
            {
                if ((Link.y + 4) % 16 >= 8)
                {
                    y -= 8;
                    half_off = true;
                }

                if (!IsTileWater(Screen.GetMetaTileTypeAtLocation(x + 8, y - 16)))
                {
                    tile_to_use = MetatileType.LADDER_EMPTY;
                }
                else
                {
                    if (half_off)
                        tile_to_use = MetatileType.LADDER_BOTTOM;
                    else
                        tile_to_use = MetatileType.LADDER_TOP;
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
            if (!IsTileWater(Screen.GetMetaTileTypeAtLocation(Link.x, Link.y + 8)) && !IsTileWater(Screen.GetMetaTileTypeAtLocation(Link.x + 15, Link.y + 8)) &&
                !IsTileWater(Screen.GetMetaTileTypeAtLocation(Link.x, Link.y + 15)) && !IsTileWater(Screen.GetMetaTileTypeAtLocation(Link.x + 15, Link.y + 15)))
            {
                //if (Link.x >= x - 16 && Link.x <= x + 16 && Link.y >= y - 16 && Link.y <= y + 16)
                //    return;
                Link.ladder_used = false;
                Screen.meta_tiles[tile_being_used].tile_index = og_tile;
                if (og_tile_2 != (MetatileType)INVALID_TILE)
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

        bool IsTileWater(MetatileType index)
        {
            if (gamemode == Gamemode.OVERWORLD)
            {
                if (index >= MetatileType.WATER && index <= MetatileType.WATER_BL)
                    return true;
                else if (index == MetatileType.WATERFALL || index == MetatileType.WATERFALL_BOTTOM)
                    return true;
                else if (index >= MetatileType.LADDER_TOP)
                    return true;
                else
                    return false;
            }
            else
            {
                return index == (MetatileType)DungeonMetatile.WATER || index >= MetatileType.LADDER_TOP;
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

        protected override void ItemSpecificActions()
        {
            if (collected)
            {
                SaveLoad.SetHeartContainerFlag(index, true);
                SaveLoad.nb_of_hearts++;
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

        protected override void ItemSpecificActions()
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
        public BombItemSprite(int x, int y, bool despawn = true) : base(x, y, despawn)
        {
            tile_index = 0x34;
            palette_index = 5;
        }

        protected override void ItemSpecificActions()
        {
            if (!collected)
                return;

            //TODO: play bomb get sound
            SaveLoad.bomb_count += 4;
            if (SaveLoad.bomb_count > SaveLoad.bomb_limit)
                SaveLoad.bomb_count = SaveLoad.bomb_limit;
            Screen.sprites.Remove(this);
        }
    }
}