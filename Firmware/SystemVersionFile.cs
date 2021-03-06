using System.IO;
using System.Runtime.InteropServices;
using static System.Text.Encoding;

namespace nxfw_tool.Firmware
{
    [StructLayout(LayoutKind.Sequential, Pack = 0x4, Size = 256)]
    public struct SystemVersionFile
    {
        public byte Major;
        public byte Minor;
        public byte Micro;
        public byte Rev;
        public string VersionPlatform;
        public string VersionHash;
        public string VersionString;
        public string VersionDescription;

        private void ReadString(BinaryReader reader, int count, ref string outStr)
        {

            foreach (char x in ASCII.GetString(reader.ReadBytes(count)))
            {
                if (x != '\x00')
                    outStr += x;
                else
                    break;
            }
        }

        public int Read(Stream stream)
        {
            BinaryReader r = new BinaryReader(stream);

            if (r.BaseStream.Length != 0x100)
                return 1;

            // Read the firmware numbers
            Major = r.ReadByte();
            Minor = r.ReadByte();
            Micro = r.ReadByte();
            Rev = r.ReadByte();
            
            // Skip the 4 bytes we don't use
            r.ReadBytes(4);

            // Read the firmware strings
            ReadString(r, 0x20, ref VersionPlatform);
            ReadString(r, 0x40, ref VersionHash);
            ReadString(r, 0x18, ref VersionString);
            ReadString(r, 0x80, ref VersionDescription);
            return 0;
        }
    }
}