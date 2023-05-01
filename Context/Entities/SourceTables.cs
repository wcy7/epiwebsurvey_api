using System;
using System.Collections.Generic;

namespace epiwebsurvey_api.Context.Entities
{
    public partial class SourceTables
    {
        public string SourceTableName { get; set; }
        public Guid FormId { get; set; }
        public string SourceTableXml { get; set; }
    }
}
