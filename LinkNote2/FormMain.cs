using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using DropNet;
using DropNet.Models;

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
            _Web.Source = new Uri(dropNetClient.BuildAuthorizeUrl());
            //            _MainForm.DocumentStream
            //var x = dropNetClient.GetAccessToken();
        }
    }
}
