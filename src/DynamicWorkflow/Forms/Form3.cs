using System;
using System.Collections.Generic;
using System.Windows.Forms;

using DynamicWorkflow.Library;

namespace DynamicWorkflow.Forms
{
    public partial class Form3 : Form
    {
        public int storeTo = 0;
        public object value = null;

        public Form3(List<Memory> mem)
        {
            InitializeComponent();

            // Fill store box
            for (int i = 0; i < mem.Count; i++)
            {
                comboBox1.Items.Add("Memory #" + (i + 1));

                if ((mem[i].full == false) && (comboBox1.SelectedIndex < 0))
                {
                    comboBox1.SelectedIndex = i;
                    storeTo = i;
                }
            }

            comboBox2.Items.Add("Boolean");
            comboBox2.Items.Add("Byte");
            comboBox2.Items.Add("SByte");
            comboBox2.Items.Add("Char");
            comboBox2.Items.Add("Decimal");
            comboBox2.Items.Add("Double");
            comboBox2.Items.Add("Single");
            comboBox2.Items.Add("Int32");
            comboBox2.Items.Add("UInt32");
            comboBox2.Items.Add("Int64");
            comboBox2.Items.Add("UInt64");
            comboBox2.Items.Add("Int16");
            comboBox2.Items.Add("UInt16");
            comboBox2.Items.Add("String");

            comboBox2.SelectedIndex = comboBox2.Items.Count - 1;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            storeTo = comboBox1.SelectedIndex;
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            CheckInput();
        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            CheckInput();
        }

        private void CheckInput()
        {
            Type type = Type.GetType("System." + comboBox2.SelectedItem.ToString());

            try
            {
                value = Convert.ChangeType(textBox1.Text, type);

                label4.Visible = false;
                button1.Enabled = true;
                button3.Enabled = true;
            }
            catch (Exception)
            {
                label4.Visible = true;
                button1.Enabled = false;
                button3.Enabled = false;
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Yes;
        }
    }
}
