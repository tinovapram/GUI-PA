using System;
using System.IO;
using System.Windows.Forms;
using System.IO.Ports;
using CustomUIControls.Graphing;

namespace TA2
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        private string serBuf = "", serbuf1 = "";
        Single[] konst = new float[6];
        string mydocpath = "D:\\TA\\LOG\\" + DateTime.Now.ToString("MM") + "\\" + DateTime.Now.ToString("dd");
        StreamWriter logFile;
        bool File_created = false;
        UInt64 baris = 0;

        private C2DPushGraph.LineHandle m_Line1Handle;
        private C2DPushGraph.LineHandle m_Line2Handle;

        private void ldPort()
        {
            string[] ArrayComPortsNames = null;
            int index = -1;
            string ComPortName = null;

            ArrayComPortsNames = SerialPort.GetPortNames();
            if (ArrayComPortsNames.Length != 0)
            {
                do
                {
                    index += 1;
                    cboPorts.Items.Add(ArrayComPortsNames[index]);
                }
                while (!((ArrayComPortsNames[index] == ComPortName) || (index == ArrayComPortsNames.GetUpperBound(0))));
                Array.Sort(ArrayComPortsNames);
                if (index == ArrayComPortsNames.GetUpperBound(0))
                {
                    ComPortName = ArrayComPortsNames[0];
                }
                cboPorts.Text = ArrayComPortsNames[0];
            }
            cboPorts.SelectedIndex = 1;
        }
        public struct Cnvrt
        {
            public float kons;
            public string msk;
        }

        private void Button2_Click(object sender, EventArgs e)
        {
            try
            {
                if (_data.IsOpen) _data.Close();
                _data.PortName = cboPorts.Text;
                _data.DataReceived += serialPort_DataReceived;
                _data.Open();
            }
            catch (Exception er)
            {
                MessageBox.Show("Port tidak dapat dibuka " + er.ToString(), "Buka Port Gagal", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            if (_data.IsOpen) { Button2.Enabled = false; Button1.Enabled = true; cboPorts.Enabled = false; _data.WriteLine("z" + '\r'); logBox.Items.Clear(); }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            ldPort();
            System.IO.Directory.CreateDirectory(mydocpath);
            //phsGrf.ChartAreas[0].AxisX.ScrollBar.Enabled = true;
            //phsGrf.ChartAreas[0].AxisX.ScaleView.Size = 1000;
            //phsGrf.ChartAreas[0].AxisY.Maximum = 30;
            //phsGrf.ChartAreas[0].AxisY.Minimum = -30;
            //phsGrf.Series[0].ChartType = SeriesChartType.Spline;
            //phsGrf.Series[1].ChartType = SeriesChartType.Spline;
            c2DPushGraph1.AddLine(0, System.Drawing.Color.White);
            c2DPushGraph1.AddLine(1, System.Drawing.Color.Red);
        }

        private void cboPorts_DropDown(object sender, EventArgs e)
        {
            cboPorts.Items.Clear();
            ldPort();
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            try
            {
                System.Threading.Thread CloseDown = new System.Threading.Thread(new System.Threading.ThreadStart(SerialClose));
                //_data.DataReceived -= serialPort_DataReceived;
                _data.Close();
                tutupFile();
            }
            catch (Exception er)
            {
                MessageBox.Show("Port tidak dapat ditutup " + er.ToString(), "Tutup Port Gagal", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            if (!_data.IsOpen) { Button1.Enabled = false; Button2.Enabled = true; cboPorts.Enabled = true; }
        }

        private void serialPort_DataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {
            serbuf1 = _data.ReadLine();
            string[] words = serbuf1.Split(',');
            if (words.Length == 7)
            {
                try
                {
                    this.Invoke((MethodInvoker)delegate { logBox.Items.Add("Data RAW Konstanta: " + serbuf1); });
                    for (byte i = 0; i < 6; i++)
                    {
                        UInt32 num = UInt32.Parse(words[i], System.Globalization.NumberStyles.AllowHexSpecifier);
                        byte[] floatVals = BitConverter.GetBytes(num);
                        konst[i] = BitConverter.ToSingle(floatVals, 0);
                    }
                    if (words[6] == "1")
                    {
                        this.Invoke((MethodInvoker)delegate { createFile(); });
                    }
                    else this.Invoke((MethodInvoker)delegate { tutupFile(); });
                    this.Invoke((MethodInvoker)delegate { BacaKons(konst); });
                }
                catch (Exception ex) { }
            }
            else if (words.Length == 8)
            {
                try
                {
                    if (File_created)
                    {
                        this.Invoke((MethodInvoker)delegate { logFile.WriteLine(serbuf1); });
                    }
                }
                catch (Exception ex) { this.Invoke((MethodInvoker)delegate { logBox.Items.Add("Data Logging gagal diproses: " + ex.Data); logBox.SelectedIndex = logBox.Items.Count - 1; }); }
            }
            else if (words.Length == 4)
            {
                this.Invoke((MethodInvoker)delegate {                   gambar(words);                });
                try
                {                    
                    if (words[3] == "1" && !File_created)
                    {
                        this.Invoke((MethodInvoker)delegate { createFile(); });
                    }
                    if (words[3] == "0" && File_created) this.Invoke((MethodInvoker)delegate { tutupFile(); });
                }
                catch (Exception ex) { this.Invoke((MethodInvoker)delegate { logBox.Items.Add("Display gagal "+serbuf1);  logBox.SelectedIndex = logBox.Items.Count - 1; }); }
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (_data.IsOpen)
            {
                if (kpbox.Enabled == true)
                {
                    _data.Write(dtBox(kpbox.Text), 0, 4);
                    _data.WriteLine("q" + '\r');
                }
                else if (kpbox2.Enabled == true)
                {
                    _data.Write(dtBox(kpbox2.Text), 0, 4);
                    _data.WriteLine("a" + '\r');
                }
                else if (kibox.Enabled == true)
                {
                    _data.Write(dtBox(kibox.Text), 0, 4);
                    _data.WriteLine("w" + '\r');
                }
                else if (kibox2.Enabled == true)
                {
                    _data.Write(dtBox(kibox2.Text), 0, 4);
                    _data.WriteLine("s" + '\r');
                }
                else if (kdbox.Enabled == true)
                {
                    _data.Write(dtBox(kdbox.Text), 0, 4);
                    _data.WriteLine("e" + '\r');
                }
                else if (kdbox2.Enabled == true)
                {
                    _data.Write(dtBox(kdbox2.Text), 0, 4);
                    _data.WriteLine("d" + '\r');
                }
            }
            unblank();
        }

        void gambar(string[] buff)
        {
            c2DPushGraph1.Push(Convert.ToInt16(Convert.ToDouble(buff[1]))+50,0);
            c2DPushGraph1.Push(Convert.ToInt16(Convert.ToDouble(buff[0])) + 50, 1);
            c2DPushGraph1.UpdateGraph();
        }

        void BacaKons(float[] buff)
        {
            kpbox.Text = buff[0].ToString("0.000000");
            kibox.Text = buff[1].ToString("0.000000");
            kdbox.Text = buff[2].ToString("0.000000");
            kpbox2.Text = buff[3].ToString("0.000000");
            kibox2.Text = buff[4].ToString("0.000000");
            kdbox2.Text = buff[5].ToString("0.000000");
            //unblank();
        }

        void blank()
        {
            kpbox.Enabled = false;
            kpbox2.Enabled = false;
            kibox.Enabled = false;
            kibox2.Enabled = false;
            kdbox.Enabled = false;
            kdbox2.Enabled = false;
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            logBox.Items.Add(e.KeyValue.ToString());
            // e.Handled = false;
        }

        private void Form1_KeyUp(object sender, KeyEventArgs e)
        {
            textBox1.Text = e.KeyValue.ToString();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            _data.WriteLine("z" + '\r');
        }

        private void button5_Click(object sender, EventArgs e)
        {
            if (_data.IsOpen)
            {
                createFile();
                textBox3.Text = DateTime.Now.ToString("HHmm");
                _data.WriteLine("p" + '\r');
                timer1.Start();
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            File_created = false;
            if (_data.IsOpen) { File_created = false; _data.WriteLine("o" + '\r'); timer1.Stop(); tutupFile(); }
        }

        private void createFile()
        {
            if (!File_created)
            {
                try
                {
                    string a = DateTime.Now.ToString("HHmm") + textBox2.Text + ".txt";
                    logFile = new StreamWriter(Path.Combine(mydocpath, a));
                    logFile.WriteLine("Kp2,Ki2,Kd2,Kp1,Ki1,Kd1");
                    logFile.WriteLine(kpbox.Text + "," + kibox.Text + "," + kdbox.Text + "," + kpbox2.Text + "," + kibox2.Text + "," + kdbox2.Text);
                    logFile.WriteLine("Waktu,Sudut Robot,Sudut Target,PWM Target,PWM Kanan,PWM Kiri,Kec. Real,Kec. Target,Detik");
                }
                catch (Exception ex)
                {
                    string a = DateTime.Now.ToString("HHmm-ss") + textBox2.Text + ".txt";
                    logFile = new StreamWriter(Path.Combine(mydocpath, a));
                    logFile.WriteLine("Kp2,Ki2,Kd2,Kp1,Ki1,Kd1");
                    logFile.WriteLine(kpbox.Text + "," + kibox.Text + "," + kdbox.Text + "," + kpbox2.Text + "," + kibox2.Text + "," + kdbox2.Text);
                    logFile.WriteLine("Waktu,Sudut Robot,Sudut Target,PWM Target,PWM Kanan,PWM Kiri,Kec. Real,Kec. Target,Detik");
                }
                File_created = true;
            }
        }

        private void tutupFile()
        {
            if (File_created)
            {
                //WriteExcel.CloseExcel();
                logFile.Close();
                logFile.Dispose();
                File_created = false;
            }
        }

        private void kpbox_MouseDown(object sender, MouseEventArgs e)
        {
            kpbox2.Enabled = false;
            kibox.Enabled = false;
            kibox2.Enabled = false;
            kdbox.Enabled = false;
            kdbox2.Enabled = false;
            //kpbox.Focus();
        }

        private void kibox_MouseDown(object sender, MouseEventArgs e)
        {
            kpbox.Enabled = false;
            kpbox2.Enabled = false;
            kibox2.Enabled = false;
            kdbox.Enabled = false;
            kdbox2.Enabled = false;
        }

        private void kdbox_MouseDown(object sender, MouseEventArgs e)
        {
            kpbox.Enabled = false;
            kpbox2.Enabled = false;
            kibox.Enabled = false;
            kibox2.Enabled = false;
            kdbox2.Enabled = false;
        }

        private void kpbox2_MouseDown(object sender, MouseEventArgs e)
        {
            kpbox.Enabled = false;
            kibox.Enabled = false;
            kibox2.Enabled = false;
            kdbox.Enabled = false;
            kdbox2.Enabled = false;
        }

        private void kibox2_MouseDown(object sender, MouseEventArgs e)
        {
            kpbox.Enabled = false;
            kpbox2.Enabled = false;
            kibox.Enabled = false;
            kdbox.Enabled = false;
            kdbox2.Enabled = false;
        }

        private void kdbox2_MouseDown(object sender, MouseEventArgs e)
        {
            kpbox.Enabled = false;
            kpbox2.Enabled = false;
            kibox.Enabled = false;
            kibox2.Enabled = false;
            kdbox.Enabled = false;
        }

        void unblank()
        {
            kpbox.Enabled = true;
            kpbox2.Enabled = true;
            kibox.Enabled = true;
            kibox2.Enabled = true;
            kdbox.Enabled = true;
            kdbox2.Enabled = true;
        }

        byte[] dtBox(string _in)
        {
            Single kon = Convert.ToSingle(_in);
            textBox1.Text = "";
            byte[] _ot = new byte[4];
            _ot = BitConverter.GetBytes(kon);
            int i = BitConverter.ToInt32(_ot, 0);
            //textBox1.Text = BitConverter.ToSingle(_ot,0).ToString();
            textBox1.Text += i.ToString("X");
            //textBox1.Text = ToBinaryString(kon);
            return _ot;
        }

        private void Form1_FormClosing_1(object sender, FormClosingEventArgs e)
        {
            if (_data.IsOpen)
            {
                e.Cancel = true; //cancel the fom closing
                System.Threading.Thread CloseDown = new System.Threading.Thread(new System.Threading.ThreadStart(CloseSerialOnExit)); //close port in new thread to avoid hang
                CloseDown.Start(); //close port in new thread to avoid hang
            }
        }

        string ToBinaryString(float value)
        {

            int bitCount = sizeof(float) * 8; // never rely on your knowledge of the size
            char[] result = new char[bitCount]; // better not use string, to avoid ineffective string concatenation repeated in a loop

            // now, most important thing: (int)value would be "semantic" cast of the same
            // mathematical value (with possible rounding), something we don't want; so:
            int intValue = System.BitConverter.ToInt32(BitConverter.GetBytes(value), 0);

            for (int bit = 0; bit < bitCount; ++bit)
            {
                int maskedValue = intValue & (1 << bit); // this is how shift and mask is done.
                if (maskedValue > 0)
                    maskedValue = 1;
                // at this point, masked value is either int 0 or 1
                result[bitCount - bit - 1] = maskedValue.ToString()[0]; // bits go right-to-left in usual Western Arabic-based notation
            }
            return new string(result); // string from character array
        }

        private void SerialClose()
        {
            try
            {
                _data.DataReceived -= serialPort_DataReceived;
                logFile.Close();
                _data.Close(); //close the serial port
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message); //catch any serial port closing error messages
            }
        }

        private void CloseSerialOnExit()
        {
            try
            {
                _data.DataReceived -= serialPort_DataReceived;
                _data.WriteLine("o");
                _data.Close(); //close the serial port
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message); //catch any serial port closing error messages
            }

            this.Invoke(new EventHandler(NowClose)); //now close back in the main thread
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            timer1.Stop();
            if (_data.IsOpen)
            {
                _data.WriteLine("H" + '\r');
            }
            //phsGrf.Series["pA"].Points.AddY(angle);
            //phsGrf.Series["pB"].Points.AddY(Convert.ToDouble(words[0]), Convert.ToDouble(words[2]));
            //phsGrf.Series["pA"].Points.AddY(buff);
            //if (phsGrf.ChartAreas[0].AxisX.Maximum > phsGrf.ChartAreas[0].AxisX.ScaleView.Size)
            //{ phsGrf.ChartAreas[0].AxisX.ScaleView.Scroll(phsGrf.ChartAreas[0].AxisX.Maximum); }
            //aGauge2.Value = Convert.ToInt32(angle);
        }

        private void button7_Click(object sender, EventArgs e)
        {
            if (_data.IsOpen)
            {
                _data.WriteLine("H" + '\r');
            }
        }

        private void UpBtn_Click(object sender, EventArgs e)
        {
            if (_data.IsOpen)
            {
                _data.WriteLine("F" + '\r');
            }
        }

        private void DownBtn_Click(object sender, EventArgs e)
        {
            if (_data.IsOpen)
            {
                _data.WriteLine("G" + '\r');
            }
        }

        private void button9_Click(object sender, EventArgs e)
        {
            if (_data.IsOpen)
            {

            }
        }

        private void button8_Click(object sender, EventArgs e)
        {
            if (_data.IsOpen)
            {

            }
        }

        private void button10_Click(object sender, EventArgs e)
        {
            if (_data.IsOpen)
            {
                _data.WriteLine("F" + '\r');
                timer1.Start();
            }
        }

        private void NowClose(object sender, EventArgs e)
        {
            //logFile.Close();
            this.Close(); //now close the form
        }
    }
}
