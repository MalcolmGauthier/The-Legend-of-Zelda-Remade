using static The_Legend_of_Zelda.OverworldCode;
namespace The_Legend_of_Zelda
{
    public static class WarpCode
    {
        public enum NPC
        {
            NONE,
            OLD_MAN,
            OLD_WOMAN,
            SHOPKEEPER,
            GOBLIN,
            ZELDA
        }

        public enum WarpType
        {
            DOOR_REPAIR_CHARGE,
            SECRET_10,
            SECRET_30,
            SECRET_100,
            MEDICINE,
            GAMBLING,
            TAKE_ANY_ROAD,
            SHOP_SHIELD_KEY_CANDLE,
            SHOP_SHIELD_BAIT_HEART,
            SHOP_SHIELD_BOMB_ARROW,
            SHOP_KEY_RING_BAIT,
            ITEM_GIVEAWAY,
            PAID_INFO,
            FREE_INFO,
            DUNGEON,
            HEALTH_UPGRADE
        }

        const byte EMPTY_TILE = 0x24;

        public static WarpType warp_info;
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

        public static void Init()
        {
            // always create 3 items, because most of the cave types contain collectables
            for (int i = 0; i < items.Length; i++)
            {
                items[i] = new StaticSprite(0x00, 4, 92 + (i * 32), 152);
                items[i].shown = false;
                items[i].unload_during_transition = true;
            }

            // initialization
            text_row_1 = new byte[0];
            text_row_2 = new byte[0];
            text_counter = 0;
            current_npc = NPC.NONE;
            item_collected = false;
            side_rupee.unload_during_transition = true;
            side_rupee.shown = true;
            level_7_entrance_timer = LEVEL_7_ENTRANCE_ANIM_DONE;

            warp_info = (WarpType)screen_warp_info[return_screen];
            text_counter = 0;

            // init text, NPC, flags, prices and other stuff
            switch (warp_info)
            {
                case WarpType.DOOR_REPAIR_CHARGE or WarpType.SECRET_10 or WarpType.SECRET_30 or WarpType.SECRET_100:
                    // these can only happen once
                    if (SaveLoad.GetGiftFlag((byte)Array.IndexOf(gift_flag_list, return_screen)))
                        break;

                    if (warp_info == WarpType.DOOR_REPAIR_CHARGE)
                    {
                        current_npc = NPC.OLD_MAN;
                        // door repair charge activates the moment the text ends, so unlike the secret rupees, you cannot leave the cave without activating this flag
                        SaveLoad.SetGiftFlag((byte)Array.IndexOf(gift_flag_list, return_screen), true);
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
                    break;

                case WarpType.MEDICINE:
                    current_npc = NPC.OLD_WOMAN;
                    if (SaveLoad.potion_shop_activated)
                    {
                        text_row_1 = new byte[19] {0xb,0x1e,0x22,0x24,0x16,0xe,0xd,0x12,0xc,0x12,0x17,0xe,0x24,0xb,0xe,0xf,0x18,0x1b,0xe}; // BUY MEDICINE BEFORE
                        text_row_2 = new byte[19] {0x22,0x18,0x1e,0x24,0x10,0x18,0x2c,0x24,0x24,0x24,0x24,0x24,0x24,0x24,0x24,0x24,0x24,0x24,0x24}; // YOU GO.

                        for (int i = 0; i < items.Length; i += 2)
                        {
                            items[i].tile_index = 0x40;
                            items[i].palette_index = i == 0 ? (byte)5 : (byte)6;
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

                case WarpType.GAMBLING:
                    current_npc = NPC.OLD_MAN;
                    text_row_1 = new byte[17] {0x15,0xe,0x1d,0x2a,0x1c,0x24,0x19,0x15,0xa,0x22,0x24,0x16,0x18,0x17,0xe,0x22,0x24}; // LET'S PLAY MONEY 
                    text_row_2 = new byte[17] {0x16,0xa,0x14,0x12,0x17,0x10,0x24,0x10,0xa,0x16,0xe,0x2c,0x24,0x24,0x24,0x24,0x24}; // MAKING GAME.    

                    Screen.sprites.Add(side_rupee);
                    Textures.ppu[0x2c8] = 0x21; // "x"

                    for (int i = 0; i < items.Length; i++)
                    {
                        items[i] = new FlickeringSprite(0x32, 6, 92 + (i * 32), 152, 8, 0x32, second_palette_index: 5);
                        Screen.sprites.Add(items[i]);
                    }

                    for (int i = 0; i < 3; i++)
                    {
                        // "-10"
                        Textures.ppu[0x2ca + i * 4] = 0x62;
                        Textures.ppu[0x2cb + i * 4] = 1;
                        Textures.ppu[0x2cc + i * 4] = 0;
                    }
                    break;

                case WarpType.TAKE_ANY_ROAD:
                    current_npc = NPC.OLD_MAN;
                    text_row_1 = new byte[23] {0x1d,0xa,0x14,0xe,0x24,0xa,0x17,0x22,0x24,0x1b,0x18,0xa,0xd,0x24,0x22,0x18,0x1e,0x24,0x20,0xa,0x17,0x1d,0x2c}; // TAKE ANY ROAD YOU WANT.

                    // add the three staircases to the screen
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

                case WarpType.SHOP_SHIELD_KEY_CANDLE or WarpType.SHOP_SHIELD_BAIT_HEART or WarpType.SHOP_SHIELD_BOMB_ARROW or WarpType.SHOP_KEY_RING_BAIT:
                    current_npc = NPC.SHOPKEEPER;
                    Screen.sprites.Add(side_rupee);
                    Textures.ppu[0x2c8] = 0x21; // "x"

                    if (warp_info is (WarpType.SHOP_SHIELD_BAIT_HEART or WarpType.SHOP_KEY_RING_BAIT))
                    {
                        text_row_1 = new byte[12] {0xb,0x18,0x22,0x28,0x24,0x1d,0x11,0x12,0x1c,0x24,0x12,0x1c}; // BOY, THIS IS
                        text_row_2 = new byte[17] {0x1b,0xe,0xa,0x15,0x15,0x22,0x24,0xe,0x21,0x19,0xe,0x17,0x1c,0x12,0x1f,0xe,0x29}; // REALLY EXPENSIVE!
                    }
                    else
                    {
                        text_row_1 = new byte[22] {0xb,0x1e,0x22,0x24,0x1c,0x18,0x16,0xe,0x1d,0x11,0x12,0x17,0x2a,0x24,0x20,0x12,0x15,0x15,0x24,0x22,0xa,0x29}; // BUY SOMETHIN' WILL YA!
                    }

                    if (warp_info == WarpType.SHOP_KEY_RING_BAIT)
                    {
                        items[0].tile_index = 0x2e;
                        items[0].palette_index = 6;
                    }
                    else
                    {
                        items[0].tile_index = 0x56;
                        items[0].palette_index = 4;
                    }

                    switch (warp_info)
                    {
                        case WarpType.SHOP_SHIELD_KEY_CANDLE:
                            items[1].tile_index = 0x2e;
                            items[1].palette_index = 6;
                            items[2].tile_index = 0x26;
                            items[2].palette_index = 5;

                            Textures.ppu[0x2ca] = 1;
                            Textures.ppu[0x2cb] = 6;
                            Textures.ppu[0x2ce] = 1;
                            Textures.ppu[0x2cf] = 0;
                            Textures.ppu[0x2d3] = 6;
                            break;
                        case WarpType.SHOP_SHIELD_BAIT_HEART:
                            items[1].tile_index = 0x22;
                            items[1].palette_index = 6;
                            items[2] = new FlickeringSprite(0x1c, 6, 156, 152, 8, 0x1c, second_palette_index: 5);

                            Textures.ppu[0x2cb] = 9;
                            Textures.ppu[0x2ce] = 1;
                            Textures.ppu[0x2cf] = 0;
                            Textures.ppu[0x2d3] = 1;
                            break;
                        case WarpType.SHOP_SHIELD_BOMB_ARROW:
                            items[1].tile_index = 0x34;
                            items[1].palette_index = 5;
                            items[2].tile_index = 0x28;
                            items[2].palette_index = 4;

                            Textures.ppu[0x2ca] = 1;
                            Textures.ppu[0x2cb] = 3;
                            Textures.ppu[0x2cf] = 2;
                            Textures.ppu[0x2d3] = 8;
                            break;
                        case WarpType.SHOP_KEY_RING_BAIT:
                            items[1].tile_index = 0x46;
                            items[1].palette_index = 5;
                            items[2].tile_index = 0x22;
                            items[2].palette_index = 6;

                            Textures.ppu[0x2cb] = 8;
                            Textures.ppu[0x2ce] = 2;
                            Textures.ppu[0x2cf] = 5;
                            Textures.ppu[0x2d3] = 6;
                            break;
                    }

                    // all prices are multiple of 10
                    for (int i = 0; i < 3; i++)
                    {
                        Textures.ppu[0x2cc + i * 4] = 0;
                    }

                    for (int i = 0; i < items.Length; i ++)
                    {
                        items[i].shown = true;
                        Screen.sprites.Add(items[i]);
                    }
                    break;

                case WarpType.ITEM_GIVEAWAY:
                    // if item gotten, empty cave
                    if ((return_screen == 10 && SaveLoad.white_sword) ||
                        (return_screen == 33 && SaveLoad.magical_sword) ||
                        (return_screen == 119 && SaveLoad.wooden_sword) ||
                        (return_screen == 14 && SaveLoad.letter))
                    {
                        break;
                    }

                    // stuff that happens for all item giveaways
                    items[1].x = 124;
                    items[1].y = 152;
                    items[1].shown = true;
                    Screen.sprites.Add(items[1]);
                    current_npc = NPC.OLD_MAN;

                    // stuff that depends on which giveaway it is
                    switch (return_screen)
                    {
                        case 10 or 33:
                            if ((SaveLoad.white_sword && return_screen == 10) ||
                                (SaveLoad.magical_sword && return_screen == 33))
                                break;

                            text_row_1 = new byte[19] { 0x16, 0xa, 0x1c, 0x1d, 0xe, 0x1b, 0x24, 0x1e, 0x1c, 0x12, 0x17, 0x10, 0x24, 0x12, 0x1d, 0x24, 0xa, 0x17, 0xd }; // MASTER USING IT AND
                            text_row_2 = new byte[19] { 0x24, 0x22, 0x18, 0x1e, 0x24, 0xc, 0xa, 0x17, 0x24, 0x11, 0xa, 0x1f, 0xe, 0x24, 0x1d, 0x11, 0x12, 0x1c, 0x2c }; //  YOU CAN HAVE THIS.
                            if (return_screen == 33)
                            {
                                items[1].tile_index = (byte)SpriteID.MAGIC_SWORD;
                                items[1].palette_index = (byte)PaletteID.SP_2;
                            }
                            else
                            {
                                items[1].tile_index = (byte)SpriteID.SWORD;
                                items[1].palette_index = (byte)PaletteID.SP_1;
                            }
                            break;

                        case 14:
                            if (SaveLoad.letter)
                                break;

                            text_row_1 = new byte[16] { 0x1c, 0x11, 0x18, 0x20, 0x24, 0x1d, 0x11, 0x12, 0x1c, 0x24, 0x1d, 0x18, 0x24, 0x1d, 0x11, 0xe }; // SHOW THIS TO THE
                            text_row_2 = new byte[10] { 0x18, 0x15, 0xd, 0x24, 0x20, 0x18, 0x16, 0xa, 0x17, 0x2c }; // OLD WOMAN.
                            items[1].tile_index = (byte)SpriteID.MAP;
                            items[1].palette_index = (byte)PaletteID.SP_1;
                            break;

                        case 119:
                            if (SaveLoad.wooden_sword)
                                break;

                            text_row_1 = new byte[20] { 0x12, 0x1d, 0x2a, 0x1c, 0x24, 0xd, 0xa, 0x17, 0x10, 0xe, 0x1b, 0x18, 0x1e, 0x1c, 0x24, 0x1d, 0x18, 0x24, 0x10, 0x18 }; // IT'S DANGEROUS TO GO
                            text_row_2 = new byte[17] { 0xa, 0x15, 0x18, 0x17, 0xe, 0x29, 0x24, 0x1d, 0xa, 0x14, 0xe, 0x24, 0x1d, 0x11, 0x12, 0x1c, 0x2c }; // ALONE! TAKE THIS.
                            items[1].tile_index = (byte)SpriteID.SWORD;
                            items[1].palette_index = (byte)PaletteID.SP_0;
                            break;
                    }

                    break;

                case WarpType.PAID_INFO:
                    current_npc = NPC.OLD_WOMAN;
                    text_row_1 = new byte[22] {0x24,0x19,0xa,0x22,0x24,0x16,0xe,0x24,0xa,0x17,0xd,0x24,0x12,0x2a,0x15,0x15,0x24,0x1d,0xa,0x15,0x14,0x2c}; //  PAY ME AND I'LL TALK.
                    Screen.sprites.Add(side_rupee);
                    Textures.ppu[0x2c8] = 0x21;

                    for (int i = 0; i < items.Length; i++)
                    {
                        items[i] = new FlickeringSprite(0x32, 6, 92 + (i * 32), 152, 8, 0x32, second_palette_index: 5);
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
                            Textures.ppu[0x2cb + i * 3 + (i / 2)] = 0x62;
                            Textures.ppu[0x2cc + i * 4] = 0;
                        }
                        Textures.ppu[0x2cc] = 5;
                        Textures.ppu[0x2cf] = 1;
                        Textures.ppu[0x2d3] = 2;
                    }
                    break;

                case WarpType.FREE_INFO:
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

                case WarpType.DUNGEON:
                    Program.gamemode = Program.Gamemode.DUNGEON;
                    DungeonCode.Init((byte)Array.IndexOf(dungeon_locations, return_screen));
                    return;

                case WarpType.HEALTH_UPGRADE:
                    if (SaveLoad.GetGiftFlag((byte)Array.IndexOf(gift_flag_list, return_screen)))
                        break;

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
                    break;
            }

            // ppu index for which the first text character will be on
            starting_byte_1 = 0x1b0 - (text_row_1.Length / 2);
            starting_byte_2 = 0x1d0 - (text_row_2.Length / 2);

            // removal needed for when re-initializating cave after activating potion shop (and also reality check)
            Screen.sprites.Remove(cave_npc);
            cave_npc = new CaveNPC(current_npc);
            Screen.sprites.Add(cave_npc);
        }

        public static void Tick()
        {
            // advance text only when link can't move
            if (!Link.can_move)
                TextTick();

            // link can only move if the text is done scrolling and the fire has appeared
            if (Link.current_action is not (Link.Action.ITEM_GET or Link.Action.ITEM_HELD_UP) && !Menu.menu_open)
                Link.can_move = (text_counter >= text_row_1.Length + text_row_2.Length && fire_appeared);

            // check for item collection
            LinkItemCollection();

            // activate the medicine shop with the map
            if (warp_info == WarpType.MEDICINE && !SaveLoad.potion_shop_activated && Control.IsPressed(Buttons.B) && Menu.current_B_item == SpriteID.MAP)
            {
                SaveLoad.potion_shop_activated = true;
                Link.can_move = false;
                // re-initialize the cave, but now with the potion shop activated
                Init();
            }

            // prevent link from going in top half of room
            if (Link.y < 144)
                Link.SetPos(new_y: 144);
        }

        // advances text forward if not finished
        static void TextTick()
        {
            if (Program.gTimer % 6 != 0 || !fire_appeared)
                return;

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

                // make sure that link can't escape the fine
                if (text_counter == text_row_1.Length + text_row_2.Length && warp_info == WarpType.DOOR_REPAIR_CHARGE)
                    Link.AddRupees(-20);
            }
        }

        // checks if link is touching an item, then runs logic for item collection (which is also found in GiveItem)
        static void LinkItemCollection()
        {
            for (int i = 0; i < items.Length; i++)
            {
                if (item_collected)
                    break;

                if (!items[i].shown)
                    continue;

                // collision box! only continue if the item at index i has been touched
                if (!(items[i].x < Link.x + 16 && items[i].x + 8 > Link.x && items[i].y < Link.y + 12 && items[i].y + 12 > Link.y))
                    continue;

                // set flags to ensure gifts can only be collected once
                switch (warp_info)
                {
                    case WarpType.SECRET_10:
                        Textures.ppu[0x2cf] = 1;
                        Textures.ppu[0x2d0] = 0;
                        SaveLoad.SetGiftFlag((byte)Array.IndexOf(gift_flag_list, return_screen), true);
                        break;
                    case WarpType.SECRET_30:
                        Textures.ppu[0x2cf] = 3;
                        Textures.ppu[0x2d0] = 0;
                        SaveLoad.SetGiftFlag((byte)Array.IndexOf(gift_flag_list, return_screen), true);
                        break;
                    case WarpType.SECRET_100:
                        Textures.ppu[0x2ce] = 1;
                        Textures.ppu[0x2cf] = 0;
                        Textures.ppu[0x2d0] = 0;
                        SaveLoad.SetGiftFlag((byte)Array.IndexOf(gift_flag_list, return_screen), true);
                        break;
                    case WarpType.ITEM_GIVEAWAY:
                        // do NOT give the sword if link does not meet these conditions
                        if (return_screen == 10 && SaveLoad.nb_of_hearts < 5)
                            return;
                        else if (return_screen == 33 && SaveLoad.nb_of_hearts < 12)
                            return;
                        break;
                    case WarpType.HEALTH_UPGRADE:
                        SaveLoad.SetGiftFlag((byte)Array.IndexOf(gift_flag_list, return_screen), true);
                        break;
                }

                // if item is able to be given. if item unable to be given, GiveItem returns false.
                if (GiveItem(i + 1))
                {
                    item_get_timer = 0;
                    items[i].x = Link.x;
                    items[i].y = Link.y - 16;
                    Link.current_action = Link.Action.ITEM_GET;
                    Link.can_move = false;
                    // check for heart container (double sprite)
                    // why is heart container not a full sprite with a counterpart?
                    // because the there's no class for a static sprite with a counterpart.
                    if (warp_info == WarpType.HEALTH_UPGRADE && i != 0)
                    {
                        Link.current_action = Link.Action.ITEM_HELD_UP;
                        items[1].x = Link.x + 0;
                        items[1].y = Link.y - 16;
                        items[2].x = Link.x + 8;
                        items[2].y = Link.y - 16;
                    }
                    break;
                }

                item_collected = true;
                break;
            }

            if (item_get_timer >= 150)
            {
                return;
            }

            //item get animation
            item_get_timer++;

            if (item_get_timer < 100)
            {
                for (int i = 0; i < items.Length; i++)
                    if (items[i].y == 152)
                        items[i].shown = (Program.gTimer % 2) == 0;

                return;
            }

            if (item_get_timer == 128)
            {
                item_get_timer += 100;
                for (int i = 0; i < items.Length; i++)
                    items[i].shown = false;
            }
        }

        // this function checks whether link can actually collect the item he wants to collect, and gives it to him if he can.
        // note that chosen item is an index for which item was touched, and is 1 indexed, thus chosen_item is always either 1, 2 or 3
        static bool GiveItem(int chosen_item)
        {
            // set to true if link is able to collect the item
            bool able_to_get_item = false;

            // door repair case not handled here, because that doesn't involve item collection
            switch (warp_info)
            {
                case WarpType.SECRET_10:
                    Link.AddRupees(10);
                    break;
                case WarpType.SECRET_30:
                    Link.AddRupees(30);
                    break;
                case WarpType.SECRET_100:
                    Link.AddRupees(100);
                    break;

                case WarpType.GAMBLING:
                    // if link is too poor, he can't gamble
                    if (!Link.AddRupees(-10, false))
                        break;

                    int[] rupee_values = new int[3];
                    int winner_rupee = Program.RNG.Next(0, 3);
                    // winning prize is either 20 or 50
                    rupee_values[winner_rupee] = 20 + 30 * Program.RNG.Next(0, 2);
                    // losing prize is either -10 or -40
                    // however, at least one of the losing prizes needs to be -10
                    int small_loss_index = Program.RNG.Next(0, 2);
                    rupee_values[(winner_rupee + small_loss_index + 1) % 3] = -10;
                    // ^1 flips 0 to 1 and vice versa
                    rupee_values[(winner_rupee + (small_loss_index ^ 1) + 1) % 3] = -10 - 30 * Program.RNG.Next(0, 2);

                    Link.AddRupees(rupee_values[chosen_item - 1]);

                    for (int i = 0; i < 3; i++)
                    {
                        if (rupee_values[i] > 0)
                            Textures.ppu[0x2ca + i * 4] = 0x64; // +
                        else
                            Textures.ppu[0x2ca + i * 4] = 0x62; // -

                        Textures.ppu[0x2cb + i * 4] = (byte)Math.Abs(rupee_values[i] / 10);
                        Textures.ppu[0x2cb + i * 4 + 1] = 0;
                    }
                    break;

                case WarpType.MEDICINE or
                     WarpType.SHOP_SHIELD_KEY_CANDLE or WarpType.SHOP_SHIELD_BAIT_HEART or WarpType.SHOP_SHIELD_BOMB_ARROW or WarpType.SHOP_KEY_RING_BAIT:

                    // data for all shops. input is shop type and item id (1 to 3), and output is price and action
                    Dictionary<(WarpType shop_type, int item_id), (byte price, Action action)> data_map = new()
                    {
                        { (WarpType.MEDICINE, 1), (40, () => { SaveLoad.blue_potion = true; }) },
                        { (WarpType.MEDICINE, 3), (68, () => { SaveLoad.red_potion = true; }) },

                        { (WarpType.SHOP_SHIELD_KEY_CANDLE, 1), (160, () => { SaveLoad.magical_shield = true; }) },
                        { (WarpType.SHOP_SHIELD_KEY_CANDLE, 2), (100, () => { SaveLoad.key_count++; }) },
                        { (WarpType.SHOP_SHIELD_KEY_CANDLE, 3), (60, () => { SaveLoad.blue_candle = true; }) },

                        { (WarpType.SHOP_SHIELD_BAIT_HEART, 1), (90, () => { SaveLoad.magical_shield = true; }) },
                        { (WarpType.SHOP_SHIELD_BAIT_HEART, 2), (100, () => { SaveLoad.bait = true; }) },
                        { (WarpType.SHOP_SHIELD_BAIT_HEART, 3), (10, () => { Link.hp += 1f; }) },

                        { (WarpType.SHOP_SHIELD_BOMB_ARROW, 1), (130, () => { SaveLoad.magical_shield = true; }) },
                        { (WarpType.SHOP_SHIELD_BOMB_ARROW, 2), (20, () => { SaveLoad.bomb_count += 4; }) },
                        { (WarpType.SHOP_SHIELD_BOMB_ARROW, 3), (80, () => { SaveLoad.arrow = true; }) },

                        { (WarpType.SHOP_KEY_RING_BAIT, 1), (80, () => { SaveLoad.key_count++; }) },
                        { (WarpType.SHOP_KEY_RING_BAIT, 2), (250, () => { SaveLoad.blue_ring = true; }) },
                        { (WarpType.SHOP_KEY_RING_BAIT, 3), (60, () => { SaveLoad.bait = true; }) },
                    };

                    for (int i = 1; i < 4; i++)
                    {
                        if (chosen_item == i)
                        {
                            if (!data_map.TryGetValue(new(warp_info, i), out (byte price, Action action) data_shop))
                                continue;

                            if (!Link.AddRupees(-data_shop.price, false))
                                continue;

                            data_shop.action();
                            SaveLoad.rupy_count -= data_shop.price;
                            able_to_get_item = true;
                            break;
                        }
                    }
                    break;

                case WarpType.ITEM_GIVEAWAY:
                    switch (return_screen)
                    {
                        case 10:
                            SaveLoad.white_sword = true;
                            break;
                        case 14:
                            SaveLoad.letter = true;
                            break;
                        case 33:
                            SaveLoad.magical_sword = true;
                            break;
                        case 119:
                            SaveLoad.wooden_sword = true;
                            break;
                    }
                    able_to_get_item = true;
                    break;

                case WarpType.PAID_INFO:

                    // READ ONLY!! (please)
                    byte[] this_aint_enough = new byte[17] { 0x1d, 0x11, 0x12, 0x1c, 0x24, 0xa, 0x12, 0x17, 0x2a, 0x1d, 0x24, 0xe, 0x17, 0x18, 0x1e, 0x10, 0x11 };
                    byte[] to_talk = new byte[8] { 0x1d, 0x18, 0x24, 0x1d, 0xa, 0x15, 0x14, 0x2c };
                    byte[] boy_youre_rich = new byte[18] { 0x24, 0xb, 0x18, 0x22, 0x28, 0x24, 0x22, 0x18, 0x1e, 0x2a, 0x1b, 0xe, 0x24, 0x1b, 0x12, 0xc, 0x11, 0x29 };
                    byte[] go_north_west_south_west = new byte[24] { 0x10, 0x18, 0x24, 0x17, 0x18, 0x1b, 0x1d, 0x11, 0x28, 0x20, 0xe, 0x1c, 0x1d, 0x28, 0x1c, 0x18, 0x1e, 0x1d, 0x11, 0x28, 0x20, 0xe, 0x1c, 0x1d };
                    byte[] to_the_forest_of_maze = new byte[22] { 0x1d, 0x18, 0x24, 0x1d, 0x11, 0xe, 0x24, 0xf, 0x18, 0x1b, 0xe, 0x1c, 0x1d, 0x24, 0x18, 0xf, 0x24, 0x16, 0xa, 0x23, 0xe, 0x2c };
                    byte[] go_up_up = new byte[10] { 0x10, 0x18, 0x24, 0x1e, 0x19, 0x28, 0x24, 0x1e, 0x19, 0x28 };
                    byte[] the_mountain_ahead = new byte[19] { 0x1d, 0x11, 0xe, 0x24, 0x16, 0x18, 0x1e, 0x17, 0x1d, 0xa, 0x12, 0x17, 0x24, 0xa, 0x11, 0xe, 0xa, 0xd, 0x2c };

                    Dictionary<(int screen, int item), (int cost, byte[] row1, byte[] row2)> textData = new()
                    {
                        { (112, 1), (10, this_aint_enough, to_talk) },
                        { (112, 2), (30, go_north_west_south_west, to_the_forest_of_maze) },
                        { (112, 3), (50, boy_youre_rich, new byte[0]) },
                        { (26, 1), (5, this_aint_enough, to_talk) },
                        { (26, 2), (10, this_aint_enough, to_talk) },
                        { (26, 3), (20, go_up_up, the_mountain_ahead) }
                    };

                    if (textData.TryGetValue((return_screen, chosen_item), out (int cost, byte[] row1, byte[] row2) data) && Link.AddRupees(-data.cost, false))
                    {
                        Link.AddRupees(-data.cost);
                        text_counter = 0;
                        EraseText();
                        text_row_1 = data.row1;
                        text_row_2 = data.row2;
                        side_rupee.shown = false;

                        starting_byte_1 = 0x1b0 - (text_row_1.Length / 2);
                        starting_byte_2 = 0x1d0 - (text_row_2.Length / 2);
                    }
                    break;

                case WarpType.HEALTH_UPGRADE:
                    if (chosen_item == 1)
                    {
                        SaveLoad.red_potion = true;
                    }
                    else
                    {
                        SaveLoad.nb_of_hearts++;
                        Link.hp += 1f;
                    }
                    able_to_get_item = true;
                    break;
            }

            // if item is collect when link's inventory is empty, set B item to newly acquired item (if applicable)
            if (Menu.current_B_item == 0 && able_to_get_item)
            {
                Menu.AutoSwitchBItem(Menu.selected_menu_index);
            }

            // of all the cases this function is used for, only the secret money doesn't make the NPC go away on collection
            if ((warp_info is not (WarpType.SECRET_10 or WarpType.SECRET_30 or WarpType.SECRET_100)) && able_to_get_item)
            {
                cave_npc.flash_timer = 0;
                EraseText();
            }

            return able_to_get_item;
        }

        // fills text area with empty.
        static void EraseText()
        {
            for (int i = 0; i < 13; i++)
            {
                for (int j = 0; j < 24; j++)
                {
                    Textures.ppu[0x184 + i * Textures.PPU_WIDTH + j] = EMPTY_TILE;
                }
            }
            
            Screen.sprites.Remove(side_rupee);
        }

        // sets the exact x and y coordinates that link will return to when exiting
        public static void SetWarpReturnPosition()
        {
            Dictionary<byte, (int x, int y)> warpPositions = new()
            {
                { 11, (112, 128) },
                { 34, (112, 128) },
                { 26, (96, 128) },
                { 98, (96, 128) },
                { 28, (48, 112) },
                { 73, (48, 112) },
                { 29, (32, 112) },
                { 33, (80, 112) },
                { 35, (64, 96) },
                { 40, (224, 160) },
                { 52, (64, 112) },
                { 61, (144, 112) },
                { 66, (96, 96) },
                { 109, (96, 96) },
                { 70, (128, 176) },
                { 71, (176, 160) },
                { 72, (176, 112) },
                { 75, (192, 96) },
                { 77, (192, 144) },
                { 78, (112, 112) },
                { 81, (160, 160) },
                { 86, (176, 176) },
                { 91, (48, 160) },
                { 99, (112, 160) },
                { 104, (48, 144) },
                { 106, (208, 160) },
                { 107, (80, 160) },
                { 120, (80, 144) },
                { 121, (96, 112) }
            };

            if (warpPositions.TryGetValue(return_screen, out (int x, int y) position))
            {
                return_x = position.x;
                return_y = position.y;
            }
            else
            {
                Link.SetPos(return_x, return_y);
            }
        }
    }
}