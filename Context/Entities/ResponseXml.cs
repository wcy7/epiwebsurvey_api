using System;
using System.Collections.Generic;

namespace epiwebsurvey_api.Context.Entities
{
    public partial class ResponseXml
    {
        public Guid ResponseId { get; set; }
        public string Xml { get; set; }
        public int? UserId { get; set; }
        public bool? IsNewRecord { get; set; }
    }
}
