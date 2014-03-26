﻿using System;
using System.Linq;
using System.Windows.Forms;
using System.IO.Ports;
using System.IO;


using Modbus.Device;
using Modbus.Utility;


namespace delta
{
    public partial class MainForm : Form
    {
        static string inipath = Directory.GetCurrentDirectory() + @"\deltadvp.ini";
        IniFile ini = new IniFile(inipath);
        readonly SerialPort port = new SerialPort("COM2");
        DateTime dt = new DateTime();
        const byte slaveId = 0;

        private void SetHexOnly(KeyPressEventArgs e)
        {
            var c = e.KeyChar;
            bool b = (c == '\b' || ('0' <= c && c <= '9') || ('A' <= c && c <= 'F') || ('a' <= c && c <= 'f'));
            if (!b)
                e.Handled = true;

        }
        private void SetNumOnly(KeyPressEventArgs e)
        {
            var c = e.KeyChar;
            bool b = (c == '\b' || ('0' <= c && c <= '9') || c == ',');
            if (!b)
                e.Handled = true;
            if (WValueEdit.Text.IndexOf(',') > 1 && c == ',')
                e.Handled = true;

        }

        private void InitPort(SerialPort com_port)
        {
            const string section = "settings";
            
            com_port.BaudRate = Convert.ToInt32(ini.IniReadValue(section, "bauderate"));
            com_port.DataBits = Convert.ToInt32(ini.IniReadValue(section, "databits"));

            switch (ini.IniReadValue(section, "parity"))
            {
                case "Even":
                    com_port.Parity =Parity.Even;
                    break;
                case "Mark":
                    com_port.Parity = Parity.Mark;
                    break;
                case "None":
                    com_port.Parity = Parity.None;
                    break;
                case "Odd":
                    com_port.Parity = Parity.Odd;
                    break;
                case "Space":
                    com_port.Parity = Parity.Space;
                    break;
                default:
                   // throw  new Exception("Not correct parity settings");
                    MessageBox.Show("Not correct parity settings");
                    break;
            }
            switch (ini.IniReadValue(section, "stopbits"))
            {
                case "None":
                    com_port.StopBits = StopBits.None;
                    break;
                case "1":
                    com_port.StopBits = StopBits.One;
                    break;
                case "1.5":
                    com_port.StopBits = StopBits.OnePointFive;
                    break;
                case "2":
                    com_port.StopBits = StopBits.Two;
                    break;
                default:
                    throw new Exception("Not correct stop bits settings");
                    
            }
            

        }

        public MainForm()
        {
            
            InitializeComponent();
            InitPort(port);
            //port.BaudRate = 115200;
            //port.DataBits = 7;
            //port.Parity = Parity.Even;
            //port.StopBits = StopBits.One;
            //try
            //{
            //    port.Open();
            //}
            //catch (Exception ex)
            //{
            //    MessageBox.Show(ex.Message);
            //    //foreach (var control in Controls.OfType<Button>())
            //    //    control.Enabled = false;
            //}
        }


        private ushort ReadIntRegister(ushort startAddress )
        {
            if (!port.IsOpen)
            try
            {
                port.Open();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return 0;
            }

            IModbusSerialMaster master = ModbusSerialMaster.CreateAscii(port);
           
            ushort[] registers = master.ReadHoldingRegisters(slaveId, startAddress, 1);
           
                return registers[0];
           
            

        }

        private float ReadFloatRegister(ushort startAddress)
        {
            if (!port.IsOpen)
                port.Open();
            IModbusSerialMaster master = ModbusSerialMaster.CreateAscii(port);
            
            ushort[] registers = master.ReadHoldingRegisters(slaveId, startAddress,2);
            return
                ModbusUtility.GetSingle(registers[1], registers[0]);


        }

        private void WriteRegisters(ushort startAddress, ushort registerValue)
        {
            if (!port.IsOpen)
                port.Open();

            IModbusSerialMaster master = ModbusSerialMaster.CreateAscii(port);

            
            master.WriteSingleRegister(slaveId, startAddress, registerValue);


        }

        private void WriteRegisters(ushort startAddress, float registerValue)
        {
            if (!port.IsOpen)
                port.Open();
            IModbusSerialMaster master = ModbusSerialMaster.CreateAscii(port);

            var registers = new ushort[4];
            byte[] bytereg = new byte[4]; 
            bytereg = BitConverter.GetBytes(registerValue);
            registers[0] = BitConverter.ToUInt16(bytereg, 0);
            registers[1] = BitConverter.ToUInt16(bytereg, 2);

            master.WriteMultipleRegisters(slaveId, startAddress, registers);

        }

        private void ButtonReadIntAddresClick(object sender, EventArgs e)
        {
            if (RAddressEdit.Text.Length > 0)
                label1.Text = ReadIntRegister(Convert.ToUInt16(RAddressEdit.Text, 16)).ToString();
            
            
        }

        private void ButtonWriteIntClick(object sender, EventArgs e)
        {
            if (WValueEdit.Text.IndexOf(',') > -1)
            {
                MessageBox.Show("Value must be integer");
                return;
            }
            WriteRegisters(Convert.ToUInt16(WAddressEdit.Text, 16), Convert.ToUInt16(WValueEdit.Text, 10));

        }

        private void ButtonWriteFloatClick(object sender, EventArgs e)
        {
            if (WAddressEdit.Text.Length > 0)
                WriteRegisters(Convert.ToUInt16(WAddressEdit.Text, 16), Convert.ToSingle(WValueEdit.Text));

            
        }

        private void ButtonReadFloatClick(object sender, EventArgs e)
        {
            if (RAddressEdit.Text.Length > 0)
                label1.Text = ReadFloatRegister(Convert.ToUInt16(RAddressEdit.Text, 16)).ToString();

        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            ushort t = ReadIntRegister(0x100b);
            float p = ReadFloatRegister(0x102e);

            chart1.Series[0].Points.AddXY(dt.Second,t );
            chart2.Series[0].Points.AddXY(dt.Second, p);

            label1.Text = "t = "+ t.ToString();
            label5.Text = "P = "+ p.ToString();
            
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            timer1.Start();
            //int x = 0;
            //for (int i = 0; i < 30; i++)
            //{
            //    double ysin = Math.Sin(x);
            //    double ycos = Math.Cos(x);
            //    chart1.Series[0].Points.AddXY(x, ysin);
            //    chart1.Series[1].Points.AddXY(x, ycos);

            //    x++;
            //}
        }

        private void RAddressEditKeyPress(object sender, KeyPressEventArgs e)
        {
          
            SetHexOnly(e);
           

        }

       

       

        private void WAddressEdit_KeyPress(object sender, KeyPressEventArgs e)
        {
           
            SetHexOnly(e);
        }

        private void WValueEdit_KeyPress(object sender, KeyPressEventArgs e)
        {
            SetNumOnly(e);
        }

        private void comPortToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form2 frm = new Form2();
            frm.Show();

        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();

        }

        private void button2_Click(object sender, EventArgs e)
        {
            timer1.Stop();
        }

        private void label6_Click(object sender, EventArgs e)
        {

        }

        private void TTaskEdit_KeyDown(object sender, KeyEventArgs e)
        {
            ushort Ttask;// =Convert.ToSingle(textBox1.Text);
            if (e.KeyCode == Keys.Enter && TTaskEdit.Text.Length > 0)
            {
                Ttask = Convert.ToUInt16(TTaskEdit.Text);
                WriteRegisters(0x100a, Ttask);
            }
        }

        private void KpFEdit_KeyDown(object sender, KeyEventArgs e)
        {
            float KpF;
            if (e.KeyCode == Keys.Enter && KpFEdit.Text.Length > 0)
            {
                KpF = Convert.ToSingle(KpFEdit.Text);
                WriteRegisters(0x100c, KpF);
            }
        }

        private void KiFEdit_KeyDown(object sender, KeyEventArgs e)
        {
            float KiF;
            if (e.KeyCode == Keys.Enter && KiFEdit.Text.Length > 0)
            {
                KiF = Convert.ToSingle(KiFEdit.Text);
                WriteRegisters(0x1034, KiF);
            }
        }




    }
}
