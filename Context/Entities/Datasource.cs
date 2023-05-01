using System;
using System.Collections.Generic;

namespace epiwebsurvey_api.Context.Entities
{
    public partial class Datasource
    {
        public Datasource()
        {
            Canvas = new HashSet<Canvas>();
            DatasourceUser = new HashSet<DatasourceUser>();
        }

        public int DatasourceId { get; set; }
        public string DatasourceName { get; set; }
        public int OrganizationId { get; set; }
        public string DatasourceServerName { get; set; }
        public string DatabaseType { get; set; }
        public string InitialCatalog { get; set; }
        public string PersistSecurityInfo { get; set; }
        public bool Eiwsdatasource { get; set; }
        public Guid? EiwssurveyId { get; set; }
        public string DatabaseUserId { get; set; }
        public string Password { get; set; }
        public string DatabaseObject { get; set; }
        public bool? Sqlquery { get; set; }
        public string Sqltext { get; set; }
        public bool Active { get; set; }
        public string Portnumber { get; set; }

        public Organization Organization { get; set; }
        public ICollection<Canvas> Canvas { get; set; }
        public ICollection<DatasourceUser> DatasourceUser { get; set; }
    }
}
