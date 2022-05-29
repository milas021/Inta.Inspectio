using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using Inta.Authentication.Shared;
using Inta.Authentication.Shared.Attributes;
using Inta.Authentication.Shared.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Inta.Inspectio.Authentication
{
    public class PermissionAuthorizationFilter : IAsyncAuthorizationFilter
    {
        private readonly HttpClient _client;

        public string ServiceName { get; } = Constants.MicroServiceName;

        public PermissionAuthorizationFilter(IHttpClientFactory clientFactory)
        {
            _client = clientFactory.CreateClient(Constants.AuthenticationHttpClient);
        }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            try
            {
                // در صورتی که منبع درخواست شده نیاز به تایید هویت نداشته باشد
                if (ResourceIsAnonymous(context.ActionDescriptor))
                    return;

                // در صورتی که مشخصات کاربر قابل استخراج نباشد، کاربر لاگین نباشد یا توکن وی منقضی شده باشد
                var userInfo = context.HttpContext.User?.Claims?.ToList().ToUserInfo();
                if (userInfo?.Roles == null || !userInfo.Roles.Any())
                {
                    context.Result = new UnauthorizedResult();
                    return;
                }

                // در صورتی که برای منبع درخواستی حداقل یک پرمیژن ثبت شده باشد و کاربر ادمین نباشد
                var permissions = GetAppliedPermissions(context.ActionDescriptor);
                if (permissions.Any() && !UserIsSuperAdmin(userInfo))
                {
                    // در صورتی که کاربر حداقل یکی از پرمیژن های منبع درخواست شده را داشته باشد
                    foreach (var permission in permissions)
                    {
                        var request = new HttpRequestMessage(HttpMethod.Get,
                            $"UserGroups/users/{userInfo.UserId}/isAuthorize?" +
                            $"permissionName={Constants.MicroServiceName}-{permission}&serviceName={Constants.MicroServiceName}");
                        request.Headers.TryAddWithoutValidation("Cookie", context.HttpContext.Request.Headers["Cookie"].ToString());
                        var response = await _client.SendAsync(request);
                        if (response.IsSuccessStatusCode)
                        {
                            var result = JsonSerializer.Deserialize<ResponseMessage<bool>>(await response.Content.ReadAsStringAsync());
                            if (result.Content)
                                return;
                        }
                    }
                    context.Result = new ForbidResult();

                }
                // در صورتی که برای منبع پرمیژنی ثبت نشده باشد یا کاربر ادمین باشد، فقط توکن کاربر بررسی می شود
                else
                {
                    //var request = new HttpRequestMessage(HttpMethod.Get, "check/token");
                    //request.Headers.TryAddWithoutValidation("Cookie", context.HttpContext.Request.Headers["Cookie"].ToString());
                    //var response = await _client.SendAsync(request);
                    //response.EnsureSuccessStatusCode();
                    //var result = JsonSerializer.Deserialize<ResponseMessage<bool>>(await response.Content.ReadAsStringAsync());
                    //if (!result.Content)
                    //{
                    //    context.Result = new UnauthorizedResult();
                    //}
                }
            }
            catch
            {
                context.Result = new UnauthorizedResult();
            }
        }

        private static bool ResourceIsAnonymous(ActionDescriptor descriptor)
        {
            if (!(descriptor is ControllerActionDescriptor actionDescriptor))
                return false; // worse case
            var attributes = actionDescriptor.MethodInfo.GetCustomAttributes<AllowAnonymousAttribute>();
            if (!attributes.Any())
            {
                return actionDescriptor.ControllerTypeInfo.GetCustomAttributes<AllowAnonymousAttribute>().Any() &&
                       !actionDescriptor.MethodInfo.GetCustomAttributes<AuthorizeAttribute>().Any();
            }

            return true;
        }


        private static bool UserIsSuperAdmin(UserInfo userInfo)
        {
            if (userInfo?.Roles == null || !userInfo.Roles.Any())
                return false;
            return userInfo.Roles.Contains("SuperAdmin");
        }


        private List<string> GetAppliedPermissions(ActionDescriptor descriptor)
        {
            if (!(descriptor is ControllerActionDescriptor actionDescriptor))
                return new List<string>();

            var permissions = actionDescriptor.MethodInfo.GetCustomAttributes<CheckPermissionAttribute>()
                .Concat(actionDescriptor.ControllerTypeInfo.GetCustomAttributes<CheckPermissionAttribute>()).ToList();

            return permissions.Select(i => i.Permission?.ToString()).ToList();
        }


    }

    public static class Constants
    {
        public const string MicroServiceName = "Sample";
        public const string AuthenticationHttpClient = nameof(AuthenticationHttpClient);


    }

    public class ResponseMessage
    {
        public ResponseMessage()
        {
        }

        public ResponseMessage(string message
        )
        {
            Message = message;
        }

        public string Message { get; set; }

    }

    public class ResponseMessage<T> : ResponseMessage
    {
        public ResponseMessage()
        {
        }

        public ResponseMessage(string message, T content)
        {
            Message = message;
            Content = content;
        }

        public T Content { get; set; }

    }
}
