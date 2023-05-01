using System;
using System.Collections.Generic;

namespace epiwebsurvey_api.Context.Entities
{
    public partial class LkSurveyType
    {
        public LkSurveyType()
        {
            SurveyMetaData = new HashSet<SurveyMetaData>();
        }

        public int SurveyTypeId { get; set; }
        public string SurveyType { get; set; }

        public ICollection<SurveyMetaData> SurveyMetaData { get; set; }
    }
}
