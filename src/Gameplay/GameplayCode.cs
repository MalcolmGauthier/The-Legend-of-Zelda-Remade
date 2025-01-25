using The_Legend_of_Zelda.Rendering;
using The_Legend_of_Zelda.Sprites;
using static The_Legend_of_Zelda.Gameplay.Program;

namespace The_Legend_of_Zelda.Gameplay
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

        private byte[] killed_enenmy_queue = new byte[20];
        private int killed_enemy_queue_index = 0;

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

        public bool OpeningAnimationDone() => opening_animation_timer == OPENING_ANIMATION_DONE;
        void OpeningAnimation()
        {
            opening_animation_timer++;

            if (OpeningAnimationDone())
            {
                Link.Show(true);
                Menu.can_open_menu = true;
                Menu.draw_hud_objects = true;
                Sound.PlaySong(bg_music, false);
                Program.can_pause = true;
                Link.can_move = true;
                if (this is DungeonCode dc && SaveLoad.GetCompassFlag(dc.current_dungeon))
                {
                    Screen.sprites.Add(dc.compass_dot);
                }
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

        public bool ScrollingDone() => scroll_animation_timer >= SCROLL_ANIMATION_DONE;
        // if player is on a raft, set on_raft to true to disable checking for held direction
        protected void Scroll(bool on_raft)
        {
            if (ScrollingDone())
            {
                if (Link.y < 64 && (Control.IsHeld(Buttons.UP) || on_raft))
                {
                    scroll_destination = (byte)(current_screen - 16);
                    scroll_animation_timer = 0;
                    scroll_direction = Direction.UP;
                }
                else if (Link.y > 223 && (Control.IsHeld(Buttons.DOWN) || on_raft))
                {
                    scroll_destination = (byte)(current_screen + 16);
                    scroll_animation_timer = 0;
                    scroll_direction = Direction.DOWN;
                }
                else if (Link.x < 1 && Control.IsHeld(Buttons.LEFT))
                {
                    scroll_destination = (byte)(current_screen - 1);
                    scroll_animation_timer = 0;
                    scroll_direction = Direction.LEFT;
                }
                else if (Link.x > 239 && (Control.IsHeld(Buttons.RIGHT) || Menu.tornado_out))
                {
                    scroll_destination = (byte)(current_screen + 1);
                    scroll_animation_timer = 0;
                    scroll_direction = Direction.RIGHT;
                }

                return;
            }

            if (scroll_animation_timer == 0)
            {
                Menu.can_open_menu = false;
                UnloadSpritesRoomTransition();
                ResetLinkPowerUps();
                // prevents interrupted bomb explosion from keeping grayscale mode on
                Palettes.grayscale_mode = false;

                if (!SpecificScrollCode(false))
                {
                    scroll_animation_timer = SCROLL_ANIMATION_DONE;
                    return;
                }

                Textures.PPUDataGroup data_group = Program.gamemode == Program.Gamemode.OVERWORLD ? Textures.PPUDataGroup.OVERWORLD : Textures.PPUDataGroup.DUNGEON;

                if (scroll_direction == Direction.DOWN)
                {
                    Textures.LoadPPUPage(data_group, scroll_destination, 1);
                }
                else if (scroll_direction == Direction.UP)
                {
                    // scrolling up is tricky because of the HUD, so we swap the screens, warp to screen one then scroll back up to 0
                    // we copy the data instead of loading the page to *not* initialize anything in the current screen
                    int tiles_per_screen = Textures.PPU_WIDTH * Textures.PPU_HEIGHT;
                    for (int i = 256; i < tiles_per_screen; i++)
                    {
                        Textures.ppu[i + (tiles_per_screen - 256)] = Textures.ppu[i];
                        Textures.ppu_plt[i + (tiles_per_screen - 256)] = Textures.ppu_plt[i];
                    }
                    Textures.LoadPPUPage(data_group, scroll_destination, 0);
                    Screen.y_scroll = 176;
                    Link.SetPos(new_y: 240);
                }
                else
                {
                    Textures.LoadPPUPage(data_group, scroll_destination, 2);
                }

                Link.can_move = false;
            }

            int FRAMES_BETWEEN_CAMERA_MOVE = Program.gamemode == Program.Gamemode.OVERWORLD ? 2 : 4;
            int VERTICAL_SCROLL_SPEED = Program.gamemode == Program.Gamemode.OVERWORLD ? 7 : 8;
            int HORIZONTAL_SCROLL_SPEED = Program.gamemode == Program.Gamemode.OVERWORLD ? 4 : 2;
            int VERTICAL_SCROLL_LENGTH = Program.gamemode == Program.Gamemode.OVERWORLD ? 49 : 87;
            int HORIZONTAL_SCROLL_LENGTH = Program.gamemode == Program.Gamemode.OVERWORLD ? 64 : 128;

            if (scroll_direction == Direction.UP || scroll_direction == Direction.DOWN)
            {
                if (Program.gTimer % FRAMES_BETWEEN_CAMERA_MOVE == 0)
                {
                    if (scroll_direction == Direction.UP)
                    {
                        Screen.y_scroll -= VERTICAL_SCROLL_SPEED;
                        if (Program.gTimer % 4 == 0)
                            Link.SetPos(new_y: Link.y - 2);
                        if (Link.y < 224)
                            Link.SetPos(new_y: 224);
                    }
                    else
                    {
                        Screen.y_scroll += VERTICAL_SCROLL_SPEED;
                        if (Program.gTimer % 3 == 0)
                            Link.SetPos(new_y: Link.y + 2);
                        if (Link.y < 65)
                            Link.SetPos(new_y: 65);
                    }
                }

                if (scroll_animation_timer == VERTICAL_SCROLL_LENGTH)
                {
                    EndScroll();
                }
            }
            else
            {
                if (scroll_direction == Direction.LEFT)
                {
                    Screen.x_scroll -= HORIZONTAL_SCROLL_SPEED;
                    if (Program.gTimer % 4 == 0)
                        Link.SetPos(new_x: Link.x - 1);
                    if (Link.x > 239)
                        Link.SetPos(new_x: 239);
                }
                else
                {
                    Screen.x_scroll += HORIZONTAL_SCROLL_SPEED;
                    if (Program.gTimer % 4 == 0)
                        Link.SetPos(new_x: Link.x + 1);
                    if (Link.x < 1)
                        Link.SetPos(new_x: 1);
                }

                if (scroll_animation_timer == HORIZONTAL_SCROLL_LENGTH)
                {
                    EndScroll();
                }
            }

            if ((int)scroll_direction < 2)
                Link.current_action = (LinkAction)scroll_direction + 2;
            else
                Link.current_action = (LinkAction)scroll_direction - 2;

            scroll_animation_timer++;
            Link.animation_timer++;
        }
        // this allows overworld and dungeon to have two sets of unique code to run in the scroll function.
        // scroll_finished as false is for code meant to run on scroll initialization, where you return false to cancel to scroll and true to continue.
        // scroll_finished as true is for code meant to run when scrolling is finished. wether you return true or false doesn't matter here.
        protected abstract bool SpecificScrollCode(bool scroll_finished);
        void EndScroll()
        {
            bool is_overworld = Program.gamemode == Program.Gamemode.OVERWORLD;

            Link.Show(true);
            Screen.x_scroll = 0;
            Screen.y_scroll = 0;
            Menu.blue_candle_limit_reached = false;
            Textures.LoadPPUPage(is_overworld ? Textures.PPUDataGroup.OVERWORLD : Textures.PPUDataGroup.DUNGEON, scroll_destination, 0);
            current_screen = scroll_destination;
            Link.can_move = is_overworld;
            if (scroll_direction == Direction.UP)
                Link.SetPos(new_y: 223);
            else if (scroll_direction == Direction.DOWN)
                Link.SetPos(new_y: 65);
            else if (scroll_direction == Direction.LEFT)
                Link.SetPos(new_x: 239);
            else
                Link.SetPos(new_x: 1);
            scroll_animation_timer = SCROLL_ANIMATION_DONE;
            Menu.can_open_menu = true;

            SpecificScrollCode(true);
        }

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

        public void AddToKillQueue(byte room_index)
        {
            killed_enemy_queue_index++;
            killed_enemy_queue_index %= killed_enenmy_queue.Length;
            killed_enenmy_queue[killed_enemy_queue_index] = room_index;
        }
        public void EmptyEnenmyKillQueue()
        {
            Array.Clear(killed_enenmy_queue, 0, killed_enenmy_queue.Length);
        }
        public int IsInKillQueue(byte room_index, int[] ignore)
        {
            for (int i = 0; i < killed_enenmy_queue.Length; i++)
            {
                if (killed_enenmy_queue[i] == room_index && !ignore.Contains(i))
                    return i;
            }

            return -1;
        }
    }
}
