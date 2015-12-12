using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NetCoreServer_GUI.Forms
{
    public partial class formShowString : Form
    {
        public formShowString(string title, object value)
        {
            InitializeComponent();
            this.Text = title;
            rtbData.Text = value.ToString();
        }

        private void formShowString_Load(object sender, EventArgs e)
        {

        }
    }
}
