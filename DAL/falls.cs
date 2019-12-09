namespace DAL
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class Fall
    {
        [Key]
        [Column(Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int id { get; set; }

        [Key]
        [Column(Order = 1)]
        public double x { get; set; }

        [Key]
        [Column(Order = 2)]
        public double y { get; set; }

        [Key]
        [Column(Order = 3)]
        public DateTime date { get; set; }

        [Key]
        [Column(Order = 4)]
        public System.Boolean isGeotagged { get; set; }

        // This is supposed to represent the distance
        // between the original estimated location
        // and the actual location (found with the
        // geotagged imaged)
        [Key]
        [Column(Order = 5)]
        public double actDist { get; set; }

        public override string ToString()
        {
            return "ID: " + id + ", LAT: " + x.ToString("F3") + ", LON: " + 
                y.ToString("F3") + ", DATE: " + date.Day.ToString() + "/" +
                date.Month.ToString() + "/" + date.Year.ToString();
        }
    }
}
