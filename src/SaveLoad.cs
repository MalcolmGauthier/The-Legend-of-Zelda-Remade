namespace The_Legend_of_Zelda
{
    internal static class SaveLoad
    {
        public static byte current_save_file;

        public static bool[] save_file_exists = new bool[3];
        public static byte[,] file_name = new byte[3, 8];
        public static bool[] second_quest = new bool[3];

        private static byte[] _nb_of_hearts = new byte[3];
        private static byte[] _bomb_limit = new byte[3];
        private static byte[] _bomb_count = new byte[3];
        private static byte[] _rupy_count = new byte[3];
        private static byte[] _key_count = new byte[3];
        private static byte[] _death_count = new byte[3];
        private static bool[] _triforce_of_power = new bool[3];
        private static bool[] _wooden_sword = new bool[3];
        private static bool[] _white_sword = new bool[3];
        private static bool[] _magical_sword = new bool[3];
        private static bool[] _raft = new bool[3];
        private static bool[] _book_of_magic = new bool[3];
        private static bool[] _blue_ring = new bool[3];
        private static bool[] _red_ring = new bool[3];
        private static bool[] _ladder = new bool[3];
        private static bool[] _magical_key = new bool[3];
        private static bool[] _power_bracelet = new bool[3];
        private static bool[] _boomerang = new bool[3];
        private static bool[] _magical_boomerang = new bool[3];
        private static bool[] _bow = new bool[3];
        private static bool[] _arrow = new bool[3];
        private static bool[] _silver_arrow = new bool[3];
        private static bool[] _blue_candle = new bool[3];
        private static bool[] _red_candle = new bool[3];
        private static bool[] _recorder = new bool[3];
        private static bool[] _bait = new bool[3];
        private static bool[] _letter = new bool[3];
        private static bool[] _blue_potion = new bool[3];
        private static bool[] _red_potion = new bool[3];
        private static bool[] _magical_rod = new bool[3];
        private static bool[] _potion_shop_activated = new bool[3];
        private static bool[] _magical_shield = new bool[3];

        private static byte[] triforce_pieces = new byte[3];
        private static ushort[] map_flags = new ushort[3];
        private static ushort[] compass_flags = new ushort[3];
        private static short[] heart_container_flags = new short[3];
        private static int[] boss_kills_flags = new int[3];
        private static long[] gift_flags = new long[3];
        private static long[] bombed_holes_flags = new long[3];
        private static long[] opened_key_doors_flags = new long[3];
        private static long[] overworld_secrets_flags = new long[3];
        private static long[,] dungeon_gift_flags = new long[3, 4];
        private static long[,] dungeon_rooms_visited_flags = new long[3, 4];
        //public static bool[] _ = new bool[3];

        public static void SaveFile(byte save_file)
        {
            try
            {
                using (Stream stream = File.OpenWrite(@"SaveData.bin"))
                {
                    BinaryWriter bw = new BinaryWriter(stream);
                    bw.Seek(save_file * 0x200, SeekOrigin.Begin);
                    bw.Write(_nb_of_hearts[save_file]);
                    bw.Write(_bomb_limit[save_file]);
                    bw.Write(_bomb_count[save_file]);
                    bw.Write(_rupy_count[save_file]);
                    bw.Write(_key_count[save_file]);
                    bw.Write(triforce_pieces[save_file]);
                    bw.Write(_triforce_of_power[save_file]);
                    bw.Write(_wooden_sword[save_file]);
                    bw.Write(_white_sword[save_file]);
                    bw.Write(_magical_sword[save_file]);
                    bw.Write(_raft[save_file]);
                    bw.Write(_book_of_magic[save_file]);
                    bw.Write(_blue_ring[save_file]);
                    bw.Write(_red_ring[save_file]);
                    bw.Write(_ladder[save_file]);
                    bw.Write(_magical_key[save_file]);
                    bw.Write(_power_bracelet[save_file]);
                    bw.Write(_boomerang[save_file]);
                    bw.Write(_magical_boomerang[save_file]);
                    bw.Write(_bow[save_file]);
                    bw.Write(_arrow[save_file]);
                    bw.Write(_silver_arrow[save_file]);
                    bw.Write(_blue_candle[save_file]);
                    bw.Write(_red_candle[save_file]);
                    bw.Write(_recorder[save_file]);
                    bw.Write(_bait[save_file]);
                    bw.Write(_letter[save_file]);
                    bw.Write(_blue_potion[save_file]);
                    bw.Write(_red_potion[save_file]);
                    bw.Write(_magical_rod[save_file]);
                    bw.Write(compass_flags[save_file]);
                    bw.Write(map_flags[save_file]);
                    bw.Write(boss_kills_flags[save_file]);
                    bw.Write(bombed_holes_flags[save_file]);
                    bw.Write(opened_key_doors_flags[save_file]);
                    bw.Write(gift_flags[save_file]);
                    bw.Write(second_quest[save_file]);
                    bw.Write(_death_count[save_file]);
                    bw.Write(heart_container_flags[save_file]);
                    bw.Write(_potion_shop_activated[save_file]);
                    bw.Write(_magical_shield[save_file]);
                    bw.Write(overworld_secrets_flags[save_file]);
                    for (int i = 0; i < 4; i++)
                        bw.Write(dungeon_gift_flags[save_file, i]);
                    for (int i = 0; i < 4; i++)
                        bw.Write(dungeon_rooms_visited_flags[save_file, i]);
                    for (int i = 0; i < 8; i++)
                        bw.Write(file_name[save_file, i]);
                    bw.Write(save_file_exists[save_file]);
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
                using (Stream stream = File.OpenRead(@"SaveData.bin"))
                {
                    BinaryReader br = new BinaryReader(stream);
                    br.ReadBytes(save_file * 0x200);
                    _nb_of_hearts[save_file] = br.ReadByte();
                    _bomb_limit[save_file] = br.ReadByte();
                    _bomb_count[save_file] = br.ReadByte();
                    _rupy_count[save_file] = br.ReadByte();
                    _key_count[save_file] = br.ReadByte();
                    triforce_pieces[save_file] = br.ReadByte();
                    _triforce_of_power[save_file] = br.ReadBoolean();
                    _wooden_sword[save_file] = br.ReadBoolean();
                    _white_sword[save_file] = br.ReadBoolean();
                    _magical_sword[save_file] = br.ReadBoolean();
                    _raft[save_file] = br.ReadBoolean();
                    _book_of_magic[save_file] = br.ReadBoolean();
                    _blue_ring[save_file] = br.ReadBoolean();
                    _red_ring[save_file] = br.ReadBoolean();
                    _ladder[save_file] = br.ReadBoolean();
                    _magical_key[save_file] = br.ReadBoolean();
                    _power_bracelet[save_file] = br.ReadBoolean();
                    _boomerang[save_file] = br.ReadBoolean();
                    _magical_boomerang[save_file] = br.ReadBoolean();
                    _bow[save_file] = br.ReadBoolean();
                    _arrow[save_file] = br.ReadBoolean();
                    _silver_arrow[save_file] = br.ReadBoolean();
                    _blue_candle[save_file] = br.ReadBoolean();
                    _red_candle[save_file] = br.ReadBoolean();
                    _recorder[save_file] = br.ReadBoolean();
                    _bait[save_file] = br.ReadBoolean();
                    _letter[save_file] = br.ReadBoolean();
                    _blue_potion[save_file] = br.ReadBoolean();
                    _red_potion[save_file] = br.ReadBoolean();
                    _magical_rod[save_file] = br.ReadBoolean();
                    compass_flags[save_file] = br.ReadUInt16();
                    map_flags[save_file] = br.ReadUInt16();
                    boss_kills_flags[save_file] = br.ReadInt32();
                    bombed_holes_flags[save_file] = br.ReadInt64();
                    opened_key_doors_flags[save_file] = br.ReadInt64();
                    gift_flags[save_file] = br.ReadInt64();
                    second_quest[save_file] = br.ReadBoolean();
                    _death_count[save_file] = br.ReadByte();
                    heart_container_flags[save_file] = br.ReadInt16();
                    _potion_shop_activated[save_file] = br.ReadBoolean();
                    _magical_shield[save_file] = br.ReadBoolean();
                    overworld_secrets_flags[save_file] = br.ReadInt64();
                    for (int i = 0; i < 4; i++)
                        dungeon_gift_flags[save_file, i] = br.ReadInt64();
                    for (int i = 0; i < 4; i++)
                        dungeon_rooms_visited_flags[save_file, i] = br.ReadInt64();
                    for (int i = 0; i < 8; i++)
                        file_name[save_file, i] = br.ReadByte();
                    save_file_exists[save_file] = br.ReadBoolean();
                }
            }
            catch (Exception ex)
            {
                SDL2.SDL.SDL_SetError(ex.Message);
            }
        }
        public static void DeleteData(byte save_file)
        {
            _nb_of_hearts[save_file] = 3;
            _bomb_limit[save_file] = 8;
            _bomb_count[save_file] = 0;
            _rupy_count[save_file] = 0;
            _key_count[save_file] = 0;
            triforce_pieces[save_file] = 0;
            _triforce_of_power[save_file] = false;
            _wooden_sword[save_file] = false;
            _white_sword[save_file] = false;
            _magical_sword[save_file] = false;
            _raft[save_file] = false;
            _book_of_magic[save_file] = false;
            _blue_ring[save_file] = false;
            _red_ring[save_file] = false;
            _ladder[save_file] = false;
            _magical_key[save_file] = false;
            _power_bracelet[save_file] = false;
            _boomerang[save_file] = false;
            _magical_boomerang[save_file] = false;
            _bow[save_file] = false;
            _arrow[save_file] = false;
            _silver_arrow[save_file] = false;
            _blue_candle[save_file] = false;
            _red_candle[save_file] = false;
            _recorder[save_file] = false;
            _bait[save_file] = false;
            _letter[save_file] = false;
            _blue_potion[save_file] = false;
            _red_potion[save_file] = false;
            _magical_rod[save_file] = false;
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
            _death_count[save_file] = 0;
            save_file_exists[save_file] = false;
            heart_container_flags[save_file] = 0;
            _potion_shop_activated[save_file] = false;
            _magical_shield[save_file] = false;
            for (int i = 0; i < 4; i++)
                dungeon_gift_flags[save_file, i] = 0;
            for (int i = 0; i < 4; i++)
                dungeon_rooms_visited_flags[save_file, i] = 0;
            SaveFile(save_file);
        }

        // returns the basic information the file select code needs to display a file correctly.
        // this is an exception because this is the only time outside of gameplay within a specific file where we need
        // to know info for a specific file of the 3.
        // format: blue ring, red ring, nb of hearts, death count
        public static (bool blue_ring, bool red_ring, byte nb_of_hearts, byte death_count) GetBasicFileInfo(int index)
        {
            return (_blue_ring[index], _red_ring[index], _nb_of_hearts[index], _death_count[index]);
        }

        public static byte nb_of_hearts
        {
            get
            {
                return _nb_of_hearts[current_save_file];
            }
            set
            {
                _nb_of_hearts[current_save_file] = value;
            }
        }
        public static byte bomb_count
        {
            get
            {
                return _bomb_count[current_save_file];
            }
            set
            {
                _bomb_count[current_save_file] = value;
            }
        }
        public static byte bomb_limit
        {
            get
            {
                return _bomb_limit[current_save_file];
            }
            set
            {
                _bomb_limit[current_save_file] = value;
            }
        }
        public static byte rupy_count
        {
            get
            {
                return _rupy_count[current_save_file];
            }
            set
            {
                _rupy_count[current_save_file] = value;
            }
        }
        public static byte key_count
        {
            get
            {
                return _key_count[current_save_file];
            }
            set
            {
                _key_count[current_save_file] = value;
            }
        }
        public static byte death_count
        {
            get
            {
                return _death_count[current_save_file];
            }
            set
            {
                _death_count[current_save_file] = value;
            }
        }
        public static bool triforce_of_power
        {
            get
            {
                return _triforce_of_power[current_save_file];
            }
            set
            {
                _triforce_of_power[current_save_file] = value;
            }
        }
        public static bool wooden_sword
        {
            get
            {
                return _wooden_sword[current_save_file];
            }
            set
            {
                _wooden_sword[current_save_file] = value;
            }
        }
        public static bool white_sword
        {
            get
            {
                return _white_sword[current_save_file];
            }
            set
            {
                _white_sword[current_save_file] = value;
            }
        }
        public static bool magical_sword
        {
            get
            {
                return _magical_sword[current_save_file];
            }
            set
            {
                _magical_sword[current_save_file] = value;
            }
        }
        public static bool raft
        {
            get
            {
                return _raft[current_save_file];
            }
            set
            {
                _raft[current_save_file] = value;
            }
        }
        public static bool book_of_magic
        {
            get
            {
                return _book_of_magic[current_save_file];
            }
            set
            {
                _book_of_magic[current_save_file] = value;
            }
        }
        public static bool blue_ring
        {
            get
            {
                return _blue_ring[current_save_file];
            }
            set
            {
                _blue_ring[current_save_file] = value;
            }
        }
        public static bool red_ring
        {
            get
            {
                return _red_ring[current_save_file];
            }
            set
            {
                _red_ring[current_save_file] = value;
            }
        }
        public static bool ladder
        {
            get
            {
                return _ladder[current_save_file];
            }
            set
            {
                _ladder[current_save_file] = value;
            }
        }
        public static bool magical_key
        {
            get
            {
                return _magical_key[current_save_file];
            }
            set
            {
                _magical_key[current_save_file] = value;
            }
        }
        public static bool power_bracelet
        {
            get
            {
                return _power_bracelet[current_save_file];
            }
            set
            {
                _power_bracelet[current_save_file] = value;
            }
        }
        public static bool boomerang
        {
            get
            {
                return _boomerang[current_save_file];
            }
            set
            {
                _boomerang[current_save_file] = value;
            }
        }
        public static bool magical_boomerang
        {
            get
            {
                return _magical_boomerang[current_save_file];
            }
            set
            {
                _magical_boomerang[current_save_file] = value;
            }
        }
        public static bool bow
        {
            get
            {
                return _bow[current_save_file];
            }
            set
            {
                _bow[current_save_file] = value;
            }
        }
        public static bool arrow
        {
            get
            {
                return _arrow[current_save_file];
            }
            set
            {
                _arrow[current_save_file] = value;
            }
        }
        public static bool silver_arrow
        {
            get
            {
                return _silver_arrow[current_save_file];
            }
            set
            {
                _silver_arrow[current_save_file] = value;
            }
        }
        public static bool blue_candle
        {
            get
            {
                return _blue_candle[current_save_file];
            }
            set
            {
                _blue_candle[current_save_file] = value;
            }
        }
        public static bool red_candle
        {
            get
            {
                return _red_candle[current_save_file];
            }
            set
            {
                _red_candle[current_save_file] = value;
            }
        }
        public static bool recorder
        {
            get
            {
                return _recorder[current_save_file];
            }
            set
            {
                _recorder[current_save_file] = value;
            }
        }
        public static bool bait
        {
            get
            {
                return _bait[current_save_file];
            }
            set
            {
                _bait[current_save_file] = value;
            }
        }
        public static bool letter
        {
            get
            {
                return _letter[current_save_file];
            }
            set
            {
                _letter[current_save_file] = value;
            }
        }
        public static bool blue_potion
        {
            get
            {
                return _blue_potion[current_save_file];
            }
            set
            {
                _blue_potion[current_save_file] = value;
            }
        }
        public static bool red_potion
        {
            get
            {
                return _red_potion[current_save_file];
            }
            set
            {
                _red_potion[current_save_file] = value;
            }
        }
        public static bool magical_rod
        {
            get
            {
                return _magical_rod[current_save_file];
            }
            set
            {
                _magical_rod[current_save_file] = value;
            }
        }
        public static bool potion_shop_activated
        {
            get
            {
                return _potion_shop_activated[current_save_file];
            }
            set
            {
                _potion_shop_activated[current_save_file] = value;
            }
        }
        public static bool magical_shield
        {
            get
            {
                return _magical_shield[current_save_file];
            }
            set
            {
                _magical_shield[current_save_file] = value;
            }
        }

        public static bool GetCompassFlag(byte compass_index)
        {
            return (compass_flags[current_save_file] & 1 << compass_index) >> compass_index == 1;
        }
        public static void SetCompassFlag(byte compass_index, bool value)
        {
            if (GetCompassFlag(compass_index) != value)
                compass_flags[current_save_file] ^= (ushort)(1 << compass_index);
        }
        public static bool GetMapFlag(byte map_index)
        {
            return (map_flags[current_save_file] & 1 << map_index) >> map_index == 1;
        }
        public static void SetMapFlag(byte map_index, bool value)
        {
            if (GetMapFlag(map_index) != value)
                map_flags[current_save_file] ^= (ushort)(1 << map_index);
        }
        public static bool GetBombedHoleFlag(byte bombed_holes_index)
        {
            return (bombed_holes_flags[current_save_file] & 1L << bombed_holes_index) >> bombed_holes_index == 1;
        }
        public static void SetBombedHoleFlag(byte bombed_holes_index, bool value)
        {
            if (GetBombedHoleFlag(bombed_holes_index) != value)
                bombed_holes_flags[current_save_file] ^= 1L << bombed_holes_index;
        }
        public static bool GetOpenedKeyDoorsFlag(byte opened_key_doors_index)
        {
            return (opened_key_doors_flags[current_save_file] & 1L << opened_key_doors_index) >> opened_key_doors_index == 1;
        }
        public static void SetOpenedKeyDoorsFlag(byte opened_key_doors_index, bool value)
        {
            if (GetOpenedKeyDoorsFlag(opened_key_doors_index) != value)
                opened_key_doors_flags[current_save_file] ^= 1L << opened_key_doors_index;
        }
        public static bool GetBossKillsFlag(byte boss_kills_index)
        {
            return (boss_kills_flags[current_save_file] & 1 << boss_kills_index) >> boss_kills_index == 1;
        }
        public static void SetBossKillsFlag(byte boss_kills_index, bool value)
        {
            if (GetBossKillsFlag(boss_kills_index) != value)
                boss_kills_flags[current_save_file] ^= 1 << boss_kills_index;
        }
        public static bool GetGiftFlag(byte gift_index)
        {
            return (gift_flags[current_save_file] & 1L << gift_index) >> gift_index == 1;
        }
        public static void SetGiftFlag(byte gift_index, bool value)
        {
            if (GetGiftFlag(gift_index) != value)
                gift_flags[current_save_file] ^= 1L << gift_index;
        }
        public static bool GetHeartContainerFlag(byte container_index)
        {
            return (heart_container_flags[current_save_file] & 1 << container_index) >> container_index == 1;
        }
        public static void SetHeartContainerFlag(byte heart_container_index, bool value)
        {
            if (GetHeartContainerFlag(heart_container_index) != value)
                heart_container_flags[current_save_file] ^= (short)(1 << heart_container_index);
        }
        public static bool GetOverworldSecretsFlag(byte container_index)
        {
            return (overworld_secrets_flags[current_save_file] & 1L << container_index) >> container_index == 1;
        }
        public static void SetOverworldSecretsFlag(byte overworld_secrets_index, bool value)
        {
            if (GetOverworldSecretsFlag(overworld_secrets_index) != value)
                overworld_secrets_flags[current_save_file] ^= 1L << overworld_secrets_index;
        }
        public static bool GetTriforceFlag(byte triforce_index)
        {
            return (triforce_pieces[current_save_file] & 1 << triforce_index) >> triforce_index == 1;
        }
        public static void SetTriforceFlag(byte triforce_index, bool value)
        {
            if (GetOverworldSecretsFlag(triforce_index) != value)
                triforce_pieces[current_save_file] ^= (byte)(1 << triforce_index);
        }
        public static bool GetDungeonGiftFlag(byte room_index)
        {
            int index_in_number = room_index & 0b111111;
            return (dungeon_gift_flags[current_save_file, room_index >> 6] & 1L << index_in_number) >> index_in_number == 1;
        }
        public static void SetDungeonGiftFlag(byte room_index, bool value)
        {
            if (GetDungeonGiftFlag(room_index) != value)
                dungeon_gift_flags[current_save_file, room_index >> 6] ^= 1L << (room_index & 0b111111);
        }
        public static bool GetDungeonVisitedRoomFlag(byte room_index)
        {
            // 4 int64s to make up the 256 bits needed to store the info, the upper 2 bits of the byte indicate which int64 to check
            int index_in_number = room_index & 0b111111;
            return (dungeon_rooms_visited_flags[current_save_file, room_index >> 6] & 1L << index_in_number) >> index_in_number == 1;
        }
        public static void SetDungeonVisitedRoomFlag(byte room_index, bool value)
        {
            if (GetDungeonVisitedRoomFlag(room_index) != value)
                dungeon_rooms_visited_flags[current_save_file, room_index >> 6] ^= 1L << (room_index & 0b111111);
        }
    }
}