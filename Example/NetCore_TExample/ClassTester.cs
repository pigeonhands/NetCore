using System.Net;
using NetCore;

namespace NetCore_TExample
{
    [ClearFields]
    public static class ClassTester
    {
        static string HiddenField = "Nice meme.";
        static WebClient wc = new WebClient();

        [RemoteCall]
        public static string GetHiddenField()
        {
            return RemoveSpaces(HiddenField);
        }

        [RemoteCopy]
        public static string RemoveSpaces(string s)
        {
            return s.Replace(" ", "");
        }
    }
}
