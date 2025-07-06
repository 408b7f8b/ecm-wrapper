using System;
using ecm_wrapper;

class DiscoveryProgram
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

            // Create EtherCAT master for discovery
            using (var master = new EcMaster(0))
            {
                Console.WriteLine("EtherCAT Master created for device discovery");

                // Reserve the master
                /* if (master.Reserve() != 0)
                {
                    Console.WriteLine("Failed to reserve master");
                    return;
                } */

                // Activate master to scan the bus
                if (master.Activate() != 0)
                {
                    Console.WriteLine("Failed to activate master");
                    return;
                }

                Console.WriteLine("Scanning EtherCAT bus for devices...\n");

                // Get master state to see how many slaves are present
                var masterState = master.GetState();
                Console.WriteLine($"Master State:");
                Console.WriteLine($"  Slaves responding: {masterState.slaves_responding}");
                Console.WriteLine($"  Link status: {(masterState.link_up ? "UP" : "DOWN")}");
                Console.WriteLine($"  AL states: 0x{masterState.al_states:X}");
                Console.WriteLine();

                // Discover devices by trying different positions
                Console.WriteLine("Discovered EtherCAT Slaves:");
                Console.WriteLine("Position | Alias | Vendor ID | Product Code | State");
                Console.WriteLine("---------|-------|-----------|--------------|-------");

                for (uint position = 0; position < masterState.slaves_responding; position++) // Check first 64 positions
                {
                    try
                    {
                        // Try to configure a slave at this position
                        // We use generic vendor/product IDs for discovery
                        var slaveInfo = master.GetSlaveInfo((ushort)position);
                        
                        // Try to read slave information (this might require SDO access)
                        Console.WriteLine($"{slaveInfo.position,8} | {slaveInfo.alias,5} | 0x{slaveInfo.vendor_id:X8} | 0x{slaveInfo.product_code:X8} | Online={slaveInfo.al_state}");
                    }
                    catch (Exception ex)
                    {
                        // Slave not present at this position, continue
                        continue;
                    }
                }
                // Deactivate master
                master.Deactivate();
                Console.WriteLine("\nMaster deactivated");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Discovery Error: {ex.Message}");
            if (ex is EtherCatException ecEx)
            {
                Console.WriteLine($"EtherCAT Error Code: {ecEx.ErrorCode}");
            }
        }

        Console.WriteLine("\nPress any key to exit...");
        Console.ReadKey();
    }
}
