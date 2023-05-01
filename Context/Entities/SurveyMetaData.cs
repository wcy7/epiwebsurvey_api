using System;
using System.Collections.Generic;

namespace epiwebsurvey_api.Context.Entities
{
    public partial class SurveyMetaData
    {
        public SurveyMetaData()
        {
            Eidatasource = new HashSet<Eidatasource>();
            ResponseDisplaySettings = new HashSet<ResponseDisplaySettings>();
            SurveyMetaDataUser = new HashSet<SurveyMetaDataUser>();
            SurveyMetadataOrganization = new HashSet<SurveyMetadataOrganization>();
            SurveyResponse = new HashSet<SurveyResponse>();
        }

        public Guid SurveyId { get; set; }
        public int OwnerId { get; set; }
        public string SurveyNumber { get; set; }
        public int SurveyTypeId { get; set; }
        public DateTime ClosingDate { get; set; }
        public string SurveyName { get; set; }
        public string OrganizationName { get; set; }
        public string DepartmentName { get; set; }
        public string IntroductionText { get; set; }
        public string TemplateXml { get; set; }
        public string ExitText { get; set; }
        public Guid UserPublishKey { get; set; }
        public long TemplateXmlsize { get; set; }
        public DateTime DateCreated { get; set; }
        public int OrganizationId { get; set; }
        public bool IsDraftMode { get; set; }
        public DateTime StartDate { get; set; }
        public Guid? ParentId { get; set; }
        public int? ViewId { get; set; }
        public bool? IsSqlproject { get; set; }
        public bool? IsShareable { get; set; }
        public bool? ShowAllRecords { get; set; }
        public int? DataAccessRuleId { get; set; }
        public DateTime? LastUpdate { get; set; }

        public Organization Organization { get; set; }
        public LkSurveyType SurveyType { get; set; }
        public ICollection<Eidatasource> Eidatasource { get; set; }
        public ICollection<ResponseDisplaySettings> ResponseDisplaySettings { get; set; }
        public ICollection<SurveyMetaDataUser> SurveyMetaDataUser { get; set; }
        public ICollection<SurveyMetadataOrganization> SurveyMetadataOrganization { get; set; }
        public ICollection<SurveyResponse> SurveyResponse { get; set; }
    }
}
