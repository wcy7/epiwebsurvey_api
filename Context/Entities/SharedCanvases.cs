using System;
using System.Collections.Generic;

namespace epiwebsurvey_api.Context.Entities
{
    public partial class SharedCanvases
    {
        public int CanvasId { get; set; }
        public int UserId { get; set; }

        public User User { get; set; }
    }
}
