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
                /* if (master.Reserve() != 0)
                {
                    Console.WriteLine("Failed to reserve master");
                    return;
                } */

                // Create a domain for process data
                var domain = master.CreateDomain();
                Console.WriteLine("Domain created");

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
                
                Dictionary<ushort, EcSlaveInfo> Slaves = new Dictionary<ushort, EcSlaveInfo>();

                for (uint position = 0; position < masterState.slaves_responding; position++) // Check first 64 positions
                {
                    try
                    {
                        // Try to configure a slave at this position
                        // We use generic vendor/product IDs for discovery
                        var slaveInfo = master.GetSlaveInfo((ushort)position);
                        Slaves.Add((ushort)position, slaveInfo);

                        // Try to read slave information (this might require SDO access)
                        Console.WriteLine($"{slaveInfo.position,8} | {slaveInfo.alias,5} | 0x{slaveInfo.vendor_id:X8} | 0x{slaveInfo.product_code:X8} | Online={slaveInfo.al_state}");
                    }
                    catch (Exception ex)
                    {
                        // Slave not present at this position, continue
                        continue;
                    }
                }

                Dictionary<ushort, EcSlaveConfig> SlaveConfigs = new Dictionary<ushort, EcSlaveConfig>();
                foreach (var slaveInfo in Slaves)
                {
                    var slave = master.SlaveConfig(
                        alias: 0,           // Alias address
                        position: slaveInfo.Key,        // Position on bus
                        vendorId: slaveInfo.Value.vendor_id,   // Beckhoff vendor ID
                        productCode: slaveInfo.Value.product_code
                    );

                    var inputPdos = EcPdoEntry.GiveInputPdos(slaveInfo.Value.product_code);
                    var outputPdos = EcPdoEntry.GiveOutputPdos(slaveInfo.Value.product_code);

                    if (inputPdos.Count > 0)
                    {
                        var pdoConfig = EcrtConfigHelper.CreateDigitalInputConfig(
                            pdoIndex: 0x1A00,
                            EcPdoEntry.EcPdoEntryInfoToParams(inputPdos)
                        );

                        if (slave.ConfigurePdos(pdoConfig) != 0)
                        {
                            Console.WriteLine("Failed to configure PDOs 2");
                            return;
                        }
                    }

                    if (outputPdos.Count > 0)
                    {
                        var pdoConfig = EcrtConfigHelper.CreateDigitalInputConfig(
                            pdoIndex: 0x1600,
                            EcPdoEntry.EcPdoEntryInfoToParams(outputPdos)
                        );

                        if (slave.ConfigurePdos(pdoConfig) != 0)
                        {
                            Console.WriteLine("Failed to configure PDOs 2");
                            return;
                        }
                    }

                    SlaveConfigs.Add(slaveInfo.Key, slave);
                }

                

                // EK1100
                /* var slave1 = master.SlaveConfig(
                    alias: 0,           // Alias address
                    position: 0,        // Position on bus
                    vendorId: 0x00000002,   // Beckhoff vendor ID
                    productCode: 0x044C2C52 
                ); */

                //slave1.ConfigureSyncManager(0, EcDirection.EC_DIR_INVALID, EcWatchdogMode.EC_WD_DISABLE);

                //keine syncconfig für ek1100

                //EL1008
                /* var slave2 = master.SlaveConfig(
                    alias: 0,           // Alias address 36820
                    position: 1,        // Position on bus
                    vendorId: 0x00000002,   // Beckhoff vendor ID
                    productCode: 0x03F03052  // EL1008 product code
                );

                //slave2.ConfigureSyncManager(3, EcDirection.EC_DIR_INVALID, EcWatchdogMode.EC_WD_DISABLE);

                var pdoConfig2 = EcrtConfigHelper.CreateDigitalInputConfig(
                    pdoIndex: 0x1A00,
                    (0x6000, 0x01, 1), // Channel 1
                    (0x6010, 0x01, 1), // Channel 2
                    (0x6020, 0x01, 1), // Channel 3
                    (0x6030, 0x01, 1), // Channel 4
                    (0x6040, 0x01, 1), // Channel 1
                    (0x6050, 0x01, 1), // Channel 2
                    (0x6060, 0x01, 1), // Channel 3
                    (0x6070, 0x01, 1)  // Channel 4
                );

                if (slave2.ConfigurePdos(pdoConfig2) != 0)
                {
                    Console.WriteLine("Failed to configure PDOs 2");
                    return;
                } */

                //EL1008
                /* var slave3 = master.SlaveConfig(
                    alias: 0,           // Alias address 40417
                    position: 2,        // Position on bus
                    vendorId: 0x00000002,   // Beckhoff vendor ID
                    productCode: 0x03F03052  // EL1008 product code
                );

                //slave3.ConfigureSyncManager(3, EcDirection.EC_DIR_INVALID, EcWatchdogMode.EC_WD_DISABLE);

                var pdoConfig3 = EcrtConfigHelper.CreateDigitalInputConfig(
                    pdoIndex: 0x1A00,
                    (0x6000, 0x01, 1), // Channel 1
                    (0x6010, 0x01, 1), // Channel 2
                    (0x6020, 0x01, 1), // Channel 3
                    (0x6030, 0x01, 1), // Channel 4
                    (0x6040, 0x01, 1), // Channel 1
                    (0x6050, 0x01, 1), // Channel 2
                    (0x6060, 0x01, 1), // Channel 3
                    (0x6070, 0x01, 1)  // Channel 4
                );

                if (slave3.ConfigurePdos(pdoConfig3) != 0)
                {
                    Console.WriteLine("Failed to configure PDOs 3");
                    return;
                }

                //EL2008
                var slave4 = master.SlaveConfig(
                    alias: 0,           // Alias address 13357
                    position: 3,        // Position on bus
                    vendorId: 0x00000002,   // Beckhoff vendor ID 0x00000002
                    productCode: 0x07D83052  // EL2008 product code 0x07D83052
                ); */

                //slave4.ConfigureSyncManager(2, EcDirection.EC_DIR_INVALID, EcWatchdogMode.EC_WD_DISABLE);

                /* var pdoConfig4 = EcrtConfigHelper.CreateDigitalOutputConfig(
                    pdoIndex: 0x1600,
                    (0x7000, 0x01, 1),
                    (0x7010, 0x01, 1),
                    (0x7020, 0x01, 1),
                    (0x7030, 0x01, 1),
                    (0x7040, 0x01, 1),
                    (0x7050, 0x01, 1),
                    (0x7060, 0x01, 1),
                    (0x7070, 0x01, 1)
                ); */

                /* var pdoConfig4 = EcrtConfigHelper.CreateDigitalOutputConfig(
                    pdoIndex: 0x1600,
                    (0x7000, 0x01, 1),
                    (0x7010, 0x01, 1),
                    (0x7020, 0x01, 1),
                    (0x7030, 0x01, 1)
                ); */

                /* var pdoConfig4_ = EcrtConfigHelper.CreateDigitalOutputConfig(
                    pdoIndex: 0x1600,
                    (0x7040, 0x01, 1),
                    (0x7050, 0x01, 1),
                    (0x7060, 0x01, 1),
                    (0x7070, 0x01, 1)
                ); */

                /* var pdoConfig4_ = EcrtConfigHelper.CreateDigitalOutputConfig(
                    pdoIndex: 0x1601,
                    (0x7040, 0x01, 1),
                    (0x7050, 0x01, 1),
                    (0x7060, 0x01, 1),
                    (0x7070, 0x01, 1)
                ); */ //nachträgliches hinzufügen funktioninert, wenn der pdoindex anders gewählt wird. ein mindestabstand scheint nicht notwendig zu sen.

                /* var pdoConfig4_ = EcrtConfigHelper.CreateDigitalOutputConfig(
                    pdoIndex: 0x1604,
                    (0x7040, 0x01, 1),
                    (0x7050, 0x01, 1),
                    (0x7060, 0x01, 1),
                    (0x7070, 0x01, 1)
                ); */

               /*  if (slave4.ConfigurePdos(pdoConfig4) != 0)
                {
                    Console.WriteLine("Failed to configure PDOs 4");
                    return;
                }

                if (slave4.ConfigurePdos(pdoConfig4_) != 0)
                {
                    Console.WriteLine("Failed to configure PDOs 4");
                    return;
                }

                //EL6731
                var slave5 = master.SlaveConfig(
                    alias: 0,           // Alias address 2074
                    position: 4,        // Position on bus
                    vendorId: 0x00000002,   // Beckhoff vendor ID
                    productCode: 0x1A4B3052  // EL1008 product code
                ); */

                //slave5.ConfigureSyncManager(2, EcDirection.EC_DIR_INVALID, EcWatchdogMode.EC_WD_DISABLE);

                /* var pdoConfig5 = EcrtConfigHelper.CreateDigitalOutputConfig(
                    pdoIndex: 0x1600,
                    (0x7000, 0x01, 1), // Channel 1
                    (0x7010, 0x01, 1), // Channel 2
                    (0x7020, 0x01, 1), // Channel 3
                    (0x7030, 0x01, 1),  // Channel 4
                    (0x7040, 0x01, 1), // Channel 1
                    (0x7050, 0x01, 1), // Channel 2
                    (0x7060, 0x01, 1), // Channel 3
                    (0x7070, 0x01, 1)  // Channel 4
                );

                if (slave5.ConfigurePdos(pdoConfig5) != 0)
                {
                    Console.WriteLine("Failed to configure PDOs 5");
                    return;
                } */

                /* var slave6 = master.SlaveConfig(
                    alias: 0,           // Alias address 42384
                    position: 5,        // Position on bus
                    vendorId: 0x00000002,   // Beckhoff vendor ID
                    productCode: 0x04562C52  // EL1008 product code
                ); */

                //slave6.ConfigureSyncManager(0, EcDirection.EC_DIR_INVALID, EcWatchdogMode.EC_WD_DISABLE);

                //keine syncconfig für ek1200

                //procudt code EL7031: 0x1B773052
                /* var pdoConfigEL7031 = new EcPdoConfig
                {
                    PdoIndex = 0x1600,
                    Entries = new[]
                    {
                        new EcPdoEntry(0x7000, 0x01, 16), // Control word
                        new EcPdoEntry(0x7000, 0x02, 32), // Velocity setpoint  
                        new EcPdoEntry(0x7000, 0x03, 32), // Position setpoint
                        new EcPdoEntry(0x7000, 0x11, 16), // Status word
                        new EcPdoEntry(0x7000, 0x12, 32), // Position feedback
                        new EcPdoEntry(0x7000, 0x13, 32), // Velocity feedback
                        new EcPdoEntry(0x7000, 0x14, 16)  // Following error
                    }
                }; */

                /* var pdoConfigInputsEL7031 = EcrtConfigHelper.CreateDigitalInputConfig(
                    pdoIndex: 0x1A00,
                    (0x7000, 0x11, 16), // Status word
                    (0x7000, 0x12, 32), // Position feedback
                    (0x7000, 0x13, 32), // Velocity feedback
                    (0x7000, 0x14, 16)  // Following error
                ); */

                //domain.RegisterPdoEntryList()

                // Register PDO entries
                uint bitPos1, bitPos2, bitPos3, bitPos4, bitPos5, bitPos6, bitPos7, bitPos8;
                Console.WriteLine(SlaveConfigs[3].RegisterPdoEntry(0x7000, 0x01, domain, out bitPos1));
                Console.WriteLine(SlaveConfigs[3].RegisterPdoEntry(0x7010, 0x01, domain, out bitPos2));
                Console.WriteLine(SlaveConfigs[3].RegisterPdoEntry(0x7020, 0x01, domain, out bitPos3));
                Console.WriteLine(SlaveConfigs[3].RegisterPdoEntry(0x7030, 0x01, domain, out bitPos4));
                Console.WriteLine(SlaveConfigs[3].RegisterPdoEntry(0x7040, 0x01, domain, out bitPos5));
                Console.WriteLine(SlaveConfigs[3].RegisterPdoEntry(0x7050, 0x01, domain, out bitPos6));
                Console.WriteLine(SlaveConfigs[3].RegisterPdoEntry(0x7060, 0x01, domain, out bitPos7));
                Console.WriteLine(SlaveConfigs[3].RegisterPdoEntry(0x7070, 0x01, domain, out bitPos8));

                Console.WriteLine($"PDO entries registered at bit positions: {bitPos1}, {bitPos2}, {bitPos3}, {bitPos4}, {bitPos5}, {bitPos6}, {bitPos7}, {bitPos8}");

                // Activate the master
                if (master.Activate() != 0)
                {
                    Console.WriteLine("Failed to activate master");
                    return;
                }

                Console.WriteLine("Master activated successfully");

                /* for (ushort i = 0; i < 6; ++i)
                {
                    master.RequestSlaveState(i, EcAlState.EC_AL_STATE_OP);
                } */

                //slave1.RequestState(EcAlState.EC_AL_STATE_OP);

                /* slave1.SetStateTimeout(EcAlState.EC_AL_STATE_PREOP, EcAlState.EC_AL_STATE_OP, 5000);
                slave2.SetStateTimeout(EcAlState.EC_AL_STATE_PREOP, EcAlState.EC_AL_STATE_OP, 5000);
                slave3.SetStateTimeout(EcAlState.EC_AL_STATE_PREOP, EcAlState.EC_AL_STATE_OP, 5000);
                slave4.SetStateTimeout(EcAlState.EC_AL_STATE_PREOP, EcAlState.EC_AL_STATE_OP, 5000);
                slave5.SetStateTimeout(EcAlState.EC_AL_STATE_PREOP, EcAlState.EC_AL_STATE_OP, 5000);
                slave6.SetStateTimeout(EcAlState.EC_AL_STATE_PREOP, EcAlState.EC_AL_STATE_OP, 5000); */

                // Get domain data pointer
                var domainData = domain.Data;
                var domainSize = domain.Size;
                Console.WriteLine($"Domain size: {domainSize} bytes");

                

                // Main cyclic loop
                Console.WriteLine("Starting cyclic operation (Press any key to stop)...");
                int cycleCount = 0;

                var value = false;

                var liste = new List<uint>()
                {
                    bitPos1, bitPos2, bitPos3, bitPos4, bitPos5, bitPos6, bitPos7, bitPos8
                };
                
                int i = 0;

                while (!Console.KeyAvailable)
                {
                    // Receive process data
                    master.Receive();
                    domain.Process();

                    // Display input states every 1000 cycles
                    if (cycleCount % 50 == 0)
                    {
                        domainData.WriteBit(true, (int)(liste[7] / 8), (int)(liste[i] % 8));

                        if (i == 7)
                        {
                            i = 0;
                        }
                        else
                        {
                            ++i;
                        }

                        domainData.WriteBit(false, (int)(liste[i] / 8), (int)(liste[i] % 8));

                        /*// Get and display master state
                        var masterState = master.GetState();
                        Console.WriteLine($"Master state: {masterState.slaves_responding} slaves responding, Link: {(masterState.link_up ? "UP" : "DOWN")}");

                        // Get and display slave state
                        var slaveState = slave1.GetState();
                        Console.WriteLine($"Slave state: Online={slaveState.online}, Operational={slaveState.operational}");
                        slaveState = slave2.GetState();
                        Console.WriteLine($"Slave state: Online={slaveState.online}, Operational={slaveState.operational}");
                        slaveState = slave3.GetState();
                        Console.WriteLine($"Slave state: Online={slaveState.online}, Operational={slaveState.operational}");
                        slaveState = slave4.GetState();
                        Console.WriteLine($"Slave state: Online={slaveState.online}, Operational={slaveState.operational}");
                        slaveState = slave5.GetState();
                        Console.WriteLine($"Slave state: Online={slaveState.online}, Operational={slaveState.operational}");
                        slaveState = slave6.GetState();
                        Console.WriteLine($"Slave state: Online={slaveState.online}, Operational={slaveState.operational}");

                        // Get and display domain state
                        var domainState = domain.GetState();
                        Console.WriteLine($"Domain state: WC={domainState.working_counter}, State={domainState.wc_state}");
                         */
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
