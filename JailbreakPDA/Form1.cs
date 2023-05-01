using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using Microsoft.Win32;

/*
 * Just a simple tool that replaces the default launch application on WinCE embedded devices with windows explorer 
 * If no explorer was found on the system then throws error before procedding, also includes some winapi function to flush registry entries to nand for sure
 * (c) e1z0 2023
 * Possible shells:
 * shell32.exe
 * explorer.exe
 * fexplore.exe
 * TODO
 * add checking if the device are already jailbroken
 */

namespace JailbreakPDA
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

        }

        const string root = "HKEY_LOCAL_MACHINE";
        const string regpath = @"\init";
        const string key = "Launch50";

        [Flags]
        public enum ExitFlags
        {
            Reboot = 0x02,
            PowerOff = 0x08
        }

        // flush registry to nand
        [System.Runtime.InteropServices.DllImport("coredll.dll")]
        public static extern int RegFlushKey(IntPtr hKey);
        // reboot option
        [System.Runtime.InteropServices.DllImport("coredll.dll")]
        public static extern Int32 SetSystemPowerState(Char[] psState, Int32 StateFlags, Int32 Options);
        // enable taskbar functions
        [System.Runtime.InteropServices.DllImport("coredll.dll")]
        public static extern IntPtr FindWindow(string className, string WindowName);
        [System.Runtime.InteropServices.DllImport("coredll.dll")]
        public static extern int ShowWindow(IntPtr hwnd, int nCmdShow);
        [System.Runtime.InteropServices.DllImport("coredll.dll")]
        public static extern bool EnableWindow(IntPtr hwnd, bool bEnable);



    
        private void button1_Click(object sender, EventArgs e)
        {
            var val = "fexplore.exe";
            var path = "\\windows\\";
            var found = false;

            label1.Text = "Changing configuration";
            
            if (File.Exists(path+val)) {
                found = true;
            }

            val = "explorer.exe";
            if (File.Exists(path+val))
            {
                found = true;
            }
            

            if (!found)
            {
                label1.Text = "Jailbreak was unsuccessul";
                MessageBox.Show("Explorer or compatible shell was not found on the windows directory!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand, MessageBoxDefaultButton.Button1);
                return;
            }


            // change registry key
            Registry.SetValue(root+"\\"+regpath,key,val,RegistryValueKind.String);

            // flush to nand
            const UInt32 HKEY_LOCAL_MACHINE = 0x80000002;
            IntPtr hkeyLocalMachine = new IntPtr(unchecked((int)HKEY_LOCAL_MACHINE));
            RegFlushKey(hkeyLocalMachine);

            // enable taskbar
            IntPtr hwnd = FindWindow("HHTaskBar", null);
            EnableWindow(hwnd, true);
            ShowWindow(hwnd, 1);

            // enable command prompt
            var rkey = Registry.LocalMachine.OpenSubKey("Drivers\\Console",true);
            rkey.SetValue("OutputTo",0);

            label1.Text = "Great Success!";
            MessageBox.Show("Done","Success",MessageBoxButtons.OK,MessageBoxIcon.Asterisk,MessageBoxDefaultButton.Button1);
            // reboot
            //const int POWER_STATE_RESET = 0x800000;
            //SetSystemPowerState(null, POWER_STATE_RESET, 0);

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            var currentval = Registry.GetValue(root + "\\" + regpath, key, null) as string;
            label2.Text = "Current shell: "+currentval;
        }
    }
}