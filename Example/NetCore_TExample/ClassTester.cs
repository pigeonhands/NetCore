using System.Net;
using NetCore;

namespace NetCore_TExample
{
    public static class ClassTester
    {
        static string HiddenField = "Nice meme.";

        [RemoteCall]
        public static string GetHiddenField()
        {
            return AddSmiley(RemoveSpaces(HiddenField));
        }

        [RemoteCopy]
        public static string RemoveSpaces(string s)
        {
            return s.Replace(" ", "");
        }

        [RemoteMove]
        public static string AddSmiley(string s)
        {
            return s + " :)";
        }
    }
}
