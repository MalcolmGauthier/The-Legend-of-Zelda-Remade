namespace The_Legend_of_Zelda
{
    internal static class SaveLoad
    {
        public static byte current_save_file;

        public static byte[] nb_of_hearts = new byte[3];
        public static byte[] bomb_limit = new byte[3];
        public static byte[] bomb_count = new byte[3];
        public static byte[] rupy_count = new byte[3];
        public static byte[] key_count = new byte[3];
        public static byte[] triforce_pieces = new byte[3];
        public static bool[] triforce_of_power = new bool[3];
        public static bool[] wooden_sword = new bool[3];
        public static bool[] white_sword = new bool[3];
        public static bool[] magical_sword = new bool[3];
        public static bool[] raft = new bool[3];
        public static bool[] book_of_magic = new bool[3];
        public static bool[] blue_ring = new bool[3];
        public static bool[] red_ring = new bool[3];
        public static bool[] ladder = new bool[3];
        public static bool[] magical_key = new bool[3];
        public static bool[] power_bracelet = new bool[3];
        public static bool[] boomerang = new bool[3];
        public static bool[] magical_boomerang = new bool[3];
        public static bool[] bow = new bool[3];
        public static bool[] arrow = new bool[3];
        public static bool[] silver_arrow = new bool[3];
        public static bool[] blue_candle = new bool[3];
        public static bool[] red_candle = new bool[3];
        public static bool[] recorder = new bool[3];
        public static bool[] bait = new bool[3];
        public static bool[] letter = new bool[3];
        public static bool[] blue_potion = new bool[3];
        public static bool[] red_potion = new bool[3];
        public static bool[] magical_rod = new bool[3];
        public static ushort[] compass_flags = new ushort[3];
        public static ushort[] map_flags = new ushort[3];
        public static int[] boss_kills_flags = new int[3];
        public static long[] bombed_holes_flags = new long[3];
        public static long[] opened_key_doors_flags = new long[3];
        public static long[] gift_flags = new long[3];
        public static byte[,] file_name = new byte[3,8];
        public static bool[] second_quest = new bool[3];
        public static byte[] death_count = new byte[3];
        public static bool[] save_file_exists = new bool[3];
        public static short[] heart_container_flags = new short[3];
        public static bool[] potion_shop_activated = new bool[3];
        public static bool[] magical_shield = new bool[3];
        public static long[] overworld_secrets_flags = new long[3];
        public static long[,] dungeon_gift_flags = new long[3, 4];
        public static long[,] dungeon_rooms_visited_flags = new long[3, 4];
        //public static bool[] _ = new bool[3];
        public static void SaveFile(byte save_file)
        {
            try
            {
                using (Stream stream = File.OpenWrite(@"C:\Users\Malcolm\source\repos\The Legend of Zelda\Data\SaveData.bin"))
                {
                    BinaryWriter bw = new BinaryWriter(stream);
                    bw.Seek(save_file * 0x200, SeekOrigin.Begin);
                    bw.Write(nb_of_hearts[save_file]);
                    bw.Write(bomb_limit[save_file]);
                    bw.Write(bomb_count[save_file]);
                    bw.Write(rupy_count[save_file]);
                    bw.Write(key_count[save_file]);
                    bw.Write(triforce_pieces[save_file]);
                    bw.Write(triforce_of_power[save_file]);
                    bw.Write(wooden_sword[save_file]);
                    bw.Write(white_sword[save_file]);
                    bw.Write(magical_sword[save_file]);
                    bw.Write(raft[save_file]);
                    bw.Write(book_of_magic[save_file]);
                    bw.Write(blue_ring[save_file]);
                    bw.Write(red_ring[save_file]);
                    bw.Write(ladder[save_file]);
                    bw.Write(magical_key[save_file]);
                    bw.Write(power_bracelet[save_file]);
                    bw.Write(boomerang[save_file]);
                    bw.Write(magical_boomerang[save_file]);
                    bw.Write(bow[save_file]);
                    bw.Write(arrow[save_file]);
                    bw.Write(silver_arrow[save_file]);
                    bw.Write(blue_candle[save_file]);
                    bw.Write(red_candle[save_file]);
                    bw.Write(recorder[save_file]);
                    bw.Write(bait[save_file]);
                    bw.Write(letter[save_file]);
                    bw.Write(blue_potion[save_file]);
                    bw.Write(red_potion[save_file]);
                    bw.Write(magical_rod[save_file]);
                    bw.Write(compass_flags[save_file]);
                    bw.Write(map_flags[save_file]);
                    bw.Write(boss_kills_flags[save_file]);
                    bw.Write(bombed_holes_flags[save_file]);
                    bw.Write(opened_key_doors_flags[save_file]);
                    bw.Write(gift_flags[save_file]);
                    for (int i = 0; i < 8; i++)
                        bw.Write(file_name[save_file, i]);
                    bw.Write(second_quest[save_file]);
                    bw.Write(death_count[save_file]);
                    bw.Write(save_file_exists[save_file]);
                    bw.Write(heart_container_flags[save_file]);
                    bw.Write(potion_shop_activated[save_file]);
                    bw.Write(magical_shield[save_file]);
                    bw.Write(overworld_secrets_flags[save_file]);
                    for (int i = 0; i < 4; i++)
                        bw.Write(dungeon_gift_flags[save_file, i]);
                    for (int i = 0; i < 4; i++)
                        bw.Write(dungeon_rooms_visited_flags[save_file, i]);
                }
            }
            catch (Exception ex)
            {
                SDL2.SDL.SDL_SetError(ex.Message);
            }
        }
        public static void LoadFile(byte save_file)
        {
            try
            {
                using (Stream stream = File.OpenRead(@"C:\Users\Malcolm\source\repos\The Legend of Zelda\Data\SaveData.bin"))
                {
                    BinaryReader br = new BinaryReader(stream);
                    br.ReadBytes(save_file * 0x200);
                    nb_of_hearts[save_file] = br.ReadByte();
                    bomb_limit[save_file] = br.ReadByte();
                    bomb_count[save_file] = br.ReadByte();
                    rupy_count[save_file] = br.ReadByte();
                    key_count[save_file] = br.ReadByte();
                    triforce_pieces[save_file] = br.ReadByte();
                    triforce_of_power[save_file] = br.ReadBoolean();
                    wooden_sword[save_file] = br.ReadBoolean();
                    white_sword[save_file] = br.ReadBoolean();
                    magical_sword[save_file] = br.ReadBoolean();
                    raft[save_file] = br.ReadBoolean();
                    book_of_magic[save_file] = br.ReadBoolean();
                    blue_ring[save_file] = br.ReadBoolean();
                    red_ring[save_file] = br.ReadBoolean();
                    ladder[save_file] = br.ReadBoolean();
                    magical_key[save_file] = br.ReadBoolean();
                    power_bracelet[save_file] = br.ReadBoolean();
                    boomerang[save_file] = br.ReadBoolean();
                    magical_boomerang[save_file] = br.ReadBoolean();
                    bow[save_file] = br.ReadBoolean();
                    arrow[save_file] = br.ReadBoolean();
                    silver_arrow[save_file] = br.ReadBoolean();
                    blue_candle[save_file] = br.ReadBoolean();
                    red_candle[save_file] = br.ReadBoolean();
                    recorder[save_file] = br.ReadBoolean();
                    bait[save_file] = br.ReadBoolean();
                    letter[save_file] = br.ReadBoolean();
                    blue_potion[save_file] = br.ReadBoolean();
                    red_potion[save_file] = br.ReadBoolean();
                    magical_rod[save_file] = br.ReadBoolean();
                    compass_flags[save_file] = br.ReadUInt16();
                    map_flags[save_file] = br.ReadUInt16();
                    boss_kills_flags[save_file] = br.ReadInt32();
                    bombed_holes_flags[save_file] = br.ReadInt64();
                    opened_key_doors_flags[save_file] = br.ReadInt64();
                    gift_flags[save_file] = br.ReadInt64();
                    for (int i = 0; i < 8; i++)
                        file_name[save_file, i] = br.ReadByte();
                    second_quest[save_file] = br.ReadBoolean();
                    death_count[save_file] = br.ReadByte();
                    save_file_exists[save_file] = br.ReadBoolean();
                    heart_container_flags[save_file] = br.ReadInt16();
                    potion_shop_activated[save_file] = br.ReadBoolean();
                    magical_shield[save_file] = br.ReadBoolean();
                    overworld_secrets_flags[save_file] = br.ReadInt64();
                    for (int i = 0; i < 4; i++)
                        dungeon_gift_flags[save_file, i] = br.ReadInt64();
                    for (int i = 0; i < 4; i++)
                        dungeon_gift_flags[save_file, i] = br.ReadInt64();
                }
            }
            catch (Exception ex)
            {
                SDL2.SDL.SDL_SetError(ex.Message);
            }
            finally
            {
                // todo: empty all save file data for save file that was attempting to be loaded
            }
        }
        public static void DeleteData(byte save_file)
        {
            nb_of_hearts[save_file] = 3;
            bomb_limit[save_file] = 8;
            bomb_count[save_file] = 0;
            rupy_count[save_file] = 0;
            key_count[save_file] = 0;
            triforce_pieces[save_file] = 0;
            triforce_of_power[save_file] = false;
            wooden_sword[save_file] = false;
            white_sword[save_file] = false;
            magical_sword[save_file] = false;
            raft[save_file] = false;
            book_of_magic[save_file] = false;
            blue_ring[save_file] = false;
            red_ring[save_file] = false;
            ladder[save_file] = false;
            magical_key[save_file] = false;
            power_bracelet[save_file] = false;
            boomerang[save_file] = false;
            magical_boomerang[save_file] = false;
            bow[save_file] = false;
            arrow[save_file] = false;
            silver_arrow[save_file] = false;
            blue_candle[save_file] = false;
            red_candle[save_file] = false;
            recorder[save_file] = false;
            bait[save_file] = false;
            letter[save_file] = false;
            blue_potion[save_file] = false;
            red_potion[save_file] = false;
            magical_rod[save_file] = false;
            compass_flags[save_file] = 0;
            map_flags[save_file] = 0;
            boss_kills_flags[save_file] = 0;
            bombed_holes_flags[save_file] = 0;
            opened_key_doors_flags[save_file] = 0;
            overworld_secrets_flags[save_file] = 0;
            gift_flags[save_file] = 0;
            for (int i = 0; i < 8; i++)
                file_name[save_file, i] = 0x24;
            second_quest[save_file] = false;
            death_count[save_file] = 0;
            save_file_exists[save_file] = false;
            heart_container_flags[save_file] = 0;
            potion_shop_activated[save_file] = false;
            magical_shield[save_file] = false;
            for (int i = 0; i < 4; i++)
                dungeon_gift_flags[save_file, i] = 0;
            for (int i = 0; i < 4; i++)
                dungeon_rooms_visited_flags[save_file, i] = 0;
            SaveFile(save_file);
        }
        public static bool GetCompassFlag(byte save_file, byte compass_index)
        {
            return (compass_flags[save_file] & (1 << compass_index)) >> compass_index == 1;
        }
        public static void SetCompassFlag(byte save_file, byte compass_index, bool value)
        {
            if (GetCompassFlag(save_file, compass_index) != value)
                compass_flags[save_file] ^= (ushort)(1 << compass_index);
        }
        public static bool GetMapFlag(byte save_file, byte map_index)
        {
            return (map_flags[save_file] & (1 << map_index)) >> map_index == 1;
        }
        public static void SetMapFlag(byte save_file, byte map_index, bool value)
        {
            if (GetMapFlag(save_file, map_index) != value)
                map_flags[save_file] ^= (ushort)(1 << map_index);
        }
        public static bool GetBombedHoleFlag(byte save_file, byte bombed_holes_index)
        {
            return (bombed_holes_flags[save_file] & (1L << bombed_holes_index)) >> bombed_holes_index == 1;
        }
        public static void SetBombedHoleFlag(byte save_file, byte bombed_holes_index, bool value)
        {
            if (GetBombedHoleFlag(save_file, bombed_holes_index) != value)
                bombed_holes_flags[save_file] ^= (long)(1L << bombed_holes_index);
        }
        public static bool GetOpenedKeyDoorsFlag(byte save_file, byte opened_key_doors_index)
        {
            return (opened_key_doors_flags[save_file] & (1L << opened_key_doors_index)) >> opened_key_doors_index == 1;
        }
        public static void SetOpenedKeyDoorsFlag(byte save_file, byte opened_key_doors_index, bool value)
        {
            if (GetOpenedKeyDoorsFlag(save_file, opened_key_doors_index) != value)
                opened_key_doors_flags[save_file] ^= (long)(1L << opened_key_doors_index);
        }
        public static bool GetBossKillsFlag(byte save_file, byte boss_kills_index)
        {
            return (boss_kills_flags[save_file] & (1 << boss_kills_index)) >> boss_kills_index == 1;
        }
        public static void SetBossKillsFlag(byte save_file, byte boss_kills_index, bool value)
        {
            if (GetBossKillsFlag(save_file, boss_kills_index) != value)
                boss_kills_flags[save_file] ^= 1 << boss_kills_index;
        }
        public static bool GetGiftFlag(byte save_file, byte gift_index)
        {
            return (gift_flags[save_file] & (1L << gift_index)) >> gift_index == 1;
        }
        public static void SetGiftFlag(byte save_file, byte gift_index, bool value)
        {
            if (GetGiftFlag(save_file, gift_index) != value)
                gift_flags[save_file] ^= (long)(1L << gift_index);
        }
        public static bool GetHeartContainerFlag(byte save_file, byte container_index)
        {
            return ((heart_container_flags[save_file] & (1 << container_index)) >> container_index) == 1;
        }
        public static void SetHeartContainerFlag(byte save_file, byte heart_container_index, bool value)
        {
            if (GetHeartContainerFlag(save_file, heart_container_index) != value)
                heart_container_flags[save_file] ^= (short)(1 << heart_container_index);
        }
        public static bool GetOverworldSecretsFlag(byte save_file, byte container_index)
        {
            return ((overworld_secrets_flags[save_file] & (1L << container_index)) >> container_index) == 1;
        }
        public static void SetOverworldSecretsFlag(byte save_file, byte overworld_secrets_index, bool value)
        {
            if (GetOverworldSecretsFlag(save_file, overworld_secrets_index) != value)
                overworld_secrets_flags[save_file] ^= 1L << overworld_secrets_index;
        }
        public static bool GetTriforceFlag(byte save_file, byte triforce_index)
        {
            return ((triforce_pieces[save_file] & (1 << triforce_index)) >> triforce_index) == 1;
        }
        public static void SetTriforceFlag(byte save_file, byte triforce_index, bool value)
        {
            if (GetOverworldSecretsFlag(save_file, triforce_index) != value)
                triforce_pieces[save_file] ^= (byte)(1 << triforce_index);
        }
        public static bool GetDungeonGiftFlag(byte save_file, byte room_index)
        {
            int index_in_number = room_index & 0b111111;
            return ((dungeon_gift_flags[save_file, room_index >> 6] & (1L << index_in_number)) >> index_in_number) == 1;
        }
        public static void SetDungeonGiftFlag(byte save_file, byte room_index, bool value)
        {
            if (GetDungeonGiftFlag(save_file, room_index) != value)
                dungeon_gift_flags[save_file, room_index >> 6] ^= 1L << (room_index & 0b111111);
        }
        public static bool GetDungeonVisitedRoomFlag(byte save_file, byte room_index)
        {
            int index_in_number = room_index & 0b111111;
            return ((dungeon_rooms_visited_flags[save_file, room_index >> 6] & (1L << index_in_number)) >> index_in_number) == 1;
        }
        public static void SetDungeonVisitedRoomFlag(byte save_file, byte room_index, bool value)
        {
            if (GetDungeonVisitedRoomFlag(save_file, room_index) != value)
                dungeon_rooms_visited_flags[save_file, room_index >> 6] ^= 1L << (room_index & 0b111111);
        }
    }
}