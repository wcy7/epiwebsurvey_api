using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using AutoMapper;
using epiwebsurvey_api.Context;
using epiwebsurvey_api.Mapper;
using epiwebsurvey_api.MessageHandlers;
using epiwebsurvey_api.Repositories;
using epiwebsurvey_api.Repositories.Interfaces;
using epiwebsurvey_api.Utils;
using System.Text.Json;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Globalization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.DataProtection;
using IdentityModel;
using System.Security.Cryptography;
using epiwebsurvey_api.Properties;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using IdentityModel.Client;
using Epi.Web.Common.Security;
using System.Web;
using System.Reflection;
using System.IO;

namespace epiwebsurvey_api
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
            services.AddMvc().AddSessionStateTempDataProvider();
            services.AddSession();
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Latest);
            services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();//User Claims
            services.AddScoped<ISurveyResponseRepository, SurveyResponseRepository>();
            services.AddControllers();
            services.Configure<EnvironmentConfig>(Configuration);
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Survey API", Version = "v1", Description = "An API to push Survey responses" });
                c.OperationFilter<AddRequiredHeaderSwagger>();
                // c.OperationFilter<OAuth2OperationFilter>();
                c.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
                {
                    Name = "oauth2",
                    Type = SecuritySchemeType.OAuth2,
                    Flows = new OpenApiOAuthFlows
                    {
                        // Implicit = new OpenApiOAuthFlow
                        AuthorizationCode = new OpenApiOAuthFlow
                        {
                            TokenUrl = new Uri(Environment.GetEnvironmentVariable("endpoint_token")),
                            AuthorizationUrl = new Uri(Environment.GetEnvironmentVariable("endpoint_authorization")),
                            Scopes = new Dictionary<string, string>
                            {
                                { "openid", "Access read operations" },
                                { "email", "Access write operations" },
                                 { "profile", "Access read operations" }
                                //{ "readAccess", "Access read operations" },
                                //{ "writeAccess", "Access write operations" }
                            }
                        }
                    }
                });
                // Set the comments path for the Swagger JSON and UI.
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);
            
            c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "oauth2" }
                        },
                        new[] { "readAccess", "writeAccess" }
                    }
                });

                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "Standard Authorization header using the Bearer scheme. Example: \"bearer {token}\"",
                    In = ParameterLocation.Header,
                    Name = "Authorization",
                    BearerFormat = "JWT",
                    Type = SecuritySchemeType.Http,                   
                    Scheme = "bearer"
                });
                var security = new Dictionary<string, IEnumerable<string>>
                {
                    {"bearer", new string[] { }},
                };                
                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                    {
                        {
                            new OpenApiSecurityScheme
                            {
                                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
                            },
                            new List<string>()
                        }
                    });
            });

            // Auto Mapper Configurations
            var mappingConfig = new MapperConfiguration(mc =>
            {
                mc.AddProfile(new MappingProfile());
            });
            IMapper mapper = mappingConfig.CreateMapper();
            services.AddSingleton(mapper);
            //services.AddDbContext<epiwebsurvey_apiContext>(options =>
            //     options.UseSqlServer(Configuration.GetConnectionString("epiinfosurveyapiContext")));
            services.AddDbContext<epiwebsurvey_apiContext>(options =>
                options.UseSqlServer(Environment.GetEnvironmentVariable("epiinfosurveyapiContext")));

            services.AddCors(c =>    //Enabling CORS 
            {
                c.AddPolicy("AllowOrigin", options => options.AllowAnyOrigin()
                );
            });
            //var sams_endpoint_authorization = Configuration["sams:endpoint_authorization"];
            //var sams_endpoint_token = Configuration["sams:endpoint_token"];
            //var sams_endpoint_user_info = Configuration["sams:endpoint_user_info"];
            //var sams_endpoint_token_validation = Configuration["sams:token_validation"];
            //var sams_endpoint_user_info_sys = Configuration["sams:user_info_sys"];
            //var sams_client_id = Configuration["sams:client_id"];
            //var sams_client_secret = Configuration["sams:client_secret"];            
            var sams_endpoint_authorization = Environment.GetEnvironmentVariable("endpoint_authorization");
            var sams_endpoint_token = Environment.GetEnvironmentVariable("endpoint_token");
            var sams_endpoint_user_info = Environment.GetEnvironmentVariable("endpoint_user_info");
            var sams_endpoint_token_validation = Environment.GetEnvironmentVariable("endpoint_token_validation");
            var sams_endpoint_user_info_sys = Environment.GetEnvironmentVariable("endpoint_user_info_sys");
            var sams_client_id = Environment.GetEnvironmentVariable("client_id");
            var sams_client_secret = Environment.GetEnvironmentVariable("client_id");
            var sams_callback_url = Environment.GetEnvironmentVariable("callback_url");
            var SecretKey = Configuration["sams:secret"];
            var Issuer = Configuration["sams:issuer"];
            services.AddAuthentication(options =>
            {
                options.DefaultScheme = "Cookies";
                options.DefaultChallengeScheme = "Bearer";
            })
                .AddCookie("Cookies", CookieAuthenticationDefaults.AuthenticationScheme,
                      options =>
                      {
                          options.ForwardDefaultSelector = ctx =>
                          {
                              if (ctx.Request.Path.StartsWithSegments("/api"))
                              {
                                  return "Bearer";
                              }
                              else
                              {
                                  return "Cookies";
                              }
                          };
                          options.LoginPath = new PathString("/Authorization/SignIn");
                          options.Cookie.Name = "authToken";                         
                          options.AccessDeniedPath = new PathString("/authorization/error/");
                          options.Cookie.SameSite = SameSiteMode.None;//Strict
                          options.Cookie.IsEssential = true;
                          options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                          options.ExpireTimeSpan = TimeSpan.FromHours(1);
                         // options.Cookie.Expiration= TimeSpan.FromHours(12);

                          options.Events = getsmasevents();
                      })
                 .AddJwtBearer("Bearer", options =>
                 {
                     options.SaveToken = true;
                     options.RequireHttpsMetadata = true;
                     //options.TokenValidationParameters = new TokenValidationParameters {
                        // RequireExpirationTime = true,
                         //RequireSignedTokens = true,
                        // ValidateIssuer = false,
                         //ValidIssuer = "https://contoso.com",
                         //ValidateIssuerSigningKey = true,
                        // ValidateAudience = false,
                        // IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SecretKey)),
                         //ValidAudience = "https://localhost:5001"


                    // };                    
                     options.Events = new JwtBearerEvents
                     {
                         OnAuthenticationFailed=ct =>
                         {
                             if (ct.Exception.GetType() == typeof(SecurityTokenExpiredException))
                             {
                                 ct.Response.Headers.Add("Token-Expired", "true");
                             }
                             else if(ct.Exception.GetType()==typeof(SecurityTokenSignatureKeyNotFoundException))
                             {
                                 ct.Response.Headers.Add("Please-login", "true");                                 
                             }
                             return Task.CompletedTask;
                         },
                         OnMessageReceived = ct =>
                         {                               
                             if(ct.HttpContext.Request.Path.Value.Contains("authorization/callback")|| ct.HttpContext.Request.Path.Value.Contains("/refreshtoken"))
                             {
                                 return Task.CompletedTask;
                             }
                             if (ct.HttpContext.Request.Headers.ContainsKey("Authorization"))
                             {
                                 var authheader = ct.HttpContext.Request.Headers["Authorization"];
                                 var access_token = authheader.ToString().Substring(7);
                                 var tokenClient = new Utils.TokenClient(Configuration);                                 
                                 var token = tokenClient.DecodeJwtToken(access_token);                               
                                 if (token != null)
                                 {
                                     var refreshToken = Cryptography.Decrypt(token.Claims
                                        .Where(c => c.Type.Equals("refresh_token"))
                                        .Select(c => c.Value).FirstOrDefault());
                                     var acessToken = Cryptography.Decrypt(token.Claims
                                      .Where(c => c.Type.Equals("access_token"))
                                      .Select(c => c.Value).FirstOrDefault());
                                     var expiry = token.Claims
                                      .Where(c => c.Type.Equals("http://schemas.microsoft.com/ws/2008/06/identity/claims/expiration"))
                                      .Select(c => c.Value).FirstOrDefault();
                                     var useremail = token.Claims
                                      .Where(c => c.Type.Equals("email"))
                                      .Select(c => c.Value).FirstOrDefault();
                                     var Username = token.Claims
                                     .Where(c => c.Type.Equals("unique_name"))
                                     .Select(c => c.Value).FirstOrDefault();
                                     var tokenResponse = tokenClient.ValidateToken(acessToken);
                                     if (tokenResponse.Result.IsValidToken == true && !string.IsNullOrEmpty(tokenResponse.Result.userid))
                                     {
                                         ct.Token = access_token;
                                         Program.userPrincipal = tokenResponse.Result.userid;
                                         var userIdentity = new ClaimsIdentity("SuperSecureLogin");
                                         userIdentity.AddClaim(new Claim(ClaimTypes.Name, tokenResponse.Result.userid, ClaimValueTypes.String, Issuer));
                                         var newPrincipal = new ClaimsPrincipal();
                                         newPrincipal.AddIdentity(userIdentity);
                                         ct.Principal = newPrincipal;
                                         ct.Success();
                                         return Task.CompletedTask;
                                     }
                                 }
                                 //else
                                 //{
                                 //    if(expiry!=null)
                                 //    {
                                 //        //DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeSeconds(expiry);
                                 //        //var exp= dateTimeOffset.LocalDateTime;
                                 //        var exp = DateTimeOffset.Parse(expiry);
                                 //        if (exp < DateTime.Now)
                                 //        {
                                 //            //var token_response = tokenClient.get_refresh_token(acessToken, refreshToken, Username,useremail).Result;
                                 //            //if (token_response.is_error)
                                 //            //{
                                 //            //    //reject Principal
                                 //            //   // ct.();
                                 //            //    return Task.CompletedTask;
                                 //            //}
                                 //           // Program.isRefresh = true;
                                 //            //Program.jwtToken = token_response.jwt_token;                                            
                                 //            //var userIdentity = new ClaimsIdentity("SuperSecureLogin");                                            
                                 //            //userIdentity.AddClaim(new Claim(ClaimTypes.Name, Username, ClaimValueTypes.String, Issuer));
                                 //            //var newPrincipal = new ClaimsPrincipal();
                                 //            //newPrincipal.AddIdentity(userIdentity);
                                 //            //ct.Principal = newPrincipal;
                                 //            //ct.Success();
                                 //            return Task.CompletedTask;
                                 //        }
                                 //    }

                                 //}
                             }                            
                             return Task.CompletedTask;
                         },
                         OnForbidden = ct =>
                         {
                             var accessToken = ct.Result;
                             return Task.CompletedTask;
                         },
                         OnTokenValidated = context =>
                         {
                             var accessToken = context.SecurityToken as JwtSecurityToken;
                             if (accessToken != null)
                             {
                                 ClaimsIdentity identity = context.Principal.Identity as ClaimsIdentity;
                                 if (identity != null)
                                 {
                                     identity.AddClaim(new Claim("access_token", accessToken.RawData));
                                 }
                             }
                             return Task.CompletedTask;
                         }
                         //  var authenticateInfo = await HttpContext.Authentication.GetAuthenticateInfoAsync("Bearer");
                         // string accessToken = authenticateInfo.Properties.Items[".Token.access_token"];
                     };
                 });              
            services.AddMvc(config =>
            {
                var policy = new AuthorizationPolicyBuilder()
                 .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme‌​)
                                .RequireAuthenticatedUser()
                                .Build();
                config.Filters.Add(new AuthorizeFilter(policy));
            });
        }
          
        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseRouting();
            app.UseCookiePolicy();

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
            app.UseSession();
            app.UseCors();
            app.UseCors(options => options.AllowAnyOrigin());//inject CORS into a container
          // app.UseCors(options => options.WithOrigins("https://localhost:5001"));
            // app.UseCors(options => options.WithOrigins("https://epiwebsurveyapi-dhis-test.services-dev.cdc.gov"));
            app.UseCors(options => options.AllowAnyHeader());
            app.UseCors(options => options.AllowAnyMethod());
            app.UseHttpsRedirection();
            app.UseStaticFiles();
            // Enable middleware to serve generated Swagger as a JSON endpoint.
            var sams_client_id = Environment.GetEnvironmentVariable("client_id");
            var sams_client_secret = Environment.GetEnvironmentVariable("client_secret");
            var sams_endpoint_authorization = Environment.GetEnvironmentVariable("endpoint_authorization");
            var sams_endpoint_token = Environment.GetEnvironmentVariable("endpoint_token");
            //var sams_endpoint_authorization = Configuration["sams:endpoint_authorization"];
            //var sams_endpoint_token = Configuration["sams:endpoint_token"];            
            //var sams_client_id = Configuration["sams:client_id"];
            //var sams_client_secret = Configuration["sams:client_secret"];          
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", " epiwebsurvey_api V1");
                c.RoutePrefix = string.Empty;
                c.OAuthRealm("test-realm");
                c.OAuthClientId(sams_client_id);
                c.OAuthClientSecret(sams_client_secret);
                c.OAuth2RedirectUrl(HttpUtility.UrlDecode(Environment.GetEnvironmentVariable("callback_url")));
                // c.OAuth2RedirectUrl("https://localhost:5001/authorization/callback");
                c.OAuthUseBasicAuthenticationWithAccessCodeGrant();
                c.OAuthScopeSeparator(" ");
                c.OAuthAppName("Swagger Api Calls");
                c.OAuthAdditionalQueryStringParams(new Dictionary<string, string>
            {
                { "resource", "{api/surveyresponse}"} //api
                });
                c.OAuthUsePkce();
                c.ConfigObject.DeepLinking = true;
            });

            app.Use(async (context, next) =>
            {
                var principal = context.User as ClaimsPrincipal;
                var accessToken = principal?.Claims
                   .FirstOrDefault(c => c.Type == "access_token");
                if (accessToken != null)
                {
                    //_logger.LogDebug(accessToken.Value);
                }
                //await next();
                DateTime expires;
               // var authenticationInfo = await context.AuthenticateAsync();

              //  var access_Token = authenticationInfo.Properties.GetTokenValue("access_token");
               // var refresh_Token = authenticationInfo.Properties.GetTokenValue("refresh_token");

                var idToken = await context.GetTokenAsync("id_token");
                var expiresToken = await context.GetTokenAsync("expires_at");
                var accesstoken = await context.GetTokenAsync("access_token");
                var refreshToken = await context.GetTokenAsync("refresh_token");

                if (refreshToken != null && (DateTime.TryParse(expiresToken, out expires)))
                {
                    if (expires < DateTime.Now) //Token is expired, let's refresh
                    {
                        var client = new HttpClient();
                        var tokenResult = client.RequestRefreshTokenAsync(new RefreshTokenRequest
                        {
                            Address = sams_endpoint_token,
                            ClientId = sams_client_secret,
                            ClientSecret = sams_client_secret,
                            RefreshToken = refreshToken
                        }).Result;


                        if (!tokenResult.IsError)
                        {
                            var oldIdToken = idToken;
                            var newAccessToken = tokenResult.AccessToken;
                            var newRefreshToken = tokenResult.RefreshToken;
                            idToken = tokenResult.IdentityToken;

                            var tokens = new List<AuthenticationToken>
                {
                    new AuthenticationToken {Name = OpenIdConnectParameterNames.IdToken, Value = tokenResult.IdentityToken},
                    new AuthenticationToken
                    {
                        Name = OpenIdConnectParameterNames.AccessToken,
                        Value = newAccessToken
                    },
                    new AuthenticationToken
                    {
                        Name = OpenIdConnectParameterNames.RefreshToken,
                        Value = newRefreshToken
                    }
                };

                            var expiresAt = DateTime.Now + TimeSpan.FromSeconds(tokenResult.ExpiresIn);
                            tokens.Add(new AuthenticationToken
                            {
                                Name = "expires_at",
                                Value = expiresAt.ToString("o", CultureInfo.InvariantCulture)
                            });

                            var result = await context.AuthenticateAsync();
                            result.Properties.StoreTokens(tokens);

                            await context.SignInAsync(result.Principal, result.Properties);
                        }
                    }
                }
                await next.Invoke();

            });

            app.UseAuthentication();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                   name: "default",
                   pattern: "{controller=Authorization}/{action=signin}/");               
                // endpoints.MapDefaultControllerRoute().RequireAuthorization();
            });           
            app.UseExceptionHandler(a => a.Run(async context =>
            {
                var exceptionHandlerPathFeature = context.Features.Get<IExceptionHandlerPathFeature>();
                var exception = exceptionHandlerPathFeature.Error;

                var result = JsonConvert.SerializeObject(new { error = exception.Message });
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync(result);
            }));
        }

        private static bool IsApiRequest(HttpRequest request)
        {
            return request.Path.StartsWithSegments(new PathString("/api"));
        }       
        private CookieAuthenticationEvents getsmasevents()
        {
            //var sams_client_id = Configuration["sams:client_id"];
            //var sams_client_secret = Configuration["sams:client_secret"];
            var sams_client_id = Environment.GetEnvironmentVariable("ClientId");
            var sams_client_secret = Environment.GetEnvironmentVariable("ClientSecret");
            var sams_endpoint_token = Configuration["sams:endpoint_token"];
            var result = new CookieAuthenticationEvents
            {
                //OnRedirectToLogin = ctx =>
                // {
                //    // if it is an ajax/api request, don't redirect
                //    // to login page.
                //    if (!(IsApiRequest(ctx.Request)))
                //     {
                //         ctx.Response.Redirect(ctx.RedirectUri);
                //         return Task.CompletedTask;
                //     }
                //     if (ctx.Request.Headers["Authorization"].Count>0)
                //     {
                //        var authheader = ctx.Request.Headers["Authorization"];
                //        if(authheader.ToString().Contains("Bearer"))
                //        {
                //            var access_token=authheader.ToString().Substring(7);
                //             var tokenClient = new TokenClient(Configuration);
                //             var tokenResponse = tokenClient.ValidateToken(access_token).Result;
                //             if(tokenResponse.ToString()=="True")
                //                 return Task.CompletedTask;
                //         }
                //     }                    
                //         ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
                //         return ctx.Response.WriteAsync("Unauthorized");

                // },
                OnValidatePrincipal = context =>
                {
                    if (context.Principal.Identity.IsAuthenticated)
                    {
                        //get the users tokens
                        var tokens = context.Properties.GetTokens();
                        var refreshToken = tokens.FirstOrDefault(t => t.Name == "refresh_token");
                        var accessToken = tokens.FirstOrDefault(t => t.Name == "access_token");
                        var exp = context.Request.Cookies["expires_at"];// tokens.FirstOrDefault(t => t.Name == "expires_at");
                        if (exp != null)
                        {
                            var expires = DateTimeOffset.Parse(exp);
                            //check to see if the token has expired
                            //if (expires < DateTime.Now)
                            //{
                            //    //token is expired, let's attempt to renew                          
                            //    var tokenClient = new Utils.TokenClient(Configuration);
                            //    var tokenResponse = tokenClient.get_refresh_token(accessToken.Value, refreshToken.Value).Result;
                            //    //check for error while renewing - any error will trigger a new login.
                            //    if (tokenResponse.is_error)
                            //    {
                            //        //reject Principal
                            //        context.RejectPrincipal();
                            //        return Task.CompletedTask;
                            //    }
                            //    //set new token values
                            //    refreshToken.Value = tokenResponse.refresh_token;
                            //    accessToken.Value = tokenResponse.access_token;
                            //    //set new expiration date
                            //    var newExpires = DateTime.UtcNow + TimeSpan.FromSeconds(tokenResponse.expires_in);
                            //    exp = newExpires.ToString("o", CultureInfo.InvariantCulture);
                            //    //set tokens in auth properties 
                            //    context.Properties.StoreTokens(tokens);
                            //    //trigger context to renew cookie with new token values
                            //    context.ShouldRenew = true;
                            //    return Task.CompletedTask;
                            //}
                            return Task.CompletedTask;
                        }
                    }
                    context.RejectPrincipal();
                    return Task.CompletedTask;
                }
            };
            return result;

        }
    }
}
