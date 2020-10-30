using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ActUtlTypeLib;
using System.IO;
using System.IO.Ports;
using System.Threading;
using System.Timers;
using SHIV_PhongCachAm.PopupWindows;
using Excel = Microsoft.Office.Interop.Excel;
using System.Diagnostics;
using System.Windows.Controls.DataVisualization.Charting;
using System.Collections.ObjectModel;
using BMS;
using BMS.Utils;
using System.Data;
using BMS.Business;
using System.Text.RegularExpressions;
using BMS.Model;
using System.Collections;
using System.Net;
using System.Net.Sockets;

// Test Save F5
namespace SHIV_PhongCachAm
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		#region <Khai báo biến>
		int currentStageRun = 0;
		int currentChart = 0;
		private bool plcButtonForward;
		private bool plcButtonBackward;
		private DataStoreObject dtVongquay, dtDongdien, dtNhapluc, dtDorung, dtTiengon, dtDienap;
		private valueNguoidanhgiaObject dtAmsac, dtHuongquay;
		private dulieuStruct dlDorung, dlTiengon;
		private labelObject lbdMaOrder, lbdSoThuTuSanPham, lbdNguoiVanHanh, lbdPID, lbdMoTaSanPham, lbdGiamToc;
		private labelObject lbdDatetime, lbdStatus;
		private labelObject lbdDienap;
		private labelObject lbdTanso;
		private labelObject lbTcDongdien, lbTcNhapluc, lbTcVongquay, lbTcDorung, lbTcTiengon, lbTcDienap, lbTcTanso;
		private labelObject lbdTimerCountCycle = new labelObject();
		private valueParse is1Parse;
		private specialOutputObject specialOutputObject;

		private float htVongquay;
		private float htDongdien;
		private float htDienap;
		private float htTanso;
		private float htNhapluc;
		private float htDorung;
		private float htTiengon;

		// Kết nối Excel
		Excel.Application myExcel;
		Excel.Worksheet workSheetDatabase;
		Excel.Worksheet workSheetKehoach;
		Excel.Worksheet myDataTemplateWorksheet;
		private int countDatainTemplate;


		// Kết nối PLC
		ActUtlType plcFX5U = new ActUtlType();
		ActUtlType plcFX3G_Shiv = new ActUtlType();
		// Kết nối RS232
		SerialPort COMCurrent, COMNoise;
		private System.Timers.Timer timerCOM;
		private checkDone checkDoneForward;
		private checkDone checkDoneBackward;

		// Thread
		private Thread continuesThread;
		private Thread PLC;
		private Thread PLC_Shiv;
		private string excelLink;
		private string DatabasePath;
		private string tempCurrentString;
		private string tempNoiseString;
		private int countGetCurrent;
		private DateTime beginTimeCycle;
		private TimeSpan currentTimeCycle;
		private valueObject valueSettingMaxRange;
		private Style styleDorung;
		private Style styleTiengon;
		private int currentM0, currentM1, currentM3;
		private int oldM0, oldM1, oldM3;
		private int endRangeCollum;
		private bool chartBusy;
		private int countNG_Noise = 0;

		public ObservableCollection<ChartViewItem> MyValue { get; private set; }
		public ObservableCollection<ChartViewItem> MyValue_TempTiengon { get; private set; }
		public ObservableCollection<ChartViewItem> MyMax { get; private set; }
		public ObservableCollection<ChartViewItem> MyMin { get; private set; }
		#endregion

		string _socketIPAddress = "192.168.1.46";
		int _socketPort = 3000;
		Socket _socket;
		ASCIIEncoding _encoding = new ASCIIEncoding();
		private Thread _threadSocket;

		public MainWindow()
		{
			//Load ra config trong database lấy takt time, địa chỉ tcp, port
			DataTable dtConfig = TextUtils.Select("SELECT TOP 1 * FROM dbo.AndonConfig with (nolock)");
			//_taktTime =TextUtils.ToInt( dtConfig.Rows[0]["Takt"]);
			_socketIPAddress = TextUtils.ToString(dtConfig.Rows[0]["TcpIp"]);
			_socketPort = TextUtils.ToInt(dtConfig.Rows[0]["SocketPort"]);
			try
			{
				IPAddress ipAddOut = IPAddress.Parse(_socketIPAddress);
				IPEndPoint endPoint = new IPEndPoint(ipAddOut, _socketPort);
				_socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
				_socket.Connect(endPoint);
			}
			catch (Exception ex)
			{
				_socket = null;
			}
			startThreadSocket();

			InitializeComponent();
			//specialOutputObject = new specialOutputObject();
			StartExcelApplication();
			DataInitialize();
			ConnectionInitialize();
			StartContinues();
			lblIsUse.Content = "Khong Su Dung";

		}
		void startThreadSocket()
		{
			_threadSocket = new Thread(resetSocket);
			_threadSocket.IsBackground = true;
			_threadSocket.Start();
		}

		void resetSocket()
		{
			while (true)
			{
				Thread.Sleep(800);
				if (_socket == null)
				{
					//Load ra config trong database lấy takt time, địa chỉ tcp, port
					DataTable dtConfig = TextUtils.Select("SELECT TOP 1 * FROM dbo.AndonConfig with (nolock)");
					//_taktTime =TextUtils.ToInt( dtConfig.Rows[0]["Takt"]);
					_socketIPAddress = TextUtils.ToString(dtConfig.Rows[0]["TcpIp"]);
					_socketPort = TextUtils.ToInt(dtConfig.Rows[0]["SocketPort"]);
					try
					{
						IPAddress ipAddOut = IPAddress.Parse(_socketIPAddress);
						IPEndPoint endPoint = new IPEndPoint(ipAddOut, _socketPort);
						_socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
						_socket.Connect(endPoint);
					}
					catch (Exception ex)
					{
						_socket = null;
					}
				}
			}
		}
		DataTable _dtData;

		/// <summary>
		/// Gửi thông điệp lên andon
		/// </summary>
		/// <param name="value">Giá trị, trạng thái</param>
		/// <param name="type">1:sự cố, 2: đã hoàn thành, 3: cập nhật SL thực tế, 4: khởi động ca</param>
		void sendDataTCP(string value, string type)
		{
			try
			{
				//Gửi tín hiệu delay xuống server Andon qua TCP/IP
				if (_socket != null && _socket.Connected)
				{
					string sendData = string.Format("{0};{1};{2}", "CD9", value, type);
					byte[] data = _encoding.GetBytes(sendData);
					_socket.Send(data);
				}
			}
			catch (Exception ex)
			{
				_socket = null;
			}
		}

		/// <summary>
		/// Khởi tạo giá trị dt, dl
		/// </summary>
		private void DataInitialize()
		{
			// Cập nhật trạng thái điện áp
			is1Parse = new valueParse();
			chk1PhaOrder.DataContext = is1Parse;

			// Cập nhật trạng thái xuất đặc biệt
			specialOutputObject = new specialOutputObject();

			// Load DB File _ Cập nhật Database khi bắt đầu chương trình
			if (Settingsm.Default.DBLocation != "")
			{
				if (File.Exists(Settingsm.Default.DBLocation))
					UpdateExcelDatabaseWhenChangeLink(Settingsm.Default.DBLocation);
				OpenExcelResultFile();
			}

			// Trạng thái PLC
			currentM0 = currentM1 = currentM3 = 0;

			// Trạng thái điện 1 pha
			is1Parse.Value = false;

			// Đặt lại trạng thái lấy dữ liệu
			currentStageRun = 1;

			dtVongquay = new DataStoreObject();
			dtDongdien = new DataStoreObject();
			dtNhapluc = new DataStoreObject();
			dtDorung = new DataStoreObject();
			dtTiengon = new DataStoreObject();
			dtTiengon.dataTiengon = true;
			dtDienap = new DataStoreObject();
			dtAmsac = new valueNguoidanhgiaObject();
			dtHuongquay = new valueNguoidanhgiaObject();


			dlDorung = new dulieuStruct(200, 200);
			dlTiengon = new dulieuStruct(200, 200);
			lbdNguoiVanHanh = new labelObject();
			lbdMaOrder = new labelObject();
			lbdSoThuTuSanPham = new labelObject();
			lbdPID = new labelObject();
			lbdMoTaSanPham = new labelObject();
			lbdGiamToc = new labelObject();
			lbdDienap = new labelObject();
			lbdTanso = new labelObject();


			lbTcDongdien = new labelObject();
			lbTcNhapluc = new labelObject();
			lbTcVongquay = new labelObject();
			lbTcDorung = new labelObject();
			lbTcTiengon = new labelObject();
			lbTcDienap = new labelObject();
			lbTcTanso = new labelObject();

			// Khởi tạo giá trị ht
			htVongquay = 0;
			htDongdien = 0;
			htNhapluc = 0;
			htDorung = 0;
			htTiengon = 0;
			htDienap = 0;
			htDorung = 0;

			for (int i = 0; i < 200; i++)
			{
				dlDorung.thuan[i] = (float)0.00001;
				dlDorung.nghich[i] = (float)0.00001;
				dlTiengon.thuan[i] = (float)0.00001;
				dlTiengon.nghich[i] = (float)0.00001;
			}

			lblVongquayFwdMax.DataContext = dtVongquay.GiatriThuan;
			lblVongquayBwdMax.DataContext = dtVongquay.GiatriNghich;
			lblVongquayLech.DataContext = dtVongquay.giatriLech;
			lblOKVongquay.DataContext = dtVongquay.GiatriDanhgia;
			//dtVongquay.giatriDanhgia.Value = 2;
			lblNGVongquay.DataContext = dtVongquay.GiatriDanhgia;

			//lblDongdienFwdMax.DataContext = dtDongdien.giatriThuan;
			lblDongdienFwdMax.DataContext = dtDongdien.GiatriThuan;
			lblDongdienBwdMax.DataContext = dtDongdien.GiatriNghich;
			lblDongdienLech.DataContext = dtDongdien.giatriLech;
			lblOKDongdien.DataContext = dtDongdien.GiatriDanhgia;
			//dtDongdien.giatriDanhgia.Value = 1;
			lblNGDongdien.DataContext = dtDongdien.GiatriDanhgia;

			lblDorungFwdMax.DataContext = dtDorung.GiatriThuan;
			//lblDorungFwdMax.DataContext = dtDorung;

			lblDorungBwdMax.DataContext = dtDorung.GiatriNghich;
			lblDorungLech.DataContext = dtDorung.giatriLech;
			lblOKDorung.DataContext = dtDorung.GiatriDanhgia;
			lblNGDorung.DataContext = dtDorung.GiatriDanhgia;

			lblTiengonFwdMax.DataContext = dtTiengon.GiatriThuan;
			lblTiengonBwdMax.DataContext = dtTiengon.GiatriNghich;
			lblTiengonLech.DataContext = dtTiengon.giatriLech;
			lblOKTiengon.DataContext = dtTiengon.GiatriDanhgia;
			lblNGTiengon.DataContext = dtTiengon.GiatriDanhgia;

			lblNhaplucFwdMax.DataContext = dtNhapluc.GiatriThuan;
			//lblNhaplucFwdMax.DataContext = dtNhapluc;
			lblNhaplucBwdMax.DataContext = dtNhapluc.GiatriNghich;
			lblNhaplucLech.DataContext = dtNhapluc.giatriLech;
			lblOKNhapluc.DataContext = dtNhapluc.GiatriDanhgia;
			lblNGNhapluc.DataContext = dtNhapluc.GiatriDanhgia;

			// Binding Tiêu chuẩn
			lblTcVongquay.DataContext = lbTcVongquay;
			lblTcDongdien.DataContext = lbTcDongdien;
			lblTCNhapluc.DataContext = lbTcNhapluc;
			lblTcDorung.DataContext = lbTcDorung;
			lblTcTiengon.DataContext = lbTcTiengon;
			lblDienapChuan.DataContext = lbTcDienap;
			lblTansoChuan.DataContext = lbTcTanso;

			// Binding giá trị Order - PID
			lblSTTSanpham.DataContext = lbdSoThuTuSanPham;
			lblMaOrder.DataContext = lbdMaOrder;
			lblNguoiVanhanh.DataContext = lbdNguoiVanHanh;
			lblPID.DataContext = lbdPID;
			lblMotaSanpham.DataContext = lbdMoTaSanPham;
			lblGiamtoc.DataContext = lbdGiamToc;

			// Binding các giá trị tự đánh giá
			lblHuongquayMax.DataContext = dtHuongquay.giatriMax;
			lblAmsacThuan.DataContext = dtAmsac.giatriThuan;
			lblAmsacNghich.DataContext = dtAmsac.giatriNghich;

			lblOKXuatluc.DataContext = dtHuongquay.giatriDanhgia;
			lblNGXuatluc.DataContext = dtHuongquay.giatriDanhgia;
			lblOKAmsac.DataContext = dtAmsac.giatriDanhgia;
			lblNGAmsac.DataContext = dtAmsac.giatriDanhgia;

			lblDienapThucte.DataContext = lbdDienap;
			lblTansoThucte.DataContext = lbdTanso;

			// Get Datetime
			lbdDatetime = new labelObject();
			lblDatetime.DataContext = lbdDatetime;
			lbdDatetime.Value = DateTime.Now.ToString("yy/MM/dd");
			// Reset Status
			lbdStatus = new labelObject();
			lblStatus.DataContext = lbdStatus;
			lbdStatus.Value = "Ready";

			// Khai báo hiển thị số đếm Cycle
			lblTimerCycle.DataContext = lbdTimerCountCycle;

			// Khởi tạo Binding đồ thị
			valueSettingMaxRange = new valueObject();
			valueSettingMaxRange.Value = 1;

			styleDorung = new Style(typeof(LineDataPoint));
			Setter tempSet1 = new Setter(LineDataPoint.BackgroundProperty, Brushes.MidnightBlue);
			styleDorung.Setters.Add(tempSet1);
			tempSet1 = new Setter(LineDataPoint.WidthProperty, (double)0);
			styleDorung.Setters.Add(tempSet1);
			tempSet1 = new Setter(LineDataPoint.HeightProperty, (double)0);
			styleDorung.Setters.Add(tempSet1);

			styleTiengon = new Style(typeof(LineDataPoint));
			tempSet1 = new Setter(LineDataPoint.BackgroundProperty, Brushes.DarkGreen);
			styleTiengon.Setters.Add(tempSet1);
			tempSet1 = new Setter(LineDataPoint.WidthProperty, (double)0);
			styleTiengon.Setters.Add(tempSet1);
			tempSet1 = new Setter(LineDataPoint.HeightProperty, (double)0);
			styleTiengon.Setters.Add(tempSet1);
		}

		/// <summary>
		/// Tạo mới và mở ứng dụng Excel
		/// Update các file Excel dữ liệu từ Server nếu có
		/// </summary>
		private void StartExcelApplication()
		{
			killAppExcel();
			myExcel = new Excel.Application();

			// Copy DB File	DB và KH từ Server
			string DBServerStr = Settingsm.Default.DBServer;
			string KHServerStr = Settingsm.Default.KHServer;
			string DbLocalStri = Settingsm.Default.DBLocation;
			string KhLoalStri = DbLocalStri.Substring(0, DbLocalStri.LastIndexOf("\\") + 1) + "KHLocal.xlsx";

			if (File.Exists(DBServerStr)) File.Copy(DBServerStr, DbLocalStri, true);
			if (File.Exists(KHServerStr)) File.Copy(KHServerStr, KhLoalStri, true);
		}

		/// <summary>
		/// Tạo luồng lấy dữ liệu phần mềm (nhiệm vụ kiểm tra bước hiện tại, chạy lần lượt các bước lấy dữ liệu)
		/// </summary>
		private void StartContinues()
		{
			continuesThread = new Thread(Chutrinh_LayDuLieu);
			continuesThread.Name = "Thread chu trinh Lay du lieu";
			continuesThread.IsBackground = false;
			continuesThread.Start();
		}

		/// <summary>
		/// Khởi tạo kết nối PLC, cổng COM
		/// Tạo timer gửi dữ liệu cổng COM
		/// </summary>
		private void ConnectionInitialize()
		{
			// Khởi tạo Thread cập nhật dữ liệu từ PLC
			plcFX5U.ActLogicalStationNumber = 11;
			PLC = new Thread(UpdateDataFromPLC);
			PLC.IsBackground = false;
			PLC.Name = "PLC Thread";
			PLC.Start();

			// Khởi tạo Thread cập nhật dữ liệu từ PLC - Shiv
			plcFX3G_Shiv.ActLogicalStationNumber = 12;
			PLC_Shiv = new Thread(UpdateDataFromPLCShiv);
			PLC_Shiv.IsBackground = false;
			PLC_Shiv.Name = "PLC Shiv Thread";
			PLC_Shiv.Start();

			// Khởi tạo kết nối COM lấy dữ liệu dòng điện nhập lực
			COMCurrent = new SerialPort("COM7", 9600, Parity.None, 8, StopBits.One);
			COMCurrent.DataReceived += receiveDataFromCOMCurrent;
			COMCurrent.ReadTimeout = 2000;
			COMCurrent.WriteTimeout = 2000;
			COMCurrent.Open();

			// Khởi tạo kết nối COM lấy dữ liệu tiếng ồn
			COMNoise = new SerialPort("COM10", 9600, Parity.None, 8, StopBits.One);
			COMNoise.DataReceived += receiveDataFromCOMNoise;
			COMNoise.ReadTimeout = 2000;
			COMNoise.WriteTimeout = 2000;
			COMNoise.Open();

			// Chạy Timer Request Data COM
			timerCOM = new System.Timers.Timer();
			timerCOM.Interval = 450;
			timerCOM.Elapsed += SentRequetDataCOM;
			timerCOM.Start();
		}

		/// <summary>
		/// Lấy dữ liệu từ PLC
		/// Vòng quay: D100
		/// Độ rung: D200
		/// </summary>
		private void UpdateDataFromPLC()
		{
			plcFX5U.Open();
			while (true)
			{
				int temp;
				int iret = plcFX5U.ReadDeviceRandom("D100", 1, out temp);
				try
				{
					htVongquay = (float)(temp * 3 / 12);
				}
				catch
				{
					htVongquay = 0;
				}

				iret = plcFX5U.ReadDeviceRandom("D200", 1, out temp);
				try
				{
					htDorung = (float)(temp * 0.026);
				}
				catch
				{
					htDorung = 0;
				}
				Thread.Sleep(50);
			}
		}

		/// <summary>
		/// Lấy dữ liệu chiều quay, nút nhấn đầu vào từ PLC của Sumitomo
		/// </summary>
		private void UpdateDataFromPLCShiv()
		{
			plcFX3G_Shiv.Open();
			while (true)
			{
				Thread.Sleep(50);
				// Lấy giá trị X3
				int temp = 0;
				int iret = plcFX3G_Shiv.ReadDeviceRandom("M203", 1, out temp);
				if (iret == 0)
				{
					if (temp == 1)
					{
						plcButtonBackward = true;
					}
					else
					{
						plcButtonBackward = false;
					}
				}

				// Lấy giá trị X4
				iret = plcFX3G_Shiv.ReadDeviceRandom("M204", 1, out temp);
				if (iret == 0)
				{
					if (temp == 1)
					{
						plcButtonForward = true;
					}
					else
					{
						plcButtonForward = false;
					}
				}

				// Lấy giá trị X? Nút nhấn Stop

				oldM0 = currentM0;
				oldM1 = currentM1;
				oldM3 = currentM3;

				iret = plcFX3G_Shiv.ReadDeviceRandom("M208", 1, out temp);
				if (temp == 0)
				{
					iret = plcFX3G_Shiv.ReadDeviceRandom("M0", 1, out currentM0);
					iret = plcFX3G_Shiv.ReadDeviceRandom("M1", 1, out currentM1);
					iret = plcFX3G_Shiv.ReadDeviceRandom("M3", 1, out currentM3);
				}

				if (iret == 0)
				{
					if ((temp == 1) && ((oldM0 == 1) || (oldM1 == 1) || (oldM3 == 1)))
					{
						NutnhanStopDungChutrinh();
					}
				}
			}
		}

		/// <summary>
		/// Nhấn nút Stop khi PLC đang chạy Auto, sẽ dừng chương trình, về trạng thái đợi tín hiệu bắt đầu chạy lại
		/// </summary>
		private void NutnhanStopDungChutrinh()
		{
			currentStageRun = 0;
			lbdStatus.Value = "Dừng chu trình kiểm tra - đợi chạy lại";
		}

		/// <summary>
		/// Gửi lệnh lấy dữ liệu qua cổng COM, tần số lấy theo timerCOM theo chu kỳ timer
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void SentRequetDataCOM(object sender, ElapsedEventArgs e)
		{
			// Sua ngay 23/12 - Them try catch de khoi dong lai cong com khi bi loi
			try
			{
				// Gui cmd get Current
				if (countGetCurrent == 3)
				{
					COMCurrent.WriteLine("MEASure?\r\n");
					countGetCurrent = 0;
				}
				else
				{
					countGetCurrent += 1;
				}

				// Gui cmd get Noise
				COMNoise.WriteLine("DOD?\r\n");
				countNG_Noise += 1;
				if (countNG_Noise > 4)
				{
					countNG_Noise = 0;
					if (!File.Exists("E:\\logErrorNoise.txt")) File.WriteAllText("E:\\logErrorNoise.txt", "Error Com Noise - " + DateTime.Now.ToString() + "\r\n");
					else File.AppendAllText("E:\\logErrorNoise.txt", "Error Com Noise - " + DateTime.Now.ToString() + "\r\n");
					if (COMNoise.IsOpen) COMNoise.Close();
					COMNoise.Open();
				}

				// 
			}
			catch (Exception ex)
			{
				if (!File.Exists("E:\\logErrorNoise.txt")) File.WriteAllText("E:\\logErrorNoise.txt", ex.ToString() + "Error Com Noise - " + DateTime.Now.ToString() + "\r\n");
				else File.AppendAllText("E:\\logErrorNoise.txt", ex.ToString() + "Error Com Noise - " + DateTime.Now.ToString() + "\r\n");

				if (!COMNoise.IsOpen)
				{
					try
					{
						COMNoise.Open();
					}
					catch { }
				}
			}
		}

		/// <summary>
		/// Xử lý dữ liệu nhận về từ cổng COM máy đo tiếng ồn
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void receiveDataFromCOMNoise(object sender, SerialDataReceivedEventArgs e)
		{
			countNG_Noise = 0;
			string tempReceiveString = "";
			tempReceiveString = COMNoise.ReadExisting();
			if (tempReceiveString.IndexOf("$") >= 0)
			{
				tempReceiveString = tempNoiseString + tempReceiveString;
				tempNoiseString = "";
				if (tempReceiveString.IndexOf("R") >= 0)
				{
					//MessageBox.Show(tempReceiveString.IndexOf(" ").ToString());
					tempReceiveString = tempReceiveString.Substring(tempReceiveString.IndexOf(" "));
				}
				if (tempReceiveString.Length > 5)
				{
					string tempNoise = tempReceiveString.Split(',')[0];
					htTiengon = float.Parse(tempNoise);
				}

				//htTiengon = htTiengon + (float)0.456789;
			}
			else
			{
				tempNoiseString += tempReceiveString;
			}
		}

		/// <summary>
		/// Xử lý dữ liệu nhận về từ cổng COM đo dòng điện, điện áp, công suất
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void receiveDataFromCOMCurrent(object sender, SerialDataReceivedEventArgs e)
		{
			Thread.Sleep(100);
			string tempReceiveString = "";
			//"V1 +222.74E+0;V2 +224.03E+0;V3 +224.77E+0;V0 +223.85E+0;A1 +000.58E-3;A2 +000.00E-3;A3 +000.00E-3;A0 +000.19E-3;W1 +777.77E+9;W2 +777.77E+9;W0 +000.00E+0;VA1 +777.77E+9;VA2 +777.77E+9;VA0 +000.07E+0;VAR1 +777.77E+9;VAR2 -777.77E+9;VAR0 +000.13E+0;PF1 +777.77E+9;PF2 -777.77E+9;PF0 +0.0000E+0;DEG1 +777.77E+9;DEG2 -777.77E+9;DEG0 +090.00E+0;IP1 +0.0000E+0;IP2 +0.0000E+0;IP3 +0.0000E+0;FREQ +59.997E+0;AH1 +000.000E-3;AH2 +000.000E-3;AH3 +000.000E-3;PWH1 +7777.77E+9;PWH2 +7777.77E+9;PWH0 +000.000E+0;MWH1 +7777.77E+9;MWH2 +7777.77E+9;MWH0 +000.000E+0;WH1 +7777.77E+9;WH2 +7777.77E+9;WH0 +000.000E+0;TIME 00000,00,00";
			try
			{
				tempReceiveString = COMCurrent.ReadExisting();
			}
			catch { }
			Console.WriteLine(tempReceiveString);
			if (tempReceiveString.IndexOf("\n") >= 0)
			{
				tempReceiveString = tempCurrentString + tempReceiveString;
				tempCurrentString = "";
				string[] tempArr = tempReceiveString.Split(';');
				foreach (var item in tempArr)
				{
					// Lấy giá trị dòng điện
					if (is1Parse.Value)
					{
						if ((item.IndexOf("A1") >= 0) && (item.IndexOf("VA") < 0))
						{
							htDongdien = float.Parse(item.Substring(3), System.Globalization.NumberStyles.Float);
							Console.WriteLine("Dong Dien " + htDongdien.ToString() + item);
						}
					}
					else
					{
						if ((item.IndexOf("A0") >= 0) && (item.IndexOf("VA") < 0))
						{
							htDongdien = float.Parse(item.Substring(3), System.Globalization.NumberStyles.Float);
							Console.WriteLine("Dong Dien " + htDongdien.ToString() + item);
						}
					}
					// Lấy giá trị nhập lực
					if (item.IndexOf("W0") >= 0)
					{
						htNhapluc = float.Parse(item.Substring(3), System.Globalization.NumberStyles.Float);
					}
					// Lấy giá trị điện áp
					if (item.IndexOf("V1") >= 0)
					{
						htDienap = float.Parse(item.Substring((item.IndexOf("V1") + 3)), System.Globalization.NumberStyles.Float);
					}

					// Lấy giá trị tần số
					if (item.IndexOf("FREQ") >= 0)
					{
						htTanso = float.Parse(item.Substring(5), System.Globalization.NumberStyles.Float);
					}
				}
			}
			else
			{
				tempCurrentString += tempReceiveString;
			}
		}


		string _order;
		string _productCode;
		string _tienTo="";
		string _stt="";
		int _stepID;
		string _stepCode;
		string _stepName;
		/// <summary>
		/// Chưa dùng
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void lblSTTSanpham_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			/// Spare

		}
		AndonModel _andon;

		private void lblLanguage_PreviewMouseDown(object sender, MouseButtonEventArgs e)
		{
			currentStageRun = 19;
		}
		private void lblDatetime_PreviewMouseDown(object sender, MouseButtonEventArgs e)
		{
			currentStageRun = 1;
		}

		/// <summary>
		/// Cập nhật vị trí hiện tại trong file dữ liệu tổng
		/// Tính toán và trả về dòng trống đầu tiên countDatainTemplate
		/// </summary>
		private void OpenExcelResultFile()
		{
			string currentDir = Environment.CurrentDirectory + "\\" + "Temp_Sum_PCA_UD.xlsx";
			string currentDailyData = "E:\\LOG_ALL\\" + DateTime.Now.ToString("yyyy_MM_dd") + "_DataCollect" + ".xlsx";
			if (!File.Exists(@currentDailyData))
			{
				File.Copy(@currentDir, @currentDailyData);
			}

			if (File.Exists(@currentDailyData))
			{
				myExcel.Workbooks.Open(@currentDailyData);
				myDataTemplateWorksheet = myExcel.ActiveWorkbook.Worksheets["Main"];
				//for (int i = 1; i < 10000; i++)
				//{
				//	if (((Excel.Range)myDataTemplateWorksheet.Cells[i, 1]).Value2 == "") { countDatainTemplate = i - 1; break; }
				//	Excel.Range temp111 = (Excel.Range)myDataTemplateWorksheet.Cells[i, 1];
				//	temp111.FindNext("");
				//}
				Excel.Range tempRange = myDataTemplateWorksheet.Range[myDataTemplateWorksheet.Cells[1, 1], myDataTemplateWorksheet.Cells[10000, 1]];
				tempRange = tempRange.Find("");
				countDatainTemplate = tempRange.Row - 1;
			}
		}

		/// <summary>
		/// Nhấn nút F1 để chuyển qua lại giữa 4 đồ thị
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void btnF1_PreviewMouseDown(object sender, MouseButtonEventArgs e)
		{
			if (currentChart < 3) currentChart += 1;
			else currentChart = 0;

			PlotChart(currentChart);
		}

		/// <summary>
		/// Chu trình Stage lấy dữ liệu
		/// Chiều thuận, chiều nghịch
		/// </summary>
		private async void Chutrinh_LayDuLieu()
		{
			currentStageRun = 0;
			while (true)
			{
				// Chu kỳ 100ms
				Thread.Sleep(75);
				if (currentStageRun <= 1) currentTimeCycle = beginTimeCycle - beginTimeCycle;
				else currentTimeCycle = DateTime.Now - beginTimeCycle;
				if (currentTimeCycle.Seconds > 0) lbdTimerCountCycle.Value = currentTimeCycle.Seconds.ToString();
				else lbdTimerCountCycle.Value = "";
				if (currentStageRun > 1) UpdateDisplayColor();
				switch (currentStageRun)
				{
					// Reset Data to Default 
					case 0:
						//ResetAllData();
						checkDoneForward = new checkDone(false);
						checkDoneBackward = new checkDone(false);
						currentStageRun = 1;
						break;
					// Wait Button Forward + Wait 5s
					case 1:
						if ((plcButtonForward)) // Mô phỏng nút nhấn
						{
							if (CheckConditionRun())
							{
								ResetValueData();
								plcFX5U.SetDevice2("M2000", 0); // Giá trị cho phép chạy chiều nghịch => false
								beginTimeCycle = DateTime.Now;
								lbdStatus.Value = "Đợi 10s cho động cơ chạy ổn định";
								plcFX5U.SetDevice("Y3", 1);
								await Wait5Second();
								await Wait5Second();
								//await Wait3Second();
								plcFX5U.SetDevice("Y3", 0);
								currentStageRun = 3;
								currentChart = 0;
							}
							else
							{
								MessageBox.Show("Thiếu điều kiện chạy!", "Missing", MessageBoxButton.OK);
							}
						}
						break;
					// Collect and Update Data
					case 3:
						// Get Datetime
						OnMotorCount();
						lbdStatus.Value = "Lấy dữ liệu chiều thuận";

						lbdDatetime.Value = DateTime.Now.ToString("yy/MM/dd");

						System.Console.Write("One Step Run - ");
						UpdateDulieu("Forward", ref checkDoneForward);
						if (checkDoneForward.Sum())
						{
							currentStageRun = 5;
						}
						break;
					// Check Direction
					case 5:
						dtVongquay.GiatriThuan.Value = htVongquay;

						currentStageRun = 7;
						break;
					// Check Amsac
					case 7:

						// Chỉnh sửa
						Thread.Sleep(100);
						plcFX5U.SetDevice2("M2000", 1); // Giá trị cho phép chạy chiều nghịch => true
						currentStageRun = 9;
						break;
					// Finish Forward
					case 9:
						currentStageRun = 11;
						break;
					// Wait Button Backward
					case 11:
						OffMotorCount();
						if ((plcButtonBackward))
						{
							beginTimeCycle = DateTime.Now;
							lbdStatus.Value = "Đợi 10s quay nghịch";
							await Wait5Second();
							await Wait5Second();
							currentStageRun = 13;
							currentChart = 1;
						}
						break;
					// Collect and Update Data
					case 13:
						OnMotorCount();
						lbdStatus.Value = "Lấy dữ liệu chiều nghịch";

						UpdateDulieu("Backward", ref checkDoneBackward);
						if (checkDoneBackward.Sum())
						{
							currentStageRun = 15;
						}
						break;
					// Check Direction
					case 15:
						dtVongquay.GiatriNghich.Value = htVongquay;
						CheckAmsac("Forward");
						CheckAmsac("Backward");

						currentStageRun = 17;
						break;
					// Check Amsac
					case 17:
						checkRotaryDirection();
						currentStageRun = 19;
						OffMotorCount();
						break;
					// Finish Forward
					case 19:
						lbdStatus.Value = "Hoàn tất lấy dữ liệu";

						//MessageBox.Show("Done");
						Dispatcher.Invoke(new Action(() =>
						{
							//DataInitialize();
						}));
						XuatRaFileCSVvaExcel();
						lbdSoThuTuSanPham.Value = "";
						currentStageRun = 0;
						break;
					default:
						break;
				}
			}
		}

		/// <summary>
		/// Cập nhật màu của các ô Data trong giao diện khi giá trị dữ liệu thay đổi
		/// </summary>
		private void UpdateDisplayColor()
		{
			Dispatcher.Invoke(() =>
			{
				float tempValue, tempMax, tempMin;
				tempValue = dtVongquay.GiatriThuan.Value; tempMax = dtVongquay.Max; tempMin = dtVongquay.Min;
				dtVongquay.GiatriThuan.Color = checkInRange(tempValue, tempMax, tempMin);
				tempValue = dtVongquay.GiatriNghich.Value; tempMax = dtVongquay.Max; tempMin = dtVongquay.Min;
				dtVongquay.GiatriNghich.Color = checkInRange(tempValue, tempMax, tempMin);

				tempValue = dtDongdien.GiatriThuan.Value; tempMax = dtDongdien.Max; tempMin = dtDongdien.Min;
				dtDongdien.GiatriThuan.Color = checkInRange(tempValue, tempMax, tempMin);
				tempValue = dtDongdien.GiatriNghich.Value; tempMax = dtDongdien.Max; tempMin = dtDongdien.Min;
				dtDongdien.GiatriNghich.Color = checkInRange(tempValue, tempMax, tempMin);

				tempValue = dtNhapluc.GiatriThuan.Value; tempMax = dtNhapluc.Max; tempMin = dtNhapluc.Min;
				dtNhapluc.GiatriThuan.Color = checkInRange(tempValue, tempMax, tempMin);
				tempValue = dtNhapluc.GiatriNghich.Value; tempMax = dtNhapluc.Max; tempMin = dtNhapluc.Min;
				dtNhapluc.GiatriNghich.Color = checkInRange(tempValue, tempMax, tempMin);

				tempValue = dtDorung.GiatriThuan.Value; tempMax = dtDorung.Max; tempMin = dtDorung.Min;
				dtDorung.GiatriThuan.Color = checkInRange(tempValue, tempMax, tempMin);
				tempValue = dtDorung.GiatriNghich.Value; tempMax = dtDorung.Max; tempMin = dtDorung.Min;
				dtDorung.GiatriNghich.Color = checkInRange(tempValue, tempMax, tempMin);

				tempValue = dtTiengon.GiatriThuan.Value; tempMax = dtTiengon.Max; tempMin = dtTiengon.Min;
				dtTiengon.GiatriThuan.Color = checkInRange(tempValue, tempMax, tempMin);
				tempValue = dtTiengon.GiatriNghich.Value; tempMax = dtTiengon.Max; tempMin = dtTiengon.Min;
				dtTiengon.GiatriNghich.Color = checkInRange(tempValue, tempMax, tempMin);
			});
		}

		/// <summary>
		/// Hàm kiểm tra dữ liệu trong khoảng cho phép
		/// Trả về giá trị 1 nếu OK, 2 nếu NG, 0 nếu chưa đủ đk đánh giá
		/// </summary>
		private int checkInRange(float tempValue, float tempMax, float tempMin)
		{
			if (tempValue > 0.00001)
			{
				if ((tempValue >= tempMin) && (tempValue <= tempMax))
                    return 1;
				else
                    return 2;
			}
			else
                return 0;
		}

		/// <summary>
		/// Kiểm tra đủ điều kiện chạy Auto
		/// </summary>
		private bool CheckConditionRun()
		{
			if ((lbdMaOrder.Value != "") && (lbdSoThuTuSanPham.Value != "") && (lbdPID.Value != "") && (lbdNguoiVanHanh.Value != "")) return true;
			return false;
		}

		/// <summary>
		/// Khởi tạo lại giá trị đo, đồ thị khi bắt đầu chu trình Auto mới
		/// </summary>
		private void ResetValueData()
		{
			// Chart Busy
			chartBusy = false;

			// Reset xuất đặc biệt
			specialOutputObject = new specialOutputObject();

			// Reset giao diện
			Dispatcher.Invoke(() =>
			{
				dtVongquay.GiatriThuan.Color = 0;
				dtVongquay.GiatriNghich.Color = 0;
				dtDongdien.GiatriThuan.Color = 0;
				dtDongdien.GiatriNghich.Color = 0;
				dtNhapluc.GiatriThuan.Color = 0;
				dtNhapluc.GiatriNghich.Color = 0;
				dtTiengon.GiatriThuan.Color = 0;
				dtTiengon.GiatriNghich.Color = 0;
				dtDorung.GiatriThuan.Color = 0;
				dtDorung.GiatriNghich.Color = 0;
			});

			// Khởi tạo giá trị ht
			htVongquay = 0;
			htDongdien = 0;
			htNhapluc = 0;
			htDorung = 0;
			htTiengon = 0;
			htDienap = 0;
			htDorung = 0;

			// Khởi tạo các giá trị hiển thị
			dtVongquay.GiatriThuan.Value = (float)-0.00001;
			dtVongquay.GiatriNghich.Value = (float)-0.00001;
			dtDongdien.GiatriThuan.Value = (float)-0.00001;
			dtDongdien.GiatriNghich.Value = (float)-0.00001;
			dtNhapluc.GiatriThuan.Value = (float)-0.00001;
			dtNhapluc.GiatriNghich.Value = (float)-0.00001;
			dtDorung.GiatriThuan.Value = (float)-0.00001;
			dtDorung.GiatriNghich.Value = (float)-0.00001;
			dtTiengon.GiatriThuan.Value = (float)-0.00001;
			dtTiengon.GiatriNghich.Value = (float)-0.00001;
			dtHuongquay.giatriThuan.Value = 0;
			dtHuongquay.giatriNghich.Value = 0;
			dtAmsac.giatriThuan.Value = 0;
			dtAmsac.giatriNghich.Value = 0;
			lbdDienap.Value = "";
			lbdTanso.Value = "";
			dtTiengon.giatriLech.Color = 0;

			for (int i = 0; i < 200; i++)
			{
				dlDorung.thuan[i] = (float)0.00001;
				dlDorung.nghich[i] = (float)0.00001;
				dlTiengon.thuan[i] = (float)0.00001;
				dlTiengon.nghich[i] = (float)0.00001;
			}

			// Khởi tạo lại đồ thị
			PlotChart(0);
		}

		/// <summary>
		/// Tắt lấy dữ liệu vòng quay PLC
		/// </summary>
		private void OffMotorCount()
		{
			plcFX5U.SetDevice("M100", 0);
		}
		/// <summary>
		/// Bật lấy dữ liệu vòng quay PLC
		/// </summary>
		private void OnMotorCount()
		{
			plcFX5U.SetDevice("M100", 1);
		}

		private void LabelF9_PreviewMouseDown(object sender, MouseButtonEventArgs e)
		{
			var temp = MessageBox.Show("RESET ALL?", "TEST", MessageBoxButton.OKCancel);
			if (temp == MessageBoxResult.OK) DataInitialize();
		}

		private void mainWD_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			continuesThread.Abort();
			PLC.Abort();
			PLC_Shiv.Abort();
			try { COMCurrent.Close(); }
			catch { }
			try { COMNoise.Close(); }
			catch { }
			// Đóng ứng dụng Excel
			try
			{
				//myExcel.ActiveWorkbook.Close(true, System.Reflection.Missing.Value, System.Reflection.Missing.Value);
				var temp = myExcel.Workbooks.Count;
				myExcel.ActiveWorkbook.Save();
				switch (temp)
				{
					case 1:
						myExcel.Workbooks[1].Close(false, System.Reflection.Missing.Value, System.Reflection.Missing.Value);
						break;
					case 2:
						myExcel.Workbooks[1].Close(false, System.Reflection.Missing.Value, System.Reflection.Missing.Value);
						myExcel.ActiveWorkbook.Close(false, System.Reflection.Missing.Value, System.Reflection.Missing.Value);
						break;
					default:
						myExcel.ActiveWorkbook.Close(false, System.Reflection.Missing.Value, System.Reflection.Missing.Value);
						myExcel.Workbooks[2].Close(false, System.Reflection.Missing.Value, System.Reflection.Missing.Value);
						myExcel.Workbooks[1].Close(false, System.Reflection.Missing.Value, System.Reflection.Missing.Value);
						myExcel.Quit();
						break;
				}
			}
			catch { }
		}

		private void lblMaOrder_PreviewMouseDown(object sender, MouseButtonEventArgs e)
		{
			SHIV_PhongCachAm.PopupWindows.inputSTTSanpham temp = new PopupWindows.inputSTTSanpham();
			temp.OrderChange += CapnhatGiatriOrderPID;
			temp.ShowDialog();
		}

		/// <summary>
		/// Tổng hợp giá trị thành từng line dạng csv để ghi ra file
		/// nvthao 13.02.2020: 
		/// Add vào database trên server kết quả kiểm tra
		/// </summary>
		private void XuatRaFileCSVvaExcel()
		{
			if (!Directory.Exists("E:\\LogSHIV")) Directory.CreateDirectory("E:\\LogSHIV");
			string path = "E:\\LogSHIV\\" + lbdSoThuTuSanPham.Value + ".csv";
			string[] lines = new string[25];
			lines[0] = $"Người kiểm tra,{lbdNguoiVanHanh.Value},STT Sản phẩm,{lbdSoThuTuSanPham.Value},Mã Order,{lbdMaOrder.Value}";
			lines[1] = $"PID,{lbdPID.Value},Mô tả sản phẩm,Giảm tốc,";
			lines[2] = $"";
			lines[3] = $"Điện áp tiêu chuẩn,Điện áp thực tế,Tần số tiêu chuẩn,Tần số thực tế";
			lines[4] = $"{lbTcDienap.Value},,{lbTcTanso.Value},,";
			lines[5] = $"";
			lines[6] = $"Thông số kiểm tra,Giá trị tiêu chuẩn,Chiều thuận,Chiều nghịch,Độ lệch,Đánh giá";
			lines[7] = $"Giá trị vòng quay,{lbTcVongquay.Value}," + dtVongquay.GetString();
			lines[8] = $"Hướng quay trục xuất lực,,,,,,";
			lines[9] = $"Giá trị dòng điẹn,{lbTcDongdien.Value}," + dtDongdien.GetString();
			lines[10] = $"Giá trị nhập lực,{lbTcNhapluc.Value}," + dtNhapluc.GetString();
			lines[11] = $"Giá trị độ rung,{lbTcDorung.Value}," + dtDorung.GetString();
			lines[12] = $"Giá trị tiếng ồn,{lbTcTiengon.Value}," + dtTiengon.GetString();
			lines[13] = $"Âm sắc,,,,,,";
			lines[14] = $",,,,,,,,";
			// Array Data Noise, Current
			lines[15] = $",,,,,,,,";
			for (int i = 0; i < 199; i++)
			{
				lines[16] += dlTiengon.thuan[i].ToString("0.00") + ",";
			}
			lines[17] = $",,,,,,,,";
			for (int i = 0; i < 199; i++)
			{
				lines[18] += dlTiengon.nghich[i].ToString("0.00") + ",";
			}
			//
			lines[19] = $",,,,,,,,";
			for (int i = 0; i < 199; i++)
			{
				lines[20] += dlDorung.thuan[i].ToString("0.00") + ",";
			}
			lines[21] = $",,,,,,,,";
			for (int i = 0; i < 199; i++)
			{
				lines[22] += dlDorung.nghich[i].ToString("0.00") + ",";
			}
			// Âm sắc thuận
			if (dtAmsac.giatriThuan.Value == 5) lines[23] = $"Đánh giá âm sắc :,OK,";
			else lines[23] = $"Đánh giá âm sắc :,NG,";
			//tempRange.Value2 = dtAmsac.giatriThuan.Value;
			if (dtAmsac.giatriNghich.Value == 5) lines[23] += "OK";
			else lines[23] += "NG";
			// Hướng quay
			if (dtHuongquay.giatriDanhgia.Value == 1) lines[24] = $"Đánh giá chiều quay :,OK,";
			else lines[24] = $"Đánh giá chiều quay :,OK,";
			// Ghi tất cả thông tin ra csv
			File.WriteAllLines(path, lines, Encoding.UTF8);

			int productID = 0;
			try
			{
				/*
                 * nvthao 13.02.2020
                 * Cất kết quả công đoạn 9 vào trong database
                 */
				int count = _dtData.Rows.Count;
				productID = TextUtils.ToInt(_dtData.Rows[0]["ProductID"]);

				for (int j = 0; j < count; j++)
				{
					Dispatcher.Invoke(() =>
					{
						ProductCheckHistoryDetailModel cModel = new ProductCheckHistoryDetailModel();
						cModel.ProductStepID = _stepID;
						cModel.ProductStepCode = "CD9";
						cModel.ProductStepName = _stepName;
						cModel.SSortOrder = TextUtils.ToInt(_dtData.Rows[j]["SSortOrder"]);

						cModel.ProductWorkingID = TextUtils.ToInt(_dtData.Rows[j]["WorkingID"]);
						cModel.ProductWorkingName = TextUtils.ToString(_dtData.Rows[j]["WorkingName"]);
						cModel.WSortOrder = TextUtils.ToInt(_dtData.Rows[j]["SortOrder"]);

						cModel.WorkerCode = lblNguoiVanhanh.Content == null ? "" : lblNguoiVanhanh.Content.ToString().Trim();
						cModel.StandardValue = TextUtils.ToString(_dtData.Rows[j]["StandardValue"]);
						cModel.ValueType = TextUtils.ToInt(_dtData.Rows[j]["ValueType"]);
						int stt = j * 10 + 10;
						string realValue = "";
						int result = 1;
						switch (stt)
						{
							case 10://Điện áp vận hành -- check mark
								realValue = lblDienapThucte.Content == null ? "" : lblDienapThucte.Content.ToString();
								break;
							case 20://Tần số dòng điện Hz -- check mark
								realValue = lblTansoThucte.Content == null ? "" : lblTansoThucte.Content.ToString();
								//if (lblOKXuatluc.Content.ToString() == "NG") result = 0;
								break;
							case 30://Dòng điện kiểm tra vận hành
								realValue = lblDongdienFwdMax.Content == null ? "" : lblDongdienFwdMax.Content.ToString();
								if (lblOKDongdien.Content.ToString().ToUpper() == "NG") result = 0;
								break;
							case 40://Dung lượng nhập lực không tải
								realValue = lblNhaplucFwdMax.Content == null ? "" : lblNhaplucFwdMax.Content.ToString();
								if (lblOKNhapluc.Content.ToString().ToUpper() == "NG") result = 0;
								break;
							case 50://Kiểm tra độ rung chiều F
								realValue = lblDorungFwdMax.Content == null ? "" : lblDorungFwdMax.Content.ToString();
								if (lblOKDorung.Content.ToString().ToUpper() == "NG") result = 0;
								break;
							case 60://Kiểm tra độ rung chiều R
								realValue = lblDorungBwdMax.Content == null ? "" : lblDorungBwdMax.Content.ToString();
								if (lblOKDorung.Content.ToString().ToUpper() == "NG") result = 0;
								break;
							case 70://Hướng quay của trục xuất lực-- check mark
								realValue = lblHuongquayMax.Content == null ? "" : lblHuongquayMax.Content.ToString();
								if (lblOKXuatluc.Content.ToString().ToUpper() == "NG") result = 0;
								break;
							case 80://Số vòng quay trục xuất lực
								realValue = lblVongquayFwdMax.Content == null ? "" : lblVongquayFwdMax.Content.ToString();
								if (lblOKVongquay.Content.ToString().ToUpper() == "NG") result = 0;
								break;
							case 90://Kiểm tra tiếng ồn không tải chiều F
								realValue = lblTiengonFwdMax.Content == null ? "" : lblTiengonFwdMax.Content.ToString();
								if (lblOKTiengon.Content.ToString().ToUpper() == "NG") result = 0;
								break;
							case 100://Kiểm tra tiếng ồn không tải chiều R
								realValue = lblTiengonBwdMax.Content == null ? "" : lblTiengonBwdMax.Content.ToString();
								if (lblOKTiengon.Content.ToString().ToUpper() == "NG") result = 0;
								break;
							case 110://Xác nhận số đóng giảm tốc hoặc mác dán giảm tốc
								realValue = lblGiamtoc.Content == null ? "" : lblGiamtoc.Content.ToString();
								//if (lblOKXuatluc.Content.ToString() == "NG") result = 0;
								break;
							case 120://Kiểm tra âm sắc-- check mark
								realValue = lblAmsacThuan.Content == null ? "" : lblAmsacThuan.Content.ToString();
								if (lblOKAmsac.Content.ToString().ToUpper() == "NG") result = 0;
								break;
							default:
								break;
						}
						cModel.RealValue = TextUtils.ToString(realValue);
						cModel.StatusResult = result;

						cModel.ValueTypeName = cModel.ValueType == 1 ? "Giá trị\n数値" : "Check mark";
						cModel.EditValue1 = "";
						cModel.EditValue2 = "";

						cModel.ProductID = productID;
						cModel.QRCode = _qrCode;// lblSTTSanpham.Content + " " + lblPID.Content;
						cModel.OrderCode = lblMaOrder.Content == null ? "" : lblMaOrder.Content.ToString();
						cModel.PackageNumber = _tienTo.Contains("-") ? _tienTo.Split('-')[1] : "";
						cModel.QtyInPackage = _stt;
						cModel.Approved = "";
						cModel.Monitor = "";
						cModel.DateLR = DateTime.Now;
						cModel.EditContent = "";
						cModel.EditDate = DateTime.Now;
						cModel.ProductCode = _productCode;

						cModel.ProductOrder = _order;

						ProductCheckHistoryDetailBO.Instance.Insert(cModel);
					});
				}
			}
			catch (Exception ex)
			{
				File.AppendAllText("E:\\ErrorLog.txt"
				   , DateTime.Now + ": Loi cat ket qua check.\n" + ex.ToString() + Environment.NewLine);
			}

			try
			{
				//Cập nhật số lượng sản phẩm thực tế vào bảng Andon
				//_andon.QtyActual += 1;
				//AndonBO.Instance.Update(_andon);
				//sendDataTCP("0", "3");

				Thread.Sleep(300);
				//Cập nhật trạng thái đã hoàn thành             
				//string sqlUpdate = string.Format("Update StatusColorCD WITH (ROWLOCK) set CD9 = 4");
				//TextUtils.ExcuteSQL(sqlUpdate);
				sendDataTCP("4", "2");
			}
			catch (Exception ex)
			{
				File.AppendAllText("E:\\ErrorLog.txt"
				, DateTime.Now + ": Loi send tcp ip sau khi cat ket qua check.\n" + ex.ToString() + Environment.NewLine);
			}

			try
			{
				Dispatcher.Invoke(() =>
				{
					//Cất vào bảng AndonDetail
					AndonDetailModel andonDetail = new AndonDetailModel();
					andonDetail.ProductCode = _productCode;
					andonDetail.ProductID = productID;
					andonDetail.ProductStepID = _stepID;
					andonDetail.QrCode = (lblSTTSanpham.Content == null ? "" : lblSTTSanpham.Content) + " " + (lblPID.Content == null ? "" : lblPID.Content);
					andonDetail.OrderCode = lblMaOrder.Content == null ? "" : lblMaOrder.Content.ToString();
					andonDetail.ProductStepCode = "CD9";
					andonDetail.PeriodTime = 0;
					andonDetail.StartTime = DateTime.Now;
					andonDetail.EndTime = DateTime.Now;
					andonDetail.MakeTime = 0;
					andonDetail.Type = 3;
					andonDetail.WorkerCode = lblNguoiVanhanh.Content == null ? "" : lblNguoiVanhanh.Content.ToString().Trim();
					AndonDetailBO.Instance.Insert(andonDetail);
				});
			}
			catch (Exception ex)
			{
				File.AppendAllText("E:\\ErrorLog.txt"
						, DateTime.Now + ": Loi cat vao bang andondetail.\n" + ex.ToString() + Environment.NewLine);
			}

			// Ghi ra file Excel
			if (myDataTemplateWorksheet != null)
			{
				countDatainTemplate += 1;
				var tempRange = (Excel.Range)myDataTemplateWorksheet.Cells[countDatainTemplate, 1];
				ExcelTemplateInput(tempRange);
			}
		}

		/// <summary>
		/// Ghi giá trị xuất đặc biệt 
		/// </summary>
		private void ExcelTemplateInputSpecial()
		{
			var tempRange = (Excel.Range)myDataTemplateWorksheet.Cells[countDatainTemplate, endRangeCollum];
			// Xuất đặc biệt
			tempRange.Value2 = specialOutputObject.Info;
			tempRange = tempRange.Offset[0, 1];
			tempRange.Value2 = specialOutputObject.UserName;
			tempRange = tempRange.Offset[0, 1];
			// Ghi thêm vào CSV
			string path = "E:\\LogSHIV\\" + lbdSoThuTuSanPham.Value + ".csv";
			string[] Temps = File.ReadAllLines(path);
			string[] tempWrites = new string[50];
			Array.Copy(Temps, tempWrites, Temps.Length);
			tempWrites[26] = "Special Output," + specialOutputObject.Info + ",By," + specialOutputObject.UserName;
			File.WriteAllLines(path, tempWrites, Encoding.UTF8);
		}

		/// <summary>
		/// Ghi dữ liệu ra file Excel tổng, lần lượt từ trái sang phải theo Template Excel cho sẵn
		/// </summary>
		/// <param name="tempRange"></param>
		private void ExcelTemplateInput(Excel.Range tempRange)
		{
			Dispatcher.Invoke(() =>
			{
				// Ngày tháng năm
				tempRange.Value2 = DateTime.Now.ToString("MM/dd/yy");
				tempRange = tempRange.Offset[0, 1];
				// STT Sản phẩm
				tempRange.Value2 = lblSTTSanpham.Content;
				tempRange = tempRange.Offset[0, 1];
				// Order
				tempRange.Value2 = lblMaOrder.Content;
				tempRange = tempRange.Offset[0, 1];
				// PID
				tempRange.Value2 = lblPID.Content;
				tempRange = tempRange.Offset[0, 1];
				// Người KT vận hành
				tempRange.Value2 = lblNguoiVanhanh.Content;
				tempRange = tempRange.Offset[0, 1];
				// Mô tả SP
				tempRange.Value2 = lblMotaSanpham.Content;
				tempRange = tempRange.Offset[0, 1];
				// Giảm tốc
				tempRange.Value2 = lblGiamtoc.Content;
				tempRange = tempRange.Offset[0, 1];
				// Điện áp tiêu chuẩn
				tempRange.Value2 = lblDienapChuan.Content;
				tempRange = tempRange.Offset[0, 1];
				// Tần số tiêu chuẩn
				tempRange.Value2 = lblTansoChuan.Content;
				tempRange = tempRange.Offset[0, 1];
				// Giá trị vòng quay chuẩn
				tempRange.Value2 = lblTcVongquay.Content;
				tempRange = tempRange.Offset[0, 1];
				// Giá trị dòng điện chuẩn
				tempRange.Value2 = lblTcDongdien.Content;
				tempRange = tempRange.Offset[0, 1];
				// Giá trị nhập lực chuẩn
				tempRange.Value2 = lblTCNhapluc.Content;
				tempRange = tempRange.Offset[0, 1];
				// Giá trị độ rung chuẩn
				tempRange.Value2 = lblTcDorung.Content;
				tempRange = tempRange.Offset[0, 1];
				// Giá trị tiếng ồn chuẩn
				tempRange.Value2 = lblTcTiengon.Content;
				tempRange = tempRange.Offset[0, 1];
				// Điện áp thực tế
				tempRange.Value2 = lblDienapThucte.Content;
				tempRange = tempRange.Offset[0, 1];
				// Tần số thực tế
				tempRange.Value2 = lblTansoThucte.Content;
				tempRange = tempRange.Offset[0, 1];
				// Vòng quay thuận
				tempRange.Value2 = lblVongquayFwdMax.Content;
				tempRange = tempRange.Offset[0, 1];
				// Vòng quay nghịch
				tempRange.Value2 = lblVongquayBwdMax.Content;
				tempRange = tempRange.Offset[0, 1];
				// Dòng điện thuận
				tempRange.Value2 = lblDongdienFwdMax.Content;
				tempRange = tempRange.Offset[0, 1];
				// Dòng điện nghịch
				tempRange.Value2 = lblDongdienBwdMax.Content;
				tempRange = tempRange.Offset[0, 1];
				// Nhập lực thuận
				tempRange.Value2 = lblNhaplucFwdMax.Content;
				tempRange = tempRange.Offset[0, 1];
				// Nhập lực nghịch
				tempRange.Value2 = lblNhaplucBwdMax.Content;
				tempRange = tempRange.Offset[0, 1];
				// Độ rung thuận
				tempRange.Value2 = lblDorungFwdMax.Content;
				tempRange = tempRange.Offset[0, 1];
				// Độ rung nghịch
				tempRange.Value2 = lblDorungBwdMax.Content;
				tempRange = tempRange.Offset[0, 1];
				// Tiếng ồn thuận
				tempRange.Value2 = lblTiengonFwdMax.Content;
				tempRange = tempRange.Offset[0, 1];
				// Tiếng ồn nghịch
				tempRange.Value2 = lblTiengonBwdMax.Content;
				tempRange = tempRange.Offset[0, 1];
				// Âm sắc thuận
				if (dtAmsac.giatriThuan.Value == 5) tempRange.Value2 = "OK";
				else tempRange.Value2 = "NG";
				//tempRange.Value2 = dtAmsac.giatriThuan.Value;
				tempRange = tempRange.Offset[0, 1];
				// Âm sắc nghịch
				if (dtAmsac.giatriNghich.Value == 5) tempRange.Value2 = "OK";
				else tempRange.Value2 = "NG";
				//tempRange.Value2 = dtAmsac.giatriNghich.Value;
				tempRange = tempRange.Offset[0, 1];
				// Hướng quay
				if (dtHuongquay.giatriDanhgia.Value == 1) tempRange.Value2 = "OK";
				else tempRange.Value2 = "NG";
				tempRange = tempRange.Offset[0, 1];
				// Vị trí điền Xuất đặc biệt
				endRangeCollum = tempRange.Column;
			});
		}

		/// <summary>
		/// Nhấn nút F1 tương đương click chuột F1
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void EventF1Push_Process(object sender, ExecutedRoutedEventArgs e)
		{
			if (!chartBusy) btnF1_PreviewMouseDown(null, null);
		}

		/// <summary> Nhấn nút S tương ứng click chuột nhập STT sản phẩm
		/// </summary>
		private void EventSPush_Process(object sender, ExecutedRoutedEventArgs e)
		{
			lblSTTSanpham_MouseDown(null, null);
		}

		/// <summary>
		/// Nhấn F8 hiển thị cửa sổ nhập STT sản phẩm - Đổ ngược dữ liệu
		/// Sau khi lấy STT, kiểm tra để load lại dữ liệu theo STT sản phẩm
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void EventF8Push_Process(object sender, ExecutedRoutedEventArgs e)
		{
			SHIV_PhongCachAm.PopupWindows.inputSTTSanpham temp = new PopupWindows.inputSTTSanpham();
			temp.STTSanphamChange += KiemtravsLoadlaidulieu;
			temp.ShowDialog();
		}

		/// <summary> Kiểm tra định dạng STT sản phẩm, load lại dữ liệu
		/// </summary>
		private void KiemtravsLoadlaidulieu(string bCode)
		{
			if ((bCode.IndexOf(" ") > 0) && (currentStageRun <= 1))
			{
				FindvsLoadData(bCode.Substring(0, bCode.IndexOf(" ")));
			}
		}

		/// <summary>
		/// Tìm file dữ liệu trong folder lưu dữ liệu, nếu có thì Load dữ liệu ra hiển thị trên giao diện
		/// </summary>
		/// <param name="bCode"></param>
		private void FindvsLoadData(string bCode)
		{
			// Tìm kiếm file trong thư mục Csv
			string folderUrl = "E:\\LogSHIV\\";
			string fileUrl = FindFileInFolder(bCode, folderUrl);
			// Kiểm tra nếu tìm được file thì load dữ liệu
			if (File.Exists(fileUrl))
			{
				LoadOldDataFromFile(fileUrl);
			}
			else
			{
				MessageBox.Show("Không tìm được file dữ liệu theo SST sản phẩm vừa nhập!");
			}
		}

		/// <summary>
		/// Chuyển đổi dữ liệu theo đường dẫn file, hiển thị lại vào phần mềm
		/// Tác dụng để kiểm tra lại dữ liệu hàng cũ
		/// </summary>
		/// <param name="fileUrl"></param>
		private void LoadOldDataFromFile(string fileUrl)
		{
			// Khởi tạo lại giá trị
			ResetValueData();

			// Đọc All line
			string[] tempLines = File.ReadAllLines(fileUrl);
			string[] temps = null;
			// Check độ dài
			if (tempLines.Length < 10)
			{
				MessageBox.Show("File thiếu dữ liệu!");
				return;
			}
			// Lấy dữ liệu 
			//lines[0] = $"Người kiểm tra,{lbdNguoiVanHanh.Value},STT Sản phẩm,{lbdSoThuTuSanPham.Value},Mã Order,{lbdMaOrder.Value}";
			temps = tempLines[0].Split(',');
			lbdNguoiVanHanh.Value = temps[1];
			lbdSoThuTuSanPham.Value = temps[3];
			lbdMaOrder.Value = temps[5];
			//lines[1] = $"PID,{lbdPID.Value},Mô tả sản phẩm,Giảm tốc,";
			temps = tempLines[1].Split(',');
			lbdPID.Value = temps[1];
			//lines[2] = $"";
			//lines[3] = $"Điện áp tiêu chuẩn,Điện áp thực tế,Tần số tiêu chuẩn,Tần số thực tế";
			//lines[4] = $"{lbTcDienap.Value},,{lbTcTanso.Value},,";
			temps = tempLines[4].Split(',');
			//lines[5] = $"";
			//lines[6] = $"Thông số kiểm tra,Giá trị tiêu chuẩn,Chiều thuận,Chiều nghịch,Độ lệch,Đánh giá";
			/////
			//string temp = "";
			//temp += GiatriThuan.Value + "," + GiatriNghich.Value + "," + _doLech + ",";
			//if (GiatriDanhgia.Value == 1) temp += "OK,";
			//else temp += "NG,";
			//return temp;
			// Tính toán lại giá trị tiêu chuẩn
			lbTcDienap.Value = temps[0];
			lbTcTanso.Value = temps[2];
			lbTcVongquay.Value = tempLines[7].Split(',')[1];
			lbTcDongdien.Value = tempLines[9].Split(',')[1];
			lbTcNhapluc.Value = tempLines[10].Split(',')[1];
			lbTcDorung.Value = tempLines[11].Split(',')[1];
			lbTcTiengon.Value = tempLines[12].Split(',')[1];
			calculateMaxMinValue();
			//lines[7] = $"Giá trị vòng quay,{lbTcVongquay.Value}," + dtVongquay.GetString();
			temps = tempLines[7].Split(',');
			dtVongquay.GiatriThuan.Value = float.Parse(temps[2]);
			dtVongquay.GiatriNghich.Value = float.Parse(temps[3]);
			//lines[8] = $"Hướng quay trục xuất lực,,,,,,";
			//lines[9] = $"Giá trị dòng điẹn,{lbTcDongdien.Value}," + dtDongdien.GetString();
			temps = tempLines[9].Split(',');
			dtDongdien.GiatriThuan.Value = float.Parse(temps[2]);
			dtDongdien.GiatriNghich.Value = float.Parse(temps[3]);
			//lines[10] = $"Giá trị nhập lực,{lbTcNhapluc.Value}," + dtNhapluc.GetString();
			temps = tempLines[10].Split(',');
			dtNhapluc.GiatriThuan.Value = float.Parse(temps[2]);
			dtNhapluc.GiatriNghich.Value = float.Parse(temps[3]);
			//lines[11] = $"Giá trị độ rung,{lbTcDorung.Value}," + dtDorung.GetString();
			temps = tempLines[11].Split(',');
			dtDorung.GiatriThuan.Value = float.Parse(temps[2]);
			dtDorung.GiatriNghich.Value = float.Parse(temps[3]);
			//lines[12] = $"Giá trị tiếng ồn,{lbTcTiengon.Value}," + dtTiengon.GetString();
			temps = tempLines[12].Split(',');
			dtTiengon.GiatriThuan.Value = float.Parse(temps[2]);
			dtTiengon.GiatriNghich.Value = float.Parse(temps[3]);
			//lines[13] = $"Âm sắc,,,,,,";
			//lines[14] = $",,,,,,,,";
			// Array Data Noise, Current
			//lines[15] = $",,,,,,,,";
			temps = tempLines[16].Split(',');
			for (int i = 0; i < 199; i++)
			{
				dlTiengon.thuan[i] = (float.Parse(temps[i]));
			}
			//lines[17] = $",,,,,,,,";
			temps = tempLines[18].Split(',');
			for (int i = 0; i < 199; i++)
			{
				dlTiengon.nghich[i] = (float.Parse(temps[i]));
			}
			//
			//lines[19] = $",,,,,,,,";
			temps = tempLines[20].Split(',');
			for (int i = 0; i < 199; i++)
			{
				dlDorung.thuan[i] = (float.Parse(temps[i]));
			}
			//lines[21] = $",,,,,,,,";
			temps = tempLines[22].Split(',');
			for (int i = 0; i < 199; i++)
			{
				dlDorung.nghich[i] = (float.Parse(temps[i]));
			}
			//// Line 23 Lấy giá trị âm sắc
			//temps = tempLines[23].Split(',');
			//if (temps[1] == "OK") dtAmsac.giatriThuan.Value = 5;
			//else dtAmsac.giatriThuan.Value = 15;
			//if (temps[2] == "OK") dtAmsac.giatriNghich.Value = 5;
			//else dtAmsac.giatriNghich.Value = 15;
			//// Line 24 Lấy giá trị hướng quay
			//temps = tempLines[24].Split(',');
			//if (temps[1] == "OK") dtHuongquay.giatriDanhgia.Value = 1;
			//else dtHuongquay.giatriDanhgia.Value = 2;
			// Load lại đồ thị
			PlotChart(0);
		}

		/// <summary>
		/// Tìm File có tên chứa STT sản phẩm - nếu có thì trả về đường dẫn file, nếu không trả về ""
		/// </summary>
		/// <param name="bCode"></param>
		/// <param name="folderUrl"></param>
		/// <returns></returns>
		private string FindFileInFolder(string bCode, string folderUrl)
		{
			string[] files = Directory.GetFiles(folderUrl, bCode + ".csv", SearchOption.AllDirectories);
			if ((files == null) || (files.Count() < 1)) return "";
			else return files[0];
		}

		/// <summary>
		/// Nhấn phím F10 - Xuất đặc biệt, hiển thị Form xuất đặc biệt
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void EventF10Push_Process(object sender, ExecutedRoutedEventArgs e)
		{
			checkSpecialOutput tempSO = new checkSpecialOutput();
			tempSO.EventConfirmButton += ProcessSpecialOutput;
			tempSO.ShowDialog();
		}

		/// <summary>
		/// Xử lý lựa chọn xuất đặc biệt - (đầu ra từ form xuất đặc biệt)
		/// </summary>
		/// <param name="OutputInfo"></param>
		/// <param name="LeaderConfirm"></param>
		private void ProcessSpecialOutput(string OutputInfo, string LeaderConfirm)
		{
			specialOutputObject.UserName = LeaderConfirm;
			specialOutputObject.Info = OutputInfo;
			ExcelTemplateInputSpecial();
		}

		/// <summary>
		/// Click chuột F10 - tương đương nhấn phím
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void EventF10Push_Process_Mouse(object sender, MouseButtonEventArgs e)
		{
			EventF10Push_Process(null, null);
		}

		private void EventF8Push_Process(object sender, MouseButtonEventArgs e)
		{
			SHIV_PhongCachAm.PopupWindows.inputSTTSanpham temp = new PopupWindows.inputSTTSanpham();
			temp.STTSanphamChange += KiemtravsLoadlaidulieu;
			temp.ShowDialog();
		}

		/// <summary>
		/// Nhấn phím O tương đương click nhập Order
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void EventOPush_Process(object sender, ExecutedRoutedEventArgs e)
		{
			lblMaOrder_PreviewMouseDown(null, null);
		}

		private void btnIsUse_Click(object sender, RoutedEventArgs e)
		{
			//if (btnIsUse.Content.ToString() == "Khong Su Dung")
			//{
			//	this.sendDataTCP("10", "10");
			//	btnIsUse.Content = "Su Dung";
			//}
			//else
			//{
			//	this.sendDataTCP("11", "10");
			//	btnIsUse.Content = "Khong Su Dung";
			//}
		}

		private void lblIsUse_PreviewMouseDown(object sender, MouseButtonEventArgs e)
		{
			if (lblIsUse.Content.ToString() == "Khong Su Dung")
			{
				this.sendDataTCP("10", "10");
				lblIsUse.Content = "Su Dung";
			}
			else
			{
				this.sendDataTCP("11", "10");
				lblIsUse.Content = "Khong Su Dung";
			}
		}

		private void lblSTTSanpham_MouseDown(object sender, MouseButtonEventArgs e)
		{
			SHIV_PhongCachAm.PopupWindows.inputSTTSanpham temp = new PopupWindows.inputSTTSanpham();
			temp.STTSanphamChange += CapnhatGiatriSTTSpham;
			temp.ShowDialog();
		}

		private void lblNguoiVanhanh_PreviewMouseDown(object sender, MouseButtonEventArgs e)
		{
			SHIV_PhongCachAm.PopupWindows.inputSTTSanpham temp = new PopupWindows.inputSTTSanpham();
			temp.OperatorChange += CapnhatNguoivanhanh;
			temp.ShowDialog();
		}

		private void CapnhatNguoivanhanh(string bCode)
		{
			if (bCode.Length > 5)
			{
				lbdNguoiVanHanh.Value = bCode;
			}
		}

		string _qrCode = "";
		/// <summary>
		/// nvthao 25.02.2020
		/// lấy ra chuỗi QRcode
		/// On start Andon
		/// </summary>
		/// <param name="bCode"></param>
		private void CapnhatGiatriSTTSpham(string bCode)
		{
			if (bCode.IndexOf(" ") > 0)
			{
				lbdSoThuTuSanPham.Value = bCode.Substring(0, bCode.IndexOf(" "));
				_qrCode = bCode;
				try
				{
					//if (lblPID.Content == null) return;
					_productCode = _qrCode.Split(' ')[1];

					string sql = string.Format(@" SELECT top 1 WS.ID ,
                                    WS.ProductStepCode,WS.Description
                            FROM    dbo.ProductStep WS
                                    INNER JOIN dbo.Product P ON P.ID = WS.ProductID
                            WHERE P.ProductCode = '{0}' and WS.ProductStepCode = 'CD9'", _productCode);

					DataTable dtStep = TextUtils.Select(sql);
					if (dtStep.Rows.Count == 0) return;
					_stepCode = TextUtils.ToString(dtStep.Rows[0]["ProductStepCode"]);
					_stepID = TextUtils.ToInt(dtStep.Rows[0]["ID"]);
					_stepName = TextUtils.ToString(dtStep.Rows[0]["Description"]);

					DataSet ds = ProductCheckHistoryDetailBO.Instance.GetDataSet("spGetWorkingByProduct_ForCD9",
						new string[] { "@WorkingStepID", "@WorkingStepCode", "@ProductCode" },
						new object[] { _stepID, _stepCode, _productCode });

					_dtData = ds.Tables[0];

					/*
					 * Tách chuỗi QrCode
					 */
					string orderCode = _qrCode;
					string[] arr1 = orderCode.Split(' ');
					if (arr1.Length > 0)
					{
						_order = arr1[0];
						//_productCode = arr1[1].Trim();
						string[] arr;
						if (_order.Contains("-"))
						{
							arr = _order.Split('-');
							_tienTo = (arr[0] + "-" + arr[1] + "-");
							_stt = arr[2];
						}
						else
						{
							arr = Regex.Split(_order, @"\D+");
							_stt = arr[arr.Length - 1];
							_tienTo = _order.Substring(0, _order.IndexOf(_stt));
						}
					}

					//Cập nhật vào bảng StatusCD
					//string sqlUpdate = string.Format("Update StatusCD WITH (ROWLOCK) set CD9 = 1");
					//TextUtils.ExcuteSQL(sqlUpdate);
				}
				catch (Exception ex)
				{
					File.AppendAllText(string.Format("E:\\ErrorLog{0}.txt",DateTime.Now.ToString("dd_MM_yyyy"))
						 , DateTime.Now + ": Loi tach barcode.\n" + ex.ToString() + Environment.NewLine);
				}
				

				try
				{
					//On start Andon

					//string dateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm");
					//BMS.Utils.Expression exp = new BMS.Utils.Expression("ShiftStartTime", dateTime, "<=");
					//BMS.Utils.Expression exp2 = new BMS.Utils.Expression("ShiftEndTime", dateTime, ">=");
					//ArrayList arr = AndonBO.Instance.FindByExpression(exp.And(exp2));
					//if (arr.Count <= 0) return;
					//_andon = arr[0] as AndonModel;
					//_andon.IsStart = true;
					//AndonBO.Instance.Update(_andon);
					//TextUtils.ExcuteSQL("exec spUpdateAndon");

					sendDataTCP("0", "4");
				}
				catch (Exception ex)
				{
					File.AppendAllText(string.Format("E:\\ErrorLog{0}.txt", DateTime.Now.ToString("dd_MM_yyyy"))
						, DateTime.Now + ": Loi send tcp ip sau khi nhap barcode.\n" + ex.ToString() + Environment.NewLine);
				}
			}
		}

		private void CapnhatGiatriOrderPID(string txtMaOder)
		{
			//if (txtSttSP.IndexOf("-") == (txtSttSP.Length - 4))
			//{
			//    // Tách mã Order từ số thứ tự sản phẩm
			//    lbdSoThuTuSanPham.Value = txtSttSP;
			//    lbdMaOrder.Value = txtSttSP.Substring(0, txtSttSP.IndexOf("-"));
			//    LayPIDVaGiatriTieuchuan(lbdMaOrder.Value);
			//}
			if (txtMaOder.Length > 5)
			{
				lbdMaOrder.Value = txtMaOder.Replace("\n", "");
				LayPIDVaGiatriTieuchuan(lbdMaOrder.Value.Substring(0, lbdMaOrder.Value.Length - 1));
				//lbdNguoiVanHanh.Value = "";	 
				is1Parse.Value = false;
				// Bổ xung Reset trạng thái 1pha/3pha
			}
		}

		private void LayPIDVaGiatriTieuchuan(string value)
		{
			if (myExcel.Workbooks.Count > 0)
			{
				// Tìm Order trong Range từ 1 => 50
				Excel.Range tempRange = (Excel.Range)workSheetKehoach.Range[workSheetKehoach.Cells[1, 3], workSheetKehoach.Cells[100000, 10]];
				Excel.Range tempSearch = tempRange.Find(value);
				if (tempSearch == null) tempSearch = (Excel.Range)workSheetKehoach.Cells[1, 1];
				// Nếu hàng và cột >1 có nghĩa là tìm thấy Order 
				if (tempSearch.Row > 1)
				{
					// Lấy giá trị PID từ cột tương ứng với hàng Order tìm được (+2)
					tempSearch = (Excel.Range)workSheetKehoach.Cells[tempSearch.Row, tempSearch.Column + 2];
					lbdPID.Value = Convert.ToString(tempSearch.Value2);
					// Thêm giá trị tiêu chuẩn theo giá trị PID
					LayCacGiatritieuchuanTheoPID(lbdPID);
				}
				else
				{
					MessageBox.Show("Can't Find Order!");
					//DataInitialize();
				}
			}
		}

		private void LayCacGiatritieuchuanTheoPID(labelObject lbdPID)
		{
			Excel.Range tempRange = workSheetDatabase.Range[workSheetDatabase.Cells[1, 1], workSheetDatabase.Cells[50000, 10]];
			Excel.Range tempSearch = tempRange.Find(lbdPID.Value);
			if (tempSearch == null)
                tempSearch = (Excel.Range)workSheetDatabase.Cells[1, 1];
			if ((tempSearch.Row > 1))
			{
				try
				{
					int dataBase_Row = tempSearch.Row;
					// Cập nhật giá trị tiêu chuẩn
					lbdMoTaSanPham.Value = System.Convert.ToString(((Excel.Range)workSheetDatabase.Cells[dataBase_Row, 4]).Value2);
					lbdGiamToc.Value = System.Convert.ToString(((Excel.Range)workSheetDatabase.Cells[dataBase_Row, 5]).Value2);
					lbTcDongdien.Value = System.Convert.ToString(((Excel.Range)workSheetDatabase.Cells[dataBase_Row, 6]).Value2);
					lbTcNhapluc.Value = System.Convert.ToString(((Excel.Range)workSheetDatabase.Cells[dataBase_Row, 7]).Value2);
					lbTcDienap.Value = System.Convert.ToString(((Excel.Range)workSheetDatabase.Cells[dataBase_Row, 8]).Value2);
					lbTcTanso.Value = System.Convert.ToString(((Excel.Range)workSheetDatabase.Cells[dataBase_Row, 9]).Value2) + "Hz";
					lbTcDorung.Value = System.Convert.ToString(((Excel.Range)workSheetDatabase.Cells[dataBase_Row, 10]).Value2);
					lbTcTiengon.Value = System.Convert.ToString(((Excel.Range)workSheetDatabase.Cells[dataBase_Row, 11]).Value2);
					lbTcVongquay.Value = System.Convert.ToString(((Excel.Range)workSheetDatabase.Cells[dataBase_Row, 12]).Value2);

					// Lấy Max/Min từ giá trị tiêu chuẩn
					calculateMaxMinValue();

					// Kết thúc chu trình, chạy lại
					currentStageRun = 0;
				}
				catch
				{
					MessageBox.Show("Database Error! Line " + tempSearch.Row.ToString());
				}
			}
			else
			{
				MessageBox.Show("Can't Find Database");
				lbdMaOrder.Value = "";
				//DataInitialize();
			}
		}

		private void calculateMaxMinValue()
		{
			string tempS = "";

			try
			{
				tempS = lbTcDongdien.Value;
				lbTcDongdien.Value = "";
				dtDongdien.Min = float.Parse(tempS.Substring(0, tempS.IndexOf('~')));
				dtDongdien.Max = float.Parse(tempS.Substring(tempS.IndexOf('~') + 1));
				lbTcDongdien.Value = tempS;
			}
			catch
			{
				MessageBox.Show("Database Error!!!" + tempS);
			}

			try
			{
				tempS = lbTcNhapluc.Value;
				lbTcNhapluc.Value = "";
				dtNhapluc.Min = 0;
				dtNhapluc.Max = float.Parse(tempS.Substring(tempS.IndexOf(" ") + 1)); // Xử lý chuỗi "<= 5.555"
				lbTcNhapluc.Value = tempS;
			}
			catch
			{
				MessageBox.Show("Database Error!!!" + tempS);
			}

			try
			{
				tempS = lbTcDienap.Value;
				lbTcDienap.Value = "";
				dtDienap.Max = dtDienap.Min = float.Parse(tempS.Substring(0, tempS.IndexOf("V"))); // Xử lý chuỗi "220V"
				lbTcDienap.Value = tempS;
			}
			catch
			{
				MessageBox.Show("Database Error!!!" + tempS);
			}

			try
			{
				tempS = lbTcDorung.Value;
				lbTcDorung.Value = "";
				dtDorung.Min = 0;
				dtDorung.Max = float.Parse(tempS.Substring(tempS.IndexOf(" ") + 1));
				lbTcDorung.Value = tempS;
			}
			catch
			{
				MessageBox.Show("Database Error!!!" + tempS);
			}

			try
			{
				tempS = lbTcTiengon.Value;
				lbTcTiengon.Value = "";
				dtTiengon.Min = 0;
				dtTiengon.Max = float.Parse(tempS.Substring(tempS.IndexOf(" ") + 1)); // Xử lý chuỗi "<= 61"
				lbTcTiengon.Value = tempS;
			}
			catch
			{
				MessageBox.Show("Database Error!!!" + tempS);
			}

			try
			{
				tempS = lbTcVongquay.Value;
				lbTcVongquay.Value = "";
				dtVongquay.Min = float.Parse(tempS.Substring(0, tempS.IndexOf('~')));
				dtVongquay.Max = float.Parse(tempS.Substring(tempS.IndexOf('~') + 1));
				lbTcVongquay.Value = tempS;
			}
			catch
			{
				MessageBox.Show("Database Error!!!" + tempS);
			}
		}

		private void killAppExcel()
		{
			foreach (var process in Process.GetProcessesByName("EXCEL"))
			{
				process.Kill();
			}
		}

		private void btnF3_MouseDown(object sender, MouseButtonEventArgs e)
		{
			excelLink = "";
			PopupWindows.settingExcel temp = new PopupWindows.settingExcel();
			temp.ExcelLinkChange += UpdateExcelDatabaseWhenChangeLink;
			temp.ShowDialog();
		}

		private void UpdateExcelDatabaseWhenChangeLink(string path)
		{
			DatabasePath = path;
			if (DatabasePath.Length > 10)
			{
				if (myExcel.Workbooks.Count > 0) myExcel.ActiveWorkbook.Close(false, System.Reflection.Missing.Value, System.Reflection.Missing.Value);
				UpdateDataBasePathtoExcelApplication();
			}
		}

		/// <summary>
		/// Mở file Excel kế hoạch và Database, hiển thị trên ứng dụng Excel đã mở
		/// </summary>
		private void UpdateDataBasePathtoExcelApplication()
		{
			string KhLoalStri = DatabasePath.Substring(0, DatabasePath.LastIndexOf("\\") + 1) + "KHLocal.xlsx";
			try
			{
				myExcel.Workbooks.Open(@KhLoalStri);
				workSheetKehoach = myExcel.ActiveWorkbook.Worksheets["KE HOACH"];
			}
			catch (Exception e)
			{
				MessageBox.Show("Fail to Update Database file!" + e.ToString());
			}

			try
			{
				myExcel.Workbooks.Open(@DatabasePath);
				workSheetDatabase = myExcel.ActiveWorkbook.Worksheets["DATA BASE"];
				//workSheetKehoach = myExcel.ActiveWorkbook.Worksheets["KE HOACH"];
				workSheetDatabase.Activate();
				myExcel.DisplayFullScreen = true;
				myExcel.Visible = true;
			}
			catch (Exception e)
			{
				MessageBox.Show("Fail to Update Database file!" + e.ToString());
			}
		}

		/// <summary>
		/// Cập nhật và phân tích dữ liệu từng chiều quay
		/// </summary>
		/// <param name="Option"></param>
		/// <param name="checkDonetemp"></param>
		private void UpdateDulieu(string Option, ref checkDone checkDonetemp)
		{
			switch (Option)
			{
				case "Forward":

					//if (!checkDonetemp.vongQuay)
					//{
					//    dtVongquay.giatriThuan.Value = htVongquay;
					//    //dtVongquay.giatriThuan.Value = (float)(DateTime.Now.Second + 1.11);
					checkDonetemp.vongQuay = true;
					//}

					if (!checkDonetemp.dongDien)
					{
						dtDongdien.GiatriThuan.Value = htDongdien;
						//dtDongdien.giatriThuan.Value = (float)(DateTime.Now.Second + 1.11);
						checkDonetemp.dongDien = true;
						lbdDienap.Value = htDienap.ToString("0.00V");
						lbdTanso.Value = htTanso.ToString("0.00Hz");
					}

					if (!checkDonetemp.nhapLuc)
					{
						dtNhapluc.GiatriThuan.Value = htNhapluc;
						//dtNhapluc.giatriThuan.Value = (float)(DateTime.Now.Second + 1.11);
						checkDonetemp.nhapLuc = true;
					}

					if (!checkDonetemp.doRung)
					{
						System.Console.WriteLine("Gia tri count do rung : " + checkDonetemp.countDorung.ToString());
						dlDorung.thuan[checkDonetemp.countDorung] = htDorung;
						dtDorung.GiatriThuan.Value = dlDorung.thuan.Max();
						checkDonetemp.countDorung += 1;
						if (checkDonetemp.countDorung > 198) checkDonetemp.doRung = true;
					}

					if (!checkDonetemp.tiengOn)
					{
						dlTiengon.thuan[checkDonetemp.countTiengon] = htTiengon;
						dtTiengon.GiatriThuan.Value = dlTiengon.thuan.Max();
						checkDonetemp.countTiengon += 1;
						if (checkDonetemp.countTiengon > 198) checkDonetemp.tiengOn = true;
					}

					if (((checkDonetemp.countDorung + 1) % 20 == 0) || (checkDonetemp.countDorung == 1)) PlotChart(currentChart);

					break;
				case "Backward":
					//if (!checkDonetemp.vongQuay)
					//{
					//    dtVongquay.giatriNghich.Value = htVongquay;
					//    //dtVongquay.giatriNghich.Value = (float)(DateTime.Now.Second + 1.11);
					checkDonetemp.vongQuay = true;
					//}

					if (!checkDonetemp.dongDien)
					{
						dtDongdien.GiatriNghich.Value = htDongdien;
						//dtDongdien.giatriNghich.Value = (float)(DateTime.Now.Second + 1.11);
						checkDonetemp.dongDien = true;
					}

					if (!checkDonetemp.nhapLuc)
					{
						dtNhapluc.GiatriNghich.Value = htNhapluc;
						//dtNhapluc.giatriNghich.Value = (float)(DateTime.Now.Second + 1.11);
						checkDonetemp.nhapLuc = true;
					}

					if (!checkDonetemp.doRung)
					{
						System.Console.WriteLine("Gia tri count do rung : " + checkDonetemp.countDorung.ToString());
						dlDorung.nghich[checkDonetemp.countDorung] = htDorung;
						dtDorung.GiatriNghich.Value = dlDorung.nghich.Max();
						checkDonetemp.countDorung += 1;
						if (checkDonetemp.countDorung > 198) checkDonetemp.doRung = true;
					}

					if (!checkDonetemp.tiengOn)
					{
						dlTiengon.nghich[checkDonetemp.countTiengon] = htTiengon;
						dtTiengon.GiatriNghich.Value = dlTiengon.nghich.Max();
						checkDonetemp.countTiengon += 1;
						if (checkDonetemp.countTiengon > 198) checkDonetemp.tiengOn = true;
					}

					if (((checkDonetemp.countDorung + 1) % 20 == 0) | (checkDonetemp.countDorung == 1)) PlotChart(currentChart);

					break;
				default:
					break;
			}
		}

		/// <summary>
		/// Vẽ đồ thị dữ liệu
		/// </summary>
		private void PlotChart(int options)
		{
			if (!chartBusy)
			{
				chartBusy = true;
				//LineChartChild.PolylineStyle = GetDashedLineStyle();
				LineChart1.Dispatcher.Invoke(new Action(() =>
				{
					if (valueSettingMaxRange.Value > 0)
						valueSettingYRange.DataContext = valueSettingMaxRange;
				}));
				switch (options)
				{
					case 0:
						if (true)
						{
							MyValue = new ObservableCollection<ChartViewItem>();
							MyValue_TempTiengon = new ObservableCollection<ChartViewItem>();
							MyMax = new ObservableCollection<ChartViewItem>();
						}
						else
						{
							LineChart1.Dispatcher.Invoke(new Action(() =>
							{
								MyValue.Clear();
								MyValue_TempTiengon.Clear();
								MyMax.Clear();
							}));
						}

						for (int i = 0; i < 200; i++)
						{
							MyValue.Add(new ChartViewItem { Key = i / (float)10.0, Value = dlDorung.thuan[i] });
						}
						MyMax.Add(new ChartViewItem { Key = 0 / (float)10.0, Value = dtDorung.Max });
						MyMax.Add(new ChartViewItem { Key = 199 / (float)10.0, Value = dtDorung.Max });

						valueSettingMaxRange.Value = (float)((dtDorung.Max) * 1.5);
						LineChart1.Dispatcher.Invoke(new Action(() =>
						{
							//LineChartChild.DataPointStyle = styleDorung;
							LineChartChild.Title = "Độ rung thuận";
							LineChartChild.DataContext = MyValue;

							LineChartChild_TempTiengon.Title = "Tiếng ồn thuận";
							LineChartChild_TempTiengon.DataContext = MyValue_TempTiengon;

							LineChartMax.DataContext = MyMax;
						}));
						break;
					case 1:
						if (true)
						{
							MyValue = new ObservableCollection<ChartViewItem>();
							MyValue_TempTiengon = new ObservableCollection<ChartViewItem>();
							MyMax = new ObservableCollection<ChartViewItem>();
						}
						else
						{
							LineChart1.Dispatcher.Invoke(new Action(() =>
							{
								MyValue.Clear();
								MyValue_TempTiengon.Clear();
								MyMax.Clear();
							}));
						}

						for (int i = 0; i < 200; i++)
						{
							MyValue.Add(new ChartViewItem { Key = i / (float)10.0, Value = dlDorung.nghich[i] });
						}
						MyMax.Add(new ChartViewItem { Key = 0 / (float)10.0, Value = dtDorung.Max });
						MyMax.Add(new ChartViewItem { Key = 199 / (float)10.0, Value = dtDorung.Max });

						valueSettingMaxRange.Value = (float)((dtDorung.Max) * 1.5);
						LineChart1.Dispatcher.Invoke(new Action(() =>
						{
							//LineChartChild.DataPointStyle = styleDorung;
							LineChartChild.Title = "Độ rung nghịch";
							LineChartChild.DataContext = MyValue;

							LineChartChild_TempTiengon.Title = "Tiếng ồn nghịch";
							LineChartChild_TempTiengon.DataContext = MyValue_TempTiengon;

							LineChartMax.DataContext = MyMax;
						}));
						break;
					case 2:
						if (true)
						{
							MyValue = new ObservableCollection<ChartViewItem>();
							MyValue_TempTiengon = new ObservableCollection<ChartViewItem>();
							MyMax = new ObservableCollection<ChartViewItem>();
						}
						else
						{
							LineChart1.Dispatcher.Invoke(new Action(() =>
							{
								MyValue.Clear();
								MyValue_TempTiengon.Clear();
								MyMax.Clear();
							}));
						}
						for (int i = 0; i < 200; i++)
						{
							MyValue_TempTiengon.Add(new ChartViewItem { Key = i / (float)10.0, Value = dlTiengon.thuan[i] });
						}
						MyMax.Add(new ChartViewItem { Key = 0 / (float)10.0, Value = dtTiengon.Max });
						MyMax.Add(new ChartViewItem { Key = 199 / (float)10.0, Value = dtTiengon.Max });

						valueSettingMaxRange.Value = (float)((dtTiengon.Max) * 1.5);

						LineChart1.Dispatcher.Invoke(new Action(() =>
						{
							//LineChartChild.DataPointStyle = styleTiengon;
							LineChartChild.Title = "Độ rung thuận";
							LineChartChild.DataContext = MyValue;

							LineChartChild_TempTiengon.Title = "Tiếng ồn thuận";
							LineChartChild_TempTiengon.DataContext = MyValue_TempTiengon;

							LineChartMax.DataContext = MyMax;
						}));
						break;
					case 3:
						if (true)
						{
							MyValue = new ObservableCollection<ChartViewItem>();
							MyValue_TempTiengon = new ObservableCollection<ChartViewItem>();
							MyMax = new ObservableCollection<ChartViewItem>();
						}
						else
						{
							LineChart1.Dispatcher.Invoke(new Action(() =>
							{
								MyValue.Clear();
								MyValue_TempTiengon.Clear();
								MyMax.Clear();
							}));
						}
						for (int i = 0; i < 200; i++)
						{
							MyValue_TempTiengon.Add(new ChartViewItem { Key = i / (float)10.0, Value = dlTiengon.nghich[i] });
						}
						MyMax.Add(new ChartViewItem { Key = 0 / (float)10.0, Value = dtTiengon.Max });
						MyMax.Add(new ChartViewItem { Key = 199 / (float)10.0, Value = dtTiengon.Max });
						valueSettingMaxRange.Value = (float)((dtTiengon.Max) * 1.5);
						LineChart1.Dispatcher.Invoke(new Action(() =>
						{

							//LineChartChild.DataPointStyle = styleTiengon;
							LineChartChild.Title = "Độ rung nghịch";
							LineChartChild.DataContext = MyValue;

							LineChartChild_TempTiengon.Title = "Tiếng ồn nghịch";
							LineChartChild_TempTiengon.DataContext = MyValue_TempTiengon;

							LineChartMax.DataContext = MyMax;
						}));
						break;
					default:
						break;
				}
				chartBusy = false;
			}

		}

		private Style GetDashedLineStyle()
		{
			var style = new Style(typeof(Polyline));
			style.Setters.Add(new Setter(Shape.StrokeDashArrayProperty,
							  new DoubleCollection(new[] { 5.0 })));
			return style;
		}

		/// <summary>
		/// Kiểm tra âm sắc theo từng chiều lựa chọn
		/// </summary>
		/// <param name="v"></param>
		private void CheckAmsac(string v)
		{
			switch (v)
			{
				case "Forward":
					Application.Current.Dispatcher.Invoke(
						(Action)delegate
						{
							PopupWindows.checkAmsacWD temp = new PopupWindows.checkAmsacWD("thuận");
							temp.isCheckedEvent += checkAmsacThuan;
							temp.ShowDialog();
						});
					break;
				case "Backward":
					Application.Current.Dispatcher.Invoke(
						(Action)delegate
						{
							PopupWindows.checkAmsacWD temp = new PopupWindows.checkAmsacWD("nghịch");
							temp.isCheckedEvent += checkAmsacNghich;
							temp.ShowDialog();
						});
					break;
				default:
					break;
			}
		}

		/// <summary>
		/// Kiểm tra phán định âm sắc và update hiển thị
		/// </summary>
		/// <param name="value"></param>
		private void checkAmsacThuan(string value)
		{
			if (value == "OK") dtAmsac.giatriThuan.Value = 5;
			else dtAmsac.giatriThuan.Value = 15;
		}
		/// <summary>
		/// Kiểm tra phán định âm sắc và update hiển thị
		/// </summary>
		/// <param name="value"></param>
		private void checkAmsacNghich(string value)
		{
			if (value == "OK") dtAmsac.giatriNghich.Value = 5;
			else dtAmsac.giatriNghich.Value = 15;
		}

		/// <summary>
		/// Kiểm tra hướng quay trục xuất lực tổng
		/// </summary>
		private void checkRotaryDirection()
		{
			Application.Current.Dispatcher.Invoke(
						(Action)delegate
						{
							PopupWindows.checkRotary temp = new PopupWindows.checkRotary("");
							temp.isCheckedEvent += checkHuongquayTong;
							temp.ShowDialog();
						});
		}

		/// <summary>
		/// Kiểm tra phán định hướng quay, sau đó Update hiển thị phán định
		/// </summary>
		/// <param name="value"></param>
		private void checkHuongquayTong(string value)
		{
			if (value == "OK")
			{
				dtHuongquay.giatriThuan.Value = dtHuongquay.giatriNghich.Value = 5;
			}
			else
			{
				dtHuongquay.giatriThuan.Value = dtHuongquay.giatriNghich.Value = 15;
			}


		}

		private void ResetAllData()
		{
			throw new NotImplementedException();
		}

		private async Task Wait5Second()
		{
			await Task.Delay(5000);
		}

		private async Task Wait3Second()
		{
			await Task.Delay(3000);
		}


	}
}
