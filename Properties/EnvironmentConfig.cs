using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace epiwebsurvey_api.Properties
{
    public class EnvironmentConfig
    {

        public string client_id { get; set; }
        public string client_secret { get; set; }
        public string endpoint_authorization { get; set; }
        public string endpoint_token { get; set; }
        public string endpoint_user_info { get; set; }
        public string endpoint_token_validation { get; set; }
        public string endpoint_user_info_sys { get; set; }
        public string callback_url { get; set; }

        public string epiinfosurveyapiContext { get; set; }
    }
}
