using static The_Legend_of_Zelda.OverworldCode;
using static The_Legend_of_Zelda.SaveLoad;
using static The_Legend_of_Zelda.Link;
namespace The_Legend_of_Zelda
{
    public static class WarpCode
    {
        public static byte warp_info;
        static byte text_counter = 0;
        static byte item_get_timer = 200;

        static int starting_byte_1 = 0;
        static int starting_byte_2 = 0;

        public static bool fire_appeared = false;
        static bool item_collected = false;

        public static Sprite[] items = new Sprite[3];
        static CaveNPC cave_npc = new CaveNPC(NPC.NONE);
        static FlickeringSprite side_rupee = new FlickeringSprite(0x32, 5, 52, 172, 8, 0x32, second_palette_index: 6);

        static NPC current_npc;

        static byte[] text_row_1 = new byte[0];
        static byte[] text_row_2 = new byte[0];
        public static readonly byte[] screen_warp_info = {
            0x00, 0x00, 0x00, 0x00, 0x04, 0x0E, 0x00, 0x00, 0x00, 0x00, 0x0B, 0x0E, 0x07, 0x04, 0x0B, 0x03,
            0x05, 0x00, 0x08, 0x02, 0x00, 0x00, 0x05, 0x00, 0x00, 0x00, 0x0C, 0x00, 0x0D, 0x06, 0x00, 0x05,
            0x00, 0x0B, 0x0E, 0x06, 0x00, 0x09, 0x08, 0x04, 0x02, 0x00, 0x00, 0x00, 0x0F, 0x02, 0x00, 0x0F,
            0x00, 0x00, 0x00, 0x04, 0x0A, 0x00, 0x00, 0x0E, 0x00, 0x00, 0x00, 0x00, 0x0E, 0x02, 0x00, 0x00,
            0x00, 0x00, 0x0E, 0x00, 0x09, 0x0E, 0x08, 0x0F, 0x02, 0x06, 0x09, 0x04, 0x00, 0x08, 0x01, 0x00,
            0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x07, 0x00,
            0x00, 0x00, 0x03, 0x00, 0x04, 0x00, 0x07, 0x02, 0x00, 0x00, 0x00, 0x03, 0x00, 0x0E, 0x00, 0x09,
            0x0C, 0x02, 0x00, 0x00, 0x0E, 0x0D, 0x05, 0x0B, 0x04, 0x06, 0x00, 0x0F, 0x05, 0x00, 0x00, 0x00
        };
        static readonly byte[] gift_flag_list = {
            1, 3, 7, 15, 19, 20, 30, 40, 44, 45, 47, 61, 71, 72, 78, 81, 86, 91, 98, 99, 103, 104, 106, 107, 113, 125
        };
        static readonly byte[] dungeon_locations = {
            55, 60, 116, 69, 11, 34, 66, 109, 5
        };
        public enum NPC
        {
            NONE,
            OLD_MAN,
            OLD_WOMAN,
            SHOPKEEPER,
            GOBLIN,
            ZELDA
        }
        public static void Init(bool overworld)
        {
            for (int i = 0; i < items.Length; i++)
                items[i] = new StaticSprite(0x00, 4, (short)(92 + (i * 32)), 152);
            text_row_1 = new byte[0];
            text_row_2 = new byte[0];
            text_counter = 0;
            current_npc = NPC.NONE;
            item_collected = false;
            side_rupee.unload_during_transition = true;
            side_rupee.shown = true;
            room_66_timer = 255;
            if (overworld)
            {
                warp_info = screen_warp_info[return_screen];
                text_counter = 0;

                for (int i = 0; i < items.Length; i++)
                    items[i].shown = false;

                switch (warp_info)
                {
                    case 0 or 1 or 2 or 3:
                        if (!SaveLoad.GetGiftFlag(current_save_file, (byte)Array.IndexOf(gift_flag_list, return_screen)))
                        {
                            if (warp_info == 0)
                            {
                                current_npc = NPC.OLD_MAN;
                                SaveLoad.SetGiftFlag(current_save_file, (byte)Array.IndexOf(gift_flag_list, return_screen), true);
                                text_row_1 = new byte[19] { 0x19, 0xa, 0x22, 0x24, 0x16, 0xe, 0x24, 0xf, 0x18, 0x1b, 0x24, 0x1d, 0x11, 0xe, 0x24, 0xd, 0x18, 0x18, 0x1b }; // PAY ME FOR THE DOOR
                                text_row_2 = new byte[14] { 0x1b, 0xe, 0x19, 0xa, 0x12, 0x1b, 0x24, 0xc, 0x11, 0xa, 0x1b, 0x10, 0xe, 0x2c }; // REPAIR CHARGE.
                            }
                            else
                            {
                                current_npc = NPC.GOBLIN;
                                text_row_1 = new byte[13] { 0x12, 0x1d, 0x2a, 0x1c, 0x24, 0xa, 0x24, 0x1c, 0xe, 0xc, 0x1b, 0xe, 0x1d }; // IT'S A SECRET
                                text_row_2 = new byte[13] { 0x1d, 0x18, 0x24, 0xe, 0x1f, 0xe, 0x1b, 0x22, 0xb, 0x18, 0xd, 0x22, 0x2c }; // TO EVERYBODY.

                                items[1] = new FlickeringSprite(0x32, 6, 124, 152, 8, 0x32, second_palette_index: 5);
                                items[1].shown = true;
                                Screen.sprites.Add(items[1]);
                            }
                        }
                        break;
                    case 4:
                        current_npc = NPC.OLD_WOMAN;
                        if (SaveLoad.potion_shop_activated[current_save_file])
                        {
                            text_row_1 = new byte[19] {0xb,0x1e,0x22,0x24,0x16,0xe,0xd,0x12,0xc,0x12,0x17,0xe,0x24,0xb,0xe,0xf,0x18,0x1b,0xe}; // BUY MEDICINE BEFORE
                            text_row_2 = new byte[19] {0x22,0x18,0x1e,0x24,0x10,0x18,0x2c,0x24,0x24,0x24,0x24,0x24,0x24,0x24,0x24,0x24,0x24,0x24,0x24}; // YOU GO.

                            for (int i = 0; i < items.Length; i+=2)
                            {
                                items[i].tile_index = 0x40;
                                items[i].palette_index = (byte)(5 + (i >> 1));
                                items[i].shown = true;
                                Screen.sprites.Add(items[i]);
                            }

                            Screen.sprites.Add(side_rupee);

                            Textures.ppu[0x2c8] = 0x21;
                            Textures.ppu[0x2cb] = 4;
                            Textures.ppu[0x2cc] = 0;
                            Textures.ppu[0x2d3] = 6;
                            Textures.ppu[0x2d4] = 8;
                        }
                        break;
                    case 5:
                        current_npc = NPC.OLD_MAN;
                        text_row_1 = new byte[17] {0x15,0xe,0x1d,0x2a,0x1c,0x24,0x19,0x15,0xa,0x22,0x24,0x16,0x18,0x17,0xe,0x22,0x24}; // LET'S PLAY MONEY 
                        text_row_2 = new byte[17] {0x16,0xa,0x14,0x12,0x17,0x10,0x24,0x10,0xa,0x16,0xe,0x2c,0x24,0x24,0x24,0x24,0x24}; // MAKING GAME.    

                        Screen.sprites.Add(side_rupee);
                        Textures.ppu[0x2c8] = 0x21;

                        for (int i = 0; i < items.Length; i++)
                        {
                            items[i] = new FlickeringSprite(0x32, 6, (short)(92 + (i * 32)), 152, 8, 0x32, second_palette_index: 5);
                            Screen.sprites.Add(items[i]);
                        }

                        for (int i = 0; i < 3; i++)
                        {
                            Textures.ppu[0x2ca + i * 4] = 0x62;
                            Textures.ppu[0x2cb + i * 4] = 1;
                            Textures.ppu[0x2cc + i * 4] = 0;
                        }
                        break;
                    case 6:
                        current_npc = NPC.OLD_MAN;
                        text_row_1 = new byte[23] {0x1d,0xa,0x14,0xe,0x24,0xa,0x17,0x22,0x24,0x1b,0x18,0xa,0xd,0x24,0x22,0x18,0x1e,0x24,0x20,0xa,0x17,0x1d,0x2c}; // TAKE ANY ROAD YOU WANT.
                        Screen.meta_tiles[101].tile_index = 0x15;
                        Screen.meta_tiles[104].tile_index = 0x15;
                        Screen.meta_tiles[107].tile_index = 0x15;
                        for (int i = 0; i < 3; i++)
                        {
                            Textures.ppu[0x28a + i * 6] = 0x70;
                            Textures.ppu[0x2aa + i * 6] = 0x71;
                            Textures.ppu[0x28b + i * 6] = 0x72;
                            Textures.ppu[0x2ab + i * 6] = 0x73;
                            Textures.ppu_plt[0x28a + i * 6] = 0;
                            Textures.ppu_plt[0x2aa + i * 6] = 0;
                            Textures.ppu_plt[0x28b + i * 6] = 0;
                            Textures.ppu_plt[0x2ab + i * 6] = 0;
                        }
                        break;
                    case 7 or 8 or 9 or 0xa:
                        current_npc = NPC.SHOPKEEPER;
                        Screen.sprites.Add(side_rupee);
                        Textures.ppu[0x2c8] = 0x21;

                        if ((warp_info & 1) == 0)
                        {
                            text_row_1 = new byte[12] {0xb,0x18,0x22,0x28,0x24,0x1d,0x11,0x12,0x1c,0x24,0x12,0x1c}; // BOY, THIS IS
                            text_row_2 = new byte[17] {0x1b,0xe,0xa,0x15,0x15,0x22,0x24,0xe,0x21,0x19,0xe,0x17,0x1c,0x12,0x1f,0xe,0x29}; // REALLY EXPENSIVE!
                        }
                        else
                        {
                            text_row_1 = new byte[22] {0xb,0x1e,0x22,0x24,0x1c,0x18,0x16,0xe,0x1d,0x11,0x12,0x17,0x2a,0x24,0x20,0x12,0x15,0x15,0x24,0x22,0xa,0x29}; // BUY SOMETHIN' WILL YA!
                        }

                        if (warp_info != 0xa)
                        {
                            items[0].tile_index = 0x56;
                            items[0].palette_index = 4;
                        }
                        else
                        {
                            items[0].tile_index = 0x2e;
                            items[0].palette_index = 6;
                        }
                        switch (warp_info)
                        {
                            case 7:
                                items[1].tile_index = 0x2e;
                                items[1].palette_index = 6;
                                break;
                            case 8:
                                items[1].tile_index = 0x22;
                                items[1].palette_index = 6;
                                break;
                            case 9:
                                items[1].tile_index = 0x34;
                                items[1].palette_index = 5;
                                break;
                            case 0xa:
                                items[1].tile_index = 0x46;
                                items[1].palette_index = 5;
                                break;
                        }
                        switch (warp_info)
                        {
                            case 7:
                                items[2].tile_index = 0x26;
                                items[2].palette_index = 5;
                                break;
                            case 8:
                                items[2] = new FlickeringSprite(0x1c, 6, 156, 152, 8, 0x1c, second_palette_index: 5);
                                break;
                            case 9:
                                items[2].tile_index = 0x28;
                                items[2].palette_index = 4;
                                break;
                            case 0xa:
                                items[2].tile_index = 0x22;
                                items[2].palette_index = 6;
                                break;
                        }

                        for (int i = 0; i < 3; i++)
                        {
                            Textures.ppu[0x2cc + i * 4] = 0;
                        }
                        if (warp_info == 7)
                        {
                            Textures.ppu[0x2ca] = 1;
                            Textures.ppu[0x2cb] = 6;
                            Textures.ppu[0x2ce] = 1;
                            Textures.ppu[0x2cf] = 0;
                            Textures.ppu[0x2d3] = 6;
                        }
                        else if (warp_info == 8)
                        {
                            Textures.ppu[0x2cb] = 9;
                            Textures.ppu[0x2ce] = 1;
                            Textures.ppu[0x2cf] = 0;
                            Textures.ppu[0x2d3] = 1;
                        }
                        else if (warp_info == 9)
                        {
                            Textures.ppu[0x2ca] = 1;
                            Textures.ppu[0x2cb] = 3;
                            Textures.ppu[0x2cf] = 2;
                            Textures.ppu[0x2d3] = 8;
                        }
                        else
                        {
                            Textures.ppu[0x2cb] = 8;
                            Textures.ppu[0x2ce] = 2;
                            Textures.ppu[0x2cf] = 5;
                            Textures.ppu[0x2d3] = 6;
                        }

                        for (int i = 0; i < items.Length; i ++)
                        {
                            items[i].shown = true;
                            Screen.sprites.Add(items[i]);
                        }
                        break;
                    case 0xb:
                        if (!CheckIfItemGotten())
                        {
                            current_npc = NPC.OLD_MAN;
                        }
                        break;
                    case 0xc:
                        current_npc = NPC.OLD_WOMAN;
                        text_row_1 = new byte[22] {0x24,0x19,0xa,0x22,0x24,0x16,0xe,0x24,0xa,0x17,0xd,0x24,0x12,0x2a,0x15,0x15,0x24,0x1d,0xa,0x15,0x14,0x2c}; //  PAY ME AND I'LL TALK.
                        Screen.sprites.Add(side_rupee);
                        Textures.ppu[0x2c8] = 0x21;

                        for (int i = 0; i < items.Length; i++)
                        {
                            items[i] = new FlickeringSprite(0x32, 6, (short)(92 + (i * 32)), 152, 8, 0x32, second_palette_index: 5);
                            Screen.sprites.Add(items[i]);
                        }

                        if (return_screen == 112)
                        {
                            for (int i = 0; i < 3; i++)
                            {
                                Textures.ppu[0x2ca + i * 4] = 0x62;
                                Textures.ppu[0x2cc + i * 4] = 0;
                            }
                            Textures.ppu[0x2cb] = 1;
                            Textures.ppu[0x2cf] = 3;
                            Textures.ppu[0x2d3] = 5;
                        }
                        else
                        {
                            for (int i = 0; i < 3; i++)
                            {
                                Textures.ppu[0x2cb + i * 3 + (i >> 1)] = 0x62;
                                Textures.ppu[0x2cc + i * 4] = 0;
                            }
                            Textures.ppu[0x2cc] = 5;
                            Textures.ppu[0x2cf] = 1;
                            Textures.ppu[0x2d3] = 2;
                        }
                        break;
                    case 0xd:
                        if (return_screen == 117)
                        {
                            current_npc = NPC.OLD_WOMAN;
                            text_row_1 = new byte[16] {0x16,0xe,0xe,0x1d,0x24,0x1d,0x11,0xe,0x24,0x18,0x15,0xd,0x24,0x16,0xa,0x17}; // MEET THE OLD MAN
                            text_row_2 = new byte[13] {0xa,0x1d,0x24,0x1d,0x11,0xe,0x24,0x10,0x1b,0xa,0x1f,0xe,0x2c}; // AT THE GRAVE.
                        }
                        else
                        {
                            current_npc = NPC.OLD_MAN;
                            text_row_1 = new byte[21] {0x1c,0xe,0xc,0x1b,0xe,0x1d,0x24,0x12,0x1c,0x24,0x12,0x17,0x24,0x1d,0x11,0xe,0x24,0x1d,0x1b,0xe,0xe}; // SECRET IS IN THE TREE
                            text_row_2 = new byte[16] {0xa,0x1d,0x24,0x1d,0x11,0xe,0x24,0xd,0xe,0xa,0xd,0x2f,0xe,0x17,0xd,0x2c}; // AT THE DEAD-END.
                        }
                        break;
                    case 0xe:
                        Program.gamemode = Program.Gamemode.DUNGEON;
                        DungeonCode.Init((byte)Array.IndexOf(dungeon_locations, return_screen));
                        return;
                    case 0xf:
                        if (!CheckIfItemGotten())
                        {
                            current_npc = NPC.OLD_MAN;
                            items[1].x = 152;
                            items[2].x = 159;
                            items[2].xflip = true;
                            for (int i = 0; i < items.Length; i++)
                            {
                                items[i].tile_index = 0x68;
                                items[i].palette_index = 6;
                                items[i].shown = true;
                                Screen.sprites.Add(items[i]);
                            }
                            items[0].tile_index = 0x40;
                            text_row_1 = new byte[22] {0x1d,0xa,0x14,0xe,0x24,0xa,0x17,0x22,0x24,0x18,0x17,0xe,0x24,0x22,0x18,0x1e,0x24,0x20,0xa,0x17,0x1d,0x2c}; // TAKE ANY ONE YOU WANT.
                        }
                        break;
                }
                starting_byte_1 = 0x1b0 - (text_row_1.Length >> 1);
                starting_byte_2 = 0x1d0 - (text_row_2.Length >> 1);

                for (int i = 0; i < items.Length; i++)
                    items[i].unload_during_transition = true;

                cave_npc = new CaveNPC(current_npc);
                Screen.sprites.Add(cave_npc);
            }
        }
        public static void Tick()
        {
            if (!Link.can_move)
                TextTick();

            if (Link.current_action != Link.Action.ITEM_GET && Link.current_action != Link.Action.ITEM_HELD_UP && !Menu.menu_open)
                Link.can_move = (text_counter >= text_row_1.Length + text_row_2.Length && fire_appeared);

            LinkItemCollection();

            if (warp_info == 4 && !potion_shop_activated[current_save_file] && Control.IsPressed(Control.Buttons.B) && Menu.current_B_item == 0x4c)
            {
                potion_shop_activated[current_save_file] = true;
                Link.can_move = false;
                Screen.sprites.Remove(cave_npc);
                Init(true);
            }
        }
        static void TextTick()
        {
            if (Program.gTimer % 6 == 0 && fire_appeared)
            {
                if (text_counter < text_row_1.Length)
                {
                    Sound.PlaySFX(Sound.SoundEffects.TEXT, true);
                    Textures.ppu[starting_byte_1 + text_counter] = text_row_1[text_counter];
                    text_counter++;
                }
                else if (text_counter < text_row_1.Length + text_row_2.Length)
                {
                    Sound.PlaySFX(Sound.SoundEffects.TEXT, true);
                    Textures.ppu[starting_byte_2 + text_counter - text_row_1.Length] = text_row_2[text_counter - text_row_1.Length];
                    text_counter++;
                    if (text_counter == text_row_1.Length + text_row_2.Length && warp_info == 0)
                        AddRupees(-20);
                }
            }
        }
        static bool CheckIfItemGotten()
        {
            switch (return_screen)
            {
                case 10 or 33:
                    if (!SaveLoad.white_sword[current_save_file])
                    {
                        text_row_1 = new byte[19] {0x16,0xa,0x1c,0x1d,0xe,0x1b,0x24,0x1e,0x1c,0x12,0x17,0x10,0x24,0x12,0x1d,0x24,0xa,0x17,0xd}; // MASTER USING IT AND
                        text_row_2 = new byte[19] {0x24,0x22,0x18,0x1e,0x24,0xc,0xa,0x17,0x24,0x11,0xa,0x1f,0xe,0x24,0x1d,0x11,0x12,0x1c,0x2c}; //  YOU CAN HAVE THIS.
                        if (return_screen == 33)
                        {
                            items[1].tile_index = 0x48;
                            items[1].palette_index = 6;
                        }
                        else
                        {
                            items[1].tile_index = 0x20;
                            items[1].palette_index = 5;
                        }
                        items[1].x = 124;
                        items[1].y = 152;
                        items[1].shown = true;
                        Screen.sprites.Add(items[1]);
                        return false;
                    }
                    break;
                case 14:
                    if (!SaveLoad.letter[current_save_file])
                    {
                        text_row_1 = new byte[16] {0x1c,0x11,0x18,0x20,0x24,0x1d,0x11,0x12,0x1c,0x24,0x1d,0x18,0x24,0x1d,0x11,0xe}; // SHOW THIS TO THE
                        text_row_2 = new byte[10] {0x18,0x15,0xd,0x24,0x20,0x18,0x16,0xa,0x17,0x2c}; // OLD WOMAN.
                        items[1].tile_index = 0x4c;
                        items[1].palette_index = 5;
                        items[1].x = 124;
                        items[1].y = 152;
                        items[1].shown = true;
                        Screen.sprites.Add(items[1]);
                        return false;
                    }
                    break;
                case 44 or 47 or 71 or 123:
                    return SaveLoad.GetGiftFlag(current_save_file, (byte)Array.IndexOf(gift_flag_list, return_screen));
                case 119:
                    if (!SaveLoad.wooden_sword[current_save_file])
                    {
                        text_row_1 = new byte[20] {0x12,0x1d,0x2a,0x1c,0x24,0xd,0xa,0x17,0x10,0xe,0x1b,0x18,0x1e,0x1c,0x24,0x1d,0x18,0x24,0x10,0x18}; // IT'S DANGEROUS TO GO
                        text_row_2 = new byte[17] {0xa,0x15,0x18,0x17,0xe,0x29,0x24,0x1d,0xa,0x14,0xe,0x24,0x1d,0x11,0x12,0x1c,0x2c}; // ALONE! TAKE THIS.
                        items[1].tile_index = 0x20;
                        items[1].palette_index = 4;
                        items[1].x = 124;
                        items[1].y = 152;
                        items[1].shown = true;
                        Screen.sprites.Add(items[1]);
                        return false;
                    }
                    break;
            }
            return true;
        }
        static void LinkItemCollection()
        {
            for (int i = 0; i < items.Length; i++)
            {
                if (items[i].shown && !item_collected)
                {
                    if (items[i].x < Link.x + 16 && items[i].x + 8 > Link.x && items[i].y < Link.y + 12 && items[i].y + 12 > Link.y)
                    {
                        switch (warp_info)
                        {
                            case 1:
                                Textures.ppu[0x2cf] = 1;
                                Textures.ppu[0x2d0] = 0;
                                SaveLoad.SetGiftFlag(current_save_file, (byte)Array.IndexOf(gift_flag_list, return_screen), true);
                                break;
                            case 2:
                                Textures.ppu[0x2cf] = 3;
                                Textures.ppu[0x2d0] = 0;
                                SaveLoad.SetGiftFlag(current_save_file, (byte)Array.IndexOf(gift_flag_list, return_screen), true);
                                break;
                            case 3:
                                Textures.ppu[0x2ce] = 1;
                                Textures.ppu[0x2cf] = 0;
                                Textures.ppu[0x2d0] = 0;
                                SaveLoad.SetGiftFlag(current_save_file, (byte)Array.IndexOf(gift_flag_list, return_screen), true);
                                break;
                            case 0xb:
                                if (return_screen == 10 && SaveLoad.nb_of_hearts[current_save_file] < 5)
                                    return;
                                else if (return_screen == 33 && SaveLoad.nb_of_hearts[current_save_file] < 12)
                                    return;
                                break;
                            case 0xf:
                                SaveLoad.SetGiftFlag(current_save_file, (byte)Array.IndexOf(gift_flag_list, return_screen), true);
                                break;
                        }

                        if (GiveItem(i + 1))
                        {
                            item_get_timer = 0;
                            items[i].x = (short)Link.x;
                            items[i].y = (short)(Link.y - 16);
                            Link.current_action = Link.Action.ITEM_GET;
                            Link.can_move = false;
                            if (warp_info == 0xf && i != 0)
                            {
                                Link.current_action = Link.Action.ITEM_HELD_UP;
                                items[1].x = (short)(Link.x + 0);
                                items[1].y = (short)(Link.y - 16);
                                items[2].x = (short)(Link.x + 8);
                                items[2].y = (short)(Link.y - 16);
                            }
                            break;
                        }
                        item_collected = true;
                    }
                }
            }

            if (item_get_timer < 150)
            {
                item_get_timer++;

                if (item_get_timer < 100)
                {
                    for (int i = 0; i < items.Length; i++)
                        if (items[i].y == 152)
                            items[i].shown = (Program.gTimer & 1) == 0;
                }

                if (item_get_timer == 128)
                {
                    item_get_timer += 100;
                    for (int i = 0; i < items.Length; i++)
                        items[i].shown = false;
                }
            }
        }
        static bool GiveItem(int chosen_item)
        {
            bool able_to_get_item = false;

            switch (warp_info)
            {
                case 1:
                    AddRupees(10);
                    break;
                case 2:
                    AddRupees(30);
                    break;
                case 3:
                    AddRupees(100);
                    break;
                case 4:
                    if (chosen_item == 1)
                    {
                        if (AddRupees(-40, false))
                        {
                            SaveLoad.blue_potion[current_save_file] = true;
                            SaveLoad.rupy_count[current_save_file] -= 40;
                            able_to_get_item = true;
                        }
                    }
                    else
                    {
                        if (AddRupees(-68, false))
                        {
                            SaveLoad.red_potion[current_save_file] = true;
                            SaveLoad.rupy_count[current_save_file] -= 68;
                            able_to_get_item = true;
                        }
                    }
                    break;
                case 5:
                    if (AddRupees(-10, false))
                    {
                        int[] rupee_values = new int[3];
                        int winner_rupee = Program.RNG.Next(0, 3);
                        rupee_values[winner_rupee] = 20 + 30 * Program.RNG.Next(0, 2);
                        rupee_values[(winner_rupee + 1) % 3] = -10 - 30 * Program.RNG.Next(0, 2);
                        if (rupee_values[(winner_rupee + 1) % 3] == -10)
                            rupee_values[(winner_rupee + 2) % 3] = -10 - 30 * Program.RNG.Next(0, 2);
                        else
                            rupee_values[(winner_rupee + 2) % 3] = -10;

                        AddRupees(rupee_values[chosen_item - 1]);

                        for (int i = 0; i < 3; i++)
                        {
                            if (rupee_values[i] > 0)
                                Textures.ppu[0x2ca + i * 4] = 0x64;
                            else
                                Textures.ppu[0x2ca + i * 4] = 0x62;

                            Textures.ppu[0x2cb + i * 4] = (byte)Math.Abs(rupee_values[i] / 10);
                        }
                        //able_to_get_item = true;
                    }
                    break;
                case 7:
                    if (chosen_item == 1)
                    {
                        if (AddRupees(-160, false))
                        {
                            SaveLoad.magical_shield[current_save_file] = true;
                            SaveLoad.rupy_count[current_save_file] -= 160;
                            able_to_get_item = true;
                        }
                    }
                    else if (chosen_item == 2)
                    {
                        if (AddRupees(-100, false))
                        {
                            SaveLoad.key_count[current_save_file]++;
                            SaveLoad.rupy_count[current_save_file] -= 100;
                            able_to_get_item = true;
                        }
                    }
                    else
                    {
                        if (AddRupees(-60, false))
                        {
                            SaveLoad.blue_candle[current_save_file] = true;
                            SaveLoad.rupy_count[current_save_file] -= 60;
                            able_to_get_item = true;
                        }
                    }
                    break;
                case 8:
                    if (chosen_item == 1)
                    {
                        if (AddRupees(-90, false))
                        {
                            SaveLoad.magical_shield[current_save_file] = true;
                            SaveLoad.rupy_count[current_save_file] -= 90;
                            able_to_get_item = true;
                        }
                    }
                    else if (chosen_item == 2)
                    {
                        if (AddRupees(-100, false))
                        {
                            SaveLoad.bait[current_save_file] = true;
                            SaveLoad.rupy_count[current_save_file] -= 100;
                            able_to_get_item = true;
                        }
                    }
                    else
                    {
                        if (AddRupees(-10, false))
                        {
                            Link.hp += 1f;
                            SaveLoad.rupy_count[current_save_file] -= 10;
                            able_to_get_item = true;
                        }
                    }
                    break;
                case 9:
                    if (chosen_item == 1)
                    {
                        if (AddRupees(-130, false))
                        {
                            SaveLoad.magical_shield[current_save_file] = true;
                            SaveLoad.rupy_count[current_save_file] -= 130;
                            able_to_get_item = true;
                        }
                    }
                    else if (chosen_item == 2)
                    {
                        if (AddRupees(-20, false))
                        {
                            SaveLoad.bomb_count[current_save_file] += 4;
                            SaveLoad.rupy_count[current_save_file] -= 20;
                            able_to_get_item = true;
                        }
                    }
                    else
                    {
                        if (AddRupees(-80, false))
                        {
                            SaveLoad.arrow[current_save_file] = true;
                            SaveLoad.rupy_count[current_save_file] -= 80;
                            able_to_get_item = true;
                        }
                    }
                    break;
                case 0xa:
                    if (chosen_item == 1)
                    {
                        if (AddRupees(-80, false))
                        {
                            SaveLoad.key_count[current_save_file]++;
                            SaveLoad.rupy_count[current_save_file] -= 80;
                            able_to_get_item = true;
                        }
                    }
                    else if (chosen_item == 2)
                    {
                        if (AddRupees(-250, false))
                        {
                            SaveLoad.blue_ring[current_save_file] = true;
                            SaveLoad.rupy_count[current_save_file] -= 250;
                            able_to_get_item = true;
                        }
                    }
                    else
                    {
                        if (AddRupees(-60, false))
                        {
                            SaveLoad.bait[current_save_file] = true;
                            SaveLoad.rupy_count[current_save_file] -= 60;
                            able_to_get_item = true;
                        }
                    }
                    break;
                case 0xb:
                    switch (return_screen)
                    {
                        case 10:
                            SaveLoad.white_sword[current_save_file] = true;
                            break;
                        case 14:
                            SaveLoad.letter[current_save_file] = true;
                            break;
                        case 33:
                            SaveLoad.magical_sword[current_save_file] = true;
                            break;
                        case 119:
                            SaveLoad.wooden_sword[current_save_file] = true;
                            break;
                    }
                    able_to_get_item = true;
                    break;
                case 0xc:
                    if (return_screen == 112)
                    {
                        if (chosen_item == 1 && AddRupees(-10, false))
                        {
                            AddRupees(-10);
                            text_counter = 0;
                            EraseText();
                            text_row_1 = new byte[17] {0x1d,0x11,0x12,0x1c,0x24,0xa,0x12,0x17,0x2a,0x1d,0x24,0xe,0x17,0x18,0x1e,0x10,0x11}; // THIS AIN'T ENOUGH
                            text_row_2 = new byte[8] {0x1d,0x18,0x24,0x1d,0xa,0x15,0x14,0x2c}; // TO TALK.
                            side_rupee.shown = false;
                        }
                        else if (chosen_item == 2 && AddRupees(-30, false))
                        {
                            AddRupees(-30);
                            text_counter = 0;
                            EraseText();
                            text_row_1 = new byte[24] {0x10,0x18,0x24,0x17,0x18,0x1b,0x1d,0x11,0x28,0x20,0xe,0x1c,0x1d,0x28,0x1c,0x18,0x1e,0x1d,0x11,0x28,0x20,0xe,0x1c,0x1d}; // GO NORTH,WEST,SOUTH,WEST
                            text_row_2 = new byte[22] {0x1d,0x18,0x24,0x1d,0x11,0xe,0x24,0xf,0x18,0x1b,0xe,0x1c,0x1d,0x24,0x18,0xf,0x24,0x16,0xa,0x23,0xe,0x2c}; // TO THE FOREST OF MAZE.
                            side_rupee.shown = false;
                        }
                        else if (chosen_item == 3 && AddRupees(-50, false))
                        {
                            AddRupees(-50);
                            text_counter = 0;
                            EraseText();
                            text_row_1 = new byte[18] {0x24,0xb,0x18,0x22,0x28,0x24,0x22,0x18,0x1e,0x2a,0x1b,0xe,0x24,0x1b,0x12,0xc,0x11,0x29}; //  BOY, YOU'RE RICH!
                            side_rupee.shown = false;
                        }
                    }
                    else
                    {
                        if (chosen_item == 1 && AddRupees(-5, false))
                        {
                            AddRupees(-5);
                            text_counter = 0;
                            EraseText();
                            text_row_1 = new byte[17] {0x1d,0x11,0x12,0x1c,0x24,0xa,0x12,0x17,0x2a,0x1d,0x24,0xe,0x17,0x18,0x1e,0x10,0x11}; // THIS AIN'T ENOUGH
                            text_row_2 = new byte[8] {0x1d,0x18,0x24,0x1d,0xa,0x15,0x14,0x2c}; // TO TALK.
                            side_rupee.shown = false;
                        }
                        else if (chosen_item == 2 && AddRupees(-10, false))
                        {
                            AddRupees(-10);
                            text_counter = 0;
                            EraseText();
                            text_row_1 = new byte[17] {0x1d,0x11,0x12,0x1c,0x24,0xa,0x12,0x17,0x2a,0x1d,0x24,0xe,0x17,0x18,0x1e,0x10,0x11}; // THIS AIN'T ENOUGH
                            text_row_2 = new byte[8] {0x1d,0x18,0x24,0x1d,0xa,0x15,0x14,0x2c}; // TO TALK.
                            side_rupee.shown = false;
                        }
                        else if (chosen_item == 3 && AddRupees(-20, false))
                        {
                            AddRupees(-20);
                            text_counter = 0;
                            EraseText();
                            text_row_1 = new byte[10] {0x10,0x18,0x24,0x1e,0x19,0x28,0x24,0x1e,0x19,0x28}; // GO UP, UP,
                            text_row_2 = new byte[19] {0x1d,0x11,0xe,0x24,0x16,0x18,0x1e,0x17,0x1d,0xa,0x12,0x17,0x24,0xa,0x11,0xe,0xa,0xd,0x2c}; // THE MOUNTAIN AHEAD.
                            side_rupee.shown = false;
                        }
                    }
                    starting_byte_1 = 0x1b0 - (text_row_1.Length >> 1);
                    starting_byte_2 = 0x1d0 - (text_row_2.Length >> 1);
                    break;
                case 0x0f:
                    if (chosen_item == 1)
                    {
                        SaveLoad.red_potion[current_save_file] = true;
                    }
                    else
                    {
                        SaveLoad.nb_of_hearts[current_save_file]++;
                        Link.hp += 1;
                    }
                    able_to_get_item = true;
                    break;
            }

            if (Menu.current_B_item == 0 && able_to_get_item)
            {
                AutoSwitchBItem(Menu.selected_menu_index, true);
                AutoSwitchBItem(Menu.selected_menu_index, false);
                Menu.MoveCursor();
                if (Menu.current_B_item == 0x36 && !boomerang[current_save_file] && !magical_boomerang[current_save_file])
                    Menu.current_B_item = 0;
            }

            if (warp_info != 1 && warp_info != 2 && warp_info != 3 && able_to_get_item)
            {
                cave_npc.flash_timer = 0;
                EraseText();
            }

            return able_to_get_item;
        }
        static void EraseText()
        {
            for (int i = 0x184; i < 0x324; i += 0x20)
            {
                for (int j = 0; j < 24; j++)
                {
                    Textures.ppu[i + j] = 0x24;
                }
            }
            if (Screen.sprites.Contains(side_rupee))
                Screen.sprites.Remove(side_rupee);
        }
        public static void SetWarpReturnPosition()
        {
            switch (return_screen)
            {
                case 11 or 34:
                    return_x = 112;
                    return_y = 128;
                    break;
                case 26 or 98:
                    return_x = 96;
                    return_y = 128;
                    break;
                case 28 or 73:
                    return_x = 48;
                    return_y = 112;
                    break;
                case 29:
                    return_x = 32;
                    return_y = 112;
                    break;
                case 33:
                    return_x = 80;
                    return_y = 112;
                    break;
                case 35:
                    return_x = 64;
                    return_y = 96;
                    break;
                case 40:
                    return_x = 224;
                    return_y = 160;
                    break;
                case 52:
                    return_x = 64;
                    return_y = 112;
                    break;
                case 61:
                    return_x = 144;
                    return_y = 112;
                    break;
                case 66 or 109:
                    return_x = 96;
                    return_y = 96;
                    break;
                case 70:
                    return_x = 128;
                    return_y = 176;
                    break;
                case 71:
                    return_x = 176;
                    return_y = 160;
                    break;
                case 72:
                    return_x = 176;
                    return_y = 112;
                    break;
                case 75:
                    return_x = 192;
                    return_y = 96;
                    break;
                case 77:
                    return_x = 192;
                    return_y = 144;
                    break;
                case 78:
                    return_x = 112;
                    return_y = 112;
                    break;
                case 81:
                    return_x = 160;
                    return_y = 160;
                    break;
                case 86:
                    return_x = 176;
                    return_y = 176;
                    break;
                case 91:
                    return_x = 48;
                    return_y = 160;
                    break;
                case 99:
                    return_x = 112;
                    return_y = 160;
                    break;
                case 104:
                    return_x = 48;
                    return_y = 144;
                    break;
                case 106:
                    return_x = 208;
                    return_y = 160;
                    break;
                case 107:
                    return_x = 80;
                    return_y = 160;
                    break;
                case 120:
                    return_x = 80;
                    return_y = 144;
                    break;
                case 121:
                    return_x = 96;
                    return_y = 112;
                    break;
                default:
                    return_x = Link.x;
                    return_y = Link.y;
                    break;
            }
        }
    }
}