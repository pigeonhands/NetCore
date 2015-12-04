using System;

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

    [AttributeUsage(System.AttributeTargets.Method)]
    public class RemoteCopyAttribute : Attribute
    {
    }

    [AttributeUsage(System.AttributeTargets.Class)]
    public class ClearFieldsAttribute : Attribute
    {
    }

}
