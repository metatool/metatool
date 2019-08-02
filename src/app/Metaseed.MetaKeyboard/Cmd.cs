namespace Metaseed.MetaKeyboard
{
    public class Utils
    {
        public static void Run (string cmd)
        {
            var procStartInfo = new System.Diagnostics.ProcessStartInfo("cmd", "/c " + cmd )
                {
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

            var proc = new System.Diagnostics.Process {StartInfo = procStartInfo};
            proc.Start();
        }
    }
}
