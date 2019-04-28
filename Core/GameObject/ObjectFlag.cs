using System;

namespace SpicyTemple.Core.GameObject
{
    [Flags]
    public enum ObjectFlag : uint
    {
        DESTROYED = 1,
        OFF = 2,
        FLAT = 4,
        TEXT = 8,
        SEE_THROUGH = 0x10,
        SHOOT_THROUGH = 0x20,
        TRANSLUCENT = 0x40,
        SHRUNK = 0x80,
        DONTDRAW = 0x100,
        INVISIBLE = 0x200,
        NO_BLOCK = 0x400,
        CLICK_THROUGH = 0x800,
        INVENTORY = 0x1000,
        DYNAMIC = 0x2000,
        PROVIDES_COVER = 0x4000,
        RANDOM_SIZE = 0x8000,
        NOHEIGHT = 0x10000,
        WADING = 0x20000,
        UNUSED_40000 = 0x40000,
        STONED = 0x80000,
        DONTLIGHT = 0x100000,
        TEXT_FLOATER = 0x200000,
        INVULNERABLE = 0x400000,
        EXTINCT = 0x800000,
        TRAP_PC = 0x1000000,
        TRAP_SPOTTED = 0x2000000,
        DISALLOW_WADING = 0x4000000,
        UNUSED_08000000 = 0x8000000,
        HEIGHT_SET = 0x10000000,
        ANIMATED_DEAD = 0x20000000,
        TELEPORTED = 0x40000000,
        RADIUS_SET = 0x80000000
    }
}