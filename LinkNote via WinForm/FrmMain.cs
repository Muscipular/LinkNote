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
    public partial class FrmMain : Form
    {
        private TreeViewEx<INode> tvNodes;

        public FrmMain()
        {
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.DoubleBuffer, true);
            InitializeComponent();
            InitTreeView();
        }

        private void FrmMain_Load(object sender, EventArgs e)
        {
            tvNodes.DataSource = Node.BaseNode.Children;
        }

        #region TreeView相关
        private void InitTreeView()
        {
            this.tvNodes = new Muscipular.LinkNote.TreeViewEx<INode>(x => x.Name);
            this.tvNodes.DataSource = null;
            this.tvNodes.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.tvNodes.Location = new System.Drawing.Point(0, 22);
            this.tvNodes.Name = "tvNodes";
            this.tvNodes.Size = new System.Drawing.Size(237, _splitContainer.Panel1.Height - this.tbxFilter.Height - 1);
            this.tvNodes.TabIndex = 0;
            this.tvNodes.ContextMenuStrip = this.treeMenu;
            this.tvNodes.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
            _splitContainer.Panel1.Controls.Add(tvNodes);
            tvNodes.AllowDrop = true;
            tvNodes.AfterSelect += tvNodes_AfterSelect;
            tvNodes.MouseDown += tvNodes_MouseDown;
            tvNodes.ItemDrag += tvNodes_ItemDrag;
            tvNodes.DragEnter += tvNodes_DragEnter;
            tvNodes.DragDrop += tvNodes_DragDrop;
            treeMenu.Closed += treeMenu_Closed;
            rtbxEditor.LanguageOption = RichTextBoxLanguageOptions.UIFonts;
            rtbxEditor.AcceptsTab = true;
        }

        #region TreeView选择相关
        private void tvNodes_AfterSelect(object sender, TreeViewEventArgs e)
        {
            ShowContent();
        }

        private void tvNodes_DragDrop(object sender, DragEventArgs e)
        {
            var node = e.Data.GetData(typeof(TreeNodeEx<INode>)) as TreeNodeEx<INode>;
            if (node == null)
                return;
            Node n = node.Node as Node;
            TreeNodeEx<INode> targetNode = tvNodes.GetNodeAt(tvNodes.PointToClient(new Point(e.X, e.Y))) as TreeNodeEx<INode>;
            (targetNode.Node as Node).AddChildNode(n);
        }

        private void tvNodes_DragEnter(object sender, DragEventArgs e)
        {
            var node = e.Data.GetData(typeof(TreeNodeEx<INode>)) as TreeNodeEx<INode>;
            e.Effect = DragDropEffects.None;
            if (node != null)
                e.Effect = DragDropEffects.All;
        }

        private void tvNodes_ItemDrag(object sender, ItemDragEventArgs e)
        {
            if (e.Button != System.Windows.Forms.MouseButtons.Left)
                return;
            tvNodes.DoDragDrop(e.Item, DragDropEffects.All);
        }
        #endregion

        #region TreeView菜单项
        private void RenameNode(object sender, EventArgs e)
        {
            var node = tvNodes.ClickedNode.Node as Node;
            node.Name = FrmInput.GetInput(this, "重命名", node.Name);
            node.SaveChange();
        }

        private void RemoveNode(object sender, EventArgs e)
        {
            if (tvNodes.ClickedNode == null)
                return;
            var node = tvNodes.ClickedNode.Node as Node;
            node.Parent.Remove(node);
        }

        private void AddNode(object sender, EventArgs e)
        {
            Node node = Node.BaseNode;
            if (tvNodes.ClickedNode != null)
            {
                node = tvNodes.ClickedNode.Node as Node;
            }
            //node.AddChildNode(DateTime.Now.ToString());
            var title = FrmInput.GetInput(this, "请输入标题");
            if (string.IsNullOrWhiteSpace(title))
                return;
            node.AddChildNode(title);
        }

        private void treeMenu_Closed(object sender, ToolStripDropDownClosedEventArgs e)
        {
            if (tvNodes.PreSelectedNode != null)
                tvNodes.SelectedNode = tvNodes.PreSelectedNode;
        }

        private void tvNodes_MouseDown(object sender, MouseEventArgs e)
        {
            if (tvNodes.SelectedNode != null)
            {
                tvNodes.ClickedNode = tvNodes.SelectedNode;
                treeMenu.Items[1].Visible = true;
                treeMenu.Items[2].Visible = true;
                return;
            }
            tvNodes.ClickedNode = null;
            treeMenu.Items[1].Visible = false;
            treeMenu.Items[2].Visible = false;
        }
        #endregion
        #endregion

        #region 顶部按钮
        private void rtbxEditor_LinkClicked(object sender, LinkClickedEventArgs e)
        {
            if (Control.ModifierKeys.HasFlag(Keys.Control))
            {
                System.Diagnostics.Process.Start(e.LinkText);
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            Save();
        }

        private void btnEncrypt_Click(object sender, EventArgs e)
        {
            Save(true);
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            rtbxEditor.Clear();
        }

        private void btnReload_Click(object sender, EventArgs e)
        {
            ShowContent();
        }

        private void btnExport_Click(object sender, EventArgs e)
        {
            ExportRtfFile();
        }

        private void btnSettings_Click(object sender, EventArgs e)
        {

        }
        #endregion

        private void ShowContent()
        {
            if (tvNodes.SelectedNode == null)
                return;
            Node node = tvNodes.SelectedNode.Node as Node;
            if (node == null)
                return;
            rtbxEditor.Clear();
            if (node.Content.Count == 0)
            {
                return;
            }
            var content = node.Content[0];
            if (content.IsEncryptable)
            {
                if (!DecryptContent(content))
                {
                    MessageBox.Show("密码错误");
                    return;
                }
            }
            var rtf = CommonHelper.GetString(content.Data);
            rtbxEditor.Rtf = rtf;
        }

        private bool DecryptContent(IDataContent content)
        {
            IEncryptable item = content as IEncryptable;
            if (item.IsEnctypted)
            {
                var pwd = FrmInput.GetPassword(this, "请输入密码");
                if (string.IsNullOrEmpty(pwd))
                    return false;
                if (!item.Decrypt(pwd))
                    return false;
            }
            return true;
        }

        private void Save(bool isEncrypt = false)
        {
            Node node = tvNodes.SelectedNode.Node as Node;
            EncryptContent content = new EncryptContent(node, CommonHelper.GetData(rtbxEditor.Rtf), 0, false, false);
            if (isEncrypt)
            {
                var pwd = FrmInput.GetPassword(this, "请输入密码");
                if (string.IsNullOrEmpty(pwd))
                    return;
                var pwdCheck = FrmInput.GetPassword(this, "请输入密码");
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
            SaveFileDialog f = new SaveFileDialog();
            f.DefaultExt = "rtf";
            f.Filter = "RTF文件|*.rtf";
            f.AddExtension = true;
            if (f.ShowDialog(this) != System.Windows.Forms.DialogResult.OK)
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
    }
}
