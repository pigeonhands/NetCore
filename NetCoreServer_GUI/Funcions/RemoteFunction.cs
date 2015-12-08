using NetCore.Networking;
using NetCoreServer_GUI.Controls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NetCoreServer_GUI.Funcions
{
    public class RemoteFunction : IDisposable
    {
        public RemoteFunctionListViewItem ListViewItem { get; set; }
        public ExecuteAction ExecuteAction
        {
            get { return _ExecuteAction; }
            set
            {
                _ExecuteAction = value;
                UpdateListView();
            }
        }
        public string FullName { get; private set; }
        public string Hash { get; private set; }
        public string File { get; private set; }
        public long ExecuteCount
        {
            get { return _ExecuteCount; }
            set
            {
                _ExecuteCount = value;
                UpdateListView();
            }
        }

        private MethodInfo Method;
        private Type ParentType;
        private Assembly loadedAssembly;
        private ExecuteAction _ExecuteAction;
        private long _ExecuteCount;
        private Control _invoke;

        public RemoteFunction(Assembly asm, Type t, MethodInfo mi, Control invokeItem)
        {
            _invoke = invokeItem;
            loadedAssembly = asm;
            ParentType = t;
            Method = mi;

            File = Path.GetFileName(loadedAssembly.Location);

            ExecuteAction = ExecuteAction.Normal;
            FullName = string.Format("{0}.{1}", ParentType.FullName, Method.Name);
            Hash = Hashing.SHA(FullName);
        }

        public object Execute(object[] args)
        {
            if (ExecuteAction == ExecuteAction.Disabled)
                return null;
            ExecuteCount++;
            return Method.Invoke(null, args);
        }


        private void UpdateListView()
        {
            if (ListViewItem == null)
                return;
            _invoke.Invoke(new MethodInvoker(() =>
            {
                ListViewItem.SubItems[3].Text = _ExecuteAction.ToString();
                ListViewItem.SubItems[4].Text = _ExecuteCount.ToString();
            }));
        }

        public void Dispose()
        {
            
        }
    }
}
