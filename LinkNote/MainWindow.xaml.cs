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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using System.Runtime.InteropServices;
using Form = System.Windows.Forms;

namespace Muscipular.LinkNote
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        System.Windows.Forms.RichTextBox rtbxEditor;
        System.Drawing.Font MSYahei = new System.Drawing.Font("Microsoft Yahei", 9.5f);
        public MainWindow()
        {
            Settings.Current.ToString();
            InitializeComponent();
            rtbxEditor = new System.Windows.Forms.RichTextBox();
            rtbxEditor.ContextMenu = new System.Windows.Forms.ContextMenu();
            rtbxEditor.ContextMenu.MenuItems.Add("复制", new EventHandler((s, e) => { rtbxEditor.Copy(); }));
            rtbxEditor.ContextMenu.MenuItems.Add("剪切", new EventHandler((s, e) => { rtbxEditor.Cut(); }));
            rtbxEditor.ContextMenu.MenuItems.Add("粘贴", new EventHandler((s, e) => { rtbxEditor.Paste(); }));
            rtbxEditor.LanguageOption = System.Windows.Forms.RichTextBoxLanguageOptions.UIFonts;
            rtbxEditor.BorderStyle = System.Windows.Forms.BorderStyle.None;
            rtbxEditor.Font = MSYahei;
            rtbxEditor.SelectionFont = MSYahei;
            rtbxEditor.AllowDrop = true;
            rtbxEditor.EnableAutoDragDrop = true;
            windowsFormsHost1.Child = rtbxEditor;
            tvMenuTree.DataContext = Node.BaseNode;
            var tmp = new Dictionary<string, object>();
            tmp["root"] = true;
            tmp["selected"] = null;
            tvMenuTree.Tag = tmp;
            tvMenuTree.ContextMenuOpening += new ContextMenuEventHandler(tvMenuTree_ContextMenuOpening);
            tvMenuTree.ContextMenu.Opened += new RoutedEventHandler(ContextMenu_Opened);
            tvMenuTree.ContextMenu.Tag = Node.BaseNode;
            tvMenuTree.ContextMenu.CommandBindings.Add(
                new CommandBinding(ApplicationCommands.New, TreeViewContentMenuHandler, TreeViewContentMenuHandler));
            tvMenuTree.ContextMenu.CommandBindings.Add(
                new CommandBinding(ApplicationCommands.Paste, TreeViewContentMenuHandler, TreeViewContentMenuHandler));
        }

        private void richTextBox1_DragEnter(object sender, System.Windows.Forms.DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.UnicodeText))
            {
                e.Effect = System.Windows.Forms.DragDropEffects.Copy;
            }
        }

        private void tvMenuTree_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            var t = tvMenuTree.Tag as Dictionary<string, object>;
            t["root"] = !(e.OriginalSource is TextBlock);
        }

        private void ContextMenu_Opened(object sender, RoutedEventArgs e)
        {
            if (!Clipboard.GetDataObject().GetDataPresent("LinkNode_Data", false))
            {
                (sender as Control).DataContext = false;
            }
            else
            {
                (sender as Control).DataContext = true;
            }
        }
        private void TreeViewContentMenuHandler(object obj, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void TreeViewContentMenuHandler(object obj, ExecutedRoutedEventArgs e)
        {
            Control c = (Control)obj;
            Node node = c.Tag as Node;
            if (e.Parameter == null)
                return;
            switch (e.Parameter.ToString())
            {
                case "添加":
                    AddNewNode(node);
                    break;
                case "删除":
                    DeleteNode(node);
                    break;
                case "剪切":
                    break;
                case "复制":
                    break;
                case "粘贴":
                    break;
                case "重命名":
                    RenameNode(node);
                    break;
                default:
                    break;
            }
        }

        private void RenameNode(Node node)
        {
            InputTextBox f = new InputTextBox();
            f.Owner = this;
            f.ShowInTaskbar = false;
            f.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterOwner;
            if (!f.ShowDialog(Title, "输入节点标题:", node.Name))
                return;
            node.Name = f.Text;
            node.SaveChange();
        }

        private void DeleteNode(Node node)
        {
            node.Delete();
        }

        private void AddNewNode(Node node)
        {
            InputTextBox f = new InputTextBox();
            f.Owner = this;
            f.ShowInTaskbar = false;
            if (!f.ShowDialog(Title, "输入节点标题:"))
                return;
            Node n = node.AddChildNode(f.Text, null);
            var tvi = (tvMenuTree.Tag as Dictionary<string, object>)["selected"] as TreeViewItem;
            if (tvi != null)
                tvi.IsExpanded = true;
            System.Windows.Threading.DispatcherTimer a = new System.Windows.Threading.DispatcherTimer();
            a.Interval = new TimeSpan(1);
            a.Tag = n;
            a.Tick += AddNewNodeTimerTick;
            a.Start();
        }

        private void AddNewNodeTimerTick(object sender, EventArgs e)
        {
            var t = (sender as System.Windows.Threading.DispatcherTimer);
            t.Stop();
            var nd = t.Tag as Node;
            var tvi = (tvMenuTree.Tag as Dictionary<string, object>)["selected"] as TreeViewItem;
            if (!(bool)(tvMenuTree.Tag as Dictionary<string, object>)["root"])
            {
                if (tvi != null)
                    tvi = tvi.ItemContainerGenerator.ContainerFromItem(nd) as TreeViewItem;
            }
            else
            {
                tvi = tvMenuTree.ItemContainerGenerator.ContainerFromItem(nd) as TreeViewItem;
            }
            if (tvi == null)
                return;
            tvi.IsExpanded = true;
            tvi.Focus();
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            Node n = (tvMenuTree.SelectedItem as Node);
            if (n == null)
                return;
            //byte[] buffer = CommonHelper.Compress(Encoding.UTF8.GetBytes(rtbxEditor.Rtf));
            //if (n.Content.Count > 0)
            //    n.Content[0].Data = buffer;
            //else
            //    n.Content.Add(buffer);
            ////n.SaveChange();
            Save(n);
        }

        private void Expander_Checked(object sender, RoutedEventArgs e)
        {
            Control c = sender as Control;
            Node n = c.DataContext as Node;
            if (n == null)
                return;
            if (n.Children.Contains(Node.VisualNode))
                n.Children = new NodeCollection<INode>(DataBase.Current.ListNode(n));
        }

        private void TextBlock_Loaded(object sender, RoutedEventArgs e)
        {
            TextBlock c = sender as TextBlock;
            c.ContextMenu.Tag = c.DataContext;
            c.ContextMenu.Opened += ContextMenu_Opened;
            c.ContextMenu.CommandBindings.Add(
                new CommandBinding(ApplicationCommands.Delete, TreeViewContentMenuHandler, TreeViewContentMenuHandler));
            c.ContextMenu.CommandBindings.Add(
                new CommandBinding(ApplicationCommands.New, TreeViewContentMenuHandler, TreeViewContentMenuHandler));
            c.ContextMenu.CommandBindings.Add(
                new CommandBinding(ApplicationCommands.Cut, TreeViewContentMenuHandler, TreeViewContentMenuHandler));
            c.ContextMenu.CommandBindings.Add(
                new CommandBinding(ApplicationCommands.Copy, TreeViewContentMenuHandler, TreeViewContentMenuHandler));
            c.ContextMenu.CommandBindings.Add(
                new CommandBinding(ApplicationCommands.Paste, TreeViewContentMenuHandler, TreeViewContentMenuHandler));
            c.ContextMenu.CommandBindings.Add(
                new CommandBinding(ApplicationCommands.Replace, TreeViewContentMenuHandler, TreeViewContentMenuHandler));
        }

        private void TreeViewItem_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            var treeViewItem = VisualUpwardSearch<TreeViewItem>(e.OriginalSource as DependencyObject) as TreeViewItem;
            if (treeViewItem != null)
            {
                treeViewItem.Focus();
                e.Handled = true;
            }
        }

        private static T VisualUpwardSearch<T>(DependencyObject source) where T : DependencyObject
        {
            while (source != null && !(source is T))
                source = VisualTreeHelper.GetParent(source);
            return source as T;
        }

        private static T VisualDownwardSearch<T>(DependencyObject source) where T : DependencyObject
        {
            if (source != null)
            {
                int count = VisualTreeHelper.GetChildrenCount(source);
                for (int i = 0; i < count; i++)
                {
                    var tmp = VisualTreeHelper.GetChild(source, i);
                    if (tmp is T)
                    {
                        return tmp as T;
                    }
                    tmp = VisualDownwardSearch<T>(tmp);
                    if (tmp != null)
                        return tmp as T;
                }
            }
            return null;
        }

        private void tvMenuTree_Selected(object sender, RoutedEventArgs e)
        {
            (tvMenuTree.Tag as Dictionary<string, object>)["selected"] = e.OriginalSource;
            Node n = tvMenuTree.SelectedItem as Node;
            ShowContent(n);
        }

        //private void ShowNodeContent(Node node)
        //{
        //    if (node == null)
        //        return;
        //    if (node.Content.Count == 0)
        //    {
        //        richTextBox1.Clear();
        //        richTextBox1.Font = MSYahei;
        //        richTextBox1.SelectionFont = MSYahei;
        //        return;
        //    }
        //    var data = (node.Content[0].Data);
        //    if (data != null && data.Length > 3 && data[0] != 0x1f && data[1] != 0x8b && data[2] != 0x08)
        //    {
        //        var buffer = new byte[data.Length];
        //        InputTextBox itbx = new InputTextBox();
        //        itbx.Owner = this;
        //        itbx.ShowInTaskbar = false;
        //        itbx.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterOwner;
        //        if (!itbx.ShowDialog("记事本", "输入密码", isPassword: true))
        //        {
        //            richTextBox1.Clear();
        //            richTextBox1.Font = MSYahei;
        //            richTextBox1.SelectionFont = MSYahei;
        //            return;
        //        }
        //        var x = Encoding.UTF8.GetBytes(itbx.Text);
        //        if (0 != CommonHelper.Decode(data, buffer, x))
        //        {
        //            MessageBox.Show("密码错误");
        //            richTextBox1.Clear();
        //            richTextBox1.Font = MSYahei;
        //            richTextBox1.SelectionFont = MSYahei;
        //            return;
        //        }
        //        data = buffer;
        //    }
        //    richTextBox1.Rtf = Encoding.UTF8.GetString(CommonHelper.Decompress(data));
        //}

        private void ShowContent(Node node)
        {
            //= tvMenuTree.SelectedItem as Node;
            if (node == null)
                return;
            rtbxEditor.Clear();
            if (node.Content.Count == 0)
                return;

            var content = node.Content[0];
            if (content.IsEncryptable)
            {
                switch (DecryptContent(content))
                {
                    case Result.Cancel:
                        return;
                    case Result.Error:
                        MessageBox.Show("密码错误");
                        return;
                    case Result.OK:
                        break;
                }
            }
            var rtf = CommonHelper.GetString(content.Data);
            rtbxEditor.Rtf = rtf;
        }
        enum Result { OK, Cancel, Error }

        private Result DecryptContent(IDataContent content)
        {
            IEncryptable item = content as IEncryptable;
            if (item.IsEnctypted)
            {
                InputTextBox itbx = new InputTextBox();
                itbx.Owner = this;
                itbx.ShowInTaskbar = false;
                itbx.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterOwner;
                if (!itbx.ShowDialog("记事本", "输入密码", isPassword: true))
                    return Result.Cancel;
                if (string.IsNullOrEmpty(itbx.Text))
                    return Result.Cancel;
                if (!item.Decrypt(itbx.Text))
                    return Result.Error;
            }
            return Result.OK;
        }
        private void btnReload_Click(object sender, RoutedEventArgs e)
        {
            ShowContent(tvMenuTree.SelectedItem as Node);
        }

        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            rtbxEditor.Clear();
            rtbxEditor.Font = MSYahei;
            rtbxEditor.SelectionFont = MSYahei;
        }

        private void btnEncrypt_Click(object sender, RoutedEventArgs e)
        {
            Save(tvMenuTree.SelectedItem as Node, true);
        }

        private void Save(Node node, bool isEncrypt = false)
        {
            if (node == null)
                return;
            EncryptContent content = new EncryptContent(node, CommonHelper.GetData(rtbxEditor.Rtf), 0, false, false);
            if (isEncrypt)
            {
                var itbx = new InputTextBox() { Owner = this, ShowInTaskbar = false, WindowStartupLocation = System.Windows.WindowStartupLocation.CenterOwner };
                if (!itbx.ShowDialog("记事本", "输入密码", isPassword: true))
                    return;
                if (string.IsNullOrEmpty(itbx.Text))
                    return;
                var pwd = itbx.Text;
                itbx = new InputTextBox() { Owner = this, ShowInTaskbar = false, WindowStartupLocation = System.Windows.WindowStartupLocation.CenterOwner };
                if (!itbx.ShowDialog("记事本", "输入密码", isPassword: true))
                    return;
                if (string.IsNullOrEmpty(itbx.Text))
                    return;
                var pwdCheck = itbx.Text;
                if (pwd != pwdCheck)
                {
                    MessageBox.Show("密码不匹配");
                    return;
                }
                if (!content.Encrypt(pwd))
                {
                    MessageBox.Show("加密失败");
                    return;
                }
            }
            if (node.Content.Count > 0)
            {
                node.Content[0] = content;
                return;
            }
            node.Content.Insert(0, content);
        }

        private void ExportRtfFile()
        {
            if (string.IsNullOrWhiteSpace(rtbxEditor.Text))
                return;
            Form.SaveFileDialog f = new Form.SaveFileDialog();
            f.DefaultExt = "rtf";
            f.Filter = "RTF文件|*.rtf";
            f.AddExtension = true;
            if (f.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                return;
            try
            {
                rtbxEditor.SaveFile(f.FileName);
            }
            catch (Exception e)
            {
                Log.LogError(e.ToString());
            }
        }

        //private void btnEncrypt_Click(object sender, RoutedEventArgs e)
        //{
        //byte[] data = CommonHelper.Compress(Encoding.UTF8.GetBytes(richTextBox1.Rtf));
        //var buffer = new byte[data.Length + 12];
        //InputTextBox itbx = new InputTextBox();
        //itbx.Owner = this;
        //itbx.ShowInTaskbar = false;
        //itbx.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterOwner;
        //if (!itbx.ShowDialog("记事本", "输入密码", isPassword: true))
        //{
        //    //richTextBox1.Clear();
        //    //richTextBox1.Font = MSYahei;
        //    //richTextBox1.SelectionFont = MSYahei;
        //    return;
        //}
        //InputTextBox itbx2 = new InputTextBox();
        //itbx2.Owner = this;
        //itbx2.ShowInTaskbar = false;
        //itbx2.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterOwner;
        //if (!itbx2.ShowDialog("记事本", "再次输入密码", isPassword: true))
        //    return;
        //if (itbx.Text != itbx2.Text)
        //{
        //    MessageBox.Show("2次输入不匹配");
        //    return;
        //}
        //var x = Encoding.UTF8.GetBytes(itbx.Text);
        //if (0 != CommonHelper.Encode(data, data.Length, buffer, buffer.Length, x, x.Length))
        //{
        //    MessageBox.Show("密码错误");
        //    richTextBox1.Clear();
        //    richTextBox1.Font = MSYahei;
        //    richTextBox1.SelectionFont = MSYahei;
        //    return;
        //}
        //data = buffer;
        //Node n = (tvMenuTree.SelectedItem as Node);
        //if (n.Content.Count > 0)
        //    n.Content[0].Data = data;
        //else
        //    n.Content.Add(data);
        //}


        Point? _originalMousePoint = null;
        private void tvMenuTree_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _originalMousePoint = e.GetPosition(this);
        }

        private void tvMenuTree_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _originalMousePoint = null;
        }

        private void treeView_DragCheck(object sender, DragEventArgs e)
        {
            if (e.Data.GetData(typeof(Node)) == null)
            {
                e.Effects = DragDropEffects.None;
            }
            e.Handled = true;
        }

        private void tvMenuTree_Drop(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.None;
            e.Handled = true;
            ItemsControl container = VisualUpwardSearch<ItemsControl>(e.OriginalSource as UIElement);
            if (container == null)
                return;
            Node original = e.Data.GetData(typeof(Node)) as Node;
            Node target = container.DataContext as Node;
            if (original == null || target == null || target == original)
                return;
            if (target.Contains(original))
                return;
            if (original.Contains(target, true, true))
                return;
            target.AddChildNode(original);
        }

        private void tvMenuTree_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (!_originalMousePoint.HasValue || (e.GetPosition(this) - _originalMousePoint.Value).LengthSquared < 4)
                    return;
                Node n = tvMenuTree.SelectedItem as Node;
                if (n == null && n != Node.BaseNode)
                    return;
                TreeViewItem container = GetContainerFromNode(n);
                if (container == null || VisualUpwardSearch<TreeViewItem>(this.InputHitTest(_originalMousePoint.Value) as UIElement) != container)
                    return;
                DragDrop.DoDragDrop(container, n, DragDropEffects.Move);
            }
        }

        private TreeViewItem GetContainerFromNode(Node n)
        {
            Stack<Node> stack = new Stack<Node>();
            while (n != null && n != Node.BaseNode)
            {
                stack.Push(n);
                n = n.Parent;
            }
            ItemsControl container = tvMenuTree;
            while (stack.Count > 0 && container != null)
            {
                n = stack.Pop();
                container = container.ItemContainerGenerator.ContainerFromItem(n) as ItemsControl;
            }
            return container as TreeViewItem;
        }
    }
}
