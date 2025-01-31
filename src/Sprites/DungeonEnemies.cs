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

        public override void EnemySpecificActions()
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

        public Keese(bool stronger) : base(AnimationMode.TWOFRAMES_M, 0x9a, 0x9c, stronger, true, 6, 0.75f, 0)
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
        }

        public override void EnemySpecificActions()
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

        public override void EnemySpecificActions()
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
            HP = 2;
            damage = 0.5f;
            facing_direction = (Direction)Program.RNG.Next(4);
            CheckIfTurn();
            current_action = ActionState.WALKING;
            palette_index = (byte)PaletteID.SP_3;
            counterpart.palette_index = palette_index;
        }

        public override void EnemySpecificActions()
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
            // if the zol takes enough damage in a single hit, it does not create children
            if (HP <= -1)
                return;

            drop_category = 0;
            new Gel(this);
            new Gel(this);
            Program.DC.nb_enemies_alive += 2;
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

        public override void EnemySpecificActions()
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

        public override void EnemySpecificActions()
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

        private void SetCurrentWall()
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

        private void FindSpawnLocation()
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

            public override void EnemySpecificActions()
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
        public Rope(bool stronger) : base(AnimationMode.TWOFRAMES, 0xa0, 0xa4, stronger, true, 10, 0.5f, 3)
        {

        }

        public override void EnemySpecificActions()
        {

        }
    }

    internal class Statues : Enemy
    {
        public Statues() : base(AnimationMode.ONEFRAME, 0x1c, 0x1c, false, false, 0, 0f, 0, true)
        {

        }

        public override void EnemySpecificActions()
        {

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
        public override void EnemySpecificActions()
        {
            Walk();
        }
    }

    internal class Bubble : Enemy
    {
        public enum BubbleType
        {
            NORMAL,
            BLUE,
            RED
        }

        public Bubble(BubbleType type) : base(AnimationMode.ONEFRAME, 0x8e, 0x8e, false, true, 0, 1f, 0)
        {

        }

        public override void EnemySpecificActions()
        {

        }
    }

    internal class Vire : Enemy
    {
        public Vire() : base(AnimationMode.TWOFRAMES_DMUM, 0xac, 0xb0, false, true, 10, 0.5f, 2)
        {

        }

        public override void EnemySpecificActions()
        {

        }
    }

    internal class Likelike : Enemy
    {
        public Likelike() : base(AnimationMode.THREEFRAMES_M, 0xa2, 0xa4, false, true, 8, 0.5f, 0)
        {

        }

        public override void EnemySpecificActions()
        {

        }
    }

    internal class PolsVoice : Enemy
    {
        public PolsVoice() : base(AnimationMode.TWOFRAMES_M, 0xa0, 0xa2, false, true, 8, 0.5f, 3)
        {

        }

        public override void EnemySpecificActions()
        {

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

        public override void EnemySpecificActions()
        {
            Walk();
        }
    }

    internal class Wizrobe : Enemy
    {
        public Wizrobe(bool stronger) : base(AnimationMode.TWOFRAMES_RRUU, 0xb4, 0xbc, stronger, false, 8, 0.5f, 3)
        {

        }

        public override void EnemySpecificActions()
        {

        }
    }
}