using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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
        internal long AutoRefill_DarkBeam_LastTime = 0;
        internal long AutoRefill_LightBeam_LastTime = 0;
        internal long Regenerate_Health_LastTime = 0;
        #endregion

        #region Constants
		internal const long GCBaseRamAddr = 0x80000000;
        internal const long OFF_CSTATEMANAGER = 0x3DC900;
        internal const long OFF_CWORLD = 0x850;
        internal const long OFF_CINVENTORY = 0x150C;
        internal const long OFF_ROOM_ID = 0x68;
        internal const long OFF_WORLD_ID = 0x6C;
        internal const String OBTAINED = "O";
        internal const String UNOBTAINED = "X";
        internal const long AUTOREFILL_DELAY_IN_SEC = 2;
        internal const long AUTOREFILL_DELAY = AUTOREFILL_DELAY_IN_SEC * 1000;
        internal const long REGEN_HEALTH_COOLDOWN_IN_MIN = 2;
        internal const long REGEN_HEALTH_COOLDOWN_IN_SEC = REGEN_HEALTH_COOLDOWN_IN_MIN * 60;
        internal const long REGEN_HEALTH_COOLDOWN = REGEN_HEALTH_COOLDOWN_IN_SEC * 1000;

        //internal const long OFF_MORPHBALLBOMBS_COUNT = 0x457D1B;
        internal const long OFF_HEALTH = 0x14;
        internal const long OFF_DARKBEAM_OBTAINED = 0x6F;
        internal const long OFF_LIGHTBEAM_OBTAINED = 0x7B;
        internal const long OFF_ANNIHILATORBEAM_OBTAINED = 0x87;
        internal const long OFF_SUPERMISSILE_OBTAINED = 0x93;
        internal const long OFF_DARKBURST_OBTAINED = 0x9F;
        internal const long OFF_SUNBURST_OBTAINED = 0xAB;
        internal const long OFF_SONICBOOM_OBTAINED = 0xB7;
        internal const long OFF_DARKVISOR_OBTAINED = 0xDC;
        internal const long OFF_ECHOVISOR_OBTAINED = 0xE7;
        internal const long OFF_DARKSUIT_OBTAINED = 0xFF;
        internal const long OFF_LIGHTSUIT_OBTAINED = 0x10B;
        internal const long OFF_BOOSTBALL_OBTAINED = 0x11F;
        internal const long OFF_SPIDERBALL_OBTAINED = 0x12F;
        internal const long OFF_MORPHBALLBOMBS_OBTAINED = 0x137;
        internal const long OFF_CHARGEBEAM_OBTAINED = 0x16B; // useless but still here for reference
        internal const long OFF_GRAPPLEBEAM_OBTAINED = 0x177;
        internal const long OFF_SPACEBOOTS_OBTAINED = 0x183;
        internal const long OFF_GRAVITYBOOST_OBTAINED = 0x18C;
        internal const long OFF_SCREWATTACK_OBTAINED = 0x19B;
        internal const long OFF_SEEKERMISSILE_OBTAINED = 0x1A7;
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
                return (ushort)(MemoryUtils.ReadUInt32(this.dolphin, this.RAMBaseAddr + InventoryOffset + OFF_MAX_ENERGYTANKS)*100 + 99);
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

        internal bool HaveSpaceBoots
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

        internal bool HaveSeekerMissile
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

        internal bool SkyTempleKeys(int index)
        {
            long InventoryOffset = GetInventoryOffset();
            if (InventoryOffset == -1)
                return false;
            switch (index)
            {
                case 0:
                    return MemoryUtils.ReadUInt8(this.dolphin, this.RAMBaseAddr + InventoryOffset + OFF_SKY_TEMPLE_KEY_1_OBTAINED) > 0;
                case 1:
                    return MemoryUtils.ReadUInt8(this.dolphin, this.RAMBaseAddr + InventoryOffset + OFF_SKY_TEMPLE_KEY_2_OBTAINED) > 0;
                case 2:
                    return MemoryUtils.ReadUInt8(this.dolphin, this.RAMBaseAddr + InventoryOffset + OFF_SKY_TEMPLE_KEY_3_OBTAINED) > 0;
                case 3:
                    return MemoryUtils.ReadUInt8(this.dolphin, this.RAMBaseAddr + InventoryOffset + OFF_SKY_TEMPLE_KEY_4_OBTAINED) > 0;
                case 4:
                    return MemoryUtils.ReadUInt8(this.dolphin, this.RAMBaseAddr + InventoryOffset + OFF_SKY_TEMPLE_KEY_5_OBTAINED) > 0;
                case 5:
                    return MemoryUtils.ReadUInt8(this.dolphin, this.RAMBaseAddr + InventoryOffset + OFF_SKY_TEMPLE_KEY_6_OBTAINED) > 0;
                case 6:
                    return MemoryUtils.ReadUInt8(this.dolphin, this.RAMBaseAddr + InventoryOffset + OFF_SKY_TEMPLE_KEY_7_OBTAINED) > 0;
                case 7:
                    return MemoryUtils.ReadUInt8(this.dolphin, this.RAMBaseAddr + InventoryOffset + OFF_SKY_TEMPLE_KEY_8_OBTAINED) > 0;
                case 8:
                    return MemoryUtils.ReadUInt8(this.dolphin, this.RAMBaseAddr + InventoryOffset + OFF_SKY_TEMPLE_KEY_9_OBTAINED) > 0;
                default:
                    throw new Exception("Invalid sky temple key requested");
            }
        }

        internal bool DarkAgonKeys(int index)
        {
            long InventoryOffset = GetInventoryOffset();
            if (InventoryOffset == -1)
                return false;
            switch (index)
            {
                case 0:
                    return MemoryUtils.ReadUInt8(this.dolphin, this.RAMBaseAddr + InventoryOffset + OFF_DARK_AGON_KEY_1_OBTAINED) > 0;
                case 1:
                    return MemoryUtils.ReadUInt8(this.dolphin, this.RAMBaseAddr + InventoryOffset + OFF_DARK_AGON_KEY_2_OBTAINED) > 0;
                case 2:
                    return MemoryUtils.ReadUInt8(this.dolphin, this.RAMBaseAddr + InventoryOffset + OFF_DARK_AGON_KEY_3_OBTAINED) > 0;
                default:
                    throw new Exception("Invalid dark agon key requested");
            }
        }

        internal bool DarkTorvusKeys(int index)
        {
            long InventoryOffset = GetInventoryOffset();
            if (InventoryOffset == -1)
                return false;
            switch (index)
            {
                case 0:
                    return MemoryUtils.ReadUInt8(this.dolphin, this.RAMBaseAddr + InventoryOffset + OFF_DARK_TORVUS_KEY_1_OBTAINED) > 0;
                case 1:
                    return MemoryUtils.ReadUInt8(this.dolphin, this.RAMBaseAddr + InventoryOffset + OFF_DARK_TORVUS_KEY_2_OBTAINED) > 0;
                case 2:
                    return MemoryUtils.ReadUInt8(this.dolphin, this.RAMBaseAddr + InventoryOffset + OFF_DARK_TORVUS_KEY_3_OBTAINED) > 0;
                default:
                    throw new Exception("Invalid dark torvus key requested");
            }
        }

        internal bool IngHiveKeys(int index)
        {
            long InventoryOffset = GetInventoryOffset();
            if (InventoryOffset == -1)
                return false;
            switch (index)
            {
                case 0:
                    return MemoryUtils.ReadUInt8(this.dolphin, this.RAMBaseAddr + InventoryOffset + OFF_ING_HIVE_KEY_1_OBTAINED) > 0;
                case 1:
                    return MemoryUtils.ReadUInt8(this.dolphin, this.RAMBaseAddr + InventoryOffset + OFF_ING_HIVE_KEY_2_OBTAINED) > 0;
                case 2:
                    return MemoryUtils.ReadUInt8(this.dolphin, this.RAMBaseAddr + InventoryOffset + OFF_ING_HIVE_KEY_3_OBTAINED) > 0;
                default:
                    throw new Exception("Invalid ing hive key requested");
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
                        this.checkBox1.Checked = setting[1] == "ON";
                }
                IsLoadingSettings = false;
            }
        }

        void SaveSettings()
        {
            using (var file = new StreamWriter(File.OpenWrite("MP2RandoAssist.ini")))
            {
                file.WriteLine("DarkMode=" + (this.checkBox1.Checked ? "ON" : "OFF"));
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
                this.comboBox1.SelectedIndex = 0;
                this.comboBox1.Update();
                this.comboBox3.SelectedIndex = 0;
                this.comboBox3.Update();
                this.comboBox4.SelectedIndex = 0;
                this.comboBox4.Update();
                this.comboBox5.SelectedIndex = 0;
                this.comboBox5.Update();
                this.dataGridView1.Rows.Add(new object[] { "Sky Temple", "?", "?", "?", "?", "?", "?", "?", "?", "?" });
                this.dataGridView1.Rows.Add(new object[] { "Dark Agon", "?", "?", "?", "", "", "", "", "", "" });
                this.dataGridView1.Rows.Add(new object[] { "Dark Torvus", "?", "?", "?", "", "", "", "", "", "" });
                this.dataGridView1.Rows.Add(new object[] { "Ing Hive", "?", "?", "?", "", "", "", "", "", "" });
                this.timer1.Enabled = true;
            } catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
                this.Close();
            }
        }
		
		private long GetWorldOffset()
        {
            long GC_CWorld = MemoryUtils.ReadUInt32BE(this.dolphin, this.RAMBaseAddr + OFF_CSTATEMANAGER + OFF_CWORLD);
            if (GC_CWorld < GCBaseRamAddr)
                return -1;
            return GC_CWorld - GCBaseRamAddr;
        }

        private long GetInventoryOffset()
        {
            long GC_CInventory = MemoryUtils.ReadUInt32BE(this.dolphin, this.RAMBaseAddr + OFF_CSTATEMANAGER + OFF_CINVENTORY);
            if (GC_CInventory < GCBaseRamAddr)
                return -1;
            return GC_CInventory - GCBaseRamAddr;
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
                this.label4.Text = "HP : " + Health + " / " + MaxHealth;
                if (Health < 30)
                    this.label4.Text += " /!\\";
                this.label1.Text = "Missiles : " + Missiles + " / " + MaxMissiles;
                if (comboBox1.SelectedIndex == 1)
                    AutoRefillMissiles();
                else if (comboBox1.SelectedIndex == 2)
                    Missiles = MaxMissiles;
                this.label3.Text = "Power Bombs : " + PowerBombs + " / " + MaxPowerBombs;
                if (comboBox3.SelectedIndex == 1)
                    AutoRefillPowerBombs();
                else if (comboBox3.SelectedIndex == 2)
                    PowerBombs = MaxPowerBombs;
                this.label26.Text = "Dark Beam : "+DarkBeamAmmo+" / "+MaxDarkBeamAmmo;
                if (comboBox4.SelectedIndex == 1)
                    AutoRefillDarkBeamAmmo();
                else if (comboBox4.SelectedIndex == 2)
                    DarkBeamAmmo = MaxDarkBeamAmmo;
                this.label27.Text = "Light Beam : " + LightBeamAmmo + " / " + MaxLightBeamAmmo;
                if (comboBox5.SelectedIndex == 1)
                    AutoRefillLightBeamAmmo();
                else if (comboBox5.SelectedIndex == 2)
                    LightBeamAmmo = MaxLightBeamAmmo;
                this.label2.Text = "Screw Attack : " + (HaveScrewAttack ? OBTAINED : UNOBTAINED);
                this.label5.Text = "Violet : " + (HaveVioletTranslator ? OBTAINED : UNOBTAINED);
                this.label6.Text = "Echo Visor : " + (HaveEchoVisor ? OBTAINED : UNOBTAINED);
                this.label7.Text = "Dark Visor : " + (HaveDarkVisor ? OBTAINED : UNOBTAINED);
                this.label8.Text = "Morph Ball Bombs : " + (HaveMorphBallBombs ? OBTAINED : UNOBTAINED);
                this.label9.Text = "Missile Launcher : " + (MaxMissiles > 0 ? OBTAINED : UNOBTAINED);
                this.label10.Text = "Super Missile : " + (HaveSuperMissile ? OBTAINED : UNOBTAINED);
                this.label11.Text = "Annihilator Beam : " + (HaveAnnihilatorBeam ? OBTAINED : UNOBTAINED);
                this.label12.Text = "Light Beam : " + (HaveLightBeam ? OBTAINED : UNOBTAINED);
                this.label13.Text = "Dark Beam : " + (HaveDarkBeam ? OBTAINED : UNOBTAINED);
                this.label14.Text = "Gravity Boost : " + (HaveGravityBoost ? OBTAINED : UNOBTAINED);
                this.label15.Text = "Grapple Beam : " + (HaveGrappleBeam ? OBTAINED : UNOBTAINED);
                this.label16.Text = "Spider Ball : " + (HaveSpiderBall ? OBTAINED : UNOBTAINED);
                this.label17.Text = "Boost Ball : " + (HaveBoostBall ? OBTAINED : UNOBTAINED);
                this.label18.Text = "Power Bombs : " + (MaxPowerBombs > 0 ? OBTAINED : UNOBTAINED);
                this.label19.Text = "Seeker Launcher : " + (HaveSeekerMissile ? OBTAINED : UNOBTAINED);
                this.label20.Text = "Dark Suit : " + (HaveDarkSuit ? OBTAINED : UNOBTAINED);
                this.label21.Text = "Light Suit : " + (HaveLightSuit ? OBTAINED : UNOBTAINED);
                this.label22.Text = "Darkburst : " + (HaveDarkBurst ? OBTAINED : UNOBTAINED);
                this.label23.Text = "Sunburst : " + (HaveSunBurst ? OBTAINED : UNOBTAINED);
                this.label24.Text = "Sonic Boom : " + (HaveSonicBoom ? OBTAINED : UNOBTAINED);
                this.label25.Text = "Space Jump Boots : " + (HaveSpaceBoots ? OBTAINED : UNOBTAINED);
                this.label28.Text = "Amber : " + (HaveAmberTranslator ? OBTAINED : UNOBTAINED);
                this.label29.Text = "Cobalt : " + (HaveCobaltTranslator ? OBTAINED : UNOBTAINED);
                this.label30.Text = "Emerald : " + (HaveEmeraldTranslator ? OBTAINED : UNOBTAINED);
                this.label31.Text = "Energy Transfer Mod : " + (HaveEnergyTransferModule ? OBTAINED : UNOBTAINED);
                new Thread(() =>
                {
                    for (int i = 0; i < 9; i++)
                    {
                        this.dataGridView1.Rows[0].Cells[i + 1].Value = SkyTempleKeys(i) ? OBTAINED : UNOBTAINED;
                        if (i < 3)
                        {
                            this.dataGridView1.Rows[1].Cells[i + 1].Value = DarkAgonKeys(i) ? OBTAINED : UNOBTAINED;
                            this.dataGridView1.Rows[2].Cells[i + 1].Value = DarkTorvusKeys(i) ? OBTAINED : UNOBTAINED;
                            this.dataGridView1.Rows[3].Cells[i + 1].Value = IngHiveKeys(i) ? OBTAINED : UNOBTAINED;
                        }
                    }
                }).Start();
                /*this.label26.Text = "Room ID : 0x" + String.Format("{0:X}", CurrentRoom);
				this.label27.Text = "World ID : 0x" + String.Format("{0:X}", CurrentWorld);*/
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
            long curTime = GetCurTimeInMilliseconds();
            if (Missiles == MaxMissiles)
                AutoRefill_Missiles_LastTime = curTime + AUTOREFILL_DELAY;
            if (MaxMissiles == 0)
                return;
            if (Missiles + 1 > MaxMissiles)
                return;
            if (curTime - AutoRefill_Missiles_LastTime <= AUTOREFILL_DELAY)
                return;
            Missiles++;
            AutoRefill_Missiles_LastTime = curTime;
        }

        private void AutoRefillPowerBombs()
        {
            long curTime = GetCurTimeInMilliseconds();
            if (PowerBombs == MaxPowerBombs)
                AutoRefill_PowerBombs_LastTime = curTime + AUTOREFILL_DELAY;
            if (MaxPowerBombs == 0)
                return;
            if (PowerBombs + 1 > MaxPowerBombs)
                return;
            if (curTime - AutoRefill_PowerBombs_LastTime <= AUTOREFILL_DELAY)
                return;
            PowerBombs++;
            AutoRefill_PowerBombs_LastTime = curTime;
        }

        private void AutoRefillDarkBeamAmmo()
        {
            long curTime = GetCurTimeInMilliseconds();
            if (DarkBeamAmmo == MaxDarkBeamAmmo)
                AutoRefill_DarkBeam_LastTime = curTime + AUTOREFILL_DELAY;
            if (MaxDarkBeamAmmo == 0)
                return;
            if (DarkBeamAmmo + 1 > MaxDarkBeamAmmo)
                return;
            if (curTime - AutoRefill_DarkBeam_LastTime <= AUTOREFILL_DELAY)
                return;
            DarkBeamAmmo++;
            AutoRefill_DarkBeam_LastTime = curTime;
        }

        private void AutoRefillLightBeamAmmo()
        {
            long curTime = GetCurTimeInMilliseconds();
            if (LightBeamAmmo == MaxLightBeamAmmo)
                AutoRefill_LightBeam_LastTime = curTime + AUTOREFILL_DELAY;
            if (MaxLightBeamAmmo == 0)
                return;
            if (LightBeamAmmo + 1 > MaxLightBeamAmmo)
                return;
            if (curTime - AutoRefill_LightBeam_LastTime <= AUTOREFILL_DELAY)
                return;
            LightBeamAmmo++;
            AutoRefill_LightBeam_LastTime = curTime;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            long curTime = GetCurTimeInMilliseconds();
            if (Health == MaxHealth)
            {
                Regenerate_Health_LastTime = curTime + REGEN_HEALTH_COOLDOWN;
                MessageBox.Show("You have all your HP!");
                return;
            }
            if (MaxHealth == 0)
                return;
            if (curTime - Regenerate_Health_LastTime <= REGEN_HEALTH_COOLDOWN)
            {
                DateTime remainingTime = new DateTime((REGEN_HEALTH_COOLDOWN - (curTime - Regenerate_Health_LastTime))*TimeSpan.TicksPerMillisecond);
                MessageBox.Show("You can regenerate in "+(remainingTime.Minute == 0 ? "" : remainingTime.Minute+" minute"+(remainingTime.Minute > 1 ? "s ":" "))+ (remainingTime.Second == 0 ? "" : remainingTime.Second + " second" + (remainingTime.Second > 1 ? "s" : "")));
                return;
            }
            Health = MaxHealth;
            Regenerate_Health_LastTime = curTime;
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
            {
                this.BackColor = Color.Black;
                this.ForeColor = Color.Gray;
                this.groupBox1.ForeColor = Color.Gray;
                this.groupBox2.ForeColor = Color.Gray;
                this.groupBox3.ForeColor = Color.Gray;
                this.groupBox4.ForeColor = Color.Gray;
                this.comboBox1.BackColor = Color.Black;
                this.comboBox1.ForeColor = Color.Gray;
                this.comboBox3.BackColor = Color.Black;
                this.comboBox3.ForeColor = Color.Gray;
                this.comboBox4.BackColor = Color.Black;
                this.comboBox4.ForeColor = Color.Gray;
                this.comboBox5.BackColor = Color.Black;
                this.comboBox5.ForeColor = Color.Gray;
                this.button1.BackColor = Color.Black;
                this.dataGridView1.BackColor = Color.Black;
                this.dataGridView1.GridColor = Color.Gray;
                this.dataGridView1.ForeColor = Color.Gray;
                this.dataGridView1.DefaultCellStyle.BackColor = Color.Black;
                this.dataGridView1.DefaultCellStyle.ForeColor = Color.Gray;
            }
            else
            {
                this.BackColor = Color.LightGoldenrodYellow;
                this.ForeColor = Color.Black;
                this.groupBox1.ForeColor = Color.Black;
                this.groupBox2.ForeColor = Color.Black;
                this.groupBox3.ForeColor = Color.Black;
                this.groupBox4.ForeColor = Color.Black;
                this.comboBox1.BackColor = Color.LightGoldenrodYellow;
                this.comboBox1.ForeColor = Color.Black;
                this.comboBox3.BackColor = Color.LightGoldenrodYellow;
                this.comboBox3.ForeColor = Color.Black;
                this.comboBox4.BackColor = Color.LightGoldenrodYellow;
                this.comboBox4.ForeColor = Color.Black;
                this.comboBox5.BackColor = Color.LightGoldenrodYellow;
                this.comboBox5.ForeColor = Color.Black;
                this.button1.BackColor = Color.LightGoldenrodYellow;
                this.dataGridView1.BackColor = Color.LightGoldenrodYellow;
                this.dataGridView1.GridColor = Color.Black;
                this.dataGridView1.ForeColor = Color.Black;
                this.dataGridView1.DefaultCellStyle.BackColor = Color.LightGoldenrodYellow;
                this.dataGridView1.DefaultCellStyle.ForeColor = Color.Black;
            }
            if (!IsLoadingSettings)
                SaveSettings();
        }
    }
}
