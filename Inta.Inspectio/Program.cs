using Inta.Authentication.Shared;
using Inta.Inspectio.Authentication;
using Inta.Inspectio.Model;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

var config = new ConfigurationBuilder()
                 .SetBasePath(Directory.GetCurrentDirectory())
                 .AddJsonFile("appsettings.json")
                 .Build();

// Add services to the container.
builder.Services.AddControllers();
builder.Services.Configure<HostSettings>(config.GetSection(nameof(HostSettings)));
builder.Services.Configure<JwtTokenSettings>(config.GetSection(nameof(JwtTokenSettings)));
ConfigureAuthentication(builder.Services, config);

builder.Services.AddHttpClient(Constants.AuthenticationHttpClient, (provider, options) =>
{
    var settings = provider.GetService<IOptions<HostSettings>>();
    options.BaseAddress = new Uri(settings.Value.AuthenticationAddress);
});

builder.Services.AddScoped<PermissionAuthorizationFilter>();
builder.Services.AddSwaggerDocument();
builder.Services.AddDbContext<DatabaseContext>();
var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();
app.UseOpenApi();
app.UseSwaggerUi3();
app.MapControllers();

app.Run();


void ConfigureAuthentication(IServiceCollection services,IConfiguration configuration)
{
    var jwtTokenSettings = configuration.GetSection(nameof(JwtTokenSettings)).Get<JwtTokenSettings>();

    services.AddScoped<PermissionAuthorizationFilter>();

    services.AddAuthentication(options =>
    {
        options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultSignInScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultForbidScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultSignOutScheme = JwtBearerDefaults.AuthenticationScheme;

    }).AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
    {
        options.ClaimsIssuer = jwtTokenSettings.Issuer;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtTokenSettings.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtTokenSettings.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtTokenSettings.ValidationKey)),
            TokenDecryptionKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtTokenSettings.DecryptionKey)),
            ValidateLifetime = true,
            NameClaimType = AuthConstants.UserNameClaimType,
            RoleClaimType = AuthConstants.UserRolesClaimType
        };
        options.SaveToken = true;
        options.Events = new JwtBearerEvents()
        {
            OnMessageReceived = context =>
            {
                context.Token = context.HttpContext.Request.Cookies[AuthConstants.CookieName];
                // a.ammari: DO NOT SET context.Result.
                return Task.CompletedTask;
            }
        };
    });

    services.AddAuthorization();
}

public class HostSettings
{
    public string AuthenticationAddress { get; set; }
}

public class JwtTokenSettings
{
    public string DecryptionKey { get; set; }
    public string ValidationKey { get; set; }
    public string Issuer { get; set; }
    public string Audience { get; set; }
}