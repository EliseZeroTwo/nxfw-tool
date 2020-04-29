using System.IO;
using System.Runtime.InteropServices;
using static System.Text.Encoding;

namespace nxfw_tool.Firmware
{
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
            r.ReadBytes(4);
            VersionPlatform = ASCII.GetString(r.ReadBytes(0x20));
            VersionHash = ASCII.GetString(r.ReadBytes(0x40));
            VersionString = ASCII.GetString(r.ReadBytes(0x18));
            VersionDescription = ASCII.GetString(r.ReadBytes(0x80)); 

            // Read the firmware strings
            ReadString(r, 0x20, ref VersionPlatform);
            ReadString(r, 0x40, ref VersionHash);
            ReadString(r, 0x18, ref VersionString);
            ReadString(r, 0x80, ref VersionDescription);
            return 0;
        }
    }
}