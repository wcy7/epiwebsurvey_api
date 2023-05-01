using System;
using System.Collections.Generic;

namespace epiwebsurvey_api.Context.Entities
{
    public partial class States
    {
        public int StateProvinceId { get; set; }
        public string StateCode { get; set; }
        public string StateName { get; set; }
    }
}
