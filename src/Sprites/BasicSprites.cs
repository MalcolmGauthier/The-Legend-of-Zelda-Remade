using The_Legend_of_Zelda.Gameplay;
using The_Legend_of_Zelda.Rendering;
using static The_Legend_of_Zelda.Gameplay.Program;

namespace The_Legend_of_Zelda.Sprites
{
    public class StaticSprite : Sprite
    {
        public StaticSprite(byte tile_index, byte palette_index, int x, int y, bool xflip = false, bool yflip = false) : base(tile_index, palette_index)
        {
            this.x = x;
            this.y = y;
            this.xflip = xflip;
            this.yflip = yflip;
        }
        public StaticSprite(SpriteID tile_index, PaletteID palette_index, int x, int y, bool xflip = false, bool yflip = false) : base((byte)tile_index, (byte)palette_index)
        {
            this.x = x;
            this.y = y;
            this.xflip = xflip;
            this.yflip = yflip;
        }

        public override void Action()
        {
            return;
        }
    }

    public class FlickeringSprite : Sprite
    {
        byte frames_per_flicker;
        byte second_tile_index;
        byte first_tile_index;
        byte first_palette_index;
        internal byte second_palette_index;
        public FlickeringSprite(byte tile_index, byte palette_index, int x, int y, byte frames_per_flicker, byte second_tile_index,
            bool xflip = false, bool yflip = false, byte second_palette_index = 255) : base(tile_index, palette_index)
        {
            this.x = x;
            this.y = y;
            this.xflip = xflip;
            this.yflip = yflip;
            this.frames_per_flicker = frames_per_flicker;
            first_tile_index = tile_index;
            this.second_tile_index = second_tile_index;
            first_palette_index = palette_index;
            if (second_palette_index != 255)
                this.second_palette_index = second_palette_index;
            else
                this.second_palette_index = palette_index;
        }

        public override void Action()
        {
            if (Program.gTimer % frames_per_flicker == 0)
            {
                if (tile_index == second_tile_index && palette_index == second_palette_index)
                {
                    tile_index = first_tile_index;
                    palette_index = first_palette_index;
                }
                else
                {
                    tile_index = second_tile_index;
                    palette_index = second_palette_index;
                }
            }
        }
    }

    public class WaterFallSprite : Sprite
    {
        public WaterFallSprite(byte tile_index, byte palette_index, int x, int y) : base(tile_index, palette_index)
        {
            this.x = x;
            this.y = y;
        }
        public override void Action()
        {
            y += 2;
            if (y == 228)
            {
                y = 178;
                tile_index -= 16;
            }
            else if (y == 194 || y == 186)
            {
                tile_index += 8;
            }
        }
    }

    internal class UndergroundFireSprite : Enemy
    {
        static byte shared_init_time = 0;
        static bool init_time_shared = false;

        public UndergroundFireSprite(int x, int y) : base(AnimationMode.TWOFRAMES, 0, 0, false, true, 6, 0, 0, true)
        {
            this.x = x;
            this.y = y;
            unaffected_by_clock = true;
            palette_index = (byte)PaletteID.SP_2;
            counterpart.palette_index = (byte)PaletteID.SP_2;

            Link.can_move = false;
            invincible = true;
            can_always_act = true;
            HP = 1;

            // sync the two smoke appearence times
            if (init_time_shared)
            {
                smoke_random_appearance = shared_init_time;
            }
            else
            {
                // advance smoke graphic to force smoke_random_appearence initialization
                Action();
                smoke_random_appearance = 4;
                shared_init_time = 4;
            }
            init_time_shared = !init_time_shared;
        }

        protected override void EnemySpecificActions()
        {
            return;
        }

        protected override void Animation()
        {
            if (Program.gTimer % 12 < 6)
            {
                tile_index = (byte)SpriteID.FIRE_R;
                xflip = true;
                counterpart.tile_index = (byte)SpriteID.FIRE_L;
                counterpart.xflip = true;
            }
            else
            {
                tile_index = (byte)SpriteID.FIRE_L;
                xflip = false;
                counterpart.tile_index = (byte)SpriteID.FIRE_R;
                counterpart.xflip = false;
            }
        }
    }

    internal class CaveNPC : Enemy
    {
        public byte flash_timer = 200;
        bool aggressive = false;

        public CaveNPC(NPCCode.NPC displayed_npc) : base(AnimationMode.ONEFRAME_M, 0x70, 0, false, true, 0, 0, 0, true)
        {
            if (displayed_npc == NPCCode.NPC.NONE)
            {
                shown = false;
                counterpart.shown = false;
                Screen.sprites.Remove(counterpart);
                Screen.sprites.Remove(this);
                return;
            }

            palette_index = (byte)PaletteID.SP_2;
            x = 120;
            y = 128;

            unload_during_transition = true;
            counterpart.unload_during_transition = true;
            counterpart.x = x + 8;
            counterpart.y = y;
            can_always_act = true;
            can_damage_link = false;
            unaffected_by_clock = true;
            HP = float.PositiveInfinity;
            if (Program.gamemode == Gamemode.OVERWORLD)
                invincible = true;

            switch (displayed_npc)
            {
                case NPCCode.NPC.OLD_MAN:
                    tile_location_1 = 0x98;
                    break;
                case NPCCode.NPC.OLD_WOMAN:
                    tile_location_1 = 0x9a;
                    break;
                case NPCCode.NPC.SHOPKEEPER:
                    tile_location_1 = 0x9c;
                    break;
                case NPCCode.NPC.GOBLIN:
                    animation_mode = AnimationMode.ONEFRAME;
                    tile_location_1 = 0xf8;
                    tile_location_2 = 0xfa;
                    break;
                case NPCCode.NPC.GORYIA:
                    animation_mode = AnimationMode.ONEFRAME;
                    tile_location_1 = 0xb0;
                    tile_location_2 = 0xb2;
                    break;
            }

            if (displayed_npc == NPCCode.NPC.SHOPKEEPER)
            {
                palette_index = (byte)PaletteID.SP_0;
                counterpart.palette_index = (byte)PaletteID.SP_0;
            }
            else
            {
                palette_index = (byte)PaletteID.SP_2;
                counterpart.palette_index = (byte)PaletteID.SP_2;
            }

            //Animation();

            // forces custom smoke spawn timer
            Action();
            smoke_random_appearance = (byte)Program.RNG.Next(0, 60);
        }

        protected override void EnemySpecificActions()
        {
            if (flash_timer < 100)
            {
                FlashDissapear();
            }
        }

        public void FlashDissapear()
        {
            flash_timer++;
            if (flash_timer % 2 == 1)
            {
                shown = true;
                counterpart.shown = true;
            }
            else
            {
                shown = false;
                counterpart.shown = false;
            }

            if (flash_timer == 100)
            {
                NPCCode.npc_gone = true;
                Program.DC.npc_active = false;
                Screen.sprites.Remove(counterpart);
                Screen.sprites.Remove(this);
            }
        }

        protected override void OnDamaged()
        {
            if (Program.gamemode == Gamemode.DUNGEON && !aggressive)
            {
                // the fire starts spitting fireballs just like the statues. the statues class handles this case
                new Statues();
                aggressive = true;
            }
        }

        protected override void OnInit()
        {
            //NPCCode.link_can_move = false;
            NPCCode.npc_appeared = true;
            Menu.can_open_menu = false;
        }
    }
}