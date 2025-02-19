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

    public class UndergroundFireSprite : Sprite, ISmokeSpawn
    {
        public int smoke_timer { get; set; } = Program.RNG.Next(0, 120);
        public bool smoke_stage { get; set; } = true;

        StaticSprite counterpart = new StaticSprite(SpriteID.SMOKE_1, PaletteID.SP_1, 0, 0);

        public UndergroundFireSprite(int x, int y) : base(0x5c, (byte)PaletteID.SP_1)
        {
            this.x = x;
            this.y = y;
            unload_during_transition = true;
            counterpart.unload_during_transition = true;
            counterpart.x = x + 8;
            counterpart.y = y;
            Screen.sprites.Add(this);
            Screen.sprites.Add(counterpart);
            smoke_timer = 30;
            Action();
            Link.can_move = false;
        }

        public override void Action()
        {
            smoke_timer++;
            if (smoke_stage)
            {
                SetSmokeGraphic();
            }
            else
            {
                Flip();
            }
        }

        void Flip()
        {
            if (Program.gTimer % 12 == 0)
            {
                tile_index = 0x5e;
                xflip = true;
                counterpart.tile_index = 0x5c;
                counterpart.xflip = true;
            }
            else if (Program.gTimer % 12 == 6)
            {
                tile_index = 0x5c;
                xflip = false;
                counterpart.tile_index = 0x5e;
                counterpart.xflip = false;
            }
        }

        public void SetSmokeGraphic()
        {
            if (smoke_timer < 70)
            {
                tile_index = 0x70;
                counterpart.tile_index = 0x70;
                counterpart.xflip = true;
            }
            else if (smoke_timer == 70)
            {
                tile_index = 0x72;
                counterpart.tile_index = 0x72;
            }
            else if (smoke_timer == 76)
            {
                tile_index = 0x74;
                counterpart.tile_index = 0x74;
            }
            else if (smoke_timer == 82)
            {
                tile_index = 0x5c;
                counterpart.tile_index = 0x5e;
                palette_index = 6;
                counterpart.palette_index = 6;
                counterpart.xflip = false;
                smoke_stage = false;
                NPCCode.fire_appeared = true;
                Link.can_move = true;
            }
        }
    }

    internal class CaveNPC : Enemy//, ISmokeSpawn
    {
        //public int smoke_timer { get; set; } = Program.RNG.Next(0, 120);
        //public bool smoke_stage { get; set; } = true;
        public byte flash_timer = 200;

        //WarpCode.NPC displayed_npc;

        public CaveNPC(NPCCode.NPC displayed_npc) : base(AnimationMode.ONEFRAME_M, 0x70, 0, false, true, 0, 0, 0, true)
        {
            palette_index = (byte)PaletteID.SP_2;
            x = 120;
            y = 128;
            //this.displayed_npc = displayed_npc;
            if (displayed_npc == NPCCode.NPC.NONE)
            {
                shown = false;
                counterpart.shown = false;
            }
            unload_during_transition = true;
            counterpart.unload_during_transition = true;
            counterpart.x = x + 8;
            counterpart.y = y;
            smoke_random_appearance = (byte)Program.RNG.Next(0, 120);
            can_always_act = true;
            can_damage_link = false;
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

            Animation();
        }

        protected override void EnemySpecificActions()
        {
            HP = float.PositiveInfinity;
            //smoke_timer++;
            //if (smoke_stage)
            //{
            //    SetSmokeGraphic();
            //}
            if (flash_timer < 100)
            {
                FlashDissapear();
            }
        }

        //public void SetSmokeGraphic()
        //{
        //    if (smoke_timer < 70)
        //    {
        //        tile_index = 0x70;
        //        counterpart.tile_index = 0x70;
        //        counterpart.xflip = true;
        //    }
        //    else if (smoke_timer == 70)
        //    {
        //        tile_index = 0x72;
        //        counterpart.tile_index = 0x72;
        //    }
        //    else if (smoke_timer == 76)
        //    {
        //        tile_index = 0x74;
        //        counterpart.tile_index = 0x74;
        //    }
        //    else if (smoke_timer == 82)
        //    {
        //        switch (displayed_npc)
        //        {
        //            case WarpCode.NPC.OLD_MAN:
        //                tile_index = 0x98;
        //                counterpart.tile_index = 0x98;
        //                break;
        //            case WarpCode.NPC.OLD_WOMAN:
        //                tile_index = 0x9a;
        //                counterpart.tile_index = 0x9a;
        //                break;
        //            case WarpCode.NPC.SHOPKEEPER:
        //                tile_index = 0x9c;
        //                counterpart.tile_index = 0x9c;
        //                break;
        //            case WarpCode.NPC.GOBLIN:
        //                tile_index = 0xf8;
        //                counterpart.tile_index = 0xfa;
        //                break;
        //        }
        //        if (displayed_npc != WarpCode.NPC.SHOPKEEPER)
        //        {
        //            palette_index = 6;
        //            counterpart.palette_index = 6;
        //        }
        //        else
        //        {
        //            palette_index = 4;
        //            counterpart.palette_index = 4;
        //        }
        //        if (displayed_npc != WarpCode.NPC.GOBLIN)
        //            counterpart.xflip = true;
        //        else
        //            counterpart.xflip = false;
        //        smoke_stage = false;
        //    }
        //}

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
            base.OnDamaged();
        }
    }

    interface ISmokeSpawn
    {
        int smoke_timer { get; set; }
        bool smoke_stage { get; set; }
        void SetSmokeGraphic();
    }
}