using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;

using DynamicWorkflow.Library;

namespace DynamicWorkflow.Forms
{
    public partial class Form1 : Form
    {
        // Repository
        List<Assembly> library = new List<Assembly>();

        // Memory
        List<Memory> memory = new List<Memory>();

        // Workflow files
        List<string> workflows = new List<string>();

        // Help
        Form5 help;

        public Form1()
        {
            InitializeComponent();

            // Remove all tab pages
            tabControl1.TabPages.Clear();

            // Create default tab page
            CreateTab();

            //LoadRepository(@"C:\dllbase");

            for (int i = 0; i < 17; i++)
            {
                memory.Add(new Memory());
            }

            DisplayMemory();

            WriteLog ("SYS", "Welcome to DynamicWorkflow v0.9.9 Beta Build 1401!");
            WriteLog ("SYS", "Virtual Machine successfully started.");
            WriteLog ("SYS", "Default memory space created: " + memory.Count + " blocks");

            WriteStatistics();
        }

        private void DisplayMemory()
        {
            listView1.Items.Clear();

            for (int i = 0; i < memory.Count; i++)
            {
                // TODO: Bugfix
                ListViewItem item = new ListViewItem(String.Format("{0,4}", (i + 1)));

                if (memory[i].full == true)
                {
                    item.SubItems.Add(memory[i].currentType.Name);
                    item.SubItems.Add(memory[i].value.ToString());
                }
                else
                {
                    item.SubItems.Add("");
                    item.SubItems.Add("");
                }

                listView1.Items.Add(item);
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void LoadRepository(string path)
        {
            List<string> tempList = new List<string>(Directory.GetFiles(path));

            treeView1.Nodes.Clear();

            for (int i = 0; i < tempList.Count; i++)
            {
                if (Path.GetExtension(tempList[i]) == ".dll")
                {
                    try
                    {
                        // Load DLL file
                        Assembly assembly = Assembly.LoadFile(tempList[i]);

                        TreeNode assemblyNode = new TreeNode(Path.GetFileName(assembly.Location));

                        // Get classes
                        foreach (Type type in assembly.GetTypes())
                        {
                            if (type.IsClass)
                            {
                                TreeNode classNode = new TreeNode(type.Name);

                                // Get class methods
                                foreach (MethodInfo method in type.GetMethods())
                                {
                                    classNode.Nodes.Add(method.ToString());
                                }

                                classNode.Expand();

                                assemblyNode.Nodes.Add(classNode);
                            }
                        }

                        library.Add(assembly);

                        assemblyNode.Expand();

                        treeView1.Nodes.Add(assemblyNode);
                    }
                    catch (Exception)
                    {
                    }
                }
            }
        }

        private void repositoryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                LoadRepository(folderBrowserDialog1.SelectedPath);

                WriteStatistics();
            }
        }

        private void runMethodToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            try
            {
                OpenRunMethodDialog ();
            }
            catch (Exception)
            {
                MessageBox.Show("Please select a method to run.", "Error running method");
            }
        }

        private void OpenRunMethodDialog ()
        {
            if (treeView1.SelectedNode.Level != 2)
            {
                return;
            }

            //
            // Get assembly
            //

            TreeNode classNode = treeView1.SelectedNode.Parent;

            TreeNode assemblyNode = classNode.Parent;

            int foundAsm = -1;

            for (int i = 0; i < library.Count; i++)
            {
                if (Path.GetFileName(library[i].Location) == assemblyNode.Text)
                {
                    foundAsm = i;
                    break;
                }
            }

            Assembly assembly = library[foundAsm];

            //
            // Get method info
            //

            foreach (Type type in assembly.GetTypes())
            {
                if ( (type.IsClass) && (type.Name == classNode.Text) )
                {
                    // Get class methods
                    foreach (MethodInfo method in type.GetMethods())
                    {
                        if (method.ToString() == treeView1.SelectedNode.Text)
                        {
                            Form2 form = new Form2(assembly, method, memory);

                            DialogResult dr = form.ShowDialog();

                            if (dr == DialogResult.OK)
                            {
                                try
                                {
                                    object result = ExecuteMethod(type, method, form.parameters, form.storeTo);

                                    DisplayMemory();

                                    if (debugToolStripMenuItem.Checked == false)
                                    {
                                        WriteLog("RUN", "Method result: " + result.ToString());
                                    }
                                }
                                catch (Exception ex)
                                {
                                    WriteLog("ERR", "Exception: Invalid parameters. " + ex.Message);
                                }
                            }
                            else if (dr == DialogResult.Yes)
                            {
                                ((TextBox)tabControl1.SelectedTab.Controls[0]).Text = ((TextBox)tabControl1.SelectedTab.Controls[0]).Text.Insert(((TextBox)tabControl1.SelectedTab.Controls[0]).SelectionStart, WriteCommand(type, method, form.parameters, form.storeTo));
                            }

                            return;
                        }
                    }

                    break;
                }
            }
        }

        private void WriteStatistics()
        {
            listView3.Items.Clear();

            //
            // Number of opened assemblies
            //

            ListViewItem item;

            item = new ListViewItem("Memory size");
            item.SubItems.Add(memory.Count.ToString());

            listView3.Items.Add(item);
            
            item = new ListViewItem("Assemblies");
            item.SubItems.Add(library.Count.ToString());

            listView3.Items.Add(item);

            int classCount = 0;
            int methodCount = 0;

            foreach (Assembly assembly in library)
            {
                foreach (Type t in assembly.GetTypes())
                {
                    if (t.IsClass)
                    {
                        classCount++;

                        methodCount += (t.GetMethods().Length);
                    }
                }
            }

            item = new ListViewItem("Classes");
            item.SubItems.Add(classCount.ToString());

            listView3.Items.Add(item);

            item = new ListViewItem("Methods");
            item.SubItems.Add(methodCount.ToString());

            listView3.Items.Add(item);

            item = new ListViewItem("Workflows");
            item.SubItems.Add(tabControl1.TabPages.Count.ToString());

            listView3.Items.Add(item);

        }

        private void WriteLog(string id, string message)
        {
            textBox2.Text += "[" + id + "]: " + message + "\r\n";
            textBox2.SelectionStart = textBox2.TextLength;
            textBox2.ScrollToCaret();
        }

        private string WriteCommand(Type type, MethodInfo method, List<Parameter> parameters, int memoryLocation)
        {
            string cmd = "";

            if (memoryLocation >= 0)
            {
                cmd = "Memory (" + (memoryLocation + 1) + ") = ";
            }
            
            cmd += type.FullName + "." + method.Name + " (" + ConstructParameters(parameters) + ")";

            return cmd + ";\r\n";
        }

        private string ConstructParameters(List<Parameter> parameters)
        {
            string param = " ";

            for (int i = 0; i < parameters.Count; i++)
            {
                if (parameters[i].isMemory >= 0)
                {
                    param = param + "Memory (" + (parameters[i].isMemory + 1) + "), ";
                }
                else
                {
                    param = param + parameters[i].value + ", ";
                }
            }

            param = param.Trim();

            if (param.Length > 0)
            {
                // Cut away last comma
                param = param.Substring (0, param.Length - 1);
            }

            return param;
        }

        private void ParseWorkflow (string workflow)
        {
            workflow = workflow.Trim();

            //
            // Nothing to do with short workflows
            //

            if (workflow.Length == 0)
            {
                return;
            }

            //
            // Creating ExecuteMethod functions
            //

            List<string> commands = new List<string>();

            int start = 0;
            int semicolon = -1;

            int lineCount = 1;

            // Find first ; not in quotes

            bool inQuote = false;

            //
            // Parse whole workflow in commands
            //

            for (int i = 0; i < workflow.Length; i++)
            {
                if (semicolon != -1)
                {
                    semicolon = -1;
                }

                // We do not care what is in quotes
                if (inQuote == true)
                {
                    if (workflow[i] == '"')
                    {
                        inQuote = false;
                    }

                    if (workflow[i] == '\\')
                    {
                        i++;
                    }
                }
                else
                {
                    if (workflow[i] == '\n')
                    {
                        lineCount++;

                        continue;
                    }

                    if (workflow[i] == '"')
                    {
                        inQuote = true;
                    }

                    if (workflow[i] == ';')
                    {
                        // Cut away command
                        commands.Add(workflow.Substring (start, i - start));
                        semicolon = i;
                        start = i + 1;
                    }
                }
            }

            if (semicolon == -1)
            {
                Exception ex = new Exception("Parse error: Expecting semicolon - line: " + lineCount);

                throw ex;
            }
            else
            {
                //
                // We have commands, now parse each command, one by one
                //

                for (int i = 0; i < commands.Count; i++)
                {
                    // Trim just in case
                    commands[i] = RemoveSpaces (commands[i]);

                    int memoryLocation = -1;

                    // Check if we have to store it in memory, we expect Int32
                    if (commands[i].StartsWith("Memory(") == true)
                    {
                        try
                        {
                            string loc = commands[i].Substring(commands[i].IndexOf("(") + 1, commands[i].IndexOf(")=") - commands[i].IndexOf("(") - 1);

                            memoryLocation = Convert.ToInt32(loc) - 1;

                            if (memoryLocation < 0)
                            {
                                throw new Exception("Non-positive memory location");
                            }
                            else if (memoryLocation >= memory.Count)
                            {
                                throw new Exception("Memory overflow.");
                            }

                            commands[i] = commands[i].Substring(commands[i].IndexOf(")=") + 2);
                        }
                        catch (Exception)
                        {
                            Exception ex = new Exception("Parse error: Cannot convert memory location: Command: " + (i + 1));

                            throw ex;
                        }
                    }
                    // Check for embedded commands
                    else if (commands[i].StartsWith("Store") == true)
                    {
                        // Get parameters
                        string parameters = commands[i].Substring(commands[i].IndexOf("(") + 1);
                        parameters = parameters.Substring(0, parameters.LastIndexOf(")"));
                        parameters = parameters.Trim();

                        string[] paramList = parameters.Split(',');

                        if (paramList.Length != 3)
                        {
                            throw new Exception("Store function takes 3 parameters: " + paramList.Length + " parameters given.");
                        }

                        //
                        // Memory checks
                        //
                        memoryLocation = Convert.ToInt32(paramList[0].Trim()) - 1;

                        if (memoryLocation < 0)
                        {
                            throw new Exception("Non-positive memory location");
                        }
                        else if (memoryLocation >= memory.Count)
                        {
                            throw new Exception("Memory overflow.");
                        }

                        //
                        // Type checks
                        //

                        try
                        {
                            if (paramList[1].Trim().ToLower() == "object")
                            {
                                throw new Exception("System.object not supported.");
                            }

                            Type type = Type.GetType("System." + paramList[1].Trim());

                            object value = Convert.ChangeType(paramList[2], type);

                            // Remove quotes with string
                            if (value.GetType().Name == "String")
                            {
                                if ( ((string)value).StartsWith("\"") && ((string)value).EndsWith("\"") )
                                {
                                    value = ((string)value).Substring(1, ((string)value).Length - 2);
                                }
                                else
                                {
                                    throw new Exception("Parse error: Expecting \" in String parameter.");
                                }
                            }

                            //
                            // Store in memory
                            //

                            memory[memoryLocation].full = true;
                            memory[memoryLocation].currentType = type;
                            memory[memoryLocation].value = value;
                        }
                        catch (Exception ex)
                        {
                            throw new Exception(ex.InnerException.Message);
                        }

                        continue;
                    }
                    
                    //
                    // We execute the rest of command
                    //

                    object result = ExecuteMethod (GetCommandType (commands[i]), GetCommandMethod (commands[i]), GetCommandParameters(commands[i]), memoryLocation);
                }
            }
        }

        private List<Parameter> GetCommandParameters (string command)
        {
            // Just get parameters
            string parameters = command.Substring(command.IndexOf("(") + 1);
            parameters = parameters.Substring(0, parameters.LastIndexOf(")"));
            parameters = parameters.Trim();

            string[] paramList = parameters.Split(',');

            if (parameters.Length == 0)
            {
                paramList = new string[0];
            }

            MethodInfo method = GetCommandMethod(command);
            ParameterInfo[] methodParams = method.GetParameters();

            if (paramList.Length != methodParams.Length)
            {
                throw new Exception ("Parse error: Method \"" + method.Name + "\" does not accept " + paramList.Length + " parameters.");
            }

            List<Parameter> returnList = new List<Parameter>();

            for (int i = 0; i < paramList.Length; i++)
            {
                Parameter param = new Parameter();

                param.parameterType = methodParams[i].ParameterType;

                // Check for memory parameters
                if (paramList[i].StartsWith("Memory(") == true)
                {
                    try
                    {
                        string loc = paramList[i].Substring(paramList[i].IndexOf("(") + 1, paramList[i].IndexOf(")") - paramList[i].IndexOf("(") - 1);

                        param.isMemory = Convert.ToInt32(loc) - 1;

                        if (param.isMemory < 0)
                        {
                            throw new Exception("Non-positive memory location.");
                        }
                        else if (param.isMemory >= memory.Count)
                        {
                            throw new Exception("Memory overflow.");
                        }
                    }
                    catch (Exception)
                    {
                        Exception ex = new Exception("Parse error: Cannot convert method memory location parameter: \"" + paramList[i] + "\"");

                        throw ex;
                    }
                }
                else
                {
                    try
                    {
                        param.value = Convert.ChangeType(paramList[i], param.parameterType);

                        // String support, cut away double quotes
                        if (param.value.GetType() == typeof(string))
                        {
                            // BugFix - if it is string and no quotes = error

                            if ((((string)param.value).EndsWith("\"") == true) && (((string)param.value).StartsWith("\"") == true))
                            {
                                string rem = (string)param.value;

                                rem = rem.Substring(1);
                                rem = rem.Substring(0, rem.Length - 1);

                                param.value = rem;
                            }
                            else
                            {
                                if (((string)param.value).StartsWith("\"") == false)
                                {
                                    throw new Exception("Parse error: Expecting double quote on start of param: " + ((string)param.value)[0]);
                                }
                                else
                                {
                                    throw new Exception("Parse error: Expecting double quote on end of param: " + ((string)param.value)[((string)param.value).Length - 1]);
                                }
                            }
                        }

                        
                    }
                    catch (Exception)
                    {
                        Exception ex = new Exception("Parse error: Cannot convert method parameter: \"" + paramList[i] + "\" to: " + param.parameterType.ToString());

                        throw ex;
                    }
                }

                returnList.Add(param);
            }

            return returnList;
        }

        private MethodInfo GetCommandMethod(string command)
        {
            // Get just the method
            string method = command.Substring(0, command.IndexOf("("));
            method = method.Substring(method.LastIndexOf(".") + 1);

            Type className = GetCommandType(command);

            foreach (MethodInfo meth in className.GetMethods())
            {
                if (meth.Name == method)
                {
                    return meth;
                }
            }

            throw new Exception("Parse error: Method \"" + method + "\" not found.");
        }

        private Type GetCommandType (string command)
        {
            string className = command.Substring(0, command.IndexOf("("));
            className = className.Substring(0, className.LastIndexOf("."));

            for (int i = 0; i < library.Count; i++)
            {
                foreach (Type t in library[i].GetTypes())
                {
                    if (className == t.FullName)
                    {
                        return t;
                    }
                }
            }

            throw new Exception("Parse error: Class \"" + className + "\" not found.");
        }

        private string RemoveSpaces(string command)
        {
            bool inQuote = false;

            string newCmd = "";

            for (int i = 0; i < command.Length; i++)
            {
                // We do not care what is in quotes
                if (inQuote == true)
                {
                    if (command[i] == '"')
                    {
                        inQuote = false;
                    }

                    if (command[i] == '\\')
                    {
                        i++;
                    }
                }
                else
                {
                    if (command[i] == '"')
                    {
                        inQuote = true;
                    }
                }

                if ( (inQuote == true) || ( (command[i] != ' ') && (command[i] != '\n') ) )
                {
                    newCmd += command[i].ToString();
                }
            }

            return newCmd.Trim();
        }

        private object ExecuteMethod(Type type, MethodInfo method, List<Parameter> parameters, int memoryLocation)
        {
            //
            // Create parameter list as objects
            //

            List<object> paramList = new List<object>();

            foreach (Parameter param in parameters)
            {
                object obj = new object();

                if (param.isMemory >= 0)
                {
                    obj = Convert.ChangeType(memory[param.isMemory].value, param.parameterType);
                }
                else
                {
                    obj = Convert.ChangeType(param.value, param.parameterType);
                }
                

                paramList.Add(obj);
            }

            //
            // Create class instance
            //

            object instance = Activator.CreateInstance(type);
            object result = method.Invoke(instance, paramList.ToArray());
            result = Convert.ChangeType(result, method.ReturnType);

            if (debugToolStripMenuItem.Checked == true)
            {
                WriteLog("RUN", "Method result: " + result.ToString());
            }

            if (memoryLocation > -1)
            {
                memory[memoryLocation].full = true;
                memory[memoryLocation].currentType = method.ReturnType;
                memory[memoryLocation].value = result;
            }

            return result;
        }

        private void treeView1_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            OpenRunMethodDialog();
        }

        private void runMethodToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                ParseWorkflow(((TextBox)tabControl1.SelectedTab.Controls[0]).Text);

                DisplayMemory();
            }
            catch (Exception ex)
            {
                WriteLog("ERR", "Exception: " + ex.Message);
            }
        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            listView2.Items.Clear();

            if (treeView1.SelectedNode == null)
            {
                return;
            }

            ListViewItem item;

            // Methods
            if (treeView1.SelectedNode.Level == 2)
            {
                //
                // Get assembly
                //

                TreeNode classNode = treeView1.SelectedNode.Parent;

                TreeNode assemblyNode = classNode.Parent;

                int foundAsm = -1;

                for (int i = 0; i < library.Count; i++)
                {
                    if (Path.GetFileName(library[i].Location) == assemblyNode.Text)
                    {
                        foundAsm = i;
                        break;
                    }
                }

                Assembly assembly = library[foundAsm];

                item = new ListViewItem("Assembly");
                item.SubItems.Add(assembly.FullName);

                listView2.Items.Add(item);

                //
                // Get method info
                //

                foreach (Type type in assembly.GetTypes())
                {
                    if ((type.IsClass) && (type.Name == classNode.Text))
                    {
                        // Get class methods
                        foreach (MethodInfo method in type.GetMethods())
                        {
                            if (method.ToString() == treeView1.SelectedNode.Text)
                            {
                                item = new ListViewItem("Namespace");
                                item.SubItems.Add(type.Namespace);

                                listView2.Items.Add(item);

                                item = new ListViewItem("Class");
                                item.SubItems.Add(type.Name);

                                listView2.Items.Add(item);

                                item = new ListViewItem("Name");
                                item.SubItems.Add(method.Name);

                                listView2.Items.Add(item);

                                item = new ListViewItem("Param count");
                                item.SubItems.Add(method.GetParameters().Length.ToString());

                                listView2.Items.Add(item);

                                item = new ListViewItem("Return type");
                                item.SubItems.Add(method.ReturnType.ToString());

                                listView2.Items.Add(item);

                                return;
                            }
                        }

                        break;
                    }
                }
            }
            // Classes
            else if (treeView1.SelectedNode.Level == 1)
            {
                TreeNode assemblyNode = treeView1.SelectedNode.Parent;

                int foundAsm = -1;

                for (int i = 0; i < library.Count; i++)
                {
                    if (Path.GetFileName(library[i].Location) == assemblyNode.Text)
                    {
                        foundAsm = i;
                        break;
                    }
                }

                Assembly assembly = library[foundAsm];

                item = new ListViewItem("Assembly");
                item.SubItems.Add(assembly.FullName);

                listView2.Items.Add(item);

                foreach (Type type in assembly.GetTypes())
                {
                    if ((type.IsClass) && (type.Name == treeView1.SelectedNode.Text))
                    {
                        item = new ListViewItem("Namespace");
                        item.SubItems.Add(type.Namespace);

                        listView2.Items.Add(item);

                        item = new ListViewItem("Name");
                        item.SubItems.Add(type.Name);

                        listView2.Items.Add(item);

                        item = new ListViewItem("Member count");
                        item.SubItems.Add(type.GetMembers().Length.ToString());

                        listView2.Items.Add(item);

                        item = new ListViewItem("Method count");
                        item.SubItems.Add(type.GetMethods().Length.ToString());

                        listView2.Items.Add(item);

                        item = new ListViewItem("Field count");
                        item.SubItems.Add(type.GetFields().Length.ToString());

                        listView2.Items.Add(item);
                    }
                }
            }
            // Assembly itself
            else
            {
                TreeNode assemblyNode = treeView1.SelectedNode;

                int foundAsm = -1;

                for (int i = 0; i < library.Count; i++)
                {
                    if (Path.GetFileName(library[i].Location) == assemblyNode.Text)
                    {
                        foundAsm = i;
                        break;
                    }
                }

                Assembly assembly = library[foundAsm];

                item = new ListViewItem("Assembly");
                item.SubItems.Add(assembly.FullName);

                listView2.Items.Add(item);

                item = new ListViewItem("Class count");
                item.SubItems.Add(assembly.GetTypes().Length.ToString());

                listView2.Items.Add(item);

                item = new ListViewItem("Location");
                item.SubItems.Add(Path.GetDirectoryName(assembly.Location));

                listView2.Items.Add(item);

                item = new ListViewItem("Filename");
                item.SubItems.Add(Path.GetFileName(assembly.Location));

                listView2.Items.Add(item);

                item = new ListViewItem("Module count");
                item.SubItems.Add(assembly.GetModules().Length.ToString());

                listView2.Items.Add(item);

                item = new ListViewItem("File count");
                item.SubItems.Add(assembly.GetFiles().Length.ToString());

                listView2.Items.Add(item);
            }
        }

        private void textBox_DragDrop(object sender, DragEventArgs e)
        {
            // Handle method drag and drop
            if (e.Data.GetDataPresent("System.Windows.Forms.TreeNode", false))
            {
                TreeNode transferNode = (TreeNode)e.Data.GetData("System.Windows.Forms.TreeNode");

                // Allow only methods to be transfered
                if (transferNode.Level != 2)
                {
                    return;
                }

                // Have to find correct method form now

                TreeNode classNode = transferNode.Parent;

                TreeNode assemblyNode = classNode.Parent;

                int foundAsm = -1;

                for (int i = 0; i < library.Count; i++)
                {
                    if (Path.GetFileName(library[i].Location) == assemblyNode.Text)
                    {
                        foundAsm = i;
                        break;
                    }
                }

                Assembly assembly = library[foundAsm];

                //
                // Get method info
                //

                foreach (Type type in assembly.GetTypes())
                {
                    if ((type.IsClass) && (type.Name == classNode.Text))
                    {
                        // Get class methods
                        foreach (MethodInfo method in type.GetMethods())
                        {
                            if (method.ToString() == transferNode.Text)
                            {
                                List<Parameter> list = new List<Parameter>();

                                for (int i = 0; i < method.GetParameters().Length; i++)
                                {
                                    Parameter param = new Parameter();
                                    param.parameterType = method.GetParameters()[i].ParameterType;

                                    /*
                                    if (param.parameterType == typeof(string))
                                    {
                                        param.value = "\"\"";
                                    }
                                    else
                                    {
                                        param.value = 0;
                                    }*/

                                    param.value = " ";

                                    list.Add(param);
                                }

                                int save = ((TextBox)tabControl1.SelectedTab.Controls[0]).SelectionStart;

                                string cmd = WriteCommand(type, method, list, -1);

                                ((TextBox)tabControl1.SelectedTab.Controls[0]).Text = ((TextBox)tabControl1.SelectedTab.Controls[0]).Text.Insert(((TextBox)tabControl1.SelectedTab.Controls[0]).SelectionStart, cmd);

                                if (list.Count > 0)
                                {
                                    ((TextBox)tabControl1.SelectedTab.Controls[0]).SelectionStart = ((TextBox)tabControl1.SelectedTab.Controls[0]).Text.IndexOf("(", save) + 1;

                                    int save2 = ((TextBox)tabControl1.SelectedTab.Controls[0]).SelectionStart;

                                    ((TextBox)tabControl1.SelectedTab.Controls[0]).Text = ((TextBox)tabControl1.SelectedTab.Controls[0]).Text.Insert(((TextBox)tabControl1.SelectedTab.Controls[0]).SelectionStart, " ");

                                    ((TextBox)tabControl1.SelectedTab.Controls[0]).SelectionStart = save2;
                                }
                                else
                                {
                                    ((TextBox)tabControl1.SelectedTab.Controls[0]).SelectionStart = save + cmd.Length;
                                }

                                return;
                            }
                        }

                        break;
                    }
                }
            }
            else if (e.Data.GetDataPresent("System.Windows.Forms.ListViewItem", false))
            {
                ListViewItem transferItem = (ListViewItem)e.Data.GetData("System.Windows.Forms.ListViewItem");

                int save = ((TextBox)tabControl1.SelectedTab.Controls[0]).SelectionStart;

                string txt = "Memory (" + Convert.ToInt32(transferItem.Text) + ")";

                ((TextBox)tabControl1.SelectedTab.Controls[0]).Text = ((TextBox)tabControl1.SelectedTab.Controls[0]).Text.Insert(((TextBox)tabControl1.SelectedTab.Controls[0]).SelectionStart, txt);

                ((TextBox)tabControl1.SelectedTab.Controls[0]).SelectionStart = save + txt.Length;
            }
        }

        private void treeView1_ItemDrag(object sender, ItemDragEventArgs e)
        {
            DoDragDrop(e.Item, DragDropEffects.Copy);
        }

        private void treeView1_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.Copy;
        }

        private void textBox_DragEnter(object sender, DragEventArgs e)
        {
            ((TextBox)tabControl1.SelectedTab.Controls[0]).Focus();

            e.Effect = DragDropEffects.Copy;
        }

        private void textBox_MouseHover(object sender, EventArgs e)
        {
            ((TextBox)tabControl1.SelectedTab.Controls[0]).Focus();
        }

        private void textBox_DragOver(object sender, DragEventArgs e)
        {
            Point location = ((TextBox)tabControl1.SelectedTab.Controls[0]).PointToScreen(Point.Empty);

            int loc = ((TextBox)tabControl1.SelectedTab.Controls[0]).GetCharIndexFromPosition(new Point(e.X - location.X, e.Y - location.Y));
            ((TextBox)tabControl1.SelectedTab.Controls[0]).SelectionStart = loc;
            ((TextBox)tabControl1.SelectedTab.Controls[0]).SelectionLength = 0;

            ((TextBox)tabControl1.SelectedTab.Controls[0]).Refresh();
        }

        private void listView1_ItemDrag(object sender, ItemDragEventArgs e)
        {
            DoDragDrop(e.Item, DragDropEffects.Copy);
        }

        private void listView1_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.Copy;
        }

        private void newWorkflowToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CreateTab ();
        }

        private void CreateTab()
        {
            CreateTab("", "");
        }
        private void CreateTab(string tabName, string tabData)
        {
            if (tabName != "")
            {
                workflows.Add(tabName);

                tabName = Path.GetFileNameWithoutExtension(tabName);
            }
            else
            {
                workflows.Add("");

                // Get name
                int number = 0;

                for (int i = 0; i < tabControl1.TabPages.Count; i++)
                {
                    // Get number
                    if (tabControl1.TabPages[i].Text.StartsWith("Workflow ") == true)
                    {
                        string txt = tabControl1.TabPages[i].Text.Replace("Workflow ", "");
                        txt = txt.Replace("*", "");
                        txt = txt.Trim();

                        try
                        {
                            number = Convert.ToInt32(txt);
                        }
                        catch (Exception)
                        {
                        }
                    }
                }

                tabName = "Workflow " + (number + 1) + " *";
            }

            //
            // Create tab
            //

            TabPage newTab = new TabPage();
            newTab.Location = new System.Drawing.Point(4, 22);
            newTab.Padding = new System.Windows.Forms.Padding(3);
            newTab.Size = new System.Drawing.Size(459, 328);
            newTab.TabIndex = 0;
            newTab.Text = tabName;
            newTab.UseVisualStyleBackColor = true;

            TextBox textBox = new TextBox();
            textBox.AllowDrop = true;
            textBox.Dock = System.Windows.Forms.DockStyle.Fill;
            textBox.Font = new System.Drawing.Font("Lucida Console", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            textBox.Location = new System.Drawing.Point(3, 3);
            textBox.Multiline = true;
            textBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            textBox.Size = new System.Drawing.Size(453, 322);
            textBox.TabIndex = 0;
            textBox.Text = tabData;
            textBox.SelectionLength = 0;
            textBox.SelectionStart = textBox.TextLength;
            textBox.ScrollToCaret();
            textBox.DragDrop += new System.Windows.Forms.DragEventHandler(this.textBox_DragDrop);
            textBox.DragEnter += new System.Windows.Forms.DragEventHandler(this.textBox_DragEnter);
            textBox.DragOver += new System.Windows.Forms.DragEventHandler(this.textBox_DragOver);
            textBox.MouseHover += new System.EventHandler(this.textBox_MouseHover);
            textBox.TextChanged += new System.EventHandler(this.textBox_TextChanged);

            newTab.Controls.Add(textBox);

            tabControl1.TabPages.Add(newTab);

            tabControl1.SelectedIndex = tabControl1.TabPages.Count - 1;

            WriteStatistics();
        }

        private void textBox_TextChanged(object sender, EventArgs e)
        {
            // Add a star to the changes
            if (tabControl1.SelectedTab.Text.EndsWith(" *") == false)
            {
                tabControl1.SelectedTab.Text += " *";
            }
        }

        private void workflowToolStripMenuItem_Click(object sender, EventArgs e)
        {
            openFileDialog1.Title = "Open Workflow ...";
            openFileDialog1.FileName = "";
            openFileDialog1.Filter = "DynamicWorkflow Workflow (*.lfw)|*.lfw";
            openFileDialog1.Multiselect = true;

            //
            // Open files
            //
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                for (int i = 0; i < openFileDialog1.FileNames.Length; i++)
                {
                    // Open file text
                    try
                    {
                        TextReader tr = new StreamReader(openFileDialog1.FileNames[i]);

                        CreateTab(openFileDialog1.FileNames[i], tr.ReadToEnd());

                        tr.Close();
                    }
                    catch (Exception ex)
                    {
                        WriteLog("ERR", "I/O Error: " + ex.Message);
                    }
                }
            }
        }

        private void workflowToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            saveFileDialog1.FileName = tabControl1.SelectedTab.Text.Replace("*","").Trim() + ".lfw";
            saveFileDialog1.Filter = "DynamicWorkflow Workflow (*.lfw)|*.lfw";
            saveFileDialog1.Title = "Save Workflow ...";

            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    TextWriter tw = new StreamWriter(saveFileDialog1.FileName);

                    tw.WriteLine(((TextBox)tabControl1.SelectedTab.Controls[0]).Text);

                    tw.Close();

                    workflows[tabControl1.SelectedIndex] = saveFileDialog1.FileName;

                    tabControl1.SelectedTab.Text = Path.GetFileNameWithoutExtension(saveFileDialog1.FileName);
                }
                catch (Exception)
                {
                    MessageBox.Show("Error saving to file: " + saveFileDialog1.FileName, "Error");
                }
            }
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Assume its for workflow

            // Check if we have the file already saved, if we dont, we display dialog -> call save as function
            if (workflows[tabControl1.SelectedIndex] == "")
            {
                workflowToolStripMenuItem1_Click(new object(), new EventArgs());
            }
            else
            {
                TextWriter tw = new StreamWriter(workflows[tabControl1.SelectedIndex]);

                tw.WriteLine(((TextBox)tabControl1.SelectedTab.Controls[0]).Text);

                tw.Close();

                tabControl1.SelectedTab.Text = tabControl1.SelectedTab.Text.Replace(" *", "");
            }
        }

        private void memoryDataToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            saveFileDialog1.FileName = "MemDump " + tabControl1.SelectedTab.Text.Replace("*", "").Trim() + ".lfm";
            saveFileDialog1.Filter = "DynamicWorkflow Memory (*.lfm)|*.lfm";
            saveFileDialog1.Title = "Save Memory Dump ...";

            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    using (Stream stream = File.Open (saveFileDialog1.FileName, FileMode.Create))
                    {
                        BinaryFormatter bin = new BinaryFormatter();
                        bin.Serialize(stream, memory);
                    }

                    WriteLog("SYS", "Saved memory dump to file: " + saveFileDialog1.FileName);
                }
                catch (Exception)
                {
                    MessageBox.Show("Error saving memory dump to file: " + saveFileDialog1.FileName, "Save Error");
                }
            }
        }

        private void memoryDataToolStripMenuItem_Click(object sender, EventArgs e)
        {
            openFileDialog1.FileName = "";
            openFileDialog1.Filter = "DynamicWorkflow Memory (*.lfm)|*.lfm";
            openFileDialog1.Title = "Open Memory Dump ...";
            openFileDialog1.Multiselect = false;

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    using (Stream stream = File.Open(openFileDialog1.FileName, FileMode.Open))
                    {
                        BinaryFormatter bin = new BinaryFormatter();

                        memory = (List<Memory>)bin.Deserialize(stream);
                    }

                    DisplayMemory ();

                    WriteStatistics ();

                    WriteLog("SYS", "Opened memory dump from file: " + openFileDialog1.FileName + " Memory size: " + memory.Count);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error opening memory dump file: " + openFileDialog1.FileName + "\r\n\r\n" + ex.Message, "Opening file error");
                }
            }
        }

        private void newTabToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CreateTab();
        }

        private void CloseTab()
        {
            int tabIndex = tabControl1.SelectedIndex;

            tabControl1.TabPages.RemoveAt(tabIndex);
            workflows.RemoveAt(tabIndex);

            // If we have only one, we will add one
            if (tabControl1.TabPages.Count == 0)
            {
                CreateTab();
            }
            // Otherwise select first tab by default
            else
            {
                tabControl1.SelectedIndex = 0;
            }

            WriteStatistics();
        }

        private void closeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CloseTab();
        }

        private void closeTabToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CloseTab();
        }

        private void cleanMemoryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < memory.Count; i++)
            {
                memory[i] = new Memory();
            }

            DisplayMemory();

            WriteLog("SYS", "Cleared all memory space.");
        }

        private void storeVarInMemoryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form3 form = new Form3(memory);

            DialogResult dr = form.ShowDialog();

            if (dr == DialogResult.OK)
            {
                memory[form.storeTo].full = true;
                memory[form.storeTo].currentType = form.value.GetType();
                memory[form.storeTo].value = form.value;

                WriteLog("SYS", "Saved variable to: " + (form.storeTo + 1) + " Type: " + form.value.GetType().Name + " Value: " + form.value.ToString());

                DisplayMemory();
            }
            else if (dr == DialogResult.Yes)
            {
                if (form.value.GetType().Name == "String")
                {
                    form.value = "\"" + ((string)form.value) + "\"";
                }

                ((TextBox)tabControl1.SelectedTab.Controls[0]).Text = ((TextBox)tabControl1.SelectedTab.Controls[0]).Text.Insert(((TextBox)tabControl1.SelectedTab.Controls[0]).SelectionStart, "Store (" + (form.storeTo + 1) + ", " + form.value.GetType().Name + ", " + form.value.ToString() + ");");
            }
        }

        private void settingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form4 form = new Form4(memory.Count);

            if (form.ShowDialog() == DialogResult.OK)
            {
                // If its smaller, we will cut out memory cells from last address up
                if (form.memorySize < memory.Count)
                {
                    memory.RemoveRange(form.memorySize, memory.Count - form.memorySize);
                }
                // Otherwise we add new cells
                {
                    int count = memory.Count;

                    for (int i = 0; i < (form.memorySize - count); i++)
                    {
                        memory.Add(new Memory());
                    }
                }

                DisplayMemory();

                WriteStatistics();
            }
        }

        private void aboutLegoFrameworToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form6 form = new Form6();

            form.ShowDialog();
        }

        private void contentsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (help == null || help.IsDisposed == true)
            {
                help = new Form5();
            }

            help.Location = new Point(this.Left + (this.Width / 2 - help.Width / 2), this.Top + (this.Height / 2 - help.Height / 2));

            if (help.Visible == false)
            {
                help.Show(this);
            }
        }

        private void debugToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (debugToolStripMenuItem.Checked == true)
            {
                debugToolStripMenuItem.Checked = false;
            }
            else
            {
                debugToolStripMenuItem.Checked = true;
            }
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            if (MessageBox.Show(this, "This program was created for educational purposes and is distributed\nin the hope that it will be useful, but WITHOUT ANY WARRANTY.\nThe author(s) take no responsibility for the damage caused by this\nprogram or it's parts and libraries.\n\nDo you agree with these terms?", "Responsibility notice", MessageBoxButtons.YesNo) == DialogResult.No)
            {
                Application.Exit();
            }
        }
    }
}
