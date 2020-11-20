using System;
using System.IdentityModel.Tokens.Jwt;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.CookiePolicy;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

namespace KeycloakUserProfileUi
{
  public class Startup
  {
    public Startup(IConfiguration configuration)
    {
      Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    // This method gets called by the runtime. Use this method to add services to the container.
    public void ConfigureServices(IServiceCollection services)
    {
      services.AddControllersWithViews();

      JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

      services.Configure<CookiePolicyOptions>(options =>
      {
        // This lambda determines whether user consent for non-essential cookies is needed for a given request.
        options.CheckConsentNeeded = context => true;
        options.MinimumSameSitePolicy = SameSiteMode.None;
        options.HttpOnly = HttpOnlyPolicy.None;
        options.Secure = CookieSecurePolicy.SameAsRequest;
      });

      services.AddAuthentication(options =>
      {
        // Store the session to cookies
        options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;

        options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;

        options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;

        // OpenId authentication
        options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
      })
      .AddCookie(options =>
      {
        options.Cookie.Name = "KEYCLOAK_NETCORE_SESSION";
        options.Cookie.IsEssential = true;
        options.Cookie.HttpOnly = false;
      })
      .AddOpenIdConnect(options =>
      {
        options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;

        // URL of the Keycloak server
        options.Authority = Configuration["Keycloak:AuthorityUrl"];

        // Client configured in the Keycloak
        options.ClientId = Configuration["Keycloak:ClientId"];

        // For testing we disable https (should be true for production)
        options.RequireHttpsMetadata = false;
        options.SaveTokens = true;

        options.GetClaimsFromUserInfoEndpoint = true;

        // OpenID flow to use
        options.ResponseType = OpenIdConnectResponseType.CodeIdToken;

        options.Scope.Add("openid");
        options.Scope.Add("profile");

        options.TokenValidationParameters = new TokenValidationParameters
        {
          ValidateIssuer = true,
          ValidIssuer = Configuration["Keycloak:AuthorityUrl"],
          ValidateLifetime = true,
          NameClaimType = "preferred_username",
        };

        options.Events.OnRedirectToIdentityProvider = context =>
        {
          context.ProtocolMessage.RedirectUri = ChangeToCorrectHttpSchema(context.ProtocolMessage.RedirectUri);

          return Task.CompletedTask;
        };

        options.Events.OnRedirectToIdentityProviderForSignOut = context =>
        {
          context.ProtocolMessage.PostLogoutRedirectUri = ChangeToCorrectHttpSchema(context.ProtocolMessage.PostLogoutRedirectUri);

          return Task.CompletedTask;
        };
      });
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
      if (env.IsDevelopment())
      {
        app.UseDeveloperExceptionPage();
      }
      else
      {
        app.UseExceptionHandler("/Home/Error");
        // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
        app.UseHsts();
      }
      app.UseHttpsRedirection();
      app.UseStaticFiles();

      app.UseRouting();

      app.UseCookiePolicy();
      app.UseAuthentication();
      app.UseAuthorization();

      app.UseEndpoints(endpoints =>
      {
        endpoints.MapControllerRoute(
                  name: "default",
                  pattern: "{controller=Home}/{action=Index}/{id?}");
      });
    }

    private string ChangeToCorrectHttpSchema(string uri)
    {
      var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
      var production = "Production".Equals(environment) ? true : false;

      return production ? uri.ToLower().Replace("http", "https") : uri;
    }
  }
}
