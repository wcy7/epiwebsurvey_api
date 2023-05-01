using System;
using System.Collections.Generic;

namespace epiwebsurvey_api.Context.Entities
{
    public partial class LkRecordSource
    {
        public LkRecordSource()
        {
            SurveyResponse = new HashSet<SurveyResponse>();
        }

        public int RecordSourceId { get; set; }
        public string RecordSource { get; set; }
        public string RecordSourceDescription { get; set; }

        public ICollection<SurveyResponse> SurveyResponse { get; set; }
    }
}
