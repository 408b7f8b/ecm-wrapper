using System;
using System.Collections.Generic;

namespace ecm_wrapper
{
    public class EcPdoConfig
    {
        public ushort PdoIndex;
        public List<EcPdoEntry> Entries;
    }

    public class EcPdoEntry
    {
        public ushort index;
        public byte subindex;
        public byte bits;
        public EcDirection dir;

        public EcPdoEntry(ushort index, byte subindex, byte bits, EcDirection dir)
        {
            this.index = index;
            this.subindex = subindex;
            this.bits = bits;
            this.dir = dir;
        }

        public static Dictionary<uint, List<EcPdoEntry>> Catalogue = new()
        {
            {
                0x044C2C52, //EK1100
                []
            },
            {
                0x03F03052, //EL1008
                new List<EcPdoEntry>()
                {
                    new(0x6000, 0x01, 1, EcDirection.EC_DIR_INPUT), // Channel 1
                    new(0x6010, 0x01, 1, EcDirection.EC_DIR_INPUT), // Channel 2
                    new(0x6020, 0x01, 1, EcDirection.EC_DIR_INPUT), // Channel 3
                    new(0x6030, 0x01, 1, EcDirection.EC_DIR_INPUT), // Channel 4
                    new(0x6040, 0x01, 1, EcDirection.EC_DIR_INPUT), // Channel 1
                    new(0x6050, 0x01, 1, EcDirection.EC_DIR_INPUT), // Channel 2
                    new(0x6060, 0x01, 1, EcDirection.EC_DIR_INPUT), // Channel 3
                    new(0x6070, 0x01, 1, EcDirection.EC_DIR_INPUT)  // Channel 4
                }
            },
            {
                0x07D83052, //EL2008
                new List<EcPdoEntry>()
                {
                    new(0x7000, 0x01, 1, EcDirection.EC_DIR_OUTPUT),
                    new(0x7010, 0x01, 1, EcDirection.EC_DIR_OUTPUT),
                    new(0x7020, 0x01, 1, EcDirection.EC_DIR_OUTPUT),
                    new(0x7030, 0x01, 1, EcDirection.EC_DIR_OUTPUT),
                    new(0x7040, 0x01, 1, EcDirection.EC_DIR_OUTPUT),
                    new(0x7050, 0x01, 1, EcDirection.EC_DIR_OUTPUT),
                    new(0x7060, 0x01, 1, EcDirection.EC_DIR_OUTPUT),
                    new(0x7070, 0x01, 1, EcDirection.EC_DIR_OUTPUT)
                }
            },
            {
                0x1A4B3052, //EL6731
                []
            },
            {
                0x04562C52, //EK1200
                []
            }
        };

        public static List<EcPdoEntryInfo> GiveInputPdos(uint productCode)
        {
            if (!Catalogue.TryGetValue(productCode, out var productPdos)) return [];

            var inputPdos = productPdos.Where(x => x.dir == EcDirection.EC_DIR_INPUT).ToList();

            if (inputPdos.Count == 0) return [];

            List<EcPdoEntryInfo> ret = [];

            foreach (var inputPdo in inputPdos)
            {
                ret.Add(new EcPdoEntryInfo()
                {
                    index = inputPdo.index,
                    subindex = inputPdo.subindex,
                    bit_length = inputPdo.bits,
                });
            }

            return ret;
        }

        public static List<EcPdoEntryInfo> GiveOutputPdos(uint productCode)
        {
            if (!Catalogue.TryGetValue(productCode, out var productPdos)) return [];

            var outputPdos = productPdos.Where(x => x.dir == EcDirection.EC_DIR_OUTPUT).ToList();

            if (outputPdos.Count == 0) return [];

            List<EcPdoEntryInfo> ret = [];

            foreach (var inputPdo in outputPdos)
            {
                ret.Add(new EcPdoEntryInfo()
                {
                    index = inputPdo.index,
                    subindex = inputPdo.subindex,
                    bit_length = inputPdo.bits,
                });
            }

            return ret;
        }

        public static (ushort index, byte subindex, byte bits)[] EcPdoEntryInfoToParams(List<EcPdoEntryInfo> infos)
        {
            return infos.Select(entry => (entry.index, entry.subindex, entry.bit_length)).ToArray();
        }
    }
}