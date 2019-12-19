using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows;
using winForm = System.Windows.Forms;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;
using static wifiuzaktankontrolpro.Win32;
using Microsoft.Win32;
using AudioSwitcher.AudioApi.CoreAudio;
using NAudio.Wave;
using NAudio.CoreAudioApi;

namespace wifiuzaktankontrolpro
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

        }


        //SES kontrol için
        CoreAudioDevice defaultPlaybackDevice;       

        //https://docs.microsoft.com/en-us/windows/win32/inputdev/wm-appcommand
        //https://www.rapidtables.com/convert/number/hex-dec-bin-converter.html //hex çevirmelerini yapmak için.
        private const int APPCOMMAND_MEDIA_STOP = 0xD0000;
        private const int APPCOMMAND_MEDIA_PLAY = 0xE0000;
        private const int APPCOMMAND_MEDIA_PREVIOUSTRACK = 0xC0000;
        private const int APPCOMMAND_MEDIA_NEXTTRACK = 0xB0000;
        private const int WM_APPCOMMAND = 0x319;

        IntPtr windowHandle;

        [DllImport("user32.dll")]
        public static extern IntPtr SendMessageW(IntPtr hWnd, int Msg,
            IntPtr wParam, IntPtr lParam);

        RegistryKey regApp = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);        

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            int processId = Process.GetCurrentProcess().Id;
            string processName = Process.GetCurrentProcess().ProcessName;            
            Process[] processlist = Process.GetProcesses();
            foreach (Process theprocess in processlist)
            {                
                if(processId != theprocess.Id && theprocess.ProcessName == processName) // id si farklı olan bu programın aynı isimdeki işlem olan işlemi sonlandırıp bu program üzerinden çalıştırma.
                {
                    MessageBoxResult result = MessageBox.Show("Program zaten çalışmakta sonlandırıp, yeniden başlatmak istediğinize emin misiniz?", "Yeniden Başlat", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (result == MessageBoxResult.Yes)
                    {
                        theprocess.Kill();
                        Thread.Sleep(3000); //Diğer program kapatılırken socket bağlantısı için biraz bekletiyoruz.                        
                    }
                    else
                    {
                        Process.GetCurrentProcess().Kill();
                    }
                }
            }

            AppHide();            
            //Form Minimize Event ! Bug olmasın diye.
            HwndSource source = (HwndSource)PresentationSource.FromVisual(this);
            source.AddHook(new HwndSourceHook(HandleMessages));

            windowHandle = new WindowInteropHelper(this).Handle; //Window handle almak için.
            if (regApp.GetValue("pcuzaktankontrolwifipro") == null)
            {
                chkBaslangic.IsChecked = false;
            }
            else
            {
                chkBaslangic.IsChecked = true;
            }

            FileStream fs = new FileStream(AppDomain.CurrentDomain.BaseDirectory + "\\Ayarlar.txt", FileMode.Open, FileAccess.Read);
            StreamReader sw = new StreamReader(fs);
            string yazi;
            while ((yazi = sw.ReadLine()) != null)
            {
                if (yazi.IndexOf("port:") != -1)
                {
                    port = yazi.Replace("port:", "");
                }
                if (yazi.IndexOf("sesportu:") != -1)
                {
                    sesPortu = yazi.Replace("sesportu:", "");                    
                }
            }
            sw.Close();
            fs.Close();


            try
            {
                TcpDinleyicisi = new TcpListener(Convert.ToInt32(port));
                TcpDinleyicisi.Start();
                txtPort.Text = "PORT: " + port;
                txtPort.Foreground = Brushes.Green;
            }
            catch
            {
                txtPort.Text = "PORT: " + port;
                txtPort.Foreground = Brushes.Red;
                MessageBox.Show(port + " Kullanılmakta Ayarlar.txt den port bilgisini değiştirebilirsiniz.");
                
            }

            serverSocketTh = new Thread(clientiOku);
            serverSocketTh.Start();

            DispatcherTimer dispatcherTimer = new DispatcherTimer();
            dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);
            dispatcherTimer.Interval = new TimeSpan(0, 0, 1);
            dispatcherTimer.Start();


            // ############ BİLGİSAYAR SESLERİNİ GÖNDERME ############
            bilgisayarSesleriniOku();

            //Ses göndermede çakışma olmaması için burada çalışması gereklidir!
            defaultPlaybackDevice = new CoreAudioController().DefaultPlaybackDevice;
        }


        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            txtIpInfo.Text = "Bağlanmanız için IP: " + GetLocalIPAddress();
        }

        WasapiLoopbackCapture waveInStream;        
        void bilgisayarSesleriniOku()
        {
            //Bilgisayar Seslerini Okumak için            
            waveInStream = new WasapiLoopbackCapture(); // Bilgisayardaki tüm sesleri verir.                        
            waveInStream.DataAvailable += new EventHandler<WaveInEventArgs>(this.OnDataAvailable); //Ses bilgileri eventa gidecek.
            waveInStream.RecordingStopped += new EventHandler<StoppedEventArgs>(this.OnDataStopped); //Durdurulduğunda çalışacak event.
            //waveInStream.StartRecording();//test
        }

        WaveStreamToConvert16 WSTC = new WaveStreamToConvert16();
        private void OnDataAvailable(object sender, WaveInEventArgs e) //38400
        {
            //byte[] output = Convert16(e.Buffer, e.BytesRecorded, waveInStream.WaveFormat);                        
            //MessageBox.Show(WSTC.Convert16(e.Buffer, e.BytesRecorded, waveInStream.WaveFormat, 16000).Length.ToString());

            if(waveInStream.CaptureState == CaptureState.Capturing)
                clienteSesleriGonder(WSTC.Convert16(e.Buffer, e.BytesRecorded, waveInStream.WaveFormat, 16000));
        }

        private void OnDataStopped(object sender, StoppedEventArgs e)
        {    
            if (waveInStream != null)
            {
                waveInStream.Dispose(); //ramden stream siliniyor.
                //Stream üzerinde daha kalmasına karşılık, Sabit veriler Tekrar oluşturuluyor.
                bilgisayarSesleriniOku();
            }
                
        }

        void clienteSesleriGonder(byte[] outStream)
        {
            new Thread(() =>
            {
                try
                {                    
                    TcpClient clientSocket = new TcpClient();
                    clientSocket.Connect(clientip, Convert.ToInt32(sesPortu)); //"127.0.0.1" //clientip                    
                    NetworkStream serverStream = clientSocket.GetStream();
                    serverStream.Write(outStream, 0, outStream.Length);
                    serverStream.Flush();
                    serverStream.Close();
                    clientSocket.Close();
                }
                catch
                {
                    //MessageBox.Show("Hata: " + ex.ToString());
                    if (waveInStream != null) //Eğer hata olursa muhtemel Clientin yani telefondaki app kapatıldığı yada bağlantının bir şekilde kesildiğinden Kayıt durdurulacak.
                    {
                        waveInStream.StopRecording();
                    }
                }
            }).Start();

        }

       

        string clientip = "";
        Socket IstemciSoketi;
        bool IstemciSoketiDinle = true;
        TcpListener TcpDinleyicisi;
        Thread serverSocketTh;
        string port = "5000";
        string sesPortu = "3090";
        void clientiOku()
        {
            //istemci server bağlama
            while (IstemciSoketiDinle)
            {
                try
                {
                    IstemciSoketi = TcpDinleyicisi.AcceptSocket();
                    if (!IstemciSoketi.Connected)
                    {
                        MessageBox.Show("Sunucu bağlantısı sağlanamadı!");
                    }
                    else
                    {
                        NetworkStream AgAkimi = new NetworkStream(IstemciSoketi);
                        StreamWriter AkimYazici = new StreamWriter(AgAkimi);
                        StreamReader AkimOkuyucu = new StreamReader(AgAkimi);
                        Random rnd = new Random();
                        try
                        {
                            string IstemciString = AkimOkuyucu.ReadLine();
                            if (ReferenceEquals(IstemciString, null))
                            {
                                //MessageBox.Show("Null");
                                Dispatcher.Invoke(() =>
                                {
                                    txtDrm.Text = "Komut: NULL";
                                });
                            }
                            else
                            {
                                if (IstemciString.IndexOf("clientip=") != -1)
                                {
                                    clientip = IstemciString.Replace("clientip=", "");
                                    Dispatcher.Invoke(() =>
                                    {
                                        txtBaglanan.Text = "Bağlanan: IP: " + clientip + "";
                                        txtBaglanan.Foreground = Brushes.Green;

                                    });
                                }
                                else if (IstemciString.IndexOf("sesBilgiAl=") != -1)
                                {
                                    clienteGonder("ses", defaultPlaybackDevice.Volume.ToString());
                                }
                                else if (IstemciString.IndexOf("ses=") != -1)
                                {
                                    int masterVolume = Convert.ToInt32(IstemciString.Replace("ses=", ""));
                                    defaultPlaybackDevice.Volume = masterVolume;
                                }
                                else if (IstemciString.IndexOf("tarayici=") != -1)
                                {
                                    string url = IstemciString.Replace("tarayici=", "");
                                    if (url.IndexOf("tarayicikpt") != -1)
                                    {

                                        url = url.Replace("tarayicikpt", "");
                                        url = url.Trim();
                                        //Açık olan tarayıcıları kapat.
                                        Process[] AllProcesses = Process.GetProcesses();
                                        foreach (Process process in AllProcesses)
                                        {
                                            if (process.MainWindowTitle != "")
                                            {
                                                string s = process.ProcessName.ToLower();
                                                if (s == "iexplore" || s == "iexplorer" || s == "chrome" || s == "firefox")
                                                    process.Kill();
                                            }
                                        }
                                        Thread.Sleep(300);
                                        Process.Start(url);

                                    }
                                    else
                                    {
                                        Process.Start(url);
                                    }

                                }
                                else if (IstemciString.IndexOf("anaSesDurum=") != -1)
                                {
                                    string anaSesDurum = IstemciString.Replace("anaSesDurum=", "");
                                    if (anaSesDurum == "ac")
                                    {
                                        defaultPlaybackDevice.Mute(false);
                                    }
                                    else if (anaSesDurum == "kpt")
                                    {
                                        defaultPlaybackDevice.Mute(true);
                                    }
                                }
                                else if (IstemciString.IndexOf("playerDurdur=") != -1)
                                {
                                    SendMessageW(windowHandle, WM_APPCOMMAND, windowHandle, (IntPtr)APPCOMMAND_MEDIA_STOP);
                                }
                                else if (IstemciString.IndexOf("playerBaslat=") != -1)
                                {
                                    SendMessageW(windowHandle, WM_APPCOMMAND, windowHandle, (IntPtr)APPCOMMAND_MEDIA_PLAY);
                                }
                                else if (IstemciString.IndexOf("playerOnceki=") != -1)
                                {
                                    SendMessageW(windowHandle, WM_APPCOMMAND, windowHandle, (IntPtr)APPCOMMAND_MEDIA_PREVIOUSTRACK);
                                }
                                else if (IstemciString.IndexOf("playerSonraki=") != -1)
                                {
                                    SendMessageW(windowHandle, WM_APPCOMMAND, windowHandle, (IntPtr)APPCOMMAND_MEDIA_NEXTTRACK);
                                }
                                else if (IstemciString.IndexOf("klavye=") != -1)
                                {
                                    string key = IstemciString.Replace("klavye=", "");
                                    try
                                    {
                                        winForm.SendKeys.SendWait(key);
                                    }
                                    catch
                                    {

                                    }


                                }
                                else if (IstemciString.IndexOf("fare=") != -1)
                                {
                                    new Thread(() =>
                                    {
                                        try
                                        {
                                            string yeniKoordinatlar = IstemciString.Replace("fare=", "");
                                            string[] point = yeniKoordinatlar.Split(',');
                                            /*int x = Convert.ToInt32(point[0]);
                                            int y = Convert.ToInt32(point[1]);*/
                                            Point mouseposition = GetMousePosition();
                                            SetCursorPos((int)(mouseposition.X + Convert.ToInt32(point[0])), (int)(mouseposition.Y + Convert.ToInt32(point[1])));
                                        }
                                        catch
                                        {

                                        }

                                    }).Start();




                                }
                                else if (IstemciString.IndexOf("fareleftclick=") != -1)
                                {
                                    Win32.mouse_event(Win32.MOUSEEVENTF_LEFTDOWN, 0, 0, 0, new System.IntPtr());
                                    Win32.mouse_event(Win32.MOUSEEVENTF_LEFTUP, 0, 0, 0, new System.IntPtr());
                                }
                                else if (IstemciString.IndexOf("farerightclick=") != -1)
                                {
                                    Win32.mouse_event(Win32.MOUSEEVENTF_RIGHTDOWN, 0, 0, 0, new System.IntPtr());
                                    Win32.mouse_event(Win32.MOUSEEVENTF_RIGHTUP, 0, 0, 0, new System.IntPtr());
                                }
                                else if (IstemciString.IndexOf("shutdown=") != -1)
                                {
                                    string shutdown = IstemciString.Replace("shutdown=", "");
                                    switch (shutdown)
                                    {
                                        case "kapat":
                                            ExitWindows.Shutdown(true);
                                            break;
                                        case "restart":
                                            ExitWindows.Reboot(true);
                                            break;
                                        case "uyku":
                                            ExitWindows.Sleep(true);
                                            break;
                                        case "kilit":
                                            ExitWindows.LogOff(true);
                                            break;
                                    }

                                }
                                else if (IstemciString.IndexOf("sesPortuBilgi=") != -1)
                                {
                                    clienteGonder("sesPortu", sesPortu);
                                }
                                else if(IstemciString.IndexOf("bilgisayarSesleriniGonder=") != -1)
                                {
                                    waveInStream.StartRecording();
                                }
                                else if (IstemciString.IndexOf("bilgisayarSesleriniDurdur=") != -1)
                                {
                                    waveInStream.StopRecording();
                                }

                                Dispatcher.Invoke(() =>
                                {
                                    txtDrm.Text = "Komut:" + IstemciString;
                                    txtDrm.Foreground = new SolidColorBrush(Color.FromArgb(255, Convert.ToByte(rnd.Next(0, 255)), Convert.ToByte(rnd.Next(0, 255)), Convert.ToByte(rnd.Next(0, 255))));
                                });


                            }
                        }
                        catch // (IOException e)
                        {
                            //MessageBox.Show("Hata:" + e);
                            break;
                        }

                    }
                    IstemciSoketi.Close();


                }
                catch // (IOException e)
                {
                    //MessageBox.Show("Hata:" + e);
                }

            }

          


        }


        void clienteGonder(string tag, string deger)
        {
            new Thread(() =>
            {
                try
                {
                    //IPAddress ip = IPAddress.Parse(clientip);
                    TcpClient clientSocket = new TcpClient();
                    clientSocket.Connect(clientip, Convert.ToInt32(port)); //EMULATOR IP 192.168.1.103
                    NetworkStream serverStream = clientSocket.GetStream();
                    byte[] outStream = Encoding.ASCII.GetBytes(tag + "=" + deger);
                    serverStream.Write(outStream, 0, outStream.Length);
                    serverStream.Flush();
                    serverStream.Close();
                    clientSocket.Close();
                }
                catch // (IOException ex)
                {
                    //MessageBox.Show("Hata: " + ex.ToString());
                }
            }).Start();

        }

        public string GetLocalIPAddress()
        {
            //Bağlantı sağlanmış ip adresleri:
            string ipAdresleri = "";
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    if (ip.ToString().IndexOf("192.168.1.") != -1)
                    {
                        ipAdresleri += ip.ToString() + " ";
                    }

                }
            }
            ipAdresleri += "\nAlternatif IP: ";
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    if (ip.ToString().IndexOf("192.168.1.") == -1)
                    {
                        ipAdresleri += ip.ToString() + " | ";
                    }
                }
            }

            ipAdresleri.Trim();
            if (ipAdresleri != "")
                return ipAdresleri;
            else
                return "Bağlı bir network ağı bulunamadı!";
        }


        private void chkBaslangic_chkBox_Checked(object sender, RoutedEventArgs e)
        {
            if (chkBaslangic.IsChecked == true)
            {
                regApp.SetValue("pcuzaktankontrolwifipro", System.Reflection.Assembly.GetExecutingAssembly().Location);
            }
            else
            {
                regApp.DeleteValue("pcuzaktankontrolwifipro");
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            AppHide();
            e.Cancel = true;
            
        }

        void AppClose()
        {
            IstemciSoketiDinle = false;

            try
            {
                TcpDinleyicisi.Stop();
            }
            catch { }

            try
            {
                serverSocketTh.Abort();
            }
            catch { }

            try
            {
                IstemciSoketi.Shutdown(SocketShutdown.Both);
                IstemciSoketi.Close();
                IstemciSoketi.Dispose();
            }
            catch { }


            Process.GetCurrentProcess().Kill();
        }

        private void TaskbarIcon_Exit_Click(object sender, RoutedEventArgs e)
        {
            AppClose();
        }

        void AppHide()
        {
            this.Hide();
            this.WindowState = WindowState.Minimized;
        }

        void AppShow()
        {
            this.Show();
            this.WindowState = WindowState.Normal;
            this.Focus();
        }

        private void TaskbarIcon_Show_Click(object sender, RoutedEventArgs e)
        {
            AppShow();
        }

        private void Taskbar_MouseClick(object sender, RoutedEventArgs e)
        {
            if (this.Visibility == Visibility.Visible)
            {
                AppHide();
            }
            else
            {
                AppShow();
            }
            
        }


        private void ntfTskIcon_TrayMouseDoubleClick(object sender, RoutedEventArgs e)
        {
            AppShow();             
        }

        //Window Minimize Event
        private IntPtr HandleMessages(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            // 0x0112 == WM_SYSCOMMAND, 'Window' command message.
            // 0xF020 == SC_MINIMIZE, command to minimize the window.
            if (msg == 0x0112 && ((int)wParam & 0xFFF0) == 0xF020)
            {
                // Cancel the minimize.
                handled = true;
                AppHide();
            }

            return IntPtr.Zero;
        }

       
    }


}
