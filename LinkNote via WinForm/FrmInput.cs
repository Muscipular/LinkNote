using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Muscipular.LinkNote
{
    public sealed partial class FrmInput : Form
    {
        private FrmInput()
        {
            InitializeComponent();
        }

        public static string GetPassword(IWin32Window owner = null, string caption = "输入密码")
        {
            string result = null;
            FrmInput f = new FrmInput();
            f.Text = caption;
            f.tbxInput.ImeMode = ImeMode.Off;
            f.tbxInput.PasswordChar = '*';
            var r = f.ShowDialog(owner);
            result = f.tbxInput.Text;
            return r == DialogResult.OK ? result : null;
        }
        public static string GetInput(IWin32Window owner = null, string caption = "输入字符", string @default = "")
        {
            string result = null;
            FrmInput f = new FrmInput();
            f.Text = caption;
            f.tbxInput.Text = @default;
            var r = f.ShowDialog(owner);
            result = f.tbxInput.Text;
            return r == DialogResult.OK ? result : null;
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.Close();
        }
    }
}
