using System;
using System.Runtime.InteropServices;
using System.Text;

namespace ecm_wrapper
{
    #region Constants and Defines

    public static class EcrtConstants
    {
        public const int ECRT_VER_MAJOR = 1;
        public const int ECRT_VER_MINOR = 6;
        public const uint EC_END = ~0U;
        public const int EC_MAX_SYNC_MANAGERS = 16;
        public const int EC_MAX_STRING_LENGTH = 64;
        public const int EC_MAX_PORTS = 4;
        public const int EC_COE_EMERGENCY_MSG_SIZE = 8;
        
        public static uint ECRT_VERSION(int major, int minor) => (uint)((major << 8) + minor);
        public static uint ECRT_VERSION_MAGIC => ECRT_VERSION(ECRT_VER_MAJOR, ECRT_VER_MINOR);
        
        public static ulong EC_TIMEVAL2NANO(TimeSpan timespan) =>
            (ulong)((timespan.Ticks / TimeSpan.TicksPerSecond - 946684800L) * 1000000000L + 
                   (timespan.Ticks % TimeSpan.TicksPerSecond) / TimeSpan.TicksPerMicrosecond * 1000L); //funzt die wirklich?
    }

    #endregion

    #region Enumerations

    public enum EcDirection
    {
        EC_DIR_INVALID = 0,
        EC_DIR_OUTPUT = 1,
        EC_DIR_INPUT = 2,
        EC_DIR_COUNT = 3
    }

    public enum EcWatchdogMode
    {
        EC_WD_DEFAULT = 0,
        EC_WD_ENABLE = 1,
        EC_WD_DISABLE = 2
    }

    public enum EcRequestState
    {
        EC_REQUEST_UNUSED = 0,
        EC_REQUEST_BUSY = 1,
        EC_REQUEST_SUCCESS = 2,
        EC_REQUEST_ERROR = 3
    }

    public enum EcAlState
    {
        EC_AL_STATE_INIT = 1,
        EC_AL_STATE_PREOP = 2,
        EC_AL_STATE_SAFEOP = 4,
        EC_AL_STATE_OP = 8
    }

    public enum EcWcState
    {
        EC_WC_ZERO = 0,
        EC_WC_INCOMPLETE = 1,
        EC_WC_COMPLETE = 2
    }

    public enum EcSlavePortDesc
    {
        EC_PORT_NOT_IMPLEMENTED = 0,
        EC_PORT_NOT_CONFIGURED = 1,
        EC_PORT_EBUS = 2,
        EC_PORT_MII = 3
    }

    #endregion

    #region Structures

    [StructLayout(LayoutKind.Sequential)]
    public struct EcMasterState
    {
        public uint slaves_responding;
        private uint _flags;
        
        public uint al_states => _flags & 0xF;
        public bool link_up => (_flags & 0x10) != 0;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct EcMasterLinkState
    {
        public uint slaves_responding;
        private uint _flags;
        
        public uint al_states => _flags & 0xF;
        public bool link_up => (_flags & 0x10) != 0;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct EcSlaveConfigState
    {
        private uint _flags;
        
        public bool online => (_flags & 0x1) != 0;
        public bool operational => (_flags & 0x2) != 0;
        public uint al_state => (_flags >> 2) & 0xF;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct EcMasterInfo
    {
        public uint slave_count;
        private uint _flags;
        public byte scan_busy;
        public ulong app_time;
        
        public bool link_up => (_flags & 0x1) != 0;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct EcMasterScanProgress
    {
        public uint slave_count;
        public uint scan_index;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct EcSlavePortLink
    {
        public byte link_up;
        public byte loop_closed;
        public byte signal_detected;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct EcSlavePortInfo
    {
        public EcSlavePortDesc desc;
        public EcSlavePortLink link;
        public uint receive_time;
        public ushort next_slave;
        public uint delay_to_next_dc;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct EcSlaveInfo
    {
        public ushort position;
        public uint vendor_id;
        public uint product_code;
        public uint revision_number;
        public uint serial_number;
        public ushort alias;
        public short current_on_ebus;
        
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = EcrtConstants.EC_MAX_PORTS)]
        public EcSlavePortInfo[] ports;
        
        public byte al_state;
        public byte error_flag;
        public byte sync_count;
        public ushort sdo_count;
        
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = EcrtConstants.EC_MAX_STRING_LENGTH)]
        public string name;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct EcDomainState
    {
        public uint working_counter;
        public EcWcState wc_state;
        public uint redundancy_active;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct EcPdoEntryInfo
    {
        public ushort index;
        public byte subindex;
        public byte bit_length;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct EcPdoInfo
    {
        public ushort index;
        public uint n_entries;
        public IntPtr entries; // EcPdoEntryInfo*
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct EcSyncInfo
    {
        public byte index;
        public EcDirection dir;
        public uint n_pdos;
        public IntPtr pdos; // EcPdoInfo*
        public EcWatchdogMode watchdog_mode;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct EcPdoEntryReg
    {
        public ushort alias;
        public ushort position;
        public uint vendor_id;
        public uint product_code;
        public ushort index;
        public byte subindex;
        public IntPtr offset; // uint*
        public IntPtr bit_position; // uint*
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct InAddr
    {
        public uint s_addr;
    }

    #endregion

    #region Native Function Declarations

    public static class EcrtNative
    {
        private const string ECRT_LIB = "ethercat";

        #region Global Functions

        [DllImport(ECRT_LIB, CallingConvention = CallingConvention.Cdecl)]
        public static extern uint ecrt_version_magic();

        [DllImport(ECRT_LIB, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr ecrt_request_master(uint master_index);

        [DllImport(ECRT_LIB, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr ecrt_open_master(uint master_index);

        [DllImport(ECRT_LIB, CallingConvention = CallingConvention.Cdecl)]
        public static extern void ecrt_release_master(IntPtr master);

        #endregion

        #region Master Methods

        [DllImport(ECRT_LIB, CallingConvention = CallingConvention.Cdecl)]
        public static extern int ecrt_master_reserve(IntPtr master);

        [DllImport(ECRT_LIB, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr ecrt_master_create_domain(IntPtr master);

        [DllImport(ECRT_LIB, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr ecrt_master_slave_config(IntPtr master, ushort alias, ushort position, uint vendor_id, uint product_code);

        [DllImport(ECRT_LIB, CallingConvention = CallingConvention.Cdecl)]
        public static extern int ecrt_master_select_reference_clock(IntPtr master, IntPtr sc);

        [DllImport(ECRT_LIB, CallingConvention = CallingConvention.Cdecl)]
        public static extern int ecrt_master(IntPtr master, out EcMasterInfo master_info);

        [DllImport(ECRT_LIB, CallingConvention = CallingConvention.Cdecl)]
        public static extern int ecrt_master_scan_progress(IntPtr master, out EcMasterScanProgress progress);

        [DllImport(ECRT_LIB, CallingConvention = CallingConvention.Cdecl)]
        public static extern int ecrt_master_get_slave(IntPtr master, ushort slave_position, out EcSlaveInfo slave_info);

        [DllImport(ECRT_LIB, CallingConvention = CallingConvention.Cdecl)]
        public static extern int ecrt_master_get_sync_manager(IntPtr master, ushort slave_position, byte sync_index, out EcSyncInfo sync);

        [DllImport(ECRT_LIB, CallingConvention = CallingConvention.Cdecl)]
        public static extern int ecrt_master_get_pdo(IntPtr master, ushort slave_position, byte sync_index, ushort pos, out EcPdoInfo pdo);

        [DllImport(ECRT_LIB, CallingConvention = CallingConvention.Cdecl)]
        public static extern int ecrt_master_get_pdo_entry(IntPtr master, ushort slave_position, byte sync_index, ushort pdo_pos, ushort entry_pos, out EcPdoEntryInfo entry);

        [DllImport(ECRT_LIB, CallingConvention = CallingConvention.Cdecl)]
        public static extern int ecrt_master_sdo_download(IntPtr master, ushort slave_position, ushort index, byte subindex, IntPtr data, UIntPtr data_size, out uint abort_code);

        [DllImport(ECRT_LIB, CallingConvention = CallingConvention.Cdecl)]
        public static extern int ecrt_master_sdo_download_complete(IntPtr master, ushort slave_position, ushort index, IntPtr data, UIntPtr data_size, out uint abort_code);

        [DllImport(ECRT_LIB, CallingConvention = CallingConvention.Cdecl)]
        public static extern int ecrt_master_sdo_upload(IntPtr master, ushort slave_position, ushort index, byte subindex, IntPtr target, UIntPtr target_size, out UIntPtr result_size, out uint abort_code);

        [DllImport(ECRT_LIB, CallingConvention = CallingConvention.Cdecl)]
        public static extern int ecrt_master_write_idn(IntPtr master, ushort slave_position, byte drive_no, ushort idn, IntPtr data, UIntPtr data_size, out ushort error_code);

        [DllImport(ECRT_LIB, CallingConvention = CallingConvention.Cdecl)]
        public static extern int ecrt_master_read_idn(IntPtr master, ushort slave_position, byte drive_no, ushort idn, IntPtr target, UIntPtr target_size, out UIntPtr result_size, out ushort error_code);

        [DllImport(ECRT_LIB, CallingConvention = CallingConvention.Cdecl)]
        public static extern int ecrt_master_activate(IntPtr master);

        [DllImport(ECRT_LIB, CallingConvention = CallingConvention.Cdecl)]
        public static extern int ecrt_master_deactivate(IntPtr master);

        [DllImport(ECRT_LIB, CallingConvention = CallingConvention.Cdecl)]
        public static extern int ecrt_master_set_send_interval(IntPtr master, UIntPtr send_interval);

        [DllImport(ECRT_LIB, CallingConvention = CallingConvention.Cdecl)]
        public static extern int ecrt_master_send(IntPtr master);

        [DllImport(ECRT_LIB, CallingConvention = CallingConvention.Cdecl)]
        public static extern int ecrt_master_receive(IntPtr master);

        [DllImport(ECRT_LIB, CallingConvention = CallingConvention.Cdecl)]
        public static extern int ecrt_master_state(IntPtr master, out EcMasterState state);

        [DllImport(ECRT_LIB, CallingConvention = CallingConvention.Cdecl)]
        public static extern int ecrt_master_link_state(IntPtr master, uint dev_idx, out EcMasterLinkState state);

        [DllImport(ECRT_LIB, CallingConvention = CallingConvention.Cdecl)]
        public static extern int ecrt_master_application_time(IntPtr master, ulong app_time);

        [DllImport(ECRT_LIB, CallingConvention = CallingConvention.Cdecl)]
        public static extern int ecrt_master_sync_reference_clock(IntPtr master);

        [DllImport(ECRT_LIB, CallingConvention = CallingConvention.Cdecl)]
        public static extern int ecrt_master_sync_reference_clock_to(IntPtr master, ulong sync_time);

        [DllImport(ECRT_LIB, CallingConvention = CallingConvention.Cdecl)]
        public static extern int ecrt_master_sync_slave_clocks(IntPtr master);

        [DllImport(ECRT_LIB, CallingConvention = CallingConvention.Cdecl)]
        public static extern int ecrt_master_reference_clock_time(IntPtr master, out uint time);

        [DllImport(ECRT_LIB, CallingConvention = CallingConvention.Cdecl)]
        public static extern int ecrt_master_sync_monitor_queue(IntPtr master);

        [DllImport(ECRT_LIB, CallingConvention = CallingConvention.Cdecl)]
        public static extern uint ecrt_master_sync_monitor_process(IntPtr master);

        [DllImport(ECRT_LIB, CallingConvention = CallingConvention.Cdecl)]
        public static extern int ecrt_master_reset(IntPtr master);

        #endregion

        #region Slave Configuration Methods

        [DllImport(ECRT_LIB, CallingConvention = CallingConvention.Cdecl)]
        public static extern int ecrt_slave_config_sync_manager(IntPtr sc, byte sync_index, EcDirection direction, EcWatchdogMode watchdog_mode);

        [DllImport(ECRT_LIB, CallingConvention = CallingConvention.Cdecl)]
        public static extern int ecrt_slave_config_watchdog(IntPtr sc, ushort watchdog_divider, ushort watchdog_intervals);

        [DllImport(ECRT_LIB, CallingConvention = CallingConvention.Cdecl)]
        public static extern int ecrt_slave_config_pdo_assign_add(IntPtr sc, byte sync_index, ushort index);

        [DllImport(ECRT_LIB, CallingConvention = CallingConvention.Cdecl)]
        public static extern int ecrt_slave_config_pdo_assign_clear(IntPtr sc, byte sync_index);

        [DllImport(ECRT_LIB, CallingConvention = CallingConvention.Cdecl)]
        public static extern int ecrt_slave_config_pdo_mapping_add(IntPtr sc, ushort pdo_index, ushort entry_index, byte entry_subindex, byte entry_bit_length);

        [DllImport(ECRT_LIB, CallingConvention = CallingConvention.Cdecl)]
        public static extern int ecrt_slave_config_pdo_mapping_clear(IntPtr sc, ushort pdo_index);

        [DllImport(ECRT_LIB, CallingConvention = CallingConvention.Cdecl)]
        public static extern int ecrt_slave_config_pdos(IntPtr sc, uint n_syncs, IntPtr syncs);

        [DllImport(ECRT_LIB, CallingConvention = CallingConvention.Cdecl)]
        public static extern int ecrt_slave_config_reg_pdo_entry(IntPtr sc, ushort entry_index, byte entry_subindex, IntPtr domain, out uint bit_position);

        [DllImport(ECRT_LIB, CallingConvention = CallingConvention.Cdecl)]
        public static extern int ecrt_slave_config_reg_pdo_entry_pos(IntPtr sc, byte sync_index, uint pdo_pos, uint entry_pos, IntPtr domain, out uint bit_position);

        [DllImport(ECRT_LIB, CallingConvention = CallingConvention.Cdecl)]
        public static extern int ecrt_slave_config_dc(IntPtr sc, ushort assign_activate, uint sync0_cycle, int sync0_shift, uint sync1_cycle, int sync1_shift);

        [DllImport(ECRT_LIB, CallingConvention = CallingConvention.Cdecl)]
        public static extern int ecrt_slave_config_sdo(IntPtr sc, ushort index, byte subindex, IntPtr data, UIntPtr size);

        [DllImport(ECRT_LIB, CallingConvention = CallingConvention.Cdecl)]
        public static extern int ecrt_slave_config_sdo8(IntPtr sc, ushort sdo_index, byte sdo_subindex, byte value);

        [DllImport(ECRT_LIB, CallingConvention = CallingConvention.Cdecl)]
        public static extern int ecrt_slave_config_sdo16(IntPtr sc, ushort sdo_index, byte sdo_subindex, ushort value);

        [DllImport(ECRT_LIB, CallingConvention = CallingConvention.Cdecl)]
        public static extern int ecrt_slave_config_sdo32(IntPtr sc, ushort sdo_index, byte sdo_subindex, uint value);

        [DllImport(ECRT_LIB, CallingConvention = CallingConvention.Cdecl)]
        public static extern int ecrt_slave_config_complete_sdo(IntPtr sc, ushort index, IntPtr data, UIntPtr size);

        [DllImport(ECRT_LIB, CallingConvention = CallingConvention.Cdecl)]
        public static extern int ecrt_slave_config_emerg_size(IntPtr sc, UIntPtr elements);

        [DllImport(ECRT_LIB, CallingConvention = CallingConvention.Cdecl)]
        public static extern int ecrt_slave_config_emerg_pop(IntPtr sc, IntPtr target);

        [DllImport(ECRT_LIB, CallingConvention = CallingConvention.Cdecl)]
        public static extern int ecrt_slave_config_emerg_clear(IntPtr sc);

        [DllImport(ECRT_LIB, CallingConvention = CallingConvention.Cdecl)]
        public static extern int ecrt_slave_config_emerg_overruns(IntPtr sc);

        [DllImport(ECRT_LIB, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr ecrt_slave_config_create_sdo_request(IntPtr sc, ushort index, byte subindex, UIntPtr size);

        [DllImport(ECRT_LIB, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr ecrt_slave_config_create_soe_request(IntPtr sc, byte drive_no, ushort idn, UIntPtr size);

        [DllImport(ECRT_LIB, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr ecrt_slave_config_create_voe_handler(IntPtr sc, UIntPtr size);

        [DllImport(ECRT_LIB, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr ecrt_slave_config_create_reg_request(IntPtr sc, UIntPtr size);

        [DllImport(ECRT_LIB, CallingConvention = CallingConvention.Cdecl)]
        public static extern int ecrt_slave_config_state(IntPtr sc, out EcSlaveConfigState state);

        [DllImport(ECRT_LIB, CallingConvention = CallingConvention.Cdecl)]
        public static extern int ecrt_slave_config_idn(IntPtr sc, byte drive_no, ushort idn, EcAlState state, IntPtr data, UIntPtr size);

        [DllImport(ECRT_LIB, CallingConvention = CallingConvention.Cdecl)]
        public static extern int ecrt_slave_config_flag(IntPtr sc, [MarshalAs(UnmanagedType.LPStr)] string key, int value);

        [DllImport(ECRT_LIB, CallingConvention = CallingConvention.Cdecl)]
        public static extern int ecrt_slave_config_eoe_mac_address(IntPtr sc, IntPtr mac_address);

        [DllImport(ECRT_LIB, CallingConvention = CallingConvention.Cdecl)]
        public static extern int ecrt_slave_config_eoe_ip_address(IntPtr sc, InAddr ip_address);

        [DllImport(ECRT_LIB, CallingConvention = CallingConvention.Cdecl)]
        public static extern int ecrt_slave_config_eoe_subnet_mask(IntPtr sc, InAddr subnet_mask);

        [DllImport(ECRT_LIB, CallingConvention = CallingConvention.Cdecl)]
        public static extern int ecrt_slave_config_eoe_default_gateway(IntPtr sc, InAddr gateway_address);

        [DllImport(ECRT_LIB, CallingConvention = CallingConvention.Cdecl)]
        public static extern int ecrt_slave_config_eoe_dns_address(IntPtr sc, InAddr dns_address);

        [DllImport(ECRT_LIB, CallingConvention = CallingConvention.Cdecl)]
        public static extern int ecrt_slave_config_eoe_hostname(IntPtr sc, [MarshalAs(UnmanagedType.LPStr)] string name);

        [DllImport(ECRT_LIB, CallingConvention = CallingConvention.Cdecl)]
        public static extern int ecrt_slave_config_state_timeout(IntPtr sc, EcAlState from_state, EcAlState to_state, uint timeout_ms);

        #endregion

        #region Domain Methods

        [DllImport(ECRT_LIB, CallingConvention = CallingConvention.Cdecl)]
        public static extern int ecrt_domain_reg_pdo_entry_list(IntPtr domain, IntPtr pdo_entry_regs);

        [DllImport(ECRT_LIB, CallingConvention = CallingConvention.Cdecl)]
        public static extern UIntPtr ecrt_domain_size(IntPtr domain);

        [DllImport(ECRT_LIB, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr ecrt_domain_data(IntPtr domain);

        [DllImport(ECRT_LIB, CallingConvention = CallingConvention.Cdecl)]
        public static extern int ecrt_domain_process(IntPtr domain);

        [DllImport(ECRT_LIB, CallingConvention = CallingConvention.Cdecl)]
        public static extern int ecrt_domain_queue(IntPtr domain);

        [DllImport(ECRT_LIB, CallingConvention = CallingConvention.Cdecl)]
        public static extern int ecrt_domain_state(IntPtr domain, out EcDomainState state);

        #endregion

        #region SDO Request Methods

        [DllImport(ECRT_LIB, CallingConvention = CallingConvention.Cdecl)]
        public static extern int ecrt_sdo_request_index(IntPtr req, ushort index, byte subindex);

        [DllImport(ECRT_LIB, CallingConvention = CallingConvention.Cdecl)]
        public static extern int ecrt_sdo_request_timeout(IntPtr req, uint timeout);

        [DllImport(ECRT_LIB, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr ecrt_sdo_request_data(IntPtr req);

        [DllImport(ECRT_LIB, CallingConvention = CallingConvention.Cdecl)]
        public static extern UIntPtr ecrt_sdo_request_data_size(IntPtr req);

        [DllImport(ECRT_LIB, CallingConvention = CallingConvention.Cdecl)]
        public static extern EcRequestState ecrt_sdo_request_state(IntPtr req);

        [DllImport(ECRT_LIB, CallingConvention = CallingConvention.Cdecl)]
        public static extern int ecrt_sdo_request_write(IntPtr req);

        [DllImport(ECRT_LIB, CallingConvention = CallingConvention.Cdecl)]
        public static extern int ecrt_sdo_request_read(IntPtr req);

        #endregion

        #region SoE Request Methods

        [DllImport(ECRT_LIB, CallingConvention = CallingConvention.Cdecl)]
        public static extern int ecrt_soe_request_idn(IntPtr req, byte drive_no, ushort idn);

        [DllImport(ECRT_LIB, CallingConvention = CallingConvention.Cdecl)]
        public static extern int ecrt_soe_request_timeout(IntPtr req, uint timeout);

        [DllImport(ECRT_LIB, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr ecrt_soe_request_data(IntPtr req);

        [DllImport(ECRT_LIB, CallingConvention = CallingConvention.Cdecl)]
        public static extern UIntPtr ecrt_soe_request_data_size(IntPtr req);

        [DllImport(ECRT_LIB, CallingConvention = CallingConvention.Cdecl)]
        public static extern EcRequestState ecrt_soe_request_state(IntPtr req);

        [DllImport(ECRT_LIB, CallingConvention = CallingConvention.Cdecl)]
        public static extern int ecrt_soe_request_write(IntPtr req);

        [DllImport(ECRT_LIB, CallingConvention = CallingConvention.Cdecl)]
        public static extern int ecrt_soe_request_read(IntPtr req);

        #endregion

        #region VoE Handler Methods

        [DllImport(ECRT_LIB, CallingConvention = CallingConvention.Cdecl)]
        public static extern int ecrt_voe_handler_send_header(IntPtr voe, uint vendor_id, ushort vendor_type);

        [DllImport(ECRT_LIB, CallingConvention = CallingConvention.Cdecl)]
        public static extern int ecrt_voe_handler_received_header(IntPtr voe, out uint vendor_id, out ushort vendor_type);

        [DllImport(ECRT_LIB, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr ecrt_voe_handler_data(IntPtr voe);

        [DllImport(ECRT_LIB, CallingConvention = CallingConvention.Cdecl)]
        public static extern UIntPtr ecrt_voe_handler_data_size(IntPtr voe);

        [DllImport(ECRT_LIB, CallingConvention = CallingConvention.Cdecl)]
        public static extern int ecrt_voe_handler_write(IntPtr voe, UIntPtr size);

        [DllImport(ECRT_LIB, CallingConvention = CallingConvention.Cdecl)]
        public static extern int ecrt_voe_handler_read(IntPtr voe);

        [DllImport(ECRT_LIB, CallingConvention = CallingConvention.Cdecl)]
        public static extern int ecrt_voe_handler_read_nosync(IntPtr voe);

        [DllImport(ECRT_LIB, CallingConvention = CallingConvention.Cdecl)]
        public static extern EcRequestState ecrt_voe_handler_execute(IntPtr voe);

        #endregion

        #region Register Request Methods

        [DllImport(ECRT_LIB, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr ecrt_reg_request_data(IntPtr req);

        [DllImport(ECRT_LIB, CallingConvention = CallingConvention.Cdecl)]
        public static extern EcRequestState ecrt_reg_request_state(IntPtr req);

        [DllImport(ECRT_LIB, CallingConvention = CallingConvention.Cdecl)]
        public static extern int ecrt_reg_request_write(IntPtr req, ushort address, UIntPtr size);

        [DllImport(ECRT_LIB, CallingConvention = CallingConvention.Cdecl)]
        public static extern int ecrt_reg_request_read(IntPtr req, ushort address, UIntPtr size);

        #endregion

        #region Floating Point Functions (Userspace only)

        [DllImport(ECRT_LIB, CallingConvention = CallingConvention.Cdecl)]
        public static extern float ecrt_read_real(IntPtr data);

        [DllImport(ECRT_LIB, CallingConvention = CallingConvention.Cdecl)]
        public static extern double ecrt_read_lreal(IntPtr data);

        [DllImport(ECRT_LIB, CallingConvention = CallingConvention.Cdecl)]
        public static extern void ecrt_write_real(IntPtr data, float value);

        [DllImport(ECRT_LIB, CallingConvention = CallingConvention.Cdecl)]
        public static extern void ecrt_write_lreal(IntPtr data, double value);

        #endregion
    }

    #endregion

    #region High-Level C# Wrapper Classes

    /// <summary>
    /// High-level wrapper for EtherCAT Master
    /// </summary>
    public class EcMaster : IDisposable
    {
        private IntPtr _masterPtr;
        private bool _disposed = false;

        public EcMaster(uint masterIndex)
        {
            _masterPtr = EcrtNative.ecrt_request_master(masterIndex);
            if (_masterPtr == IntPtr.Zero)
                throw new InvalidOperationException($"Failed to request master {masterIndex}");
        }

        public static EcMaster Open(uint masterIndex)
        {
            var masterPtr = EcrtNative.ecrt_open_master(masterIndex);
            if (masterPtr == IntPtr.Zero)
                throw new InvalidOperationException($"Failed to open master {masterIndex}");
            
            var master = new EcMaster();
            master._masterPtr = masterPtr;
            return master;
        }

        private EcMaster() { }

        public int Reserve()
        {
            CheckDisposed();
            return EcrtNative.ecrt_master_reserve(_masterPtr);
        }

        public EcDomain CreateDomain()
        {
            CheckDisposed();
            var domainPtr = EcrtNative.ecrt_master_create_domain(_masterPtr);
            if (domainPtr == IntPtr.Zero)
                throw new InvalidOperationException("Failed to create domain");
            return new EcDomain(domainPtr);
        }

        public EcSlaveConfig SlaveConfig(ushort alias, ushort position, uint vendorId, uint productCode)
        {
            CheckDisposed();
            var configPtr = EcrtNative.ecrt_master_slave_config(_masterPtr, alias, position, vendorId, productCode);
            if (configPtr == IntPtr.Zero)
                throw new InvalidOperationException("Failed to create slave configuration");
            return new EcSlaveConfig(configPtr);
        }

        public int SelectReferenceClock(EcSlaveConfig slaveConfig = null)
        {
            CheckDisposed();
            return EcrtNative.ecrt_master_select_reference_clock(_masterPtr, slaveConfig?._configPtr ?? IntPtr.Zero);
        }

        public EcMasterInfo GetInfo()
        {
            CheckDisposed();
            EcMasterInfo info;
            var result = EcrtNative.ecrt_master(_masterPtr, out info);
            if (result != 0)
                throw new InvalidOperationException($"Failed to get master info: {result}");
            return info;
        }

        public EcMasterScanProgress GetScanProgress()
        {
            CheckDisposed();
            EcMasterScanProgress progress;
            var result = EcrtNative.ecrt_master_scan_progress(_masterPtr, out progress);
            if (result != 0)
                throw new InvalidOperationException($"Failed to get scan progress: {result}");
            return progress;
        }

        public EcSlaveInfo GetSlaveInfo(ushort slavePosition)
        {
            CheckDisposed();
            EcSlaveInfo info;
            var result = EcrtNative.ecrt_master_get_slave(_masterPtr, slavePosition, out info);
            if (result != 0)
                throw new InvalidOperationException($"Failed to get slave info: {result}");
            return info;
        }

        public int SdoDownload(ushort slavePosition, ushort index, byte subindex, byte[] data, out uint abortCode)
        {
            CheckDisposed();
            var dataPtr = Marshal.AllocHGlobal(data.Length);
            try
            {
                Marshal.Copy(data, 0, dataPtr, data.Length);
                return EcrtNative.ecrt_master_sdo_download(_masterPtr, slavePosition, index, subindex, dataPtr, (UIntPtr)data.Length, out abortCode);
            }
            finally
            {
                Marshal.FreeHGlobal(dataPtr);
            }
        }

        public int SdoUpload(ushort slavePosition, ushort index, byte subindex, byte[] buffer, out int resultSize, out uint abortCode)
        {
            CheckDisposed();
            var bufferPtr = Marshal.AllocHGlobal(buffer.Length);
            try
            {
                UIntPtr resultSizePtr;
                var result = EcrtNative.ecrt_master_sdo_upload(_masterPtr, slavePosition, index, subindex, bufferPtr, (UIntPtr)buffer.Length, out resultSizePtr, out abortCode);
                resultSize = (int)resultSizePtr;
                if (result == 0 && resultSize > 0)
                {
                    Marshal.Copy(bufferPtr, buffer, 0, Math.Min(resultSize, buffer.Length));
                }
                return result;
            }
            finally
            {
                Marshal.FreeHGlobal(bufferPtr);
            }
        }

        public int Activate()
        {
            CheckDisposed();
            return EcrtNative.ecrt_master_activate(_masterPtr);
        }

        public int Deactivate()
        {
            CheckDisposed();
            return EcrtNative.ecrt_master_deactivate(_masterPtr);
        }

        public int SetSendInterval(uint sendInterval)
        {
            CheckDisposed();
            return EcrtNative.ecrt_master_set_send_interval(_masterPtr, (UIntPtr)sendInterval);
        }

        public int Send()
        {
            CheckDisposed();
            return EcrtNative.ecrt_master_send(_masterPtr);
        }

        public int Receive()
        {
            CheckDisposed();
            return EcrtNative.ecrt_master_receive(_masterPtr);
        }

        public EcMasterState GetState()
        {
            CheckDisposed();
            EcMasterState state;
            var result = EcrtNative.ecrt_master_state(_masterPtr, out state);
            if (result != 0)
                throw new InvalidOperationException($"Failed to get master state: {result}");
            return state;
        }

        public EcMasterLinkState GetLinkState(uint deviceIndex)
        {
            CheckDisposed();
            EcMasterLinkState state;
            var result = EcrtNative.ecrt_master_link_state(_masterPtr, deviceIndex, out state);
            if (result != 0)
                throw new InvalidOperationException($"Failed to get link state: {result}");
            return state;
        }

        public int SetApplicationTime(ulong appTime)
        {
            CheckDisposed();
            return EcrtNative.ecrt_master_application_time(_masterPtr, appTime);
        }

        public int SyncReferenceClock()
        {
            CheckDisposed();
            return EcrtNative.ecrt_master_sync_reference_clock(_masterPtr);
        }

        public int SyncReferenceClockTo(ulong syncTime)
        {
            CheckDisposed();
            return EcrtNative.ecrt_master_sync_reference_clock_to(_masterPtr, syncTime);
        }

        public int SyncSlaveClocks()
        {
            CheckDisposed();
            return EcrtNative.ecrt_master_sync_slave_clocks(_masterPtr);
        }

        public int GetReferenceClockTime(out uint time)
        {
            CheckDisposed();
            return EcrtNative.ecrt_master_reference_clock_time(_masterPtr, out time);
        }

        public int Reset()
        {
            CheckDisposed();
            return EcrtNative.ecrt_master_reset(_masterPtr);
        }

        private void CheckDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(EcMaster));
        }

        public void Dispose()
        {
            if (!_disposed && _masterPtr != IntPtr.Zero)
            {
                EcrtNative.ecrt_release_master(_masterPtr);
                _masterPtr = IntPtr.Zero;
                _disposed = true;
            }
        }
    }

    /// <summary>
    /// High-level wrapper for EtherCAT Domain
    /// </summary>
    public class EcDomain
    {
        internal IntPtr _domainPtr;

        internal EcDomain(IntPtr domainPtr)
        {
            _domainPtr = domainPtr;
        }

        public int RegisterPdoEntryList(EcPdoEntryReg[] registrations)
        {
            var size = Marshal.SizeOf<EcPdoEntryReg>();
            var arrayPtr = Marshal.AllocHGlobal(size * (registrations.Length + 1));
            try
            {
                for (int i = 0; i < registrations.Length; i++)
                {
                    Marshal.StructureToPtr(registrations[i], arrayPtr + i * size, false);
                }
                // Add terminator
                var terminator = new EcPdoEntryReg { index = 0 };
                Marshal.StructureToPtr(terminator, arrayPtr + registrations.Length * size, false);
                
                return EcrtNative.ecrt_domain_reg_pdo_entry_list(_domainPtr, arrayPtr);
            }
            finally
            {
                Marshal.FreeHGlobal(arrayPtr);
            }
        }

        public uint Size => (uint)EcrtNative.ecrt_domain_size(_domainPtr);

        public IntPtr Data => EcrtNative.ecrt_domain_data(_domainPtr);

        public int Process()
        {
            return EcrtNative.ecrt_domain_process(_domainPtr);
        }

        public int Queue()
        {
            return EcrtNative.ecrt_domain_queue(_domainPtr);
        }

        public EcDomainState GetState()
        {
            EcDomainState state;
            var result = EcrtNative.ecrt_domain_state(_domainPtr, out state);
            if (result != 0)
                throw new InvalidOperationException($"Failed to get domain state: {result}");
            return state;
        }
    }

    /// <summary>
    /// High-level wrapper for EtherCAT Slave Configuration
    /// </summary>
    public class EcSlaveConfig
    {
        internal IntPtr _configPtr;

        internal EcSlaveConfig(IntPtr configPtr)
        {
            _configPtr = configPtr;
        }

        public int ConfigureSyncManager(byte syncIndex, EcDirection direction, EcWatchdogMode watchdogMode)
        {
            return EcrtNative.ecrt_slave_config_sync_manager(_configPtr, syncIndex, direction, watchdogMode);
        }

        public int ConfigureWatchdog(ushort watchdogDivider, ushort watchdogIntervals)
        {
            return EcrtNative.ecrt_slave_config_watchdog(_configPtr, watchdogDivider, watchdogIntervals);
        }

        public int ConfigurePdos(EcSyncInfo[] syncs)
        {
            var size = Marshal.SizeOf<EcSyncInfo>();
            var arrayPtr = Marshal.AllocHGlobal(size * syncs.Length);
            try
            {
                for (int i = 0; i < syncs.Length; i++)
                {
                    Marshal.StructureToPtr(syncs[i], arrayPtr + i * size, false);
                }
                return EcrtNative.ecrt_slave_config_pdos(_configPtr, (uint)syncs.Length, arrayPtr);
            }
            finally
            {
                Marshal.FreeHGlobal(arrayPtr);
            }
        }

        public int RegisterPdoEntry(ushort entryIndex, byte entrySubindex, EcDomain domain, out uint bitPosition)
        {
            return EcrtNative.ecrt_slave_config_reg_pdo_entry(_configPtr, entryIndex, entrySubindex, domain._domainPtr, out bitPosition);
        }

        public int ConfigureDistributedClocks(ushort assignActivate, uint sync0Cycle, int sync0Shift, uint sync1Cycle, int sync1Shift)
        {
            return EcrtNative.ecrt_slave_config_dc(_configPtr, assignActivate, sync0Cycle, sync0Shift, sync1Cycle, sync1Shift);
        }

        public int ConfigureSdo(ushort index, byte subindex, byte[] data)
        {
            var dataPtr = Marshal.AllocHGlobal(data.Length);
            try
            {
                Marshal.Copy(data, 0, dataPtr, data.Length);
                return EcrtNative.ecrt_slave_config_sdo(_configPtr, index, subindex, dataPtr, (UIntPtr)data.Length);
            }
            finally
            {
                Marshal.FreeHGlobal(dataPtr);
            }
        }

        public int ConfigureSdo8(ushort sdoIndex, byte sdoSubindex, byte value)
        {
            return EcrtNative.ecrt_slave_config_sdo8(_configPtr, sdoIndex, sdoSubindex, value);
        }

        public int ConfigureSdo16(ushort sdoIndex, byte sdoSubindex, ushort value)
        {
            return EcrtNative.ecrt_slave_config_sdo16(_configPtr, sdoIndex, sdoSubindex, value);
        }

        public int ConfigureSdo32(ushort sdoIndex, byte sdoSubindex, uint value)
        {
            return EcrtNative.ecrt_slave_config_sdo32(_configPtr, sdoIndex, sdoSubindex, value);
        }

        public int ConfigureCompleteSdo(ushort index, byte[] data)
        {
            var dataPtr = Marshal.AllocHGlobal(data.Length);
            try
            {
                Marshal.Copy(data, 0, dataPtr, data.Length);
                return EcrtNative.ecrt_slave_config_complete_sdo(_configPtr, index, dataPtr, (UIntPtr)data.Length);
            }
            finally
            {
                Marshal.FreeHGlobal(dataPtr);
            }
        }

        public int SetEmergencySize(uint elements)
        {
            return EcrtNative.ecrt_slave_config_emerg_size(_configPtr, (UIntPtr)elements);
        }

        public int PopEmergency(byte[] target)
        {
            if (target.Length < EcrtConstants.EC_COE_EMERGENCY_MSG_SIZE)
                throw new ArgumentException($"Target buffer must be at least {EcrtConstants.EC_COE_EMERGENCY_MSG_SIZE} bytes");

            var targetPtr = Marshal.AllocHGlobal(target.Length);
            try
            {
                var result = EcrtNative.ecrt_slave_config_emerg_pop(_configPtr, targetPtr);
                if (result == 0)
                {
                    Marshal.Copy(targetPtr, target, 0, Math.Min(target.Length, EcrtConstants.EC_COE_EMERGENCY_MSG_SIZE));
                }
                return result;
            }
            finally
            {
                Marshal.FreeHGlobal(targetPtr);
            }
        }

        public int ClearEmergency()
        {
            return EcrtNative.ecrt_slave_config_emerg_clear(_configPtr);
        }

        public int GetEmergencyOverruns()
        {
            return EcrtNative.ecrt_slave_config_emerg_overruns(_configPtr);
        }

        public EcSdoRequest CreateSdoRequest(ushort index, byte subindex, uint size)
        {
            var requestPtr = EcrtNative.ecrt_slave_config_create_sdo_request(_configPtr, index, subindex, (UIntPtr)size);
            if (requestPtr == IntPtr.Zero)
                throw new InvalidOperationException("Failed to create SDO request");
            return new EcSdoRequest(requestPtr);
        }

        public EcSoeRequest CreateSoeRequest(byte driveNo, ushort idn, uint size)
        {
            var requestPtr = EcrtNative.ecrt_slave_config_create_soe_request(_configPtr, driveNo, idn, (UIntPtr)size);
            if (requestPtr == IntPtr.Zero)
                throw new InvalidOperationException("Failed to create SoE request");
            return new EcSoeRequest(requestPtr);
        }

        public EcVoeHandler CreateVoeHandler(uint size)
        {
            var handlerPtr = EcrtNative.ecrt_slave_config_create_voe_handler(_configPtr, (UIntPtr)size);
            if (handlerPtr == IntPtr.Zero)
                throw new InvalidOperationException("Failed to create VoE handler");
            return new EcVoeHandler(handlerPtr);
        }

        public EcRegRequest CreateRegRequest(uint size)
        {
            var requestPtr = EcrtNative.ecrt_slave_config_create_reg_request(_configPtr, (UIntPtr)size);
            if (requestPtr == IntPtr.Zero)
                throw new InvalidOperationException("Failed to create register request");
            return new EcRegRequest(requestPtr);
        }

        public EcSlaveConfigState GetState()
        {
            EcSlaveConfigState state;
            var result = EcrtNative.ecrt_slave_config_state(_configPtr, out state);
            if (result != 0)
                throw new InvalidOperationException($"Failed to get slave config state: {result}");
            return state;
        }

        public int ConfigureIdn(byte driveNo, ushort idn, EcAlState state, byte[] data)
        {
            var dataPtr = Marshal.AllocHGlobal(data.Length);
            try
            {
                Marshal.Copy(data, 0, dataPtr, data.Length);
                return EcrtNative.ecrt_slave_config_idn(_configPtr, driveNo, idn, state, dataPtr, (UIntPtr)data.Length);
            }
            finally
            {
                Marshal.FreeHGlobal(dataPtr);
            }
        }

        public int SetFlag(string key, int value)
        {
            return EcrtNative.ecrt_slave_config_flag(_configPtr, key, value);
        }

        public int SetEoeMacAddress(byte[] macAddress)
        {
            if (macAddress.Length != 6)
                throw new ArgumentException("MAC address must be 6 bytes");

            var macPtr = Marshal.AllocHGlobal(6);
            try
            {
                Marshal.Copy(macAddress, 0, macPtr, 6);
                return EcrtNative.ecrt_slave_config_eoe_mac_address(_configPtr, macPtr);
            }
            finally
            {
                Marshal.FreeHGlobal(macPtr);
            }
        }

        public int SetEoeIpAddress(uint ipAddress)
        {
            var addr = new InAddr { s_addr = ipAddress };
            return EcrtNative.ecrt_slave_config_eoe_ip_address(_configPtr, addr);
        }

        public int SetEoeSubnetMask(uint subnetMask)
        {
            var addr = new InAddr { s_addr = subnetMask };
            return EcrtNative.ecrt_slave_config_eoe_subnet_mask(_configPtr, addr);
        }

        public int SetEoeDefaultGateway(uint gatewayAddress)
        {
            var addr = new InAddr { s_addr = gatewayAddress };
            return EcrtNative.ecrt_slave_config_eoe_default_gateway(_configPtr, addr);
        }

        public int SetEoeDnsAddress(uint dnsAddress)
        {
            var addr = new InAddr { s_addr = dnsAddress };
            return EcrtNative.ecrt_slave_config_eoe_dns_address(_configPtr, addr);
        }

        public int SetEoeHostname(string hostname)
        {
            return EcrtNative.ecrt_slave_config_eoe_hostname(_configPtr, hostname);
        }

        public int SetStateTimeout(EcAlState fromState, EcAlState toState, uint timeoutMs)
        {
            return EcrtNative.ecrt_slave_config_state_timeout(_configPtr, fromState, toState, timeoutMs);
        }
    }

    /// <summary>
    /// High-level wrapper for EtherCAT SDO Request
    /// </summary>
    public class EcSdoRequest
    {
        private IntPtr _requestPtr;

        internal EcSdoRequest(IntPtr requestPtr)
        {
            _requestPtr = requestPtr;
        }

        public int SetIndex(ushort index, byte subindex)
        {
            return EcrtNative.ecrt_sdo_request_index(_requestPtr, index, subindex);
        }

        public int SetTimeout(uint timeout)
        {
            return EcrtNative.ecrt_sdo_request_timeout(_requestPtr, timeout);
        }

        public IntPtr Data => EcrtNative.ecrt_sdo_request_data(_requestPtr);

        public uint DataSize => (uint)EcrtNative.ecrt_sdo_request_data_size(_requestPtr);

        public EcRequestState State => EcrtNative.ecrt_sdo_request_state(_requestPtr);

        public int Write()
        {
            return EcrtNative.ecrt_sdo_request_write(_requestPtr);
        }

        public int Read()
        {
            return EcrtNative.ecrt_sdo_request_read(_requestPtr);
        }

        public void WriteData(byte[] data)
        {
            if (data.Length > DataSize)
                throw new ArgumentException("Data too large for request buffer");
            Marshal.Copy(data, 0, Data, data.Length);
        }

        public byte[] ReadData()
        {
            var size = (int)DataSize;
            var data = new byte[size];
            Marshal.Copy(Data, data, 0, size);
            return data;
        }
    }

    /// <summary>
    /// High-level wrapper for EtherCAT SoE Request
    /// </summary>
    public class EcSoeRequest
    {
        private IntPtr _requestPtr;

        internal EcSoeRequest(IntPtr requestPtr)
        {
            _requestPtr = requestPtr;
        }

        public int SetIdn(byte driveNo, ushort idn)
        {
            return EcrtNative.ecrt_soe_request_idn(_requestPtr, driveNo, idn);
        }

        public int SetTimeout(uint timeout)
        {
            return EcrtNative.ecrt_soe_request_timeout(_requestPtr, timeout);
        }

        public IntPtr Data => EcrtNative.ecrt_soe_request_data(_requestPtr);

        public uint DataSize => (uint)EcrtNative.ecrt_soe_request_data_size(_requestPtr);

        public EcRequestState State => EcrtNative.ecrt_soe_request_state(_requestPtr);

        public int Write()
        {
            return EcrtNative.ecrt_soe_request_write(_requestPtr);
        }

        public int Read()
        {
            return EcrtNative.ecrt_soe_request_read(_requestPtr);
        }

        public void WriteData(byte[] data)
        {
            if (data.Length > DataSize)
                throw new ArgumentException("Data too large for request buffer");
            Marshal.Copy(data, 0, Data, data.Length);
        }

        public byte[] ReadData()
        {
            var size = (int)DataSize;
            var data = new byte[size];
            Marshal.Copy(Data, data, 0, size);
            return data;
        }
    }

    /// <summary>
    /// High-level wrapper for EtherCAT VoE Handler
    /// </summary>
    public class EcVoeHandler
    {
        private IntPtr _handlerPtr;

        internal EcVoeHandler(IntPtr handlerPtr)
        {
            _handlerPtr = handlerPtr;
        }

        public int SendHeader(uint vendorId, ushort vendorType)
        {
            return EcrtNative.ecrt_voe_handler_send_header(_handlerPtr, vendorId, vendorType);
        }

        public int GetReceivedHeader(out uint vendorId, out ushort vendorType)
        {
            return EcrtNative.ecrt_voe_handler_received_header(_handlerPtr, out vendorId, out vendorType);
        }

        public IntPtr Data => EcrtNative.ecrt_voe_handler_data(_handlerPtr);

        public uint DataSize => (uint)EcrtNative.ecrt_voe_handler_data_size(_handlerPtr);

        public int Write(uint size)
        {
            return EcrtNative.ecrt_voe_handler_write(_handlerPtr, (UIntPtr)size);
        }

        public int Read()
        {
            return EcrtNative.ecrt_voe_handler_read(_handlerPtr);
        }

        public int ReadNoSync()
        {
            return EcrtNative.ecrt_voe_handler_read_nosync(_handlerPtr);
        }

        public EcRequestState Execute()
        {
            return EcrtNative.ecrt_voe_handler_execute(_handlerPtr);
        }

        public void WriteData(byte[] data)
        {
            Marshal.Copy(data, 0, Data, data.Length);
        }

        public byte[] ReadData()
        {
            var size = (int)DataSize;
            var data = new byte[size];
            Marshal.Copy(Data, data, 0, size);
            return data;
        }
    }

    /// <summary>
    /// High-level wrapper for EtherCAT Register Request
    /// </summary>
    public class EcRegRequest
    {
        private IntPtr _requestPtr;

        internal EcRegRequest(IntPtr requestPtr)
        {
            _requestPtr = requestPtr;
        }

        public IntPtr Data => EcrtNative.ecrt_reg_request_data(_requestPtr);

        public EcRequestState State => EcrtNative.ecrt_reg_request_state(_requestPtr);

        public int Write(ushort address, uint size)
        {
            return EcrtNative.ecrt_reg_request_write(_requestPtr, address, (UIntPtr)size);
        }

        public int Read(ushort address, uint size)
        {
            return EcrtNative.ecrt_reg_request_read(_requestPtr, address, (UIntPtr)size);
        }

        public void WriteData(byte[] data)
        {
            Marshal.Copy(data, 0, Data, data.Length);
        }

        public byte[] ReadData(int size)
        {
            var data = new byte[size];
            Marshal.Copy(Data, data, 0, size);
            return data;
        }
    }

    #endregion

    #region Data Access Helper Methods

    /// <summary>
    /// Helper methods for reading/writing EtherCAT data with proper endianness
    /// </summary>
    public static class EcDataAccess
    {
        // Read methods
        public static byte ReadU8(IntPtr data) => Marshal.ReadByte(data);
        public static sbyte ReadS8(IntPtr data) => (sbyte)Marshal.ReadByte(data);
        
        public static ushort ReadU16(IntPtr data)
        {
            var bytes = new byte[2];
            Marshal.Copy(data, bytes, 0, 2);
            return BitConverter.ToUInt16(bytes, 0);
        }
        
        public static short ReadS16(IntPtr data)
        {
            var bytes = new byte[2];
            Marshal.Copy(data, bytes, 0, 2);
            return BitConverter.ToInt16(bytes, 0);
        }
        
        public static uint ReadU32(IntPtr data)
        {
            var bytes = new byte[4];
            Marshal.Copy(data, bytes, 0, 4);
            return BitConverter.ToUInt32(bytes, 0);
        }
        
        public static int ReadS32(IntPtr data)
        {
            var bytes = new byte[4];
            Marshal.Copy(data, bytes, 0, 4);
            return BitConverter.ToInt32(bytes, 0);
        }
        
        public static ulong ReadU64(IntPtr data)
        {
            var bytes = new byte[8];
            Marshal.Copy(data, bytes, 0, 8);
            return BitConverter.ToUInt64(bytes, 0);
        }
        
        public static long ReadS64(IntPtr data)
        {
            var bytes = new byte[8];
            Marshal.Copy(data, bytes, 0, 8);
            return BitConverter.ToInt64(bytes, 0);
        }
        
        public static float ReadReal(IntPtr data)
        {
            return EcrtNative.ecrt_read_real(data);
        }
        
        public static double ReadLReal(IntPtr data)
        {
            return EcrtNative.ecrt_read_lreal(data);
        }
        
        public static bool ReadBit(IntPtr data, int position)
        {
            var byteValue = Marshal.ReadByte(data);
            return (byteValue & (1 << position)) != 0;
        }

        // Write methods
        public static void WriteU8(IntPtr data, byte value)
        {
            Marshal.WriteByte(data, value);
        }
        
        public static void WriteS8(IntPtr data, sbyte value)
        {
            Marshal.WriteByte(data, (byte)value);
        }
        
        public static void WriteU16(IntPtr data, ushort value)
        {
            var bytes = BitConverter.GetBytes(value);
            Marshal.Copy(bytes, 0, data, 2);
        }
        
        public static void WriteS16(IntPtr data, short value)
        {
            var bytes = BitConverter.GetBytes(value);
            Marshal.Copy(bytes, 0, data, 2);
        }
        
        public static void WriteU32(IntPtr data, uint value)
        {
            var bytes = BitConverter.GetBytes(value);
            Marshal.Copy(bytes, 0, data, 4);
        }
        
        public static void WriteS32(IntPtr data, int value)
        {
            var bytes = BitConverter.GetBytes(value);
            Marshal.Copy(bytes, 0, data, 4);
        }
        
        public static void WriteU64(IntPtr data, ulong value)
        {
            var bytes = BitConverter.GetBytes(value);
            Marshal.Copy(bytes, 0, data, 8);
        }
        
        public static void WriteS64(IntPtr data, long value)
        {
            var bytes = BitConverter.GetBytes(value);
            Marshal.Copy(bytes, 0, data, 8);
        }
        
        public static void WriteReal(IntPtr data, float value)
        {
            EcrtNative.ecrt_write_real(data, value);
        }
        
        public static void WriteLReal(IntPtr data, double value)
        {
            EcrtNative.ecrt_write_lreal(data, value);
        }
        
        public static void WriteBit(IntPtr data, int position, bool value)
        {
            var byteValue = Marshal.ReadByte(data);
            if (value)
                byteValue |= (byte)(1 << position);
            else
                byteValue &= (byte)~(1 << position);
            Marshal.WriteByte(data, byteValue);
        }

        // Array read methods
        public static byte[] ReadBytes(IntPtr data, int length)
        {
            var bytes = new byte[length];
            Marshal.Copy(data, bytes, 0, length);
            return bytes;
        }
        
        public static ushort[] ReadU16Array(IntPtr data, int count)
        {
            var result = new ushort[count];
            for (int i = 0; i < count; i++)
            {
                result[i] = ReadU16(data + i * 2);
            }
            return result;
        }
        
        public static uint[] ReadU32Array(IntPtr data, int count)
        {
            var result = new uint[count];
            for (int i = 0; i < count; i++)
            {
                result[i] = ReadU32(data + i * 4);
            }
            return result;
        }

        // Array write methods
        public static void WriteBytes(IntPtr data, byte[] values)
        {
            Marshal.Copy(values, 0, data, values.Length);
        }
        
        public static void WriteU16Array(IntPtr data, ushort[] values)
        {
            for (int i = 0; i < values.Length; i++)
            {
                WriteU16(data + i * 2, values[i]);
            }
        }
        
        public static void WriteU32Array(IntPtr data, uint[] values)
        {
            for (int i = 0; i < values.Length; i++)
            {
                WriteU32(data + i * 4, values[i]);
            }
        }
    }

    #endregion

    #region Helper Classes and Extensions

    /// <summary>
    /// Helper class for building PDO configurations
    /// </summary>
    public class PdoConfigurationBuilder
    {
        private List<EcPdoEntryInfo> _entries = new List<EcPdoEntryInfo>();
        private List<EcPdoInfo> _pdos = new List<EcPdoInfo>();
        private List<EcSyncInfo> _syncs = new List<EcSyncInfo>();

        public PdoConfigurationBuilder AddEntry(ushort index, byte subindex, byte bitLength)
        {
            _entries.Add(new EcPdoEntryInfo
            {
                index = index,
                subindex = subindex,
                bit_length = bitLength
            });
            return this;
        }

        public PdoConfigurationBuilder AddPdo(ushort index, EcPdoEntryInfo[] entries = null)
        {
            var entriesPtr = IntPtr.Zero;
            uint entryCount = 0;

            if (entries != null && entries.Length > 0)
            {
                entryCount = (uint)entries.Length;
                var size = Marshal.SizeOf<EcPdoEntryInfo>();
                entriesPtr = Marshal.AllocHGlobal(size * entries.Length);
                for (int i = 0; i < entries.Length; i++)
                {
                    Marshal.StructureToPtr(entries[i], entriesPtr + i * size, false);
                }
            }

            _pdos.Add(new EcPdoInfo
            {
                index = index,
                n_entries = entryCount,
                entries = entriesPtr
            });
            return this;
        }

        public PdoConfigurationBuilder AddSyncManager(byte index, EcDirection direction, 
            EcPdoInfo[] pdos = null, EcWatchdogMode watchdogMode = EcWatchdogMode.EC_WD_DEFAULT)
        {
            var pdosPtr = IntPtr.Zero;
            uint pdoCount = 0;

            if (pdos != null && pdos.Length > 0)
            {
                pdoCount = (uint)pdos.Length;
                var size = Marshal.SizeOf<EcPdoInfo>();
                pdosPtr = Marshal.AllocHGlobal(size * pdos.Length);
                for (int i = 0; i < pdos.Length; i++)
                {
                    Marshal.StructureToPtr(pdos[i], pdosPtr + i * size, false);
                }
            }

            _syncs.Add(new EcSyncInfo
            {
                index = index,
                dir = direction,
                n_pdos = pdoCount,
                pdos = pdosPtr,
                watchdog_mode = watchdogMode
            });
            return this;
        }

        public EcSyncInfo[] Build()
        {
            // Add terminator
            _syncs.Add(new EcSyncInfo { index = 0xFF });
            return _syncs.ToArray();
        }

        public void Dispose()
        {
            // Free allocated memory for PDO entries and PDOs
            foreach (var pdo in _pdos)
            {
                if (pdo.entries != IntPtr.Zero)
                    Marshal.FreeHGlobal(pdo.entries);
            }

            foreach (var sync in _syncs)
            {
                if (sync.pdos != IntPtr.Zero)
                    Marshal.FreeHGlobal(sync.pdos);
            }
        }
    }

    /// <summary>
    /// Extension methods for easier data access
    /// </summary>
    public static class EcDataExtensions
    {
        public static byte ReadU8(this IntPtr data, int offset = 0)
        {
            return EcDataAccess.ReadU8(data + offset);
        }

        public static ushort ReadU16(this IntPtr data, int offset = 0)
        {
            return EcDataAccess.ReadU16(data + offset);
        }

        public static uint ReadU32(this IntPtr data, int offset = 0)
        {
            return EcDataAccess.ReadU32(data + offset);
        }

        public static ulong ReadU64(this IntPtr data, int offset = 0)
        {
            return EcDataAccess.ReadU64(data + offset);
        }

        public static float ReadReal(this IntPtr data, int offset = 0)
        {
            return EcDataAccess.ReadReal(data + offset);
        }

        public static double ReadLReal(this IntPtr data, int offset = 0)
        {
            return EcDataAccess.ReadLReal(data + offset);
        }

        public static bool ReadBit(this IntPtr data, int offset, int bitPosition)
        {
            return EcDataAccess.ReadBit(data + offset, bitPosition);
        }

        public static void WriteU8(this IntPtr data, byte value, int offset = 0)
        {
            EcDataAccess.WriteU8(data + offset, value);
        }

        public static void WriteU16(this IntPtr data, ushort value, int offset = 0)
        {
            EcDataAccess.WriteU16(data + offset, value);
        }

        public static void WriteU32(this IntPtr data, uint value, int offset = 0)
        {
            EcDataAccess.WriteU32(data + offset, value);
        }

        public static void WriteU64(this IntPtr data, ulong value, int offset = 0)
        {
            EcDataAccess.WriteU64(data + offset, value);
        }

        public static void WriteReal(this IntPtr data, float value, int offset = 0)
        {
            EcDataAccess.WriteReal(data + offset, value);
        }

        public static void WriteLReal(this IntPtr data, double value, int offset = 0)
        {
            EcDataAccess.WriteLReal(data + offset, value);
        }

        public static void WriteBit(this IntPtr data, bool value, int offset, int bitPosition)
        {
            EcDataAccess.WriteBit(data + offset, bitPosition, value);
        }
    }

    /// <summary>
    /// Utility class for IP address conversion
    /// </summary>
    public static class IpAddressHelper
    {
        public static uint StringToIpAddress(string ipString)
        {
            var parts = ipString.Split('.');
            if (parts.Length != 4)
                throw new ArgumentException("Invalid IP address format");

            uint result = 0;
            for (int i = 0; i < 4; i++)
            {
                if (!byte.TryParse(parts[i], out byte part))
                    throw new ArgumentException("Invalid IP address format");
                result |= (uint)(part << (i * 8));
            }
            return result;
        }

        public static string IpAddressToString(uint ipAddress)
        {
            return $"{ipAddress & 0xFF}.{(ipAddress >> 8) & 0xFF}.{(ipAddress >> 16) & 0xFF}.{(ipAddress >> 24) & 0xFF}";
        }

        public static InAddr CreateInAddr(string ipString)
        {
            return new InAddr { s_addr = StringToIpAddress(ipString) };
        }
    }

    #endregion

    #region Exception Classes

    /// <summary>
    /// Base exception for EtherCAT operations
    /// </summary>
    public class EtherCatException : Exception
    {
        public int ErrorCode { get; }

        public EtherCatException(string message) : base(message) { }
        public EtherCatException(string message, Exception innerException) : base(message, innerException) { }
        public EtherCatException(string message, int errorCode) : base(message)
        {
            ErrorCode = errorCode;
        }
    }

    /// <summary>
    /// Exception thrown when master operations fail
    /// </summary>
    public class EtherCatMasterException : EtherCatException
    {
        public EtherCatMasterException(string message) : base(message) { }
        public EtherCatMasterException(string message, int errorCode) : base(message, errorCode) { }
    }

    /// <summary>
    /// Exception thrown when slave operations fail
    /// </summary>
    public class EtherCatSlaveException : EtherCatException
    {
        public ushort SlavePosition { get; }

        public EtherCatSlaveException(string message, ushort slavePosition) : base(message)
        {
            SlavePosition = slavePosition;
        }
        public EtherCatSlaveException(string message, ushort slavePosition, int errorCode) : base(message, errorCode)
        {
            SlavePosition = slavePosition;
        }
    }

    /// <summary>
    /// Exception thrown when SDO operations fail
    /// </summary>
    public class EtherCatSdoException : EtherCatSlaveException
    {
        public ushort Index { get; }
        public byte Subindex { get; }
        public uint AbortCode { get; }

        public EtherCatSdoException(string message, ushort slavePosition, ushort index, byte subindex, uint abortCode) 
            : base(message, slavePosition)
        {
            Index = index;
            Subindex = subindex;
            AbortCode = abortCode;
        }
    }

    #endregion

    #region Version Information

    /// <summary>
    /// EtherCAT library version information
    /// </summary>
    public static class EcrtVersion
    {
        public static uint GetVersionMagic()
        {
            return EcrtNative.ecrt_version_magic();
        }

        public static bool IsCompatible()
        {
            return GetVersionMagic() == EcrtConstants.ECRT_VERSION_MAGIC;
        }

        public static (int major, int minor) GetVersion()
        {
            var magic = GetVersionMagic();
            return ((int)(magic >> 8), (int)(magic & 0xFF));
        }

        public static string GetVersionString()
        {
            var (major, minor) = GetVersion();
            return $"{major}.{minor}";
        }
    }

    #endregion

    #region Async Wrappers

    /// <summary>
    /// Async wrapper for SDO operations
    /// </summary>
    public static class EcrtAsync
    {
        public static async Task<byte[]> SdoUploadAsync(EcMaster master, ushort slavePosition, 
            ushort index, byte subindex, int bufferSize = 1024, CancellationToken cancellationToken = default)
        {
            return await Task.Run(() =>
            {
                var buffer = new byte[bufferSize];
                uint abortCode;
                var result = master.SdoUpload(slavePosition, index, subindex, buffer, out int resultSize, out abortCode);
                
                if (result != 0)
                    throw new EtherCatSdoException($"SDO upload failed: {result}", slavePosition, index, subindex, abortCode);
                
                var data = new byte[resultSize];
                Array.Copy(buffer, data, resultSize);
                return data;
            }, cancellationToken);
        }

        public static async Task SdoDownloadAsync(EcMaster master, ushort slavePosition, 
            ushort index, byte subindex, byte[] data, CancellationToken cancellationToken = default)
        {
            await Task.Run(() =>
            {
                uint abortCode;
                var result = master.SdoDownload(slavePosition, index, subindex, data, out abortCode);
                
                if (result != 0)
                    throw new EtherCatSdoException($"SDO download failed: {result}", slavePosition, index, subindex, abortCode);
            }, cancellationToken);
        }

        public static async Task<T> WaitForRequestAsync<T>(Func<EcRequestState> getState, 
            Func<T> getResult, TimeSpan timeout, CancellationToken cancellationToken = default)
        {
            var startTime = DateTime.UtcNow;
            
            while (DateTime.UtcNow - startTime < timeout)
            {
                cancellationToken.ThrowIfCancellationRequested();
                
                var state = getState();
                switch (state)
                {
                    case EcRequestState.EC_REQUEST_SUCCESS:
                        return getResult();
                    case EcRequestState.EC_REQUEST_ERROR:
                        throw new EtherCatException("Request failed");
                    case EcRequestState.EC_REQUEST_BUSY:
                        await Task.Delay(1, cancellationToken);
                        break;
                    case EcRequestState.EC_REQUEST_UNUSED:
                        throw new EtherCatException("Request not started");
                }
            }
            
            throw new TimeoutException("Request timed out");
        }
    }

    #endregion

    #region Configuration Helpers

    /// <summary>
    /// Helper class for common EtherCAT configurations
    /// </summary>
    public static class EcrtConfigHelper
    {
        /// <summary>
        /// Create a simple digital input configuration
        /// </summary>
        public static EcSyncInfo[] CreateDigitalInputConfig(ushort pdoIndex, params (ushort index, byte subindex, byte bits)[] entries)
        {
            var entryInfos = entries.Select(e => new EcPdoEntryInfo
            {
                index = e.index,
                subindex = e.subindex,
                bit_length = e.bits
            }).ToArray();

            return CreateSyncConfig(3, EcDirection.EC_DIR_INPUT, pdoIndex, entryInfos);
        }

        /// <summary>
        /// Create a simple digital output configuration
        /// </summary>
        public static EcSyncInfo[] CreateDigitalOutputConfig(ushort pdoIndex, params (ushort index, byte subindex, byte bits)[] entries)
        {
            var entryInfos = entries.Select(e => new EcPdoEntryInfo
            {
                index = e.index,
                subindex = e.subindex,
                bit_length = e.bits
            }).ToArray();

            return CreateSyncConfig(2, EcDirection.EC_DIR_OUTPUT, pdoIndex, entryInfos);
        }

        /// <summary>
        /// Create a simple analog input configuration
        /// </summary>
        public static EcSyncInfo[] CreateAnalogInputConfig(ushort pdoIndex, params (ushort index, byte subindex)[] entries)
        {
            var entryInfos = entries.Select(e => new EcPdoEntryInfo
            {
                index = e.index,
                subindex = e.subindex,
                bit_length = 16 // Assume 16-bit analog values
            }).ToArray();

            return CreateSyncConfig(3, EcDirection.EC_DIR_INPUT, pdoIndex, entryInfos);
        }

        private static EcSyncInfo[] CreateSyncConfig(byte syncIndex, EcDirection direction, ushort pdoIndex, EcPdoEntryInfo[] entries)
        {
            var entriesSize = Marshal.SizeOf<EcPdoEntryInfo>();
            var entriesPtr = Marshal.AllocHGlobal(entriesSize * entries.Length);
            
            for (int i = 0; i < entries.Length; i++)
            {
                Marshal.StructureToPtr(entries[i], entriesPtr + i * entriesSize, false);
            }

            var pdo = new EcPdoInfo
            {
                index = pdoIndex,
                n_entries = (uint)entries.Length,
                entries = entriesPtr
            };

            var pdoSize = Marshal.SizeOf<EcPdoInfo>();
            var pdoPtr = Marshal.AllocHGlobal(pdoSize);
            Marshal.StructureToPtr(pdo, pdoPtr, false);

            return new EcSyncInfo[]
            {
                new EcSyncInfo
                {
                    index = syncIndex,
                    dir = direction,
                    n_pdos = 1,
                    pdos = pdoPtr,
                    watchdog_mode = EcWatchdogMode.EC_WD_DEFAULT
                },
                new EcSyncInfo { index = 0xFF } // Terminator
            };
        }

        /// <summary>
        /// Common distributed clocks configuration for synchronous operation
        /// </summary>
        public static void ConfigureDistributedClocks(EcSlaveConfig slaveConfig, uint cycleTimeNs, int shiftTimeNs = 0)
        {
            // Common DC configuration: SYNC0 enabled, SYNC1 disabled
            const ushort ASSIGN_ACTIVATE = 0x0300; // Enable SYNC0
            
            var result = slaveConfig.ConfigureDistributedClocks(
                ASSIGN_ACTIVATE, 
                cycleTimeNs, 
                shiftTimeNs, 
                0, // SYNC1 cycle time (disabled)
                0  // SYNC1 shift time
            );
            
            if (result != 0)
                throw new EtherCatException($"Failed to configure distributed clocks: {result}");
        }

        /// <summary>
        /// Configure common EoE (Ethernet over EtherCAT) settings
        /// </summary>
        public static void ConfigureEoE(EcSlaveConfig slaveConfig, string ipAddress, string subnetMask, 
            string gateway = null, string dns = null, string hostname = null, byte[] macAddress = null)
        {
            if (macAddress != null)
            {
                var result = slaveConfig.SetEoeMacAddress(macAddress);
                if (result != 0)
                    throw new EtherCatException($"Failed to set EoE MAC address: {result}");
            }

            var ipResult = slaveConfig.SetEoeIpAddress(IpAddressHelper.StringToIpAddress(ipAddress));
            if (ipResult != 0)
                throw new EtherCatException($"Failed to set EoE IP address: {ipResult}");

            var maskResult = slaveConfig.SetEoeSubnetMask(IpAddressHelper.StringToIpAddress(subnetMask));
            if (maskResult != 0)
                throw new EtherCatException($"Failed to set EoE subnet mask: {maskResult}");

            if (!string.IsNullOrEmpty(gateway))
            {
                var gwResult = slaveConfig.SetEoeDefaultGateway(IpAddressHelper.StringToIpAddress(gateway));
                if (gwResult != 0)
                    throw new EtherCatException($"Failed to set EoE gateway: {gwResult}");
            }

            if (!string.IsNullOrEmpty(dns))
            {
                var dnsResult = slaveConfig.SetEoeDnsAddress(IpAddressHelper.StringToIpAddress(dns));
                if (dnsResult != 0)
                    throw new EtherCatException($"Failed to set EoE DNS: {dnsResult}");
            }

            if (!string.IsNullOrEmpty(hostname))
            {
                var hostResult = slaveConfig.SetEoeHostname(hostname);
                if (hostResult != 0)
                    throw new EtherCatException($"Failed to set EoE hostname: {hostResult}");
            }
        }
    }

    #endregion

    #region Logging and Diagnostics

    /// <summary>
    /// Diagnostic helper for EtherCAT operations
    /// </summary>
    public static class EcrtDiagnostics
    {
        public static string GetMasterStateString(EcMasterState state)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Slaves responding: {state.slaves_responding}");
            sb.AppendLine($"Link up: {state.link_up}");
            sb.AppendLine("AL States:");
            if ((state.al_states & 1) != 0) sb.AppendLine("  - INIT");
            if ((state.al_states & 2) != 0) sb.AppendLine("  - PREOP");
            if ((state.al_states & 4) != 0) sb.AppendLine("  - SAFEOP");
            if ((state.al_states & 8) != 0) sb.AppendLine("  - OP");
            return sb.ToString();
        }

        public static string GetSlaveConfigStateString(EcSlaveConfigState state)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Online: {state.online}");
            sb.AppendLine($"Operational: {state.operational}");
            sb.AppendLine($"AL State: {GetAlStateString((EcAlState)state.al_state)}");
            return sb.ToString();
        }

        public static string GetDomainStateString(EcDomainState state)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Working counter: {state.working_counter}");
            sb.AppendLine($"WC State: {state.wc_state}");
            sb.AppendLine($"Redundancy active: {state.redundancy_active != 0}");
            return sb.ToString();
        }

        public static string GetAlStateString(EcAlState state)
        {
            return state switch
            {
                EcAlState.EC_AL_STATE_INIT => "INIT",
                EcAlState.EC_AL_STATE_PREOP => "PREOP",
                EcAlState.EC_AL_STATE_SAFEOP => "SAFEOP",
                EcAlState.EC_AL_STATE_OP => "OP",
                _ => $"Unknown ({(int)state})"
            };
        }

        public static string GetRequestStateString(EcRequestState state)
        {
            return state switch
            {
                EcRequestState.EC_REQUEST_UNUSED => "UNUSED",
                EcRequestState.EC_REQUEST_BUSY => "BUSY",
                EcRequestState.EC_REQUEST_SUCCESS => "SUCCESS",
                EcRequestState.EC_REQUEST_ERROR => "ERROR",
                _ => $"Unknown ({(int)state})"
            };
        }

        public static void LogSlaveInfo(EcSlaveInfo info, Action<string> logger)
        {
            logger($"Slave {info.position}:");
            logger($"  Vendor ID: 0x{info.vendor_id:X8}");
            logger($"  Product Code: 0x{info.product_code:X8}");
            logger($"  Revision: 0x{info.revision_number:X8}");
            logger($"  Serial: 0x{info.serial_number:X8}");
            logger($"  Alias: {info.alias}");
            logger($"  Name: {info.name}");
            logger($"  AL State: {GetAlStateString((EcAlState)info.al_state)}");
            logger($"  Error Flag: {info.error_flag}");
            logger($"  Current on E-Bus: {info.current_on_ebus} mA");
            logger($"  Sync Managers: {info.sync_count}");
            logger($"  SDOs: {info.sdo_count}");
        }
    }

    #endregion
}
