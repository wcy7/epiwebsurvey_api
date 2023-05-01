using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace epiwebsurvey_api.Context.Entities
{
    public class SurveyResponse
    {
        public SurveyResponse()
        {
            this.SurveyResponse2 = new HashSet<SurveyResponse>();
            // InverseRelateParent = new HashSet<SurveyResponse>();
            this.Users = new HashSet<User>();
        }

        public System.Guid ResponseId { get; set; }
        public System.Guid SurveyId { get; set; }
        public System.DateTime DateUpdated { get; set; }
        public Nullable<System.DateTime> DateCompleted { get; set; }
        public int StatusId { get; set; }
        public string ResponseXML { get; set; }
        public string ResponsePasscode { get; set; }
        public Nullable<long> ResponseXMLSize { get; set; }
        public System.DateTime DateCreated { get; set; }
        public bool IsDraftMode { get; set; }
        public bool IsLocked { get; set; }
        public Nullable<System.Guid> ParentRecordId { get; set; }
        public Nullable<System.Guid> RelateParentId { get; set; }
        public Nullable<int> RecordSourceId { get; set; }
        public Nullable<int> OrganizationId { get; set; }
        public Nullable<long> ResponseJsonSize { get; set; }
        public string ResponseJson { get; set; }
        public SurveyResponse RelateParent { get; set; }
        public  LkStatus lk_Status { get; set; }
        public  SurveyMetaData SurveyMetaData { get; set; }
        //public  ICollection<SurveyResponse> SurveyResponse12 { get; set; }
        //public ICollection<SurveyResponse> InverseRelateParent { get; set; }
        //public SurveyResponse SurveyResponse2 { get; set; }
        public ICollection<SurveyResponse> SurveyResponse2 { get; set; }
        public  ICollection<User> Users { get; set; }
        public  SurveyResponseTracking SurveyResponseTracking { get; set; }
        public  Organization Organization { get; set; }
        public  LkRecordSource lk_RecordSource { get; set; }
        public ICollection<SurveyResponseUser> SurveyResponseUser { get; set; }
    }
}
