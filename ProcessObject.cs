using System.Diagnostics;
using System.IO;

namespace WPF进程管理器
{
    internal class ProcessObject
    {
        public ProcessObject(string path) => this.path = path;
        private string path;
        private Process process;
        private bool isShow;
        /// <summary>
        /// 启动进程
        /// </summary>
        /// <returns>true:成功 false:失败</returns>
        public bool RunProcess()
        {
            process = new Process();
            process.StartInfo.FileName = path;
            process.StartInfo.WorkingDirectory = Path.GetDirectoryName(path);
            process.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
            process.StartInfo.UseShellExecute = true;
            try { 
                process.Start();
                isShow = true;
                return true; 
            }
            catch  { return false; }
        }
        /// <summary>
        /// 重启
        /// </summary>
        public void RebootProcess()
        { 
            KillProcess();
            RunProcess();
        }
        /// <summary>
        /// 关闭
        /// </summary>
        public void KillProcess()
        {
            if(process.HasExited) { return; }
            process.CloseMainWindow();
            isShow = false;
        }

        public bool ShowProcess()
        {
            if (isShow)
            {
                try { 
                    MainWindow.ShowWindow(process.MainWindowHandle, 0);
                    isShow = false;
                }//隐藏
                catch { isShow = true; }
            }
            else
            {
                try { 
                    MainWindow.ShowWindow(process.MainWindowHandle, 5);
                    isShow = true;
                }//显示
                catch { isShow = false; }
            }
            return isShow;
        }
    }
}
