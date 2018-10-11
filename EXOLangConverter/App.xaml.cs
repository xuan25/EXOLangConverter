using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace EXOLangConverter
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            // 在异常由应用程序引发但未进行处理时发生。主要指的是UI线程。
            this.DispatcherUnhandledException += new DispatcherUnhandledExceptionEventHandler(Application_DispatcherUnhandledException);
            //  当某个异常未被捕获时出现。主要指的是非UI线程
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
        }

        void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Exception ex = e.ExceptionObject as Exception;
            MessageBox.Show("An unexpected and unrecoverable problem has occourred. Launcher will now exit.", "Unexpected operation", MessageBoxButton.OK, MessageBoxImage.Error);
            CrashLog("Non-UI thread exceptions : \n\n" + string.Format("Captured an unhandled exception：{0}\r\nException Message：{1}\r\nException StackTrace：{2}", ex.GetType(), ex.Message, ex.StackTrace));
            //MessageBox.Show("Non-UI thread exceptions : \n\n" + string.Format("Captured an unhandled exception：{0}\r\nException Message：{1}\r\nException StackTrace：{2}", ex.GetType(), ex.Message, ex.StackTrace));
            System.Environment.Exit(0);
        }

        private void Application_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            Exception ex = e.Exception;
            MessageBox.Show("An unexpected problem has occourred. Some operation has been terminated.", "Unexpected operation", MessageBoxButton.OK, MessageBoxImage.Information);
            CrashLog("UI thread exception : \n\n" + string.Format("Captured an unhandled exception：{0}\r\nException Message：{1}\r\nException StackTrace：{2}", ex.GetType(), ex.Message, ex.StackTrace));
            //MessageBox.Show("UI thread exception : \n\n" + string.Format("Captured an unhandled exception：{0}\r\nException Message：{1}\r\nException StackTrace：{2}", ex.GetType(), ex.Message, ex.StackTrace));
            e.Handled = true;
        }

        private void CrashLog(string message)
        {
            Directory.CreateDirectory(Environment.CurrentDirectory + "\\CrashLog\\");
            string time = DateTime.Now.ToString().Replace(':', '-').Replace('/', '-');

            int i = 0;
            while (i < 100)
            {
                string filename = time;
                if (i != 0)
                {
                    filename = filename + " (" + i + ")";
                }
                if (!File.Exists(Environment.CurrentDirectory + "\\CrashLog\\" + filename + ".log"))
                {
                    try
                    {
                        StreamWriter SW = new StreamWriter(Environment.CurrentDirectory + "\\CrashLog\\" + filename + ".log", false);
                        SW.WriteLine(message);
                        SW.Close();
                        break;
                    }
                    catch { }
                }
                i++;
            }

        }
    }
}
