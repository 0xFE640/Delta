﻿using System.Windows.Forms;
using  System.IO.Ports;
using System.IO;

namespace Delta
{
    using System.Diagnostics.CodeAnalysis;

    public partial class PortForm : Form
    {
      public  static bool port_status_changed = false;
        static readonly string inipath = Directory.GetCurrentDirectory() + @"\deltadvp.ini";
        readonly  IniFile ini = new IniFile(inipath);
        private const string section = "settings";

        public PortForm()
        {

            InitializeComponent();
            string[] ports = SerialPort.GetPortNames();
            foreach (string port in ports)
                ComPortComboBox.Items.Add(port);

            ComPortComboBox.Text = ini.IniReadValue(section, "comport");
            BaudeRateComboBox.Text = ini.IniReadValue(section, "bauderate");
            DataBitsComboBox.Text = ini.IniReadValue(section, "databits");
            ParityComboBox.Text = ini.IniReadValue(section, "parity");
            StopBitsComboBox.Text = ini.IniReadValue(section, "stopbits");
            
        }

        [SuppressMessage("StyleCop.CSharp.ReadabilityRules", "SA1126:PrefixCallsCorrectly", Justification = "Reviewed. Suppression is OK here.")]
        private void button1_Click(object sender, System.EventArgs e)
        {
            ini.IniWriteValue(section,"comport",ComPortComboBox.Text);
            ini.IniWriteValue(section, "bauderate", BaudeRateComboBox.Text);
            ini.IniWriteValue(section, "databits", DataBitsComboBox.Text);
            ini.IniWriteValue(section, "parity", ParityComboBox.Text);
            ini.IniWriteValue(section, "stopbits", StopBitsComboBox.Text);
            port_status_changed = true;
            if (ActiveForm != null) ActiveForm.Close();
            
        }
    }
}
