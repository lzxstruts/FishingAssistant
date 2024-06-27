using CSCore.CoreAudioAPI;
using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace FishingAssistant
{
    public partial class FishingAssistant : Form
    {

        [DllImport("user32", CharSet = CharSet.Ansi, EntryPoint = "FindWindowA", ExactSpelling = true, SetLastError = true)]
        private static extern int FindWindow([MarshalAs(UnmanagedType.VBByRefStr)] ref string lpClassName, [MarshalAs(UnmanagedType.VBByRefStr)] ref string lpWindowName);

        [DllImport("user32", CharSet = CharSet.Ansi, EntryPoint = "MapVirtualKeyA", ExactSpelling = true, SetLastError = true)]
        private static extern int MapVirtualKey(int wCode, int wMapType);

        [DllImport("user32", CharSet = CharSet.Ansi, EntryPoint = "FindWindowExA", ExactSpelling = true, SetLastError = true)]
        private static extern int FindWindowEx(int hWnd1, int hWnd2, [MarshalAs(UnmanagedType.VBByRefStr)] ref string lpsz1, [MarshalAs(UnmanagedType.VBByRefStr)] ref string lpsz2);

        [DllImport("user32", CharSet = CharSet.Ansi, EntryPoint = "PostMessageA", ExactSpelling = true, SetLastError = true)]
        private static extern int PostMessage(int hwnd, int wMsg, int wParam, int lParam);

        [DllImport("kernel32", CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = true)]
        private static extern void Sleep(int dwMilliseconds);


        private AudioSessionControl _wowAudioSession;
        private Thread mtaThread;
        private int dhwnd;
        private bool isRun = false;
        private int volume = 8;

        public FishingAssistant()
        {
            InitializeComponent();



        }

        private AudioSessionManager2 GetDefaultAudioSessionManager(DataFlow dataFlow)
        {
            MMDeviceEnumerator val = new MMDeviceEnumerator();
            MMDevice val2 = val.GetDefaultAudioEndpoint(dataFlow, Role.Console);
            Console.WriteLine("找到默认音频设备: " + val2.FriendlyName);
            return AudioSessionManager2.FromMMDevice(val2);
        }

        private AudioSessionControl GetWowAudioSession()
        {
            using (var defaultAudioSessionManager = GetDefaultAudioSessionManager(0))
            {
                using (var sessionEnumerator = defaultAudioSessionManager.GetSessionEnumerator())
                {
                    foreach (AudioSessionControl session in sessionEnumerator)
                    {
                        using (var sessionControl = session.QueryInterface<AudioSessionControl2>())
                        {
                            if (sessionControl.Process.ProcessName == "Wow" || sessionControl.Process.ProcessName == "WowClassic")
                            {
                                Console.WriteLine("魔兽音频已找到");
                                return session;
                            }
                        }
                    }
                }
            }
            return null;
        }

        private void startBtn_Click(object sender, EventArgs e)
        {
            if (isRun)
            {
                FishingStop();
            }
            else
            {
                FishingStar();
            }
        }

        private void FishingStop()
        {
            startBtn.Text = "开始";
            isRun = false;
            label1.Text = "魔兽句柄:0";
            dhwnd = 0;
            currentState = 0;
        }

        private void FishingStar()
        {

            mtaThread = new Thread((ThreadStart)delegate
            {
                _wowAudioSession = GetWowAudioSession();
                if (_wowAudioSession == null)
                {
                    MessageBox.Show("没有找到魔兽世界音频", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                Application.Run();
            });
            mtaThread.SetApartmentState(ApartmentState.MTA);
            mtaThread.IsBackground = true;
            mtaThread.Start();


            FindWowProcess();

            if (dhwnd == 0)
            {
                MessageBox.Show("没有找到魔兽世界", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                startBtn.Text = "停止";
                string text = textBox1.Text;
                int num;
                if (int.TryParse(text, out num))
                {
                    volume = num;
                }
                else
                {
                    volume = 8;
                }
                isRun = true;

                DoFishing();
            }



        }

        private async void DoFishing()
        {
            currentState = 0;
            await DoFishingAsync();
        }

        // 1等待中 0未开始 
        private int currentState = 0;
        private async Task DoFishingAsync()
        {
            int count = 0;
            while (isRun)
            {
                //检测声音
                var peakPercentage = GetWowAudioPeakValue(_wowAudioSession);
                // 当音量大于设定值时执行
                if (peakPercentage > volume && currentState == 1)
                {
                    Console.WriteLine("检测到有鱼" + peakPercentage);
                    SendKey(dhwnd, 70);
                    await Task.Delay(1300);
                    currentState = 0;
                    count = 0;
                }
                if (currentState == 0)
                {
                    SendKey(dhwnd, 49);
                    Console.WriteLine("发送钓鱼按键");
                    await Task.Delay(3000);
                    currentState = 1;
                    count = 0;
                }
                if (count>=200)
                {
                    SendKey(dhwnd, 49);
                    Console.WriteLine("发送钓鱼按键");
                    await Task.Delay(3000);
                    currentState = 1;
                    count = 0;
                }

                count++;
                await Task.Delay(100);
            }
        }


        private float GetWowAudioPeakValue(AudioSessionControl wowAudioSession)
        {
            if (wowAudioSession != null)
            {
                using (var audioMeterInformation = wowAudioSession.QueryInterface<AudioMeterInformation>())
                {
                    return audioMeterInformation.GetPeakValue() * 100f;
                }
            }
            return 0;
        }




        private void FindWowProcess()
        {
            string text = "GxWindowClass";
            string text2 = null;
            dhwnd = FindWindow(ref text, ref text2);
            label1.Text = "魔兽句柄:" + dhwnd.ToString();
            if (dhwnd != 0)
            {
                Console.WriteLine("魔兽已找到 " + dhwnd.ToString());
            }
        }

        public object SendKey(int hwnd, int vkey)
        {
            Random random = new Random(DateTime.Now.Millisecond);
            int num = PostMessage(hwnd, 256, vkey, random.Next(999, 99999));
            Sleep(20);
            num = PostMessage(hwnd, 257, vkey, random.Next(-999999, -999));
            return num;
        }

        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsDigit(e.KeyChar) && !char.IsControl(e.KeyChar))
            {
                e.Handled = true;
            }
        }

    }
}
