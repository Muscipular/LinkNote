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
            var ass = Assembly.GetExecutingAssembly();
            var x = new[] {
                "LinkNote2.Libs.DropNet.dll",
                "LinkNote2.Libs.Newtonsoft.Json.dll",
                "LinkNote2.Libs.RestSharp.dll"
            };
            foreach (var s in x)
            {
                var stream = ass.GetManifestResourceStream(s);
                if (stream == null)
                {
                    continue;
                }
                byte[] data = new byte[stream.Length];
                stream.Read(data, 0, data.Length);
                stream.Dispose();
                AppDomain.CurrentDomain.Load(data);
            }
            WebCore.Initialize(new WebConfig()
            {
                HomeURL = new Uri("asset://www.app.local/index.html"),
                LogLevel = LogLevel.Verbose,
            });
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new FormMain());
        }
    }
}
