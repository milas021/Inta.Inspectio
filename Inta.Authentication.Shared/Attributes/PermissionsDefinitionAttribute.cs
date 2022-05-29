﻿using System;

namespace Inta.Authentication.Shared.Attributes
{
    [AttributeUsage(AttributeTargets.Enum)]
    public class PermissionsDefinitionAttribute : Attribute
    {
        public PermissionsDefinitionAttribute(string serviceName)
        {
            ServiceName = serviceName;
        }

        public string ServiceName { get; set; }
    }
}
