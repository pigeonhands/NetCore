using System.Net;
using NetCore;
using System.Windows.Forms;
using System.Data;
using System.Data.SqlClient;
using System;

namespace NetCore_TExample
{
    public class ClassTester
    {
        static string HiddenField = "Nice meme.";

        [RemoteCall]
        public static string ReturnSqlCommand()
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

        [RemoteCall]
        public static string ThrowError()
        {
            throw new Exception("This is an exception being thrown");
        }
    }
}
