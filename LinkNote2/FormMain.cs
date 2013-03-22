using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Awesomium.Core;
using DropNet;
using DropNet.Models;
using LinkNote2.Data;

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
            _Web.Source = new Uri(dropNetClient.BuildAuthorizeUrl("asset://www.app.local/index.html"));
            _Web.TargetURLChanged += _Web_TargetURLChanged;
            //            _MainForm.DocumentStream
            //var x = dropNetClient.GetAccessToken();
        }

        void _Web_TargetURLChanged(object sender, Awesomium.Core.UrlEventArgs e)
        {

        }
    }
}
