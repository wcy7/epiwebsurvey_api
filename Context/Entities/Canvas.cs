using System;
using System.Collections.Generic;

namespace epiwebsurvey_api.Context.Entities
{
    public partial class Canvas
    {
        public int CanvasId { get; set; }
        public Guid CanvasGuid { get; set; }
        public string CanvasName { get; set; }
        public int UserId { get; set; }
        public string CanvasDescription { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
        public int DatasourceId { get; set; }
        public string CanvasContent { get; set; }

        public Datasource Datasource { get; set; }
        public User User { get; set; }
    }
}
