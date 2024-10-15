using static SDL2.SDL;
namespace The_Legend_of_Zelda
{
    internal enum Buttons
    {
        UP,
        LEFT,
        DOWN,
        RIGHT,
        A,
        B,
        START,
        SELECT
    }

    internal static class Control
    {
        static byte held_buttons = 0;
        static byte pressed_buttons = 0;

        static bool can_screenshot = true;

        static void Push(Buttons button)
        {
            byte index = (byte)(1 << (int)button);
            if (((pressed_buttons & index) > 0) && ((held_buttons & index) > 0))
                pressed_buttons -= index;
            if ((held_buttons & index) == 0)
            {
                held_buttons += index;
                pressed_buttons += index;
            }
        }
        static void Release(Buttons button)
        {
            byte index = (byte)(1 << (int)button);
            if ((held_buttons & index) > 0)
                held_buttons -= index;
        }
        public static bool IsPressed(Buttons button)
        {
            return ((pressed_buttons & (byte)(1 << (int)button)) > 0);
        }
        public static bool IsHeld(Buttons button)
        {
            return ((held_buttons & (byte)(1 << (int)button)) > 0);
        }
        public static void Tick()
        {
            pressed_buttons = 0;
            while (SDL_PollEvent(out Program.e) == 1)
            {
                switch (Program.e.type)
                {
                    case SDL_EventType.SDL_QUIT:
                        Program.exit = true;
                        break;
                    case SDL_EventType.SDL_KEYDOWN:
                        switch (Program.e.key.keysym.sym)
                        {
                            case SDL_Keycode.SDLK_ESCAPE:
                                Program.exit = true;
                                break;
                            case SDL_Keycode.SDLK_w:
                                Push(Buttons.UP);
                                break;
                            case SDL_Keycode.SDLK_a:
                                Push(Buttons.LEFT);
                                break;
                            case SDL_Keycode.SDLK_s:
                                Push(Buttons.DOWN);
                                break;
                            case SDL_Keycode.SDLK_d:
                                Push(Buttons.RIGHT);
                                break;
                            case SDL_Keycode.SDLK_j:
                                Push(Buttons.B);
                                break;
                            case SDL_Keycode.SDLK_k:
                                Push(Buttons.A);
                                break;
                            case SDL_Keycode.SDLK_g:
                                Push(Buttons.SELECT);
                                break;
                            case SDL_Keycode.SDLK_h:
                                Push(Buttons.START);
                                break;
                            case SDL_Keycode.SDLK_TAB:
                                if (Program.fast_forward_with_tab)
                                    Program.uncap_fps = true;
                                break;
                            case SDL_Keycode.SDLK_F1:
                                if (can_screenshot)
                                {
                                    Program.Screenshot();
                                    can_screenshot = false;
                                }
                                break;
                        }
                        break;
                    case SDL_EventType.SDL_KEYUP:
                        switch (Program.e.key.keysym.sym)
                        {
                            case SDL_Keycode.SDLK_w:
                                Release(Buttons.UP);
                                break;
                            case SDL_Keycode.SDLK_a:
                                Release(Buttons.LEFT);
                                break;
                            case SDL_Keycode.SDLK_s:
                                Release(Buttons.DOWN);
                                break;
                            case SDL_Keycode.SDLK_d:
                                Release(Buttons.RIGHT);
                                break;
                            case SDL_Keycode.SDLK_j:
                                Release(Buttons.B);
                                break;
                            case SDL_Keycode.SDLK_k:
                                Release(Buttons.A);
                                break;
                            case SDL_Keycode.SDLK_g:
                                Release(Buttons.SELECT);
                                break;
                            case SDL_Keycode.SDLK_h:
                                Release(Buttons.START);
                                break;
                            case SDL_Keycode.SDLK_TAB:
                                Program.uncap_fps = false;
                                break;
                            case SDL_Keycode.SDLK_F1:
                                can_screenshot = true;
                                break;
                        }
                        break;
                }
            }
        }
    }
}