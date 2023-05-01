using AutoMapper;
using Epi.Web.Common.BusinessObject;
using  epiwebsurvey_api.Context.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace epiwebsurvey_api.Mapper
{
    public class MappingProfile:Profile
    {
        public MappingProfile()
        {
           // CreateMap<QuestionModel, SurveyInfoBO>();
           // CreateMap<SurveyInfoBO, QuestionModel>();
           // CreateMap<QuestionDTO, QuestionModel>();
           // CreateMap<QuestionModel, QuestionDTO>();
           // CreateMap<Models.QuestionDetaiModel, Epi.Web.Common.DTO.QuestionDetaiModel>();
           // CreateMap<Models.QuestionDetaiModel, Epi.Web.Common.DTO.QuestionDetaiModel>()
               // .ForMember(dest => dest.Question_Listvalues, opt => opt.MapFrom(src => src.Question_Listvalues));
           // CreateMap<QuestionModel, QuestionDTO>()
               // .ForMember(dest => dest.QuestionDetailList, opt => opt.MapFrom(src => src.QuestionDetailList));

            CreateMap<SurveyInfoBO, SurveyMetaData>()
                .ForMember(d => d.ClosingDate, o => o.MapFrom(s => s.ClosingDate))
                  .ForMember(d => d.SurveyId, o => o.MapFrom(s => s.SurveyId))
                    .ForMember(d => d.SurveyName, o => o.MapFrom(s => s.SurveyName))
                      .ForMember(d => d.TemplateXml, o => o.MapFrom(s => s.XML))
                        .ForMember(d => d.IsDraftMode, o => o.MapFrom(s => s.IsDraftMode))
                          .ForMember(d => d.SurveyTypeId, o => o.MapFrom(s => s.SurveyType))
                           .ForMember(d => d.UserPublishKey, o => o.MapFrom(s => s.UserPublishKey))
                           .ForMember(d => d.DateCreated, o => o.MapFrom(s => s.DateCreated))
                          .ForMember(d => d.LastUpdate, o => o.MapFrom(s => s.LastUpdate))
                           .ForMember(d => d.IsSqlproject, o => o.MapFrom(s => s.IsSqlProject))
                             .ForMember(d => d.TemplateXmlsize, o => o.MapFrom(s => s.TemplateXMLSize))
                           .ForAllOtherMembers(opts => opts.Ignore());

            CreateMap<SurveyMetaData, SurveyInfoBO>()
                .ForMember(d => d.ClosingDate, o => o.MapFrom(s => s.ClosingDate))
                  .ForMember(d => d.SurveyId, o => o.MapFrom(s => s.SurveyId))
                    .ForMember(d => d.SurveyName, o => o.MapFrom(s => s.SurveyName))
                      .ForMember(d => d.XML, o => o.MapFrom(s => s.TemplateXml))
                        .ForMember(d => d.IsDraftMode, o => o.MapFrom(s => s.IsDraftMode))
                          .ForMember(d => d.SurveyType, o => o.MapFrom(s => s.SurveyTypeId))
                           .ForMember(d => d.UserPublishKey, o => o.MapFrom(s => s.UserPublishKey))
                             .ForMember(d => d.DateCreated, o => o.MapFrom(s => s.DateCreated))
                          .ForMember(d => d.LastUpdate, o => o.MapFrom(s => s.LastUpdate))
                           .ForMember(d => d.IsSqlProject, o => o.MapFrom(s => s.IsSqlproject))
                             .ForMember(d => d.TemplateXMLSize, o => o.MapFrom(s => s.TemplateXmlsize))
                           .ForAllOtherMembers(opts => opts.Ignore());
            CreateMap<SurveyResponse, SurveyResponseBO>();
            CreateMap<SurveyResponseBO, SurveyResponse>();
        }

    }
}
