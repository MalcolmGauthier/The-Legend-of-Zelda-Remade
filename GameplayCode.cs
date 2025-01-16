namespace The_Legend_of_Zelda
{
    public abstract class GameplayCode
    {
        public const byte OPENING_ANIMATION_DONE = 200;
        public const int SCROLL_ANIMATION_DONE = 500;

        public byte current_screen;
        protected byte opening_animation_timer = 0;
        public byte scroll_destination { get; protected set; }
        protected int scroll_animation_timer = SCROLL_ANIMATION_DONE;

        public Direction scroll_direction { get; protected set; }
        private Sound.Songs bg_music;
        private float bg_music_start;

        private (byte room_index, int enemy_id)[] killed_enenmy_queue = new(byte, int)[20];

        public void Tick()
        {
            if (opening_animation_timer < OPENING_ANIMATION_DONE)
            {
                OpeningAnimation();
                return;
            }

            Menu.DrawHUD();
            Menu.Tick();
            Link.Tick();

            SpecificCode();

            if (!Sound.IsMusicPlaying())
            {
                Sound.PlaySong(bg_music, false);
                Sound.JumpTo(bg_music_start);
            }
        }

        protected abstract void SpecificCode();

        void OpeningAnimation()
        {
            opening_animation_timer++;

            if (opening_animation_timer >= OPENING_ANIMATION_DONE)
            {
                Link.Show(true);
                Menu.can_open_menu = true;
                Menu.draw_hud_objects = true;
                Sound.PlaySong(bg_music, false);
                Program.can_pause = true;
                Menu.map_dot.shown = true;
                Link.can_move = true;
                return;
            }

            Menu.can_open_menu = false;
            Link.can_move = false;
            Link.Show(false);
            Menu.draw_hud_objects = false;

            if (opening_animation_timer % 5 != 0)
                return;

            int num_rows_to_erase = 16 - opening_animation_timer / 5;
            if (num_rows_to_erase <= 0)
            {
                opening_animation_timer = OPENING_ANIMATION_DONE - 1;
            }

            Textures.LoadPPUPage(this is OverworldCode ? Textures.PPUDataGroup.OVERWORLD : Textures.PPUDataGroup.DUNGEON, current_screen, 0);
            int left_index_start = Textures.PPU_WIDTH * 8;
            int right_index_start = Textures.PPU_WIDTH * 9 - 1;

            for (int i = 0; i < num_rows_to_erase; i++)
            {
                for (int j = 0; j < 22; j++)
                {
                    Textures.ppu[left_index_start + j * Textures.PPU_WIDTH + i] = 0x24;
                    Textures.ppu[right_index_start + j * Textures.PPU_WIDTH - i] = 0x24;
                }
            }
        }

        public bool OpeningAnimationDone() => opening_animation_timer == OPENING_ANIMATION_DONE;
        public bool ScrollingDone() => scroll_animation_timer == SCROLL_ANIMATION_DONE;

        protected Sound.Songs UpdateBGMusic(Sound.Songs new_song, float start)
        {
            bg_music = new_song;
            bg_music_start = start;
            return bg_music;
        }

        // reset all of the variables keeping track of which items link has thrown onto the field
        // and other stuff that needs to go away on a screen transition
        public void ResetLinkPowerUps()
        {
            Menu.fire_out = 0;
            Menu.boomerang_out = false;
            Menu.arrow_out = false;
            Menu.magic_wave_out = false;
            Menu.bait_out = false;
            Menu.bomb_out = false;
            // if link is in the tornado, we don't want to destroy the tornado
            if (Link.shown) Menu.tornado_out = false;
            Menu.sword_proj_out = false;
            Link.clock_flash = false;
            Link.iframes_timer = 0;
            Link.self.palette_index = 4;
            Link.counterpart.palette_index = 4;
        }

        // unloads all sprites that should unload when screen changes
        public void UnloadSpritesRoomTransition()
        {
            // no foreach, because foreach throws error when modifying list
            for (int i = 0; i < Screen.sprites.Count; i++)
            {
                if (Screen.sprites[i].unload_during_transition)
                {
                    Screen.sprites.RemoveAt(i);
                    i--;
                }
            }
        }
    }
}
