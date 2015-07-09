using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace DynamicWorkflow.Forms
{
    public partial class Form4 : Form
    {
        public int memorySize;

        public Form4(int size)
        {
            InitializeComponent();

            memorySize = size;

            numericUpDown1.Value = size;
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            memorySize = (int)numericUpDown1.Value;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
        }
    }
}
