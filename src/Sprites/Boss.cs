namespace The_Legend_of_Zelda.Sprites
{
    internal class Boss : Enemy
    {
        public Boss(bool stronger, int frames_between_anim, float speed, byte drop_category) : base(AnimationMode.ONEFRAME, 0, 0, stronger, false, frames_between_anim, speed, drop_category, true)
        {

        }

        protected override void EnemySpecificActions()
        {

        }
    }
}