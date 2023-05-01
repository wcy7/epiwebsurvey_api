using System;
using System.Collections.Generic;

namespace epiwebsurvey_api.Context.Entities
{
    public partial class ResponseDisplaySettings
    {
        public Guid FormId { get; set; }
        public string ColumnName { get; set; }
        public int SortOrder { get; set; }

        public SurveyMetaData Form { get; set; }
    }
}
