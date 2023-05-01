using System;
using System.Collections.Generic;

namespace epiwebsurvey_api.Context.Entities
{
    public partial class SurveyMetaDataUser
    {
        public int UserId { get; set; }
        public Guid FormId { get; set; }

        public SurveyMetaData Form { get; set; }
        public User User { get; set; }
    }
}
