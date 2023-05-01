﻿using Epi.Web.Common.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace epiwebsurvey_api.Utils
{
    public class TokenClient
    {
        private IConfiguration _configuration;
        string sams_endpoint_authorization;
        string sams_endpoint_token;
        string sams_endpoint_user_info;
        string sams_endpoint_token_validation;
        string sams_endpoint_user_info_sys;
        string sams_client_id;
        string sams_callback_url;
        string issuer;
        string sams_client_secret;

        public TokenClient(IConfiguration configuration)
        {
            _configuration = configuration;

            //sams_endpoint_authorization = _configuration["sams:endpoint_authorization"];
            //sams_endpoint_token = _configuration["sams:endpoint_token"];
            //sams_endpoint_user_info = _configuration["sams:endpoint_user_info"];
            //sams_endpoint_token_validation = _configuration["sams:endpoint_token_validation"];
            //sams_endpoint_user_info_sys = _configuration["sams:endpoint_user_info"];
            //sams_client_id = _configuration["sams:client_id"];
            //sams_callback_url = HttpUtility.UrlDecode(Environment.GetEnvironmentVariable("callback_url"));
            sams_client_id = Environment.GetEnvironmentVariable("client_id");
             sams_client_secret = Environment.GetEnvironmentVariable("client_secret");
             sams_endpoint_authorization = Environment.GetEnvironmentVariable("endpoint_authorization");
             sams_endpoint_token = Environment.GetEnvironmentVariable("endpoint_token");
             sams_endpoint_user_info = Environment.GetEnvironmentVariable("endpoint_user_info");
             sams_endpoint_token_validation = Environment.GetEnvironmentVariable("endpoint_token_validation");
             sams_endpoint_user_info_sys = Environment.GetEnvironmentVariable("endpoint_user_info_sys");
             sams_callback_url = HttpUtility.UrlDecode(Environment.GetEnvironmentVariable("callback_url"));            
             issuer= _configuration["sams:issuer"];
        }

        public async Task<(bool IsValidToken, string userid)> ValidateToken(string accessToken)
        {
            string userid = ""; bool IsValidToken = false;                                   
            HttpClient client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Post, sams_endpoint_token_validation);
            request.Content = new FormUrlEncodedContent(new Dictionary<string, string> {
                { "access_token", accessToken} });
            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();
            var payload = JObject.Parse(await response.Content.ReadAsStringAsync());
            if (payload.Value<string>("status") == "ok")
            {
                IsValidToken = true;
                var user_info_sys_request = new HttpRequestMessage(HttpMethod.Post, sams_endpoint_user_info);// + "?token=" + id_token);
                user_info_sys_request.Headers.Add("Authorization", "Bearer " + accessToken);
                user_info_sys_request.Headers.Add("client_id", sams_client_id);
                user_info_sys_request.Headers.Add("client_secret", sams_client_secret);
                response = await client.SendAsync(user_info_sys_request);
                response.EnsureSuccessStatusCode();
                var temp_string = await response.Content.ReadAsStringAsync();
                payload = JObject.Parse(temp_string);
                userid = payload.Value<string>("email").Substring(0, payload.Value<string>("email").IndexOf('@'));
            }         
            return (IsValidToken, userid);
        }
        public async Task<(string id_token, string access_token)> Get_Id_And_Access_Token
        (
            string code,
            string state
        )
        {

            System.Diagnostics.Debug.WriteLine($"code: {code}");
            System.Diagnostics.Debug.WriteLine($"state: {state}");

            HttpClient client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Post, sams_endpoint_token);


            /*
            request.Content = new FormUrlEncodedContent(new Dictionary<string, string> {
                { "client_id", sams_client_id },
                { "client_secret", sams_client_secret },
                { "grant_type", "client_credentials" },
                { "code", code },
            });
             */

            request.Content = new FormUrlEncodedContent(new Dictionary<string, string> {
                { "client_id", sams_client_id },
                { "client_secret", sams_client_secret },
                { "grant_type", "authorization_code" },
                { "code", code },
                { "scope", "openid profile email"},
                {"redirect_uri", sams_callback_url }
            });


            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var payload = JObject.Parse(await response.Content.ReadAsStringAsync());
            var access_token = payload.Value<string>("access_token");
            var scope = payload.Value<string>("scope");



            var id_token = payload.Value<string>("id_token");

            return (id_token, access_token);

            /*
                        var id_array = id_token.Split('.');


                        var replaced_value = id_array[1].Replace('-', '+').Replace('_', '/');
                        var base64 = replaced_value.PadRight(replaced_value.Length + (4 - replaced_value.Length % 4) % 4, '=');


                        var id_0 = DecodeToken(id_array[0]);
                        var id_1 = DecodeToken(id_array[1]);

                        var id_body = Base64Decode(base64);

                        var user_info_sys_request = new HttpRequestMessage(HttpMethod.Post, sams_endpoint_user_info + "?token=" + id_token);


                        user_info_sys_request.Headers.Add("Authorization","Bearer " + access_token); 
                        user_info_sys_request.Headers.Add("client_id", sams_client_id); 
                        user_info_sys_request.Headers.Add("client_secret", sams_client_secret); 
             */
            /* 
            user_info_sys_request.Content = new FormUrlEncodedContent(new Dictionary<string, string> {
                { "client_id", sams_client_id },
                { "client_secret", sams_client_secret },
                { "grant_type", "client_credentials" },
                { "scope", scope },
            });
            */


            /*
                        response = await client.SendAsync(user_info_sys_request);

                        return response;

                        response.EnsureSuccessStatusCode();

                        var temp_string = await response.Content.ReadAsStringAsync();
                        payload = JObject.Parse(temp_string);


                        var email = payload.Value<string>("email");
                         */

            /*
                        //check if user exists
                        var config_couchdb_url = _configuration["mmria_settings:couchdb_url"];
                        var config_timer_user_name = _configuration["mmria_settings:timer_user_name"];
                        var config_timer_password = _configuration["mmria_settings:timer_password"];

                        mmria.common.model.couchdb.user user = null;
                        try
                        {
                            string request_string = config_couchdb_url + "/_users/" + System.Web.HttpUtility.HtmlEncode("org.couchdb.user:" + email.ToLower());
                            var user_curl = new mmria.server.cURL("GET", null, request_string, null, config_timer_user_name, config_timer_password);
                            var responseFromServer = await user_curl.executeAsync();

                            user = Newtonsoft.Json.JsonConvert.DeserializeObject<mmria.common.model.couchdb.user>(responseFromServer);
                        }
                        catch(Exception ex)
                        {
                            Console.WriteLine (ex);

                        } 

                        mmria.common.model.couchdb.document_put_response user_save_result = null;

                        if(user == null)// if user does NOT exists create user with email
                        {
                            user = add_new_user(email.ToLower(), Guid.NewGuid().ToString());

                            try
                            {
                                Newtonsoft.Json.JsonSerializerSettings settings = new Newtonsoft.Json.JsonSerializerSettings ();
                                settings.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore;
                                var object_string = Newtonsoft.Json.JsonConvert.SerializeObject(user, settings);

                                string user_db_url = config_couchdb_url + "/_users/"  + user._id;

                                var user_curl = new mmria.server.cURL("PUT", null, user_db_url, object_string, config_timer_user_name, config_timer_password);
                                var responseFromServer = await user_curl.executeAsync();
                                user_save_result = Newtonsoft.Json.JsonConvert.DeserializeObject<mmria.common.model.couchdb.document_put_response>(responseFromServer);

                            }
                            catch(Exception ex) 
                            {
                                Console.WriteLine (ex);
                            }
                        }
                         */
        }

        public async Task<string> get_user_info_sys_request
        (
            string id_token,
            string access_token
        )
        {
            HttpClient client = new HttpClient();
            var user_info_sys_request = new HttpRequestMessage(HttpMethod.Post, sams_endpoint_user_info + "?token=" + id_token);


            user_info_sys_request.Headers.Add("Authorization", "Bearer " + access_token);
            user_info_sys_request.Headers.Add("client_id", sams_client_id);
            user_info_sys_request.Headers.Add("client_secret", sams_client_secret);

            /* 
            user_info_sys_request.Content = new FormUrlEncodedContent(new Dictionary<string, string> {
                { "client_id", sams_client_id },
                { "client_secret", sams_client_secret },
                { "grant_type", "client_credentials" },
                { "scope", scope },
            });
            */



            var response = await client.SendAsync(user_info_sys_request);

            //return response;

            response.EnsureSuccessStatusCode();

            var temp_string = await response.Content.ReadAsStringAsync();

            return temp_string;
        }



        public struct refresh_token_struct
        {
            public string access_token;
            public int expires_in;
            public string refresh_token;
            public string jwt_token;

            public bool is_error;
        }

        public async Task<refresh_token_struct> get_refresh_token
        (
            string p_access_token,
            string p_refresh_token,
            string p_userid,
            string p_emailid
        )
        {
            /*
            https://auth0.com/learn/refresh-tokens/

            curl -X POST -H 'Authorization: Basic dGVzdGNsaWVudDpzZWNyZXQ=' -d 'refresh_token=fdb8fdbecf1d03ce5e6125c067733c0d51de209c&grant_type=refresh_token' localhost:3000/oauth/token

            {
                "token_type":"bearer",
                "access_token":"eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJ1c2VyIjoiVlx1MDAxNcKbwoNUwoonbFPCu8KhwrYiLCJpYXQiOjE0NDQyNjI4NjYsImV4cCI6MTQ0NDI2Mjg4Nn0.Dww7TC-d0teDAgsmKHw7bhF2THNichsE6rVJq9xu_2s",
                "expires_in":20,
                "refresh_token":"7fd15938c823cf58e78019bea2af142f9449696a"
            }
             */
            refresh_token_struct result = new refresh_token_struct();

            HttpClient client = new HttpClient();
            var user_refresh_request = new HttpRequestMessage(HttpMethod.Post, sams_endpoint_token);
            user_refresh_request.Content = new FormUrlEncodedContent(new Dictionary<string, string> {
                { "client_id", sams_client_id },
                { "client_secret", sams_client_secret },
                { "grant_type", "refresh_token" },
                { "refresh_token", p_refresh_token }
            });           

            var response = await client.SendAsync(user_refresh_request);
            if (response.IsSuccessStatusCode)
            {                
                var payload = JObject.Parse(await response.Content.ReadAsStringAsync());
                string access_token = Cryptography.Encrypt(payload.Value<string>("access_token"));
                var expires_in = payload.Value<int>("expires_in");
                var unix_time = DateTimeOffset.UtcNow.AddSeconds(expires_in);
                string refresh_token = Cryptography.Encrypt(payload.Value<string>("refresh_token"));                               


                var claims = new List<Claim>();
                claims.Add(new Claim(ClaimTypes.Name, p_userid, ClaimValueTypes.String, issuer));
                claims.Add(new Claim(ClaimTypes.Email, p_emailid, ClaimValueTypes.String, issuer));//email
                claims.Add(new Claim("refresh_token", refresh_token, ClaimValueTypes.String, issuer));//refreshtoken
                claims.Add(new Claim(ClaimTypes.Expiration, unix_time.ToLocalTime().ToString(), ClaimValueTypes.DateTime, issuer));//expires
                claims.Add(new Claim("access_token", access_token, ClaimValueTypes.String, issuer));//accesstoken
                string token= GenerateJwtToken(claims);
                result.access_token = payload.Value<string>("access_token");
                result.expires_in = payload.Value<int>("expires_in");
                result.refresh_token = payload.Value<string>("refresh_token");
                result.jwt_token = token;
            }
            else
            {
                result.is_error = true;
            }

            return result;


            /*
            https://auth0.com/learn/refresh-tokens/

            curl -X POST -H 'Authorization: Basic dGVzdGNsaWVudDpzZWNyZXQ=' -d 'refresh_token=fdb8fdbecf1d03ce5e6125c067733c0d51de209c&grant_type=refresh_token' localhost:3000/oauth/token

            {
                "token_type":"bearer",
                "access_token":"eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJ1c2VyIjoiVlx1MDAxNcKbwoNUwoonbFPCu8KhwrYiLCJpYXQiOjE0NDQyNjI4NjYsImV4cCI6MTQ0NDI2Mjg4Nn0.Dww7TC-d0teDAgsmKHw7bhF2THNichsE6rVJq9xu_2s",
                "expires_in":20,
                "refresh_token":"7fd15938c823cf58e78019bea2af142f9449696a"
            }
             */

        }

        public JwtSecurityToken DecodeJwtToken(string jwtToken)
        {
            try
            {
                var handler = new JwtSecurityTokenHandler();
                return handler.ReadJwtToken(jwtToken);
            }
            catch(Exception ex)
            {
                // throw new ArgumentException(ex.Message);
                return null;
            }
        }
        public string GenerateJwtToken(List<Claim> claims)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["sams:Secret"]);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                //Subject = new ClaimsIdentity(new Claim[]
                //{
                //    //new Claim(ClaimTypes.Name, user.Id.ToString())
                //    claims
                //}),
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
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
