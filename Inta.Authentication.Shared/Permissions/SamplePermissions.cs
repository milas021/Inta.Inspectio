using System;
using System.Collections.Generic;
using System.Text;
using Inta.Authentication.Shared.Attributes;

namespace Inta.Authentication.Shared.Permissions
{
    [PermissionsDefinition("Sample")]
    public enum SamplePermissions
    {
        /// <summary>
        /// دریافت اطلاعات درخواست
        /// </summary>
        [PermissionDescription("تست پرمیژن ها", "دریافت اطلاعات درخواست")]
        ViewRequestInfo,
    }
}
