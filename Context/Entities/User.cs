using System;
using System.Collections.Generic;

namespace epiwebsurvey_api.Context.Entities
{
    public partial class User
    {
        public User()
        {
            Canvas = new HashSet<Canvas>();
            DatasourceUser = new HashSet<DatasourceUser>();
            SharedCanvases = new HashSet<SharedCanvases>();
            SurveyMetaDataUser = new HashSet<SurveyMetaDataUser>();
            SurveyResponseUser = new HashSet<SurveyResponseUser>();
            UserOrganization = new HashSet<UserOrganization>();
        }

        public int UserId { get; set; }
        public string UserName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PasswordHash { get; set; }
        public bool ResetPassword { get; set; }
        public string EmailAddress { get; set; }
        public string PhoneNumber { get; set; }
        public Guid? Uguid { get; set; }

        public ICollection<Canvas> Canvas { get; set; }
        public ICollection<DatasourceUser> DatasourceUser { get; set; }
        public ICollection<SharedCanvases> SharedCanvases { get; set; }
        public ICollection<SurveyMetaDataUser> SurveyMetaDataUser { get; set; }
        public ICollection<SurveyResponseUser> SurveyResponseUser { get; set; }
        public ICollection<UserOrganization> UserOrganization { get; set; }
    }
}
