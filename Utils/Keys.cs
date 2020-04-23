using LibHac;
using LibHac.FsService;
using System;
using System.IO;

namespace nxfw_tool.Utils
{
    public static class Keys
    {
        public static Keyset Keyset = new Keyset();
        public static void TryLoadKeys()
        {
            if (File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "/.switch/prod.keys"))
            {
                Console.WriteLine($"Using {Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)}/.switch/prod.keys");
                ExternalKeyReader.ReadKeyFile(Keyset, Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "/.switch/prod.keys");
            }
            else
            {
                Console.WriteLine("Couldn't load any keys!");
            }
        }
    }
}