using System;
using System.Collections.Generic;

namespace epiwebsurvey_api.Context.Entities
{
    public partial class LkStatus
    {
        public LkStatus()
        {
            SurveyResponse = new HashSet<SurveyResponse>();
        }

        public int StatusId { get; set; }
        public string Status { get; set; }

        public ICollection<SurveyResponse> SurveyResponse { get; set; }
    }
}
