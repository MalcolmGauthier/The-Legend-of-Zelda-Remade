using static The_Legend_of_Zelda.Gameplay.Program;
using The_Legend_of_Zelda.Rendering;

namespace The_Legend_of_Zelda.Sprites
{
    internal abstract class Enemy : Sprite
    {
        // tells the animation code how to read the sprite data memory
        public enum AnimationMode
        {
            ONEFRAME,         // bubble
            ONEFRAME_M,       // blade trap
            ONEFRAME_DU,      // ghini
            ONEFRAME_DMUM,    // zora

            TWOFRAMES,        // rock, rope
            TWOFRAMES_S,      // stalfos, gibdo
            TWOFRAMES_M,      // leever, peahat, tektite, polsvoice, zol, keese
            TWOFRAMES_HM,     // wallmaster
            TWOFRAMES_SOLO,   // gel, moldorm
            TWOFRAMES_RRDU,   // lynel, moblin
            TWOFRAMES_RRUU,   // wizrobes
            TWOFRAMES_DURR,   // goriya
            TWOFRAMES_DMUM,   // vire
            TWOFRAMES_DDUU,   // armos
            TWOFRAMES_DDUU_M, // vire
            TWOFRAMES_DMDMLL, // octorok
            TWOFRAMES_DDURR,  // darknut

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

        const int NOT_STUNNED = -1;

        protected int local_timer = 0;
        protected int target_x;
        protected int target_y;
        int time_when_stunned = NOT_STUNNED;

        protected bool pause_animation = false;
        protected bool can_damage_link = true;
        protected bool appeared = false;
        protected bool smoke_appearance = false;
        protected bool stronger = false;
        protected bool spawn_hidden = false;
        protected bool stunnable = true;
        protected bool die_when_stunned = false;
        public bool invincible = false;
        public bool bomb_death = false;
        bool target_antilink = false;

        protected byte smoke_random_appearance = 1;
        protected byte frames_between_anim = 0;
        protected byte iframes_timer = 0;
        protected byte knockback_timer = 0;
        protected byte drop_category = 0;
        protected byte tile_location_1 = 0;
        protected byte tile_location_2 = 0;
        byte nb_of_times_moved = 0;
        byte og_palette;

        protected float HP = 0;
        protected float speed = 0;
        protected float damage = 0;

        protected Direction facing_direction;
        protected Direction knockback_direction;
        public ActionState current_action = ActionState.DEFAULT;
        public ActionState unstunned_action = ActionState.DEFAULT;
        AnimationMode animation_mode;

        protected StaticSprite counterpart = new StaticSprite(SpriteID.BLANK, 0, 0, 0);

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
            target_antilink = RNG.Next() % 2 == 0;

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

            OnInit();
            Animation();
        }

        protected virtual void OnInit()
        {
            return;
        }

        public override void Action()
        {
            // only do stuff if link can move!!! (and he's not on a raft)
            if (!Link.can_move && !OC.raft_flag)
                return;

            // not being appeared either means you're smoking or dying
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
                if (animation_mode is not (AnimationMode.ONEFRAME or AnimationMode.ONEFRAME_M))
                {
                    Animation();
                }

                if (IsWithinLink() && !Link.clock_flash && can_damage_link)
                {
                    Link.knockback_direction = Link.facing_direction.Opposite();
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

        public abstract void EnemySpecificActions();

        bool IsPositionValid(int x, int y)
        {
            MetatileType tile_id = Screen.GetMetaTileTypeAtLocation(x, y);

            if (gamemode == Gamemode.DUNGEON)
            {
                return IsValidTile((DungeonMetatile)tile_id);
            }

            return IsValidTile(tile_id);
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
                x += 16 - x % 16;

            if (y % 16 < 8)
                y -= y % 16;
            else
                y += 16 - y % 16;
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
                if (OC.overworld_screens_side_entrance.Contains(OC.current_screen) && gamemode == Gamemode.OVERWORLD)
                {
                    SpawnOnEdge();
                    shown = false;
                    counterpart.shown = false;
                }
                smoke_random_appearance = (byte)RNG.Next(10, 90);
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
            int count = 0;
            bool care_about_dir = true;
            while (true)
            {
                count++;

                // after 100 tries, stop caring about if the enemies spawn on the same side as link
                if (count == 100)
                {
                    care_about_dir = false;
                }
                // after 200 tries, give up entirely and kill the enemy
                else if (count > 200)
                {
                    HP = 0;
                    return;
                }

                switch ((Direction)RNG.Next(4))
                {
                    case Direction.UP:
                        x = RNG.Next(1, 15) * 16;
                        y = 64;
                        facing_direction = Direction.DOWN;
                        break;
                    case Direction.DOWN:
                        x = RNG.Next(1, 15) * 16;
                        y = 224;
                        facing_direction = Direction.UP;
                        break;
                    case Direction.LEFT:
                        x = 0;
                        y = RNG.Next(6, 14) * 16;
                        facing_direction = Direction.RIGHT;
                        break;
                    case Direction.RIGHT:
                        x = 240;
                        y = RNG.Next(6, 14) * 16;
                        facing_direction = Direction.LEFT;
                        break;
                }

                if (IsValidTile(Screen.GetMetaTileTypeAtLocation(x, y)) && !IsWithinLink() &&
                    (care_about_dir ? facing_direction != Link.facing_direction : true))
                    break;
            }
        }

        void Animation()
        {
            bool flip;
            int next_tile = 2;
            int tile_to_use;

            switch (animation_mode)
            {
                //TODO: one frame anims don't need updates
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
                        counterpart.tile_index = tile_location_1;
                    }
                    else
                    {
                        tile_index = tile_location_2;
                        counterpart.tile_index = tile_location_2;
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

                case AnimationMode.TWOFRAMES_HM:
                    tile_to_use = tile_location_1 + next_tile;
                    if (FirstHalfOfAnimation())
                    {
                        tile_to_use = tile_location_2;
                    }
                    tile_index = tile_location_1;
                    counterpart.tile_index = (byte)tile_to_use;
                    break;

                case AnimationMode.TWOFRAMES_S:
                    flip = FirstHalfOfAnimation();
                    if (flip)
                    {
                        tile_index = tile_location_1;
                        counterpart.tile_index = (byte)(tile_location_1 + next_tile);
                    }
                    else
                    {
                        tile_index = (byte)(tile_location_1 + next_tile);
                        counterpart.tile_index = tile_location_1;
                    }
                    xflip = !flip;
                    counterpart.xflip = !flip;
                    break;

                case AnimationMode.TWOFRAMES_SOLO:
                    if (FirstHalfOfAnimation())
                    {
                        tile_index = tile_location_1;
                    }
                    else
                    {
                        tile_index = tile_location_2;
                    }
                    break;

                case AnimationMode.TWOFRAMES_M:
                    counterpart.xflip = true;
                    flip = FirstHalfOfAnimation();
                    if (flip)
                    {
                        tile_index = tile_location_1;
                        counterpart.tile_index = tile_location_1;
                    }
                    else
                    {
                        tile_index = tile_location_2;
                        counterpart.tile_index = tile_location_2;
                    }
                    break;

                case AnimationMode.TWOFRAMES_RRDU:
                    flip = FirstHalfOfAnimation();
                    tile_to_use = 1;
                    if (flip)
                    {
                        tile_to_use = -1;
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

                case AnimationMode.TWOFRAMES_DURR:
                    flip = FirstHalfOfAnimation();
                    switch (facing_direction)
                    {
                        case Direction.DOWN:
                            if (flip)
                            {
                                tile_index = tile_location_1;
                                counterpart.tile_index = (byte)(tile_location_1 + next_tile);
                            }
                            else
                            {
                                tile_index = (byte)(tile_location_1 + next_tile);
                                counterpart.tile_index = tile_location_1;
                            }
                            xflip = !flip;
                            counterpart.xflip = !flip;
                            break;

                        case Direction.UP:
                            if (flip)
                            {
                                tile_index = (byte)(tile_location_1 + 4);
                                counterpart.tile_index = (byte)(tile_location_1 + next_tile + 4);
                            }
                            else
                            {
                                tile_index = (byte)(tile_location_1 + next_tile + 4);
                                counterpart.tile_index = (byte)(tile_location_1 + 4);
                            }
                            xflip = !flip;
                            counterpart.xflip = !flip;
                            break;

                        case Direction.LEFT:
                            if (flip)
                            {
                                tile_index = (byte)(tile_location_2 + next_tile);
                                counterpart.tile_index = (byte)(tile_location_2);
                            }
                            else
                            {
                                tile_index = (byte)(tile_location_2 + next_tile + 4);
                                counterpart.tile_index = (byte)(tile_location_2 + 4);
                            }
                            xflip = true;
                            counterpart.xflip = true;
                            break;

                        case Direction.RIGHT:
                            if (flip)
                            {
                                tile_index = (byte)(tile_location_2);
                                counterpart.tile_index = (byte)(tile_location_2 + next_tile);
                            }
                            else
                            {
                                tile_index = (byte)(tile_location_2 + 4);
                                counterpart.tile_index = (byte)(tile_location_2 + next_tile + 4);
                            }
                            xflip = false;
                            counterpart.xflip = false;
                            break;
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

            int counter = 0;
            while (true)
            {
                new_x = RNG.Next(2, 14);
                new_y = RNG.Next(2, 9);

                MetatileType tile = Screen.meta_tiles[new_y * 16 + new_x].tile_index;
                if (gamemode == Gamemode.OVERWORLD ? IsValidTile(tile) : IsValidTile((DungeonMetatile)tile))
                {
                    break;
                }

                // anti inf loop
                counter++;
                if (counter > 100)
                    break;
            }

            x = new_x * 16;
            y = new_y * 16 + 64;
        }

        bool IsValidTile(MetatileType tile)
        {
            return tile is (MetatileType.GROUND or MetatileType.BLACK_SQUARE_WARP or MetatileType.DOCK or MetatileType.STAIRS or MetatileType.SAND) ||
                (tile >= MetatileType.WATER_INNER_TR && tile <= MetatileType.WATER_INNER_BR);
        }
        bool IsValidTile(DungeonMetatile tile)
        {
            // left and right doors
            // ideally the tiles used by the side doors would be different... oh well
            if (tile is DungeonMetatile.ROOM_TOP)
            {
                return y < 140;
            }

            return tile is (DungeonMetatile.GROUND or DungeonMetatile.SAND or DungeonMetatile.VOID or DungeonMetatile.STAIRS or 
                DungeonMetatile.ROOM_TOP or DungeonMetatile.TOP_DOOR_OPEN_L or DungeonMetatile.TOP_DOOR_OPEN_R);
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
                // 1/4 chance to roll for direction change towards target
                if (RNG.Next(4) == 0)
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

                    facing_direction = possible_directions[RNG.Next(possible_directions.Count)];
                }
                CheckIfTurn();
            }

            // every time an enemy moves 1 tile, it has a 1/64 chance of switching its target from link to anti-link and vice versa
            if (x % 16 == 0 && y % 16 == 0)
            {
                if (RNG.Next(64) == 0)
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

            int frame_speed;
            if (speed % 1 == 0.5f)
            {
                if (local_timer % 2 == 0)
                    frame_speed = (int)MathF.Ceiling(speed);
                else
                    frame_speed = (int)MathF.Floor(speed);
            }
            else
            {
                frame_speed = (int)speed;
            }

            if (facing_direction == Direction.UP)
                y -= frame_speed;
            else if (facing_direction == Direction.DOWN)
                y += frame_speed;
            else if (facing_direction == Direction.LEFT)
                x -= frame_speed;
            else
                x += frame_speed;
        }

        // returns true if turned
        public bool CheckIfTurn()
        {
            Direction rtrn_value = facing_direction;
            bool valid_direction = false;
            byte counter = 0;
            while (!valid_direction)
            {
                // prevents infinite loop. enemies walking in walls is better than a crash
                counter++;
                if (counter > 32)
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
                    facing_direction = (Direction)(((int)facing_direction + RNG.Next(1, 4)) % 4);
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
                new_palette = (byte)((gTimer >> 1) % 4 + 4);

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
                knockback_direction = Link.facing_direction;
                byte damage = 1;
                // wand whack does 4 damage
                if (SaveLoad.magical_sword || Link.wand_out)
                    damage = 4;
                else if (SaveLoad.white_sword)
                    damage = 2;
                TakeDamage(damage);
            }
        }

        public void TakeDamage(float damage_taken)
        {
            if (invincible || iframes_timer > 0)
                return;

            // parry this mf
            if (this is Darknut && knockback_direction == facing_direction.Opposite())
            {
                Sound.PlaySFX(Sound.SoundEffects.BLOCK);
                return;
            }

            HP -= damage_taken;
            if (damage != 0)
            {
                iframes_timer = 48;
                // if aligned on grid
                if ((int)knockback_direction < 2 && x % 16 == 0 || (int)knockback_direction >= 2 && y % 16 == 0)
                    knockback_timer = 8;
            }
            og_palette = palette_index;

            if (HP <= 0)
            {
                local_timer = 0;
                appeared = false;
                knockback_timer = 0;
                OnDeath();
            }
        }

        public void Stunned()
        {
            const int FRAME_STUNNED = 180;

            if (time_when_stunned == NOT_STUNNED)
                time_when_stunned = gTimer;

            if (die_when_stunned)
            {
                HP = 0;
                appeared = false;
                local_timer = 0;
                return;
            }

            if (gTimer >= time_when_stunned + FRAME_STUNNED || !stunnable)
            {
                current_action = unstunned_action;
                time_when_stunned = NOT_STUNNED;
                local_timer -= FRAME_STUNNED;
                return;
            }
        }

        public bool CheckIfEdgeHit()
        {
            return x >= 232 || x <= 8 || y <= 64 || y >= 216;
        }

        bool Knockback()
        {
            if (knockback_timer <= 0)
                return false;

            // setpos overwrites safex and safey, so we have to manually remember link's last non-wall pos
            int backup_x = x, backup_y = y;
            knockback_timer--;

            if (knockback_direction == Direction.UP)
                y -= 4;
            else if (knockback_direction == Direction.DOWN)
                y += 4;
            else if (knockback_direction == Direction.LEFT)
                x -= 4;
            else
                x += 4;

            if (!(IsValidTile((DungeonMetatile)Screen.GetMetaTileTypeAtLocation(x, y)) && IsValidTile((DungeonMetatile)Screen.GetMetaTileTypeAtLocation(x + 15, y)) &&
                  IsValidTile((DungeonMetatile)Screen.GetMetaTileTypeAtLocation(x, y + 15)) && IsValidTile((DungeonMetatile)Screen.GetMetaTileTypeAtLocation(x + 15, y + 15)))
                || y < 64 || y > 224 || x < 0 || x > 240)
            {
                knockback_timer = 0;
                x = backup_x;
                y = backup_y;
            }

            return knockback_timer > 0;
        }

        void DropItem()
        {
            // every tenth enemy killed damageless drops a bomb (if killed with bomb)!
            if (Link.nb_of_ens_killed_damageless % 10 == 0 && Link.nb_of_ens_killed_damageless != 0 && bomb_death)
            {
                new BombItemSprite(x + 4, y);
                return;
            }
            // every 16th enemy killed damageless drops a fairy!
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
                    if (RNG.Next(100) > 30)
                        break;

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
                    break;
                case 2:
                    if (RNG.Next(100) > 40)
                        break;

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
                    break;
                case 3:
                    if (RNG.Next(100) > 60)

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
                    break;
                case 4:
                    if (RNG.Next(100) > 40)
                        break;

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

            if (local_timer >= 20)
            {
                if (this is not Zora)
                {
                    if (gamemode == Gamemode.OVERWORLD)
                        OC.AddToKillQueue(OC.current_screen);
                    else
                        DC.AddToKillQueue(DC.current_screen);
                }
                Link.nb_of_ens_killed++;
                Link.nb_of_ens_killed_damageless++;
                DC.nb_enemies_alive--;
                DropItem();
                Screen.sprites.Remove(counterpart);
                Screen.sprites.Remove(this);
            }
        }

        protected virtual void OnDeath()
        {
            return;
        }
    }
}