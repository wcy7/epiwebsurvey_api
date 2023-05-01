using System;
using System.Collections.Generic;
using System.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Epi.Web.Common.Security;
using epiwebsurvey_api.Properties;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;

namespace epiwebsurvey_api.Controllers
{
    [AllowAnonymous]
    [EnableCors("AllowOrigin")]   
    public class AuthorizationController : Controller
    {
        private IConfiguration _configuration;
        private IHttpContextAccessor _accessor;
        private HttpContext hcontext;
        private static string _returnURL;
        private readonly EnvironmentConfig _envconfiguration;

        public AuthorizationController(IHttpContextAccessor httpContextAccessor,  IConfiguration configuration, IOptions<EnvironmentConfig> EnvconfigurationEnv)
        {
            _accessor = httpContextAccessor;           
            _configuration = configuration;
             hcontext= _accessor.HttpContext;
            _envconfiguration = EnvconfigurationEnv.Value;                     
        }
        [AllowAnonymous]        
        public async Task<ActionResult> callback(string returnurl = "/")
        {
            // var sams_endpoint_authorization = _configuration["sams:endpoint_authorization"];
            // var sams_endpoint_token = _configuration["sams:endpoint_token"];
            // var sams_endpoint_user_info = _configuration["sams:endpoint_user_info"];
            // var sams_endpoint_token_validation = _configuration["sams:endpoint_token_validation"];
            // var sams_endpoint_user_info_sys = _configuration["sams:endpoint_user_info_sys"];
            //var sams_client_id = _configuration["sams:client_id"];
            //var sams_client_secret = _configuration["sams:client_secret"];(Environment.GetEnvironmentVariable("callback_url")
            var sams_client_id = _envconfiguration.client_id;
            var sams_client_secret = _envconfiguration.client_secret;
            var sams_endpoint_authorization = _envconfiguration.endpoint_authorization;
            var sams_endpoint_token = _envconfiguration.endpoint_token;
            var sams_endpoint_user_info = _envconfiguration.endpoint_user_info;
            var sams_endpoint_token_validation = _envconfiguration.endpoint_token_validation;
            var sams_endpoint_user_info_sys = _envconfiguration.endpoint_user_info_sys;
            var sams_callback_url = HttpUtility.UrlDecode(Environment.GetEnvironmentVariable("callback_url"));
            var issuer = _configuration["sams:issuer"];
            //?code=6c17b2a3-d65a-44fd-a28c-9aee982f80be&state=a4c8326ca5574999aa13ca02e9384c3d
            // Retrieve code and state from query string, pring for debugging
            if (!string.IsNullOrEmpty(Request.QueryString.Value))
            {
                var querystring = Request.QueryString.Value;
                var querystring_skip = querystring.Substring(1, querystring.Length - 1);
                var querystring_array = querystring_skip.Split("&");
                var querystring_dictionary = new Dictionary<string, string>();
                foreach (string item in querystring_array)
                {
                    var pair = item.Split("=");
                    querystring_dictionary.Add(pair[0], pair[1]);
                }
                var code1 = querystring_dictionary["code"];
                var state1 = querystring_dictionary["state"];
                System.Diagnostics.Debug.WriteLine($"code: {code1}");
                System.Diagnostics.Debug.WriteLine($"state: {state1}");
                HttpClient client = new HttpClient();
                var request = new HttpRequestMessage(HttpMethod.Post, sams_endpoint_token);
                request.Content = new FormUrlEncodedContent(new Dictionary<string, string> {
                { "client_id", sams_client_id },
                { "client_secret", sams_client_secret },
                { "grant_type", "authorization_code" },
                { "code", code1 },
                { "scope", "openid profile email"},
               // {"redirect_uri", "https://localhost:5001/authorization/callback" }
                {"redirect_uri",sams_callback_url }
            });

                var response = await client.SendAsync(request);
                response.EnsureSuccessStatusCode();
                var payload = JObject.Parse(await response.Content.ReadAsStringAsync());
                var access_token = payload.Value<string>("access_token");
                var refresh_token = payload.Value<string>("refresh_token");
                var expires_in= payload.Value<int>("expires_in");
                var unixtime= DateTimeOffset.UtcNow.AddSeconds(expires_in);
                var session_data = new System.Collections.Generic.Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
                session_data["access_token"] = access_token;
                session_data["refresh_token"] = refresh_token;
                session_data["expires_at"] = expires_in.ToString();
                var scope = payload.Value<string>("scope");
                HttpContext.Session.SetString("access_token", access_token);
                HttpContext.Session.SetString("refresh_token", refresh_token);
                var unix_time = DateTimeOffset.UtcNow.AddSeconds(expires_in);
                HttpContext.Session.SetString("expires_at", unix_time.ToString());               
                var id_token = payload.Value<string>("id_token");                
                var id_array = id_token.Split('.');
                var replaced_value = id_array[1].Replace('-', '+').Replace('_', '/');
                var base64 = replaced_value.PadRight(replaced_value.Length + (4 - replaced_value.Length % 4) % 4, '=');
                var id_0 = DecodeToken(id_array[0]);
                var id_1 = DecodeToken(id_array[1]);
                var id_body = Base64Decode(base64);
                var user_info_sys_request = new HttpRequestMessage(HttpMethod.Post, sams_endpoint_user_info + "?token=" + id_token);
                user_info_sys_request.Headers.Add("Authorization", "Bearer " + access_token);
                user_info_sys_request.Headers.Add("client_id", sams_client_id);
                user_info_sys_request.Headers.Add("client_secret", sams_client_secret);            
                response = await client.SendAsync(user_info_sys_request);
                response.EnsureSuccessStatusCode();
                var temp_string = await response.Content.ReadAsStringAsync();
                payload = JObject.Parse(temp_string);
                var email = payload.Value<string>("email");
                await create_user_principal(email.Substring(0, email.IndexOf('@')), email,unixtime.DateTime);
                // Response.Cookies.Append("expires_at", unix_time.ToString(), new CookieOptions { HttpOnly = false });
                //Response.Cookies.Append("refresh_token", refresh_token, new CookieOptions { HttpOnly = false });
                Response.Cookies.Append("expires_at", unix_time.ToString(), new CookieOptions { HttpOnly = true });                 
                var claims = new List<Claim>();
                claims.Add(new Claim(ClaimTypes.Name, email.Substring(0, email.IndexOf('@')), ClaimValueTypes.String, issuer));
                claims.Add(new Claim(ClaimTypes.Email, email, ClaimValueTypes.String, issuer));//email
                claims.Add(new Claim("refresh_token", Cryptography.Encrypt(refresh_token), ClaimValueTypes.String, issuer));//refreshtoken
                claims.Add(new Claim(ClaimTypes.Expiration, unix_time.ToLocalTime().ToString(), ClaimValueTypes.DateTime, issuer));//expires
                claims.Add(new Claim("access_token", Cryptography.Encrypt(access_token), ClaimValueTypes.String, issuer));//accesstoken
                var tokenClient = new Utils.TokenClient(_configuration);
                string jwttoken = tokenClient.GenerateJwtToken(claims);
                Response.Cookies.Append("authToken", jwttoken, new CookieOptions { HttpOnly = false });
                if (string.IsNullOrEmpty(_returnURL))
                    return new JsonResult(jwttoken);
                else
                {
                    string url = _returnURL;
                    _returnURL = null;
                    return Redirect(url + "?token=" + jwttoken);
                }
            }
            else
            {
                var state1 = Guid.NewGuid().ToString("N");
                var nonce = Guid.NewGuid().ToString("N");
                var sams_url = $"{sams_endpoint_authorization}?" +
                    "&client_id=" + sams_client_id +
                     //"&prompt=select_account" +
                     // "&redirect_uri=" + $"https://localhost:5001/authorization/callback" +
                     "&redirect_uri=" + HttpUtility.UrlDecode(Environment.GetEnvironmentVariable("callback_url")) +
                    "&response_type=code" +
                   "&scope=" + System.Web.HttpUtility.HtmlDecode("openid profile email") +
                    "&state=" + state1 +
                    "&nonce=" + nonce;
                System.Diagnostics.Debug.WriteLine($"url: {sams_url}");

                return Redirect(sams_url);
            }
           
        }                
        public async Task create_user_principal(string user_name,string user_email,DateTime p_session_expire_date_time)
        {
            string Issuer = _configuration["sams:issuer"];
            var claims = new List<Claim>();
            claims.Add(new Claim(ClaimTypes.Name, user_name, ClaimValueTypes.String, Issuer));
            claims.Add(new Claim(ClaimTypes.Email, user_email, ClaimValueTypes.String, Issuer));
            var userIdentity = new ClaimsIdentity("SuperSecureLogin");
            userIdentity.AddClaims(claims);
            var userPrincipal = new ClaimsPrincipal(userIdentity);            
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                userPrincipal,
                new AuthenticationProperties
                {
                    ExpiresUtc = p_session_expire_date_time,
                    IsPersistent = false,
                    AllowRefresh = true,
                });
        }
        [AllowAnonymous]
        public  ActionResult SignIn(string returnurl="/")
        {
            if (returnurl!="/")
                _returnURL = returnurl;
            //var sams_endpoint_authorization = _configuration["sams:endpoint_authorization"];
            //var sams_endpoint_token = _configuration["sams:endpoint_token"];
            //var sams_endpoint_user_info = _configuration["sams:endpoint_user_info"];
            //var sams_endpoint_token_validation = _configuration["sams:endpoint_token_validation"];
            //var sams_endpoint_user_info_sys = _configuration["sams:endpoint_user_info_sys"];
            //var sams_client_id = _configuration["sams:client_id"];
            //var sams_client_secret = _configuration["sams:client_secret"];
            var sams_client_id = _envconfiguration.client_id;
            var sams_client_secret = _envconfiguration.client_secret;
            var sams_endpoint_authorization = _envconfiguration.endpoint_authorization;
            var sams_endpoint_token = _envconfiguration.endpoint_token;
            var sams_endpoint_user_info = _envconfiguration.endpoint_user_info;
            var sams_endpoint_token_validation = _envconfiguration.endpoint_token_validation;
            var sams_endpoint_user_info_sys = _envconfiguration.endpoint_user_info_sys;
            var sams_callback_url = HttpUtility.UrlDecode(Environment.GetEnvironmentVariable("callback_url"));
            var state = Guid.NewGuid().ToString("N");
            var nonce = Guid.NewGuid().ToString("N");
            var sams_url = $"{sams_endpoint_authorization}?" +
                "&client_id=" + sams_client_id +
                  // "&redirect_uri=" + $"https://localhost:5001/authorization/callback" +
                  "&redirect_uri=" + HttpUtility.UrlDecode(Environment.GetEnvironmentVariable("callback_url")) +
                "&response_type=code" +
                "&scope=" + System.Web.HttpUtility.HtmlEncode("openid profile email") +
                "&state=" + state +
                "&nonce=" + nonce;
            System.Diagnostics.Debug.WriteLine($"url: {sams_url}");
            return Redirect(sams_url);
        }
      
        
		/// <summary>
        /// Generate new token  
        /// </summary>  
         [Route("refreshtoken")]
        /// /// <remarks>               
        /// Sample request:        
        /// RefreshToken /refreshtoken  
        /// {        
        ///    "JWT": "184c49fe56ba20iygf56k88f8iwcbfdck81f799yetqeeioieqe7vnjtt8da5rhwsfldfigerytqev638dbe6w3883nfmmvikvf9rcnhuf8eoekdm"          
        ///  }
        /// </remarks>
         /// <returns>JWT Token</returns> 
        /// <response >JWT Token</response>
        /// <response >Error message when Token is invalid</response
        
		[HttpGet]

       
        public string RefreshToken(string jwtToken)
        {
            if (!string.IsNullOrEmpty(jwtToken))
            {
                try
                {
                    var tokenClient = new Utils.TokenClient(_configuration);
                    var token = tokenClient.DecodeJwtToken(jwtToken);
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

                    var token_response = tokenClient.get_refresh_token(acessToken, refreshToken, Username, useremail).Result;
                    if (token_response.is_error)
                    {
                        return "Token validity expired.Please Authenticate.";
                    }
                  //  return new JsonResult((new Dictionary<string, object>{ { "Token", token_response.jwt_token }}));
                    return  token_response.jwt_token;
                }
                catch(Exception ex)
                {
                    return  "Invalid token.Please Authenticate";                    
                }
            }
            return "Error generating token.Please Authenticate.";            

        }
        [AllowAnonymous]
        public  ActionResult Error(string message)        
        
        {
            return new JsonResult(new Dictionary<string, object>
                  {
                    { "error", message}                   
                  });
        }
        private string DecodeToken(string p_value)
        {
            var replaced_value = p_value.Replace('-', '+').Replace('_', '/');
            var base64 = replaced_value.PadRight(replaced_value.Length + (4 - replaced_value.Length % 4) % 4, '=');
            return Base64Decode(base64);
        }
        private string Base64Decode(string base64EncodedData)
        {
            var base64EncodedBytes = System.Convert.FromBase64String(base64EncodedData);
            return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
        }
    }
}