using System;
using System.Windows.Forms;
using System.IO;
using Microsoft.Win32.SafeHandles;
using System.Runtime.InteropServices;
namespace Searcher
{
    static class Program
    {
        public static string ADSName;
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            string appPath = System.Reflection.Assembly.GetExecutingAssembly().CodeBase;
            appPath = appPath.Substring(8).Replace(@"/", @"\");
            string appName = Path.GetFileName(appPath);
            ADSName = appName + ":" + "data.csv";
            if (args == null || args.Length == 0)
            {
                if (!NativeMethods.PathFileExists(ADSName))
                    MessageBox.Show("No data stream. In order to load data, drag and drop a CSV file with two columns onto the executable.","",MessageBoxButtons.OK,MessageBoxIcon.Information);
                else
                    Application.Run(new Searcher());
            }
            else
            {
                string filePath = args[0];
                try
                {
                    SafeFileHandle ADS = NativeMethods.CreateFile(ADSName,
                    FileAccess.Write,
                    FileShare.Read,
                    IntPtr.Zero,
                    FileMode.Create,
                    0,
                    IntPtr.Zero);
                    if (ADS.IsInvalid)
                    {
                        Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error());
                    }
                    var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                    var dataStream = new FileStream(ADS, FileAccess.ReadWrite);
                    fileStream.CopyTo(dataStream);
                    fileStream.Close();

                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message + "\r\n" + ex.HResult);
                }
                MessageBox.Show("Data loaded.","",MessageBoxButtons.OK,MessageBoxIcon.Information);
                Application.Run(new Searcher());
            }
        }
    }
}
