using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using Epi.Web.Common.BusinessObject;
using Epi.Web.Common.Message;
using  epiwebsurvey_api.Context;
using  epiwebsurvey_api.Models;
using  epiwebsurvey_api.Repositories;
using  epiwebsurvey_api.Repositories.Interfaces;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace epiwebsurvey_api.Controllers
{
     [Authorize]    
   // [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    //[Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme)]
    // [Authorize(AuthenticationSchemes = "Bearer,Cookies")]   
    [Route("api/[controller]")]
    [ApiController]
    [EnableCors("AllowOrigin")]
    public class SurveyResponseController : ControllerBase
    {

        private ISurveyResponseRepository _isurveyAnswerRepository;
        private HttpContext hcontext;
        private IConfiguration _configuration;
        private string username = "";

        public SurveyResponseController(ISurveyResponseRepository isurvyeyAnswerepository, IHttpContextAccessor haccess, IConfiguration configuration)
        {
            _isurveyAnswerRepository = isurvyeyAnswerepository;
            hcontext = haccess.HttpContext;
            _configuration = configuration;
        }

        /// <summary>
        /// Query the responses for a given SurveyId after successful Authentication
        /// </summary>    

         /// <returns>HTTPRespose code with json</returns> 
         /// <response code="200">Ok</response>
        /// <response code="415">UnsupportedMediaType- SurveyId is invalid or does not exist</response>
        /// <response code="401"> Unauthorized- invalid token</response>
        [HttpGet]
        [Produces("application/json")]
        public IActionResult Get()
        {
            string surveyid = Request.Headers["SurveyId"];
            string json = ""; var username = string.Empty;
            Guid id = Guid.Empty;
            ContentResult content = new ContentResult();
            try
            {                
                ClaimsPrincipal cp = hcontext.User;               
                if (HttpContext.User.Identity is ClaimsIdentity identity)
                {
                    username = identity.Name.ToLower();
                    Program.userPrincipal = username.ToLower();
                }
                username = Program.userPrincipal;
                id = new Guid(surveyid);
            }
            catch(Exception ex)
            {
                //return  Content(HttpStatusCode.UnsupportedMediaType + Environment.NewLine+ex.Message.ToString());
                content.StatusCode = 415;
                content.Content = HttpStatusCode.UnsupportedMediaType.ToString() + Environment.NewLine + ex.Message.ToString();
                return content;
            }
            if(string.IsNullOrEmpty(Program.userPrincipal) ||id==null )
            {
                //  return   Content(HttpStatusCode.UnsupportedMediaType + Environment.NewLine+"inValid SurveyId/User"); 
                content.StatusCode = 415;
                content.Content = HttpStatusCode.UnsupportedMediaType.ToString() + Environment.NewLine + "inValid SurveyId/User";
                return content;
            }
            else
            {
                EntitySurveyResponseDao SurveyResponseDao = new EntitySurveyResponseDao();
                json = SurveyResponseDao.GetSurveyResponseByUser(surveyid, username);

            }
            // return Content(HttpStatusCode.OK + Environment.NewLine + json);  
            content.StatusCode = 200;
            content.Content = HttpStatusCode.OK.ToString() + Environment.NewLine + json;
            return content;
        }

        /// <summary>
        /// Query the responses for a given SurveyId and given key value pair after successful Authentication
        /// </summary>    
        /// /// <remarks>               
        /// Sample request:        
        /// GetFilteredResponse api/SurveyResponse  
        /// {               
        ///       "FirstName": "John",  
        ///       "LastName":"Keifer"
        ///  }
        /// </remarks>
        /// <returns>HTTPRespose code with json</returns> 
        /// <response code="200">Ok</response>
        /// <response code="415">UnsupportedMediaType- SurveyId is invalid or does not exist or JsonKeyval is in incorrect format</response>
        /// <response code="401"> Unauthorized- invalid token</response>
        [HttpGet]
        [Route("[action]")]
        [Produces("application/json")]
        public IActionResult GetFilteredResponse(string JsonKeyval)
        {
            string surveyid = Request.Headers["SurveyId"];
            string json = ""; var username = string.Empty;
            Guid id = Guid.Empty;
            ContentResult content = new ContentResult();
            Dictionary<string, string> keyvalupair = new Dictionary<string, string>();
            var value = JsonKeyval;
            try
            {
                ClaimsPrincipal cp = hcontext.User;
                if (HttpContext.User.Identity is ClaimsIdentity identity)
                {
                    username = identity.Name.ToLower();
                    Program.userPrincipal = username.ToLower();
                }
                username = Program.userPrincipal;
                id = new Guid(surveyid);
            }
            catch (Exception ex)
            {
                //return  Content(HttpStatusCode.UnsupportedMediaType + Environment.NewLine+ex.Message.ToString());
                content.StatusCode = 415;
                content.Content = HttpStatusCode.UnsupportedMediaType.ToString() + Environment.NewLine + ex.Message.ToString();
                return content;
            }
            if (string.IsNullOrEmpty(Program.userPrincipal) || id == null)
            {
                //  return   Content(HttpStatusCode.UnsupportedMediaType + Environment.NewLine+"inValid SurveyId/User"); 
                content.StatusCode = 415;
                content.Content = HttpStatusCode.UnsupportedMediaType.ToString() + Environment.NewLine + "inValid SurveyId/User";
                return content;
            }
            else
            {
                var settings = new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    MissingMemberHandling = MissingMemberHandling.Ignore
                };
                if (!string.IsNullOrEmpty(value))
                {
                    try
                    {
                        string val = value.Replace("\\", "");
                        keyvalupair = JsonConvert.DeserializeObject<Dictionary<string, string>>(val, settings);
                    }
                    catch (Exception ex)
                    {
                        //return Content(HttpStatusCode.UnsupportedMediaType.ToString()+"  "+ ex.Message.ToString());
                        content.StatusCode = 415;
                        content.Content = HttpStatusCode.UnsupportedMediaType.ToString() + "  " + ex.Message.ToString();
                        return content;
                    }
                }
                EntitySurveyResponseDao SurveyResponseDao = new EntitySurveyResponseDao();
                json = SurveyResponseDao.GetFilteredSurveyResponseByUser(surveyid, username, keyvalupair);

            }
            // return Content(HttpStatusCode.OK + Environment.NewLine + json);  
            content.StatusCode = 200;
            content.Content = HttpStatusCode.OK.ToString() + Environment.NewLine + json;
            return content;
        }

        /// <summary>
        /// Submit response after successful Authentication
        /// </summary>    
        /// /// <remarks>               
        /// Sample request:        
        /// Put api/SurveyResponse  
        /// {        
        ///       "id": "3BA54128-B3E2-681E-B1D1-2EB2EcB1453f",
        ///       "FirstName": "John",  
        ///       "LastName":"Keifer"
        ///  }
        /// </remarks>
        /// <returns>status code,ResponseId,Passcode</returns>         
        /// <response code="200">Ok- response created/updated</response>
        /// <response code="415">UnsupportedMediaType- SurveyId is invalid or does not exist or invalid json format</response>
        /// <response code="401"> Unauthorized- invalid token</response>
        [HttpPut]
        [Produces("application/json")]
        public IActionResult Put([FromBody] object json)
        { 
            Dictionary<string, string> keyvalupair = new Dictionary<string, string>();                      
            var value = json;
            ContentResult content = new ContentResult();
            string surveyid=Request.Headers["SurveyId"];
            var settings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                MissingMemberHandling = MissingMemberHandling.Ignore
            };
            try
            {
                keyvalupair = JsonConvert.DeserializeObject<Dictionary<string, string>>(value.ToString(), settings);
            }
            catch (Exception ex)
            {                
                //return Content(HttpStatusCode.UnsupportedMediaType.ToString()+"  "+ ex.Message.ToString());
                content.StatusCode = 415;
                content.Content = HttpStatusCode.UnsupportedMediaType.ToString() + "  " + ex.Message.ToString();
                return content;
            }
            string responseId; SurveyAnswerModel surveyanswerModel = new SurveyAnswerModel();
            try
            {
                SurveyInfoBO survyeinfo = _isurveyAnswerRepository.GetSurveyInfoById(surveyid);
                if (survyeinfo == null)
                {
                   // return Content(HttpStatusCode.UnsupportedMediaType.ToString() + "  "+"Response not generated");
                    content.StatusCode = 415;
                    content.Content = HttpStatusCode.UnsupportedMediaType.ToString() + "  " + "Response not generated";
                    return content;
                }               
                surveyanswerModel.SurveyId = new Guid(survyeinfo.SurveyId);
                surveyanswerModel.OrgKey = survyeinfo.OrganizationKey;
                surveyanswerModel.PublisherKey = survyeinfo.UserPublishKey;
            }
            catch (Exception ex)
            {
               // return Content(HttpStatusCode.UnsupportedMediaType.ToString() + "  "+ ex.Message.ToString());
                content.StatusCode = 415;
                content.Content = HttpStatusCode.UnsupportedMediaType.ToString() + "  " + ex.Message.ToString();
                return content;
            }                                  
                var item = keyvalupair.Where(x => x.Key.ToLower() == "responseid" || x.Key.ToLower() == "id").FirstOrDefault(); //  if (keyvalupair.TryGetValue("ResponseId", out ResponseId))
                if (item.Value != null)
                {
                    responseId = item.Value;
                    surveyanswerModel.SurveyQuestionAnswerListField = keyvalupair;
                    var Result = _isurveyAnswerRepository.Update(surveyanswerModel, responseId, json.ToString());
                    if (Result.SurveyResponseID != null)
                    {                        
                      //201.200 Created .Ok Success with response body.                                                
                         // return Content("Status:" + Result.Status + Environment.NewLine + "ResponseId:" + Result.SurveyResponseID + Environment.NewLine + "PassCode:" + Result.SurveyResponsePassCode);
                    content.StatusCode = 200;
                    content.Content = HttpStatusCode.OK.ToString() +"   "+ "Status:" + Result.Status + Environment.NewLine + "ResponseId:" + Result.SurveyResponseID + Environment.NewLine + "PassCode:" + Result.SurveyResponsePassCode;
                    return content;
                }
                    else
                    {
                    // The requested operation is not permitted for the user. This error can also be caused by ACL failures, or business rule or data policy constraints.
                    // return Content(HttpStatusCode.UnsupportedMediaType.ToString() + "  "+"Response not generated");
                    content.StatusCode = 415;
                    content.Content = HttpStatusCode.UnsupportedMediaType.ToString() + "  " + "Response not generated";
                    return content;

                    }
                }
                else
                {
                //415 Unsupported media type The endpoint does not support the format of the request body.             
                // return Content(HttpStatusCode.UnsupportedMediaType.ToString() + "  "+"Response not generated");
                content.StatusCode = 415;
                content.Content = HttpStatusCode.UnsupportedMediaType.ToString() + "  " + "Response not generated";
                return content;
            }                        
        }

        /// <summary>
        /// Delete response after successful Authentication
        /// </summary>    
        /// /// <remarks>               
        /// Sample request:        
        /// Delete api/SurveyResponse  
        /// {        
        ///    "ResponseId": "184c49fe-ba20-gf56-88f8-cbfdck81f799"          
        ///  }
        /// </remarks>
        /// <returns>response code</returns> 
        /// <response code="200">Ok- Response deleted</response>
        /// <response code="415">UnsupportedMediaType- SurveyId is invalid/ResponseId is invalid</response>
        /// <response code="403">Forbidden- ResponseId does not exist</response>
        /// <response code="401"> Unauthorized- invalid token</response>
        [HttpDelete("{ResponseId}")]        
        public IActionResult Delete(string ResponseId)
        {
            Dictionary<string, string> keyvalupair = new Dictionary<string, string>();
            var value = ResponseId;
            string surveyid = Request.Headers["SurveyId"];
            ContentResult content = new ContentResult();
            var settings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                MissingMemberHandling = MissingMemberHandling.Ignore
            };
            SurveyAnswerModel surveyanswerModel = new SurveyAnswerModel();
            try
            {
                SurveyInfoBO survyeinfo = _isurveyAnswerRepository.GetSurveyInfoById(surveyid);
                if (survyeinfo == null)
                {                                        
                    content.StatusCode = 415;
                    content.Content = HttpStatusCode.UnsupportedMediaType.ToString() + "  " + "SurveyId is invalid";
                    return content;                  
                    
                }
                surveyanswerModel.SurveyId = new Guid(survyeinfo.SurveyId);
                surveyanswerModel.OrgKey = survyeinfo.OrganizationKey;
                surveyanswerModel.PublisherKey = survyeinfo.UserPublishKey;
            }
            catch (Exception ex)
            {
                return Content(HttpStatusCode.UnsupportedMediaType.ToString() +"  "+ ex.Message.ToString());
            }
            if (value != null)
            {                
                try
                {
                    _isurveyAnswerRepository.Remove(value);
                }
                catch (Exception ex)
                {
                    //  return Content(HttpStatusCode.Forbidden.ToString()+"  "+"ResponseId does not exist");//The request has not succeeded. The information returned with the response is dependent on the method used in the request.                   
                    content.StatusCode = 403;
                    content.Content = HttpStatusCode.Forbidden.ToString() + "  " + "ResponseId does not exist";
                    return content;

                }
                //  return Content(HttpStatusCode.OK.ToString()+ Environment.NewLine + "Response Deleted.");//The request has succeeded. The information returned with the response is dependent on the method used in the request.               
                content.StatusCode = 200;
                content.Content = HttpStatusCode.OK.ToString() + "  " + "Response Deleted.";
                return content;

            }
            else
            {
                // return Content(HttpStatusCode.UnsupportedMediaType.ToString() + "  " + "ResponseId is null");//415 Unsupported media type The endpoint does not support the format of the request body. 
                content.StatusCode = 415;
                content.Content = HttpStatusCode.UnsupportedMediaType.ToString() + "  " + "ResponseId is null";
                return content;

            }
        }
    }
}
