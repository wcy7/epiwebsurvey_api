using System;
using System.Collections.Generic;

namespace epiwebsurvey_api.Context.Entities
{
    public partial class SurveyResponseUser
    {
        public int UserId { get; set; }
        public Guid ResponseId { get; set; }

        public SurveyResponse Response { get; set; }
        public User User { get; set; }
    }
}
