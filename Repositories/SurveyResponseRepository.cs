using Epi.Web.BLL;
using Epi.Web.Common.BusinessObject;
using Epi.Web.Common.DTO;
using Epi.Web.Common.Message;
using Epi.Web.Common.Security;
using epiwebsurvey_api.Context;
using epiwebsurvey_api.Models;
using epiwebsurvey_api.Repositories.Interfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using static Epi.Web.BLL.SurveyResponse;

namespace epiwebsurvey_api.Repositories
{
    public class SurveyResponseRepository : ISurveyResponseRepository
    {
        private List<SurveyAnswerModel> SurveyAnswerModelList = new List<SurveyAnswerModel>();

        public Guid SurveyId
        {
            get; set;
        }

        public Guid OrgKey
        {
            get; set;
        }

        public Guid PublisherKey
        {
            get; set;
        }

        public string RootID
        {
            get; set;
        }

        public SurveyResponseRepository()
        {
        }

        public SurveyControlsResponse GetSurveyControlsList(SurveyControlsRequest pRequestMessage)
        {
            SurveyControlsResponse SurveyControlsResponse = new SurveyControlsResponse();
            try
            {               
                Epi.Web.BLL.DataInterfaces.ISurveyInfoDao pSurveyInfoDao = new EntitySurveyInfoDao();
                SurveyInfo Implementation = new SurveyInfo(pSurveyInfoDao);
                SurveyControlsResponse = Implementation.GetSurveyControlsforApi(pRequestMessage.SurveyId);
            }
            catch (Exception ex)
            {
                SurveyControlsResponse.Message = "Error";
                throw ex;
            }
            return SurveyControlsResponse;
        }

        public List<SourceTableDTO> GetSourceTables(string SurveyId)
        {            
            SourceTablesResponse SourceTables = new SourceTablesResponse();
            SurveyControlsResponse SurveyControlsResponse = new SurveyControlsResponse();
            try
            {
                EntitySurveyInfoDao ISurveyInfoDao = new EntitySurveyInfoDao();
                SurveyInfo Implementation = new SurveyInfo(ISurveyInfoDao);
                SourceTables.List = Epi.Web.Common.ObjectMapping.Mapper.ToSourceTableDTO(Implementation.GetSourceTables(SurveyId));
            }
            catch (Exception ex)
            {
                SurveyControlsResponse.Message = "Error";
                throw ex;
            }
            return SourceTables.List;
        }

        public Dictionary<string, string> ProcessValforLegalControls(IEnumerable<SurveyControlDTO> surveyControlList, Dictionary<string, string> SurveyQuestionAnswerList)
        {
            List<SourceTableDTO> SourceTables = null;
            if (string.IsNullOrEmpty(RootID))
                SourceTables = GetSourceTables(SurveyId.ToString());
            else
                SourceTables = GetSourceTables(RootID);
            if (SourceTables.Count > 0)
            {
                foreach (SurveyControlDTO s in surveyControlList)
                {
                    if (SurveyQuestionAnswerList.Keys.Contains(s.ControlId))
                    {
                        string val = SurveyQuestionAnswerList[s.ControlId];
                        var SourceTableXml1 = SourceTables.Where(x => x.TableName == s.SourceTableName).Select(y => y.TableXml).ToList();
                        XDocument SourceTableXml = XDocument.Parse(SourceTableXml1[0].ToString());
                        var _ControlValues = from _ControlValue in SourceTableXml.Descendants("SourceTable")
                                             select _ControlValue;
                        List<string> legalvals = new List<string>();
                        foreach (var _ControlValue in _ControlValues)
                        {
                            var _SourceTableValues = from _SourceTableValue in _ControlValues.Descendants("Item")

                                                     select _SourceTableValue;

                            foreach (var _SourceTableValue in _SourceTableValues)
                            {
                                legalvals.Add(_SourceTableValue.Attributes().FirstOrDefault().Value.Trim());
                            }
                        }
                        SurveyQuestionAnswerList[s.ControlId] = legalvals.ElementAt(Convert.ToInt32(val) - 1);
                    }
                }
            }
            return SurveyQuestionAnswerList;
        }


        public Dictionary<string, string> ProcessModforRadioControls(IEnumerable<SurveyControlDTO> surveyControlList, Dictionary<string, string> SurveyQuestionAnswerList)
        {
            foreach (SurveyControlDTO s in surveyControlList)
            {
                if (SurveyQuestionAnswerList.Keys.Contains(s.ControlId))
                {
                    int val = Convert.ToInt32(SurveyQuestionAnswerList[s.ControlId]);
                    SurveyQuestionAnswerList[s.ControlId] = (val % 100).ToString();
                }
            }
            return SurveyQuestionAnswerList;
        }

        public Dictionary<string, string> ProcessValforCheckBoxControls(IEnumerable<SurveyControlDTO> surveyControlList, Dictionary<string, string> SurveyQuestionAnswerList)
        {
            foreach (SurveyControlDTO s in surveyControlList)
            {
                if (SurveyQuestionAnswerList.Keys.Contains(s.ControlId))
                {
                    string val = SurveyQuestionAnswerList[s.ControlId];
                    if (val != null && val.ToLower() == "false")
                        SurveyQuestionAnswerList[s.ControlId] = "No";
                    else if (val != null && val.ToLower() == "true")
                        SurveyQuestionAnswerList[s.ControlId] = "Yes";
                }
            }
            return SurveyQuestionAnswerList;
        }

        public Dictionary<string, string> ProcessValforYesNoControls(IEnumerable<SurveyControlDTO> surveyControlList, Dictionary<string, string> SurveyQuestionAnswerList)
        {
            foreach (SurveyControlDTO s in surveyControlList)
            {
                if (SurveyQuestionAnswerList.Keys.Contains(s.ControlId))
                {
                    string val = SurveyQuestionAnswerList[s.ControlId];
                    if (val != null && val == "2")
                        SurveyQuestionAnswerList[s.ControlId] = "1";
                    else if (val != null && val == "1")
                        SurveyQuestionAnswerList[s.ControlId] = "0";
                }
            }
            return SurveyQuestionAnswerList;
        }

        /// <summary>
        /// Inserts SurveyResponse 
        /// </summary>
        /// <param name="SurveyAnswerModel"></param>
        /// <returns>response </returns>
        public PreFilledAnswerResponse SetSurveyAnswer(SurveyAnswerModel request, string json)
        {
            PreFilledAnswerResponse response;
            SurveyControlsResponse SurveyControlsResponse = new SurveyControlsResponse();
            SurveyControlsRequest surveyControlsRequest = new SurveyControlsRequest();
            surveyControlsRequest.SurveyId = request.SurveyId.ToString();
            try
            {
                Epi.Web.BLL.DataInterfaces.ISurveyResponseDao SurveyResponseDao = new EntitySurveyResponseDao();
                Epi.Web.BLL.DataInterfaces.ISurveyInfoDao surveyInfoDao = new EntitySurveyInfoDao();
                SurveyResponse Implementation = new SurveyResponse(SurveyResponseDao, surveyInfoDao);
                PreFilledAnswerRequest prefilledanswerRequest = new PreFilledAnswerRequest();
                Dictionary<string, string> Values = new Dictionary<string, string>();
                prefilledanswerRequest.AnswerInfo.UserPublishKey = request.PublisherKey;
                prefilledanswerRequest.AnswerInfo.OrganizationKey = request.OrgKey;
                prefilledanswerRequest.AnswerInfo.SurveyId = request.SurveyId;
                prefilledanswerRequest.AnswerInfo.UserPublishKey = request.PublisherKey;
                List<SurveyInfoBO> SurveyBOList = GetSurveyInfo(prefilledanswerRequest);
                GetRootFormId(prefilledanswerRequest);
                prefilledanswerRequest.AnswerInfo.SurveyId = request.SurveyId;
                SurveyControlsResponse = GetSurveyControlsList(surveyControlsRequest);
                Dictionary<string, string> FilteredAnswerList = new Dictionary<string, string>();
                var radiolist = SurveyControlsResponse.SurveyControlList.Where(x => x.ControlType == "GroupBoxRadioList");
                FilteredAnswerList = ProcessModforRadioControls(radiolist, request.SurveyQuestionAnswerListField);
                var checkboxLsit = SurveyControlsResponse.SurveyControlList.Where(x => x.ControlType == "CheckBox");
                FilteredAnswerList = ProcessValforCheckBoxControls(checkboxLsit, FilteredAnswerList);
                var yesNoList = SurveyControlsResponse.SurveyControlList.Where(x => x.ControlType == "YesNo");
                FilteredAnswerList = ProcessValforYesNoControls(yesNoList, FilteredAnswerList);
                var legalvalList = SurveyControlsResponse.SurveyControlList.Where(x => x.ControlType == "LegalValues");
                FilteredAnswerList = ProcessValforLegalControls(legalvalList, FilteredAnswerList);
                foreach (KeyValuePair<string, string> entry in FilteredAnswerList)
                {
                    Values.Add(entry.Key, entry.Value);
                }
                prefilledanswerRequest.AnswerInfo.SurveyQuestionAnswerList = Values;
                response = Implementation.SetSurveyAnswer(prefilledanswerRequest,json);
                return response;
            }
            catch (Exception ex)
            {
                PassCodeDTO DTOList = new PassCodeDTO();
                response = new PreFilledAnswerResponse(DTOList);
                if (response.ErrorMessageList != null)
                    response.ErrorMessageList.Add("Failed", "Failed to insert Response");
                response.Status = ((SurveyResponse.Message)1).ToString();
                return response;
            }
        }

        public void GetRootFormId(PreFilledAnswerRequest request)
        {
            Epi.Web.BLL.DataInterfaces.ISurveyResponseDao SurveyResponseDao = new EntitySurveyResponseDao();
            Epi.Web.BLL.DataInterfaces.ISurveyInfoDao surveyInfoDao = new EntitySurveyInfoDao();
            SurveyResponse Implementation = new SurveyResponse(SurveyResponseDao, surveyInfoDao);
            List<SurveyInfoBO> SurveyBOList = Implementation.GetSurveyInfo(request);//
            if (string.IsNullOrEmpty(SurveyBOList[0].ParentId))
            {
                RootID = SurveyBOList[0].SurveyId;
            }
            else
            {
                request.AnswerInfo.SurveyId = new Guid(SurveyBOList[0].ParentId);
                GetRootFormId(request);
            }
        }

        public List<SurveyInfoBO> GetSurveyInfo(PreFilledAnswerRequest request)
        {
            Epi.Web.BLL.DataInterfaces.ISurveyResponseDao SurveyResponseDao = new EntitySurveyResponseDao();
            Epi.Web.BLL.DataInterfaces.ISurveyInfoDao surveyInfoDao = new EntitySurveyInfoDao();
            SurveyResponse Implementation = new SurveyResponse(SurveyResponseDao, surveyInfoDao);
            List<SurveyInfoBO> SurveyBOList = Implementation.GetSurveyInfo(request);//           
            return SurveyBOList;
        }

        /// <summary>
        /// Updates SurveyResponse 
        /// </summary>
        /// <param name="SurveyAnswerModel",name="ResponseId"></param>
        /// <returns>response </returns>
        public PreFilledAnswerResponse Update(SurveyAnswerModel request, string ResponseId,string json)
        {
            PreFilledAnswerResponse response;
            SurveyControlsResponse SurveyControlsResponse = new SurveyControlsResponse();
            SurveyControlsRequest surveyControlsRequest = new SurveyControlsRequest();
            surveyControlsRequest.SurveyId = request.SurveyId.ToString();

            try
            {
                Epi.Web.BLL.DataInterfaces.ISurveyResponseDao SurveyResponseDao = new EntitySurveyResponseDao();
                Epi.Web.BLL.DataInterfaces.ISurveyInfoDao surveyInfoDao = new EntitySurveyInfoDao();
                SurveyResponse Implementation = new SurveyResponse(SurveyResponseDao, surveyInfoDao);
                PreFilledAnswerRequest prefilledanswerRequest = new PreFilledAnswerRequest();
                Dictionary<string, string> Values = new Dictionary<string, string>();
                prefilledanswerRequest.AnswerInfo.UserPublishKey = request.PublisherKey;
                prefilledanswerRequest.AnswerInfo.OrganizationKey = request.OrgKey;
                prefilledanswerRequest.AnswerInfo.SurveyId = request.SurveyId;
                prefilledanswerRequest.AnswerInfo.UserPublishKey = request.PublisherKey;
                List<SurveyInfoBO> SurveyBOList = GetSurveyInfo(prefilledanswerRequest);
                GetRootFormId(prefilledanswerRequest);
                prefilledanswerRequest.AnswerInfo.SurveyId = request.SurveyId;
                SurveyControlsResponse = GetSurveyControlsList(surveyControlsRequest);
                Dictionary<string, string> FilteredAnswerList = new Dictionary<string, string>();
                var radiolist = SurveyControlsResponse.SurveyControlList.Where(x => x.ControlType == "GroupBoxRadioList");
                FilteredAnswerList = ProcessModforRadioControls(radiolist, request.SurveyQuestionAnswerListField);
                var checkboxLsit = SurveyControlsResponse.SurveyControlList.Where(x => x.ControlType == "CheckBox");
                FilteredAnswerList = ProcessValforCheckBoxControls(checkboxLsit, FilteredAnswerList);
                var yesNoList = SurveyControlsResponse.SurveyControlList.Where(x => x.ControlType == "YesNo");
                FilteredAnswerList = ProcessValforYesNoControls(yesNoList, FilteredAnswerList);
                var legalvalList = SurveyControlsResponse.SurveyControlList.Where(x => x.ControlType == "LegalValues");
                FilteredAnswerList = ProcessValforLegalControls(legalvalList, FilteredAnswerList);

                var updatedtime = FilteredAnswerList.Where(x => x.Key.ToLower() == "_updatestamp").FirstOrDefault();
                var Responsekey = FilteredAnswerList.Where(x => x.Key.ToLower() == "responseid" || x.Key.ToLower() == "id").FirstOrDefault().Key;
                var fkey = FilteredAnswerList.Where(x => x.Key.ToLower() == "fkey").FirstOrDefault();
                foreach (KeyValuePair<string, string> entry in FilteredAnswerList)
                {
                    Values.Add(entry.Key, entry.Value);
                }

                try
                {
                    var survey = Implementation.GetSurveyResponseById(new List<string> { ResponseId }, request.PublisherKey);
                }
                catch (Exception ex)
                {
                    prefilledanswerRequest.AnswerInfo.SurveyQuestionAnswerList = Values;
                    string xml = Implementation.CreateResponseXml(prefilledanswerRequest, SurveyBOList);
                    Epi.Web.Common.Json.SurveyResponseJson surveyResponsejson = new Epi.Web.Common.Json.SurveyResponseJson();
                    SurveyResponseBO surveyanswer = new SurveyResponseBO();
                    surveyanswer.XML = xml; surveyanswer.SurveyId = prefilledanswerRequest.AnswerInfo.SurveyId.ToString(); surveyanswer.ResponseId = ResponseId;
                    string Json = GetResponseJson(Values, request.SurveyId.ToString(), ResponseId);
                    response = Implementation.SetSurveyAnswer(prefilledanswerRequest, Json);
                    response.Status = "Created";
                    return response;
                }


                Values.Remove(Responsekey);
                if (updatedtime.Key != null)
                    Values.Remove(updatedtime.Key);
                if (fkey.Key != null)
                    Values.Remove(fkey.Key);

                prefilledanswerRequest.AnswerInfo.SurveyQuestionAnswerList = Values;

                Dictionary<string, string> ErrorMessageList = new Dictionary<string, string>();
                string Xml = Implementation.CreateResponseXml(prefilledanswerRequest, SurveyBOList);//
                ErrorMessageList = Implementation.ValidateResponse(SurveyBOList, prefilledanswerRequest);//
                if (fkey.Key != null)
                {
                    try
                    {
                        var survey = Implementation.GetSurveyResponseById(new List<string> { fkey.Value }, request.PublisherKey);
                    }
                    catch (Exception ex)
                    {
                        SurveyResponseBO surveyresponsebO = new SurveyResponseBO();
                        surveyresponsebO.SurveyId = SurveyBOList[0].ParentId;
                        surveyresponsebO.ResponseId = fkey.Value.ToString();
                        surveyresponsebO.XML = "  ";
                        surveyresponsebO.Status = 3;
                        surveyresponsebO.RecrodSourceId = (int)Epi.Web.Common.BusinessRule.ValidationRecordSourceId.MA;
                        surveyresponsebO.DateUpdated = DateTime.Now;
                        surveyresponsebO.DateCreated = surveyresponsebO.DateUpdated;
                        surveyresponsebO.DateCompleted = surveyresponsebO.DateUpdated;
                        //surveyresponsebO.Json = json;
                        surveyresponsebO = Implementation.InsertSurveyResponse(surveyresponsebO);

                    }
                }

                if (ErrorMessageList.Count() > 0)
                {
                    response = new PreFilledAnswerResponse();
                    response.ErrorMessageList = ErrorMessageList;
                    response.ErrorMessageList.Add("SurveyId", request.SurveyId.ToString());
                    response.ErrorMessageList.Add("ResponseId", ResponseId);
                    response.Status = ((Message)1).ToString();
                    Implementation.InsertErrorLog(response.ErrorMessageList);
                }
                SurveyResponseBO surveyresponseBO = new SurveyResponseBO(); SurveyResponseBO SurveyResponse = new SurveyResponseBO();
                UserAuthenticationRequestBO UserAuthenticationRequestBO = new UserAuthenticationRequestBO();
                surveyresponseBO.SurveyId = request.SurveyId.ToString();
                surveyresponseBO.ResponseId = ResponseId.ToString();
                surveyresponseBO.XML = Xml;
                System.DateTime dateTime = new System.DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
                if (updatedtime.Key != null)
                {
                    surveyresponseBO.DateUpdated = dateTime.AddMilliseconds(Convert.ToDouble(updatedtime.Value.ToString())).ToLocalTime();
                    surveyresponseBO.DateCompleted = dateTime.AddMilliseconds(Convert.ToDouble(updatedtime.Value.ToString())).ToLocalTime();
                }
                else
                {
                    surveyresponseBO.DateUpdated = DateTime.Now;
                    surveyresponseBO.DateCompleted = DateTime.Now;
                }
                if (fkey.Key != null)
                {
                    surveyresponseBO.RelateParentId = fkey.Value;
                }
                surveyresponseBO.Json = GetResponseJson(Values, request.SurveyId.ToString(), ResponseId);
                surveyresponseBO.Status = 3;
                SurveyResponse = Implementation.UpdateSurveyResponse(surveyresponseBO);
                UserAuthenticationRequestBO = Epi.Web.Common.ObjectMapping.Mapper.ToBusinessObject(ResponseId);
                Implementation.SavePassCode(UserAuthenticationRequestBO);

                //return Response
                string ResponseUrl = ConfigurationManager.AppSettings["ResponseURL"];
                response = new PreFilledAnswerResponse(Epi.Web.Common.ObjectMapping.Mapper.ToDataTransferObjects(UserAuthenticationRequestBO));
                response.SurveyResponseUrl = ResponseUrl + UserAuthenticationRequestBO.ResponseId;
                response.Status = ((Message)2).ToString();
                return response;
            }
            catch (Exception ex)
            {
                PassCodeDTO DTOList = new PassCodeDTO();
                response = new PreFilledAnswerResponse(DTOList);
                if (response.ErrorMessageList != null)
                    response.ErrorMessageList.Add("Failed", "Failed to insert Response");
                response.Status = ((SurveyResponse.Message)1).ToString();
                return response;
            }
        }


        public string GetResponseJson(Dictionary<string, string> Values, string SurveyId, string ResponseId)
        {
            string json = "";
            Epi.Web.Common.Json.ResponseDetail responseDetail = new Epi.Web.Common.Json.ResponseDetail();
            Dictionary<string, object> ResponseQA = new Dictionary<string, object>();           
            foreach (KeyValuePair<string, string> entry in Values)
            {
                if (entry.Key == "_updatestamp" || entry.Key.ToLower() == "responseid" || entry.Key.ToLower() == "id" | entry.Key == "fkey")
                {
                }
                else
                    ResponseQA.Add(entry.Key, entry.Value);
            }
            ResponseQA.Add("ResponseId", ResponseId);
            responseDetail.ResponseId = ResponseId;
            responseDetail.FormId = SurveyId;
            responseDetail.ResponseQA = ResponseQA;
            json = JsonConvert.SerializeObject(responseDetail);
            return json;
        }

        /// <summary>
        /// Validate Header information coming from the request
        /// </summary>
        /// <param name="SurveyId"></param>
        /// <returns>response </returns>
        public SurveyInfoBO GetSurveyInfoById(string SurveyId)
        {
            Epi.Web.BLL.DataInterfaces.ISurveyResponseDao SurveyResponseDao = new EntitySurveyResponseDao();
            Epi.Web.BLL.DataInterfaces.ISurveyInfoDao surveyInfoDao = new EntitySurveyInfoDao();
            SurveyInfo SurveyInfo = new SurveyInfo(surveyInfoDao);
            try
            {
                var surveyInfo = SurveyInfo.GetSurveyInfoById(SurveyId);
                if (surveyInfo != null)
                {
                    Epi.Web.BLL.DataInterfaces.IOrganizationDao OrgDao = new EntityOrganizationDao();
                    Organization Organization = new Organization(OrgDao);                    
                    var OrgBo = Organization.GetOrganizationById(surveyInfo.OrganizationId);                    
                    surveyInfo.OrganizationKey = new Guid(Cryptography.Decrypt(OrgBo.OrganizationKey));
                }
                return surveyInfo;
            }
            catch (Exception ex)
            {
                return null;               
            }

        }

        /// <summary>
        /// Updates SurveyResponse 
        /// </summary>
        /// <param name="id"></param>
        public void Remove(string id)
        {
            Epi.Web.BLL.DataInterfaces.ISurveyResponseDao SurveyResponseDao = new EntitySurveyResponseDao();
            Epi.Web.BLL.DataInterfaces.ISurveyInfoDao surveyInfoDao = new EntitySurveyInfoDao();
            SurveyResponse Implementation = new SurveyResponse(SurveyResponseDao, surveyInfoDao);
            SurveyResponseBO surveyresponseBO = new SurveyResponseBO();
            surveyresponseBO.ResponseId = id;
            surveyresponseBO.Status = 0;
            Implementation.UpdateRecordStatus(surveyresponseBO);
        }



    }
}
