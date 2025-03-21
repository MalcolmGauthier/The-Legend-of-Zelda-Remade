using System;
using The_Legend_of_Zelda.Gameplay;
using The_Legend_of_Zelda.Rendering;
using static The_Legend_of_Zelda.Gameplay.Program;

namespace The_Legend_of_Zelda.Sprites
{
    internal abstract class Boss : Enemy
    {
        protected StaticSprite[] body = [];
        protected EightDirection facing_direction_8d;

        protected int turnaround_timer_min = 0;
        protected int turnaround_timer_max = 0;
        int turnaround_timer_8d = 0;

        public Boss(byte drop_category) : base(AnimationMode.ONEFRAME, 0, 0, false, false, 0, 0, drop_category, true)
        {
            tile_index = (byte)SpriteID.BLANK;
            counterpart.tile_index = (byte)SpriteID.BLANK;
            tile_location_1 = tile_index;
            tile_location_2 = tile_index;
            stunnable = false;
        }

        protected override void OnDeath()
        {
            shown = true;
            counterpart.shown = true;

            SaveLoad.SetBossKillsFlag((byte)Array.IndexOf(DC.rooms_with_bosses, DC.current_screen), true);

            foreach (Sprite s in body)
                Screen.sprites.Remove(s);
        }

        protected void ShootBalls(Direction dir)
        {
            Sprite[] s = new Sprite[3];

            if (dir is Direction.LEFT or Direction.RIGHT)
            {
                s[0] = new MagicOrbProjectileSprite(x + 8, y, 0);
                s[1] = new MagicOrbProjectileSprite(x + 8, y, 1);
                s[2] = new MagicOrbProjectileSprite(x + 8, y, -1);
            }
        }

        protected void Move8D(float speed)
        {
            if (--turnaround_timer_8d <= 0)
            {
                facing_direction_8d = PickNewDirection();
                turnaround_timer_8d = RNG.Next(turnaround_timer_min, turnaround_timer_max);
            }

            if (speed % 1 != 0)
            {
                float remainder = speed % 1;

                // calculate the cycle length and when to add an extra speed unit
                int cycleLength = (int)Math.Round(1f / remainder);
                if (local_timer % cycleLength < remainder * cycleLength)
                    speed = (int)Math.Ceiling(speed);
                else
                    speed = (int)Math.Floor(speed);
            }

            if (facing_direction_8d == EightDirection.UP || facing_direction_8d == EightDirection.UPLEFT ||
                facing_direction_8d == EightDirection.UPRIGHT)
            {
                y -= (int)speed;
            }
            else if (facing_direction_8d == EightDirection.DOWN || facing_direction_8d == EightDirection.DOWNLEFT ||
                facing_direction_8d == EightDirection.DOWNRIGHT)
            {
                y += (int)speed;
            }

            if (facing_direction_8d == EightDirection.LEFT || facing_direction_8d == EightDirection.UPLEFT ||
                facing_direction_8d == EightDirection.DOWNLEFT)
            {
                x -= (int)speed;
            }
            else if (facing_direction_8d == EightDirection.RIGHT || facing_direction_8d == EightDirection.UPRIGHT ||
                facing_direction_8d == EightDirection.DOWNRIGHT)
            {
                x += (int)speed;
            }

            CheckBounds();
        }

        EightDirection PickNewDirection()
        {
            EightDirection return_val = facing_direction_8d;
            int rng = Program.RNG.Next(3);

            if (rng == 2)
                return return_val;

            if (facing_direction_8d == EightDirection.UP)
                return rng == 0 ? EightDirection.UPLEFT : EightDirection.UPRIGHT;
            if (facing_direction_8d == EightDirection.DOWN)
                return rng == 0 ? EightDirection.DOWNLEFT : EightDirection.DOWNRIGHT;
            if (facing_direction_8d == EightDirection.LEFT)
                return rng == 0 ? EightDirection.UPLEFT : EightDirection.DOWNLEFT;
            if (facing_direction_8d == EightDirection.RIGHT)
                return rng == 0 ? EightDirection.UPRIGHT : EightDirection.DOWNRIGHT;
            if (facing_direction_8d == EightDirection.UPLEFT)
                return rng == 0 ? EightDirection.UP : EightDirection.LEFT;
            if (facing_direction_8d == EightDirection.DOWNLEFT)
                return rng == 0 ? EightDirection.DOWN : EightDirection.LEFT;
            if (facing_direction_8d == EightDirection.UPRIGHT)
                return rng == 0 ? EightDirection.UP : EightDirection.RIGHT;
            else
                return rng == 0 ? EightDirection.RIGHT : EightDirection.DOWN;
        }

        protected virtual void CheckBounds()
        {
            if (x <= 32 || x >= 224 - width || y <= 96 || y >= 208 - height)
            {
                // the math works out.
                if (facing_direction_8d < (EightDirection)4)
                    facing_direction_8d ^= (EightDirection)1;
                else
                    facing_direction_8d ^= (EightDirection)0b11;

                // don't call move8d(1) because that contains this method and would throw stackoverflow exception
                if (facing_direction_8d == EightDirection.UP || facing_direction_8d == EightDirection.UPLEFT ||
                facing_direction_8d == EightDirection.UPRIGHT)
                {
                    y--;
                }
                else if (facing_direction_8d == EightDirection.DOWN || facing_direction_8d == EightDirection.DOWNLEFT ||
                    facing_direction_8d == EightDirection.DOWNRIGHT)
                {
                    y++;
                }

                if (facing_direction_8d == EightDirection.LEFT || facing_direction_8d == EightDirection.UPLEFT ||
                    facing_direction_8d == EightDirection.DOWNLEFT)
                {
                    x--;
                }
                else if (facing_direction_8d == EightDirection.RIGHT || facing_direction_8d == EightDirection.UPRIGHT ||
                    facing_direction_8d == EightDirection.DOWNRIGHT)
                {
                    x++;
                }
            }
        }
    }


    internal class Aquamentus : Boss
    {
        int turnaround_timer = 0;
        int shooting_timer = 0;

        public Aquamentus() : base(4)
        {
            smoke_appearance = true;
            instant_smoke = true;
            HP = 6;
            this.speed = 0.125f;
            this.body = new StaticSprite[6];

            Palettes.LoadPaletteGroup(PaletteID.SP_3, Palettes.PaletteGroups.AQUAMENTUS);
            palette_index = (byte)PaletteID.SP_3;
            facing_direction = Program.RNG.Next(2) == 0 ? Direction.LEFT : Direction.RIGHT;

            x = 176;
            y = 128;
            body[0] = new(0xc0, (byte)PaletteID.SP_3, 0, 0);
            body[1] = new(0xc4, (byte)PaletteID.SP_3, 0, 0);
            body[2] = new(0xc8, (byte)PaletteID.SP_3, 0, 0);
            body[3] = new(0xce, (byte)PaletteID.SP_3, 0, 0);
            body[4] = new(0xd0, (byte)PaletteID.SP_3, 0, 0);
            body[5] = new(0xd2, (byte)PaletteID.SP_3, 0, 0);
            foreach (Sprite s in body)
            {
                s.unload_during_transition = true;
            }
        }

        protected override void EnemySpecificActions()
        {
            if (local_timer % 8 == 0)
            {
                if (facing_direction == Direction.LEFT)
                    x--;
                else
                    x++;
            }

            turnaround_timer--;
            if (turnaround_timer <= 0)
            {
                turnaround_timer = Program.RNG.Next(60, 240);
                if (x < 176)
                    facing_direction = Direction.RIGHT;
                else
                    facing_direction = Direction.LEFT;
            }

            shooting_timer--;
            if (shooting_timer <= 0)
            {
                body[0].tile_index = 0xcc;
                shooting_timer = Program.RNG.Next(90, 240);
                ShootBalls(Direction.LEFT);
            }
            else if (shooting_timer == 32)
            {
                body[0].tile_index = 0xc0;
            }

            for (int i = 0; i < body.Length; i++)
            {
                body[i].x = x + (i % 3) * 8;
                body[i].y = y + (i / 3) * 16;
                body[i].palette_index = palette_index;
            }
        }

        protected override void Animation()
        {
            if (local_timer % 32 == 0)
            {
                body[3].tile_index = 0xce;
                body[4].tile_index = 0xd0;
                body[5].tile_index = 0xd2;
            }
            else if (local_timer % 32 == 16)
            {
                body[3].tile_index = 0xc2;
                body[4].tile_index = 0xc6;
                body[5].tile_index = 0xca;
            }
        }

        protected override void OnInit()
        {
            shown = false;
            counterpart.shown = false;
            for (int i = 0; i < body.Length; i++)
            {
                body[i].x = x + (i % 3) * 8;
                body[i].y = y + (i / 3) * 16;
                body[i].palette_index = palette_index;
                Screen.sprites.Add(body[i]);
            }
        }
    }

    internal class Dodongo : Boss
    {
        static HashSet<BombSprite> bombs_on_screen = new();
        static int bomb_set_lock = 0;

        int real_hp;
        bool triple;

        public Dodongo(bool triple = false, int custom_x = 80, int custom_y = 160): base(4)
        {
            smoke_appearance = true;
            instant_smoke = true;
            HP = 1;
            stunnable = false;
            invincible = true;
            frames_between_anim = 8;
            real_hp = 3;
            speed = 0.5f;
            body = new StaticSprite[4];

            Palettes.LoadPaletteGroup(PaletteID.SP_3, Palettes.PaletteGroups.DODONGO);
            palette_index = (byte)PaletteID.SP_3;
            current_action = ActionState.WALKING;
            facing_direction = Program.RNG.Next(2) == 0 ? Direction.UP : Direction.RIGHT;

            x = custom_x;
            y = custom_y;
            this.triple = triple;
            SetCustomSize(16, 8);
            bombs_on_screen.Clear();
            for (int i = 0; i < body.Length; i++)
            {
                body[i] = new(0, PaletteID.SP_3, x + i * 8, y);
                body[i].unload_during_transition = true;
            }

            this.triple = triple;
        }

        protected override void EnemySpecificActions()
        {
            switch (current_action)
            {
                case ActionState.WALKING:
                    Walk();
                    CheckForBombs();
                    break;

                // still
                case ActionState.DEFAULT:
                    if (local_timer >= 32)
                    {
                        current_action = ActionState.FLYING;
                        frames_between_anim = 0;
                        local_timer = 0;
                    }
                    break;

                // boom
                case ActionState.FLYING:
                    if (local_timer == 1)
                    {
                        switch (facing_direction)
                        {
                            case Direction.UP:
                                body[0].tile_index = 0xfe;
                                body[1].tile_index = 0xfe;
                                body[1].xflip = true;
                                break;
                            case Direction.DOWN:
                                body[0].tile_index = 0xf8;
                                body[1].tile_index = 0xf8;
                                body[1].xflip = true;
                                break;
                            case Direction.LEFT:
                                body[3].tile_index = 0xec;
                                body[2].tile_index = 0xee;
                                body[1].tile_index = 0xf0;
                                body[0].tile_index = 0xf2;
                                // no need to set xflip because left walk anim already does that
                                break;
                            case Direction.RIGHT:
                                body[0].tile_index = 0xec;
                                body[1].tile_index = 0xee;
                                body[2].tile_index = 0xf0;
                                body[3].tile_index = 0xf2;
                                break;
                        }
                    }

                    if (local_timer >= 65)
                    {
                        if (real_hp <= 0)
                        {
                            current_action = ActionState.BURROWING;
                            local_timer = 0;
                            frames_between_anim = 1;
                            Animation();
                            return;
                        }

                        current_action = ActionState.WALKING;
                        frames_between_anim = 8;
                        Animation();
                    }
                    break;

                // dying
                case ActionState.BURROWING:
                    if (local_timer % 4 == 0)
                        body[0].shown = true;
                    else if (local_timer % 4 == 2)
                        body[0].shown = false;

                    body[1].shown = body[0].shown;
                    body[2].shown = body[0].shown;
                    body[3].shown = body[0].shown;

                    if (local_timer >= 64)
                    {
                        HP = 0;
                        appeared = false;
                        local_timer = 0;
                        OnDeath();
                    }
                    break;

                // stunned from smoke
                case ActionState.RESTING:
                    if (local_timer >= 330)
                    {
                        current_action = ActionState.WALKING;
                        frames_between_anim = 8;
                        invincible = true;
                    }
                    break;
            }

            for (int i = 0; i < body.Length; i++)
            {
                body[i].y = y;
                body[i].x = x + i * 8;
            }
        }

        void CheckForBombs()
        {
            // update bomb list to keep track of bombs
            if (bomb_set_lock != Program.gTimer)
            {
                // add new bombs
                foreach (Sprite s in Screen.sprites)
                {
                    if (s is BombSprite b)
                    {
                        bombs_on_screen.Add(b);
                    }
                }

                // remove deleted bombs
                List<BombSprite> l = bombs_on_screen.ToList();
                for (int i = 0; i < l.Count; i++)
                {
                    if (!Screen.sprites.Contains(l[i]))
                    {
                        bombs_on_screen.Remove(l[i]);
                        l.RemoveAt(i);
                        i--;
                    }
                }

                bomb_set_lock = Program.gTimer;
            }

            bool bomb_eaten = false;

            foreach (BombSprite bomb in bombs_on_screen)
            {
                // dodongo dislikes smoke.
                if (bomb.explosion_timer == 37)
                {
                    if (Math.Abs(bomb.x - x + 8) <= 8 && Math.Abs(bomb.y - y) <= 8)
                    {
                        current_action = ActionState.RESTING;
                        local_timer = 0;
                        frames_between_anim = 32;
                        invincible = false;
                        return;
                    }

                    continue;
                }

                // bomb fully exploded
                if (bomb.explosion_timer < 37)
                {
                    continue;
                }

                // dodongo eating bomb
                switch (facing_direction)
                {
                    case Direction.UP:
                        if (bomb.x >= x && bomb.x <= x + 8 && bomb.y <= y - 4 && bomb.y >= y - 8)
                            bomb_eaten = true;
                        break;
                    case Direction.DOWN:
                        if (bomb.x >= x && bomb.x <= x + 8 && bomb.y >= y + 12 && bomb.y <= y + 16)
                            bomb_eaten = true;
                        break;
                    case Direction.LEFT:
                        if (bomb.y >= y - 4 && bomb.y <= y + 4 && bomb.x <= x - 4 && bomb.x >= x - 8)
                            bomb_eaten = true;
                        break;
                    case Direction.RIGHT:
                        if (bomb.y >= y - 4 && bomb.y <= y + 4 && bomb.x >= x + 28 && bomb.x <= x + 32)
                            bomb_eaten = true;
                        break;
                }

                if (bomb_eaten)
                {
                    real_hp--;
                    Screen.sprites.Remove(bomb);
                    Menu.bomb_out = false;
                    current_action = ActionState.DEFAULT;
                    local_timer = 0;
                    return;
                }
            }
        }

        // cancel all damage. damage can only be done by eating bomb.
        // unless stunned. stunned dodongos are one-hit.
        protected override void OnDamaged()
        {
            if (current_action == ActionState.RESTING)
                return;

            iframes_timer = 0;
            knockback_timer = 0;
            HP = 1;
        }

        protected override void Animation()
        {
            if (frames_between_anim == 0)
                return;

            bool side_movement = facing_direction == Direction.LEFT || facing_direction == Direction.RIGHT;
            bool anim_first_half = local_timer % (frames_between_anim * 2) < frames_between_anim;
            body[2].shown = side_movement;
            body[3].shown = side_movement;

            switch (facing_direction)
            {
                case Direction.UP:
                    body[0].xflip = !anim_first_half;
                    body[1].xflip = !anim_first_half;
                    if (anim_first_half)
                    {
                        body[0].tile_index = 0xfa;
                        body[1].tile_index = 0xfc;
                    }
                    else
                    {
                        body[0].tile_index = 0xfc;
                        body[1].tile_index = 0xfa;
                    }
                    break;
                case Direction.DOWN:
                    body[0].xflip = !anim_first_half;
                    body[1].xflip = !anim_first_half;
                    if (anim_first_half)
                    {
                        body[0].tile_index = 0xf4;
                        body[1].tile_index = 0xf6;
                    }
                    else
                    {
                        body[0].tile_index = 0xf6;
                        body[1].tile_index = 0xf4;
                    }
                    break;
                case Direction.LEFT:
                    foreach (Sprite s in body)
                        s.xflip = true;
                    if (anim_first_half)
                    {
                        body[0].tile_index = 0xe2;
                        body[1].tile_index = 0xe0;
                        body[2].tile_index = 0xde;
                        body[3].tile_index = 0xdc;
                    }
                    else
                    {
                        body[0].tile_index = 0xea;
                        body[1].tile_index = 0xe8;
                        body[2].tile_index = 0xe6;
                        body[3].tile_index = 0xe4;
                    }
                    break;
                case Direction.RIGHT:
                    foreach (Sprite s in body)
                        s.xflip = false;
                    if (anim_first_half)
                    {
                        body[0].tile_index = 0xdc;
                        body[1].tile_index = 0xde;
                        body[2].tile_index = 0xe0;
                        body[3].tile_index = 0xe2;
                    }
                    else
                    {
                        body[0].tile_index = 0xe4;
                        body[1].tile_index = 0xe6;
                        body[2].tile_index = 0xe8;
                        body[3].tile_index = 0xea;
                    }
                    break;
            }
        }

        protected override void OnInit()
        {
            shown = false;
            counterpart.shown = false;
            Animation();
            for (int i = 0; i < body.Length; i++)
            {
                body[i].x = x + i * 8;
                body[i].palette_index = palette_index;
                Screen.sprites.Add(body[i]);
            }
        }

        protected override bool OnTileMoved()
        {
            // prevent dodongo from walking onto rightmost tile collumn
            if (x >= 192 && facing_direction == Direction.RIGHT)
            {
                facing_direction = (Direction)RNG.Next(3);
            }

            return true;
        }

        protected override void OnDeath()
        {
            base.OnDeath();

            // triple dodongos do not count as bosses, apparently
            if (triple)
                SaveLoad.SetBossKillsFlag((byte)Array.IndexOf(DC.rooms_with_bosses, DC.current_screen), false);
        }
    }

    internal class Manhandla : Boss
    {
        private class ManhandlaHead : Enemy
        {
            Manhandla parent;
            int shooting_timer = 0;
            int animation_timer = 0;
            bool frame_2 = false;

            public ManhandlaHead(Manhandla parent, Direction dir) : base(0, 0xe0, 0xe4, false, false, 5, 0, 0, true)
            {
                this.parent = parent;
                this.facing_direction = dir;
                shooting_timer = RNG.Next(30);
                palette_index = parent.palette_index;
                counterpart.palette_index = parent.palette_index;
                DC.nb_enemies_alive++;
                stunnable = false;
                HP = 4;

                switch (dir)
                {
                    case Direction.DOWN:
                        yflip = true;
                        counterpart.yflip = true;
                        goto case Direction.UP;
                    case Direction.UP:
                        tile_location_1 = 0xe8;
                        tile_location_2 = 0xea;
                        counterpart.xflip = true;
                        break;

                    case Direction.RIGHT:
                        tile_location_1 = 0xe4;
                        tile_location_2 = 0xe0;
                        xflip = true;
                        counterpart.xflip = true;
                        break;
                    case Direction.LEFT:
                        tile_location_1 = 0xe0;
                        tile_location_2 = 0xe4;
                        break;
                }

                Animation();
            }

            protected override void EnemySpecificActions()
            {
                shooting_timer--;
                if (shooting_timer <= 0)
                {
                    new MagicOrbProjectileSprite(x + 4, y);
                    shooting_timer = RNG.Next(60, 120);
                }
            }

            protected override void Animation()
            {
                animation_timer--;

                if (animation_timer > 0)
                    return;

                byte tile = frame_2 ? tile_location_2 : tile_location_1;
                frame_2 = !frame_2;

                if (facing_direction == Direction.UP ||  facing_direction == Direction.DOWN)
                {
                    tile_index = tile;
                    counterpart.tile_index = tile;
                }
                else if (facing_direction == Direction.RIGHT)
                {
                    // lol
                    tile_index = (byte)(tile ^ 2);
                    counterpart.tile_index = tile;
                }
                else
                {
                    tile_index = tile;
                    counterpart.tile_index = (byte)(tile ^ 2);
                }

                animation_timer = RNG.Next(5, 15);
            }

            // apparently manhandla is immune to fire
            public override bool OnProjectileHit()
            {
                return hit_cause != typeof(ThrownFireSprite);
            }

            protected override void OnDeath()
            {
                parent.speed += 0.5f;
            }
        }

        ManhandlaHead[] heads = new ManhandlaHead[4];

        public Manhandla() : base(4)
        {
            smoke_appearance = false;
            HP = 1;
            invincible = true;
            frames_between_anim = 8;
            speed = 0.5f;

            tile_location_1 = 0xec;
            tile_location_2 = 0xec;
            counterpart.xflip = true;
            animation_mode = AnimationMode.ONEFRAME_M;
            palette_index = (byte)PaletteID.SP_1;
            current_action = ActionState.WALKING;
            facing_direction = Program.RNG.Next(2) == 0 ? Direction.UP : Direction.RIGHT;
            turnaround_timer_min = 30;
            turnaround_timer_max = 120;

            x = 128;
            y = 128;
            for (int i = 0; i < heads.Length; i++)
            {
                heads[i] = new ManhandlaHead(this, (Direction)i);

                heads[i].x = x;
                heads[i].y = y;

                switch (heads[i].facing_direction)
                {
                    case Direction.UP:
                        heads[i].y -= 16;
                        break;
                    case Direction.DOWN:
                        heads[i].y += 16;
                        break;
                    case Direction.LEFT:
                        heads[i].x -= 16;
                        break;
                    case Direction.RIGHT:
                        heads[i].x += 16;
                        break;
                }
            }
        }

        protected override void EnemySpecificActions()
        {
            // normal spd: 0.5u/f
            // 3 heads: 1u/f
            // 2 heads: 1.5u/f
            // 1 head: 2u/f
            Move8D(speed);

            int head_count = 0;
            foreach (ManhandlaHead m in heads)
            {
                if (m.HP <= 0)
                    continue;

                head_count++;
                m.x = x;
                m.y = y;

                switch (m.facing_direction)
                {
                    case Direction.UP:
                        m.y -= 16;
                        break;
                    case Direction.DOWN:
                        m.y += 16;
                        break;
                    case Direction.LEFT:
                        m.x -= 16;
                        break;
                    case Direction.RIGHT:
                        m.x += 16;
                        break;
                }
            }

            if (head_count == 0)
            {
                HP = 0;
                appeared = false;
                OnDeath();
                return;
            }
        }
    }

    internal class Gleeok : Boss
    {
        private class GleeokHead : Enemy
        {
            public const int NECK_START_X = 124;
            public const int NECK_START_Y = 112;

            int projectile_timer = 0;
            int random_turn_timer = 0;
            Gleeok parent;
            EightDirection direction;
            StaticSprite[] neck = new StaticSprite[3];

            public GleeokHead(Gleeok parent) : base(0, 0xdc, 0x1c, false, false, 0, 0, 0, true)
            {
                this.parent = parent;
                this.x = 124;
                this.y = 132;
                direction = (EightDirection)RNG.Next(4, 8);
                tile_index = 0xdc;
                palette_index = (byte)PaletteID.SP_3;
                HP = 10;
                stunnable = false;
                for (int i = 0; i < neck.Length; i++)
                {
                    neck[i] = new StaticSprite(0xda, (byte)PaletteID.SP_3, 0, 0);
                    Screen.sprites.Add(neck[i]);
                }

                EnemySpecificActions();
            }

            protected override void EnemySpecificActions()
            {
                projectile_timer--;
                if (projectile_timer <= 0)    
                {
                    new MagicOrbProjectileSprite(x, y);
                    projectile_timer = RNG.Next(120, 240);
                }

                Move8D();
                CheckBounds();

                if (invincible)
                    return;

                for (int i = 0; i < neck.Length; i++)
                {
                    // overengineered way of just saying 0.25, 0.5 and 0.75
                    float factor = (i + 1) * (1f / (neck.Length + 1));
                    // squaring the factor makes it not a straight line
                    neck[i].x = (int)(x + (NECK_START_X - x) * factor * factor);
                    neck[i].y = (int)(y + (NECK_START_Y - y) * factor);
                }
            }

            protected override void Animation()
            {
                if (!invincible)
                    return;

                if (local_timer % 2 == 0)
                    tile_index = 0xde;
                else
                    tile_index = 0xee;

                counterpart.tile_index = tile_index;
            }

            protected override void OnDamaged()
            {
                parent.flash_timer = flash_timer;
                flash_timer = 0;
            }

            public override bool OnProjectileHit()
            {
                return hit_cause == typeof(SwordProjectileSprite);
            }

            protected override void OnDeath()
            {
                invincible = true;
                appeared = false;
                HP = 1;
                foreach (Sprite s in neck)
                {
                    Screen.sprites.Remove(s);
                }

                tile_index = 0xde;
                counterpart.tile_index = 0xde;
                counterpart.xflip = true;
                palette_index = (byte)PaletteID.SP_2;
                counterpart.palette_index = (byte)PaletteID.SP_2;
            }

            public void Kill()
            {
                Screen.sprites.Remove(this);
                Screen.sprites.Remove(counterpart);
            }


            void Move8D()
            {
                if (!invincible)
                {
                    if (local_timer % 4 != 0)
                        return;
                }
                else
                {
                    if (local_timer % 8 == 0)
                        return;
                }

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
                random_turn_timer--;

                if (!invincible)
                {
                    if (random_turn_timer <= 0)
                    {
                        random_turn_timer = RNG.Next(120, 600);
                        direction ^= (EightDirection)0b11;
                    }

                    // lol bitwise reflection
                    if (y < 120)
                        direction |= (EightDirection)0b10;
                    else if (y > 142)
                        direction &= (EightDirection)0b101;
                    else if (x < 104)
                        direction |= (EightDirection)0b1;
                    else if (x > 144)
                        direction &= (EightDirection)0b110;
                }
                else
                {
                    if (random_turn_timer <= 0)
                    {
                        random_turn_timer = RNG.Next(15, 60);
                        direction = PickNewDirection();
                    }

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
        }


        GleeokHead[] heads;

        public Gleeok(int heads) : base(4)
        {
            this.heads = new GleeokHead[heads];
            body = new StaticSprite[7];
            invincible = true;
            palette_index = (byte)PaletteID.SP_3;
            frames_between_anim = 17;
            HP = 1;
            x = 116;
            y = 88;
            SetCustomSize(32, 16);

            Palettes.LoadPaletteGroup(PaletteID.SP_3, Palettes.PaletteGroups.GLEEOK);

            for (int i = 0; i < heads; i++)
            {
                this.heads[i] = new GleeokHead(this);
            }

            // give priority to neck root
            body[6] = new StaticSprite(0xda, (byte)PaletteID.SP_3, GleeokHead.NECK_START_X, GleeokHead.NECK_START_Y);
            body[6].unload_during_transition = true;
            Screen.sprites.Add(body[6]);
            for (int i = 0; i < body.Length - 1; i++)
            {
                body[i] = new StaticSprite(0, (byte)PaletteID.SP_3, x + (i % 3) * 8, y + (i / 3) * 16);
                body[i].unload_during_transition = true;
                Screen.sprites.Add(body[i]);
            }

            body[4].tile_index = 0xc6;
            Animation();
        }

        protected override void EnemySpecificActions()
        {
            int head_count = 0;
            foreach (GleeokHead g in heads)
            {
                // if red floating head
                if (g.invincible)
                    continue;

                head_count++;
            }

            if (head_count == 0)
            {
                HP = 0;
                appeared = false;
                local_timer = 0;
                x += 8;
                y += 16;
                OnDeath();
                foreach (GleeokHead g in heads)
                {
                    g.Kill();
                }
                return;
            }
        }

        protected override void Animation()
        {
            int cycle_index = local_timer % (frames_between_anim * 4);

            if (cycle_index < frames_between_anim)
            {
                body[0].tile_index = 0xc0;
                body[1].tile_index = 0xc4;
                body[2].tile_index = 0xc8;
                body[3].tile_index = 0xc2;
                body[5].tile_index = 0xca;
            }
            else if (cycle_index < frames_between_anim * 2 ||
                    cycle_index >= frames_between_anim * 3)
            {
                body[0].tile_index = 0xcc;
                body[1].tile_index = 0xc4;
                body[2].tile_index = 0xce;
                body[3].tile_index = 0xc2;
                body[5].tile_index = 0xd0;
            }
            else
            {
                body[0].tile_index = 0xd2;
                body[1].tile_index = 0xd6;
                body[2].tile_index = 0xd8;
                body[3].tile_index = 0xd4;
                body[5].tile_index = 0xd0;
            }

            for (int i = 0; i < body.Length; i++)
            {
                body[i].palette_index = palette_index;
            }
        }
    }

    internal class Digdogger : Boss
    {
        bool triple;
        bool is_small;
        float frames_between_movement = 3;

        public Digdogger(bool triple, Digdogger? parent = null) : base(0)
        {
            this.triple = triple;
            this.is_small = parent is not null;
            x = parent is not null ? parent.x : 168;
            y = parent is not null ? parent.y : 176;

            HP = 8;
            smoke_appearance = true;
            instant_smoke = true;
            Palettes.LoadPaletteGroup(PaletteID.SP_3, Palettes.PaletteGroups.DODONGO);

            tile_index = 0xda;
            counterpart.tile_index = tile_index;
            palette_index = (byte)PaletteID.SP_3;
            counterpart.palette_index = palette_index;

            if (is_small)
            {
                turnaround_timer_min = 5;
                turnaround_timer_max = 15;
            }
            else
            {
                turnaround_timer_min = 15;
                turnaround_timer_max = 60;
            }
        }

        protected override void EnemySpecificActions()
        {
            if (is_small && local_timer % 60 == 0 && frames_between_movement > 0.5f)
            {
                frames_between_movement -= 0.5f;
            }
            
            Move8D(1 / frames_between_movement);

            if (!is_small)
            {
                for (int i = 0; i < body.Length; i++)
                {
                    body[i].x = x - 8 + (i % 4) * 8;
                    body[i].y = y - 8 + (i / 4) * 16;
                }
            }
        }

        public void OnRecorderPlayed()
        {
            if (is_small)
                return;

            foreach (Sprite s in body)
            {
                Screen.sprites.Remove(s);
            }
            local_timer = 0;
            appeared = false;
            SetCustomSize(16, 16);
            is_small = true;
            turnaround_timer_min = 5;
            turnaround_timer_max = 15;
            invincible = false;
            shown = true;
            counterpart.shown = true;

            if (triple)
            {
                new Digdogger(false, this);
                new Digdogger(false, this);
                DC.nb_enemies_alive += 2;
            }
        }


        protected override void Animation()
        {
            if (!is_small)
            {
                shown = local_timer % 2 == 0;
                counterpart.shown = !shown;
            }

            if (local_timer % 6 == 0)
            {
                tile_index = 0xda;
                counterpart.tile_index = tile_index;
            }
            else if (local_timer % 6 == 3)
            {
                tile_index = 0xd8;
                counterpart.tile_index = tile_index;
            }
        }

        // can't be in constructor. dumb.
        protected override void OnInit()
        {
            if (!is_small)
            {
                // xy0 not in top left, rather near center.
                SetCustomSize(24, 24);
                invincible = true;

                body = new StaticSprite[8];
                for (int i = 0; i < body.Length; i++)
                {
                    byte tile = (byte)(i % 4 == 0 || i % 4 == 3 ? 0xd4 : 0xd6);
                    body[i] = new(tile, (byte)PaletteID.SP_3, x - 8 + (i % 4) * 8, y - 8 + (i / 4) * 16);
                    body[i].xflip = i % 4 >= 2;
                    body[i].yflip = i >= 4;
                    body[i].unload_during_transition = true;
                    Screen.sprites.Add(body[i]);
                }
            }
            counterpart.xflip = true;
        }
    }

    internal class Gohma : Boss
    {
        public Gohma(bool harder) : base(0)
        {
        }

        protected override void EnemySpecificActions()
        {
            throw new NotImplementedException();
        }
    }

    internal class Moldorm : Boss
    {
        public Moldorm() : base(4)
        {
        }

        protected override void EnemySpecificActions()
        {
            throw new NotImplementedException();
        }
    }

    internal class Patra : Boss
    {
        public enum PatraType
        {
            EXPANDING,
            ROTATING
        }

        PatraType type;

        public Patra(PatraType pattern) : base(4)
        {
            this.type = pattern;
        }

        protected override void EnemySpecificActions()
        {
            switch (type)
            {
                default: break;
            }
        }
    }

    internal class Ganon : Boss
    {
        public Ganon() : base(0)
        {
        }

        protected override void EnemySpecificActions()
        {
            throw new NotImplementedException();
        }
    }
}