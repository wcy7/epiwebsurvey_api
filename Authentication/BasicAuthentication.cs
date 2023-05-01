using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace epiwebsurvey_api.Authentication
{
    public class BasicAuthentication : AuthenticationSchemeOptions
    {
        public string Realm { get; set; }

        






    }



    public class BasicAuthenticationHandler
    {
        private readonly IBasicAuthenticationService _authenticationService;

        public BasicAuthenticationHandler(
            IOptionsMonitor<BasicAuthenticationOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock,
            IBasicAuthenticationService authenticationService)
            : base(options, logger, encoder, clock)
        {
            _authenticationService = authenticationService;
        }

    }


}
