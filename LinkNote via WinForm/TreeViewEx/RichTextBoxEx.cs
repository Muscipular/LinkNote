using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Muscipular.LinkNote
{
    public class RichTextBoxEx : RichTextBox
    {
       // public RichTextBoxEx()
       // {
       //     this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint, true);
       //     this.Refresh();
       // }
       // protected override void OnPaint(PaintEventArgs e)
       // {
       //     base.OnPaint(e);
       //      var g = this.CreateGraphics();
       //     ControlPaint.DrawBorder(g, this.ClientRectangle, Color.FromArgb(0x33, 0x66, 0x44), ButtonBorderStyle.Solid);
       //     g.Dispose();
       //}
       // protected override void OnPaintBackground(PaintEventArgs pevent)
       // {
       //     base.OnPaintBackground(pevent);
       //     var g = this.CreateGraphics();
       //     ControlPaint.DrawBorder(g, this.ClientRectangle, Color.FromArgb(0x33, 0x66, 0x44), ButtonBorderStyle.Solid);
       //     g.Dispose();
       //     //var g = this.CreateGraphics();
       //     //ControlPaint.DrawBorder(g, new Rectangle(1, 1, this.Width - 1, this.Height - 1), Color.FromArgb(0xff, 0x33, 0x66, 0x44), ButtonBorderStyle.Solid);
       //     ////this.Refresh();
       //     //g.Dispose();
       // }
        public new void Clear()
        {
            base.Clear();
            base.ClearUndo();
        }

        public void Clear(bool clearUndo)
        {
            base.Clear();
            if (clearUndo)
                base.ClearUndo();
        }
    }
}
