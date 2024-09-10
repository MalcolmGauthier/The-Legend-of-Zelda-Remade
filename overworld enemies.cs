namespace The_Legend_of_Zelda
{
    internal abstract class Enemy : Sprite
    {
        public int local_timer = 0;
        public int target_x;
        public int target_y;
        int time_when_stunned = -1;

        public bool pause_animation = false;
        public bool invincible = false;
        public bool can_damage_link = true;
        public bool appeared = false;
        public bool smoke_appearance = false;
        public bool stronger = false;
        public bool spawn_hidden = false;
        public bool stunnable = true;
        bool target_antilink = false;

        public byte smoke_random_appearance = 1;
        public byte frames_between_anim = 0;
        public byte iframes_timer = 0;
        public byte knockback_timer = 0;
        public byte drop_category = 0;
        public byte tile_location_1 = 0;
        public byte tile_location_2 = 0;
        byte nb_of_times_moved = 0;
        byte og_palette;

        public float HP = 0;
        public float speed = 0;
        public float damage = 0;

        public Direction facing_direction;
        public Direction knockback_direction;
        public ActionState current_action = ActionState.DEFAULT;
        public ActionState unstunned_action;
        AnimationMode animation_mode;

        public StaticSprite counterpart = new StaticSprite(0, 0, 0, 0);

        public enum AnimationMode
        {
            ONEFRAME,         // bubble
            ONEFRAME_M,       // blade trap
            ONEFRAME_DU,      // ghini
            ONEFRAME_DMUM,    // zora

            TWOFRAMES,        // rock, rope
            TWOFRAMES_M,      // leever, peahat, tektite, pols voice, zol
            TWOFRAMES_HM,     // wallmaster
            TWOFRAMES_SOLO,   // gel
            TWOFRAMES_RRDU,   // lynel, moblin
            TWOFRAMES_RRUU,   // wizrobes
            TWOFRAMES_DURR,   // goriya
            TWOFRAMES_DDUU,   // armos
            TWOFRAMES_DDUU_M, // vire
            TWOFRAMES_DMDMLL, // octorok

            THREEFRAMES_M     // likelike
        }
        public enum ActionState
        {
            DEFAULT,
            WALKING,
            SHOOTING_PROJECTILE,
            RISING,
            BURROWING,
            FLYING,
            RESTING,
            JUMPING,
            STUNNED,
        }
        public Enemy(AnimationMode animation_mode, byte tile_location_1, byte tile_location_2, bool stronger, bool smoke_appearance, 
            byte frames_between_animation_frames, float speed, byte drop_category, bool set_spawn = false) : base(0, 0)
        {
            this.animation_mode = animation_mode;
            this.tile_location_1 = tile_location_1;
            this.tile_location_2 = tile_location_2;
            this.smoke_appearance = smoke_appearance;
            this.speed = speed;
            this.drop_category = drop_category;
            frames_between_anim = frames_between_animation_frames;
            this.stronger = stronger;

            unload_during_transition = true;
            counterpart.unload_during_transition = true;
            if (!set_spawn)
                FindValidSpawnLocation();
            target_antilink = Program.RNG.Next() % 2 == 0;

            Screen.sprites.Add(this);
            Screen.sprites.Add(counterpart);
        }
        void Init()
        {
            appeared = true;
            palette_index = og_palette;
            counterpart.palette_index = og_palette;
            if (!spawn_hidden)
            {
                shown = true;
                counterpart.shown = true;
            }

            Animation();
        }
        public override void Action()
        {
            if (Link.can_move)
            {
                if (!appeared)
                {
                    if (HP <= 0)
                    {
                        Die();
                    }
                    else
                    {
                        SetSmokeGraphic();
                    }
                }
                else
                {
                    Animation();

                    if (IsWithinLink() && !Link.clock_flash && can_damage_link)
                    {
                        Link.knockback_direction = (Direction)((int)Link.facing_direction ^ 1);
                        Link.TakeDamage(damage);
                    }
                    TouchingSword();

                    if (current_action == ActionState.STUNNED || Link.clock_flash)
                        Stunned();
                    else
                        EnemySpecificActions();

                    if (iframes_timer > 0)
                        HitFlash();
                    Knockback();
                }

                counterpart.x = x + 8;
                counterpart.y = y;
                local_timer++;
            }
        }
        public abstract void EnemySpecificActions();
        bool IsPositionValid(int x, int y)
        {
            byte tile_id = (byte)Screen.GetTileIndexAtLocation(x, y);

            switch (tile_id)
            {
                case 1 or 3 or 0x13 or 0x14 or 0x15 or 0x16:
                    return true;
                default:
                    return false;
            }
        }
        bool IsWithinLink()
        {
            return x + 12 >= Link.x && x - 12 <= Link.x && 
                   y + 12 >= Link.y && y - 12 <= Link.y;
        }
        void SetPosMod16()
        {
            if (x % 16 < 8)
                x -= x % 16;
            else
                x += 16 - (x % 16);

            if (y % 16 < 8)
                y -= y % 16;
            else
                y += 16 - (y % 16);
        }
        void SetSmokeGraphic()
        {
            if (!smoke_appearance)
            {
                og_palette = palette_index;
                Init();
                return;
            }

            if (local_timer == 0)
            {
                og_palette = palette_index;
                if (OverworldCode.overworld_screens_side_entrance.Contains(OverworldCode.current_screen) && Program.gamemode == Program.Gamemode.OVERWORLD)
                {
                    SpawnOnEdge();
                    shown = false;
                    counterpart.shown = false;
                }
                smoke_random_appearance = (byte)Program.RNG.Next(10, 90);
                palette_index = 5;
                counterpart.palette_index = 5;
                tile_index = 0x70;
                counterpart.tile_index = 0x70;
                counterpart.xflip = true;
            }
            else if (local_timer == smoke_random_appearance)
            {
                tile_index = 0x72;
                counterpart.tile_index = 0x72;
            }
            else if (local_timer == smoke_random_appearance + 6)
            {
                tile_index = 0x74;
                counterpart.tile_index = 0x74;
            }
            else if (local_timer == smoke_random_appearance + 12)
            {
                counterpart.xflip = false;
                Init();
            }
        }
        public void SpawnOnEdge()
        {
            byte count = 0;
            bool valid_pos = false;
            bool care_about_dir = true;
            while (!valid_pos)
            {
                count++;
                if (count == 100)
                    care_about_dir = false;
                else if (count > 200)
                {
                    HP = 0;
                    return;
                }

                switch ((Direction)Program.RNG.Next(4))
                {
                    case Direction.UP:
                        x = Program.RNG.Next(1, 15) * 16;
                        y = 64;
                        facing_direction = Direction.DOWN;
                        break;
                    case Direction.DOWN:
                        x = Program.RNG.Next(1, 15) * 16;
                        y = 224;
                        facing_direction = Direction.UP;
                        break;
                    case Direction.LEFT:
                        x = 0;
                        y = Program.RNG.Next(6, 14) * 16;
                        facing_direction = Direction.RIGHT;
                        break;
                    case Direction.RIGHT:
                        x = 240;
                        y = Program.RNG.Next(6, 14) * 16;
                        facing_direction = Direction.LEFT;
                        break;
                }

                if (IsValidTile(Screen.GetTileIndexAtLocation(x, y)) && !IsWithinLink() && 
                    (care_about_dir ? facing_direction != (Direction)Link.facing_direction : true))
                    valid_pos = true;
            }
        }
        void Animation()
        { // readable code? no thanks, i'm full.
            bool flip;
            int next_tile = 2;
            int tile_to_use;

            switch (animation_mode)
            {
                case AnimationMode.ONEFRAME or AnimationMode.ONEFRAME_M:
                    tile_index = tile_location_1;
                    if (animation_mode == AnimationMode.ONEFRAME_M)
                    {
                        counterpart.tile_index = tile_index;
                        counterpart.xflip = true;
                    }
                    else
                    {
                        counterpart.tile_index = (byte)(tile_location_1 + 2);
                    }
                    break;

                case AnimationMode.ONEFRAME_DU:
                    flip = facing_direction != Direction.UP;
                    if (flip)
                    {
                        tile_index = tile_location_1;
                        counterpart.tile_index = (byte)(tile_location_1 + next_tile);
                    }
                    else
                    {
                        tile_index = tile_location_2;
                        counterpart.tile_index = (byte)(tile_location_2 + next_tile);
                    }
                    break;

                case AnimationMode.ONEFRAME_DMUM:
                    counterpart.xflip = true;
                    flip = facing_direction == Direction.DOWN;
                    if (flip)
                    {
                        tile_index = tile_location_1;
                        counterpart.tile_index = (byte)(tile_location_1);
                    }
                    else
                    {
                        tile_index = tile_location_2;
                        counterpart.tile_index = (byte)(tile_location_2);
                    }
                    break;

                case AnimationMode.TWOFRAMES:
                    flip = FirstHalfOfAnimation();
                    if (flip)
                    {
                        tile_index = tile_location_1;
                        counterpart.tile_index = (byte)(tile_location_1 + next_tile);
                    }
                    else
                    {
                        tile_index = tile_location_2;
                        counterpart.tile_index = (byte)(tile_location_2 + next_tile);
                    }
                    break;

                case AnimationMode.TWOFRAMES_M:
                    counterpart.xflip = true;
                    flip = FirstHalfOfAnimation();
                    if (flip)
                    {
                        tile_index = tile_location_1;
                        counterpart.tile_index = (byte)(tile_location_1);
                    }
                    else
                    {
                        tile_index = tile_location_2;
                        counterpart.tile_index = (byte)(tile_location_2);
                    }
                    break;

                case AnimationMode.TWOFRAMES_RRDU:
                    flip = false;
                    tile_to_use = 1;
                    if (FirstHalfOfAnimation())
                    {
                        tile_to_use = -1;
                        flip = !flip;
                    }

                    if ((int)facing_direction < 2)
                    {
                        if (facing_direction == Direction.DOWN)
                        {
                            tile_index = (byte)(tile_location_2 + 1 - tile_to_use);
                            counterpart.tile_index = (byte)(tile_location_2 + 1 + tile_to_use);
                        }
                        else
                        {
                            tile_index = (byte)(tile_location_2 + 5 - tile_to_use);
                            counterpart.tile_index = (byte)(tile_location_2 + 5 + tile_to_use);
                        }
                        xflip = flip;
                        counterpart.xflip = flip;
                    }
                    else
                    {
                        flip = facing_direction == Direction.LEFT;
                        if (facing_direction == Direction.LEFT)
                            next_tile = 1;
                        else
                            next_tile = -1;

                        tile_to_use = 2 * (tile_to_use + 1);
                        tile_index = (byte)(tile_location_1 + 1 + next_tile + tile_to_use);
                        counterpart.tile_index = (byte)(tile_location_1 + 1 - next_tile + tile_to_use);
                        xflip = flip;
                        counterpart.xflip = flip;
                    }
                    break;

                case AnimationMode.TWOFRAMES_DDUU:
                    tile_to_use = 0;
                    if (FirstHalfOfAnimation())
                        tile_to_use = 4;

                    if (facing_direction == Direction.UP)
                        tile_to_use += 8;
                    
                    tile_index = (byte)(tile_location_1 + tile_to_use);
                    counterpart.tile_index = (byte)(tile_location_1 + 2 + tile_to_use);
                    break;

                case AnimationMode.TWOFRAMES_DMDMLL:
                    if (FirstHalfOfAnimation())
                        next_tile = 2;
                    else
                        next_tile = 0;

                    if ((int)facing_direction < 2)
                    {
                        if (facing_direction == Direction.UP)
                            flip = true;
                        else
                            flip = false;
                        yflip = flip;
                        counterpart.yflip = flip;

                        tile_index = (byte)(tile_location_1 + next_tile);
                        counterpart.tile_index = (byte)(tile_location_1 + next_tile);
                        counterpart.xflip = true;
                        xflip = false;
                    }
                    else
                    {
                        flip = facing_direction == Direction.RIGHT;
                        xflip = flip;
                        counterpart.xflip = flip;

                        if (facing_direction == Direction.LEFT)
                            tile_to_use = -1;
                        else
                            tile_to_use = 1;

                        tile_index = (byte)(tile_location_1 + 5 + tile_to_use + next_tile * 2);
                        counterpart.tile_index = (byte)(tile_location_1 + 5 - tile_to_use + next_tile * 2);
                    }
                    break;
            }
            bool FirstHalfOfAnimation()
            {
                if (frames_between_anim == 0)
                    return true;
                return local_timer % (frames_between_anim * 2) < frames_between_anim;
            }
        }
        public void FindValidSpawnLocation()
        {
            int new_x;
            int new_y;
            byte counter = 0;

            while (true)
            {
                new_x = Program.RNG.Next(2, 14);
                new_y = Program.RNG.Next(2, 9);

                byte tile = Screen.meta_tiles[new_y * 16 + new_x].tile_index;
                if (IsValidTile(tile))
                {
                    break;
                }

                counter++; // anti inf loop
                if (counter > 100)
                    break;
            }

            x = new_x * 16;
            y = new_y * 16 + 64;
        }
        bool IsValidTile(byte tile)
        {
            return tile == 1 || tile == 3 || (tile >= 0x14 && tile <= 0x16) || (tile >= 0x26 && tile <= 0x29);
        }
        public void Walk()
        {
            if (knockback_timer > 0)
                return;

            nb_of_times_moved++;
            if (nb_of_times_moved >= 16f / speed)
            {
                nb_of_times_moved = 0;
                SetPosMod16();
                if (Program.RNG.Next(4) == 0)
                {
                    facing_direction = PickDirection();
                }
                CheckIfTurn();
            }
            UpdateTarget();

            int frame_speed;
            if (speed % 1 == 0.5f)
            {
                if (local_timer % 2 == 0)
                    frame_speed = (int)MathF.Ceiling(speed);
                else
                    frame_speed = (int)MathF.Floor(speed);
            }
            else
                frame_speed = (int)speed;

            if (facing_direction == Direction.UP)
                y -= frame_speed;
            else if (facing_direction == Direction.DOWN)
                y += frame_speed;
            else if (facing_direction == Direction.LEFT)
                x -= frame_speed;
            else
                x += frame_speed;

            void UpdateTarget()
            {
                if (x % 16 == 0 && y % 16 == 0)
                {
                    if (Program.RNG.Next(64) == 0)
                        target_antilink = !target_antilink;
                }

                if (target_antilink)
                {
                    target_x = 256 - Link.x;
                    target_y = 240 - Link.y;
                }
                else
                {
                    target_x = Link.x;
                    target_y = Link.y;
                }
            }
            Direction PickDirection()
            {
                List<Direction> possible_directions = new List<Direction> { Direction.UP, Direction.DOWN, Direction.LEFT, Direction.RIGHT };
                //possible_directions.Remove(facing_direction);

                if (x < target_x)
                    possible_directions.Remove(Direction.LEFT);
                else
                    possible_directions.Remove(Direction.RIGHT);

                if (y < target_y)
                    possible_directions.Remove(Direction.UP);
                else
                    possible_directions.Remove(Direction.DOWN);

                if (possible_directions.Count == 0)
                    possible_directions.Add(facing_direction);
                return possible_directions[Program.RNG.Next(possible_directions.Count)];
            }
        }
        public bool CheckIfTurn()
        { // returns true if turned
            Direction rtrn_value = facing_direction;
            bool valid_direction = false;
            byte counter = 0;
            while (!valid_direction)
            {
                counter++;
                if (counter > 32) // prevents infinite loop. enemies walking in walls is better than a crash
                    break;

                int test_x = -1, test_y = -1;
                if (facing_direction == Direction.UP)
                    test_y = y - 8;
                else if (facing_direction == Direction.DOWN)
                    test_y = y + 24;
                else if (facing_direction == Direction.LEFT)
                    test_x = x - 8;
                else
                    test_x = x + 24;

                if (test_x == -1)
                    test_x = x + 8;
                else
                    test_y = y + 8;

                if (!IsPositionValid(test_x, test_y) || test_x < 0 || test_x > 256 || test_y < 64 || test_y > 240)
                    facing_direction = (Direction)(((int)facing_direction + Program.RNG.Next(1, 4)) % 4);
                else
                    valid_direction = true;
            }
            return facing_direction != rtrn_value;
        }
        void HitFlash()
        {
            byte new_palette;
            if (iframes_timer == 1)
                new_palette = og_palette;
            else
                new_palette = (byte)((Program.gTimer >> 1) % 4 + 4);

            palette_index = new_palette;
            counterpart.palette_index = new_palette;
            iframes_timer--;
        }
        void TouchingSword()
        {
            if (!Link.sword_out && !Link.wand_out)
                return;

            bool touching = false;
            if (Link.facing_direction == Direction.UP)
            {
                if (x > Link.x - 9 && x < Link.x + 9 && y > Link.y - 26 && y < Link.y + 8)
                    touching = true;
            }
            else if (Link.facing_direction == Direction.DOWN)
            {
                if (x > Link.x - 9 && x < Link.x + 9 && y > Link.y - 8 && y < Link.y + 26)
                    touching = true;
            }
            else if (Link.facing_direction == Direction.LEFT)
            {
                if (x > Link.x - 26 && x < Link.x + 8 && y > Link.y - 9 && y < Link.y + 9)
                    touching = true;
            }
            else
            {
                if (x > Link.x - 8 && x < Link.x + 26 && y > Link.y - 9 && y < Link.y + 9)
                    touching = true;
            }

            if (touching)
            {
                knockback_direction = (Direction)Link.facing_direction;
                byte damage = 1;
                if (SaveLoad.magical_sword[SaveLoad.current_save_file] || Link.wand_out) // wand whack does 4 damage
                    damage = 4;
                else if (SaveLoad.white_sword[SaveLoad.current_save_file])
                    damage = 2;
                TakeDamage(damage);
            }
        }
        public void TakeDamage(float damage_taken)
        {
            if (invincible || iframes_timer > 0)
                return;

            HP -= damage_taken;
            if (damage != 0)
            {
                iframes_timer = 48;
                if (((int)knockback_direction < 2 && x % 16 == 0) || ((int)knockback_direction >= 2 && y % 16 == 0)) // if aligned on grid
                    knockback_timer = 8;
            }
            og_palette = palette_index;
            if (HP <= 0)
            {
                local_timer = 0;
                appeared = false;
                knockback_timer = 0;
            }
        }
        public void Stunned()
        {
            if (time_when_stunned == -1)
                time_when_stunned = Program.gTimer;

            if (Program.gTimer >= time_when_stunned + 180 || !stunnable)
            {
                current_action = unstunned_action;
                time_when_stunned = -1;
                local_timer -= 180;
                return;
            }
        }
        public bool CheckIfEdgeHit()
        {
            return x >= 232 || x <= 8 || y <= 64 || y >= 216;
        }
        bool Knockback()
        {
            if (knockback_timer > 0)
            {
                int backup_x = x, backup_y = y; // setpos overwrites safex and safey, so we have to manually remember link's last non-wall pos
                knockback_timer--;

                if (knockback_direction == Direction.UP)
                    y -= 4;
                else if (knockback_direction == Direction.DOWN)
                    y += 4;
                else if (knockback_direction == Direction.LEFT)
                    x -= 4;
                else
                    x += 4;

                if (!Check() || y < 64 || y > 224 || x < 0 || x > 240)
                {
                    knockback_timer = 0;
                    x = backup_x;
                    y = backup_y;
                }
            }

            return knockback_timer > 0;

            bool Check()
            {
                return IsValidTile(Screen.GetTileIndexAtLocation(x, y)) && IsValidTile(Screen.GetTileIndexAtLocation(x + 15, y)) &&
                    IsValidTile(Screen.GetTileIndexAtLocation(x, y + 15)) && IsValidTile(Screen.GetTileIndexAtLocation(x + 15, y + 15));
            }
        }
        void DropItem()
        {
            if (Link.nb_of_ens_killed_damageless % 10 == 0 && Link.nb_of_ens_killed_damageless != 0)
            {
                new BombItemSprite(x + 4, y);
                return;
            }
            else if (Link.nb_of_ens_killed_damageless % 16 == 0 && Link.nb_of_ens_killed_damageless != 0)
            {
                new FairyItemSprite(x, y);
                return;
            }

            switch (drop_category)
            {
                case 0:
                    break;
                case 1:
                    if (Program.RNG.Next(100) <= 30)
                    {
                        switch (Link.nb_of_ens_killed % 10)
                        {
                            case 4:
                                new FairyItemSprite(x + 4, y);
                                break;
                            case 2 or 6 or 7 or 0:
                                new HeartItemSprite(x + 4, y);
                                break;
                            default:
                                new RupySprite(x + 4, y, false);
                                break;
                        }
                    }
                    break;
                case 2:
                    if (Program.RNG.Next(100) <= 40)
                    {
                        switch (Link.nb_of_ens_killed % 10)
                        {
                            case 3:
                                new ClockItemSprite(x, y);
                                break;
                            case 2 or 4 or 7:
                                new RupySprite(x + 4, y, false);
                                break;
                            case 1 or 6 or 8:
                                new BombItemSprite(x + 4, y);
                                break;
                            default:
                                new HeartItemSprite(x + 4, y);
                                break;
                        }
                    }
                    break;
                case 3:
                    if (Program.RNG.Next(100) <= 60)
                    {
                        switch (Link.nb_of_ens_killed % 10)
                        {
                            case 6:
                                new ClockItemSprite(x, y);
                                break;
                            case 2 or 5:
                                new HeartItemSprite(x + 4, y);
                                break;
                            case 4 or 0:
                                new RupySprite(x + 4, y, true);
                                break;
                            default:
                                new RupySprite(x + 4, y, false);
                                break;
                        }
                    }
                    break;
                case 4:
                    if (Program.RNG.Next(100) <= 40)
                    {
                        switch (Link.nb_of_ens_killed % 10)
                        {
                            case 2 or 5:
                                new FairyItemSprite(x + 4, y);
                                break;
                            case 3 or 9:
                                new RupySprite(x + 4, y, false);
                                break;
                            default:
                                new HeartItemSprite(x + 4, y);
                                break;
                        }
                    }
                    break;
            }
        }
        void Die()
        {
            byte new_palette = (byte)(local_timer % 4 + 4);
            palette_index = new_palette;
            counterpart.palette_index = new_palette;
            xflip = false;
            counterpart.xflip = true;

            if (local_timer <= 6 || local_timer >= 13)
            {
                tile_index = 0x62;
                counterpart.tile_index = 0x62;
            }
            else
            {
                tile_index = 0x64;
                counterpart.tile_index = 0x64;
            }

            if (local_timer == 20)
            {
                Link.nb_of_ens_killed++;
                Link.nb_of_ens_killed_damageless++;
                DropItem();
                Screen.sprites.Remove(counterpart);
                Screen.sprites.Remove(this);
            }
        }
    }
    internal class Octorok : Enemy
    {
        int when_to_shoot;
        public Octorok (bool stronger) : base(AnimationMode.TWOFRAMES_DMDMLL, 0xb0, 0xb2, stronger, true, 6, 0.5f, 1)
        {
            when_to_shoot = Program.RNG.Next(30, 100);
            damage = 0.5f;
            facing_direction = (Direction)Program.RNG.Next(4);
            CheckIfTurn();
            current_action = ActionState.WALKING;

            if (Program.RNG.Next(5) == 0)
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
                        when_to_shoot = Program.RNG.Next(180, 600);
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
            when_to_shoot = Program.RNG.Next(30, 60);
            damage = 0.5f;
            facing_direction = (Direction)Program.RNG.Next(4);
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

            if (OverworldCode.overworld_screens_side_entrance.Contains(OverworldCode.current_screen) && Program.gamemode == Program.Gamemode.OVERWORLD)
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
                    new ArrowSprite(x + 4, y, (Direction)facing_direction, false);
                }
                else if (local_timer >= 54)
                {
                    current_action = ActionState.WALKING;
                    local_timer = 0;
                    when_to_shoot = Program.RNG.Next(180, 600);
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
            when_to_shoot = Program.RNG.Next(30, 60);
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
            facing_direction = (Direction)Program.RNG.Next(4);
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
                    new SwordProjectileSprite(x + 4, y - 4, (Direction)facing_direction, false);
                }
                else if (local_timer >= 54)
                {
                    current_action = ActionState.WALKING;
                    local_timer = 0;
                    when_to_shoot = Program.RNG.Next(180, 600);
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

                new_x = Program.RNG.Next(0, 16) * 16;
                new_y = Program.RNG.Next(4, 15) * 16;
                if (IsTileValid(Screen.GetTileIndexAtLocation(new_x, new_y)))
                {
                    valid_location_found = true;
                    x = new_x;
                    y = new_y;
                }
            }
        }
        bool IsTileValid(int index)
        {
            return index >= 0xa && index <= 0x12;
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
                        if (Program.RNG.Next(30) != 0)
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
        bool IsValidTile(byte tile)
        {
            return tile == 1 || tile == 0x16;
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
                    new_x = Program.RNG.Next(1, 15) * 16;
                    new_y = Program.RNG.Next(5, 14) * 16;
                    if (IsValidTile(Screen.GetTileIndexAtLocation(new_x, new_y)))
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

                if (!(Control.IsHeld(Control.Buttons.UP) || Control.IsHeld(Control.Buttons.DOWN) ||
                    Control.IsHeld(Control.Buttons.LEFT) || Control.IsHeld(Control.Buttons.RIGHT)))
                {
                    new_x = -new_x;
                    new_y = -new_y;
                }

                if (!IsValidTile(Screen.GetTileIndexAtLocation(new_x + Link.x, new_y + Link.y)) || IsOOB(new_x + Link.x, new_y + Link.y))
                {
                    new_x = -new_x;
                    new_y = -new_y;
                    if (!IsValidTile(Screen.GetTileIndexAtLocation(new_x + Link.x, new_y + Link.y)) || IsOOB(new_x + Link.x, new_y + Link.y))
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
                        if (!IsValidTile(Screen.GetTileIndexAtLocation(new_x + Link.x, new_y + Link.y)) || IsOOB(new_x + Link.x, new_y + Link.y))
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
            go_left = Convert.ToBoolean(Program.RNG.Next(2));
            Spawn();
        }
        public override void EnemySpecificActions()
        {
            switch (current_action)
            {
                case ActionState.RESTING:
                    if (local_timer == 1)
                    {
                        when_to_drop = Program.RNG.Next(2, 64);
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
                        when_to_drop = Program.RNG.Next(2, 120);
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
            x = Program.RNG.Next(1, 239);
        }
        void PickNewDirection()
        {
            bool value = Convert.ToBoolean(Program.RNG.Next(5));
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
                        jump_height = Program.RNG.Next(20, 45);
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
                                jump_height = Program.RNG.Next(40, 50);
                            else
                                jump_height = Program.RNG.Next(10, 20);
                            break;
                        }

                        local_timer = 0;
                        if (!second_jump && Program.RNG.Next(2) == 0 && !stronger)
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
                            when_to_stop = Program.RNG.Next(45, 90);
                        else
                            when_to_stop = Program.RNG.Next(90, 180);
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
            bool value = Convert.ToBoolean(Program.RNG.Next(7));
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
                        if ((local_timer <= 24 && local_timer % 4 == 0) || (local_timer > 24 && local_timer % 8 == 0))
                        {
                            Move8D();
                        }
                        break;
                    }

                    if (local_timer == 49)
                    {
                        frames_between_anim = 0;
                        invincible = false;
                        when_to_stop = Program.RNG.Next(120, 240);
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
                        direction = (EightDirection)Program.RNG.Next(8);
                        when_to_stop = Program.RNG.Next(60, 120);
                        frames_between_anim = 2;
                    }

                    if (local_timer <= 54)
                    {
                        if ((local_timer <= 24 && local_timer % 8 == 0) || (local_timer > 24 && local_timer % 4 == 0))
                            Move8D();

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

                for (int i = 0; i < Screen.sprites.Count; i++)
                {
                    if (Screen.sprites[i] is Ghini)
                    {
                        Ghini spr = (Ghini)Screen.sprites[i];
                        if (spr.is_master)
                        {
                            this.master = spr;
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
                        if ((local_timer <= 24 && local_timer % 4 == 0) || (local_timer > 24 && local_timer % 8 == 0))
                        {
                            Move8D();
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
                        frames_between_anim = 8;
                    }
                    else if (local_timer == 24)
                    {
                        frames_between_anim = 4;
                    }
                    else if (local_timer == 55)
                    {
                        direction = (EightDirection)Program.RNG.Next(8);
                        when_to_stop = Program.RNG.Next(60, 120);
                        frames_between_anim = 2;
                    }

                    if (local_timer <= 54)
                    {
                        if ((local_timer <= 24 && local_timer % 8 == 0) || (local_timer > 24 && local_timer % 4 == 0))
                            Move8D();

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
            speed = Program.RNG.Next(1, 5) / 2f;
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
            int ppu_tile_location = 256 + (metatile_index >> 4) * 64 + (metatile_index % 16) * 2;
            if (OverworldCode.current_screen == 36 && !SaveLoad.power_bracelet[SaveLoad.current_save_file] && metatile_index == 78)
                new PowerBraceletSprite(x + 4, y);
            byte screen = OverworldCode.current_screen;
            if ((screen == 11 && metatile_index == 75) || (screen == 34 && metatile_index == 67) || (screen == 28 && metatile_index == 75) ||
                (screen == 52 && metatile_index == 68) || (screen == 61 && metatile_index == 73) || (screen == 78 && metatile_index == 74))
            {
                Screen.meta_tiles[metatile_index].tile_index = 0x15;
                Textures.ppu[ppu_tile_location] = 0x70;
                Textures.ppu[ppu_tile_location + 1] = 0x72;
                Textures.ppu[ppu_tile_location + 32] = 0x71;
                Textures.ppu[ppu_tile_location + 33] = 0x73;
                //TODO: play secret found sfx
            }
            else
            {
                Screen.meta_tiles[metatile_index].tile_index = 0x1;
                Textures.ppu[ppu_tile_location] = 0x26;
                Textures.ppu[ppu_tile_location + 1] = 0x26;
                Textures.ppu[ppu_tile_location + 32] = 0x26;
                Textures.ppu[ppu_tile_location + 33] = 0x26;
            }
        }
    }
}