using System.Diagnostics;

namespace WPF进程管理器
{
    internal class ProcessObject
    {
        public ProcessObject(string path) => this.path = path;
        private string path;
        private Process process;
        private bool isShow = true;
        /// <summary>
        /// 启动进程
        /// </summary>
        /// <returns>true:成功 false:失败</returns>
        public bool RunProcess()
        {
            process = new Process();
            process.StartInfo.FileName = path;
            process.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
            process.StartInfo.UseShellExecute = true;
            try { process.Start(); return true; }
            catch { return false; }
        }
        /// <summary>
        /// 重启
        /// </summary>
        public void RebootProcess()
        { 
            KillProcess();
            RunProcess();
            isShow = true;
        }
        /// <summary>
        /// 关闭
        /// </summary>
        public void KillProcess()
        {
            if(process.HasExited) { return; }
            process.CloseMainWindow();
        }

        /// <summary>
        /// 如果显示就隐藏，反之显示
        /// </summary>
        /// <returns>返回最后的状态</returns>
        public bool ShowProcess()
        {
            if (isShow)
            {
                MainWindow.ShowWindow(process.MainWindowHandle, 0);
                isShow = false;
            }
            else
            {
                MainWindow.ShowWindow(process.MainWindowHandle, 5);
                isShow = true;
            }
            return isShow;
        }
    }
}
