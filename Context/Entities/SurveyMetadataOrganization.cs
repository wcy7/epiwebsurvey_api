using System;
using System.Collections.Generic;

namespace epiwebsurvey_api.Context.Entities
{
    public partial class SurveyMetadataOrganization
    {
        public Guid SurveyId { get; set; }
        public int OrganizationId { get; set; }

        public Organization Organization { get; set; }
        public SurveyMetaData Survey { get; set; }
    }
}
