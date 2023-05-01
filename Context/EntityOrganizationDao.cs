using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Epi.Web.Common.BusinessObject;

namespace epiwebsurvey_api.Context
{
    public class EntityOrganizationDao : Epi.Web.BLL.DataInterfaces.IOrganizationDao
    {
        public string Connectionstring { get; set; }
        public IMapper Imapper { get; set; }
        public OrganizationBO GetOrganizationInfoById(int OrgId)
        {

            OrganizationBO OrganizationBO = new OrganizationBO();
            try
            {
                 epiwebsurvey_apiContext ConnectionObj = new  epiwebsurvey_apiContext();
                using (var Context = ConnectionObj)
                {
                    var Query = (from response in Context.Organization
                                 where response.OrganizationId == OrgId
                                 select response);
                    if (Query.Count() > 0)
                    {
                        OrganizationBO = Mapper.Map(Query.SingleOrDefault());

                    }
                    else
                    {
                        return null;
                    }

                }
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            return OrganizationBO;
        }
        public void DeleteOrganization(OrganizationBO Organization)
        {
            throw new NotImplementedException();
        }

        public List<OrganizationBO> GetOrganizationInfo()
        {
            throw new NotImplementedException();
        }      

        public OrganizationBO GetOrganizationInfoByKey(string key)
        {
            throw new NotImplementedException();
        }

        public List<OrganizationBO> GetOrganizationInfoByOrgKey(string gOrgKeyEncrypted)
        {
            throw new NotImplementedException();
        }

        public List<OrganizationBO> GetOrganizationKeys(string OrganizationName)
        {
            throw new NotImplementedException();
        }

        public List<OrganizationBO> GetOrganizationNames()
        {
            throw new NotImplementedException();
        }

        public void InsertOrganization(OrganizationBO Organization)
        {
            throw new NotImplementedException();
        }

        public void UpdateOrganization(OrganizationBO Organization)
        {
            throw new NotImplementedException();
        }
    }
}
