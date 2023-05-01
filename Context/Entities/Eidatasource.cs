using System;
using System.Collections.Generic;

namespace epiwebsurvey_api.Context.Entities
{
    public partial class Eidatasource
    {
        public int DatasourceId { get; set; }
        public string DatasourceServerName { get; set; }
        public string DatabaseType { get; set; }
        public string InitialCatalog { get; set; }
        public string PersistSecurityInfo { get; set; }
        public Guid? SurveyId { get; set; }
        public string DatabaseUserId { get; set; }
        public string Password { get; set; }

        public SurveyMetaData Survey { get; set; }
    }
}
