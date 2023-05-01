using System;
using System.Collections.Generic;

namespace epiwebsurvey_api.Context.Entities
{
    public partial class SurveyResponseTracking
    {
        public Guid ResponseId { get; set; }
        public bool IsSqlresponse { get; set; }
        public bool IsResponseinsertedToEpi7 { get; set; }

        public SurveyResponse Response { get; set; }
    }
}
