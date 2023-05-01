using System;
using System.Collections.Generic;

namespace epiwebsurvey_api.Context.Entities
{
    public partial class UserOrganization
    {
        public int UserId { get; set; }
        public int OrganizationId { get; set; }
        public int RoleId { get; set; }
        public bool Active { get; set; }

        public Organization Organization { get; set; }
        public Role Role { get; set; }
        public User User { get; set; }
    }
}
