using System;
using System.Threading;
using ecm_wrapper;

class Program
{
    static void Main(string[] args)
    {
        try
        {
            // Check EtherCAT library version compatibility
            if (!EcrtVersion.IsCompatible())
            {
                Console.WriteLine($"Version mismatch! Expected: {EcrtConstants.ECRT_VER_MAJOR}.{EcrtConstants.ECRT_VER_MINOR}, " +
                                $"Got: {EcrtVersion.GetVersionString()}");
                return;
            }

            Console.WriteLine($"EtherCAT RT Library Version: {EcrtVersion.GetVersionString()}");

            // Create and configure EtherCAT master
            using (var master = new EcMaster(0)) // Master index 0
            {
                Console.WriteLine("EtherCAT Master created successfully");

                // Reserve the master
                if (master.Reserve() != 0)
                {
                    Console.WriteLine("Failed to reserve master");
                    return;
                }

                // Create a domain for process data
                var domain = master.CreateDomain();
                Console.WriteLine("Domain created");

                // Configure a slave (example: Beckhoff EL1008 digital input terminal)
                var slave = master.SlaveConfig(
                    alias: 0,           // Alias address
                    position: 0,        // Position on bus
                    vendorId: 0x00000002,   // Beckhoff vendor ID
                    productCode: 0x03F03F82  // EL1008 product code
                );

                // Configure PDOs for digital inputs
                var pdoConfig = EcrtConfigHelper.CreateDigitalInputConfig(
                    pdoIndex: 0x1A00,
                    (0x6000, 0x01, 1), // Channel 1
                    (0x6010, 0x01, 1), // Channel 2
                    (0x6020, 0x01, 1), // Channel 3
                    (0x6030, 0x01, 1)  // Channel 4
                );

                if (slave.ConfigurePdos(pdoConfig) != 0)
                {
                    Console.WriteLine("Failed to configure PDOs");
                    return;
                }

                // Register PDO entries
                uint bitPos1, bitPos2, bitPos3, bitPos4;
                slave.RegisterPdoEntry(0x6000, 0x01, domain, out bitPos1);
                slave.RegisterPdoEntry(0x6010, 0x01, domain, out bitPos2);
                slave.RegisterPdoEntry(0x6020, 0x01, domain, out bitPos3);
                slave.RegisterPdoEntry(0x6030, 0x01, domain, out bitPos4);

                Console.WriteLine($"PDO entries registered at bit positions: {bitPos1}, {bitPos2}, {bitPos3}, {bitPos4}");

                // Activate the master
                if (master.Activate() != 0)
                {
                    Console.WriteLine("Failed to activate master");
                    return;
                }

                Console.WriteLine("Master activated successfully");

                // Get domain data pointer
                var domainData = domain.Data;
                var domainSize = domain.Size;
                Console.WriteLine($"Domain size: {domainSize} bytes");

                // Main cyclic loop
                Console.WriteLine("Starting cyclic operation (Press any key to stop)...");
                int cycleCount = 0;

                while (!Console.KeyAvailable)
                {
                    // Receive process data
                    master.Receive();
                    domain.Process();

                    // Read input states
                    bool input1 = domainData.ReadBit((int)(bitPos1 / 8), (int)(bitPos1 % 8));
                    bool input2 = domainData.ReadBit((int)(bitPos2 / 8), (int)(bitPos2 % 8));
                    bool input3 = domainData.ReadBit((int)(bitPos3 / 8), (int)(bitPos3 % 8));
                    bool input4 = domainData.ReadBit((int)(bitPos4 / 8), (int)(bitPos4 % 8));

                    // Display input states every 1000 cycles
                    if (cycleCount % 1000 == 0)
                    {
                        Console.WriteLine($"Cycle {cycleCount}: Inputs = [{(input1 ? "1" : "0")}, {(input2 ? "1" : "0")}, {(input3 ? "1" : "0")}, {(input4 ? "1" : "0")}]");

                        // Get and display master state
                        var masterState = master.GetState();
                        Console.WriteLine($"Master state: {masterState.slaves_responding} slaves responding, Link: {(masterState.link_up ? "UP" : "DOWN")}");

                        // Get and display slave state
                        var slaveState = slave.GetState();
                        Console.WriteLine($"Slave state: Online={slaveState.online}, Operational={slaveState.operational}");

                        // Get and display domain state
                        var domainState = domain.GetState();
                        Console.WriteLine($"Domain state: WC={domainState.working_counter}, State={domainState.wc_state}");
                    }

                    // Send process data
                    domain.Queue();
                    master.Send();

                    cycleCount++;

                    // Sleep for 1ms (1kHz cycle)
                    Thread.Sleep(1);
                }

                Console.WriteLine("Stopping cyclic operation...");

                // Deactivate master
                master.Deactivate();
                Console.WriteLine("Master deactivated");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            if (ex is EtherCatException ecEx)
            {
                Console.WriteLine($"EtherCAT Error Code: {ecEx.ErrorCode}");
            }
        }

        Console.WriteLine("Press any key to exit...");
        Console.ReadKey();
    }
}
