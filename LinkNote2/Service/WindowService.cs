using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace LinkNote2.Service
{
    class WindowService
    {
        #region Static
        private static WindowService _Instance;
        public static void Init()
        {
            Init(Application.OpenForms.Cast<Form>().First());
        }

        public static void Init(Form mainForm)
        {
            if (mainForm == null)
            {
                throw new ArgumentNullException("mainForm must be not null!");
            }
            _Instance = new WindowService(mainForm);
        }
        #endregion

        #region Instance
        private readonly Form _form;

        private WindowService(Form form)
        {
            _form = form;
        }

        public static WindowService Instance
        {
            get
            {
                if (_Instance == null)
                {
                    throw new InvalidOperationException("Must be Init");
                }
                return _Instance;
            }
        }

        public Form Form
        {
            get { return _form; }
        }

        public DialogResult ShowMessage(string message, string title)
        {
            return MessageBox.Show(_form, message, title);
        }
        
        #endregion
    }
}
