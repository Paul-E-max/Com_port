using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace COMport
{
    public partial class EnterLabel : Form
    {
        public string newLabel { get; set; }

        public EnterLabel(string CurrentLabel)
        {
            InitializeComponent();
            NewLabelTextBox.Text = CurrentLabel;
        }

        private void OKbutton_Click(object sender, EventArgs e)
        {
            newLabel = NewLabelTextBox.Text;
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
