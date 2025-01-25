namespace The_Legend_of_Zelda.Sprites
{
    internal class Stalfos : Enemy
    {
        public Stalfos() : base(AnimationMode.TWOFRAMES_S, 0xa8, 0xa8, false, true, 8, 0.5f, 3)
        {

        }

        public override void EnemySpecificActions()
        {
            
        }
    }

    internal class Keese : Enemy
    {
        public Keese(bool stronger) : base(AnimationMode.TWOFRAMES_M, 0x9a, 0x9c, stronger, true, 6, 0.75f, 0)
        {

        }

        public override void EnemySpecificActions()
        {

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
        public Bubble() : base(AnimationMode.ONEFRAME, 0x8e, 0x8e, false, true, 0, 1f, 0)
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

        }

        public override void EnemySpecificActions()
        {

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