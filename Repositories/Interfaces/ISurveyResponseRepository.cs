using Epi.Web.Common.BusinessObject;
using Epi.Web.Common.Message;
using  epiwebsurvey_api.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace epiwebsurvey_api.Repositories.Interfaces
{
    public interface ISurveyResponseRepository
    {
        Guid SurveyId
        {
            get; set;
        }

        Guid OrgKey
        {
            get; set;
        }

        Guid PublisherKey
        {
            get; set;
        }
        PreFilledAnswerResponse SetSurveyAnswer(SurveyAnswerModel item, string strjson);
        void Remove(string id);
        PreFilledAnswerResponse Update(SurveyAnswerModel item, string ResponseId,string json);
        SurveyInfoBO GetSurveyInfoById(string SurveyId);
    }
}
