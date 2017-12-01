using System;
using System.Windows.Forms;

namespace EINK_DEBUG.ActionForms
{
    public partial class Action1Echo : Form
    {
        private MainForm Parent;
        public Action1Echo(MainForm parent)
        {
            InitializeComponent();
            Parent = parent;
        }

        private void btnGo_Click(object sender, EventArgs e)
        {
            Parent.Action1EchoCall(txtbEchoData.Text);
            
        }
    }
}
