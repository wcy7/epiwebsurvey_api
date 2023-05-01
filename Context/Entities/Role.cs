using System;
using System.Collections.Generic;

namespace epiwebsurvey_api.Context.Entities
{
    public partial class Role
    {
        public Role()
        {
            UserOrganization = new HashSet<UserOrganization>();
        }

        public int RoleId { get; set; }
        public int RoleValue { get; set; }
        public string RoleDescription { get; set; }

        public ICollection<UserOrganization> UserOrganization { get; set; }
    }
}
