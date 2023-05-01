using System;
using System.Collections.Generic;

namespace epiwebsurvey_api.Context.Entities
{ 
    public partial class Admin
    {
        public Admin()
        {
            Address = new HashSet<Address>();
        }

        public Guid AdminId { get; set; }
        public string AdminEmail { get; set; }
        public int OrganizationId { get; set; }
        public bool? IsActive { get; set; }
        public bool Notify { get; set; }
        public string LastName { get; set; }
        public string FirstName { get; set; }
        public string PhoneNumber { get; set; }

        public Organization Organization { get; set; }
        public ICollection<Address> Address { get; set; }
    }
}
