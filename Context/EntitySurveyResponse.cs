using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Epi.Web.Common.BusinessObject;
using Epi.Web.Common.Criteria;
using  epiwebsurvey_api.Context.Entities;
using  epiwebsurvey_api.Models;
using Newtonsoft.Json;

namespace epiwebsurvey_api.Context
{
    public class EntitySurveyResponseDao : Epi.Web.BLL.DataInterfaces.ISurveyResponseDao
    {
        public string Connectionstring { get; set; }
        public IMapper Imapper { get; set; }

        /// Gets a specific SurveyResponse.
        /// </summary>
        /// <param name="SurveyResponseId">Unique SurveyResponse identifier.</param>
        /// <returns>SurveyResponse.</returns>
        public List<SurveyResponseBO> GetSurveyResponse(List<string> SurveyResponseIdList, Guid UserPublishKey, int PageNumber = -1, int PageSize = -1)
        {


            List<SurveyResponseBO> result = new List<SurveyResponseBO>();
             epiwebsurvey_apiContext ConnectionObj = new  epiwebsurvey_apiContext();
            try
            {
                using (var Context = ConnectionObj)
                {

                    if (SurveyResponseIdList.Count > 0)
                    {
                        foreach (string surveyResponseId in SurveyResponseIdList.Distinct())
                        {
                            Guid Id = new Guid(surveyResponseId);

                           // using (var Context = ConnectionObj)
                            //{

                                result.Add(Mapper.Map(Context.SurveyResponse.FirstOrDefault(x => x.ResponseId == Id)));
                           // }
                        }
                    }
                    else
                    {
                      //  using (var Context = ConnectionObj)
                        //{

                            result = Mapper.Map(Context.SurveyResponse.ToList());
                        //}
                    }

                    if (PageNumber > 0 && PageSize > 0)
                    {
                        result.Sort(CompareByDateCreated);
                        // remove the items to skip
                        if (PageNumber * PageSize - PageSize > 0)
                        {
                            result.RemoveRange(0, PageSize);
                        }

                        if (PageNumber * PageSize < result.Count)
                        {
                            result.RemoveRange(PageNumber * PageSize, result.Count - PageNumber * PageSize);
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                throw (ex);
            }


            return result;
        }
        /// <summary>
        /// Inserts a new SurveyResponse via API. 
        /// </summary>
        /// <remarks>
        /// Following insert, SurveyResponse object will contain the new identifier.
        /// </remarks>  
        /// <param name="SurveyResponse">SurveyResponse.</param>
        public void InsertSurveyResponseApi(SurveyResponseBO SurveyResponse)
        {
            try
            {
                 epiwebsurvey_apiContext ConnectionObj = new  epiwebsurvey_apiContext();
                using (var Context = ConnectionObj)
                {
                    Entities.SurveyResponse SurveyResponseEntity = Mapper.ToEF(SurveyResponse);                   

                    if (!string.IsNullOrEmpty(SurveyResponse.Json))
                    {
                        SurveyResponseEntity.ResponseXMLSize = RemoveWhitespace(SurveyResponse.Json).Length;
                    }
                    Context.SurveyResponse.Add(SurveyResponseEntity);
                    Context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                throw (ex);
            }

        }

        /// <summary>
        /// Inserts a new SurveyResponse. 
        /// </summary>
        /// <remarks>
        /// Following insert, SurveyResponse object will contain the new identifier.
        /// </remarks>  
        /// <param name="SurveyResponse">SurveyResponse.</param>
        public void InsertSurveyResponse(SurveyResponseBO SurveyResponse)
        {
            try
            {
                 epiwebsurvey_apiContext ConnectionObj = new  epiwebsurvey_apiContext();
                using (var Context = ConnectionObj)
                {
                    Entities.SurveyResponse SurveyResponseEntity = Mapper.ToEF(SurveyResponse);
                    try
                    {
                        SurveyResponseEntity.RecordSourceId = Context.LkRecordSource.Where(u => u.RecordSource == "EIWS").Select(u => u.RecordSourceId).SingleOrDefault();
                    }
                    catch (Exception)
                    {

                    }
                    Context.SurveyResponse.Add(SurveyResponseEntity);

                    Context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                throw (ex);
            }

        }

        /// <summary>
        /// Gets SurveyResponses per a SurveyId.
        /// </summary>
        /// <param name="SurveyResponseId">Unique SurveyResponse identifier.</param>
        /// <returns>SurveyResponse.</returns>
        public List<SurveyResponseBO> GetSurveyResponseBySurveyId(List<string> SurveyIdList, Guid UserPublishKey, int PageNumber = -1, int PageSize = -1)
        {

            List<SurveyResponseBO> result = new List<SurveyResponseBO>();

            try
            {
                 epiwebsurvey_apiContext ConnectionObj = new  epiwebsurvey_apiContext();
                foreach (string surveyResponseId in SurveyIdList.Distinct())
                {
                    Guid Id = new Guid(surveyResponseId);

                    using (var Context = ConnectionObj)
                    {

                        result.Add(Mapper.Map(Context.SurveyResponse.FirstOrDefault(x => x.SurveyId == Id)));
                    }
                }
            }
            catch (Exception ex)
            {
                throw (ex);
            }

            if (PageNumber > 0 && PageSize > 0)
            {
                result.Sort(CompareByDateCreated);
                // remove the items to skip
                if (PageNumber * PageSize - PageSize > 0)
                {
                    result.RemoveRange(0, PageSize);
                }

                if (PageNumber * PageSize < result.Count)
                {
                    result.RemoveRange(PageNumber * PageSize, result.Count - PageNumber * PageSize);
                }
            }


            return result;
        }



        private static int CompareByDateCreated(SurveyResponseBO x, SurveyResponseBO y)
        {
            return x.DateCreated.CompareTo(y.DateCreated);
        }

        public void UpdatePassCode(UserAuthenticationRequestBO passcodeBO)
        {

            try
            {
                Guid Id = new Guid(passcodeBO.ResponseId);
                 epiwebsurvey_apiContext ConnectionObj = new  epiwebsurvey_apiContext();
                //Update Survey
                using (var Context = ConnectionObj)
                {
                    var Query = from response in Context.SurveyResponse
                                where response.ResponseId == Id
                                select response;

                    var DataRow = Query.Single();

                    DataRow.ResponsePasscode = passcodeBO.PassCode;
                    Context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }


        /// <summary>
        /// Updates a SurveyResponse.
        /// </summary>
        /// <param name="SurveyResponse">SurveyResponse.</param>
        public void UpdateSurveyResponse(SurveyResponseBO SurveyResponse)
        {
            try
            {
                Guid Id = new Guid(SurveyResponse.ResponseId);
                 epiwebsurvey_apiContext ConnectionObj = new  epiwebsurvey_apiContext();
                //Update Survey
                using (var Context = ConnectionObj)
                {
                    var Query = from response in Context.SurveyResponse
                                where response.ResponseId == Id
                                select response;

                    var DataRow = Query.Single();

                    if (!string.IsNullOrEmpty(SurveyResponse.RelateParentId) && SurveyResponse.RelateParentId != Guid.Empty.ToString())
                    {
                        DataRow.RelateParentId = new Guid(SurveyResponse.RelateParentId);
                    }

                    DataRow.ResponseXML = SurveyResponse.XML;                    
                    DataRow.DateCompleted = SurveyResponse.DateCompleted;
                    DataRow.StatusId = SurveyResponse.Status;
                    DataRow.DateUpdated = DateTime.Now;
                    //   DataRow.ResponsePasscode = SurveyResponse.ResponsePassCode;
                    DataRow.IsDraftMode = SurveyResponse.IsDraftMode;
                    DataRow.ResponseXMLSize = RemoveWhitespace(SurveyResponse.XML).Length;
                    DataRow.ResponseJson = SurveyResponse.Json;
                    if (!string.IsNullOrEmpty(SurveyResponse.Json))
                    {
                        DataRow.ResponseJsonSize = RemoveWhitespace(SurveyResponse.Json).Length;
                    }
                    Context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        public static string RemoveWhitespace(string xml)
        {
            System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex(@">\s*<");
            xml = regex.Replace(xml, "><");

            return xml.Trim();
        }
        public void UpdateRecordStatus(SurveyResponseBO SurveyResponseBO)
        {

            try
            {
                Guid Id = new Guid(SurveyResponseBO.ResponseId);
                 epiwebsurvey_apiContext ConnectionObj = new  epiwebsurvey_apiContext();

                using (var Context = ConnectionObj)
                {
                    var Query = from response in Context.SurveyResponse
                                where response.ResponseId == Id
                                select response;

                    var DataRow = Query.Single();

                    if (DataRow.StatusId == 3 && SurveyResponseBO.Status == 4)
                    {

                        DataRow.StatusId = SurveyResponseBO.Status;
                    }

                    if (SurveyResponseBO.Status != 4)
                    {

                        DataRow.StatusId = SurveyResponseBO.Status;
                    }


                    Context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                throw (ex);
            }

        }

        public void InsertErrorLog(Dictionary<string, string> pValue)
        {           
                Guid surveyId = Guid.Empty, responseId = Guid.Empty; StringBuilder ErrText = new StringBuilder();
                 epiwebsurvey_apiContext ConnectionObj = new  epiwebsurvey_apiContext();              
                try
                {                
                foreach (KeyValuePair<string, string> kvp in pValue)
                {
                    if (kvp.Key == "SurveyId")
                    {
                        surveyId = new Guid(kvp.Value.ToString());
                    }
                    else if (kvp.Key == "ResponseId")
                    {
                        responseId = new Guid(kvp.Value.ToString());
                    }
                    else
                        ErrText.Append(" " + kvp.Key + " " + kvp.Value + ". ");
                }

                    var Connection = new SqlConnection( epiwebsurvey_apiContext._connectionString);
                    Connection.Open();
                    var Command = Connection.CreateCommand();
                    Command.CommandType = CommandType.StoredProcedure;
                    Command.CommandText = "usp_log_to_errorlog";
                    Command.Parameters.AddWithValue("@SurveyId", surveyId);
                    Command.Parameters.AddWithValue("@ResponseId", responseId);
                    Command.Parameters.AddWithValue("@ErrorText", ErrText.ToString());
                    Command.ExecuteNonQuery();
                }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        public string GetResponseParentId(string ResponseId)
        {
            SurveyResponseBO result = new SurveyResponseBO();
             epiwebsurvey_apiContext ConnectionObj = new  epiwebsurvey_apiContext();
            try
            {

                Guid Id = new Guid(ResponseId);

                using (var Context = ConnectionObj)
                {


                    SurveyResponse Response = Context.SurveyResponse.Where(x => x.ResponseId == Id).First();
                    result = (Mapper.Map(Response));

                }

            }
            catch (Exception ex)
            {
                throw (ex);
            }
            if (!string.IsNullOrEmpty(result.ParentRecordId))
            {
                return result.ParentRecordId;
            }
            else
            {
                return "";
            }
        }

        public SurveyResponseBO GetResponseXml(string ResponseId)
        {
            SurveyResponseBO result = new SurveyResponseBO();
             epiwebsurvey_apiContext ConnectionObj = new  epiwebsurvey_apiContext();

            try
            {

                Guid Id = new Guid(ResponseId);

                using (var Context = ConnectionObj)
                {


                    var Response = Context.ResponseXml.Where(x => x.ResponseId == Id);
                    if (Response.Count() > 0)
                    {
                        result = (Mapper.Map(Response.Single()));

                    }

                }

            }
            catch (Exception ex)
            {
                throw (ex);
            }




            return result;
        }


        public string GetSurveyResponseByUser(string SurveyId, string username)
        {
            StringBuilder json = new StringBuilder("");

            // List<SurveyResponseBO> result = new List<SurveyResponseBO>();
            // epiwebsurvey_apiContext ConnectionObj = new  epiwebsurvey_apiContext();
            try
            {               
                using (SqlConnection connection = new SqlConnection( epiwebsurvey_apiContext._connectionString))
                {
                    connection.Open();
                    string commandString = "select ResponseJson from SurveyResponse r inner join SurveyMetaData m on r.SurveyId = m.SurveyId inner join UserOrganization uo on m.OrganizationId = uo.OrganizationID inner join [User] u on lower(uo.UserID) = u.UserID where r.ResponseJson is not null and r.SurveyId = '" + SurveyId + "' and u.UserName = '" + username + "' order by DateUpdated desc";
                    using (SqlCommand command = new SqlCommand(commandString, connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    string row = JsonConvert.DeserializeObject<ResponseModel>(reader.GetFieldValue<string>(0)).ResponseQA.ToString();
                                    if (!row.Equals("{}"))
                                    {
                                        json.Append(row);
                                        json.Append(",");
                                    }
                                }
                                if (json.Length > 1)
                                {
                                    json.Remove(json.Length - 1, 1);
                                }
                            }
                        }
                    }
                }



            }
            catch (Exception ex)
            {
                throw (ex);
            }
            return json.ToString();
        }

        public string GetFilteredSurveyResponseByUser(string SurveyId, string username, Dictionary<string, string> keyval = null)
        {
            //StringBuilder json = new StringBuilder("[");
            StringBuilder json = new StringBuilder("");

            // List<SurveyResponseBO> result = new List<SurveyResponseBO>();
            // epiwebsurvey_apiContext ConnectionObj = new  epiwebsurvey_apiContext();
            try
            {
                using (SqlConnection connection = new SqlConnection(epiwebsurvey_apiContext._connectionString))
                {
                    connection.Open();
                    string commandString = "select ResponseJson from SurveyResponse r inner join SurveyMetaData m on r.SurveyId = m.SurveyId inner join UserOrganization uo on m.OrganizationId = uo.OrganizationID inner join [User] u on lower(uo.UserID) = u.UserID where r.ResponseJson is not null and r.SurveyId = '" + SurveyId + "' and u.UserName = '" + username + "' order by DateUpdated desc";
                    using (SqlCommand command = new SqlCommand(commandString, connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    string row = JsonConvert.DeserializeObject<ResponseModel>(reader.GetFieldValue<string>(0)).ResponseQA.ToString();
                                    if (!row.Equals("{}"))
                                    {
                                        if (keyval == null)
                                        {
                                            json.Append(row);
                                            json.Append(",");
                                        }
                                        else
                                        {
                                            var settings = new JsonSerializerSettings
                                            {
                                                NullValueHandling = NullValueHandling.Ignore,
                                                MissingMemberHandling = MissingMemberHandling.Ignore
                                            };
                                            Dictionary<string, string> keyvalupair = new Dictionary<string, string>();
                                            Dictionary<string, string> filteredval = new Dictionary<string, string>();
                                            keyvalupair = JsonConvert.DeserializeObject<Dictionary<string, string>>(row, settings);
                                            foreach (var k in keyval)
                                            {
                                                if (keyvalupair.ContainsKey(k.Key) && keyvalupair.ContainsValue(k.Value))
                                                {
                                                    filteredval.Add(k.ToString(), keyvalupair[k.Key]);
                                                }
                                            }
                                            if (keyval.Count == filteredval.Count)
                                            {
                                                json.Append(row);
                                                json.Append(",");
                                            }
                                        }
                                    }
                                }
                                if (json.Length > 1)
                                {
                                    json.Remove(json.Length - 1, 1);
                                }
                            }
                        }
                    }
                }



            }
            catch (Exception ex)
            {
                throw (ex);
            }
            return json.ToString();
        }

        public void DeleteSurveyResponse(SurveyResponseBO SurveyResponse)
        {
            throw new NotImplementedException();
        }

        public List<SurveyResponseBO> GetAncestorResponseIdsByChildId(string ChildId)
        {
            throw new NotImplementedException();
        }

        public UserAuthenticationResponseBO GetAuthenticationResponse(UserAuthenticationRequestBO passcodeBO)
        {
            throw new NotImplementedException();
        }

        public List<SurveyResponseBO> GetFormResponseByFormId(string FormId, int PageNumber, int PageSize)
        {
            throw new NotImplementedException();
        }

        public List<SurveyResponseBO> GetFormResponseByFormId(SurveyAnswerCriteria criteria)
        {
            throw new NotImplementedException();
        }

        public SurveyResponseBO GetFormResponseByParentRecordId(string ResponseId)
        {
            throw new NotImplementedException();
        }

        public int GetFormResponseCount(string FormId)
        {
            throw new NotImplementedException();
        }

        public int GetFormResponseCount(SurveyAnswerCriteria Criteria)
        {
            throw new NotImplementedException();
        }       

        public List<SurveyResponseBO> GetResponsesByRelatedFormId(string ResponseId, string SurveyId)
        {
            throw new NotImplementedException();
        }

        public List<SurveyResponseBO> GetResponsesHierarchyIdsByRootId(string RootId)
        {
            throw new NotImplementedException();
        }         

        public List<SurveyResponseBO> GetSurveyResponse(List<string> SurveyAnswerIdList, string pSurveyId, DateTime pDateCompleted, bool pIsDraftMode = false, int pStatusId = -1, int PageNumber = -1, int PageSize = -1)
        {
            throw new NotImplementedException();
        }

        public List<SurveyResponseBO> GetSurveyResponseBySurveyIdSize(List<string> SurveyIdList, Guid UserPublishKey, int PageNumber = -1, int PageSize = -1, int ResponseMaxSize = -1)
        {
            throw new NotImplementedException();
        }

        public List<SurveyResponseBO> GetSurveyResponseSize(List<string> SurveyResponseIdList, Guid UserPublishKey, int PageNumber = -1, int PageSize = -1, int ResponseMaxSize = -1)
        {
            throw new NotImplementedException();
        }

        public List<SurveyResponseBO> GetSurveyResponseSize(List<string> SurveyAnswerIdList, string pSurveyId, DateTime pDateCompleted, bool pIsDraftMode = false, int pStatusId = -1, int PageNumber = -1, int PageSize = -1, int ResponseMaxSize = -1)
        {
            throw new NotImplementedException();
        }

        public bool HasResponse(SurveyAnswerCriteria Criteria)
        {
            throw new NotImplementedException();
        }

        public void InsertChildSurveyResponse(SurveyResponseBO SurveyResponse)
        {
            throw new NotImplementedException();
        }       

        public void InsertResponseXml(ResponseXmlBO responseXmlBO)
        {
            throw new NotImplementedException();
        }
        public void SetJsonColumn(string json, string responseid)
        {
            throw new NotImplementedException();
        }
      
    }
}
