using The_Legend_of_Zelda.Gameplay;
using The_Legend_of_Zelda.Rendering;
using static The_Legend_of_Zelda.Gameplay.Program;

namespace The_Legend_of_Zelda.Sprites
{
    internal abstract class Boss : Enemy
    {
        protected StaticSprite[] body = [];

        public Boss(byte drop_category) : base(AnimationMode.ONEFRAME, 0, 0, false, false, 0, 0, drop_category, true)
        {
            tile_index = (byte)SpriteID.BLANK;
            counterpart.tile_index = (byte)SpriteID.BLANK;
            tile_location_1 = tile_index;
            tile_location_2 = tile_index;
        }

        protected override void OnDeath()
        {
            shown = true;
            counterpart.shown = true;
            foreach (Sprite s in body)
                Screen.sprites.Remove(s);
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
                shooting_timer = Program.RNG.Next(60, 240);
                //int ratio = (int)(Math.Abs(Link.x - x) * 0.4f);
                new MagicOrbProjectileSprite(x + 8, y, true, -2, 0);
                new MagicOrbProjectileSprite(x + 8, y, true, -2, 1);
                new MagicOrbProjectileSprite(x + 8, y, true, -2, -1);
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
        public Dodongo(int frames_between_anim, float speed, byte drop_category) : base(drop_category)
        {
        }

        protected override void EnemySpecificActions()
        {
            throw new NotImplementedException();
        }
    }

    internal class Manhandla : Boss
    {
        public Manhandla(int frames_between_anim, float speed, byte drop_category) : base(drop_category)
        {
        }

        protected override void EnemySpecificActions()
        {
            throw new NotImplementedException();
        }
    }

    internal class Gleeok : Boss
    {
        public Gleeok(int frames_between_anim, float speed, byte drop_category) : base(drop_category)
        {
        }

        protected override void EnemySpecificActions()
        {
            throw new NotImplementedException();
        }
    }

    internal class Digdogger : Boss
    {
        public Digdogger(int frames_between_anim, float speed, byte drop_category) : base(drop_category)
        {
        }

        protected override void EnemySpecificActions()
        {
            throw new NotImplementedException();
        }
    }

    internal class Gohma : Boss
    {
        public Gohma(int frames_between_anim, float speed, byte drop_category) : base(drop_category)
        {
        }

        protected override void EnemySpecificActions()
        {
            throw new NotImplementedException();
        }
    }

    internal class Moldorm : Boss
    {
        public Moldorm(int frames_between_anim, float speed, byte drop_category) : base(drop_category)
        {
        }

        protected override void EnemySpecificActions()
        {
            throw new NotImplementedException();
        }
    }

    internal class Lanmola : Boss
    {
        public Lanmola(int frames_between_anim, float speed, byte drop_category) : base(drop_category)
        {
        }

        protected override void EnemySpecificActions()
        {
            throw new NotImplementedException();
        }
    }

    internal class Patra : Boss
    {
        public Patra(int frames_between_anim, float speed, byte drop_category) : base(drop_category)
        {
        }

        protected override void EnemySpecificActions()
        {
            throw new NotImplementedException();
        }
    }

    internal class Ganon : Boss
    {
        public Ganon(int frames_between_anim, float speed, byte drop_category) : base(drop_category)
        {
        }

        protected override void EnemySpecificActions()
        {
            throw new NotImplementedException();
        }
    }
}