using The_Legend_of_Zelda.Gameplay;
using The_Legend_of_Zelda.Rendering;
using The_Legend_of_Zelda.Sprites;
using static The_Legend_of_Zelda.Gameplay.Program;

namespace The_Legend_of_Zelda.Sprites
{
    internal class Octorok : Enemy
    {
        int when_to_shoot;

        public Octorok(bool stronger) : base(AnimationMode.TWOFRAMES_DMDMLL, 0xb0, 0xb2, stronger, true, 6, 0.5f, 1)
        {
            when_to_shoot = RNG.Next(30, 100);
            damage = 0.5f;
            facing_direction = (Direction)RNG.Next(4);
            CheckIfTurn();
            current_action = ActionState.WALKING;

            if (RNG.Next(5) == 0)
                speed = 1;

            if (!stronger)
            {
                palette_index = 6;
                HP = 1;
            }
            else
            {
                palette_index = 5;
                HP = 2;
                drop_category = 2;
            }
        }

        public override void EnemySpecificActions()
        {
            switch (current_action)
            {
                case ActionState.SHOOTING_PROJECTILE:
                    if (local_timer > 120)
                        local_timer = 0;

                    if (local_timer == 36)
                    {
                        new RockProjectileSprite(x + 4, y, facing_direction);
                    }
                    else if (local_timer >= 54)
                    {
                        current_action = ActionState.WALKING;
                        local_timer = 0;
                        when_to_shoot = RNG.Next(180, 600);
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
    }

    internal class Moblin : Enemy
    {
        int when_to_shoot;

        public Moblin(bool stronger) : base(AnimationMode.TWOFRAMES_RRDU, 0xf0, 0xf8, stronger, false, 6, 0.5f, 1)
        {
            when_to_shoot = RNG.Next(30, 60);
            damage = 0.5f;
            facing_direction = (Direction)RNG.Next(4);
            CheckIfTurn();
            current_action = ActionState.WALKING;
            appeared = true;

            byte palette_to_apply;
            if (!stronger)
            {
                palette_to_apply = 6;
                HP = 2;
            }
            else
            {
                palette_to_apply = 7;
                drop_category = 2;
                HP = 3;
            }
            palette_index = palette_to_apply;
            counterpart.palette_index = palette_to_apply;

            if (OC.overworld_screens_side_entrance.Contains(OC.current_screen))
            {
                smoke_appearance = true;
                appeared = false;
                SpawnOnEdge();
            }
        }

        public override void EnemySpecificActions()
        {
            if (current_action == ActionState.SHOOTING_PROJECTILE)
            {
                if (local_timer > 120)
                    local_timer = 0;

                if (local_timer == 36)
                {
                    new ArrowSprite(x + 4, y, facing_direction, false);
                }
                else if (local_timer >= 54)
                {
                    current_action = ActionState.WALKING;
                    local_timer = 0;
                    when_to_shoot = RNG.Next(180, 600);
                }
            }
            else if (current_action == ActionState.WALKING)
            {
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
            }
        }
    }

    internal class Lynel : Enemy
    {
        int when_to_shoot;

        public Lynel(bool stronger) : base(AnimationMode.TWOFRAMES_RRDU, 0xce, 0xd6, stronger, true, 6, 0.5f, 4)
        {
            when_to_shoot = RNG.Next(30, 60);
            byte palette;
            if (stronger)
            {
                damage = 2;
                palette = 5;
                HP = 6;
                drop_category = 2;
            }
            else
            {
                damage = 1;
                palette = 6;
                HP = 4;
            }
            palette_index = palette;
            counterpart.palette_index = palette;
            facing_direction = (Direction)RNG.Next(4);
            CheckIfTurn();
            current_action = ActionState.WALKING;
        }

        public override void EnemySpecificActions()
        {
            if (current_action == ActionState.SHOOTING_PROJECTILE)
            {
                if (local_timer > 120)
                    local_timer = 0;

                if (local_timer == 36)
                {
                    new SwordProjectileSprite(x + 4, y - 4, facing_direction, false);
                }
                else if (local_timer >= 54)
                {
                    current_action = ActionState.WALKING;
                    local_timer = 0;
                    when_to_shoot = RNG.Next(180, 600);
                }
            }
            else if (current_action == ActionState.WALKING)
            {
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
            }
        }
    }

    internal class Zora : Enemy
    {
        public Zora() : base(AnimationMode.ONEFRAME_DMUM, 0xbc, 0xbe, false, false, 0, 0, 4)
        {
            stunnable = false;
            HP = 2;
            damage = 0.5f;
            current_action = ActionState.BURROWING;
            palette_index = 7;
            counterpart.palette_index = 7;
            FindNewLocation();
            local_timer = 99;
            spawn_hidden = true;
        }

        public override void EnemySpecificActions()
        {
            switch (current_action)
            {
                case ActionState.DEFAULT:
                    if (local_timer == 1)
                    {
                        invincible = false;
                        tile_location_1 = 0xec;
                        tile_location_2 = 0xee;
                        if (Link.y < y)
                            facing_direction = Direction.UP;
                        else
                            facing_direction = Direction.DOWN;
                    }
                    else if (local_timer == 20)
                    {
                        new MagicOrbProjectileSprite(x + 4, y);
                    }
                    else if (local_timer >= 72)
                    {
                        current_action = ActionState.BURROWING;
                        invincible = true;
                        local_timer = 0;
                    }
                    break;
                case ActionState.BURROWING:
                    if (local_timer == 1)
                    {
                        can_damage_link = false;
                        tile_location_1 = 0xbc;
                        tile_location_2 = 0xbe;
                    }
                    else if (local_timer == 96)
                    {
                        shown = false;
                        counterpart.shown = false;
                    }
                    else if (local_timer == 98)
                    {
                        shown = true;
                        counterpart.shown = true;
                        FindNewLocation();
                        HP += 1;
                        if (HP > 2)
                            HP = 2;
                    }
                    else if (local_timer >= 130)
                    {
                        can_damage_link = true;
                        current_action = ActionState.DEFAULT;
                        local_timer = 0;
                    }

                    if (local_timer % 12 == 0)
                        facing_direction = Direction.UP;
                    else if (local_timer % 12 == 6)
                        facing_direction = Direction.DOWN;
                    break;
            }
        }

        void FindNewLocation()
        {
            bool valid_location_found = false;
            int new_x, new_y;
            int counter = 0;
            while (!valid_location_found)
            {
                counter++;
                if (counter > 300)
                    return;

                new_x = RNG.Next(0, 16) * 16;
                new_y = RNG.Next(4, 15) * 16;
                if (IsTileValid(Screen.GetMetaTileTypeAtLocation(new_x, new_y)))
                {
                    valid_location_found = true;
                    x = new_x;
                    y = new_y;
                }
            }
        }

        bool IsTileValid(MetatileType index)
        {
            return index >= MetatileType.WATER && index <= MetatileType.WATER_BL;
        }
    }

    internal class Leever : Enemy
    {
        Direction walking_dir;

        public Leever(bool stronger) : base(AnimationMode.TWOFRAMES_M, 0xbc, 0xbe, stronger, false, 6, 0.5f, 3)
        {
            damage = 0.5f;
            if (stronger)
            {
                HP = 4;
                drop_category = 1;
            }
            else
            {
                HP = 2;
            }
            palette_index = 6;
            counterpart.palette_index = 6;
            current_action = ActionState.BURROWING;
            local_timer = 23;
            shown = false;
            counterpart.shown = false;
            spawn_hidden = true;
        }

        public override void EnemySpecificActions()
        {
            switch (current_action)
            {
                case ActionState.WALKING:
                    if (stronger)
                    {
                        Walk();
                        if (local_timer > 256)
                        {
                            local_timer = 0;
                            current_action = ActionState.BURROWING;
                        }
                    }
                    else
                    {
                        if (walking_dir != facing_direction)
                        {
                            local_timer = 0;
                            current_action = ActionState.BURROWING;
                        }
                        else
                            Walk();
                    }
                    break;
                case ActionState.BURROWING:
                    if (local_timer == 1)
                    {
                        speed = 0;
                        tile_location_1 = 0xc0;
                        tile_location_2 = 0xc0;
                    }
                    else if (local_timer == 9)
                    {
                        can_damage_link = false;
                        palette_index = 6;
                        counterpart.palette_index = 6;
                        tile_location_1 = 0xbc;
                        tile_location_2 = 0xbe;
                    }
                    else if (local_timer == 24)
                    {
                        shown = false;
                        counterpart.shown = false;
                        if (RNG.Next(30) != 0)
                            local_timer--;
                    }
                    else if (local_timer == 25)
                    {
                        shown = true;
                        counterpart.shown = true;
                        PickNewLocation();
                        CheckIfTurn();
                    }
                    else if (local_timer == 42)
                    {
                        can_damage_link = true;
                        palette_index = (byte)(stronger ? 5 : 6);
                        counterpart.palette_index = (byte)(stronger ? 5 : 6);
                        tile_location_1 = 0xc0;
                        tile_location_2 = 0xc0;
                    }
                    else if (local_timer == 50)
                    {
                        speed = 0.5f;
                        local_timer = 0;
                        tile_location_1 = 0xc2;
                        tile_location_2 = 0xc4;
                        current_action = ActionState.WALKING;
                    }
                    break;
            }
        }

        bool IsValidTile(MetatileType tile)
        {
            return tile is (MetatileType.GROUND or MetatileType.SAND);
        }

        bool IsOOB(int x, int y)
        {
            return x < 0 || x > 240 || y < 64 || y > 224;
        }

        void PickNewLocation()
        {
            int new_x, new_y;
            if (stronger)
            {
                while (true)
                {
                    new_x = RNG.Next(1, 15) * 16;
                    new_y = RNG.Next(5, 14) * 16;
                    if (IsValidTile(Screen.GetMetaTileTypeAtLocation(new_x, new_y)))
                        break;
                }
                CheckIfTurn();
            }
            else
            {
                new_x = 0;
                new_y = 0;

                if (Link.facing_direction == Direction.UP)
                    new_y = 48;
                else if (Link.facing_direction == Direction.DOWN)
                    new_y = -48;
                else if (Link.facing_direction == Direction.LEFT)
                    new_x = 48;
                else if (Link.facing_direction == Direction.RIGHT)
                    new_x = -48;

                if (!(Control.IsHeld(Buttons.UP) || Control.IsHeld(Buttons.DOWN) ||
                    Control.IsHeld(Buttons.LEFT) || Control.IsHeld(Buttons.RIGHT)))
                {
                    new_x = -new_x;
                    new_y = -new_y;
                }

                if (!IsValidTile(Screen.GetMetaTileTypeAtLocation(new_x + Link.x, new_y + Link.y)) || IsOOB(new_x + Link.x, new_y + Link.y))
                {
                    new_x = -new_x;
                    new_y = -new_y;
                    if (!IsValidTile(Screen.GetMetaTileTypeAtLocation(new_x + Link.x, new_y + Link.y)) || IsOOB(new_x + Link.x, new_y + Link.y))
                    {
                        if (new_x == 0)
                        {
                            new_x = 48;
                            new_y = 0;
                        }
                        else
                        {
                            new_x = 0;
                            new_y = 48;
                        }
                        if (!IsValidTile(Screen.GetMetaTileTypeAtLocation(new_x + Link.x, new_y + Link.y)) || IsOOB(new_x + Link.x, new_y + Link.y))
                        {
                            new_x = -new_x;
                            new_y = -new_y;
                        }
                    }
                }

                if (MathF.Abs(new_x) > MathF.Abs(new_y))
                {
                    if (new_x < 0)
                        facing_direction = Direction.RIGHT;
                    else
                        facing_direction = Direction.LEFT;
                }
                else
                {
                    if (new_y < 0)
                        facing_direction = Direction.DOWN;
                    else
                        facing_direction = Direction.UP;
                }
                walking_dir = facing_direction;
                new_x += Link.x;
                new_y += Link.y;
            }
            x = new_x & 0xFFF0;
            y = new_y & 0xFFF0;
        }
    }

    internal class Rock : Enemy
    {
        int when_to_drop;
        bool go_left;

        public Rock() : base(AnimationMode.TWOFRAMES, 0x90, 0xe8, false, false, 6, 0, 0)
        {
            stunnable = false;
            invincible = true;
            damage = 0.5f;
            HP = 1;
            palette_index = 6;
            counterpart.palette_index = 6;
            spawn_hidden = true;
            shown = false;
            counterpart.shown = false;
            current_action = ActionState.DEFAULT;
            go_left = Convert.ToBoolean(RNG.Next(2));
            Spawn();
        }

        public override void EnemySpecificActions()
        {
            switch (current_action)
            {
                case ActionState.RESTING:
                    if (local_timer == 1)
                    {
                        when_to_drop = RNG.Next(2, 64);
                    }
                    else if (local_timer > when_to_drop)
                    {
                        local_timer = 0;
                        current_action = ActionState.WALKING;
                    }
                    break;
                case ActionState.WALKING:
                    if (local_timer < 6)
                        y--;
                    else if (local_timer > 8)
                        y += 2;

                    if (local_timer > 28)
                    {
                        local_timer = 0;
                        PickNewDirection();
                    }

                    if (x <= 0 || x >= 240)
                        go_left = !go_left;

                    if (go_left)
                        x--;
                    else
                        x++;

                    facing_direction = (Direction)((int)Link.facing_direction ^ 1);

                    if (y > 240)
                    {
                        local_timer = 0;
                        current_action = ActionState.DEFAULT;
                    }
                    break;
                case ActionState.DEFAULT:
                    if (local_timer == 1)
                    {
                        when_to_drop = RNG.Next(2, 120);
                        shown = false;
                        counterpart.shown = false;
                    }
                    else if (local_timer > when_to_drop)
                    {
                        shown = true;
                        counterpart.shown = true;
                        Spawn();
                        local_timer = 0;
                        current_action = ActionState.RESTING;
                    }
                    break;
            }
        }

        void Spawn()
        {
            y = 64;
            x = RNG.Next(1, 239);
        }

        // heavy bias towards link
        void PickNewDirection()
        {
            bool value = RNG.Next(5) != 0;
            if (x < Link.x)
                value = !value;
            go_left = value;
        }
    }

    internal class Tektite : Enemy
    {
        int when_to_stop;
        int jump_height;
        bool second_jump;
        bool go_left;

        public Tektite(bool stronger) : base(AnimationMode.TWOFRAMES_M, 0xca, 0xcc, stronger, true, 16, 0, 1)
        {
            byte palette;
            if (stronger)
            {
                palette = 5;
                drop_category = 3;
            }
            else
            {
                palette = 6;
            }
            damage = 0.5f;
            HP = 1;
            palette_index = palette;
            counterpart.palette_index = palette;
            current_action = ActionState.RESTING;
        }

        public override void EnemySpecificActions()
        {
            switch (current_action)
            {
                case ActionState.JUMPING:
                    if (local_timer == 1)
                    {
                        jump_height = RNG.Next(20, 45);
                    }

                    if (local_timer < 50 - jump_height)
                        y -= 2;
                    else if (local_timer > 50 - jump_height + 10)
                        y += 2;

                    if (y < 32)
                        local_timer++;
                    else if (y > 224)
                        local_timer = 51;

                    if (local_timer > 50)
                    {
                        if (y < 64 || y > 224)
                        {
                            local_timer = 1;
                            if (y < 64)
                                jump_height = RNG.Next(40, 50);
                            else
                                jump_height = RNG.Next(10, 20);
                            break;
                        }

                        local_timer = 0;
                        if (!second_jump && RNG.Next(2) == 0 && !stronger)
                        {
                            second_jump = true;
                            break;
                        }
                        second_jump = false;
                        PickNewDirection();
                        current_action = ActionState.RESTING;
                    }

                    if (x <= 0 || x >= 240)
                        go_left = !go_left;

                    if (go_left)
                        x--;
                    else
                        x++;
                    break;
                case ActionState.RESTING:
                    if (local_timer == 1)
                    {
                        frames_between_anim = 16;
                        if (!stronger)
                            when_to_stop = RNG.Next(45, 90);
                        else
                            when_to_stop = RNG.Next(90, 180);
                    }
                    else if (local_timer > when_to_stop)
                    {
                        local_timer = 0;
                        current_action = ActionState.JUMPING;
                    }
                    break;
            }
        }

        void PickNewDirection()
        {
            bool value = Convert.ToBoolean(RNG.Next(7));
            if (x < target_x)
                value = !value;
            go_left = value;
        }
    }

    internal class Peahat : Enemy
    {
        int when_to_stop;
        int num_times_turned;
        EightDirection direction;

        public Peahat() : base(AnimationMode.TWOFRAMES_M, 0xc6, 0xc8, false, false, 0, 0, 4)
        {
            current_action = ActionState.FLYING;
            invincible = true;
            palette_index = 6;
            counterpart.palette_index = 6;
            HP = 2;
            damage = 0.5f;
            speed = 0.5f;
            direction = EightDirection.UP;
        }

        public override void EnemySpecificActions()
        {
            switch (current_action)
            {
                case ActionState.RESTING:
                    if (local_timer == 1)
                    {
                        frames_between_anim = 4;
                    }
                    else if (local_timer == 24)
                    {
                        frames_between_anim = 8;
                    }
                    if (local_timer <= 48)
                    {
                        if (local_timer % frames_between_anim == 0)
                        {
                            Move8D();
                        }
                        break;
                    }

                    if (local_timer == 49)
                    {
                        frames_between_anim = 0;
                        invincible = false;
                        when_to_stop = RNG.Next(120, 240);
                    }
                    else if (local_timer > when_to_stop + 49)
                    {
                        local_timer = 0;
                        num_times_turned = 0;
                        invincible = true;
                        current_action = ActionState.FLYING;
                    }
                    break;
                case ActionState.FLYING:
                    if (local_timer == 1)
                    {
                        frames_between_anim = 8;
                    }
                    else if (local_timer == 24)
                    {
                        frames_between_anim = 4;
                    }
                    else if (local_timer == 55)
                    {
                        direction = PickNewDirection();
                        when_to_stop = RNG.Next(60, 120);
                        frames_between_anim = 2;
                    }

                    if (local_timer <= 54)
                    {
                        if (local_timer <= 24 && local_timer % 8 == 0 || local_timer > 24 && local_timer % 4 == 0)
                            Move8D();

                        break;
                    }

                    if (local_timer > 56 + when_to_stop)
                    {
                        num_times_turned++;
                        if (num_times_turned < 5 || RNG.Next(8) != 0)
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

                        local_timer = 56;
                    }

                    Move8D();
                    break;
            }
        }

        void Move8D()
        {
            if (local_timer % 2 != 0)
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
    }

    internal class Ghini : Enemy
    {
        public bool is_master;
        int when_to_stop;
        int num_times_turned;
        EightDirection direction = EightDirection.DOWN;
        Ghini? master = null;

        public Ghini(bool master, int x = -1, int y = -1) : base(AnimationMode.ONEFRAME_DU, 0xe0, 0xe4, false, true, 0, 0.5f, 3, true)
        {
            is_master = master;
            palette_index = 5;
            counterpart.palette_index = 5;
            damage = 1;
            HP = 9;
            target_x = Link.x;
            target_y = Link.y;
            if (!master)
            {
                invincible = true;
                smoke_appearance = false;
                facing_direction = Direction.DOWN;
                current_action = ActionState.DEFAULT;
                this.x = x;
                this.y = y;
                shown = false;
                counterpart.shown = false;
                spawn_hidden = true;
                speed = 1;

                foreach (Sprite spr in Screen.sprites)
                {
                    if (spr is Ghini other)
                    {
                        if (other.is_master)
                        {
                            this.master = other;
                            break;
                        }
                    }
                }
            }
            else
            {
                FindValidSpawnLocation();
                current_action = ActionState.WALKING;
            }
        }

        public override void EnemySpecificActions()
        {
            switch (current_action)
            {
                case ActionState.DEFAULT:
                    bool show = local_timer % 2 == 1;
                    shown = show;
                    counterpart.shown = show;

                    if (local_timer == 1)
                    {
                        shown = true;
                        counterpart.shown = true;
                    }
                    if (local_timer > 60)
                    {
                        local_timer = 0;
                        current_action = ActionState.WALKING;
                    }
                    break;
                case ActionState.WALKING:
                    if (is_master)
                        Walk();
                    else
                        current_action = ActionState.FLYING;
                    break;
                case ActionState.RESTING: // non master ghosts fly just like peahats. copied code lol
                    if (local_timer == 1)
                    {
                        frames_between_anim = 4;
                    }
                    else if (local_timer == 24)
                    {
                        frames_between_anim = 8;
                    }
                    if (local_timer <= 48)
                    {
                        if (local_timer <= 24 && local_timer % 4 == 0 || local_timer > 24 && local_timer % 8 == 0)
                        {
                            Move8D();
                        }
                        break;
                    }

                    if (local_timer == 49)
                    {
                        frames_between_anim = 0;
                        when_to_stop = RNG.Next(60, 120);
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
                        frames_between_anim = 8;
                    }
                    else if (local_timer == 24)
                    {
                        frames_between_anim = 4;
                    }
                    else if (local_timer == 55)
                    {
                        direction = (EightDirection)RNG.Next(8);
                        when_to_stop = RNG.Next(60, 120);
                        frames_between_anim = 2;
                    }

                    if (local_timer <= 54)
                    {
                        if (local_timer <= 24 && local_timer % 8 == 0 || local_timer > 24 && local_timer % 4 == 0)
                            Move8D();

                        break;
                    }

                    if (local_timer > 56 + when_to_stop)
                    {
                        num_times_turned++;
                        if (num_times_turned < 5 || RNG.Next(8) != 0)
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

                        local_timer = 56;
                    }

                    if (direction is EightDirection.UP or EightDirection.UPLEFT or EightDirection.UPRIGHT)
                        facing_direction = Direction.UP;
                    else
                        facing_direction = Direction.DOWN;
                    Move8D();
                    break;
            }

            if (!is_master && master != null)
            {
                if (master.HP <= 0)
                {
                    HP = 0;
                    appeared = false;
                }
            }
        }

        void Move8D()
        {
            if (local_timer % 2 != 0)
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

    internal class Armos : Enemy
    {
        int metatile_index;

        public Armos(int metatile_index, int x, int y) : base(AnimationMode.TWOFRAMES_DDUU, 0xa0, 0xa4, false, false, 6, 0, 0, true)
        {
            palette_index = 6;
            counterpart.palette_index = 6;
            damage = 1;
            HP = 3;
            target_x = Link.x;
            target_y = Link.y;
            speed = RNG.Next(1, 5) / 2f;
            facing_direction = Direction.DOWN;
            this.x = x;
            this.y = y;
            this.metatile_index = metatile_index;
        }

        public override void EnemySpecificActions()
        {
            switch (current_action)
            {
                case ActionState.DEFAULT:
                    bool show = local_timer % 2 == 1;
                    shown = show;
                    counterpart.shown = show;

                    if (local_timer == 1)
                    {
                        shown = true;
                        counterpart.shown = true;
                    }
                    if (local_timer > 60)
                    {
                        local_timer = 0;
                        ReplaceOGTile();
                        current_action = ActionState.WALKING;
                    }
                    break;
                case ActionState.WALKING:
                    Walk();
                    break;
            }
        }

        void ReplaceOGTile()
        {
            int ppu_tile_location = 256 + (metatile_index >> 4) * 64 + metatile_index % 16 * 2;
            if (OC.current_screen == 36 && !SaveLoad.power_bracelet && metatile_index == 78)
                new PowerBraceletSprite(x + 4, y);
            byte screen = OC.current_screen;
            if (screen == 11 && metatile_index == 75 || screen == 34 && metatile_index == 67 || screen == 28 && metatile_index == 75 ||
                screen == 52 && metatile_index == 68 || screen == 61 && metatile_index == 73 || screen == 78 && metatile_index == 74)
            {
                Screen.meta_tiles[metatile_index].tile_index = MetatileType.STAIRS;
                Textures.ppu[ppu_tile_location] = 0x70;
                Textures.ppu[ppu_tile_location + 1] = 0x72;
                Textures.ppu[ppu_tile_location + 32] = 0x71;
                Textures.ppu[ppu_tile_location + 33] = 0x73;
                //TODO: play secret found sfx
            }
            else
            {
                Screen.meta_tiles[metatile_index].tile_index = MetatileType.GROUND;
                Textures.ppu[ppu_tile_location] = 0x26;
                Textures.ppu[ppu_tile_location + 1] = 0x26;
                Textures.ppu[ppu_tile_location + 32] = 0x26;
                Textures.ppu[ppu_tile_location + 33] = 0x26;
            }
        }
    }
}