// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
// @File:       EnterLabel.cs
// @Project:    DISCOVER_COM_port
// @Author:     Foster & Freeman Ltd - Michael Dodd
// @Created:    21.04.2023
//
// @Brief:      Simple COM port communications device for use with DISCOVER.
//
// @Tools:      Visual Studio 2019, C#
//
// @Revision:
// 21.04.2023-MD Initial version.
//
// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

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
