using System.IO;
using System.Runtime.CompilerServices;
using The_Legend_of_Zelda.Sprites;
using The_Legend_of_Zelda.Rendering;
using static SDL2.SDL;
using static SDL2.SDL_mixer;

namespace The_Legend_of_Zelda.Gameplay
{
    public static unsafe class Program
    {
        public enum Gamemode
        {
            TITLESCREEN,
            FILESELECT,
            OVERWORLD,
            DUNGEON,
            DEATH,
            CREDITS,
            SCREENTEST
        }

        private const double NES_FPS = 60.0988; // the NES runs at just under 60.1 frames per second. This is the goal.
        public const int NES_OUTPUT_WIDTH = 256;
        public const int NES_OUTPUT_HEIGHT = 240;

        public static IntPtr window;
        public static SDL_Event e;

        public static bool exit = false;
        public static bool game_paused = false;
        public static bool can_pause = false;

        public static int gTimer = 0;
        const int TicksPerFrame = (int)(TimeSpan.TicksPerSecond / NES_FPS);
        static long frame_time;
        static int screenshot_message_timer = 0;
        static string screenshot_file_name = "";

        public static Gamemode gamemode = Gamemode.TITLESCREEN;
        public static Random RNG = new Random();
        public static OverworldCode OC = new();
        public static DungeonCode DC = new();
        public static LinkSprite Link = new();

        // debug
        static long last_fps_display = DateTime.Now.Ticks;
        static ushort fps_count = 0, last_fps_count;
        private static List<(object val, string info)> debug_draw_vals = new();
        public static bool advance_1_frame = false;
        public static bool full_pause = false;
        // DEBUG FLAGS
        public static bool show_fps = false, input_display = false, uncap_fps = false, mute_sound = true, screentest = false,
            fast_forward = true, pause_whenever = true, frame_advance = true, reset = true, logger = true;

        static void Main()
        {
            Init();

            while (!exit)
            {
                if (!full_pause)
                    gTimer++;
                Control.Poll();

                if (!Paused())
                {
                    Code();
                    Render();
                }
                SDL_RenderPresent(Screen.render);

                if (!uncap_fps)
                    while (frame_time > DateTime.Now.Ticks - TicksPerFrame) ;
                frame_time = DateTime.Now.Ticks;
            }

            SDL_DestroyRenderer(Screen.render);
            SDL_DestroyWindow(window);
            Mix_Quit();
            SDL_Quit();
        }
        
        static void Init()
        {
            SDL_Init(SDL_INIT_VIDEO | SDL_INIT_AUDIO);
            window = SDL_CreateWindow("The Legend of Zelda", SDL_WINDOWPOS_UNDEFINED, SDL_WINDOWPOS_UNDEFINED, NES_OUTPUT_WIDTH * 2, NES_OUTPUT_HEIGHT * 2,
                SDL_WindowFlags.SDL_WINDOW_SHOWN | SDL_WindowFlags.SDL_WINDOW_RESIZABLE);
            SDL_PollEvent(out e);
            Palettes.Init();
            Textures.Init();
            Screen.Init();
            SDL_SetRenderDrawColor(Screen.render, 0, 0, 0, 255);
            SDL_RenderPresent(Screen.render);

            if (screentest)
                gamemode = Gamemode.SCREENTEST;

            Sound.Init();
        }

        static void Code()
        {
            switch (gamemode)
            {
                case Gamemode.SCREENTEST:
                    Screen.ScreenTest();
                    break;

                case Gamemode.TITLESCREEN:
                    TitlescreenCode.Tick();
                    break;
                case Gamemode.FILESELECT:
                    FileSelectCode.Tick();
                    break;
                case Gamemode.OVERWORLD:
                    OC.Tick();
                    break;
                case Gamemode.DUNGEON:
                    DC.Tick();
                    break;
                case Gamemode.DEATH:
                    DeathCode.Tick();
                    break;
                case Gamemode.CREDITS:
                    CreditsCode.Tick();
                    break;
            }

            for (int i = 0; i < Screen.sprites.Count; i++)
            {
                Screen.sprites[i].Action();
            }
        }

        static void Render()
        {
            Screen.Render();

            SDL_GetWindowSize(window, out int w, out int h);
            DebugText.SetScreenDimensions(w, h);
            if (show_fps)
            {
                if (last_fps_display + 10000000 < DateTime.Now.Ticks)
                {
                    last_fps_count = fps_count;
                    last_fps_display = DateTime.Now.Ticks;
                    fps_count = 0;
                }
                DebugText.DisplayText(last_fps_count.ToString(), 10, 10, 1);
                fps_count++;


            }
            if (input_display)
            {
                // held buttons shown as red dots
                SDL_SetRenderDrawColor(Screen.render, 255, 0, 0, 255);
                if (Control.IsHeld(Buttons.LEFT))
                    SDL_RenderDrawPoint(Screen.render, 10, 30);
                if (Control.IsHeld(Buttons.UP))
                    SDL_RenderDrawPoint(Screen.render, 12, 28);
                if (Control.IsHeld(Buttons.RIGHT))
                    SDL_RenderDrawPoint(Screen.render, 14, 30);
                if (Control.IsHeld(Buttons.DOWN))
                    SDL_RenderDrawPoint(Screen.render, 12, 32);
                if (Control.IsHeld(Buttons.A))
                    SDL_RenderDrawPoint(Screen.render, 30, 29);
                if (Control.IsHeld(Buttons.B))
                    SDL_RenderDrawPoint(Screen.render, 33, 29);
                if (Control.IsHeld(Buttons.START))
                    SDL_RenderDrawPoint(Screen.render, 20, 30);
                if (Control.IsHeld(Buttons.SELECT))
                    SDL_RenderDrawPoint(Screen.render, 23, 30);

                // freshly pressed buttons shown as green dots for 1 frame
                SDL_SetRenderDrawColor(Screen.render, 0, 255, 0, 255);
                if (Control.IsPressed(Buttons.LEFT))
                    SDL_RenderDrawPoint(Screen.render, 10, 30);
                if (Control.IsPressed(Buttons.UP))
                    SDL_RenderDrawPoint(Screen.render, 12, 28);
                if (Control.IsPressed(Buttons.RIGHT))
                    SDL_RenderDrawPoint(Screen.render, 14, 30);
                if (Control.IsPressed(Buttons.DOWN))
                    SDL_RenderDrawPoint(Screen.render, 12, 32);
                if (Control.IsPressed(Buttons.A))
                    SDL_RenderDrawPoint(Screen.render, 30, 29);
                if (Control.IsPressed(Buttons.B))
                    SDL_RenderDrawPoint(Screen.render, 33, 29);
                if (Control.IsPressed(Buttons.START))
                    SDL_RenderDrawPoint(Screen.render, 20, 30);
                if (Control.IsPressed(Buttons.SELECT))
                    SDL_RenderDrawPoint(Screen.render, 23, 30);
            }
            if (screenshot_message_timer > 0)
            {
                screenshot_message_timer--;
                DebugText.DisplayText($"saved screenshot at {screenshot_file_name}", 10, 10, 1, 0xff0000);
            }
            if (logger)
            {
                for (int i = 0; i < debug_draw_vals.Count; i++)
                {
                    string info = debug_draw_vals[i].info + (string.IsNullOrEmpty(debug_draw_vals[i].info) ? "" : ": ");
                    DebugText.DisplayText(info + debug_draw_vals[i].val.ToString() ?? "null",
                        10, 15 * i + 10, 1, 0xff00ff);
                }

                if (!full_pause || !advance_1_frame)
                    debug_draw_vals.Clear();
            }
            string err = SDL_GetError();
            if (err != "")
            {
                DebugText.DisplayText("///sdl error/// " + err, 10, 10, 1, RNG.Next() % 0xffffff);
            }
        }

        static bool Paused()
        {
            if (advance_1_frame)
            {
                advance_1_frame = false;
                full_pause = true;
                gTimer++;
                return false;
            }

            if (pause_whenever && full_pause)
            {
                DebugText.DisplayText("II", 10, 10, 3, 0xff0000);
                return true;
            }

            if (gamemode != Gamemode.OVERWORLD && gamemode != Gamemode.DUNGEON)
                return false;

            if (game_paused && Control.IsPressed(Buttons.SELECT))
            {
                game_paused = false;
                Sound.PauseMusic(true);
            }
            else if (can_pause && Control.IsPressed(Buttons.SELECT))
            {
                game_paused = true;
                Sound.PauseMusic();
            }

            return game_paused;
        }

        public static void Screenshot()
        {
            // little endian >:(
            byte[] bmp_header =
            {
                0x42, 0x4D,             // BM - Windows BMP format
                0xE6, 0xF0, 0x00, 0x00, // File size: 61,750 bytes
                0x00, 0x00, 0x00, 0x00, // Reserved (unused)
                0x36, 0x01, 0x00, 0x00, // Pixel data offset (310 bytes)
                0x28, 0x00, 0x00, 0x00, // DIB header size (40 bytes)
                0x00, 0x01, 0x00, 0x00, // Width: 256 pixels
                0xF0, 0x00, 0x00, 0x00, // Height: 240 pixels
                0x01, 0x00,             // Number of planes (1)
                0x08, 0x00,             // Bits per pixel (8-bit indexed color)
                0x00, 0x00, 0x00, 0x00, // Compression (None)
                0x00, 0xF0, 0x00, 0x00, // Image data size (61,440 bytes)
                0x13, 0x0B, 0x00, 0x00, // printing info
                0x13, 0x0B, 0x00, 0x00, // printing info
                0x40, 0x00, 0x00, 0x00, // Number of colors in palette (64)
                0x00, 0x00, 0x00, 0x00  // Important colors (0 = all)
            };

            uint i = 1;
            while (File.Exists(@$"screenshots\capture{i}.bmp"))
            {
                i++;
                if (i == 0)
                    return;
            }

            using (FileStream stream = File.Create(@$"screenshots\capture{i}.bmp"))
            {
                BinaryWriter bw = new BinaryWriter(stream);

                bw.Write(bmp_header);

                // update: screenshots now used indexed color, and thus take up 60kb instead of 150kb
                // color palette
                for (int p = 0; p < 64; p++)
                {
                    bw.Write(Palettes.color_list[p].b);
                    bw.Write(Palettes.color_list[p].g);
                    bw.Write(Palettes.color_list[p].r);
                    bw.Write((byte)0); // must be 0
                }

                unsafe
                {
                    byte* ref_screen = (byte*)((SDL_Surface*)Screen.window_surface)->pixels;

                    for (int y = 239; y >= 0; y--)
                    {
                        for (int x = 0; x < 256; x++)
                        {
                            bw.Write(Palettes.active_palette_list[ref_screen[(y << 8) + x]]);
                        }
                    }
                }

                screenshot_file_name = stream.Name;
                screenshot_message_timer = 500;
            }
        }

        public static void Reset()
        {
            gTimer = 0;
            gamemode = Gamemode.TITLESCREEN;
            Screen.sprites.Clear();
            Palettes.Init();
            Textures.Init();
            game_paused = false;
            full_pause = false;
            advance_1_frame = false;
            Screen.x_scroll = 0;
            Screen.y_scroll = 0;
            OC = new();
            DC = new();
            Link = new();
            Sound.PlaySong(Sound.Songs.SPLASH, false);
        }

        public static void DebugLog(string message, object obj)
        {
            debug_draw_vals.Add((obj, message));
        }
        public static void DebugLog(object message)
        {
            debug_draw_vals.Add((message, ""));
        }
    }

    // copied from older project. this is an old version of my sdl debug text code
    public static class DebugText
    {
        // documentation texte
        // 
        //   text: le texte qui sera affiché à l'écran
        //         charactères supportés: a-z, 0-9, +, -, é, è, ê, à,  , ., ,, ', :, \, /, ", (, ), \n
        //         lettres sont majuscule seuelement, mais le texte qui rentre dans la fonction doit être minuscule, les majuscules seront automatiquement
        //         convertis en minuscules avant d'êtres déssinés.
        //         \n fonctionne et est la seule facon de passer à une prochaine ligne dans le même appel de texte, et quand la ligne est sautée, il revient
        //         au x de départ.
        //   x, y: position haut gauche du premier charactère affiché.
        //         mettre short.MinValue (ou -32768) va centrer le texte au millieu de l'écran.
        //   size: nombre entier qui donne le multiple de la largeure et hauteure.
        //         la largeur d'un charactère sera de 5 * size, et la hauteur de 10 * size.
        //  color: couleure RGB du texte, où blanc est la valeure par défaut
        //         R, G et B attachés ensemble en un chiffre, où les bits 23 à 16 sont pour le rouge, 15 à 8 pour le vert et 7 à 0 pour le bleu.
        //  alpha: transparence du texte, 100% opaque par défaut. Ceci sera la valeure a du RGBA de sdl, et va automatiquement
        //         arrondir à une valeure de byte si dépassement.
        // scroll: le nb. de charactères que seront affichés à l'écran, peu importe la longeure du texte.
        //         int.MaxValue par défaut, ce qui le met automatiquement à la longeure du texte, car si la valeure entrée est plus grande que la longeure du texte,
        //         scroll sera ajusté pour ne pas dépasser la longeure du texte. si scroll est négatif, aucun texte n'est affiché.
        //
        // toutes ces valeures peuvent êtres complètements différentes d'une image à l'autre, mais marchent mieux avec un changement lent.
        private static short extra_y = 0;
        private static int ret_length = 0;
        static int w, h;
        private static void DisplayChar(char charac, int x, int y, byte size, short i, byte r, byte g, byte b, byte a)
        {
            SDL_SetRenderDrawColor(Screen.render, r, g, b, a);
            y += extra_y;
            x -= ret_length;
            if (x + size * 5 > w)
            {
                extra_y += (short)(13 * size);
                ret_length = (i + 1) * 8 * size;
            }
            switch (charac)
            {
                case 'a':
                    SDL_RenderDrawLine(Screen.render, x, y + 10 * size, x, y);
                    SDL_RenderDrawLine(Screen.render, x, y, x + 5 * size, y);
                    SDL_RenderDrawLine(Screen.render, x + 5 * size, y, x + 5 * size, y + 10 * size);
                    SDL_RenderDrawLine(Screen.render, x + 5 * size, y + 5 * size, x, y + 5 * size);
                    break;
                case 'b':
                    SDL_RenderDrawLine(Screen.render, x, y + 10 * size, x, y);
                    SDL_RenderDrawLine(Screen.render, x, y, x + 5 * size, y);
                    SDL_RenderDrawLine(Screen.render, x + 5 * size, y, x + 5 * size, y + 3 * size);
                    SDL_RenderDrawLine(Screen.render, x + 5 * size, y + 3 * size, x, y + 5 * size);
                    SDL_RenderDrawLine(Screen.render, x, y + 5 * size, x + 5 * size, y + 7 * size);
                    SDL_RenderDrawLine(Screen.render, x + 5 * size, y + 7 * size, x + 5 * size, y + 10 * size);
                    SDL_RenderDrawLine(Screen.render, x + 5 * size, y + 10 * size, x, y + 10 * size);
                    break;
                case 'c':
                    SDL_RenderDrawLine(Screen.render, x, y + 10 * size, x, y);
                    SDL_RenderDrawLine(Screen.render, x, y, x + 5 * size, y);
                    SDL_RenderDrawLine(Screen.render, x + 5 * size, y + 10 * size, x, y + 10 * size);
                    break;
                case 'd':
                    SDL_RenderDrawLine(Screen.render, x, y + 10 * size, x, y);
                    SDL_RenderDrawLine(Screen.render, x, y, x + 2 * size, y);
                    SDL_RenderDrawLine(Screen.render, x + 2 * size, y + 10 * size, x, y + 10 * size);
                    SDL_RenderDrawLine(Screen.render, x + 2 * size, y + 10 * size, x + 5 * size, y + 5 * size);
                    SDL_RenderDrawLine(Screen.render, x + 2 * size, y, x + 5 * size, y + 5 * size);
                    break;
                case 'e':
                    SDL_RenderDrawLine(Screen.render, x, y + 10 * size, x, y);
                    SDL_RenderDrawLine(Screen.render, x, y, x + 5 * size, y);
                    SDL_RenderDrawLine(Screen.render, x + 5 * size, y + 10 * size, x, y + 10 * size);
                    SDL_RenderDrawLine(Screen.render, x + 5 * size, y + 5 * size, x, y + 5 * size);
                    break;
                case 'f':
                    SDL_RenderDrawLine(Screen.render, x, y + 10 * size, x, y);
                    SDL_RenderDrawLine(Screen.render, x, y, x + 5 * size, y);
                    SDL_RenderDrawLine(Screen.render, x + 5 * size, y + 5 * size, x, y + 5 * size);
                    break;
                case 'g':
                    SDL_RenderDrawLine(Screen.render, x, y + 10 * size, x, y);
                    SDL_RenderDrawLine(Screen.render, x, y, x + 5 * size, y);
                    SDL_RenderDrawLine(Screen.render, x + 5 * size, y + 10 * size, x, y + 10 * size);
                    SDL_RenderDrawLine(Screen.render, x + 5 * size, y + 10 * size, x + 5 * size, y + 5 * size);
                    SDL_RenderDrawLine(Screen.render, x + 5 * size, y + 5 * size, x + 3 * size, y + 5 * size);
                    break;
                case 'h':
                    SDL_RenderDrawLine(Screen.render, x, y + 10 * size, x, y);
                    SDL_RenderDrawLine(Screen.render, x + 5 * size, y, x + 5 * size, y + 10 * size);
                    SDL_RenderDrawLine(Screen.render, x + 5 * size, y + 5 * size, x, y + 5 * size);
                    break;
                case 'i':
                    SDL_RenderDrawLine(Screen.render, x, y, x + 5 * size, y);
                    SDL_RenderDrawLine(Screen.render, (int)(x + 2.5f * size), y, (int)(x + 2.5f * size), y + 10 * size);
                    SDL_RenderDrawLine(Screen.render, x + 5 * size, y + 10 * size, x, y + 10 * size);
                    break;
                case 'j':
                    SDL_RenderDrawLine(Screen.render, x + 5 * size, y, x + 5 * size, y + 10 * size);
                    SDL_RenderDrawLine(Screen.render, x + 5 * size, y + 10 * size, x, y + 10 * size);
                    SDL_RenderDrawLine(Screen.render, x, y + 10 * size, x, y + 7 * size);
                    break;
                case 'k':
                    SDL_RenderDrawLine(Screen.render, x, y + 10 * size, x, y);
                    SDL_RenderDrawLine(Screen.render, x, y + 5 * size, x + 5 * size, y);
                    SDL_RenderDrawLine(Screen.render, x, y + 5 * size, x + 5 * size, y + 10 * size);
                    break;
                case 'l':
                    SDL_RenderDrawLine(Screen.render, x, y + 10 * size, x, y);
                    SDL_RenderDrawLine(Screen.render, x + 5 * size, y + 10 * size, x, y + 10 * size);
                    break;
                case 'm':
                    SDL_RenderDrawLine(Screen.render, x, y + 10 * size, x, y);
                    SDL_RenderDrawLine(Screen.render, x + 5 * size, y, x + 5 * size, y + 10 * size);
                    SDL_RenderDrawLine(Screen.render, x, y, (int)(x + 2.5f * size), y + 5 * size);
                    SDL_RenderDrawLine(Screen.render, x + 5 * size, y, (int)(x + 2.5f * size), y + 5 * size);
                    break;
                case 'n':
                    SDL_RenderDrawLine(Screen.render, x, y + 10 * size, x, y);
                    SDL_RenderDrawLine(Screen.render, x + 5 * size, y, x + 5 * size, y + 10 * size);
                    SDL_RenderDrawLine(Screen.render, x, y, x + 5 * size, y + 10 * size);
                    break;
                case 'o':
                    SDL_RenderDrawLine(Screen.render, x, y + 10 * size, x, y);
                    SDL_RenderDrawLine(Screen.render, x + 5 * size, y, x + 5 * size, y + 10 * size);
                    SDL_RenderDrawLine(Screen.render, x, y, x + 5 * size, y);
                    SDL_RenderDrawLine(Screen.render, x + 5 * size, y + 10 * size, x, y + 10 * size);
                    break;
                case 'p':
                    SDL_RenderDrawLine(Screen.render, x, y + 10 * size, x, y);
                    SDL_RenderDrawLine(Screen.render, x, y, x + 5 * size, y);
                    SDL_RenderDrawLine(Screen.render, x + 5 * size, y, x + 5 * size, y + 5 * size);
                    SDL_RenderDrawLine(Screen.render, x + 5 * size, y + 5 * size, x, y + 5 * size);
                    break;
                case 'q':
                    SDL_RenderDrawLine(Screen.render, x, y + 10 * size, x, y);
                    SDL_RenderDrawLine(Screen.render, x + 4 * size, y, x + 4 * size, y + 10 * size);
                    SDL_RenderDrawLine(Screen.render, x, y, x + 4 * size, y);
                    SDL_RenderDrawLine(Screen.render, x + 4 * size, y + 10 * size, x, y + 10 * size);
                    SDL_RenderDrawLine(Screen.render, x + 5 * size, y + 10 * size, x + 3 * size, y + 5 * size);
                    break;
                case 'r':
                    SDL_RenderDrawLine(Screen.render, x, y + 10 * size, x, y);
                    SDL_RenderDrawLine(Screen.render, x, y, x + 5 * size, y);
                    SDL_RenderDrawLine(Screen.render, x + 5 * size, y, x + 5 * size, y + 5 * size);
                    SDL_RenderDrawLine(Screen.render, x + 5 * size, y + 5 * size, x, y + 5 * size);
                    SDL_RenderDrawLine(Screen.render, x + 2 * size, y + 5 * size, x + 5 * size, y + 10 * size);
                    break;
                case 's':
                    SDL_RenderDrawLine(Screen.render, x, y, x + 5 * size, y);
                    SDL_RenderDrawLine(Screen.render, x + 5 * size, y + 10 * size, x, y + 10 * size);
                    SDL_RenderDrawLine(Screen.render, x + 5 * size, y + 5 * size, x, y + 5 * size);
                    SDL_RenderDrawLine(Screen.render, x, y, x, y + 5 * size);
                    SDL_RenderDrawLine(Screen.render, x + 5 * size, y + 10 * size, x + 5 * size, y + 5 * size);
                    break;
                case 't':
                    SDL_RenderDrawLine(Screen.render, (int)(x + 2.5f * size), y, (int)(x + 2.5f * size), y + 10 * size);
                    SDL_RenderDrawLine(Screen.render, x, y, x + 5 * size, y);
                    break;
                case 'u':
                    SDL_RenderDrawLine(Screen.render, x, y + 10 * size, x, y);
                    SDL_RenderDrawLine(Screen.render, x + 5 * size, y, x + 5 * size, y + 10 * size);
                    SDL_RenderDrawLine(Screen.render, x + 5 * size, y + 10 * size, x, y + 10 * size);
                    break;
                case 'v':
                    SDL_RenderDrawLine(Screen.render, x, y, (int)(x + 2.5f * size), y + 10 * size);
                    SDL_RenderDrawLine(Screen.render, x + 5 * size, y, (int)(x + 2.5f * size), y + 10 * size);
                    break;
                case 'w':
                    SDL_RenderDrawLine(Screen.render, x, y + 10 * size, x, y);
                    SDL_RenderDrawLine(Screen.render, x + 5 * size, y, x + 5 * size, y + 10 * size);
                    SDL_RenderDrawLine(Screen.render, x + 5 * size, y + 10 * size, x, y + 10 * size);
                    SDL_RenderDrawLine(Screen.render, (int)(x + 2.5f * size), y + 10 * size, (int)(x + 2.5f * size), y + 4 * size);
                    break;
                case 'x':
                    SDL_RenderDrawLine(Screen.render, x, y, x + 5 * size, y + 10 * size);
                    SDL_RenderDrawLine(Screen.render, x + 5 * size, y, x, y + 10 * size);
                    break;
                case 'y':
                    SDL_RenderDrawLine(Screen.render, x + 5 * size, y, x, y + 10 * size);
                    SDL_RenderDrawLine(Screen.render, x, y, (int)(x + 2.5f * size), y + 5 * size);
                    break;
                case 'z':
                    SDL_RenderDrawLine(Screen.render, x, y, x + 5 * size, y);
                    SDL_RenderDrawLine(Screen.render, x + 5 * size, y + 10 * size, x, y + 10 * size);
                    SDL_RenderDrawLine(Screen.render, x + 5 * size, y, x, y + 10 * size);
                    break;
                case '0':
                    SDL_RenderDrawLine(Screen.render, x + 5 * size, y, x, y + 10 * size);
                    SDL_RenderDrawLine(Screen.render, x, y + 10 * size, x, y);
                    SDL_RenderDrawLine(Screen.render, x + 5 * size, y, x + 5 * size, y + 10 * size);
                    SDL_RenderDrawLine(Screen.render, x, y, x + 5 * size, y);
                    SDL_RenderDrawLine(Screen.render, x + 5 * size, y + 10 * size, x, y + 10 * size);
                    break;
                case '1':
                    SDL_RenderDrawLine(Screen.render, x, y + 3 * size, (int)(x + 2.5f * size), y);
                    SDL_RenderDrawLine(Screen.render, (int)(x + 2.5f * size), y, (int)(x + 2.5f * size), y + 10 * size);
                    SDL_RenderDrawLine(Screen.render, x + 5 * size, y + 10 * size, x, y + 10 * size);
                    break;
                case '2':
                    SDL_RenderDrawLine(Screen.render, x, y + 3 * size, x + 1 * size, y);
                    SDL_RenderDrawLine(Screen.render, x + 1 * size, y, x + 4 * size, y);
                    SDL_RenderDrawLine(Screen.render, x + 5 * size, y + 3 * size, x + 4 * size, y);
                    SDL_RenderDrawLine(Screen.render, x + 5 * size, y + 3 * size, x, y + 10 * size);
                    SDL_RenderDrawLine(Screen.render, x + 5 * size, y + 10 * size, x, y + 10 * size);
                    break;
                case '3':
                    SDL_RenderDrawLine(Screen.render, x + 5 * size, y + 10 * size, x + 5 * size, y);
                    SDL_RenderDrawLine(Screen.render, x, y, x + 5 * size, y);
                    SDL_RenderDrawLine(Screen.render, x + 5 * size, y + 10 * size, x, y + 10 * size);
                    SDL_RenderDrawLine(Screen.render, x + 5 * size, y + 5 * size, x, y + 5 * size);
                    break;
                case '4':
                    SDL_RenderDrawLine(Screen.render, x, y + 5 * size, x, y);
                    SDL_RenderDrawLine(Screen.render, x + 5 * size, y, x + 5 * size, y + 10 * size);
                    SDL_RenderDrawLine(Screen.render, x + 5 * size, y + 5 * size, x, y + 5 * size);
                    break;
                case '5':
                    SDL_RenderDrawLine(Screen.render, x + 5 * size, y, x, y);
                    SDL_RenderDrawLine(Screen.render, x, y + 5 * size, x, y);
                    SDL_RenderDrawLine(Screen.render, x + 5 * size, y + 5 * size, x, y + 5 * size);
                    SDL_RenderDrawLine(Screen.render, x + 5 * size, y + 5 * size, x + 5 * size, y + 8 * size);
                    SDL_RenderDrawLine(Screen.render, x + 5 * size, y + 8 * size, x + 4 * size, y + 10 * size);
                    SDL_RenderDrawLine(Screen.render, x + 4 * size, y + 10 * size, x + 1 * size, y + 10 * size);
                    SDL_RenderDrawLine(Screen.render, x + 1 * size, y + 10 * size, x, y + 8 * size);
                    break;
                case '6':
                    SDL_RenderDrawLine(Screen.render, x, y + 10 * size, x, y);
                    SDL_RenderDrawLine(Screen.render, x, y, x + 5 * size, y);
                    SDL_RenderDrawLine(Screen.render, x + 5 * size, y + 10 * size, x, y + 10 * size);
                    SDL_RenderDrawLine(Screen.render, x + 5 * size, y + 10 * size, x + 5 * size, y + 5 * size);
                    SDL_RenderDrawLine(Screen.render, x + 5 * size, y + 5 * size, x, y + 5 * size);
                    break;
                case '7':
                    SDL_RenderDrawLine(Screen.render, x + 5 * size, y, x, y);
                    SDL_RenderDrawLine(Screen.render, x + 5 * size, y, x, y + 10 * size);
                    break;
                case '8':
                    SDL_RenderDrawLine(Screen.render, x + 5 * size, y + 5 * size, x, y + 5 * size);
                    SDL_RenderDrawLine(Screen.render, x, y + 10 * size, x, y);
                    SDL_RenderDrawLine(Screen.render, x + 5 * size, y, x + 5 * size, y + 10 * size);
                    SDL_RenderDrawLine(Screen.render, x, y, x + 5 * size, y);
                    SDL_RenderDrawLine(Screen.render, x + 5 * size, y + 10 * size, x, y + 10 * size);
                    break;
                case '9':
                    SDL_RenderDrawLine(Screen.render, x + 5 * size, y + 10 * size, x + 5 * size, y);
                    SDL_RenderDrawLine(Screen.render, x, y, x + 5 * size, y);
                    SDL_RenderDrawLine(Screen.render, x + 5 * size, y + 10 * size, x, y + 10 * size);
                    SDL_RenderDrawLine(Screen.render, x, y, x, y + 5 * size);
                    SDL_RenderDrawLine(Screen.render, x + 5 * size, y + 5 * size, x, y + 5 * size);
                    break;
                case ' ':
                    break;
                case '.':
                    SDL_RenderDrawPoint(Screen.render, x, y + 10 * size);
                    break;
                case ':':
                    SDL_RenderDrawPoint(Screen.render, x, y + 3 * size);
                    SDL_RenderDrawPoint(Screen.render, x, y + 7 * size);
                    break;
                case '\n':
                    extra_y += (short)(13 * size);
                    ret_length = (i + 1) * 8 * size;
                    break;
                case ',':
                    SDL_RenderDrawLine(Screen.render, x + 2 * size, y + 8 * size, x, y + 10 * size);
                    break;
                case '\'':
                    SDL_RenderDrawLine(Screen.render, x + 2 * size, y + 2 * size, x, y);
                    break;
                case 'é':
                    SDL_RenderDrawLine(Screen.render, x, y + 10 * size, x, y);
                    SDL_RenderDrawLine(Screen.render, x, y, x + 5 * size, y);
                    SDL_RenderDrawLine(Screen.render, x + 5 * size, y + 10 * size, x, y + 10 * size);
                    SDL_RenderDrawLine(Screen.render, x + 5 * size, y + 5 * size, x, y + 5 * size);
                    SDL_RenderDrawLine(Screen.render, x + 1 * size, y - 2, x + 4 * size, y - 4);
                    break;
                case 'è':
                    SDL_RenderDrawLine(Screen.render, x, y + 10 * size, x, y);
                    SDL_RenderDrawLine(Screen.render, x, y, x + 5 * size, y);
                    SDL_RenderDrawLine(Screen.render, x + 5 * size, y + 10 * size, x, y + 10 * size);
                    SDL_RenderDrawLine(Screen.render, x + 5 * size, y + 5 * size, x, y + 5 * size);
                    SDL_RenderDrawLine(Screen.render, x + 1 * size, y - 4, x + 4 * size, y - 2);
                    break;
                case 'à':
                    SDL_RenderDrawLine(Screen.render, x + 1 * size, y - 2, x + 4 * size, y);
                    SDL_RenderDrawLine(Screen.render, x, y + 10 * size, x, y);
                    SDL_RenderDrawLine(Screen.render, x, y, x + 5 * size, y);
                    SDL_RenderDrawLine(Screen.render, x + 5 * size, y, x + 5 * size, y + 10 * size);
                    SDL_RenderDrawLine(Screen.render, x + 5 * size, y + 5 * size, x, y + 5 * size);
                    break;
                case '"':
                    SDL_RenderDrawLine(Screen.render, x + 2 * size, y, x + 2 * size, y + 3 * size);
                    SDL_RenderDrawLine(Screen.render, x + 4 * size, y, x + 4 * size, y + 3 * size);
                    break;
                case '-':
                    SDL_RenderDrawLine(Screen.render, x + 1 * size, y + 5 * size, x + 4 * size, y + 5 * size);
                    break;
                case 'ê':
                    SDL_RenderDrawLine(Screen.render, x, y + 10 * size, x, y);
                    SDL_RenderDrawLine(Screen.render, x, y, x + 5 * size, y);
                    SDL_RenderDrawLine(Screen.render, x + 5 * size, y + 10 * size, x, y + 10 * size);
                    SDL_RenderDrawLine(Screen.render, x + 5 * size, y + 5 * size, x, y + 5 * size);
                    SDL_RenderDrawLine(Screen.render, x + 1 * size, y - 2, (int)(x + 2.5f * size), y - 4);
                    SDL_RenderDrawLine(Screen.render, x + 4 * size, y - 2, (int)(x + 2.5f * size), y - 4);
                    break;
                case '/':
                    SDL_RenderDrawLine(Screen.render, x, y + 10 * size, x + 5 * size, y);
                    break;
                case '\\':
                    SDL_RenderDrawLine(Screen.render, x + 5 * size, y + 10 * size, x, y);
                    break;
                case '(':
                    SDL_RenderDrawLine(Screen.render, x + 4 * size, y, x + 2 * size, y + 3 * size);
                    SDL_RenderDrawLine(Screen.render, x + 2 * size, y + 3 * size, x + 2 * size, y + 7 * size);
                    SDL_RenderDrawLine(Screen.render, x + 4 * size, y + 10 * size, x + 2 * size, y + 7 * size);
                    break;
                case ')':
                    SDL_RenderDrawLine(Screen.render, x + 1 * size, y, x + 3 * size, y + 3 * size);
                    SDL_RenderDrawLine(Screen.render, x + 3 * size, y + 3 * size, x + 3 * size, y + 7 * size);
                    SDL_RenderDrawLine(Screen.render, x + 1 * size, y + 10 * size, x + 3 * size, y + 7 * size);
                    break;
                case '+':
                    SDL_RenderDrawLine(Screen.render, x + 0 * size, y + 5 * size, x + 4 * size, y + 5 * size);
                    SDL_RenderDrawLine(Screen.render, x + (int)(2.5f * size), y + 3 * size, x + (int)(2.5f * size), y + 7 * size);
                    break;
                    //case '':
                    //    break;
            }
        }
        public static void DisplayText(string text, int x, int y, byte size, int color = 0xFFFFFF, short alpha = 255, int scroll = int.MaxValue)
        {
            extra_y = 0;
            ret_length = 0;

            if (scroll > text.Length)
                scroll = text.Length;
            if (scroll < -1)
                return;

            if (alpha < 0)
                alpha = 0;
            if (alpha > 255)
                alpha = 255;

            if (x == short.MinValue)
                x = (short)(128 - (8 * size * text.Length - 1) / 2);
            if (y == short.MinValue)
                y = (short)(128 - 10 * size / 2);

            text = text.ToLower();
            for (short i = 0; i < scroll; i++)
            {
                DisplayChar(text[i], x + i * 8 * size, y, size, i, (byte)((color & 0xFF0000) >> 16), (byte)((color & 0x00FF00) >> 8), (byte)(color & 0x0000FF), (byte)alpha);
            }
        }
        public static void SetScreenDimensions(int new_w, int new_h)
        {
            w = new_w;
            h = new_h;
        }
    }
}