using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace MP2RandoAssist
{
    public partial class Form1 : Form
    {
        bool IsLoadingSettings = false;

        #region Dolphin Instances and Checks
        Process dolphin;
        long RAMBaseAddr;
        bool Is32BitProcess;
        bool Exiting = false;
		bool WasInSaveStation = false;
        #endregion

        #region Auto Refill Timestamps
        internal long AutoRefill_Missiles_LastTime = 0;
        internal long AutoRefill_PowerBombs_LastTime = 0;
        internal long AutoRefill_Health_LastTime = 0;
        #endregion

        #region Constants
        // File address to exe address + 0x31FE
		internal const long OFF_CGAMESTATE_PAL = 0x3C6D88;
        internal const long OFF_CGAMESTATE_NTSC = 0x3C5B68;
		internal const long GCBaseRamAddr = 0x80000000;
        internal const long OFF_CPLAYER_PAL = 0x3CB960;
        internal const long OFF_CPLAYER_NTSC = 0x3CA740;
        internal const long OFF_CPLAYERBOMB = 0xEBC;
        internal const long OFF_CPLAYERBOMB_MBB_COUNT = 0x78B;
        internal const long OFF_CSTATEMANAGER_PAL = 0x3DC900;
        internal const long OFF_CSTATEMANAGER_NTSC = 0x3DB6E0;
        internal const long OFF_CWORLD = 0x8D0;
        internal const long OFF_CPLAYERSTATE = 0x150C;
        internal const long OFF_ROOM_ID = 0x88;
        internal const long OFF_WORLD_ID = 0x6C;
        internal const long OFF_ETANK_HEALTH_CAPACITY_NTSC = 0x41abe0;
        internal const long OFF_BASE_HEALTH_CAPACITY_NTSC = 0x41abe4;
        internal const long OFF_ETANK_HEALTH_CAPACITY_PAL = 0x41bed8;
        internal const long OFF_BASE_HEALTH_CAPACITY_PAL = 0x41bedc;
        internal const long OFF_UNCHARGED_AMMO_COST_NTSC = 0x3aa8c8;
        internal const long OFF_CHARGED_AMMO_COST_NTSC = 0x3aa8d8;
        internal const long OFF_CHARGE_COMBO_COST_NTSC = 0x3aa8e8;
        internal const long OFF_CHARGE_COMBO_MISSILE_COST_NTSC = 0x3a74ac;
        internal const long OFF_BEAM_TYPE_COST_NTSC = 0x1cccb0;
        internal const long OFF_UNCHARGED_AMMO_COST_PAL = 0x3abc28;
        internal const long OFF_CHARGED_AMMO_COST_PAL = 0x3abc38;
        internal const long OFF_CHARGE_COMBO_COST_PAL = 0x3abc48;
        internal const long OFF_CHARGE_COMBO_MISSILE_COST_PAL = 0x3a7c04;
        internal const long OFF_BEAM_TYPE_COST_PAL = 0x1ccfe4;
        internal const long AUTOREFILL_DELAY_IN_SEC = 2;
        internal const long AUTOREFILL_DELAY = AUTOREFILL_DELAY_IN_SEC * 1000;

        internal const long OFF_GROUND_SIDEWAYS_SPEEDCAP_PAL = 0x736F8C;
        internal const long OFF_MIDAIR_SIDEWAYS_SPEEDCAP_PAL = 0x736F90;
        internal const long OFF_LOCK_SPEEDCAP_PAL = 0x3AB304;
        internal const long OFF_GROUND_SIDEWAYS_SPEEDCAP_NTSC = 0x72E8CC;
        internal const long OFF_MIDAIR_SIDEWAYS_SPEEDCAP_NTSC = 0x72E8D0;
        internal const long OFF_LOCK_SPEEDCAP_NTSC = 0x3A9FB4;
        internal const long OFF_MORPHBALLBOMBS_COUNT_PAL = 0xB9006B;
        internal const long OFF_MORPHBALLBOMBS_COUNT_NTSC = 0xB3AE8B;
        internal const long OFF_HEALTH = 0x14;
        internal const long OFF_DARKBEAM_OBTAINED = 0x6F;
        internal const long OFF_LIGHTBEAM_OBTAINED = 0x7B;
        internal const long OFF_ANNIHILATORBEAM_OBTAINED = 0x87;
        internal const long OFF_SUPERMISSILE_OBTAINED = 0x93;
        internal const long OFF_DARKBURST_OBTAINED = 0x9F;
        internal const long OFF_SUNBURST_OBTAINED = 0xAB;
        internal const long OFF_SONICBOOM_OBTAINED = 0xB7;
        internal const long OFF_SCANVISOR_OBTAINED = 0xCF;
        internal const long OFF_DARKVISOR_OBTAINED = 0xDB;
        internal const long OFF_ECHOVISOR_OBTAINED = 0xE7;
        internal const long OFF_VARIASUIT_OBTAINED = 0xEF;
        internal const long OFF_DARKSUIT_OBTAINED = 0xFF;
        internal const long OFF_LIGHTSUIT_OBTAINED = 0x10B;
        internal const long OFF_MORPHBALL_OBTAINED = 0x117;
        internal const long OFF_BOOSTBALL_OBTAINED = 0x11F;
        internal const long OFF_SPIDERBALL_OBTAINED = 0x12F;
        internal const long OFF_MORPHBALLBOMBS_OBTAINED = 0x137;
        internal const long OFF_CHARGEBEAM_OBTAINED = 0x16B;
        internal const long OFF_GRAPPLEBEAM_OBTAINED = 0x177;
        internal const long OFF_SPACEBOOTS_OBTAINED = 0x183;
        internal const long OFF_GRAVITYBOOST_OBTAINED = 0x18B;
        internal const long OFF_SEEKERMISSILE_OBTAINED = 0x19B;
        internal const long OFF_SCREWATTACK_OBTAINED = 0x1A7;
        internal const long OFF_SKY_TEMPLE_KEY_1_OBTAINED = 0x1BF;
        internal const long OFF_SKY_TEMPLE_KEY_2_OBTAINED = OFF_SKY_TEMPLE_KEY_1_OBTAINED + 0x0C;
        internal const long OFF_SKY_TEMPLE_KEY_3_OBTAINED = OFF_SKY_TEMPLE_KEY_2_OBTAINED + 0x0C;
        internal const long OFF_DARK_AGON_KEY_1_OBTAINED = 0x1E3;
        internal const long OFF_DARK_AGON_KEY_2_OBTAINED = OFF_DARK_AGON_KEY_1_OBTAINED +0x0C;
        internal const long OFF_DARK_AGON_KEY_3_OBTAINED = OFF_DARK_AGON_KEY_2_OBTAINED + 0x0C;
        internal const long OFF_DARK_TORVUS_KEY_1_OBTAINED = OFF_DARK_AGON_KEY_3_OBTAINED + 0x0C;
        internal const long OFF_DARK_TORVUS_KEY_2_OBTAINED = OFF_DARK_TORVUS_KEY_1_OBTAINED + 0x0C;
        internal const long OFF_DARK_TORVUS_KEY_3_OBTAINED = OFF_DARK_TORVUS_KEY_2_OBTAINED + 0x0C;
        internal const long OFF_ING_HIVE_KEY_1_OBTAINED = OFF_DARK_TORVUS_KEY_3_OBTAINED + 0x0C;
        internal const long OFF_ING_HIVE_KEY_2_OBTAINED = OFF_ING_HIVE_KEY_1_OBTAINED + 0x0C;
        internal const long OFF_ING_HIVE_KEY_3_OBTAINED = OFF_ING_HIVE_KEY_2_OBTAINED + 0x0C;
        internal const long OFF_ENERGYTANKS = 0x257;
        internal const long OFF_MAX_ENERGYTANKS = OFF_ENERGYTANKS + 4;
        internal const long OFF_POWERBOMBS = 0x263;
        internal const long OFF_MAX_POWERBOMBS = OFF_POWERBOMBS + 4;
        internal const long OFF_MISSILES = 0x26F;
        internal const long OFF_MAX_MISSILES = OFF_MISSILES + 4;
        internal const long OFF_DARKBEAM_AMMO = 0x27B;
        internal const long OFF_MAX_DARKBEAM_AMMO = OFF_DARKBEAM_AMMO + 4;
        internal const long OFF_LIGHTBEAM_AMMO = 0x287;
        internal const long OFF_MAX_LIGHTBEAM_AMMO = OFF_LIGHTBEAM_AMMO + 4;
        internal const long OFF_VIOLET_TRANSLATOR_OBTAINED = 0x4EF;
        internal const long OFF_AMBER_TRANSLATOR_OBTAINED = 0x4FB;
        internal const long OFF_EMERALD_TRANSLATOR_OBTAINED = 0x507;
        internal const long OFF_COBALT_TRANSLATOR_OBTAINED = 0x513;
        internal const long OFF_SKY_TEMPLE_KEY_4_OBTAINED = 0x51F;
        internal const long OFF_SKY_TEMPLE_KEY_5_OBTAINED = OFF_SKY_TEMPLE_KEY_4_OBTAINED + 0x0C;
        internal const long OFF_SKY_TEMPLE_KEY_6_OBTAINED = OFF_SKY_TEMPLE_KEY_5_OBTAINED + 0x0C;
        internal const long OFF_SKY_TEMPLE_KEY_7_OBTAINED = OFF_SKY_TEMPLE_KEY_6_OBTAINED + 0x0C;
        internal const long OFF_SKY_TEMPLE_KEY_8_OBTAINED = OFF_SKY_TEMPLE_KEY_7_OBTAINED + 0x0C;
        internal const long OFF_SKY_TEMPLE_KEY_9_OBTAINED = OFF_SKY_TEMPLE_KEY_8_OBTAINED + 0x0C;
        internal const long OFF_ENERGY_TRANSFER_MODULE_OBTAINED = 0x567;
        #endregion

        #region C Imports
        public enum AllocationProtectEnum : uint
        {
            PAGE_EXECUTE = 0x00000010,
            PAGE_EXECUTE_READ = 0x00000020,
            PAGE_EXECUTE_READWRITE = 0x00000040,
            PAGE_EXECUTE_WRITECOPY = 0x00000080,
            PAGE_NOACCESS = 0x00000001,
            PAGE_READONLY = 0x00000002,
            PAGE_READWRITE = 0x00000004,
            PAGE_WRITECOPY = 0x00000008,
            PAGE_GUARD = 0x00000100,
            PAGE_NOCACHE = 0x00000200,
            PAGE_WRITECOMBINE = 0x00000400
        }

        public enum StateEnum : uint
        {
            MEM_COMMIT = 0x1000,
            MEM_FREE = 0x10000,
            MEM_RESERVE = 0x2000
        }

        public enum TypeEnum : uint
        {
            MEM_IMAGE = 0x1000000,
            MEM_MAPPED = 0x40000,
            MEM_PRIVATE = 0x20000
        }

        public struct MEMORY_BASIC_INFORMATION
        {
            public IntPtr BaseAddress;
            public IntPtr AllocationBase;
            public AllocationProtectEnum AllocationProtect;
            public IntPtr RegionSize;
            public StateEnum State;
            public AllocationProtectEnum Protect;
            public TypeEnum Type;
        }

        [DllImport("kernel32.dll")]
        static extern int VirtualQueryEx(IntPtr hProcess, IntPtr lpAddress, out MEMORY_BASIC_INFORMATION lpBuffer, uint dwLength);
        #endregion

        #region Dolphin
        public String Game_Code
        {
            get
            {
                return Encoding.ASCII.GetString(MemoryUtils.Read(this.dolphin, this.RAMBaseAddr, 6)).Trim('\0');
            }
        }
        #endregion

        #region Metroid Prime 2 Echoes
        internal bool IsPAL
        {
            get
            {
                if (MemoryUtils.ReadString(this.dolphin, this.RAMBaseAddr + 0x3ad710) == "!#$MetroidBuildInfo!#$Build v1.035 10/27/2004 19:48:17")
                    return true;
                return false;
            }
        }
		internal uint CurrentWorld
        {
            get
            {
                long WorldOffset = this.GetWorldOffset();
                if (WorldOffset == -1)
                    return UInt32.MaxValue;
                return MemoryUtils.ReadUInt32BE(this.dolphin, this.RAMBaseAddr + WorldOffset + OFF_WORLD_ID);
            }
        }

        internal uint CurrentRoom
        {
            get
            {
                long WorldOffset = this.GetWorldOffset();
                if (WorldOffset == -1)
                    return UInt32.MaxValue;
                return MemoryUtils.ReadUInt32BE(this.dolphin, this.RAMBaseAddr + WorldOffset + OFF_ROOM_ID);
            }
        }

        internal bool IsInSaveStationRoom
        {
            get
            {
                if (CurrentWorld == 0x0A) // Impact Crater
                {
                    return CurrentRoom == 0x00;   // Entrance
                }
                else if (CurrentWorld == 0x11) // Magmoor Caverns
                {
                    return CurrentRoom == 0x03 || // Save Station Magmoor A
                           CurrentRoom == 0x1C;   // Save Station Magmoor B
                }
                else if (CurrentWorld == 0x13) // Phazon Mines
                {
                    return CurrentRoom == 0x04 || // Save Station Mines A
                           CurrentRoom == 0x1E || // Save Station Mines B
                           CurrentRoom == 0x22;   // Save Station Mines C
                }
                else if (CurrentWorld == 0x18) // Chozo Ruins
                {
                    return CurrentRoom == 0x16 || // Save Station 1
                           CurrentRoom == 0x27 || // Save Station 2
                           CurrentRoom == 0x3B;   // Save Station 3
                }
                else if (CurrentWorld == 0x19) // Tallon Overworld
                {
                    return CurrentRoom == 0x00 || // Landing Site
                           CurrentRoom == 0x1C;   // Save Station in Crashed Frigate
                }
                else if (CurrentWorld == 0x1B) // Phendrana Drifts
                {
                    return CurrentRoom == 0x04 || // Save Station B
                           CurrentRoom == 0x11 || // Save Station A
                           CurrentRoom == 0x21 || // Save Station D
                           CurrentRoom == 0x2D;   // Save Station C
                }

                return false;
            }
        }

        internal bool IsPlayerIngame
        {
            get
            {
                return GetInventoryOffset() != -1;
            }
        }

        internal ushort Health
        {
            get {
                long InventoryOffset = GetInventoryOffset();
                if (InventoryOffset == -1)
                    return 0;
                return (ushort)MemoryUtils.ReadFloat32(this.dolphin, this.RAMBaseAddr + InventoryOffset + OFF_HEALTH);
            }
            set {
                long InventoryOffset = GetInventoryOffset();
                if (InventoryOffset == -1)
                    return;
                MemoryUtils.WriteFloat32(this.dolphin, this.RAMBaseAddr + InventoryOffset + OFF_HEALTH, (float)value);
            }
        }

        internal ushort MaxHealth
        {
            get
            {
                long InventoryOffset = GetInventoryOffset();
                if (InventoryOffset == -1)
                    return 0;
                ushort ETankCount = (ushort)MemoryUtils.ReadUInt32(this.dolphin, this.RAMBaseAddr + InventoryOffset + OFF_MAX_ENERGYTANKS);
                ushort ETankHealthPerUnit = (ushort)MemoryUtils.ReadFloat32(this.dolphin, this.RAMBaseAddr + (IsPAL ? OFF_ETANK_HEALTH_CAPACITY_PAL : OFF_ETANK_HEALTH_CAPACITY_NTSC));
                ushort BaseHealth = (ushort)MemoryUtils.ReadFloat32(this.dolphin, this.RAMBaseAddr + (IsPAL ? OFF_BASE_HEALTH_CAPACITY_PAL : OFF_BASE_HEALTH_CAPACITY_NTSC));
                return (ushort)(ETankCount * ETankHealthPerUnit + BaseHealth);
            }
        }

        internal int MorphBallBombs
        {
            get
            {
                if (CPlayerBomb == -1)
                    return 0;
                return MemoryUtils.ReadUInt8(this.dolphin, this.RAMBaseAddr + CPlayerBomb + OFF_CPLAYERBOMB_MBB_COUNT);
            }

            set
            {
                if (CPlayerBomb == -1)
                    return;
                MemoryUtils.WriteUInt8(this.dolphin, this.RAMBaseAddr + CPlayerBomb + OFF_CPLAYERBOMB_MBB_COUNT, (byte)value);
            }
        }

        internal int MaxMorphBallBombs
        {
            get
            {
                return 3;
            }
        }

        internal bool HaveLightBeam
        {
            get
            {
                long InventoryOffset = GetInventoryOffset();
                if (InventoryOffset == -1)
                    return false;
                return MemoryUtils.ReadUInt8(this.dolphin, this.RAMBaseAddr + InventoryOffset + OFF_LIGHTBEAM_OBTAINED) > 0;
            }
            set
            {
                long InventoryOffset = GetInventoryOffset();
                if (InventoryOffset == -1)
                    return;
                MemoryUtils.WriteUInt8(this.dolphin, this.RAMBaseAddr + InventoryOffset + OFF_LIGHTBEAM_OBTAINED, (byte)(value ? 1 : 0));
            }
        }

        internal bool HaveDarkBeam
        {
            get
            {
                long InventoryOffset = GetInventoryOffset();
                if (InventoryOffset == -1)
                    return false;
                return MemoryUtils.ReadUInt8(this.dolphin, this.RAMBaseAddr + InventoryOffset + OFF_DARKBEAM_OBTAINED) > 0;
            }
            set
            {
                long InventoryOffset = GetInventoryOffset();
                if (InventoryOffset == -1)
                    return;
                MemoryUtils.WriteUInt8(this.dolphin, this.RAMBaseAddr + InventoryOffset + OFF_DARKBEAM_OBTAINED, (byte)(value ? 1 : 0));
            }
        }

        internal bool HaveAnnihilatorBeam
        {
            get
            {
                long InventoryOffset = GetInventoryOffset();
                if (InventoryOffset == -1)
                    return false;
                return MemoryUtils.ReadUInt8(this.dolphin, this.RAMBaseAddr + InventoryOffset + OFF_ANNIHILATORBEAM_OBTAINED) > 0;
            }
            set
            {
                long InventoryOffset = GetInventoryOffset();
                if (InventoryOffset == -1)
                    return;
                MemoryUtils.WriteUInt8(this.dolphin, this.RAMBaseAddr + InventoryOffset + OFF_ANNIHILATORBEAM_OBTAINED, (byte)(value ? 1 : 0));
            }
        }

        internal bool HaveSuperMissile
        {
            get
            {
                long InventoryOffset = GetInventoryOffset();
                if (InventoryOffset == -1)
                    return false;
                return MemoryUtils.ReadUInt8(this.dolphin, this.RAMBaseAddr + InventoryOffset + OFF_SUPERMISSILE_OBTAINED) > 0;
            }
            set
            {
                long InventoryOffset = GetInventoryOffset();
                if (InventoryOffset == -1)
                    return;
                MemoryUtils.WriteUInt8(this.dolphin, this.RAMBaseAddr + InventoryOffset + OFF_SUPERMISSILE_OBTAINED, (byte)(value ? 1 : 0));
            }
        }

        internal bool HaveDarkBurst
        {
            get
            {
                long InventoryOffset = GetInventoryOffset();
                if (InventoryOffset == -1)
                    return false;
                return MemoryUtils.ReadUInt8(this.dolphin, this.RAMBaseAddr + InventoryOffset + OFF_DARKBURST_OBTAINED) > 0;
            }
            set
            {
                long InventoryOffset = GetInventoryOffset();
                if (InventoryOffset == -1)
                    return;
                MemoryUtils.WriteUInt8(this.dolphin, this.RAMBaseAddr + InventoryOffset + OFF_DARKBURST_OBTAINED, (byte)(value ? 1 : 0));
            }
        }

        internal bool HaveSunBurst
        {
            get
            {
                long InventoryOffset = GetInventoryOffset();
                if (InventoryOffset == -1)
                    return false;
                return MemoryUtils.ReadUInt8(this.dolphin, this.RAMBaseAddr + InventoryOffset + OFF_SUNBURST_OBTAINED) > 0;
            }
            set
            {
                long InventoryOffset = GetInventoryOffset();
                if (InventoryOffset == -1)
                    return;
                MemoryUtils.WriteUInt8(this.dolphin, this.RAMBaseAddr + InventoryOffset + OFF_SUNBURST_OBTAINED, (byte)(value ? 1 : 0));
            }
        }

        internal bool HaveSonicBoom
        {
            get
            {
                long InventoryOffset = GetInventoryOffset();
                if (InventoryOffset == -1)
                    return false;
                return MemoryUtils.ReadUInt8(this.dolphin, this.RAMBaseAddr + InventoryOffset + OFF_SONICBOOM_OBTAINED) > 0;
            }
            set
            {
                long InventoryOffset = GetInventoryOffset();
                if (InventoryOffset == -1)
                    return;
                MemoryUtils.WriteUInt8(this.dolphin, this.RAMBaseAddr + InventoryOffset + OFF_SONICBOOM_OBTAINED, (byte)(value ? 1 : 0));
            }
        }

        internal bool HaveScanVisor
        {
            get
            {
                long InventoryOffset = GetInventoryOffset();
                if (InventoryOffset == -1)
                    return false;
                return MemoryUtils.ReadUInt8(this.dolphin, this.RAMBaseAddr + InventoryOffset + OFF_SCANVISOR_OBTAINED) > 0;
            }
            set
            {
                long InventoryOffset = GetInventoryOffset();
                if (InventoryOffset == -1)
                    return;
                MemoryUtils.WriteUInt8(this.dolphin, this.RAMBaseAddr + InventoryOffset + OFF_SCANVISOR_OBTAINED, (byte)(value ? 1 : 0));
            }
        }

        internal bool HaveDarkVisor
        {
            get
            {
                long InventoryOffset = GetInventoryOffset();
                if (InventoryOffset == -1)
                    return false;
                return MemoryUtils.ReadUInt8(this.dolphin, this.RAMBaseAddr + InventoryOffset + OFF_DARKVISOR_OBTAINED) > 0;
            }
            set
            {
                long InventoryOffset = GetInventoryOffset();
                if (InventoryOffset == -1)
                    return;
                MemoryUtils.WriteUInt8(this.dolphin, this.RAMBaseAddr + InventoryOffset + OFF_DARKVISOR_OBTAINED, (byte)(value ? 1 : 0));
            }
        }

        internal bool HaveEchoVisor
        {
            get
            {
                long InventoryOffset = GetInventoryOffset();
                if (InventoryOffset == -1)
                    return false;
                return MemoryUtils.ReadUInt8(this.dolphin, this.RAMBaseAddr + InventoryOffset + OFF_ECHOVISOR_OBTAINED) > 0;
            }
            set
            {
                long InventoryOffset = GetInventoryOffset();
                if (InventoryOffset == -1)
                    return;
                MemoryUtils.WriteUInt8(this.dolphin, this.RAMBaseAddr + InventoryOffset + OFF_ECHOVISOR_OBTAINED, (byte)(value ? 1 : 0));
            }
        }

        internal bool HaveDarkSuit
        {
            get
            {
                long InventoryOffset = GetInventoryOffset();
                if (InventoryOffset == -1)
                    return false;
                return MemoryUtils.ReadUInt8(this.dolphin, this.RAMBaseAddr + InventoryOffset + OFF_DARKSUIT_OBTAINED) > 0;
            }
            set
            {
                long InventoryOffset = GetInventoryOffset();
                if (InventoryOffset == -1)
                    return;
                MemoryUtils.WriteUInt8(this.dolphin, this.RAMBaseAddr + InventoryOffset + OFF_DARKSUIT_OBTAINED, (byte)(value ? 1 : 0));
            }
        }

        internal bool HaveLightSuit
        {
            get
            {
                long InventoryOffset = GetInventoryOffset();
                if (InventoryOffset == -1)
                    return false;
                return MemoryUtils.ReadUInt8(this.dolphin, this.RAMBaseAddr + InventoryOffset + OFF_LIGHTSUIT_OBTAINED) > 0;
            }
            set
            {
                long InventoryOffset = GetInventoryOffset();
                if (InventoryOffset == -1)
                    return;
                MemoryUtils.WriteUInt8(this.dolphin, this.RAMBaseAddr + InventoryOffset + OFF_LIGHTSUIT_OBTAINED, (byte)(value ? 1 : 0));
            }
        }

        internal bool HaveBoostBall
        {
            get
            {
                long InventoryOffset = GetInventoryOffset();
                if (InventoryOffset == -1)
                    return false;
                return MemoryUtils.ReadUInt8(this.dolphin, this.RAMBaseAddr + InventoryOffset + OFF_BOOSTBALL_OBTAINED) > 0;
            }
            set
            {
                long InventoryOffset = GetInventoryOffset();
                if (InventoryOffset == -1)
                    return;
                MemoryUtils.WriteUInt8(this.dolphin, this.RAMBaseAddr + InventoryOffset + OFF_BOOSTBALL_OBTAINED, (byte)(value ? 1 : 0));
            }
        }

        internal bool HaveSpiderBall
        {
            get
            {
                long InventoryOffset = GetInventoryOffset();
                if (InventoryOffset == -1)
                    return false;
                return MemoryUtils.ReadUInt8(this.dolphin, this.RAMBaseAddr + InventoryOffset + OFF_SPIDERBALL_OBTAINED) > 0;
            }
            set
            {
                long InventoryOffset = GetInventoryOffset();
                if (InventoryOffset == -1)
                    return;
                MemoryUtils.WriteUInt8(this.dolphin, this.RAMBaseAddr + InventoryOffset + OFF_SPIDERBALL_OBTAINED, (byte)(value ? 1 : 0));
            }
        }

        internal bool HaveMorphBall
        {
            get
            {
                long InventoryOffset = GetInventoryOffset();
                if (InventoryOffset == -1)
                    return false;
                return MemoryUtils.ReadUInt8(this.dolphin, this.RAMBaseAddr + InventoryOffset + OFF_MORPHBALL_OBTAINED) > 0;
            }
            set
            {
                long InventoryOffset = GetInventoryOffset();
                if (InventoryOffset == -1)
                    return;
                MemoryUtils.WriteUInt8(this.dolphin, this.RAMBaseAddr + InventoryOffset + OFF_MORPHBALL_OBTAINED, (byte)(value ? 1 : 0));
            }
        }

        internal bool HaveMorphBallBombs
        {
            get
            {
                long InventoryOffset = GetInventoryOffset();
                if (InventoryOffset == -1)
                    return false;
                return MemoryUtils.ReadUInt8(this.dolphin, this.RAMBaseAddr + InventoryOffset + OFF_MORPHBALLBOMBS_OBTAINED) > 0;
            }
            set
            {
                long InventoryOffset = GetInventoryOffset();
                if (InventoryOffset == -1)
                    return;
                MemoryUtils.WriteUInt8(this.dolphin, this.RAMBaseAddr + InventoryOffset + OFF_MORPHBALLBOMBS_OBTAINED, (byte)(value ? 1 : 0));
            }
        }

        internal bool HaveChargeBeam
        {
            get
            {
                long InventoryOffset = GetInventoryOffset();
                if (InventoryOffset == -1)
                    return false;
                return MemoryUtils.ReadUInt8(this.dolphin, this.RAMBaseAddr + InventoryOffset + OFF_CHARGEBEAM_OBTAINED) > 0;
            }
            set
            {
                long InventoryOffset = GetInventoryOffset();
                if (InventoryOffset == -1)
                    return;
                MemoryUtils.WriteUInt8(this.dolphin, this.RAMBaseAddr + InventoryOffset + OFF_CHARGEBEAM_OBTAINED, (byte)(value ? 1 : 0));
            }
        }

        internal bool HaveGrappleBeam
        {
            get
            {
                long InventoryOffset = GetInventoryOffset();
                if (InventoryOffset == -1)
                    return false;
                return MemoryUtils.ReadUInt8(this.dolphin, this.RAMBaseAddr + InventoryOffset + OFF_GRAPPLEBEAM_OBTAINED) > 0;
            }
            set
            {
                long InventoryOffset = GetInventoryOffset();
                if (InventoryOffset == -1)
                    return;
                MemoryUtils.WriteUInt8(this.dolphin, this.RAMBaseAddr + InventoryOffset + OFF_GRAPPLEBEAM_OBTAINED, (byte)(value ? 1 : 0));
            }
        }

        internal bool HaveSpaceJumpBoots
        {
            get
            {
                long InventoryOffset = GetInventoryOffset();
                if (InventoryOffset == -1)
                    return false;
                return MemoryUtils.ReadUInt8(this.dolphin, this.RAMBaseAddr + InventoryOffset + OFF_SPACEBOOTS_OBTAINED) > 0;
            }
            set
            {
                long InventoryOffset = GetInventoryOffset();
                if (InventoryOffset == -1)
                    return;
                MemoryUtils.WriteUInt8(this.dolphin, this.RAMBaseAddr + InventoryOffset + OFF_SPACEBOOTS_OBTAINED, (byte)(value ? 1 : 0));
            }
        }

        internal bool HaveGravityBoost
        {
            get
            {
                long InventoryOffset = GetInventoryOffset();
                if (InventoryOffset == -1)
                    return false;
                return MemoryUtils.ReadUInt8(this.dolphin, this.RAMBaseAddr + InventoryOffset + OFF_GRAVITYBOOST_OBTAINED) > 0;
            }
            set
            {
                long InventoryOffset = GetInventoryOffset();
                if (InventoryOffset == -1)
                    return;
                MemoryUtils.WriteUInt8(this.dolphin, this.RAMBaseAddr + InventoryOffset + OFF_GRAVITYBOOST_OBTAINED, (byte)(value ? 1 : 0));
            }
        }

        internal bool HaveScrewAttack
        {
            get
            {
                long InventoryOffset = GetInventoryOffset();
                if (InventoryOffset == -1)
                    return false;
                return MemoryUtils.ReadUInt8(this.dolphin, this.RAMBaseAddr + InventoryOffset + OFF_SCREWATTACK_OBTAINED) > 0;
            }
            set
            {
                long InventoryOffset = GetInventoryOffset();
                if (InventoryOffset == -1)
                    return;
                MemoryUtils.WriteUInt8(this.dolphin, this.RAMBaseAddr + InventoryOffset + OFF_SCREWATTACK_OBTAINED, (byte)(value ? 1 : 0));
            }
        }

        internal bool HaveSeekerLauncher
        {
            get
            {
                long InventoryOffset = GetInventoryOffset();
                if (InventoryOffset == -1)
                    return false;
                return MemoryUtils.ReadUInt8(this.dolphin, this.RAMBaseAddr + InventoryOffset + OFF_SEEKERMISSILE_OBTAINED) > 0;
            }
            set
            {
                long InventoryOffset = GetInventoryOffset();
                if (InventoryOffset == -1)
                    return;
                MemoryUtils.WriteUInt8(this.dolphin, this.RAMBaseAddr + InventoryOffset + OFF_SEEKERMISSILE_OBTAINED, (byte)(value ? 1 : 0));
            }
        }

        internal byte PowerBombs
        {
            get
            {
                long InventoryOffset = GetInventoryOffset();
                if (InventoryOffset == -1)
                    return 0;
                return MemoryUtils.ReadUInt8(this.dolphin, this.RAMBaseAddr + InventoryOffset + OFF_POWERBOMBS);
            }
            set
            {
                long InventoryOffset = GetInventoryOffset();
                if (InventoryOffset == -1)
                    return;
                MemoryUtils.WriteUInt8(this.dolphin, this.RAMBaseAddr + InventoryOffset + OFF_POWERBOMBS, value);
            }
        }

        internal byte MaxPowerBombs
        {
            get
            {
                long InventoryOffset = GetInventoryOffset();
                if (InventoryOffset == -1)
                    return 0;
                return MemoryUtils.ReadUInt8(this.dolphin, this.RAMBaseAddr + InventoryOffset + OFF_MAX_POWERBOMBS);
            }
            set
            {
                long InventoryOffset = GetInventoryOffset();
                if (InventoryOffset == -1)
                    return;
                MemoryUtils.WriteUInt8(this.dolphin, this.RAMBaseAddr + InventoryOffset + OFF_MAX_POWERBOMBS, value);
            }
        }

        internal ushort Missiles
        {
            get
            {
                long InventoryOffset = GetInventoryOffset();
                if (InventoryOffset == -1)
                    return 0;
                return MemoryUtils.ReadUInt16(this.dolphin, this.RAMBaseAddr + InventoryOffset + OFF_MISSILES);
            }
            set
            {
                long InventoryOffset = GetInventoryOffset();
                if (InventoryOffset == -1)
                    return;
                MemoryUtils.WriteUInt16(this.dolphin, this.RAMBaseAddr + InventoryOffset + OFF_MISSILES, value);
            }
        }

        internal ushort MaxMissiles
        {
            get
            {
                long InventoryOffset = GetInventoryOffset();
                if (InventoryOffset == -1)
                    return 0;
                return MemoryUtils.ReadUInt16(this.dolphin, this.RAMBaseAddr + InventoryOffset + OFF_MAX_MISSILES);
            }
            set
            {
                long InventoryOffset = GetInventoryOffset();
                if (InventoryOffset == -1)
                    return;
                MemoryUtils.WriteUInt16(this.dolphin, this.RAMBaseAddr + InventoryOffset + OFF_MAX_MISSILES, value);
            }
        }

        internal ushort DarkBeamAmmo
        {
            get
            {
                long InventoryOffset = GetInventoryOffset();
                if (InventoryOffset == -1)
                    return 0;
                return MemoryUtils.ReadUInt16(this.dolphin, this.RAMBaseAddr + InventoryOffset + OFF_DARKBEAM_AMMO);
            }
            set
            {
                long InventoryOffset = GetInventoryOffset();
                if (InventoryOffset == -1)
                    return;
                MemoryUtils.WriteUInt16(this.dolphin, this.RAMBaseAddr + InventoryOffset + OFF_DARKBEAM_AMMO, value);
            }
        }

        internal ushort MaxDarkBeamAmmo
        {
            get
            {
                long InventoryOffset = GetInventoryOffset();
                if (InventoryOffset == -1)
                    return 0;
                return MemoryUtils.ReadUInt16(this.dolphin, this.RAMBaseAddr + InventoryOffset + OFF_MAX_DARKBEAM_AMMO);
            }
            set
            {
                long InventoryOffset = GetInventoryOffset();
                if (InventoryOffset == -1)
                    return;
                MemoryUtils.WriteUInt16(this.dolphin, this.RAMBaseAddr + InventoryOffset + OFF_MAX_DARKBEAM_AMMO, value);
            }
        }

        internal ushort LightBeamAmmo
        {
            get
            {
                long InventoryOffset = GetInventoryOffset();
                if (InventoryOffset == -1)
                    return 0;
                return MemoryUtils.ReadUInt16(this.dolphin, this.RAMBaseAddr + InventoryOffset + OFF_LIGHTBEAM_AMMO);
            }
            set
            {
                long InventoryOffset = GetInventoryOffset();
                if (InventoryOffset == -1)
                    return;
                MemoryUtils.WriteUInt16(this.dolphin, this.RAMBaseAddr + InventoryOffset + OFF_LIGHTBEAM_AMMO, value);
            }
        }

        internal ushort MaxLightBeamAmmo
        {
            get
            {
                long InventoryOffset = GetInventoryOffset();
                if (InventoryOffset == -1)
                    return 0;
                return MemoryUtils.ReadUInt16(this.dolphin, this.RAMBaseAddr + InventoryOffset + OFF_MAX_LIGHTBEAM_AMMO);
            }
            set
            {
                long InventoryOffset = GetInventoryOffset();
                if (InventoryOffset == -1)
                    return;
                MemoryUtils.WriteUInt16(this.dolphin, this.RAMBaseAddr + InventoryOffset + OFF_MAX_LIGHTBEAM_AMMO, value);
            }
        }

        internal bool HaveVioletTranslator
        {
            get
            {
                long InventoryOffset = GetInventoryOffset();
                if (InventoryOffset == -1)
                    return false;
                return MemoryUtils.ReadUInt8(this.dolphin, this.RAMBaseAddr + InventoryOffset + OFF_VIOLET_TRANSLATOR_OBTAINED) > 0;
            }
            set
            {
                long InventoryOffset = GetInventoryOffset();
                if (InventoryOffset == -1)
                    return;
                MemoryUtils.WriteUInt8(this.dolphin, this.RAMBaseAddr + InventoryOffset + OFF_VIOLET_TRANSLATOR_OBTAINED, (byte)(value ? 1 : 0));
            }
        }

        internal bool HaveAmberTranslator
        {
            get
            {
                long InventoryOffset = GetInventoryOffset();
                if (InventoryOffset == -1)
                    return false;
                return MemoryUtils.ReadUInt8(this.dolphin, this.RAMBaseAddr + InventoryOffset + OFF_AMBER_TRANSLATOR_OBTAINED) > 0;
            }
            set
            {
                long InventoryOffset = GetInventoryOffset();
                if (InventoryOffset == -1)
                    return;
                MemoryUtils.WriteUInt8(this.dolphin, this.RAMBaseAddr + InventoryOffset + OFF_AMBER_TRANSLATOR_OBTAINED, (byte)(value ? 1 : 0));
            }
        }

        internal bool HaveEmeraldTranslator
        {
            get
            {
                long InventoryOffset = GetInventoryOffset();
                if (InventoryOffset == -1)
                    return false;
                return MemoryUtils.ReadUInt8(this.dolphin, this.RAMBaseAddr + InventoryOffset + OFF_EMERALD_TRANSLATOR_OBTAINED) > 0;
            }
            set
            {
                long InventoryOffset = GetInventoryOffset();
                if (InventoryOffset == -1)
                    return;
                MemoryUtils.WriteUInt8(this.dolphin, this.RAMBaseAddr + InventoryOffset + OFF_EMERALD_TRANSLATOR_OBTAINED, (byte)(value ? 1 : 0));
            }
        }

        internal bool HaveCobaltTranslator
        {
            get
            {
                long InventoryOffset = GetInventoryOffset();
                if (InventoryOffset == -1)
                    return false;
                return MemoryUtils.ReadUInt8(this.dolphin, this.RAMBaseAddr + InventoryOffset + OFF_COBALT_TRANSLATOR_OBTAINED) > 0;
            }
            set
            {
                long InventoryOffset = GetInventoryOffset();
                if (InventoryOffset == -1)
                    return;
                MemoryUtils.WriteUInt8(this.dolphin, this.RAMBaseAddr + InventoryOffset + OFF_COBALT_TRANSLATOR_OBTAINED, (byte)(value ? 1 : 0));
            }
        }

        internal bool HaveEnergyTransferModule
        {
            get
            {
                long InventoryOffset = GetInventoryOffset();
                if (InventoryOffset == -1)
                    return false;
                return MemoryUtils.ReadUInt8(this.dolphin, this.RAMBaseAddr + InventoryOffset + OFF_ENERGY_TRANSFER_MODULE_OBTAINED) > 0;
            }
            set
            {
                long InventoryOffset = GetInventoryOffset();
                if (InventoryOffset == -1)
                    return;
                MemoryUtils.WriteUInt8(this.dolphin, this.RAMBaseAddr + InventoryOffset + OFF_ENERGY_TRANSFER_MODULE_OBTAINED, (byte)(value ? 1 : 0));
            }
        }

        internal bool[] SkyTempleKeys()
        {
            List<bool> keys = new bool[] { false, false, false, false, false, false, false, false, false }.ToList();
            long InventoryOffset = GetInventoryOffset();
            if (InventoryOffset == -1)
                return keys.ToArray();
            keys[0] = MemoryUtils.ReadUInt8(this.dolphin, this.RAMBaseAddr + InventoryOffset + OFF_SKY_TEMPLE_KEY_1_OBTAINED) > 0;
            keys[1] = MemoryUtils.ReadUInt8(this.dolphin, this.RAMBaseAddr + InventoryOffset + OFF_SKY_TEMPLE_KEY_2_OBTAINED) > 0;
            keys[2] = MemoryUtils.ReadUInt8(this.dolphin, this.RAMBaseAddr + InventoryOffset + OFF_SKY_TEMPLE_KEY_3_OBTAINED) > 0;
            keys[3] = MemoryUtils.ReadUInt8(this.dolphin, this.RAMBaseAddr + InventoryOffset + OFF_SKY_TEMPLE_KEY_4_OBTAINED) > 0;
            keys[4] = MemoryUtils.ReadUInt8(this.dolphin, this.RAMBaseAddr + InventoryOffset + OFF_SKY_TEMPLE_KEY_5_OBTAINED) > 0;
            keys[5] = MemoryUtils.ReadUInt8(this.dolphin, this.RAMBaseAddr + InventoryOffset + OFF_SKY_TEMPLE_KEY_6_OBTAINED) > 0;
            keys[6] = MemoryUtils.ReadUInt8(this.dolphin, this.RAMBaseAddr + InventoryOffset + OFF_SKY_TEMPLE_KEY_7_OBTAINED) > 0;
            keys[7] = MemoryUtils.ReadUInt8(this.dolphin, this.RAMBaseAddr + InventoryOffset + OFF_SKY_TEMPLE_KEY_8_OBTAINED) > 0;
            keys[8] = MemoryUtils.ReadUInt8(this.dolphin, this.RAMBaseAddr + InventoryOffset + OFF_SKY_TEMPLE_KEY_9_OBTAINED) > 0;
            return keys.ToArray();
        }

        internal bool[] DarkAgonKeys()
        {
            List<bool> keys = new bool[] { false, false, false, false, false, false, false, false, false }.ToList();
            long InventoryOffset = GetInventoryOffset();
            if (InventoryOffset == -1)
                return keys.ToArray();
            keys[0] = MemoryUtils.ReadUInt8(this.dolphin, this.RAMBaseAddr + InventoryOffset + OFF_DARK_AGON_KEY_1_OBTAINED) > 0;
            keys[1] = MemoryUtils.ReadUInt8(this.dolphin, this.RAMBaseAddr + InventoryOffset + OFF_DARK_AGON_KEY_2_OBTAINED) > 0;
            keys[2] = MemoryUtils.ReadUInt8(this.dolphin, this.RAMBaseAddr + InventoryOffset + OFF_DARK_AGON_KEY_3_OBTAINED) > 0;
            return keys.ToArray();
        }

        internal bool[] DarkTorvusKeys()
        {
            List<bool> keys = new bool[] { false, false, false, false, false, false, false, false, false }.ToList();
            long InventoryOffset = GetInventoryOffset();
            if (InventoryOffset == -1)
                return keys.ToArray();
            keys[0] = MemoryUtils.ReadUInt8(this.dolphin, this.RAMBaseAddr + InventoryOffset + OFF_DARK_TORVUS_KEY_1_OBTAINED) > 0;
            keys[1] = MemoryUtils.ReadUInt8(this.dolphin, this.RAMBaseAddr + InventoryOffset + OFF_DARK_TORVUS_KEY_2_OBTAINED) > 0;
            keys[2] = MemoryUtils.ReadUInt8(this.dolphin, this.RAMBaseAddr + InventoryOffset + OFF_DARK_TORVUS_KEY_3_OBTAINED) > 0;
            return keys.ToArray();
        }

        internal bool[] IngHiveKeys()
        {
            List<bool> keys = new bool[] { false, false, false, false, false, false, false, false, false }.ToList();
            long InventoryOffset = GetInventoryOffset();
            if (InventoryOffset == -1)
                return keys.ToArray();
            keys[0] = MemoryUtils.ReadUInt8(this.dolphin, this.RAMBaseAddr + InventoryOffset + OFF_ING_HIVE_KEY_1_OBTAINED) > 0;
            keys[1] = MemoryUtils.ReadUInt8(this.dolphin, this.RAMBaseAddr + InventoryOffset + OFF_ING_HIVE_KEY_2_OBTAINED) > 0;
            keys[2] = MemoryUtils.ReadUInt8(this.dolphin, this.RAMBaseAddr + InventoryOffset + OFF_ING_HIVE_KEY_3_OBTAINED) > 0;
            return keys.ToArray();
        }

        internal bool AmmoSystem
        {
            set
            {
                long OFF_UNCHARGED_AMMO_COST = IsPAL ? OFF_UNCHARGED_AMMO_COST_PAL : OFF_UNCHARGED_AMMO_COST_NTSC;
                long OFF_CHARGED_AMMO_COST = IsPAL ? OFF_CHARGED_AMMO_COST_PAL : OFF_CHARGED_AMMO_COST_NTSC;
                long OFF_CHARGE_COMBO_COST = IsPAL ? OFF_CHARGE_COMBO_COST_PAL : OFF_CHARGE_COMBO_COST_NTSC;
                MemoryUtils.WriteUInt32BE(this.dolphin, this.RAMBaseAddr + OFF_UNCHARGED_AMMO_COST, 0); // Power Beam
                MemoryUtils.WriteUInt32BE(this.dolphin, this.RAMBaseAddr + OFF_UNCHARGED_AMMO_COST + 4, (uint)(value ? 1 : 0)); // Dark Beam
                MemoryUtils.WriteUInt32BE(this.dolphin, this.RAMBaseAddr + OFF_UNCHARGED_AMMO_COST + 8, (uint)(value ? 1 : 0)); // Light Beam
                MemoryUtils.WriteUInt32BE(this.dolphin, this.RAMBaseAddr + OFF_UNCHARGED_AMMO_COST + 12, (uint)(value ? 1 : 0)); // Annihilator Beam
                MemoryUtils.WriteUInt32BE(this.dolphin, this.RAMBaseAddr + OFF_CHARGED_AMMO_COST, 0); // Power Beam
                MemoryUtils.WriteUInt32BE(this.dolphin, this.RAMBaseAddr + OFF_CHARGED_AMMO_COST + 4, (uint)(value ? 5 : 0)); // Dark Beam
                MemoryUtils.WriteUInt32BE(this.dolphin, this.RAMBaseAddr + OFF_CHARGED_AMMO_COST + 8, (uint)(value ? 5 : 0)); // Light Beam
                MemoryUtils.WriteUInt32BE(this.dolphin, this.RAMBaseAddr + OFF_CHARGED_AMMO_COST + 12, (uint)(value ? 5 : 0)); // Annihilator Beam
                MemoryUtils.WriteUInt32BE(this.dolphin, this.RAMBaseAddr + OFF_CHARGE_COMBO_COST, 0); // Power Beam
                MemoryUtils.WriteUInt32BE(this.dolphin, this.RAMBaseAddr + OFF_CHARGE_COMBO_COST + 4, (uint)(value ? 30 : 0)); // Dark Beam
                MemoryUtils.WriteUInt32BE(this.dolphin, this.RAMBaseAddr + OFF_CHARGE_COMBO_COST + 8, (uint)(value ? 30 : 0)); // Light Beam
                MemoryUtils.WriteUInt32BE(this.dolphin, this.RAMBaseAddr + OFF_CHARGE_COMBO_COST + 12, (uint)(value ? 30 : 0)); // Annihilator Beam
            }
        }

        internal bool Prime1_Dash
        {
            set
            {
                long OFF_LOCK_SPEEDCAP = IsPAL ? OFF_LOCK_SPEEDCAP_PAL : OFF_LOCK_SPEEDCAP_NTSC;
                long OFF_MIDAIR_SIDEWAYS_SPEEDCAP = IsPAL ? OFF_MIDAIR_SIDEWAYS_SPEEDCAP_PAL : OFF_MIDAIR_SIDEWAYS_SPEEDCAP_NTSC;
                MemoryUtils.WriteFloat32(this.dolphin, this.RAMBaseAddr + OFF_LOCK_SPEEDCAP, value ? 40.0f : 18.0f);
                MemoryUtils.WriteFloat32(this.dolphin, this.RAMBaseAddr + OFF_MIDAIR_SIDEWAYS_SPEEDCAP, value ? 40.0f : 16.5f);
            }
        }
        #endregion

        public Form1()
        {
            InitializeComponent();
        }

        void LoadSettings()
        {
            if (!File.Exists("MP2RandoAssist.ini"))
                SaveSettings();
            using (var file = new StreamReader(File.OpenRead("MP2RandoAssist.ini")))
            {
                IsLoadingSettings = true;
                while (!file.EndOfStream)
                {
                    String line = file.ReadLine();
                    if (!line.Contains('='))
                        continue;
                    String[] setting = line.Split('=');
                    if (setting[0] == "DarkMode")
                        this.chkBoxDarkMode.Checked = setting[1] == "ON";
                }
                IsLoadingSettings = false;
            }
        }

        void SaveSettings()
        {
            using (var file = new StreamWriter(File.OpenWrite("MP2RandoAssist.ini")))
            {
                file.WriteLine("DarkMode=" + (this.chkBoxDarkMode.Checked ? "ON" : "OFF"));
            }
        }

        long GetCurTimeInMilliseconds()
        {
            return DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
        }

        bool IsValidChar(char c)
        {
            if (c >= 'A' && c <= 'Z')
                return true;
            if (c >= '0' && c <= '9')
                return true;
            return false;
        }

        long GetRAMBaseAddr()
        {
            long MaxAddress = Int64.MaxValue;
            long address = 0x8000000;
            do
            {
                MEMORY_BASIC_INFORMATION m;
                if(VirtualQueryEx(this.dolphin.Handle, (IntPtr)address, out m, (uint)Marshal.SizeOf(typeof(MEMORY_BASIC_INFORMATION)))==0)
                    break;
                String game_code = Encoding.ASCII.GetString(MemoryUtils.Read(this.dolphin, this.Is32BitProcess ? m.AllocationBase.ToInt32() : m.AllocationBase.ToInt64(), 8)).Trim('\0');
                bool game_code_check = ((game_code.Length - 6)*(game_code.Length-3)) <= 0;
                for (int i = 0; i < game_code.Length; i++) if (!IsValidChar(game_code[i])) game_code_check = false;
                if (m.Type == TypeEnum.MEM_MAPPED && m.AllocationProtect == AllocationProtectEnum.PAGE_READWRITE && m.State != StateEnum.MEM_FREE && m.RegionSize.ToInt64() > 0x20000 && game_code_check)
                {
                    if (this.Is32BitProcess)
                        return m.AllocationBase.ToInt32();
                    else
                        return m.AllocationBase.ToInt64();
                }
                if (address == (long)m.BaseAddress + (long)m.RegionSize)
                    break;
                address = (long)m.BaseAddress + (long)m.RegionSize;
            } while (address <= MaxAddress);
            return 0;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            LoadSettings();
            try
            {
                this.dolphin = Process.GetProcessesByName("dolphin").Length == 0 ? null : Process.GetProcessesByName("dolphin").First();
                if (dolphin == null)
                {
                    MessageBox.Show("Dolphin is not running!\r\nExiting...");
                    this.Close();
                    return;
                }
                this.Is32BitProcess = this.dolphin.MainModule.BaseAddress.ToInt64() < UInt32.MaxValue;
                this.RAMBaseAddr = GetRAMBaseAddr();
                if (this.RAMBaseAddr == 0)
                {
                    MessageBox.Show("Metroid Prime 2 Echoes is not running!\r\nExiting...");
                    this.Close();
                    return;
                }
                if (!Game_Code.StartsWith("G2M"))
                {
                    MessageBox.Show("Metroid Prime 2 Echoes is not running!\r\nExiting...");
                    this.Close();
                    return;
                }
                this.FormClosing += (s, ev) =>
                {
                    AmmoSystem = true;
                    Prime1_Dash = false;
                    this.Exiting = true;
                };
                this.timer1.Enabled = true;
            } catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
                this.Close();
            }
        }

        internal long CPlayer
        {
            get
            {
                long GC_CPlayer = MemoryUtils.ReadUInt32BE(this.dolphin, this.RAMBaseAddr + (IsPAL ? OFF_CPLAYER_PAL : OFF_CPLAYER_NTSC));
                if (GC_CPlayer > GCBaseRamAddr)
                    return GC_CPlayer - GCBaseRamAddr;
                return -1;
            }
        }

        internal long CPlayerBomb
        {
            get
            {
                if (CPlayer == -1)
                    return -1;
                long GC_CPlayerBomb = MemoryUtils.ReadUInt32BE(this.dolphin, this.RAMBaseAddr + CPlayer + OFF_CPLAYERBOMB);
                if (GC_CPlayerBomb > GCBaseRamAddr)
                    return GC_CPlayerBomb - GCBaseRamAddr;
                return -1;
            }
        }

        private long GetGameStateOffset()
        {
            long GC_CGameState = MemoryUtils.ReadUInt32BE(this.dolphin, this.RAMBaseAddr + (IsPAL ? OFF_CGAMESTATE_PAL : OFF_CGAMESTATE_NTSC) + 0x134);
            if (GC_CGameState > GCBaseRamAddr)
                return GC_CGameState - GCBaseRamAddr;
            return -1;
        }
		
		private long GetWorldOffset()
        {
            long GC_CWorld = MemoryUtils.ReadUInt32BE(this.dolphin, this.RAMBaseAddr + (IsPAL ? OFF_CSTATEMANAGER_PAL : OFF_CSTATEMANAGER_NTSC) + OFF_CWORLD);
            if (GC_CWorld > GCBaseRamAddr)
                return GC_CWorld - GCBaseRamAddr;
            return -1;
        }

        private long GetInventoryOffset()
        {
            long GC_CInventory = MemoryUtils.ReadUInt32BE(this.dolphin, this.RAMBaseAddr + (IsPAL ? OFF_CSTATEMANAGER_PAL : OFF_CSTATEMANAGER_NTSC) + OFF_CPLAYERSTATE);
            if (GC_CInventory > GCBaseRamAddr)
                return GC_CInventory - GCBaseRamAddr;
            return -1;
        }
		
		internal long IGT
        {
            get
            {
                long CGameState = GetGameStateOffset();
                if (CGameState == -1)
                    return -1;
                return (long)(MemoryUtils.ReadFloat64(this.dolphin, this.RAMBaseAddr + CGameState + 0x48) * 1000);
            }
        }

        internal string IGTAsStr
        {
            get
            {
                if (IGT == -1)
                    return "00:00:00.000";
                return String.Format("{0:00}:{1:00}:{2:00}.{3:000}", IGT / (60 * 60 * 1000), (IGT / (60 * 1000)) % 60, (IGT / 1000) % 60, IGT % 1000);
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (this.Exiting)
                return;
            try
            {
                if (!Game_Code.StartsWith("G2M"))
                {
                    new Thread(() => MessageBox.Show("Either Dolphin or the game is not running!\r\nExiting...")).Start();
                    this.Close();
                    this.Exiting = true;
                    return;
                }
                if (!IsPlayerIngame)
                    return;
				/*if (WasInSaveStation != IsInSaveStationRoom && IsInSaveStationRoom)
					Health = MaxHealth;
				WasInSaveStation = IsInSaveStationRoom;*/
                this.lblHealth.Text = "HP : " + Health + " / " + MaxHealth;
                if (Health < 30)
                    this.lblHealth.Text += " /!\\";
                this.lblMissilesAmmo.Text = "Missiles : " + Missiles + " / " + MaxMissiles;
                this.lblMorphBallBombsAmmo.Text = "Morph Ball Bombs : " + MorphBallBombs + " / " + MaxMorphBallBombs;
                this.lblPowerBombsAmmo.Text = "Power Bombs : " + PowerBombs + " / " + MaxPowerBombs;
                this.lblDarkBeamAmmo.Text = "Dark Beam : "+DarkBeamAmmo+" / "+MaxDarkBeamAmmo;
                this.lblLightBeamAmmo.Text = "Light Beam : " + LightBeamAmmo + " / " + MaxLightBeamAmmo;
                this.chkBoxSpaceJumpBoots.Checked = HaveSpaceJumpBoots;
                this.chkBoxGravityBoost.Checked = HaveGravityBoost;
                this.chkBoxScrewAttack.Checked = HaveScrewAttack;
                this.chkBoxScanVisor.Checked = HaveScanVisor;
                this.chkBoxEchoVisor.Checked = HaveEchoVisor;
                this.chkBoxDarkVisor.Checked = HaveDarkVisor;
                this.chkBoxMorphBall.Checked = HaveMorphBall;
                this.chkBoxMorphBallBombs.Checked = HaveMorphBallBombs;
                this.chkBoxPowerBombs.Checked = MaxPowerBombs > 0;
                this.chkBoxSpiderBall.Checked = HaveSpiderBall;
                this.chkBoxBoostBall.Checked = HaveBoostBall;
                this.chkBoxMissileLauncher.Checked = MaxMissiles > 0;
                this.chkBoxSeekerLauncher.Checked = HaveSeekerLauncher;
                this.chkBoxLightBeam.Checked = HaveLightBeam;
                this.chkBoxDarkBeam.Checked = HaveDarkBeam;
                this.chkBoxAnnihilatorBeam.Checked = HaveAnnihilatorBeam;
                this.chkBoxChargeBeam.Checked = HaveChargeBeam;
                this.chkBoxGrappleBeam.Checked = HaveGrappleBeam;
                this.chkBoxDarkSuit.Checked = HaveDarkSuit;
                this.chkBoxLightSuit.Checked = HaveLightSuit;
                this.chkBoxSuperMissile.Checked = HaveSuperMissile;
                this.chkBoxDarkBurst.Checked = HaveDarkBurst;
                this.chkBoxSunBurst.Checked = HaveSunBurst;
                this.chkBoxSonicBoom.Checked = HaveSonicBoom;
                this.chkBoxVioletTranslator.Checked = HaveVioletTranslator;
                this.chkBoxAmberTranslator.Checked = HaveAmberTranslator;
                this.chkBoxCobaltTranslator.Checked = HaveCobaltTranslator;
                this.chkBoxEmeraldTranslator.Checked = HaveEmeraldTranslator;
                this.chkBoxEnergyTransferModule.Checked = HaveEnergyTransferModule;
                this.lblIGT.Text = IGTAsStr;
                this.lblSkyTempleKeys.Text = "Sky Temple : " + SkyTempleKeys().Where(key => key == true).Count() + " / 9";
                this.lblDarkAgonKeys.Text = "Dark Agon : " + DarkAgonKeys().Where(key => key == true).Count() + " / 3";
                this.lblDarkTorvusKeys.Text = "Dark Torvus : " + DarkTorvusKeys().Where(key => key == true).Count() + " / 3";
                this.lblIngHiveKeys.Text = "Ing Hive : " + IngHiveKeys().Where(key => key == true).Count() + " / 3";
                /*this.label26.Text = "Room ID : 0x" + String.Format("{0:X}", CurrentRoom);
				this.label27.Text = "World ID : 0x" + String.Format("{0:X}", CurrentWorld);*/

                // Easy Mode
                if (chkBoxEasyMode.Checked)
                {
                    AutoRefillMissiles();
                    AutoRefillPowerBombs();
                    AutoRefillHealth();
                }
                // Morph Ball Bombs Insta Refill
                if(chkBoxMBBInstaRefill.Checked)
                {
                    MorphBallBombs = MaxMorphBallBombs;
                }
            } catch
            {
                if (!this.Exiting)
                {
                    new Thread(() => MessageBox.Show("Either Dolphin or the game is not running!\r\nExiting...")).Start();
                    this.Close();
                    this.Exiting = true;
                }
            }
        }

        private void AutoRefillMissiles()
        {
            if (MaxMissiles == 0)
                return;

            long curTime = GetCurTimeInMilliseconds();
            if (Missiles == MaxMissiles)
            {
                AutoRefill_Missiles_LastTime = curTime + AUTOREFILL_DELAY;
                return;
            }
            if (curTime - AutoRefill_Missiles_LastTime <= AUTOREFILL_DELAY)
                return;
            if (Missiles + 1 <= MaxMissiles)
            {
                Missiles++;
                AutoRefill_Missiles_LastTime = curTime;
            }
        }

        private void AutoRefillPowerBombs()
        {
            if (MaxPowerBombs == 0)
                return;

            long curTime = GetCurTimeInMilliseconds();
            if (PowerBombs == MaxPowerBombs)
            {
                AutoRefill_PowerBombs_LastTime = curTime + AUTOREFILL_DELAY;
                return;
            }
            if (curTime - AutoRefill_PowerBombs_LastTime <= AUTOREFILL_DELAY)
                return;
            if (PowerBombs + 1 <= MaxPowerBombs)
            {
                PowerBombs++;
                AutoRefill_PowerBombs_LastTime = curTime;
            }
        }

        private void AutoRefillHealth()
        {
            if (MaxHealth == 0 || this.chkBoxLightSuit.Checked)
                return;
            int HealthRefill = this.chkBoxDarkSuit.Checked ? 1 : 3;
            long curTime = GetCurTimeInMilliseconds();
            if (Health == MaxHealth)
            {
                AutoRefill_Health_LastTime = curTime + AUTOREFILL_DELAY;
                return;
            }
            if (curTime - AutoRefill_Health_LastTime <= AUTOREFILL_DELAY)
                return;
            for (int i = 0; i < HealthRefill; i++)
            {
                if (Health + 1 <= MaxHealth)
                {
                    Health++;
                    AutoRefill_Health_LastTime = curTime;
                }
            }
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (chkBoxDarkMode.Checked)
            {
                this.BackColor = Color.Black;
                this.ForeColor = Color.Gray;
                this.groupBox1.ForeColor = Color.Gray;
                this.groupBox2.ForeColor = Color.Gray;
                this.groupBox3.ForeColor = Color.Gray;
                this.groupBox4.ForeColor = Color.Gray;
                this.groupBox5.ForeColor = Color.Gray;
            }
            else
            {
                this.BackColor = Color.LightGoldenrodYellow;
                this.ForeColor = Color.Black;
                this.groupBox1.ForeColor = Color.Black;
                this.groupBox2.ForeColor = Color.Black;
                this.groupBox3.ForeColor = Color.Black;
                this.groupBox4.ForeColor = Color.Black;
                this.groupBox5.ForeColor = Color.Gray;
            }
            if (!IsLoadingSettings)
                SaveSettings();
        }

        private void chkBoxNoAmmoSystem_CheckedChanged(object sender, EventArgs e)
        {
            AmmoSystem = !chkBoxNoAmmoSystem.Checked;
        }

        private void chkBoxPrime1_Dash_CheckedChanged(object sender, EventArgs e)
        {
            Prime1_Dash = chkBoxPrime1_Dash.Checked;
        }
    }
}
