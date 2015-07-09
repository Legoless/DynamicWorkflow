using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Reflection;
using System.Text;
using System.Windows.Forms;

using DynamicWorkflow.Library;

namespace DynamicWorkflow.Forms
{
    public partial class Form2 : Form
    {
        public List<Parameter> parameters = new List<Parameter>();
        public int storeTo = -1;

        private Assembly assembly;
        private MethodInfo method;
        private List<Memory> memory;

        public Form2(Assembly asm, MethodInfo meth, List<Memory> mem)
        {
            InitializeComponent();

            assembly = asm;
            method = meth;
            memory = mem;

            label6.Text = "Method: " + meth.ToString();            

            // Param combo box
            int count = 0;

            foreach (ParameterInfo param in method.GetParameters())
            {
                comboBox1.Items.Add("#" + (count + 1).ToString() + " " + param.ParameterType.ToString().Replace("System.", ""));

                ListViewItem item = new ListViewItem("#" + (count + 1).ToString());
                item.SubItems.Add(param.ParameterType.ToString().Replace("System.", ""));
                item.SubItems.Add("");

                listView1.Items.Add (item);

                count++;

                parameters.Add(new Parameter());
                parameters[parameters.Count - 1].parameterType = param.ParameterType;
            }

            if (count == 0)
            {
                comboBox1.Items.Add("None");
                comboBox1.SelectedIndex = 0;
                comboBox1.Enabled = false;

                comboBox3.Enabled = false;

                button5.Enabled = false;
                button6.Enabled = false;
            }
            else
            {
                comboBox1.SelectedIndex = 0;
            }

            // Fill store box
            for (int i = 0; i < memory.Count; i++)
            {
                // No result storing on void
                if (method.ReturnType != typeof(void))
                {
                    comboBox2.Items.Add("Memory #" + (i + 1));

                    if ((memory[i].full == false) && (comboBox2.SelectedIndex < 0))
                    {
                        comboBox2.SelectedIndex = i;
                        storeTo = i;
                    }
                }
            }

            if (method.ReturnType != typeof(void))
            {
                comboBox2.Items.Add("None");
            }
        }

        private void comboBox3_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox3.SelectedItem.ToString() == "Custom")
            {
                textBox1.Enabled = true;
            }
            else
            {
                textBox1.Enabled = false;
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            if (comboBox3.SelectedItem.ToString() == "Custom")
            {
                // Logic to convert from string to correct parameter
                parameters[comboBox1.SelectedIndex].value = textBox1.Text;
            }
            else
            {
                int memorySlot = 0;

                int counter = -1;

                for (int i = 0; i < memory.Count; i++)
                {
                    if (parameters[comboBox1.SelectedIndex].parameterType == memory[i].currentType)
                    {
                        counter++;
                    }

                    if (counter == comboBox3.SelectedIndex)
                    {
                        memorySlot = i;
                        break;
                    }
                }

                parameters[comboBox1.SelectedIndex].isMemory = memorySlot;
            }

            DisplayParameters();
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            comboBox3.Items.Clear();

            if (parameters.Count > 0)
            {
                for (int i = 0; i < memory.Count; i++)
                {
                    if (parameters[comboBox1.SelectedIndex].parameterType == memory[i].currentType)
                    {
                        comboBox3.Items.Add("Memory #" + (i + 1));
                    }
                }
            }

            comboBox3.Items.Add("Custom");

            comboBox3.SelectedIndex = comboBox3.Items.Count - 1;
        }

        private void button5_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < listView1.Items.Count; i++)
            {
                if (listView1.Items[i].Selected == true)
                {
                    parameters[i].value = null;
                    parameters[i].isMemory = -1;
                }
            }

            DisplayParameters();
        }


        private void DisplayParameters()
        {
            int count = 0;

            listView1.Items.Clear();

            foreach (ParameterInfo param in method.GetParameters())
            {
                ListViewItem item = new ListViewItem("#" + (count + 1).ToString());
                item.SubItems.Add(param.ParameterType.ToString().Replace("System.", ""));

                if (parameters[count].isMemory >= 0)
                {
                    item.SubItems.Add("Memory #" + (parameters[count].isMemory + 1));
                }
                else
                {
                    if (parameters[count].value != null)
                    {
                        item.SubItems.Add(parameters[count].value.ToString());
                    }
                }

                listView1.Items.Add(item);

                count++;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            storeTo = comboBox2.SelectedIndex;

            // No storing supported
            if (comboBox2.SelectedIndex == comboBox2.Items.Count - 1)
            {
                storeTo = -1;
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Yes;
            this.Close();
        }
    }
}
