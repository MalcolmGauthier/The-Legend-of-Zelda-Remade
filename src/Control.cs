﻿using The_Legend_of_Zelda.Gameplay;
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
        static int held_buttons = 0;
        static int pressed_buttons = 0;

        static bool pause_held = false;
        static bool frame_adv_held = false;
        static int frame_adv_held_timer = 0;
        static bool screenshot_held = false;
        static bool reset_held = false;

        static void Push(Buttons button)
        {
            int index = 1 << (int)button;

            // if button is in both pressed buttons and held buttons, then it's been held for more than 1 frame, so it's no longer pressed
            if ((pressed_buttons & index) != 0 && (held_buttons & index) != 0)
                pressed_buttons -= index;

            // if button is not in held buttons, that means it just got pressed. put it in pressed and held buttons
            if ((held_buttons & index) == 0)
            {
                held_buttons += index;
                pressed_buttons += index;
            }
        }

        static void Release(Buttons button)
        {
            int index = 1 << (int)button;

            // if button in held buttons, remove it from there
            if ((held_buttons & index) != 0)
                held_buttons -= index;

            // no need to check for if button is in pressed buttons, pressed buttons gets reset before each keyboard poll
        }

        public static bool IsPressed(Buttons button)
        {
            return (pressed_buttons & 1 << (int)button) > 0;
        }

        public static bool IsHeld(Buttons button)
        {
            return (held_buttons & 1 << (int)button) > 0;
        }

        public static void Poll()
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
                                if (Program.fast_forward)
                                    Program.uncap_fps = true;
                                break;
                            case SDL_Keycode.SDLK_PERIOD:
                                frame_adv_held_timer++;
                                if (Program.frame_advance && (!frame_adv_held || frame_adv_held_timer > 15))
                                {
                                    Program.advance_1_frame = true;
                                    frame_adv_held = true;
                                }
                                break;
                            case SDL_Keycode.SDLK_p:
                                if (Program.pause_whenever && !pause_held)
                                {
                                    Program.full_pause = !Program.full_pause;
                                    pause_held = true;
                                }
                                break;
                            case SDL_Keycode.SDLK_F1:
                                if (!screenshot_held)
                                {
                                    Program.Screenshot();
                                    screenshot_held = true;
                                }
                                break;
                            case SDL_Keycode.SDLK_r:
                                if (!reset_held)
                                {
                                    reset_held = true;
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
                                screenshot_held = false;
                                break;
                            case SDL_Keycode.SDLK_PERIOD:
                                frame_adv_held = false;
                                frame_adv_held_timer = 0;
                                break;
                            case SDL_Keycode.SDLK_p:
                                pause_held = false;
                                break;
                            case SDL_Keycode.SDLK_r:
                                if (reset_held)
                                {
                                    reset_held = false;
                                    Program.Reset();
                                }
                                break;
                        }
                        break;
                }
            }
        }
    }
}