using Heracles.Core.Models.EMR;
using System;

namespace Heracles.Application.Models
{
    public class ImageEntry
    {
        public ImageEntry(ISeries series, string fieldName)
        {
            CreationDate = series.CreationDate;
            Description = series.Description;
            FieldName = fieldName;
            Location = series.Location;
            LesionDepth = series.LesionDepth;
            NumberOfInstances = series.NumberOfInstances;
            Type = "Ultrasound";
        }

        public DateTime CreationDate { get; set; }
        public string Description { get; set; }
        public string ThumbNailFilename { get; set; }
        public string Type { get; set; }
        public string FieldName { get; set; }
        public string Location { get; set; }
        public int NumberOfInstances { get; set; }
        public double LesionDepth { get; set; }
    }
}
