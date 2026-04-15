# PLCServer - Keyence PLC Monitor

## Mo ta
Ung dung C# Windows Forms de ket noi va giam sat PLC Keyence KV Series thong qua TCP Socket (Direct Ethernet).

## Tinh nang chinh

### 1. Ket noi PLC
- Nhap IP va Port tuy y
- Nut Ket noi/Ngat ket noi
- Hien thi trang thai ket noi (mau xanh/do)

### 2. Quan ly Device (DM, MR, W, ...)
- Them/xoa khoi device tuy y
- Ho tro cac loai device:
  * DM: Data Memory
  * MR: Internal Relay
  * W: Work Area
  * R: Register
  * Z: Index Register
  * TM: Timer
  * TR: Timer Relay
  * CM: Counter
  * EM: Expansion Relay
  * FM: File Register

### 3. Doc/Ghi device
- Tu dong doc (checkbox)
- Nut doc ngay
- Nut ghi gia tri
- Hien thi trang thai ON/OFF (cho relay)
- Thoi gian cap nhat cuoi

## Cau truc file

`
PLCServer/
„¥„Ÿ„Ÿ Program.cs        # Entry point
„¥„Ÿ„Ÿ MainForm.cs       # Form chinh, quan ly ket noi
„¥„Ÿ„Ÿ DeviceBlock.cs    # UserControl cho tung device
„¥„Ÿ„Ÿ PLCServer.csproj  # Project file
„¤„Ÿ„Ÿ App.config        # Config file
`

## Thong so ky thuat

| Thong so | Gia tri |
|----------|---------|
| Framework | .NET Framework 4.8 |
| Protocol | TCP Socket (Direct Ethernet) |
| Encoding | Shift-JIS (932) |
| Default IP | 192.168.1.10 |
| Default Port | 8501 |
| Timeout | 5000ms |

## Lenh PLC su dung

`
RD  [device][address]     - Doc 1 device
WR  [device][address] [val] - Ghi gia tri
`

Vi du:
- RD DM1000.D  - Doc Data Memory 1000
- RD MR100.U   - Doc Internal Relay 100
- WR DM1000.D 123 - Ghi 123 vao DM1000

## Build va chay

1. Mo solution trong Visual Studio
2. Build (Ctrl+Shift+B)
3. Chay (F5)

Hoac dung command line:
`
msbuild PLCServer.csproj /p:Configuration=Release
bin\Release\PLCServer.exe
`

## Giao dien

`
+-----------------------------+
|  Ket noi PLC                |
|  IP: [192.168.1.10] Port: [8501]  [Ket noi] [Ngat] |
|  Trang thai: Da ket noi (XANH) |
+-----------------------------+
| [+ Them khoi Device]        |
+-----------------------------+
| +-------------------------+ |
| | Device Block            | |
| | Loai: [DM] Dia chi: [1000] | |
| | Gia tri: [12345] Trang thai: OK (XANH) | |
| | [x] Tu dong doc [Doc ngay] [Ghi gia tri] [Xoa] | |
| +-------------------------+ |
| +-------------------------+ |
| | Device Block            | |
| | Loai: [MR] Dia chi: [100]  | |
| | Gia tri: [1] Trang thai: ON (XANH) | |
| | [x] Tu dong doc [Doc ngay] [Ghi gia tri] [Xoa] | |
| +-------------------------+ |
+-----------------------------+
`

## Luu y
- Dam bao PC va PLC cung mang
- Port 8501 khong bi firewall chan
- PLC da bat Direct Ethernet mode
