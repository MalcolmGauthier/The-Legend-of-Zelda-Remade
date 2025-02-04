using System.Buffers.Text;
using The_Legend_of_Zelda.Gameplay;
using The_Legend_of_Zelda.Rendering;

namespace The_Legend_of_Zelda.Sprites
{
    internal class Stalfos : Enemy
    {
        public Stalfos() : base(AnimationMode.TWOFRAMES_S, 0xa8, 0xa8, false, true, 8, 0.5f, 3)
        {
            HP = 2;
            damage = 0.5f;
            facing_direction = (Direction)Program.RNG.Next(4);
            CheckIfTurn();
            current_action = ActionState.WALKING;
            palette_index = (byte)PaletteID.SP_2;
            counterpart.palette_index = palette_index;
        }

        protected override void EnemySpecificActions()
        {
            Walk();
        }
    }

    internal class Keese : Enemy
    {
        // most code copied from peahat. the enemies are quite similar.
        int when_to_stop;
        int num_times_turned;
        EightDirection direction;

        public Keese(bool stronger, Enemy? parent = null) : base(AnimationMode.TWOFRAMES_M, 0x9a, 0x9c, stronger, true, 6, 0.75f, 0, true)
        {
            current_action = ActionState.FLYING;
            palette_index = (byte)PaletteID.SP_1;
            if (stronger)
            {
                palette_index = (byte)PaletteID.SP_2;
            }
            counterpart.palette_index = palette_index;
            HP = 1;
            damage = 0.5f;
            speed = 0.5f;
            direction = (EightDirection)Program.RNG.Next(8);
            die_when_stunned = true;

            if (parent is not null)
            {
                x = parent.x;
                y = parent.y;
            }
            else
            {
                FindValidSpawnLocation();
            }
        }

        protected override void EnemySpecificActions()
        {
            switch (current_action)
            {
                case ActionState.RESTING:
                    if (local_timer == 1)
                    {
                        frames_between_anim = 12;
                    }
                    else if (local_timer == 24)
                    {
                        frames_between_anim = 24;
                    }
                    if (local_timer <= 48)
                    {
                        if (local_timer <= 24 && local_timer % 4 == 0 || local_timer > 24 && local_timer % 8 == 0)
                        {
                            Move8D();
                            CheckBounds();
                        }
                        break;
                    }

                    if (local_timer == 49)
                    {
                        frames_between_anim = 0;
                        when_to_stop = Program.RNG.Next(60, 120);
                    }
                    else if (local_timer > when_to_stop + 49)
                    {
                        local_timer = 0;
                        num_times_turned = 0;
                        current_action = ActionState.FLYING;
                    }
                    break;

                case ActionState.FLYING:
                    if (local_timer == 1)
                    {
                        frames_between_anim = 24;
                    }
                    else if (local_timer == 24)
                    {
                        frames_between_anim = 12;
                    }
                    else if (local_timer == 55)
                    {
                        direction = PickNewDirection();
                        when_to_stop = Program.RNG.Next(15, 60);
                        frames_between_anim = 6;
                    }

                    if (local_timer <= 54)
                    {
                        if (local_timer % frames_between_anim == 0)
                        {
                            Move8D();
                            CheckBounds();
                        }

                        break;
                    }

                    if (local_timer > 56 + when_to_stop)
                    {
                        num_times_turned++;
                        if (num_times_turned < 5 || Program.RNG.Next(8) != 0)
                        {
                            local_timer = 54;
                        }
                        else
                        {
                            local_timer = 0;
                            num_times_turned = 0;
                            current_action = ActionState.RESTING;
                            return;
                        }
                    }

                    if (local_timer % 4 != 0)
                    {
                        Move8D();
                        CheckBounds();
                    }

                    break;
            }
        }

        void Move8D()
        {
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

        // either turns left, turns right or continues forward
        EightDirection PickNewDirection()
        {
            EightDirection return_val = direction;
            int rng = Program.RNG.Next(3);

            if (rng == 2)
                return return_val;

            if (direction == EightDirection.UP)
                return rng == 0 ? EightDirection.UPLEFT : EightDirection.UPRIGHT;
            if (direction == EightDirection.DOWN)
                return rng == 0 ? EightDirection.DOWNLEFT : EightDirection.DOWNRIGHT;
            if (direction == EightDirection.LEFT)
                return rng == 0 ? EightDirection.UPLEFT : EightDirection.DOWNLEFT;
            if (direction == EightDirection.RIGHT)
                return rng == 0 ? EightDirection.UPRIGHT : EightDirection.DOWNRIGHT;
            if (direction == EightDirection.UPLEFT)
                return rng == 0 ? EightDirection.UP : EightDirection.LEFT;
            if (direction == EightDirection.DOWNLEFT)
                return rng == 0 ? EightDirection.DOWN : EightDirection.LEFT;
            if (direction == EightDirection.UPRIGHT)
                return rng == 0 ? EightDirection.UP : EightDirection.RIGHT;
            else
                return rng == 0 ? EightDirection.RIGHT : EightDirection.DOWN;
        }

        void CheckBounds()
        {
            if (x <= 32 || x >= 208 || y <= 96 || y >= 192)
            {
                // the math works out.
                if (direction < (EightDirection)4)
                    direction ^= (EightDirection)1;
                else
                    direction ^= (EightDirection)0b11;

                Move8D();
            }
        }
    }

    internal class Gel : Enemy
    {
        int rest_timer = 0;
        // the Enemy class doesn't expect enemies being 1 sprite wide, so we puppet our own visual sprite. the real enemy is invisible
        public StaticSprite puppet;

        public Gel(Zol? parent) : base(AnimationMode.TWOFRAMES_SOLO, 0x92, 0x94, false, true, 2, 1f, 0, true)
        {
            puppet = new(0x92, (byte)PaletteID.SP_3, x, y);
            puppet.unload_during_transition = true;
            puppet.shown = false;
            Screen.sprites.Add(puppet);
            HP = 1;
            damage = 0.5f;
            facing_direction = (Direction)Program.RNG.Next(4);
            CheckIfTurn();
            current_action = ActionState.WALKING;
            palette_index = (byte)PaletteID.SP_3;
            puppet.palette_index = palette_index;
            die_when_stunned = true;

            if (parent is not null)
            {
                x = parent.x;
                y = parent.y;
            }
            else
            {
                FindValidSpawnLocation();
            }
        }

        protected override void EnemySpecificActions()
        {
            if (current_action == ActionState.RESTING)
            {
                if (local_timer >= rest_timer)
                {
                    current_action = ActionState.WALKING;
                    local_timer = 0;
                }
            }
            else if (current_action == ActionState.WALKING)
            {
                if (x % 16 == 0 && y % 16 == 0 && local_timer > 8)
                {
                    local_timer = 0;
                    rest_timer = Program.RNG.Next(60);
                    current_action = ActionState.RESTING;
                    return;
                }

                Walk();
            }

            puppet.shown = true;
            puppet.palette_index = palette_index;
            puppet.tile_index = tile_index;
            puppet.x = x + 4;
            puppet.y = y;
        }

        protected override void OnInit()
        {
            shown = false;
            counterpart.shown = false;
        }

        protected override void OnDeath()
        {
            Screen.sprites.Remove(puppet);
            shown = true;
            counterpart.shown = true;
        }
    }

    internal class Zol : Enemy
    {
        int rest_timer = 0;

        public Zol() : base(AnimationMode.TWOFRAMES_M, 0xa8, 0xaa, false, true, 9, 0.5f, 3)
        {
            HP = 1;
            damage = 0.5f;
            facing_direction = (Direction)Program.RNG.Next(4);
            CheckIfTurn();
            current_action = ActionState.WALKING;
            palette_index = (byte)PaletteID.SP_3;
            counterpart.palette_index = palette_index;
        }

        protected override void EnemySpecificActions()
        {
            if (current_action == ActionState.RESTING)
            {
                if (local_timer >= rest_timer)
                {
                    current_action = ActionState.WALKING;
                    local_timer = 0;
                }
            }
            else if (current_action == ActionState.WALKING)
            {
                if (x % 16 == 0 && y % 16 == 0 && local_timer > 8)
                {
                    local_timer = 0;
                    rest_timer = Program.RNG.Next(30, 60);
                    current_action = ActionState.RESTING;
                    return;
                }

                Walk();
            }
        }

        protected override void OnDeath()
        {
            // if the zol not hit by white/magic sword, it creates children
            if (hit_cause == typeof(SwordProjectileSprite) && (SaveLoad.white_sword || SaveLoad.magical_sword))
                return;

            drop_category = 0;
            new Gel(this);
            new Gel(this);
            Program.DC.nb_enemies_alive += 2;
            // we need to remove the sprite immediately, so we manually handle the removal
            Program.DC.nb_enemies_alive--;
            Screen.sprites.Remove(counterpart);
            Screen.sprites.Remove(this);
        }
    }

    internal class Goriya : Enemy, IBoomerangThrower
    {
        int when_to_shoot;
        public bool boomerang_out { get; set; }
        public Direction boomerang_throw_dir { get => facing_direction; set => facing_direction = value; }

        public Goriya(bool stronger) : base(AnimationMode.TWOFRAMES_DURR, 0xb0, 0xb8, stronger, true, 6, 0.5f, 2)
        {
            HP = 3;
            damage = 0.5f;
            facing_direction = (Direction)Program.RNG.Next(4);
            CheckIfTurn();
            current_action = ActionState.WALKING;
            palette_index = (byte)PaletteID.SP_2;
            this.stronger = stronger;
            if (stronger)
            {
                drop_category = 4;
                palette_index = (byte)PaletteID.SP_1;
            }
            counterpart.palette_index = palette_index;
        }

        protected override void EnemySpecificActions()
        {
            switch (current_action)
            {
                case ActionState.SHOOTING_PROJECTILE:
                    if (local_timer == 36)
                    {
                        new BoomerangSprite(x + 4, y, false, stronger, this);
                    }
                    break;
                case ActionState.WALKING:
                    when_to_shoot--;
                    if (local_timer >= when_to_shoot)
                    {
                        current_action = ActionState.SHOOTING_PROJECTILE;
                        local_timer = 0;
                    }
                    else
                    {
                        Walk();
                    }
                    break;
            }
        }

        public void BoomerangRetreive()
        {
            current_action = ActionState.WALKING;
            local_timer = 0;
            when_to_shoot = Program.RNG.Next(180, 600);
        }
    }

    internal class Wallmaster : Enemy
    {
        static int wallmaster_timer = 120;
        static int wallmaster_timer_lock;
        Direction current_wall;
        Direction towards_link;

        bool caught_link = false;
        Direction link_caught_direction;

        bool graphics_flipped = false;

        public Wallmaster() : base(AnimationMode.TWOFRAMES_HM, 0xac, 0x9e, false, false, 8, 0.5f, 3, true)
        {
            current_action = ActionState.DEFAULT;
            palette_index = (byte)PaletteID.SP_1;
            counterpart.palette_index = palette_index;
            dungeon_wall_mask = true;
            counterpart.dungeon_wall_mask = true;
            damage = 0;
            HP = 2;
            wallmaster_timer_lock = Program.gTimer;
            can_damage_link = false;
        }

        protected override void EnemySpecificActions()
        {
            // lock system to make sure wallmaster timer only increments once per frame
            if (wallmaster_timer_lock != Program.gTimer)
            {
                wallmaster_timer_lock = Program.gTimer;
                wallmaster_timer++;
            }

            switch (current_action)
            {
                case ActionState.DEFAULT:
                    if ((Program.Link.x is (32 or 208) || Program.Link.y is (96 or 192)) && wallmaster_timer >= 120)
                    {
                        current_action = ActionState.RISING;
                        local_timer = 0;
                        wallmaster_timer = 0;
                        SetCurrentWall();
                        facing_direction = current_wall.Opposite();
                        FindSpawnLocation();
                        SetGraphics();
                    }
                    break;

                case ActionState.RISING:
                    if (local_timer % 2 == 0)
                        Move1px();

                    if (local_timer >= 64)
                    {
                        current_action = ActionState.WALKING;
                        local_timer = 0;
                        facing_direction = towards_link;
                    }
                    break;

                case ActionState.WALKING:
                    if (local_timer % 2 == 0)
                        Move1px();

                    if (Math.Abs(x - Program.Link.x) <= 8 && Math.Abs(y - Program.Link.y) <= 8 && !caught_link)
                    {
                        caught_link = true;
                        Program.Link.SetPos(x, y);
                        Program.Link.iframes_timer = 255;
                        link_caught_direction = Program.Link.facing_direction;
                        Program.Link.using_item = true;
                        // make hand appear on top of link. usually we always want link to have priority, but this time is different
                        // the hands dissapear after the dungeon restarts, so this doesn't need to be undone manually
                        Screen.sprites.Remove(this);
                        Screen.sprites.Remove(counterpart);
                        Screen.sprites.Insert(0, this);
                        Screen.sprites.Insert(0, counterpart);
                    }

                    if (local_timer >= 96)
                    {
                        facing_direction = current_wall;
                        local_timer = 0;
                        current_action = ActionState.BURROWING;
                    }
                    break;

                case ActionState.BURROWING:
                    if (local_timer % 2 == 0)
                        Move1px();

                    if (local_timer >= 64)
                    {
                        current_action = ActionState.DEFAULT;

                        if (caught_link)
                        {
                            Program.Link.using_item = false;
                            Textures.LoadPPUPage(Textures.PPUDataGroup.OTHER, Textures.OtherPPUPages.EMPTY, 0);
                            Program.DC.Init(Program.DC.current_dungeon);
                            return;
                        }
                    }
                    break;
            }

            if (caught_link)
            {
                //Program.Link.animation_timer++;
                Program.Link.facing_direction = link_caught_direction;
                Program.Link.SetPos(x, y);
                Program.Link.iframes_timer = 255;
            }
        }

        void Move1px()
        {
            switch (facing_direction)
            {
                case Direction.UP:
                    y--;
                    break;
                case Direction.DOWN:
                    y++;
                    break;
                case Direction.LEFT:
                    x--;
                    break;
                case Direction.RIGHT:
                    x++;
                    break;
            }
        }

        void SetCurrentWall()
        {
            if (Program.Link.x == 32)
                current_wall = Direction.LEFT;
            else if (Program.Link.x == 208)
                current_wall = Direction.RIGHT;
            else if (Program.Link.y == 96)
                current_wall = Direction.UP;
            else if (Program.Link.y == 192)
                current_wall = Direction.DOWN;
        }

        void FindSpawnLocation()
        {
            switch (current_wall)
            {
                case Direction.UP:
                case Direction.DOWN:
                    if (Program.Link.facing_direction == Direction.LEFT)
                    {
                        x = Program.Link.x - 48;
                        towards_link = Direction.RIGHT;
                    }
                    else
                    {
                        x = Program.Link.x + 48;
                        towards_link = Direction.LEFT;
                    }

                    if (current_wall == Direction.UP)
                        y = Program.Link.y - 32;
                    else
                        y = Program.Link.y + 32;
                    break;

                case Direction.LEFT:
                case Direction.RIGHT:
                    if (Program.Link.facing_direction == Direction.UP)
                    {
                        y = Program.Link.y - 48;
                        towards_link = Direction.DOWN;
                    }
                    else
                    {
                        y = Program.Link.y + 48;
                        towards_link = Direction.UP;
                    }

                    if (current_wall == Direction.LEFT)
                        x = Program.Link.x - 32;
                    else
                        x = Program.Link.x + 32;
                    break;
            }
        }

        void SetGraphics()
        {
            bool do_yflip = current_wall == Direction.UP || towards_link == Direction.DOWN;
            bool do_xflip = current_wall == Direction.RIGHT || towards_link == Direction.LEFT;

            yflip = do_yflip;
            counterpart.yflip = yflip;

            xflip = do_xflip;
            if (graphics_flipped != do_xflip)
            {
                (tile_location_1, tile_location_2) = (tile_location_2, tile_location_1);
                graphics_flipped = !graphics_flipped;
            }
            counterpart.xflip = xflip;
        }
    }

    internal static class BladeTrapManager
    {
        private class BladeTrapSprite : Enemy
        {
            readonly Direction h_direction;
            readonly Direction v_direction;

            readonly int spawn_x, spawn_y;
            int move_timer = 0;

            public BladeTrapSprite(int x, int y) : base(AnimationMode.ONEFRAME_M, 0x96, 0x96, false, false, 0, 1.75f, 0, true)
            {
                this.x = x;
                this.y = y;
                spawn_x = x;
                spawn_y = y;
                invincible = true;
                HP = float.PositiveInfinity;
                damage = 1f;
                palette_index = (byte)PaletteID.SP_1;
                counterpart.palette_index = palette_index;
                current_action = ActionState.DEFAULT;

                h_direction = x < 128 ? Direction.RIGHT : Direction.LEFT;
                v_direction = y < 144 ? Direction.DOWN : Direction.UP;
            }

            protected override void EnemySpecificActions()
            {
                if (current_action == ActionState.DEFAULT)
                {
                    if (Math.Abs(Program.Link.x - x) < 14)
                    {
                        current_action = ActionState.WALKING;
                        facing_direction = v_direction;
                    }
                    else if (Math.Abs(Program.Link.y - y) < 14)
                    {
                        current_action = ActionState.WALKING;
                        facing_direction = h_direction;
                    }
                }
                else if (current_action == ActionState.WALKING)
                {
                    if (facing_direction == h_direction || facing_direction == v_direction)
                    {
                        // 1.75f spd
                        Move1px();
                        if (local_timer % 4 != 0)
                            Move1px();

                        move_timer++;
                        if (facing_direction == v_direction && move_timer >= 25 ||
                            facing_direction == h_direction && move_timer >= 49)
                        {
                            facing_direction = facing_direction.Opposite();
                            move_timer = 0;
                        }
                    }
                    else
                    {
                        // 0.5f spd
                        if (local_timer % 2 == 0)
                            Move1px();

                        if (x == spawn_x && y == spawn_y)
                        {
                            current_action = ActionState.DEFAULT;
                        }
                    }
                }
            }

            private void Move1px()
            {
                switch (facing_direction)
                {
                    case Direction.UP:
                        y--;
                        break;
                    case Direction.DOWN:
                        y++;
                        break;
                    case Direction.LEFT:
                        x--;
                        break;
                    case Direction.RIGHT:
                        x++;
                        break;
                }
            }
        }

        readonly static (byte x, byte y)[] sprite_positions =
        {
            (32, 96),
            (208, 96),
            (32, 192),
            (208, 192),
            (64, 140),
            (176, 140),
        };

        static BladeTrapSprite[] blades = new BladeTrapSprite[6];

        public static void CreateTraps()
        {
            // don't spawn blade traps if they are still active!!
            foreach (Sprite s in Screen.sprites)
                if (s is BladeTrapSprite) return;

            int num_of_sprites;
            if (Program.DC.room_list[Program.DC.current_screen] == (byte)DungeonCode.RoomType.SPIKE_TRAP)
                num_of_sprites = 6;
            else
                num_of_sprites = 4;

            for (int i = 0; i < num_of_sprites; i++)
            {
                blades[i] = new(sprite_positions[i].x, sprite_positions[i].y);
            }
        }
    }

    internal class Rope : Enemy
    {
        Direction flying_direction;

        public Rope(bool stronger) : base(AnimationMode.TWOFRAMES_LR, 0xa0, 0xa4, stronger, true, 10, 0.5f, 3)
        {
            HP = 1;
            damage = 0.5f;
            facing_direction = (Direction)Program.RNG.Next(4);
            CheckIfTurn();
            current_action = ActionState.WALKING;
            palette_index = (byte)PaletteID.SP_2;
            if (stronger)
            {
                drop_category = 4;
                palette_index = (byte)PaletteID.SP_1;
            }
            counterpart.palette_index = palette_index;
        }

        protected override void EnemySpecificActions()
        {
            switch (current_action)
            {
                case ActionState.WALKING:
                    Walk();

                    // !!! not checked by CheckForLink();
                    if (x % 16 != 0 || y % 16 != 0)
                        break;

                    Direction? direction_to_link = CheckForLink();
                    if (direction_to_link is not null)
                    {
                        current_action = ActionState.FLYING;
                        facing_direction = direction_to_link.Value;
                        flying_direction = facing_direction;
                        no_random_turn_flag = true;
                        speed = 1.5f;
                    }
                    break;

                case ActionState.FLYING:
                    Walk();

                    // when Walk() encounters a wall, it will turn the enemy. that's when to stop.
                    if (facing_direction != flying_direction)
                    {
                        current_action = ActionState.WALKING;
                        no_random_turn_flag = false;
                        speed = 0.5f;
                    }
                    break;
            }
        }

        Direction? CheckForLink()
        {
            if (Program.Link.x == x)
                return Program.Link.y > y ? Direction.DOWN : Direction.UP;
            else if (Program.Link.y == y)
                return Program.Link.x > x ? Direction.RIGHT : Direction.LEFT;
            else
                return null;
        }
    }

    internal class Statues : Sprite
    {
        readonly (byte x, byte y)[] orb_starting_positions;

        int[] fireball_timers;

        public Statues() : base(0x1c, 0)
        {
            unload_during_transition = true;
            shown = false;

            if (Program.DC.room_list[Program.DC.current_screen] == (byte)DungeonCode.RoomType.STATUE_DUO)
            {
                fireball_timers = new int[2];
                orb_starting_positions = new (byte x, byte y)[2] { (104, 148), (140, 140) };
            }
            else
            {
                fireball_timers = new int[4];
                orb_starting_positions = new (byte x, byte y)[4] { (204, 92), (40, 100), (40, 192), (204, 188) };
            }

            Screen.sprites.Add(this);
        }

        public override void Action()
        {
            for (int i = 0; i < fireball_timers.Length; i++)
            {
                fireball_timers[i]--;

                if (fireball_timers[i] <= 0)
                {
                    new MagicOrbProjectileSprite(orb_starting_positions[i].x, orb_starting_positions[i].y);
                    fireball_timers[i] = Program.RNG.Next(90, 240);
                }
            }
        }
    }

    internal class Darknut : Enemy
    {
        public Darknut(bool stronger) : base(AnimationMode.TWOFRAMES_DDURR, 0xac, 0xb8, stronger, true, 8, 0.5f, 2)
        {
            palette_index = (byte)PaletteID.SP_2;
            HP = 4;
            damage = 1;
            if (stronger)
            {
                drop_category = 4;
                palette_index = (byte)PaletteID.SP_1;
                HP = 8;
                damage = 2;
            }
            counterpart.palette_index = palette_index;
            facing_direction = (Direction)Program.RNG.Next(4);
            CheckIfTurn();
            current_action = ActionState.WALKING;
        }

        // parrying is implemented in base class because if how unique it is
        protected override void EnemySpecificActions()
        {
            Walk();
        }

        //TODO: add turning 180 deg?
    }

    internal class Bubble : Enemy
    {
        public enum BubbleType
        {
            NORMAL,
            BLUE,
            RED
        }

        BubbleType type;
        static int bubble_timer = 0;
        static int bubble_timer_lock;
        // 2nd quest only
        public static bool perma_sword_loss = false;

        public Bubble(BubbleType type) : base(AnimationMode.ONEFRAME, 0x8e, 0x8e, false, true, 0, 1f, 0)
        {
            this.type = type;
            invincible = true;
            HP = float.MaxValue;
            damage = 0;
            bubble_timer = 0;
            bubble_timer_lock = Program.gTimer;
        }

        protected override void EnemySpecificActions()
        {
            Walk();

            if (bubble_timer_lock != Program.gTimer)
            {
                bubble_timer--;
                bubble_timer_lock = Program.gTimer;
            }

            if (bubble_timer <= 0 && !perma_sword_loss)
            {
                Program.Link.can_use_sword = true;
            }

            palette_index = (byte)((++palette_index) % 4 + 4);
            counterpart.palette_index = palette_index;
        }

        protected override void OnDamageLink()
        {
            switch (type)
            {
                case BubbleType.NORMAL:
                    Program.Link.can_use_sword = false;
                    bubble_timer = 150;
                    break;
                case BubbleType.BLUE:
                    Program.Link.can_use_sword = true;
                    perma_sword_loss = false;
                    break;
                case BubbleType.RED:
                    Program.Link.can_use_sword = false;
                    perma_sword_loss = true;
                    break;
            }
        }
    }

    internal class Vire : Enemy
    {
        int base_y;

        public Vire() : base(AnimationMode.TWOFRAMES_DDUU_M, 0xac, 0xb0, false, true, 10, 0.5f, 2)
        {
            palette_index = (byte)PaletteID.SP_1;
            HP = 1;
            damage = 1;
            counterpart.palette_index = palette_index;
            facing_direction = (Direction)Program.RNG.Next(4);
            CheckIfTurn();
            current_action = ActionState.WALKING;
        }

        protected override void EnemySpecificActions()
        {
            switch (current_action)
            {
                case ActionState.WALKING:
                    Walk();
                    break;

                case ActionState.JUMPING:
                    Walk();
                    switch (local_timer)
                    {
                        case 1 or 2:
                            y -= 3;
                            break;
                        case 3 or 4:
                            y -= 2;
                            break;
                        case 5 or 6 or 7 or 8 or 10 or 12:
                            y -= 1;
                            break;

                        case 20 or 22 or 24 or 25 or 26 or 27:
                            y += 1;
                            break;
                        case 28 or 29:
                            y += 2;
                            break;
                        case 30:
                            y += 3;
                            break;
                        case 31:
                            y += 3;
                            y = base_y;
                            break;
                    }
                    break;
            }
        }

        protected override bool OnTileMoved()
        {
            CheckIfTurn();

            if (facing_direction is (Direction.LEFT or Direction.RIGHT))
            {
                current_action = ActionState.JUMPING;
                local_timer = 0;
                base_y = y;
            }
            else
            {
                current_action = ActionState.WALKING;
            }

            return current_action == ActionState.WALKING;
        }

        protected override void OnDeath()
        {
            // if the vire not hit with magic sword, it creates children
            if (hit_cause == typeof(SwordProjectileSprite) && SaveLoad.magical_sword)
                return;

            drop_category = 0;
            new Keese(true, this);
            new Keese(true, this);
            Program.DC.nb_enemies_alive += 2;
            // we need to remove the sprite immediately, so we manually handle the removal
            Program.DC.nb_enemies_alive--;
            Screen.sprites.Remove(counterpart);
            Screen.sprites.Remove(this);
        }
    }

    internal class LikeLike : Enemy
    {
        // notify other LikeLikes that link has been taken
        static bool link_is_caught = false;

        // you have 96 frames to kill the likelike to preserve your magic shield
        int shield_loss_timer = 96;
        int capture_x, capture_y;

        public LikeLike() : base(AnimationMode.THREEFRAMES_M, 0xa2, 0xa4, false, true, 8, 0.5f, 0)
        {
            damage = 2;
            HP = 10;
            facing_direction = (Direction)Program.RNG.Next(4);
            CheckIfTurn();
            current_action = ActionState.WALKING;
            palette_index = (byte)PaletteID.SP_2;
            counterpart.palette_index = palette_index;
            can_damage_link = false;
        }

        protected override void EnemySpecificActions()
        {
            switch (current_action)
            {
                case ActionState.WALKING:
                    Walk();

                    if (Math.Abs(Program.Link.x - x) <= 8 && Math.Abs(Program.Link.y - y) <= 8 && !link_is_caught)
                    {
                        link_is_caught = true;
                        current_action = ActionState.BURROWING;
                        x = Program.Link.x;
                        y = Program.Link.y;
                        (capture_x, capture_y) = (x, y);
                        Screen.sprites.Remove(this);
                        Screen.sprites.Remove(counterpart);
                        Screen.sprites.Insert(0, this);
                        Screen.sprites.Insert(0, counterpart);
                    }
                    break;

                case ActionState.BURROWING:
                    can_damage_link = false;
                    Program.Link.SetPos(x, y);
                    x = capture_x;
                    y = capture_y;
                    shield_loss_timer--;
                    if (shield_loss_timer <= 0)
                    {
                        // L
                        SaveLoad.magical_shield = false;
                    }
                    break;
            }
        }

        protected override void OnDeath()
        {
            link_is_caught = false;
        }
    }

    internal class PolsVoice : Enemy
    {
        public PolsVoice() : base(AnimationMode.TWOFRAMES_M, 0xa0, 0xa2, false, true, 8, 0.5f, 3)
        {
            palette_index = (byte)PaletteID.SP_0;
            HP = 4;
            damage = 1;
            counterpart.palette_index = palette_index;
            facing_direction = (Direction)Program.RNG.Next(4);
            CheckIfTurn();
            current_action = ActionState.WALKING;
        }

        protected override void EnemySpecificActions()
        {
            switch (current_action)
            {
                case ActionState.WALKING:
                    no_random_turn_flag = false;
                    Walk();
                    break;

                case ActionState.JUMPING:
                    Walk();
                    switch (local_timer)
                    {
                        case 1 or 2 or 3 or 4:
                            y -= 3;
                            break;
                        case 5 or 6 or 7 or 8:
                            y -= 2;
                            break;
                        case 9 or 10 or 11 or 12 or 13 or 14 or 15 or 16 or 18 or 20 or 22 or 24:
                            y -= 1;
                            break;

                        case 55 or 54 or 53 or 52 or 51 or 50 or 49 or 48 or 46 or 44 or 42 or 40:
                            y += 1;
                            break;
                        case 59 or 58 or 57 or 56:
                            y += 2;
                            break;
                        case 62 or 61 or 60:
                            y += 3;
                            break;
                        case 63:
                            y += 3;
                            current_action = ActionState.WALKING;
                            break;
                    }

                    // bounds check
                    if (x < 32)
                        facing_direction = Direction.RIGHT;
                    else if (x > 208)
                        facing_direction = Direction.LEFT;
                    // being above the top wall is fine, wanting to stay there isn't
                    else if (y <= 114 && facing_direction == Direction.UP && local_timer < 2)
                    {
                        y = 96;
                        local_timer = 0;
                        facing_direction = Direction.DOWN;
                    }
                    else if (y > 192)
                    {
                        local_timer = 0;
                        facing_direction = Direction.UP;
                    }
                    break;
            }
        }

        protected override bool OnTileMoved()
        {
            if (current_action == ActionState.WALKING)
            {
                // jump two tiles over if obstacle in way or random chance or not on valid tile
                Direction test = facing_direction;
                if (Program.RNG.Next(8) == 0 || CheckIfTurn() || !IsValidTile((DungeonMetatile)Screen.GetMetaTileTypeAtLocation(x + 8, y + 8)))
                {
                    current_action = ActionState.JUMPING;
                    facing_direction = test;
                    no_random_turn_flag = true;
                    local_timer = 0;
                }
            }

            return current_action == ActionState.WALKING;
        }

        // polsvoices die instantly to the arrow, and the arrow keeps going
        public override bool OnProjectileHit()
        {
            if (hit_cause == typeof(ArrowSprite))
            {
                TakeDamage(9999);
                return false;
            }

            return true;
        }
    }

    internal class Gibdo : Enemy
    {
        public Gibdo() : base(AnimationMode.TWOFRAMES_S, 0xa4, 0xa4, false, true, 8, 0.5f, 2)
        {
            HP = 7;
            damage = 2f;
            facing_direction = (Direction)Program.RNG.Next(4);
            CheckIfTurn();
            current_action = ActionState.WALKING;
            palette_index = (byte)PaletteID.SP_1;
            counterpart.palette_index = palette_index;
        }

        protected override void EnemySpecificActions()
        {
            Walk();
        }
    }

    internal class WizrobeOrange : Enemy
    {
        static bool master_alive = false;
        static bool master_active = false;
        bool is_master;

        static int wizrobe_timer = 0;
        static int wizrobe_timer_lock = 0;

        public WizrobeOrange() : base(AnimationMode.TWOFRAMES_RRUU, 0xb4, 0xbc, false, false, 4, 0f, 2)
        {
            HP = 3;
            damage = 1f;
            current_action = ActionState.DEFAULT;
            palette_index = (byte)PaletteID.SP_2;
            counterpart.palette_index = palette_index;
            shown = false;
            counterpart.shown = false;
            spawn_hidden = true;
            if (!master_active)
            {
                is_master = true;
                master_active = true;
            }
        }

        protected override void EnemySpecificActions()
        {
            if (wizrobe_timer_lock != Program.gTimer)
            {
                wizrobe_timer_lock = Program.gTimer;
                wizrobe_timer++;
            }

            switch (wizrobe_timer)
            {
                case 1:
                    invincible = false;
                    target_x = Program.Link.x;
                    target_y = Program.Link.y;
                    FindSpawnLocation();
                    break;

                case 80:
                    new MagicBeamSprite(x, y, facing_direction, false);
                    break;

                case 158:
                    invincible = true;
                    x = 0;
                    y = 0;
                    break;

                case 210:
                    wizrobe_timer = 0;
                    break;
            }

            if ((wizrobe_timer > 0 && wizrobe_timer <= 64) || (wizrobe_timer > 142 && wizrobe_timer < 158))
            {
                shown = wizrobe_timer % 2 == 0;
                counterpart.shown = shown;
            }
        }

        public override bool OnProjectileHit()
        {
            return (hit_cause == typeof(SwordProjectileSprite) || hit_cause == typeof(BombSprite));
        }

        protected override void OnDeath()
        {
            if (master_alive && !is_master)
            {
                master_active = false;
            }

            if (is_master && master_active)
            {
                foreach (Sprite s in Screen.sprites)
                {
                    if (s is WizrobeOrange w && w != this)
                    {
                        w.HP = 0;
                        w.drop_category = 0;
                        w.appeared = false;
                    }
                }
            }
        }

        void FindSpawnLocation()
        {
            // (x & (~0xf)) == (x - (x % 16))
            int target_tile_x = target_x & (~0xf);
            int target_tile_y = target_y & (~0xf);
            int test_x = x;
            int test_y = y;

            int random_direction = Program.RNG.Next(4);
            List<(int x, int y)> valid_positions = new();
            for (int i = 0; i < 4; i++)
            {
                test_x = target_tile_x;
                test_y = target_tile_y;
                facing_direction = ((Direction)((i + random_direction) % 4)).Opposite();

                for (int j = 0; j < 12; j++)
                {
                    switch ((Direction)((i + random_direction) % 4))
                    {
                        case Direction.UP:
                            test_y -= 16;
                            break;
                        case Direction.DOWN:
                            test_y += 16;
                            break;
                        case Direction.LEFT:
                            test_x -= 16;
                            break;
                        case Direction.RIGHT:
                            test_x += 16;
                            break;
                    }

                    if (test_x < 32 || test_x > 208 || test_y < 96 || test_y > 192)
                        continue;

                    if (IsValidTile((DungeonMetatile)Screen.GetMetaTileTypeAtLocation(test_x + 8, test_y + 8)))
                    {
                        valid_positions.Add((test_x, test_y));
                    }
                }

                if (valid_positions.Count > 0)
                {
                    int index = Program.RNG.Next(valid_positions.Count);
                    test_x = valid_positions[index].x;
                    test_y = valid_positions[index].y;
                    break;
                }
            }

            x = test_x;
            y = test_y;
        }

        public static void ResetData()
        {
            master_active = false;
            master_alive = false;
            wizrobe_timer = 0;
        }
    }

    internal class WizrobeBlue : Enemy
    {
        EightDirection fly_direction;

        public WizrobeBlue() : base(AnimationMode.TWOFRAMES_RRUU, 0xb4, 0xbc, true, false, 8, 0.5f, 1)
        {
            HP = 5;
            damage = 2f;
            facing_direction = (Direction)Program.RNG.Next(4);
            CheckIfTurn();
            current_action = ActionState.WALKING;
            palette_index = (byte)PaletteID.SP_1;
            counterpart.palette_index = palette_index;
        }

        protected override void EnemySpecificActions()
        {
            switch (current_action)
            {
                case ActionState.WALKING:
                    shown = true;
                    counterpart.shown = true;
                    Walk();

                    if (FacingLink() && local_timer % 32 == 0)
                    {
                        new MagicBeamSprite(x, y, facing_direction, false);
                    }
                    break;

                // random diagonal teleport
                case ActionState.FLYING:
                    if (local_timer < 32)
                        break;

                    Move8D();

                    shown = local_timer % 2 == 0;
                    counterpart.shown = local_timer % 2 == 0;

                    if (local_timer >= 63)
                    {
                        if (!IsValidTile((DungeonMetatile)Screen.GetMetaTileTypeAtLocation(x + 8, y + 8)))
                        {
                            current_action = ActionState.JUMPING;
                            local_timer = 0;
                            break;
                        }

                        current_action = ActionState.WALKING;
                        CheckIfTurn();
                    }
                    break;

                // predictable forward teleport
                case ActionState.JUMPING:
                    Move8D();

                    shown = local_timer % 2 == 0;
                    counterpart.shown = local_timer % 2 == 0;

                    if (x < 32)
                        fly_direction = EightDirection.RIGHT;
                    else if (x > 208)
                        fly_direction = EightDirection.LEFT;
                    else if (y < 96)
                        fly_direction = EightDirection.DOWN;
                    else if (y > 192)
                        fly_direction = EightDirection.UP;

                    if (local_timer >= 31)
                    {
                        // keep going if landed on invalid tile
                        if (!IsValidTile((DungeonMetatile)Screen.GetMetaTileTypeAtLocation(x + 8, y + 8)))
                        {
                            local_timer = 0;
                            break;
                        }

                        current_action = ActionState.WALKING;
                        CheckIfTurn();
                    }
                    break;
            }
        }

        protected override bool OnTileMoved()
        {
            if (current_action == ActionState.WALKING)
            {
                // jump over if obstacle in way or random chance or not on valid tile
                Direction test = facing_direction;
                if (CheckIfTurn() || !IsValidTile((DungeonMetatile)Screen.GetMetaTileTypeAtLocation(x + 8, y + 8)))
                {
                    if ((x == 32 && test == Direction.LEFT) || (x == 208 && test == Direction.RIGHT) ||
                        (y == 96 && test == Direction.UP) || (y == 192 && test == Direction.DOWN))
                        return true;

                    current_action = ActionState.JUMPING;
                    facing_direction = test;
                    fly_direction = (EightDirection)facing_direction;
                    local_timer = 0;
                    return false;
                }

                if (Program.RNG.Next(16) == 0)
                {
                    fly_direction = (EightDirection)(Program.RNG.Next(4) + 4);

                    switch (fly_direction)
                    {
                        case EightDirection.UPLEFT:
                            if (y < 128 || x < 64)
                                return true;
                            facing_direction = Direction.UP;
                            break;

                        case EightDirection.UPRIGHT:
                            if (y < 128 || x > 192)
                                return true;
                            facing_direction = Direction.UP;
                            break;

                        case EightDirection.DOWNLEFT:
                            if (y > 176 || x < 64)
                                return true;
                            facing_direction = Direction.LEFT;
                            break;

                        case EightDirection.DOWNRIGHT:
                            if (y > 176 || x > 192)
                                return true;
                            facing_direction = Direction.RIGHT;
                            break;
                    }

                    current_action = ActionState.FLYING;
                    local_timer = 0;
                }
            }

            return current_action == ActionState.WALKING;
        }

        public override bool OnProjectileHit()
        {
            return (hit_cause == typeof(SwordProjectileSprite) || hit_cause == typeof(BombSprite));
        }

        void Move8D()
        {
            if (fly_direction == EightDirection.UP || fly_direction == EightDirection.UPLEFT ||
                fly_direction == EightDirection.UPRIGHT)
            {
                y--;
            }
            else if (fly_direction == EightDirection.DOWN || fly_direction == EightDirection.DOWNLEFT ||
                fly_direction == EightDirection.DOWNRIGHT)
            {
                y++;
            }

            if (fly_direction == EightDirection.LEFT || fly_direction == EightDirection.UPLEFT ||
                fly_direction == EightDirection.DOWNLEFT)
            {
                x--;
            }
            else if (fly_direction == EightDirection.RIGHT || fly_direction == EightDirection.UPRIGHT ||
                fly_direction == EightDirection.DOWNRIGHT)
            {
                x++;
            }
        }

        bool FacingLink()
        {
            switch (facing_direction)
            {
                case Direction.UP:
                    return Program.Link.x == x && Program.Link.y < y;
                case Direction.DOWN:
                    return Program.Link.x == x && Program.Link.y > y;
                case Direction.LEFT:
                    return Program.Link.y == y && Program.Link.x < x;
                case Direction.RIGHT:
                    return Program.Link.y == y && Program.Link.x > x;

                default:
                    return false;
            }
        }
    }
}