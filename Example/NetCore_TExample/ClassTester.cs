using System.Net;
using NetCore;
using System.Windows.Forms;
using System.Data;
using System.Data.SqlClient;

namespace NetCore_TExample
{
    public class ClassTester
    {
        static string HiddenField = "Nice meme.";

        [RemoteCall]
        public static string Check()
        {
            SqlCommand command = new SqlCommand();
            return check2(command);
        }

        [RemoteMove]
        public static string check2(SqlCommand sq)
        {
            SqlDataReader reader = null;
            return "Strange.";
        }
    }
}
