using System;
using System.Collections.Generic;

namespace epiwebsurvey_api.Context.Entities
{
    public partial class SurveyMetaDataView
    {
        public Guid SurveyId { get; set; }
        public string ViewTableName { get; set; }
    }
}
