using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Muscipular.LinkNote
{
    /// <summary>
    /// InputTextBox.xaml 的交互逻辑
    /// </summary>
    public partial class InputTextBox : Window
    {
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register(
                "Text",
                typeof(string), typeof(InputTextBox),
                new PropertyMetadata(string.Empty)
            );
        public InputTextBox()
        {
            InitializeComponent();
            tbxContent.DataContext = this;
            tbxContent.Loaded += new RoutedEventHandler((obj, e) =>
            {
                tbxContent.Focus();
            });
        }
        public string Text
        {
            get
            {
                if (pwd1.Visibility == System.Windows.Visibility.Visible)
                    return pwd1.Password;
                return GetValue(TextProperty) as string ?? string.Empty;
            }
            set
            {
                if (pwd1.Visibility == System.Windows.Visibility.Visible)
                    pwd1.Password = value ?? string.Empty;
                SetValue(TextProperty, value ?? string.Empty);
            }
        }
        public new bool ShowDialog()
        {
            base.ShowDialog(); 
            tbxContent.Focus();
            return Text != string.Empty;
        }
        public bool ShowDialog(string title, string message, string inputDefault = "", bool isPassword = false)
        {
            this.DataContext = new { Title = title, Message = message };
            if (isPassword)
            {
                pwd1.SetValue(TextBlock.VisibilityProperty, System.Windows.Visibility.Visible);
            }
            else
            {
                tbxContent.SetValue(TextBlock.VisibilityProperty, System.Windows.Visibility.Visible);
            }
            tbxContent.Text = inputDefault;
            return ShowDialog();
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            if (sender == btnCancle)
                Text = string.Empty;
            this.Close();
        }
    }
}
