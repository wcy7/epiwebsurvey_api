using System;
using System.Collections.Generic;

namespace epiwebsurvey_api.Context.Entities
{
    public partial class DatasourceUser
    {
        public int DatasourceId { get; set; }
        public int UserId { get; set; }

        public Datasource Datasource { get; set; }
        public User User { get; set; }
    }
}
