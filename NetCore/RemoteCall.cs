using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetCore
{
    [AttributeUsage(System.AttributeTargets.Method)]
    public class RemoteCallAttribute : Attribute
    { 
    }

    [AttributeUsage(System.AttributeTargets.Method)]
    public class RemoteMoveAttribute : Attribute
    {
    }

}
