using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace Searcher
{
    public partial class Searcher : Form
    {
        public static List<string> keywords;
        public static List<string> values;

        public Searcher()
        {
            InitializeComponent();
            try
            {
                SafeFileHandle safeADSHandle = NativeMethods.CreateFile(Program.ADSName,
                FileAccess.Read,
                FileShare.ReadWrite,
                IntPtr.Zero,
                FileMode.Open,
                0,
                IntPtr.Zero);
                //var safeADSHandle = new SafeFileHandle(handle, true);
                if (safeADSHandle.IsInvalid)
                {
                    Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error());
                }
                var stream = new FileStream(safeADSHandle, FileAccess.Read);
                var reader = new StreamReader(stream);
                Searcher.keywords = new List<string>();
                Searcher.values = new List<string>();
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    var values = line.Split(',');
                    Searcher.keywords.Add(values[0]);
                    Searcher.values.Add(values[1]);
                }
                stream.Close();
                cbKeyword.DataSource = Searcher.keywords;
                cbKeyword.AutoCompleteSource = AutoCompleteSource.ListItems;
            }
            catch(IndexOutOfRangeException ex)
            {
                MessageBox.Show("The currently loaded data is not in the correct format. In order to overwrite the data, drag and drop a CSV file with two columns onto the executable. The program will terminate.","",MessageBoxButtons.OK,MessageBoxIcon.Error);
                Environment.Exit(-1);
            }
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            try
            {
                tbResult.Text = Searcher.values[cbKeyword.SelectedIndex];
            }
            catch(ArgumentOutOfRangeException ex)
            {
                if (ex.HResult == -2146233086)
                {
                    MessageBox.Show("Not in the list.","", MessageBoxButtons.OK,MessageBoxIcon.Error);
                    return;
                }
            }
        }

        private void btnClearData_Click(object sender, EventArgs e)
        {
            DialogResult clearData = MessageBox.Show("Do you want to delete the currently loaded data?","", MessageBoxButtons.YesNo,MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
            if (clearData == DialogResult.Yes)
            {
                bool x = NativeMethods.DeleteFileW(Program.ADSName);
                if (!x) Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error());
                cbKeyword.DataSource = null;
                MessageBox.Show("Data cleared. In order to load new data, drag and drop a CSV file with two columns onto the executable.","",MessageBoxButtons.OK,MessageBoxIcon.Information);
                Application.Exit();
            }
        }
    }
}

public partial class NativeMethods
{
    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    public static extern SafeFileHandle CreateFile(
        [MarshalAs(UnmanagedType.LPTStr)] string filename,
        [MarshalAs(UnmanagedType.U4)] FileAccess access,
        [MarshalAs(UnmanagedType.U4)] FileShare share,
        IntPtr securityAttributes,
        [MarshalAs(UnmanagedType.U4)] FileMode creationDisposition,
        [MarshalAs(UnmanagedType.U4)] FileAttributes flagsAndAttributes,
        IntPtr templateFile);

    [DllImport("shlwapi.dll", EntryPoint = "PathFileExistsW", SetLastError = true, CharSet = CharSet.Unicode)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool PathFileExists([MarshalAs(UnmanagedType.LPTStr)]string pszPath);


    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool DeleteFileW([MarshalAs(UnmanagedType.LPWStr)]string lpFileName);
}

