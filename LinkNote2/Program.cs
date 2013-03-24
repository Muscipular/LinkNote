using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using Awesomium.Core;
using DropNet;

namespace LinkNote2
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        private static void Main()
        {
            WebCore.Initialize(new WebConfig()
            {
//                HomeURL = new Uri("asset://www.app.local/index.html"),
                LogLevel = LogLevel.Verbose,
            });
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new FormMain());
        }
    }
}
