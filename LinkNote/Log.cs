using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace System
{
    public static class Log
    {
        static StreamWriter sw;
        static Log()
        {
            try
            {
                if (File.GetLastWriteTime("log.log").Date == DateTime.Now.Date)
                    sw = new StreamWriter("log.log", true, System.Text.Encoding.UTF8);
                else
                    sw = new StreamWriter("log.log", false, System.Text.Encoding.UTF8);
            }
            catch { }
        }
        public static void LogDebug(object obj)
        {
#if !DEBUG
            return;
#else
            try
            {
                if (sw == null)
                    return;
                sw.WriteLine("{0}\t{1}\t{2}", DateTime.Now.ToString(), "Debug", obj.ToString());
                sw.Flush();
            }
            catch { }
#endif
        }
        public static void LogInfo(object obj)
        {
            try
            {
                if (sw == null)
                    return;
                sw.WriteLine("{0}\t{1}\t{2}", DateTime.Now.ToString(), "Info", obj.ToString());
                sw.Flush();
            }
            catch { }
        }
        public static void LogError(object obj)
        {
            try
            {
                if (sw == null)
                    return;
                sw.WriteLine("{0}\t{1}\t{2}", DateTime.Now.ToString(), "Error", obj.ToString());
                sw.Flush();
            }
            catch { }
        }
        public static void LogWarn(object obj)
        {
            try
            {
                if (sw == null)
                    return;
                sw.WriteLine("{0}\t{1}\t{2}", DateTime.Now.ToString(), "Warn", obj.ToString());
                sw.Flush();
            }
            catch { }
        }
    }
}
