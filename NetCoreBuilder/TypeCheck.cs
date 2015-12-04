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
        public static bool KeepMethod(TypeDefinition type, MethodDefinition method, out Visibility visibility)
        {
            bool hasAttribute = false;
            Visibility vis = Visibility.Public; ;


            if (method.Name == ".cctor" && type.IsSealed && type.IsAbstract)
            {
                hasAttribute = true;

                foreach (var i in type.CustomAttributes)
                {
                    if (i.AttributeType.FullName == "NetCore.ClearFieldsAttribute")
                    {
                        vis = Visibility.PrivateMove;
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
                        vis = Visibility.PrivateMove;
                        break;
                    }
                    if (i.AttributeType.FullName == "NetCore.RemoteCopyAttribute")
                    {
                        hasAttribute = true;
                        vis = Visibility.PrivateCopy;
                        break;
                    }
                }
            }
            visibility = vis;
            return hasAttribute;
        }
    }

    public enum Visibility
    {
        Public,
        PrivateCopy,
        PrivateMove
    }
}
