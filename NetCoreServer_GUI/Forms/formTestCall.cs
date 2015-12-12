using NetCoreServer_GUI.Funcions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NetCoreServer_GUI.Forms
{
    public partial class formTestCall : Form
    {
        public formTestCall(RemoteFunction[] functions)
        {
            InitializeComponent();

            Stopwatch execTimer = new Stopwatch();
            foreach(RemoteFunction f in functions)
            {
                ListViewItem i = new ListViewItem(f.Hash);
                try
                {
                    ParameterInfo[] paramInfo = f.Method.GetParameters();
                    List<object> paramiters = new List<object>();

                    foreach(ParameterInfo p in paramInfo)
                    {
                        paramiters.Add(p.DefaultValue);
                    }

                    execTimer.Start();
                    object result = f.Method.Invoke(null, paramiters.ToArray());
                    execTimer.Stop();

                    i.SubItems.Add("False");
                    i.SubItems.Add(result == null ? "Null" : result.ToString());
                    i.SubItems.Add(result.GetType().ToString());
                }
                catch(Exception ex)
                {
                    execTimer.Stop();
                    i.SubItems.Add("True");
                    i.SubItems.Add(ex.InnerException.Message);
                    i.SubItems.Add(ex.InnerException.GetType().ToString());
                }
                finally
                {
                    i.SubItems.Add(execTimer.ElapsedMilliseconds.ToString());
                    execTimer.Reset();
                    lvTestResults.Items.Add(i);
                }
            }
        }

        private void formTestCall_Load(object sender, EventArgs e)
        {

        }
    }
}
