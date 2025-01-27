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

        public Keese(bool stronger) : base(AnimationMode.TWOFRAMES_M, 0x9a, 0x9c, stronger, true, 6, 1f, 0)
        {
            current_action = ActionState.FLYING;
            palette_index = (byte)PaletteID.SP_1;
            if (stronger)
            {
                palette_index = (byte)PaletteID.SP_2;
            }
            counterpart.palette_index = palette_index;
            HP = 2;
            damage = 0.5f;
            speed = 0.5f;
            direction = (EightDirection)Program.RNG.Next(8);
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
                        }
                        break;
                    }

                    if (local_timer == 49)
                    {
                        frames_between_anim = 0;
                        when_to_stop = Program.RNG.Next(120, 240);
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
                        direction = (EightDirection)Program.RNG.Next(8);
                        when_to_stop = Program.RNG.Next(60, 120);
                        frames_between_anim = 6;
                    }

                    if (local_timer <= 54)
                    {
                        if (local_timer % frames_between_anim == 0)
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

                    if (x <= 32 || x >= 208 || y <= 96 || y >= 192)
                    {
                        // the math works out.
                        if (direction < (EightDirection)4)
                            direction ^= (EightDirection)1;
                        else
                            direction ^= (EightDirection)0b11;
                        //if (x <= 32)
                        //    direction = EightDirection.RIGHT;
                        //else if (x >= 208)
                        //    direction = EightDirection.LEFT;
                        //else if (y <= 96)
                        //    direction = EightDirection.DOWN;
                        //else
                        //    direction = EightDirection.UP;

                        local_timer = 56;
                    }

                    Move8D();
                    break;
            }
        }

        void Move8D()
        {
            if (local_timer % 4 == 0)
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

    internal class Gel : Enemy
    {
        public Gel() : base(AnimationMode.TWOFRAMES_SOLO, 0x92, 0x94, false, true, 2, 1f, 0)
        {

        }

        public override void EnemySpecificActions()
        {

        }
    }

    internal class Zol : Enemy
    {
        public Zol() : base(AnimationMode.TWOFRAMES_M, 0xa8, 0xaa, false, true, 9, 0.4f, 3)
        {

        }

        public override void EnemySpecificActions()
        {

        }
    }

    internal class Goriya : Enemy
    {
        public Goriya(bool stronger) : base(AnimationMode.TWOFRAMES_DURR, 0xb0, 0xb8, stronger, true, 6, 0.5f, 2)
        {
            if (stronger)
            {
                drop_category = 4;
            }
        }

        public override void EnemySpecificActions()
        {

        }
    }

    internal class Wallmaster : Enemy
    {
        public Wallmaster() : base(AnimationMode.TWOFRAMES_HM, 0xac, 0x9e, false, false, 8, 0.4f, 3, true)
        {

        }

        public override void EnemySpecificActions()
        {

        }
    }

    internal class BladeTrap : Enemy
    {
        public BladeTrap() : base(AnimationMode.ONEFRAME_M, 0x96, 0x96, false, false, 0, 1.75f, 0, true)
        {

        }

        public override void EnemySpecificActions()
        {

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
            if (stronger)
            {
                drop_category = 4;
            }
        }

        public override void EnemySpecificActions()
        {

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