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
                    Exception innerEx = ex.InnerException;
                    execTimer.Stop();
                    i.SubItems.Add("True");
                    i.SubItems.Add(innerEx.Message);
                    i.SubItems.Add(innerEx.GetType().ToString());
                    i.Tag = innerEx;
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

        private void lvTestResults_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (lvTestResults.SelectedItems.Count < 1)
                return;
            ListViewItem i = lvTestResults.SelectedItems[0];
            if (i.Tag is Exception)
            {
                using (formShowString exShow = new formShowString("Exception", i.Tag))
                {
                    exShow.ShowDialog();
                }
            }
        }
    }
}
