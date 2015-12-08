using NetCoreServer_GUI.Funcions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NetCoreServer_GUI.Controls
{
    public class RemoteFunctionListViewItem : ListViewItem
    {
        public RemoteFunction Function { get; private set; }
        public RemoteFunctionListViewItem(RemoteFunction function) : base(function.Hash)
        {
            Function = function;
            SubItems.Add(function.FullName);
            SubItems.Add(function.File);
            SubItems.Add(function.ExecuteAction.ToString());
            SubItems.Add(function.ExecuteCount.ToString());
            function.ListViewItem = this;
        }
    }
}
