using System;
using System.Collections.Generic;

namespace epiwebsurvey_api.Context.Entities
{
    public partial class DataAccessRules
    {
        public int RuleId { get; set; }
        public string RuleName { get; set; }
        public string RuleDescription { get; set; }
    }
}
