using System;
using System.Collections.Generic;
using System.Drawing;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace PLCServer
{
    public partial class MainForm : Form
    {
        private TcpClient plcClient;
        private NetworkStream plcStream;
        private bool isConnected = false;
        private Thread monitorThread;
        private bool stopMonitoring = false;
        private List<DeviceBlock> deviceBlocks = new List<DeviceBlock>();
        private Panel blocksPanel;
        private Button addBlockBtn;
        
        public MainForm()
        {
            InitializeComponent();
        }
        
        private void InitializeComponent()
        {
            this.Text = "PLC Keyence Server - Device Monitor";
            this.Size = new Size(1200, 800);
            this.StartPosition = FormStartPosition.CenterScreen;
            
            // Connection Block (luon co dinh)
            var connBlock = CreateConnectionBlock();
            connBlock.Location = new Point(10, 10);
            this.Controls.Add(connBlock);
            
            // Add Block Button
            addBlockBtn = new Button();
            addBlockBtn.Text = "+ Thêm khối Device";
            addBlockBtn.Size = new Size(150, 30);
            addBlockBtn.Location = new Point(10, connBlock.Bottom + 10);
            addBlockBtn.Click += AddBlockBtn_Click;
            this.Controls.Add(addBlockBtn);
            
            // Scrollable panel for device blocks
            blocksPanel = new Panel();
            blocksPanel.AutoScroll = true;
            blocksPanel.Location = new Point(10, addBlockBtn.Bottom + 10);
            blocksPanel.Size = new Size(1160, 680);
            blocksPanel.BorderStyle = BorderStyle.FixedSingle;
            this.Controls.Add(blocksPanel);
        }
        
        private GroupBox CreateConnectionBlock()
        {
            var group = new GroupBox();
            group.Text = "Kết nối PLC";
            group.Size = new Size(400, 140);
            
            // IP Label
            var lblIP = new Label();
            lblIP.Text = "IP Address:";
            lblIP.Location = new Point(10, 25);
            lblIP.Size = new Size(80, 20);
            group.Controls.Add(lblIP);
            
            // IP TextBox
            var txtIP = new TextBox();
            txtIP.Name = "txtIP";
            txtIP.Text = "192.168.1.10";
            txtIP.Location = new Point(90, 22);
            txtIP.Size = new Size(120, 20);
            group.Controls.Add(txtIP);
            
            // Port Label
            var lblPort = new Label();
            lblPort.Text = "Port:";
            lblPort.Location = new Point(220, 25);
            lblPort.Size = new Size(40, 20);
            group.Controls.Add(lblPort);
            
            // Port TextBox
            var txtPort = new TextBox();
            txtPort.Name = "txtPort";
            txtPort.Text = "8501";
            txtPort.Location = new Point(260, 22);
            txtPort.Size = new Size(60, 20);
            group.Controls.Add(txtPort);
            
            // Status Label
            var lblStatus = new Label();
            lblStatus.Name = "lblStatus";
            lblStatus.Text = "Trạng thái: Chưa kết nối";
            lblStatus.Location = new Point(10, 55);
            lblStatus.Size = new Size(300, 20);
            lblStatus.ForeColor = Color.Red;
            group.Controls.Add(lblStatus);
            
            // Connect Button
            var btnConnect = new Button();
            btnConnect.Name = "btnConnect";
            btnConnect.Text = "Kết nối";
            btnConnect.Location = new Point(10, 85);
            btnConnect.Size = new Size(100, 30);
            btnConnect.Click += BtnConnect_Click;
            group.Controls.Add(btnConnect);
            
            // Disconnect Button
            var btnDisconnect = new Button();
            btnDisconnect.Name = "btnDisconnect";
            btnDisconnect.Text = "Ngắt kết nối";
            btnDisconnect.Location = new Point(120, 85);
            btnDisconnect.Size = new Size(100, 30);
            btnDisconnect.Enabled = false;
            btnDisconnect.Click += BtnDisconnect_Click;
            group.Controls.Add(btnDisconnect);
            
            return group;
        }
        
        private void BtnConnect_Click(object sender, EventArgs e)
        {
            var connGroup = this.Controls[0] as GroupBox;
            var txtIP = connGroup.Controls["txtIP"] as TextBox;
            var txtPort = connGroup.Controls["txtPort"] as TextBox;
            var lblStatus = connGroup.Controls["lblStatus"] as Label;
            var btnConnect = connGroup.Controls["btnConnect"] as Button;
            var btnDisconnect = connGroup.Controls["btnDisconnect"] as Button;
            
            try
            {
                if (!IPAddress.TryParse(txtIP.Text, out IPAddress ip))
                {
                    MessageBox.Show("IP không hợp lệ!");
                    return;
                }
                
                if (!int.TryParse(txtPort.Text, out int port) || port < 1 || port > 65535)
                {
                    MessageBox.Show("Port không hợp lệ! (1-65535)");
                    return;
                }
                
                plcClient = new TcpClient();
                plcClient.Connect(ip, port);
                plcStream = plcClient.GetStream();
                plcStream.ReadTimeout = 5000;
                plcStream.WriteTimeout = 5000;
                
                isConnected = true;
                lblStatus.Text = "Trạng thái: Đã kết nối " + txtIP.Text + ":" + port;
                lblStatus.ForeColor = Color.Green;
                btnConnect.Enabled = false;
                btnDisconnect.Enabled = true;
                txtIP.Enabled = false;
                txtPort.Enabled = false;
                
                // Start monitoring thread
                stopMonitoring = false;
                monitorThread = new Thread(MonitorDevices);
                monitorThread.IsBackground = true;
                monitorThread.Start();
                
                MessageBox.Show("Kết nối thành công!");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi kết nối: " + ex.Message);
            }
        }
        
        private void BtnDisconnect_Click(object sender, EventArgs e)
        {
            DisconnectPLC();
        }
        
        private void DisconnectPLC()
        {
            stopMonitoring = true;
            
            if (plcStream != null)
            {
                plcStream.Close();
                plcStream = null;
            }
            
            if (plcClient != null)
            {
                plcClient.Close();
                plcClient = null;
            }
            
            isConnected = false;
            
            var connGroup = this.Controls[0] as GroupBox;
            var lblStatus = connGroup.Controls["lblStatus"] as Label;
            var btnConnect = connGroup.Controls["btnConnect"] as Button;
            var btnDisconnect = connGroup.Controls["btnDisconnect"] as Button;
            var txtIP = connGroup.Controls["txtIP"] as TextBox;
            var txtPort = connGroup.Controls["txtPort"] as TextBox;
            
            lblStatus.Text = "Trạng thái: Chưa kết nối";
            lblStatus.ForeColor = Color.Red;
            btnConnect.Enabled = true;
            btnDisconnect.Enabled = false;
            txtIP.Enabled = true;
            txtPort.Enabled = true;
        }
        
        private void AddBlockBtn_Click(object sender, EventArgs e)
        {
            var block = new DeviceBlock(this);
            deviceBlocks.Add(block);
            
            // Tinh toan vi tri
            int yPos = 10;
            foreach (var b in deviceBlocks)
            {
                if (b != block)
                    yPos = Math.Max(yPos, b.Bottom + 10);
            }
            
            block.Location = new Point(10, yPos);
            blocksPanel.Controls.Add(block);
        }
        
        public void RemoveDeviceBlock(DeviceBlock block)
        {
            deviceBlocks.Remove(block);
            blocksPanel.Controls.Remove(block);
            
            // Re-layout
            int yPos = 10;
            foreach (var b in deviceBlocks)
            {
                b.Location = new Point(10, yPos);
                yPos = b.Bottom + 10;
            }
        }
        
        private void MonitorDevices()
        {
            while (!stopMonitoring && isConnected)
            {
                try
                {
                    foreach (var block in deviceBlocks)
                    {
                        if (block.NeedsUpdate)
                        {
                            ReadDeviceValue(block);
                        }
                    }
                    Thread.Sleep(500);
                }
                catch { }
            }
        }
        
        private void ReadDeviceValue(DeviceBlock block)
        {
            if (!isConnected || plcStream == null) return;
            
            try
            {
                string command = block.GetReadCommand();
                byte[] sendData = Encoding.ASCII.GetBytes(command);
                
                lock (plcStream)
                {
                    plcStream.Write(sendData, 0, sendData.Length);
                    
                    byte[] receiveBuffer = new byte[256];
                    int bytesRead = plcStream.Read(receiveBuffer, 0, receiveBuffer.Length);
                    string response = Encoding.GetEncoding(932).GetString(receiveBuffer, 0, bytesRead);
                    
                    block.UpdateValue(response);
                }
            }
            catch (Exception ex)
            {
                block.SetError(ex.Message);
            }
        }
        
        public bool WriteDeviceValue(string deviceType, string address, string value)
        {
            if (!isConnected || plcStream == null) return false;
            
            try
            {
                string command = $"WR {deviceType}{address}.D {value}\r";
                byte[] sendData = Encoding.ASCII.GetBytes(command);
                
                lock (plcStream)
                {
                    plcStream.Write(sendData, 0, sendData.Length);
                    
                    byte[] receiveBuffer = new byte[256];
                    int bytesRead = plcStream.Read(receiveBuffer, 0, receiveBuffer.Length);
                    string response = Encoding.GetEncoding(932).GetString(receiveBuffer, 0, bytesRead);
                    
                    return response.Contains("OK");
                }
            }
            catch
            {
                return false;
            }
        }
        
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            DisconnectPLC();
            base.OnFormClosing(e);
        }
    }
}
