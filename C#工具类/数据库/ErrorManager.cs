using System;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Threading;

namespace FenWeiPingYuanService
{
    public class ErrorManager
    {
        //读写锁，当资源处于写入模式时，其他线程写入需要等待本次写入结束之后才能继续写入
        static ReaderWriterLockSlim LogWriteLock = new ReaderWriterLockSlim();
        private static string _lastError = String.Empty;
        /// <summary>
        /// 错误日志
        /// </summary>
        /// <param name="ex">异常对象</param>
        public static void AddErrorToLog(Exception ex, string message)
        {
            try
            {
                LogWriteLock.EnterWriteLock();
                if (_lastError != ex.Message)
                {
                    StringBuilder error = new System.Text.StringBuilder();
                    //构建错误消息
                    error.Append("错误时间：").AppendLine(DateTime.Now.ToString());
                    error.Append("错误消息：").AppendLine(ex.Message);
                    error.Append("堆栈跟踪：").AppendLine(ex.StackTrace);
                    if (message != null)
                    {
                        error.Append("辅助信息：").AppendLine(message);
                    }
                    error.AppendLine();
                    if (!Directory.Exists(Application.StartupPath + @"\ErrorLogs"))
                    {
                        Directory.CreateDirectory(Application.StartupPath + @"\ErrorLogs");
                    }
                    string errorLogPath = Application.StartupPath + @"\ErrorLogs\" + DateTime.Now.ToString("MM-dd") + "error.txt";
                    System.IO.File.AppendAllText(errorLogPath, error.ToString());
                    _lastError = ex.Message;
                }
            }
            finally
            {
                LogWriteLock.ExitWriteLock();
            }
        }
        public static void AddErrorToLog(string str)
        {
            try
            {
                LogWriteLock.EnterWriteLock();
                if (!Directory.Exists(Application.StartupPath + @"\ErrorLogs"))
                {
                    Directory.CreateDirectory(Application.StartupPath + @"\ErrorLogs");
                }
                string errorLogPath = Application.StartupPath + @"\ErrorLogs\" + DateTime.Now.ToString("MM-dd") + "tips.txt";
                System.IO.File.AppendAllText(errorLogPath, str);

            }
            finally
            {
                LogWriteLock.ExitWriteLock();
            }
        }
    }
}
