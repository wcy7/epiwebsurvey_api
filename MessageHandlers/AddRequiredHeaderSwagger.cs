using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Swashbuckle.AspNetCore.Swagger;
using Microsoft.OpenApi.Any;

namespace epiwebsurvey_api.MessageHandlers
{
    public class AddRequiredHeaderSwagger: IOperationFilter
    {     
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {         
            if(context.MethodInfo.Name=="RefreshToken")
            {
                return;
            }
                if (operation.Parameters == null)
                operation.Parameters = new List<OpenApiParameter>();
            operation.Parameters.Add(new OpenApiParameter
            {
                Name = "SurveyId",
                In = ParameterLocation.Header,
                Required = true,
               // AllowEmptyValue=false,                
                Schema = new OpenApiSchema
                {
                    Type = "string"                    
                }
               
            });
        }
    }
}
