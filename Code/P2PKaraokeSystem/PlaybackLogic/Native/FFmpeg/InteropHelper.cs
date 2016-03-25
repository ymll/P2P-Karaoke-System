using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace P2PKaraokeSystem.PlaybackLogic.Native.FFmpeg
{
    public class InteropHelper
    {
        public const string LD_LIBRARY_PATH = "LD_LIBRARY_PATH";

        public static void RegisterLibrariesSearchPath(string path)
        {
            SetDllDirectory(path);
        }

        [DllImport("kernel32", SetLastError = true)]
        private static extern bool SetDllDirectory(string lpPathName);
    }
}
