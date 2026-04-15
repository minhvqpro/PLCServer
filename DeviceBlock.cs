using System;
using System.Drawing;
using System.Windows.Forms;

namespace PLCServer
{
    public class DeviceBlock : GroupBox
    {
        private MainForm parentForm;
        private ComboBox cmbDeviceType;
        private TextBox txtAddress;
        private TextBox txtValue;
        private Label lblStatus;
        private CheckBox chkAutoRead;
        private Button btnRead;
        private Button btnWrite;
        private Button btnRemove;
        private Label lblLastUpdate;
        
        public bool NeedsUpdate => chkAutoRead.Checked;
        
        public DeviceBlock(MainForm parent)
        {
            this.parentForm = parent;
            this.Size = new Size(1120, 120);
            this.Text = "Device Block";
            
            InitializeComponents();
        }
        
        private void InitializeComponents()
        {
            // Device Type
            var lblType = new Label();
            lblType.Text = "Loại:";
            lblType.Location = new Point(10, 25);
            lblType.Size = new Size(50, 20);
            this.Controls.Add(lblType);
            
            cmbDeviceType = new ComboBox();
            cmbDeviceType.Items.AddRange(new string[] { "DM", "MR", "W", "R", "Z", "TM", "TR", "CM", "EM", "FM" });
            cmbDeviceType.SelectedIndex = 0; // DM
            cmbDeviceType.Location = new Point(60, 22);
            cmbDeviceType.Size = new Size(80, 25);
            cmbDeviceType.DropDownStyle = ComboBoxStyle.DropDownList;
            this.Controls.Add(cmbDeviceType);
            
            // Address
            var lblAddr = new Label();
            lblAddr.Text = "Địa chỉ:";
            lblAddr.Location = new Point(150, 25);
            lblAddr.Size = new Size(50, 20);
            this.Controls.Add(lblAddr);
            
            txtAddress = new TextBox();
            txtAddress.Text = "1000";
            txtAddress.Location = new Point(200, 22);
            txtAddress.Size = new Size(80, 20);
            this.Controls.Add(txtAddress);
            
            // Value display
            var lblVal = new Label();
            lblVal.Text = "Giá trị:";
            lblVal.Location = new Point(290, 25);
            lblVal.Size = new Size(50, 20);
            this.Controls.Add(lblVal);
            
            txtValue = new TextBox();
            txtValue.Text = "0";
            txtValue.Location = new Point(340, 22);
            txtValue.Size = new Size(100, 20);
            this.Controls.Add(txtValue);
            
            // Status
            lblStatus = new Label();
            lblStatus.Text = "Trạng thái: --";
            lblStatus.Location = new Point(450, 25);
            lblStatus.Size = new Size(200, 20);
            lblStatus.ForeColor = Color.Gray;
            this.Controls.Add(lblStatus);
            
            // Auto read checkbox
            chkAutoRead = new CheckBox();
            chkAutoRead.Text = "Tự động đọc";
            chkAutoRead.Location = new Point(10, 55);
            chkAutoRead.Size = new Size(100, 25);
            chkAutoRead.Checked = true;
            this.Controls.Add(chkAutoRead);
            
            // Read button
            btnRead = new Button();
            btnRead.Text = "Đọc ngay";
            btnRead.Location = new Point(120, 52);
            btnRead.Size = new Size(80, 28);
            btnRead.Click += BtnRead_Click;
            this.Controls.Add(btnRead);
            
            // Write button
            btnWrite = new Button();
            btnWrite.Text = "Ghi giá trị";
            btnWrite.Location = new Point(210, 52);
            btnWrite.Size = new Size(90, 28);
            btnWrite.Click += BtnWrite_Click;
            this.Controls.Add(btnWrite);
            
            // Last update
            lblLastUpdate = new Label();
            lblLastUpdate.Text = "Cập nhật: --";
            lblLastUpdate.Location = new Point(310, 58);
            lblLastUpdate.Size = new Size(200, 20);
            lblLastUpdate.ForeColor = Color.Gray;
            this.Controls.Add(lblLastUpdate);
            
            // Remove button
            btnRemove = new Button();
            btnRemove.Text = "Xóa khối";
            btnRemove.Location = new Point(1020, 20);
            btnRemove.Size = new Size(80, 25);
            btnRemove.ForeColor = Color.Red;
            btnRemove.Click += BtnRemove_Click;
            this.Controls.Add(btnRemove);
            
            // Description
            var lblDesc = new Label();
            lblDesc.Text = "DM: Data Memory | MR: Internal Relay | W: Work Area | R: Register";
            lblDesc.Location = new Point(10, 90);
            lblDesc.Size = new Size(500, 20);
            lblDesc.ForeColor = Color.DarkGray;
            lblDesc.Font = new Font(Font.FontFamily, 8);
            this.Controls.Add(lblDesc);
        }
        
        private void BtnRead_Click(object sender, EventArgs e)
        {
            chkAutoRead.Checked = true;
        }
        
        private void BtnWrite_Click(object sender, EventArgs e)
        {
            if (!parentForm.WriteDeviceValue(cmbDeviceType.Text, txtAddress.Text, txtValue.Text))
            {
                MessageBox.Show("Lỗi ghi giá trị!");
            }
            else
            {
                lblStatus.Text = "Trạng thái: Đã ghi " + txtValue.Text;
                lblStatus.ForeColor = Color.Blue;
            }
        }
        
        private void BtnRemove_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Xóa khối này?", "Xác nhận", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                parentForm.RemoveDeviceBlock(this);
            }
        }
        
        public string GetReadCommand()
        {
            string deviceType = cmbDeviceType.Text;
            string address = txtAddress.Text;
            
            if (deviceType == "DM" || deviceType == "W" || deviceType == "R" || deviceType == "Z")
            {
                return $"RD {deviceType}{address}.D\r";
            }
            else if (deviceType == "MR" || deviceType == "TM" || deviceType == "TR")
            {
                return $"RD {deviceType}{address}.U\r";
            }
            else
            {
                return $"RD {deviceType}{address}.D\r";
            }
        }
        
        public void UpdateValue(string response)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action<string>(UpdateValue), response);
                return;
            }
            
            try
            {
                // Parse response from PLC
                // Format: "RD DM1000.D 12345\r"
                if (response.Contains("RD") || response.Contains("RDS"))
                {
                    var parts = response.Split(new char[] { ' ', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length >= 3)
                    {
                        string value = parts[parts.Length - 1];
                        txtValue.Text = value;
                        
                        // For relay (bit) devices
                        if (cmbDeviceType.Text == "MR" || cmbDeviceType.Text == "TM" || cmbDeviceType.Text == "TR")
                        {
                            bool isOn = value == "1" || value.ToUpper() == "ON";
                            lblStatus.Text = "Trạng thái: " + (isOn ? "ON" : "OFF");
                            lblStatus.ForeColor = isOn ? Color.Green : Color.Red;
                        }
                        else
                        {
                            lblStatus.Text = "Trạng thái: OK";
                            lblStatus.ForeColor = Color.Green;
                        }
                        
                        lblLastUpdate.Text = "Cập nhật: " + DateTime.Now.ToString("HH:mm:ss");
                    }
                }
            }
            catch (Exception ex)
            {
                SetError(ex.Message);
            }
        }
        
        public void SetError(string error)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action<string>(SetError), error);
                return;
            }
            
            lblStatus.Text = "Lỗi: " + error;
            lblStatus.ForeColor = Color.Red;
        }
    }
}
