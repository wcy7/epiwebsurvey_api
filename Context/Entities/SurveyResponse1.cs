using System;
using System.Collections.Generic;

namespace epiwebsurvey_api.Context.Entities
{
    public partial class SurveyResponse1
    {
        public SurveyResponse1()
        {
           // InverseRelateParent = new HashSet<SurveyResponse1>();
           // SurveyResponseUser = new HashSet<SurveyResponseUser>();
        }

        public Guid ResponseId { get; set; }
        public Guid SurveyId { get; set; }
        public DateTime DateUpdated { get; set; }
        public DateTime? DateCompleted { get; set; }
        public int StatusId { get; set; }
        public string ResponseXml { get; set; }
        public string? ResponsePasscode { get; set; }
        public long? ResponseXmlsize { get; set; }
        public DateTime DateCreated { get; set; }
        public bool IsDraftMode { get; set; }
        public bool IsLocked { get; set; }
        public Guid? ParentRecordId { get; set; }
        public Guid? RelateParentId { get; set; }
        public int? RecordSourceId { get; set; }
        public int? OrganizationId { get; set; }

        public LkRecordSource RecordSource { get; set; }
        public SurveyResponse1 RelateParent { get; set; }
        public LkStatus Status { get; set; }
        public SurveyMetaData SurveyMetaData { get; set; }
        public SurveyResponseTracking SurveyResponseTracking { get; set; }
        public ICollection<SurveyResponse> InverseRelateParent { get; set; }
        public ICollection<SurveyResponseUser> SurveyResponseUser { get; set; }
       
        public string ResponseJson { get; set; }
        public long? ResponseJsonSize { get; set; }
    }
}
