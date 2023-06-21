using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml.Linq;
using Button = System.Windows.Controls.Button;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;
using Window = System.Windows.Window;

namespace WPF进程管理器
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private Dictionary<string, string> buttonUni;//按钮图标
        private Dictionary<string, ProcessObject> processDict;//进程字典
        private Dictionary<string, Border> BorderDict;//Border字典
        private Grid inputPopup;
        private string processBox;//进程Borde控件模板
        private Brush green, red;
        private Border rightClickBorder;
        private NotifyIcon notifyIcon;
        private bool isSearch = false;
        [DllImport("user32.dll")]
        public static extern bool ShowWindow(IntPtr hWnd, int _value);

        public MainWindow()//启动时运行
        {
            //先检测自己是否打开
            Process[] processes = Process.GetProcessesByName("ProcessManager");
            if (processes.Length > 1)
            {
                // 程序已运行,存在进程
                System.Windows.MessageBox.Show("你已打开一个进程管理器", "警告", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                Close();
            }
            InitializeComponent();
            InitResources();
            AutoStart();
            ShowBalloonTip("进程管理器", "进入后台运行");
            Hide();
        }
        /// <summary>
        /// 初始化资源
        /// </summary>
        private void InitResources()
        {
            inputPopup = (Grid)FindName("popup");
            processBox = XamlWriter.Save(ProcessBox);
            buttonUni = new Dictionary<string, string>
        {
            { "启动", "▶" },
            { "停止", "⬛" },
            { "重启", "🔁" },
            { "显示", "🔑" },
            { "隐藏", "🔒" },
            { "搜索", "🔍" },
            { "清除", "🧹" },
        };
            processDict = new Dictionary<string, ProcessObject>();
            BorderDict = new Dictionary<string, Border>();
            green = (Brush)new BrushConverter().ConvertFromString("#2bd47d");
            red = (Brush)new BrushConverter().ConvertFromString("#d13333");
            Dictionary<string, string> dict = XmlWith.ReadXmlFile();
            if (dict != null)
            {
                foreach (string key in dict.Keys)
                {
                    CreateProcessBox(key, dict[key]);
                }
            }
            string backPath = XmlWith.GetBackgroundPath();
            if (backPath != "null") appBackGround.ImageSource = new BitmapImage(new Uri(backPath));
            CreateTuoPan();
        }
        //自启
        private void AutoStart()
        {
            foreach (string key in BorderDict.Keys)  StartProcess(key);
        }
        //创建托盘
        private void CreateTuoPan()
        {
            notifyIcon = new NotifyIcon();
            //设置图标 
            notifyIcon.Icon = System.Drawing.Icon.ExtractAssociatedIcon(Assembly.GetEntryAssembly().Location);
            //显示图标
            notifyIcon.Visible = true;
            //菜单
            System.Windows.Forms.ContextMenu contextMenu = new System.Windows.Forms.ContextMenu();
            System.Windows.Forms.MenuItem openItem = new System.Windows.Forms.MenuItem("👋打开程序");
            openItem.Click += (s, e) => Show();//显示窗口
            contextMenu.MenuItems.Add(openItem);
/*            System.Windows.Forms.MenuItem exitItem = new System.Windows.Forms.MenuItem("👻退出程序");
            exitItem.Click += (s, e) => Close();
            contextMenu.MenuItems.Add(exitItem);*/
            System.Windows.Forms.MenuItem whatItem = new System.Windows.Forms.MenuItem("🚀这是什么");
            whatItem.Click += (s, e) =>
                ShowBalloonTip("Stand对你说:", "这是什么？我也不知道|･ω･｀)");
            contextMenu.MenuItems.Add(whatItem);
            notifyIcon.ContextMenu = contextMenu;
            //鼠标点击出现
            notifyIcon.MouseClick += (sender, args) => Show();
            //处理气球提示点击 
            notifyIcon.BalloonTipClicked += (sender, args) => Show();
            //弹出消息的图标
            notifyIcon.BalloonTipIcon = ToolTipIcon.Info;
        }
        /// <summary>
        /// 右下角弹出消息
        /// </summary>
        private void ShowBalloonTip(string title, string message)
        {
            notifyIcon.BalloonTipTitle = title;
            notifyIcon.BalloonTipText = message;
            notifyIcon.ShowBalloonTip(3000);
        }
        //最小化
        private void MinimizeButton_Click(object sender, RoutedEventArgs e)=>WindowState = WindowState.Minimized;
        //隐藏程序
        private void HideButton_Click(object sender, RoutedEventArgs e)
        {
            ShowBalloonTip("进程管理器", "进入后台运行");
            Hide();
        }
        //主窗口标题栏左键拖动
        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)=> DragMove();
        //右键菜单
        private void ProProcessRightDown(object sender, MouseButtonEventArgs e)
        {
            rightClickBorder = (Border)sender;
            Point position = Mouse.GetPosition(rightClickBorder);
            System.Windows.Controls.ContextMenu menu = new System.Windows.Controls.ContextMenu();
            menu.Template = (ControlTemplate)FindResource("ContextMenu");
            menu.PlacementTarget = rightClickBorder;
            menu.PlacementRectangle = new Rect(position, new Size(0, 0));
            menu.IsOpen = true;
        }
        //编辑
        private void WriteProcess(object sender, RoutedEventArgs e)
        {
            string name = RightClickBtn();
            TextBlock ProcessName = (TextBlock)BorderDict[name].FindName("ProcessName");
            TextBlock ProcessPath = (TextBlock)BorderDict[name].FindName("ProcessPath");
            InputPathBox.Text = ProcessName.Text;
            InputNamebox.Text = ProcessPath.Text;
            AddPopup();
        }
        //删除
        private void DeleteProcess(object sender, RoutedEventArgs e)
        {
            string name = RightClickBtn();
            BorderDict.Remove(name);
            ProcessPanel.Children.Remove(rightClickBorder);
            XmlWith.DeleteToXmlFile(name);
        }
        //右键菜单的按钮
        private string RightClickBtn()
        {
            string name = (string)rightClickBorder.Tag;
            if (processDict.ContainsKey(name))
            {
                processDict[name].KillProcess();//先停止运行
                processDict.Remove(name);
            }
            return name;
        }
        //添加程序窗口显隐
        private void AddPopup()
        {
            if (inputPopup.Visibility == Visibility.Visible) inputPopup.Visibility = Visibility.Hidden;
            else inputPopup.Visibility = Visibility.Visible;
        }
        //选择文件
        private void ChooseFile(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "所有文件|*.*";
            openFileDialog.InitialDirectory = @"C:\";
            openFileDialog.Title = "选择文件";
            openFileDialog.Multiselect = false;//取消多选
            if (openFileDialog.ShowDialog() == true)
            {
                // 用户选择了文件,进行后续操作
                string path = openFileDialog.FileName;
                InputPathBox.Text = path;
            }
        }
        //确定选择
        private void AddRightClick(object sender, RoutedEventArgs e)
        {
            //有一个为空，第一个字数字，有空格
            if (InputPathBox.Text == "" || InputNamebox.Text == "" || 
                char.IsDigit(InputNamebox.Text[0]) || InputNamebox.Text.Contains(" ")) 
                return;
            //如果是需要编辑的
            if (rightClickBorder != null)
            {
                TextBlock ChangePath = (TextBlock)rightClickBorder.FindName("ProcessPath");
                //名字已存在，只修改路径
                if (BorderDict.ContainsKey(InputNamebox.Text)) ChangePath.Text = InputPathBox.Text;//路径
                else 
                {
                    TextBlock ChangeName = (TextBlock)rightClickBorder.FindName("ProcessName");
                    Button Changebutton = (Button)rightClickBorder.FindName("ProcessBtn1");
                    Button Changebutton2 = (Button)rightClickBorder.FindName("ProcessBtn2");
                    Button Changebutton3 = (Button)rightClickBorder.FindName("ProcessBtn3");
                    ChangeName.Text = InputNamebox.Text;//进程名
                    ChangePath.Text = InputPathBox.Text;//路径
                    BorderDict.Remove((string)rightClickBorder.Tag);//删掉
                    BorderDict.Add(InputNamebox.Text, rightClickBorder);//添加
                    rightClickBorder.Tag = ChangeName.Text;//tag设置为进程名
                    Changebutton.Tag = ChangeName.Text;
                    Changebutton2.Tag = ChangeName.Text;
                    Changebutton3.Tag = ChangeName.Text;
                }
                XmlWith.DeleteToXmlFile(InputNamebox.Text);//如果是编辑的先删除
            }
            if (BorderDict.ContainsKey(InputNamebox.Text)) return;
            CreateProcessBox(InputNamebox.Text, InputPathBox.Text);
            XmlWith.WriteToXmlFile(InputNamebox.Text, InputPathBox.Text);//写入
            AddPopup();//最后关闭窗口
        }
        //添加程序按钮
        private void AddProcessClick(object sender, RoutedEventArgs e)
        {
            rightClickBorder = null;
            InputPathBox.Text = "";
            InputNamebox.Text = "";
            AddPopup();
        }
        //启动关闭进程按钮
        private void SatrtProcessClick(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            string name = (string)button.Tag;
            StartProcess(name);
        }
        /// <summary>
        /// 启动进程
        /// </summary>
        /// <param name="name"></param>
        void StartProcess(string name)
        {
            Border border = BorderDict[name];
            Button button = (Button)border.FindName("ProcessBtn1");
            Button button2 = (Button)border.FindName("ProcessBtn2");
            Button button3 = (Button)border.FindName("ProcessBtn3");
            TextBlock ProcessPath = (TextBlock)border.FindName("ProcessPath");
            //名字已存在
            if (processDict.ContainsKey(name))
            {
                processDict[name].KillProcess();
                processDict.Remove(name);//移除
                button.Content = buttonUni["启动"];
                button.ToolTip = "启动进程";
                button.Foreground = green;
                button2.Visibility = Visibility.Hidden;
                button3.Visibility = Visibility.Hidden;
            }
            else
            {
                ProcessObject processObject = new ProcessObject(ProcessPath.Text);
                if (!processObject.RunProcess())
                {
                    ShowBalloonTip(name, "启动失败");
                    return;
                }
                ShowBalloonTip(name, "成功启动");
                processDict.Add(name, processObject);
                button.Content = buttonUni["停止"];
                button.ToolTip = "关闭进程";
                button.Foreground = red;
                button2.Visibility = Visibility.Visible;
                button3.Visibility = Visibility.Visible;
                Thread.Sleep(1000); // 暂停 1 秒钟
                ShowProcess(button2, name);
            }
        }
        //显示隐藏按钮
        private void ShowProcessClick(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            string name = (string)button.Tag;
            ShowProcess(button, name);
        }
        private void ShowProcess(Button button, string name)
        {
            if (processDict[name].ShowProcess())//如果返回结果是显示
            {
                button.Content = buttonUni["隐藏"];
                button.ToolTip = "隐藏进程";
                button.Foreground = green;
            }
            else
            {
                button.Content = buttonUni["显示"];
                button.ToolTip = "显示进程";
                button.Foreground = red;
            }
        }
        //重启按钮
        private void RebootProcessClick(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            string name = (string)button.Tag;
            processDict[name].RebootProcess();
            Border border = BorderDict[name];
            Button button2 = (Button)border.FindName("ProcessBtn2");
            button2.Content = buttonUni["隐藏"];
            button2.ToolTip = "隐藏进程";
            button2.Foreground = green;
        }
        //创建进程控件
        private void CreateProcessBox(string name,string path)
        {
            Border border = (Border)XamlReader.Parse(processBox);
            TextBlock ProcessName = (TextBlock)border.FindName("ProcessName");
            TextBlock ProcessPath = (TextBlock)border.FindName("ProcessPath");
            ProcessName.Text = name;//进程名
            ProcessPath.Text = path;//路径
            Button button = (Button)border.FindName("ProcessBtn1");
            Button button2 = (Button)border.FindName("ProcessBtn2");
            Button button3 = (Button)border.FindName("ProcessBtn3");
            button.Click += SatrtProcessClick;//点击事件
            button2.Click += ShowProcessClick;
            button3.Click += RebootProcessClick;
            border.Tag = ProcessName.Text;//tag设置为进程名
            button.Tag = ProcessName.Text;
            button2.Tag = ProcessName.Text;
            button3.Tag = ProcessName.Text;
            border.Visibility = Visibility.Visible;//显示
            border.MouseRightButtonDown += ProProcessRightDown;//右键菜单
            ProcessPanel.Children.Add(border);
            BorderDict.Add(ProcessName.Text, border);
        }
        //改变背景图
        private void ChangeBackground(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "图片文件 (*.jpg;*.png)|*.jpg;*.png";
            openFileDialog.Title = "选择软件背景图";
            openFileDialog.Multiselect = false;//取消多选
            if (openFileDialog.ShowDialog() == true)
            {
                string fileName = openFileDialog.FileName;
                string ext = Path.GetExtension(fileName);
                if (ext == ".jpg" || ext == ".png")
                {
                    appBackGround.ImageSource = new BitmapImage(new Uri(fileName));
                    XmlWith.ChangeBackgroundPath(fileName);
                }
            }
        }
        private void StandSay(object sender, RoutedEventArgs e)=>ShowBalloonTip("Stand对你说:", "你很皮嘛？_(:3」∠)_");
        //搜索
        private void SearchButton_Click(object sender, RoutedEventArgs e) 
        {
            Button button = (Button)sender;
            if (isSearch)//如果是搜索状态，则需要清除
            {
                SearchTextBox.Text = "";
                foreach (Border border in BorderDict.Values) border.Visibility = Visibility.Visible;
                button.Foreground = new SolidColorBrush(Colors.White);
                button.Content = buttonUni["搜索"];
                button.ToolTip = "搜索程序";
                isSearch = false;
            }
            else
            {
                if (SearchTextBox.Text == "") return;
                foreach (string key in BorderDict.Keys)
                {
                    if (!key.Contains(SearchTextBox.Text)) BorderDict[key].Visibility = Visibility.Collapsed;
                }
                button.Foreground = green;
                button.Content = buttonUni["清除"];
                button.ToolTip = "清除搜索";
                isSearch = true;
            }
        }
    }
}
