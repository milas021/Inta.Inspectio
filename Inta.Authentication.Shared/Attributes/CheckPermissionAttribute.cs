using System;

namespace Inta.Authentication.Shared.Attributes
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
    public class CheckPermissionAttribute : Attribute
    {
        // a.ammari : multiple types of Permission exists
        public object Permission { get; }

        public CheckPermissionAttribute(object permission)
        {
            Permission = permission;
        }
    }
}
