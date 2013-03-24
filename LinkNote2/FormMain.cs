using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using Awesomium.Core;
using DropNet;
using DropNet.Models;
using LinkNote2.Data;
using LinkNote2.Service;

namespace LinkNote2
{
    public partial class FormMain : Form
    {
        public FormMain()
        {
            InitializeComponent();

        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);

            WindowService.Init(this);

            var dropNetClient = DropBoxHelper.CurrentClient();
            dropNetClient.GetToken();
#if DEBUG
            var dataSource = new DirectoryDataSource(Environment.CurrentDirectory + "\\..\\..\\Asset\\www\\");
#else
            var dataSource = new GzipDataSource(Environment.CurrentDirectory + "\\core.pak");
#endif
            _Web.WebSession = WebCore.CreateWebSession(".\\tmp\\", new WebPreferences()
            {
                FileAccessFromFileURL = true,
                UniversalAccessFromFileURL = true,
                WebSecurity = false,
                CanScriptsAccessClipboard = true,
            });
            _Web.WebSession.AddDataSource("www.app.local", dataSource);
            _Web.WebSession.AddDataSource("service.app.local", new GzipDataSource(Environment.CurrentDirectory + "\\core.pak"));
            _Web.DocumentReady += _Web_ProcessCreated;
            _Web.Source = new Uri("asset://www.app.local/index.html");
            //            _Web.Source = new Uri(dropNetClient.BuildAuthorizeUrl("asset://www.app.local/index.html"));
            //var x = dropNetClient.GetAccessToken();
        }

        void _Web_ProcessCreated(object sender, WebViewEventArgs e)
        {
            _Web.DocumentReady -= _Web_ProcessCreated;
            dynamic appService = (JSObject)_Web.CreateGlobalJavascriptObject("$app");
            dynamic winService = (JSObject)_Web.CreateGlobalJavascriptObject("$win");
            dynamic indexService = (JSObject)_Web.CreateGlobalJavascriptObject("$index");
            dynamic commonService = (JSObject)_Web.CreateGlobalJavascriptObject("$common");
            appService.win = winService;
            appService.index = indexService;
            appService.common = commonService;

            winService.ShowMessage = (JavascriptMethodEventHandler)((s, ex) =>
            {
                var msg = ex.Arguments.Length > 1 ? ex.Arguments[0].ToString() : string.Empty;
                var title = ex.Arguments.Length > 2 ? ex.Arguments[1].ToString() : string.Empty;
                ex.Result = new JSValue((int)WindowService.Instance.ShowMessage(msg, title));
            });
        }
    }
}
