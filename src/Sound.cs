using static SDL2.SDL;
using static SDL2.SDL_mixer;
using The_Legend_of_Zelda.Gameplay;

namespace The_Legend_of_Zelda
{
    public static class Sound
    {
        const float VOLUME = 0.02f;
        readonly static byte _volume = (byte)(Math.Abs(VOLUME) * SDL_MIX_MAXVOLUME);

        public enum Songs
        {
            SPLASH,
            OVERWORLD,
            DUNGEON,
            DEATH,
            DEATH_MOUNTAIN,
            CREDITS
        }
        public enum SoundEffects
        {
            ARROW,
            BLOCK,
            BOMB_PLACE,
            BOOMERANG,
            EXPLOSION,
            FIRE,
            ITEM,
            ITEM_GET,
            HEART,
            HURT,
            MAGIC,
            RECORDER,
            RUPEE,
            SECRET,
            STAIRS,
            SWORD,
            TEXT
        }

        public static IntPtr music = IntPtr.Zero;
        public static IntPtr[] SFX = { IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero };
        static byte[] sfx_order = { 0, 0, 0, 0 };
        static int recorder_sfx_channel = -1;
        public static bool recorder_playing = false;


        public static Dictionary<Songs, string> music_list = new Dictionary<Songs, string>()
        {
            {Songs.SPLASH, @"Data\MUSIC\splash.ogg"},
            {Songs.OVERWORLD, @"Data\MUSIC\overworld.ogg"},
            {Songs.DUNGEON, @"Data\MUSIC\dungeon.ogg"},
            {Songs.DEATH_MOUNTAIN, @"Data\MUSIC\death_mountain.ogg"},
            {Songs.DEATH, @"Data\MUSIC\death.ogg"},
            {Songs.CREDITS, @"Data\MUSIC\credits.ogg"}
        };
        public static Dictionary<SoundEffects, string> sfx_list = new Dictionary<SoundEffects, string>()
        {
            {SoundEffects.ARROW, @"Data\SFX\arrow.wav"},
            {SoundEffects.BLOCK, @"Data\SFX\block.wav"},
            {SoundEffects.BOMB_PLACE, @"Data\SFX\bomb_place.wav"},
            {SoundEffects.BOOMERANG, @"Data\SFX\boomerang.wav"},
            {SoundEffects.EXPLOSION, @"Data\SFX\explosion.wav"},
            {SoundEffects.FIRE, @"Data\SFX\fire.wav"},
            {SoundEffects.ITEM, @"Data\SFX\special_item.wav"},
            {SoundEffects.ITEM_GET, @"Data\SFX\_.wav"},
            {SoundEffects.HEART, @"Data\SFX\heart.wav"},
            {SoundEffects.HURT, @"Data\SFX\hurt.wav"},
            {SoundEffects.MAGIC, @"Data\SFX\magic.wav"},
            {SoundEffects.RECORDER, @"Data\SFX\recorder.wav"},
            {SoundEffects.RUPEE, @"Data\SFX\rupee.wav"},
            {SoundEffects.SECRET, @"Data\SFX\_.wav"},
            {SoundEffects.STAIRS, @"Data\SFX\stairs.wav"},
            {SoundEffects.SWORD, @"Data\SFX\sword.wav"},
            {SoundEffects.TEXT, @"Data\SFX\text.wav"},
        };

        public static void Init()
        {
            if (Program.mute_sound)
                return;

            Mix_OpenAudio(44100, MIX_DEFAULT_FORMAT, 1, 2048);

            if (Program.gamemode != Program.Gamemode.SCREENTEST)
                PlaySong(Songs.SPLASH, false);

            Mix_AllocateChannels(4);
        }

        public static void PlaySong(Songs song, bool loop = true)
        {
            if (Program.mute_sound)
                return;

            int loops;
            if (loop)
                loops = -1;
            else
                loops = 1;

            if (music is not 0)
                Mix_FreeMusic(music);

            music_list.TryGetValue(song, out string? path);
            if (path == null)
                return;

            music = Mix_LoadMUS(path);
            if (music == 0)
                return;

            Mix_VolumeMusic(_volume);
            Mix_PlayMusic(music, loops);
        }

        public static void PlaySFX(SoundEffects sfx_to_play, bool full_fill = false)
        {
            if (Program.mute_sound)
                return;

            for (int i = 0; i < SFX.Length + 1; i++)
            {
                if (full_fill)
                {
                    for (byte j = 0; j < SFX.Length; j++)
                    {
                        Mix_HaltChannel(j);
                    }
                }
                else if (i == SFX.Length)
                {
                    int oldest_channel = Array.IndexOf(sfx_order, sfx_order.Max());
                    Mix_HaltChannel(oldest_channel);
                    sfx_order[oldest_channel] = 0;
                    i = oldest_channel;
                }

                if (Mix_Playing(i) == 0)
                {
                    if (sfx_to_play == SoundEffects.RECORDER)
                    {
                        recorder_playing = true;
                        recorder_sfx_channel = i;
                    }
                    string? path;
                    sfx_list.TryGetValue(sfx_to_play, out path);
                    if (path == null)
                        return;
                    Mix_FreeChunk(SFX[i]);
                    SFX[i] = Mix_LoadWAV(path);
                    if (SFX[i] == IntPtr.Zero)
                        return;
                    Mix_VolumeChunk(SFX[i], _volume);
                    Mix_PlayChannel(i, SFX[i], 0);
                    for (byte j = 0; j < SFX.Length; j++)
                    {
                        if (sfx_order[j] != 0)
                            sfx_order[j]++;
                    }
                    sfx_order[i] = 1;
                    return;
                }
            }
        }

        public static void PauseMusic(bool unpause = false)
        {
            if (Program.mute_sound)
                return;

            if (Mix_PausedMusic() != 0)
            {
                if (!unpause)
                    return;
            }

            if (!unpause)
                Mix_PauseMusic();
            else
                Mix_ResumeMusic();
        }

        public static bool IsMusicPlaying()
        {
            // needs to be checked before calling Mix_PlayingMusic, because when sound is muted, 
            // SDL_Mixer is not initialized and thus gives an error upon calling any of its functions
            if (Program.mute_sound)
                return true;

            return Mix_PlayingMusic() != 0;
        }

        public static void JumpTo(float timestamp)
        {
            if (Program.mute_sound)
                return;

            Mix_SetMusicPosition(timestamp);
        }

        // will return true if recorder sfx is playing
        public static bool RecorderPlaying()
        {
            if (Program.mute_sound)
                return true;

            if (recorder_sfx_channel == -1 || !recorder_playing)
                return false;

            if (Mix_Playing(recorder_sfx_channel) == 0 && recorder_playing)
            {
                recorder_sfx_channel = -1;
                recorder_playing = false;
                return false;
            }

            return true;
        }
    }
}