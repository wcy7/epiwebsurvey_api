using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace epiwebsurvey_api.Models
{
    public class SurveyAnswerModel
    {
        public Guid SurveyId
        {
            get; set;
        }

        public Guid OrgKey
        {
            get; set;
        }

        public Guid PublisherKey
        {
            get; set;
        }

        public System.Collections.Generic.Dictionary<string, string> SurveyQuestionAnswerListField
        {
            get; set;
        }

    }
}
