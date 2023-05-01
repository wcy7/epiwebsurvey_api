using System;
using System.Collections.Generic;

namespace epiwebsurvey_api.Context.Entities
{
    public partial class Organization
    {
        public Organization()
        {
            Admin = new HashSet<Admin>();
            Datasource = new HashSet<Datasource>();
            SurveyMetaData = new HashSet<SurveyMetaData>();
            SurveyMetadataOrganization = new HashSet<SurveyMetadataOrganization>();
            UserOrganization = new HashSet<UserOrganization>();
        }

        public int OrganizationId { get; set; }
        public string OrganizationKey { get; set; }
        public string Organization1 { get; set; }
        public bool IsEnabled { get; set; }
        public bool IsHostOrganization { get; set; }

        public ICollection<Admin> Admin { get; set; }
        public ICollection<Datasource> Datasource { get; set; }
        public ICollection<SurveyMetaData> SurveyMetaData { get; set; }
        public ICollection<SurveyMetadataOrganization> SurveyMetadataOrganization { get; set; }
        public ICollection<UserOrganization> UserOrganization { get; set; }
    }
}
