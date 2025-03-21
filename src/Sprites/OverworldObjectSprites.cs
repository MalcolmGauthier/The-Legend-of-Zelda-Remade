﻿using The_Legend_of_Zelda.Gameplay;
using The_Legend_of_Zelda.Rendering;
using static The_Legend_of_Zelda.Gameplay.Program;

namespace The_Legend_of_Zelda.Sprites
{
    internal class OverworldFairySprite : FlickeringSprite
    {
        bool used = false;
        short animation_timer = 0;
        FairyHeartSprite[] hearts = new FairyHeartSprite[8];

        public OverworldFairySprite() : base(0x50, 6, 124, 126, 4, 0x52)
        {
            // TODO: replace with fairy sound
            Sound.PlaySFX(Sound.SoundEffects.HEART);
            unload_during_transition = true;
            Screen.sprites.Add(this);
        }

        public override void Action()
        {
            base.Action();

            if (!used)
            {
                if (LinkInRange())
                {
                    used = true;
                    Link.full_heal_flag = true;
                    OC.fairy_animation_active = true;
                }
            }
            else
            {
                if (animation_timer % 11 == 0 && animation_timer <= 77)
                {
                    int heart_index = animation_timer / 11;
                    hearts[heart_index] = new FairyHeartSprite((byte)heart_index, this);
                }
                else if (animation_timer >= 100 && Link.hp == SaveLoad.nb_of_hearts)
                {
                    for (int i = 0; i < hearts.Length; i++)
                        hearts[i].Kill();

                    OC.fairy_animation_active = false;
                }

                if (OC.fairy_animation_active)
                    Link.can_move = false;
                else
                    Link.can_move = true;

                animation_timer++;
            }
        }

        bool LinkInRange()
        {
            return Link.y <= 176 && Link.y >= 168 && Link.x >= 112 && Link.x <= 128;
        }

        class FairyHeartSprite : Sprite
        {
            byte heart_index;
            OverworldFairySprite host;

            public FairyHeartSprite(byte heart_index, OverworldFairySprite host) : base(0xf2, 6)
            {
                use_chr_rom = true;
                UpdateTexture();
                this.heart_index = heart_index;
                this.host = host;
                Screen.sprites.Add(this);
            }

            public override void Action()
            {
                x = -53 * (int)MathF.Sin((host.animation_timer - 11 * heart_index) / 88f * 2 * MathF.PI) + 126;
                y = -53 * (int)MathF.Cos((host.animation_timer - 11 * heart_index) / 88f * 2 * MathF.PI) + 150;
            }

            public void Kill()
            {
                Screen.sprites.Remove(this);
            }
        }
    }

    internal class RaftSprite : Sprite
    {
        StaticSprite counterpart = new StaticSprite(0x6c, 4, Link.x + 8, Link.y, true);

        public RaftSprite() : base(0x6c, 4)
        {
            x = Link.x;
            y = Link.y;
            Screen.sprites.Add(this);
            Screen.sprites.Add(counterpart);
        }

        public override void Action()
        {
            x = Link.x;
            y = Link.y;
            counterpart.x = Link.x + 8;
            counterpart.y = Link.y;

            if (Link.facing_direction == Direction.UP)
                Link.current_action = LinkAction.WALKING_UP;
            else
                Link.current_action = LinkAction.WALKING_DOWN;

            if ((Screen.GetMetaTileTypeAtLocation(Link.x + 8, Link.y - 1) == MetatileType.DOCK && Link.facing_direction == Direction.DOWN) ||
                (!OC.ScrollingDone() && Link.facing_direction == Direction.UP))
            {
                OC.raft_flag = false;
                Link.can_move = true;
                Screen.sprites.Remove(counterpart);
                Screen.sprites.Remove(this);
            }
        }
    }

    internal class MovingTileSprite : Sprite
    {
        public enum MovingTile
        {
            ROCK,
            GREEN_ROCK,
            TOMBSTONE,
            DUNGEON_BLOCK
        }

        byte local_timer = 0;
        byte plt_to_pick;
        int metatile_index;
        int ppu_tile_location;
        MovingTile moving_tile;
        Direction direction;
        StaticSprite counterpart = new StaticSprite(0, 7, 0, 0);

        public MovingTileSprite(MovingTile moving_tile, int metatile_index) : base(0, 7)
        {
            use_chr_rom = true;
            counterpart.use_chr_rom = true;
            this.moving_tile = moving_tile;
            this.metatile_index = metatile_index;

            byte new_tile = 0;
            switch (moving_tile)
            {
                case MovingTile.ROCK:
                    new_tile = 0xc8;
                    plt_to_pick = 3;
                    break;
                case MovingTile.GREEN_ROCK:
                    new_tile = 0xc8;
                    plt_to_pick = 2;
                    break;
                case MovingTile.TOMBSTONE:
                    new_tile = 0xbc;
                    plt_to_pick = 0;
                    break;
                case MovingTile.DUNGEON_BLOCK:
                    new_tile = 0xb0;
                    plt_to_pick = 2;
                    break;
            }

            tile_index = new_tile;
            counterpart.tile_index = (byte)(new_tile + 2);
            for (byte i = 0; i < 4; i++)
            {
                Palettes.LoadPalette(PaletteID.SP_3, i, (Color)Palettes.active_palette_list[plt_to_pick * 4 + i]);
            }

            UpdateTexture();
            counterpart.UpdateTexture();
            if (moving_tile == MovingTile.DUNGEON_BLOCK)
                Screen.meta_tiles[metatile_index].tile_index_D = DungeonMetatile.WALL;
            else
                Screen.meta_tiles[metatile_index].tile_index = MetatileType.ROCK;
            Screen.meta_tiles[metatile_index].special = false;
            x = metatile_index % 16 * 16;
            y = (metatile_index / 16) * 16 + 64;

            ppu_tile_location = 256 + (metatile_index >> 4) * 64 + metatile_index % 16 * 2;
            if (moving_tile == MovingTile.DUNGEON_BLOCK)
            {
                Textures.ppu[ppu_tile_location] = 0x74;
                Textures.ppu[ppu_tile_location + 1] = 0x76;
                Textures.ppu[ppu_tile_location + 32] = 0x75;
                Textures.ppu[ppu_tile_location + 33] = 0x77;
            }
            else
            {
                Textures.ppu[ppu_tile_location] = 0x26;
                Textures.ppu[ppu_tile_location + 1] = 0x26;
                Textures.ppu[ppu_tile_location + 32] = 0x26;
                Textures.ppu[ppu_tile_location + 33] = 0x26;
            }

            direction = Link.facing_direction;
            Screen.sprites.Add(this);
            Screen.sprites.Add(counterpart);
        }

        public override void Action()
        {
            if (local_timer == 32)
            {
                bool is_overworld = gamemode == Gamemode.OVERWORLD;
                if (is_overworld && moving_tile != MovingTile.TOMBSTONE)
                {
                    SaveLoad.SetOverworldSecretsFlag((byte)Array.IndexOf(OC.screens_with_secrets_list, OC.current_screen), true);
                    Textures.LoadPPUPage(Textures.PPUDataGroup.OVERWORLD, OC.current_screen, 0);
                }

                if (moving_tile == MovingTile.TOMBSTONE)
                {
                    Textures.ppu[ppu_tile_location] = 0x70;
                    Textures.ppu[ppu_tile_location + 1] = 0x72;
                    Textures.ppu[ppu_tile_location + 32] = 0x71;
                    Textures.ppu[ppu_tile_location + 33] = 0x73;
                }
                else if (moving_tile == MovingTile.DUNGEON_BLOCK)
                {
                    Textures.ppu[ppu_tile_location] = 0x74;
                    Textures.ppu[ppu_tile_location + 1] = 0x76;
                    Textures.ppu[ppu_tile_location + 32] = 0x75;
                    Textures.ppu[ppu_tile_location + 33] = 0x77;
                }
                else
                {
                    Textures.ppu[ppu_tile_location] = 0x26;
                    Textures.ppu[ppu_tile_location + 1] = 0x26;
                    Textures.ppu[ppu_tile_location + 32] = 0x26;
                    Textures.ppu[ppu_tile_location + 33] = 0x26;
                }

                // TODO: play secret sfx

                int offset;
                if (direction == Direction.UP)
                    offset = -16;
                else if (direction == Direction.DOWN)
                    offset = 16;
                else if (direction == Direction.LEFT)
                    offset = -1;
                else
                    offset = 1;

                if (is_overworld)
                {
                    if (moving_tile == MovingTile.TOMBSTONE)
                        Screen.meta_tiles[metatile_index].tile_index = MetatileType.STAIRS;
                    else
                        Screen.meta_tiles[metatile_index].tile_index = MetatileType.GROUND;
                    Screen.meta_tiles[metatile_index + offset].tile_index = MetatileType.ROCK_SNAIL;
                }
                else
                {
                    Screen.meta_tiles[metatile_index].tile_index_D = DungeonMetatile.GROUND;
                    Screen.meta_tiles[metatile_index + offset].tile_index_D = DungeonMetatile.WALL;
                    DC.block_push_flag = true;
                }
                Screen.meta_tiles[metatile_index + offset].special = false;

                if (MathF.Abs(offset) == 16)
                    offset *= 2;
                ppu_tile_location += offset * 2;

                byte tile_to_use = tile_index;

                Textures.ppu[ppu_tile_location] = tile_to_use;
                Textures.ppu[ppu_tile_location + 1] = (byte)(tile_to_use + 2);
                Textures.ppu[ppu_tile_location + 32] = (byte)(tile_to_use + 1);
                Textures.ppu[ppu_tile_location + 33] = (byte)(tile_to_use + 3);

                if (is_overworld)
                {
                    Palettes.LoadPaletteGroup(PaletteID.SP_3, Palettes.PaletteGroups.OVERWORLD_DARK_ENEMIES);
                }

                Screen.sprites.Remove(counterpart);
                Screen.sprites.Remove(this);
                return;
            }

            if (local_timer % 2 == 1)
            {
                if (direction == Direction.UP)
                    y--;
                else if (direction == Direction.DOWN)
                    y++;
                else if (direction == Direction.LEFT)
                    x--;
                else
                    x++;
            }

            local_timer++;
            counterpart.x = x + 8;
            counterpart.y = y;
        }
    }

    internal class PowerBraceletSprite : ItemDropSprite
    {
        public PowerBraceletSprite(int x, int y) : base(x, y, false)
        {
            tile_index = 0x4e;
            palette_index = 6;
            return;
        }

        protected override void ItemSpecificActions()
        {
            if (collected)
            {
                SaveLoad.power_bracelet = true;
                Screen.sprites.Remove(this);
            }
        }
    }
}