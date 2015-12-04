using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetCoreBuilder
{
    public class TypeCheck
    {
        public static bool KeepMethod(TypeDefinition type, MethodDefinition method, out TransportAction visibility)
        {
            bool hasAttribute = false;
            TransportAction vis = TransportAction.Public;

            
            if (method.Name == ".cctor")
            {
                hasAttribute = true;
                vis = TransportAction.Copy;
                
                foreach (var i in type.CustomAttributes)
                {
                    if (i.AttributeType.FullName == "NetCore.ClearFieldsAttribute")
                    {
                        vis = TransportAction.MoveClear;
                        break;
                    }
                }
                
            }
            else
            {
                foreach (var i in method.CustomAttributes)
                {
                    if (i.AttributeType.FullName == "NetCore.RemoteCallAttribute")
                    {
                        hasAttribute = true;
                        break;
                    }
                    if (i.AttributeType.FullName == "NetCore.RemoteMoveAttribute")
                    {
                        hasAttribute = true;
                        vis = TransportAction.Move;
                        break;
                    }
                    if (i.AttributeType.FullName == "NetCore.RemoteCopyAttribute")
                    {
                        hasAttribute = true;
                        vis = TransportAction.Copy;
                        break;
                    }
                }
            }
            visibility = vis;
            return hasAttribute;

        }
    }

    public enum TransportAction
    {
        Public,
        Copy,
        Move,
        MoveClear
    }
}
