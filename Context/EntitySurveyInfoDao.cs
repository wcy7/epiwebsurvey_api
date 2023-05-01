using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Epi.Web.Common.BusinessObject;
using  epiwebsurvey_api.Context.Entities;

namespace epiwebsurvey_api.Context
{
    public class EntitySurveyInfoDao : Epi.Web.BLL.DataInterfaces.ISurveyInfoDao
    {
        public string Connectionstring { get; set; }
        public IMapper Imapper { get; set; }


        public  epiwebsurvey_apiContext CreateContext()
        {

             epiwebsurvey_apiContext ConnectionObj = new  epiwebsurvey_apiContext();
            Connectionstring =  epiwebsurvey_apiContext._connectionString;
            return ConnectionObj;
        }
        /// <summary>
        /// Gets SurveyInfo based on a list of ids
        /// </summary>
        /// <param name="SurveyInfoId">Unique SurveyInfo identifier.</param>
        /// <returns>SurveyInfo.</returns>
        public List<SurveyInfoBO> GetSurveyInfo(List<string> SurveyInfoIdList, int PageNumber = -1, int PageSize = -1)
        {
            List<SurveyInfoBO> result = new List<SurveyInfoBO>();
            string OrganizationName = ""; int OrgId = 0;
             epiwebsurvey_apiContext ConnectionObj = CreateContext();
            using (var Context = ConnectionObj)
            {
                if (SurveyInfoIdList.Count > 0)
                {
                    try
                    {
                        foreach (string surveyInfoId in SurveyInfoIdList.Distinct())
                        {
                            Guid Id = new Guid(surveyInfoId);

                            // using (var Context = ConnectionObj)
                            // {
                            //  var SurveyMetaDataEntity = Imapper.Map<SurveyMetaData>(SurveyInfoBO);
                            result.Add(Mapper.Map(Context.SurveyMetaData.FirstOrDefault(x => x.SurveyId == Id)));
                            // }
                        }
                    }
                    catch (Exception ex)
                    {
                        throw (ex);
                    }
                }
                else
                {
                    try
                    {
                        // using (var Context = ConnectionObj)
                        // {
                        result = Mapper.Map(Context.SurveyMetaData.ToList());
                        // }
                    }
                    catch (Exception ex)
                    {
                        throw (ex);
                    }
                }
                if (result != null && string.IsNullOrEmpty((result[0].OrganizationName)))
                {
                    try
                    {
                        // using (var Context = ConnectionObj)
                        // {
                        OrgId = Convert.ToInt32(result[0].OrganizationId);
                        OrganizationName = Context.Organization.FirstOrDefault(x => x.OrganizationId == OrgId).Organization1;
                        result[0].PublishedOrgName = OrganizationName;
                        // }
                    }
                    catch (Exception ex)
                    {
                        throw (ex);
                    }
                }

                // remove the items to skip
                // remove the items after the page size
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

            return result;
        }

        private static int CompareByDateCreated(SurveyInfoBO x, SurveyInfoBO y)
        {
            return x.DateCreated.CompareTo(y.DateCreated);
        }

        public List<SourceTableBO> GetSourceTables(string FormId)
        {
            List<SourceTableBO> result = new List<SourceTableBO>();
           // string ConnectionString = DataObjectFactory._ADOConnectionString;
            SqlConnection Connection = new SqlConnection( epiwebsurvey_apiContext._connectionString);


            Connection.Open();

            SqlCommand Command = new SqlCommand();
            Command.Connection = Connection;
            try
            {
                Command.CommandType = CommandType.Text;
                Command.CommandText = "select * from Sourcetables  where  FormId = @Form_Id";
                var param = new SqlParameter("Form_Id", SqlDbType.VarChar);
                param.Value = FormId;
                Command.Parameters.Add(param);
                // Command.ExecuteNonQuery();
                SqlDataAdapter Adapter = new SqlDataAdapter(Command);
                DataSet DS = new DataSet();
                Adapter.Fill(DS);
                if (DS.Tables.Count > 0)
                {
                    result = Mapper.MapToSourceTableBO(DS.Tables[0]);
                }
                Connection.Close();
            }
            catch (Exception)
            {
                Connection.Close();

            }

            return result;
        }

        /// <summary>
        /// Gets SurveyInfo based on criteria
        /// </summary>
        /// <param name="SurveyInfoId">Unique SurveyInfo identifier.</param>
        /// <returns>SurveyInfo.</returns>
        public List<SurveyInfoBO> GetSurveyInfo(List<string> SurveyInfoIdList, DateTime pClosingDate, string pOrganizationKey, int pSurveyType = -1, int PageNumber = -1, int PageSize = -1)
        {
            List<SurveyInfoBO> result = new List<SurveyInfoBO>();

            List<SurveyMetaData> responseList = new List<SurveyMetaData>();

            int OrganizationId = 0;
             epiwebsurvey_apiContext ConnectionObj = new  epiwebsurvey_apiContext();
            using (var Context = ConnectionObj)
            {
                try
                {
                   // using (var Context = ConnectionObj)
                   // {

                        OrganizationId = Context.Organization.FirstOrDefault(x => x.OrganizationKey == pOrganizationKey).OrganizationId;
                   // }
                }
                catch (Exception ex)
                {
                    throw (ex);
                }

                if (SurveyInfoIdList.Count > 0)
                {
                    foreach (string surveyInfoId in SurveyInfoIdList.Distinct())
                    {
                        Guid Id = new Guid(surveyInfoId);
                        try
                        {
                           // using (var Context = ConnectionObj)
                           // {
                                responseList.Add(Context.SurveyMetaData.FirstOrDefault(x => x.SurveyId == Id && x.OrganizationId == OrganizationId));
                            //}
                        }
                        catch (Exception ex)
                        {
                            throw (ex);
                        }
                    }
                }
                else
                {
                    //using (var Context = ConnectionObj)
                   // {
                        responseList = Context.SurveyMetaData.ToList();

                   // }
                }


                if (responseList.Count > 0 && responseList[0] != null)
                {

                    if (pSurveyType > -1)
                    {
                        List<SurveyMetaData> statusList = new List<SurveyMetaData>();
                        statusList.AddRange(responseList.Where(x => x.SurveyTypeId == pSurveyType));
                        responseList = statusList;
                    }

                    if (OrganizationId > 0)
                    {
                        List<SurveyMetaData> OIdList = new List<SurveyMetaData>();
                        OIdList.AddRange(responseList.Where(x => x.OrganizationId == OrganizationId));
                        responseList = OIdList;

                    }

                    if (pClosingDate != null)
                    {
                        if (pClosingDate > DateTime.MinValue)
                        {
                            List<SurveyMetaData> dateList = new List<SurveyMetaData>();

                            dateList.AddRange(responseList.Where(x => x.ClosingDate.Month >= pClosingDate.Month && x.ClosingDate.Year >= pClosingDate.Year && x.ClosingDate.Day >= pClosingDate.Day));
                            responseList = dateList;
                        }
                    }
                    result = Mapper.Map(responseList);

                    // remove the items to skip
                    // remove the items after the page size
                    if (PageNumber > 0 && PageSize > 0)
                    {
                        result.Sort(CompareByDateCreated);

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
            return result;
        }

        public List<SurveyInfoBO> GetSurveyInfoByOrgKeyAndPublishKey(string SurveyId, string Okey, Guid publishKey)
        {
            List<SurveyInfoBO> result = new List<SurveyInfoBO>();

            List<SurveyMetaData> responseList = new List<SurveyMetaData>();

            int OrganizationId = 0;
             epiwebsurvey_apiContext ConnectionObj = new  epiwebsurvey_apiContext();
            using (var Context = ConnectionObj)
            {
                try
                {
                    //using (var Context = ConnectionObj)
                   // {

                        var Query = (from response in Context.Organization
                                     where response.OrganizationKey == Okey
                                     select response).SingleOrDefault();

                        if (Query != null)
                        {
                            OrganizationId = Query.OrganizationId;
                        }

                    //}
                }
                catch (Exception ex)
                {
                    throw (ex);
                }

                if (!string.IsNullOrEmpty(SurveyId))
                {
                    try
                    {
                        Guid Id = new Guid(SurveyId);
                      //  using (var Context = ConnectionObj)
                       // {
                            responseList.Add(Context.SurveyMetaData.FirstOrDefault(x => x.SurveyId == Id && x.OrganizationId == OrganizationId && x.UserPublishKey == publishKey));
                            if (responseList[0] != null)
                            {
                                result = Mapper.Map(responseList);
                            }
                        //}
                    }
                    catch (Exception ex)
                    {
                        throw (ex);
                    }
                }
            }

            return result;
        }



        public void DeleteSurveyInfo(SurveyInfoBO SurveyInfo)
        {
            throw new NotImplementedException();
        }

        public List<SurveyInfoBO> GetAllSurveysByOrgKey(string OrgKey)
        {
            throw new NotImplementedException();
        }

        public List<SurveyInfoBO> GetChildInfoByParentId(string ParentFormId, int ViewId)
        {
            throw new NotImplementedException();
        }

        public List<SurveyInfoBO> GetFormsHierarchyIdsByRootId(string RootId)
        {
            throw new NotImplementedException();
        }

        public int GetOrganizationId(string OrgKey)
        {
            throw new NotImplementedException();
        }

        public SurveyInfoBO GetParentInfoByChildId(string ChildId)
        {
            throw new NotImplementedException();
        }         
     
        public List<SurveyInfoBO> GetSurveyInfoByOrgKey(string SurveyId, string pOrganizationKey)
        {
            throw new NotImplementedException();
        }      

        public List<SurveyInfoBO> GetSurveySizeInfo(List<string> SurveyInfoIdList, int PageNumber = -1, int PageSize = -1, int ResponsesTotalsize = -1)
        {
            throw new NotImplementedException();
        }

        public List<SurveyInfoBO> GetSurveySizeInfo(List<string> SurveyInfoIdList, DateTime pClosingDate, string Okey, int pSurveyType = -1, int PageNumber = -1, int PageSize = -1, int ResponsesTotalsize = -1)
        {
            throw new NotImplementedException();
        }

        public void InsertConnectionString(DbConnectionStringBO ConnectionString)
        {
            throw new NotImplementedException();
        }

        public void InsertFormdefaultSettings(string FormId, bool IsSqlProject, List<string> ControlsNameList)
        {
            throw new NotImplementedException();
        }

        public void InsertSourceTable(string SourcetableXml, string SourcetableName, string FormId)
        {
            throw new NotImplementedException();
        }

        public void InsertSurveyInfo(SurveyInfoBO SurveyInfo)
        {
            throw new NotImplementedException();
        }

        public void UpdateConnectionString(DbConnectionStringBO ConnectionString)
        {
            throw new NotImplementedException();
        }

        public void UpdateParentId(string SurveyId, int ViewId, string ParentId)
        {
            throw new NotImplementedException();
        }

        public void UpdateSourceTable(string p, string SourcetableName, string FormId)
        {
            throw new NotImplementedException();
        }

        public void UpdateSurveyInfo(SurveyInfoBO SurveyInfo)
        {
            throw new NotImplementedException();
        }

        public void ValidateServername(SurveyInfoBO pRequestMessage)
        {
            throw new NotImplementedException();
        }
    }
}
