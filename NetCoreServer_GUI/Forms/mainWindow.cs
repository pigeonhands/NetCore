using NetCore;
using NetCore.Networking;
using NetCoreServer_GUI.Controls;
using NetCoreServer_GUI.Funcions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NetCoreServer_GUI.Forms
{
    public partial class mainWindow : Form
    {
        Dictionary<string, RemoteFunction> LoadedFunctions = new Dictionary<string, RemoteFunction>();
        eSock.Server _server;
        public mainWindow()
        {
            InitializeComponent();
        }

        private void mainWindow_Load(object sender, EventArgs e)
        {
            LoadModules();
        }


        private void LoadModules()
        {
            DirectoryInfo di = new DirectoryInfo("Modules");
            if (!di.Exists)
                di.Create();
            foreach (FileInfo fi in di.GetFiles("*.ncm"))
            {
                try
                {
                    Assembly asm = Assembly.LoadFile(fi.FullName);
                    foreach (Type t in asm.GetTypes())
                    {

                        foreach (MethodInfo mi in t.GetMethods())
                        {
                            if (!Attribute.IsDefined(mi, typeof(RemoteCallAttribute)))
                                continue;
                            RemoteFunction function = new RemoteFunction(asm, t, mi, lvFunctions);
                            if (LoadedFunctions.ContainsKey(function.Hash))
                            {
                                function.Dispose();
                            }
                            else
                            {
                                LoadedFunctions.Add(function.Hash, function);
                                lvFunctions.Items.Add(new RemoteFunctionListViewItem(function));
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogError(ex);
                }
            }
        }

        private void LogError(Exception ex)
        {
            if (lvFunctions.InvokeRequired)
            {
                lvFunctions.Invoke(new MethodInvoker(() => 
                {
                    lvFunctions.Items.Add(new ListViewItem(ex.Message));
                }));

            }
            else
            {
                lvFunctions.Items.Add(new ListViewItem(ex.Message));
            }
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            _server = new eSock.Server();

            _server.OnDataRetrieved += _server_OnDataRetrieved;

            if (!_server.Start((int)nudPort.Value))
            {
                MessageBox.Show("Failed to start.");
                return;
            }

            btnStart.Enabled = false;
            nudPort.Enabled = false;
        }

        private void _server_OnDataRetrieved(eSock.Server sender, eSock.Server.eSockClient client, object[] data)
        {
            try
            {
                NetworkHeaders header = (NetworkHeaders)data[0];

                if (header == NetworkHeaders.Handshake)
                {
                    string key = Guid.NewGuid().ToString();
                    client.Send((byte)NetworkHeaders.AcceptHandshake, key);
                    client.Encryption.EncryptionKey = key;
                    client.Encryption.Enabled = true;
                    return;
                }

                if (header == NetworkHeaders.RemoteCall)
                {
                    string functionHash = (string)data[1];

                    if (!LoadedFunctions.ContainsKey(functionHash))
                    {
                        Console.WriteLine("Invalid call ({0})", functionHash);
                        client.Send(null);
                        return;
                    }

                    object[] args = (object[])data[2];

                    RemoteFunction function = LoadedFunctions[functionHash];
                    client.Send(function.Execute(args));
                    Console.WriteLine("Function Call ({0}) Value={1}", function, functionHash);
                }
            }
            catch (Exception ex)
            {
                client.Send(null);
                LogError(ex);
            }
        }

        private void enableToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach(RemoteFunctionListViewItem i in lvFunctions.Items)
            {
                i.Function.ExecuteAction = ExecuteAction.Normal;
            }
        }

        private void disableToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (RemoteFunctionListViewItem i in lvFunctions.Items)
            {
                i.Function.ExecuteAction = ExecuteAction.Disabled;
            }
        }

        private void testCallToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (lvFunctions.Items.Count < 1)
                return;
            List<RemoteFunction> functions = new List<RemoteFunction>();
            foreach (RemoteFunctionListViewItem i in lvFunctions.Items)
            {
                functions.Add(i.Function);
            }

            using (formTestCall testCall = new formTestCall(functions.ToArray()))
            {
                testCall.ShowDialog();
            }
        }
    }
}
