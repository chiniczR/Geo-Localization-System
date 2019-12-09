namespace DAL
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class user
    {
        [Key]
        [Column(Order = 0)]
        [StringLength(10)]
        public string username { get; set; }

        [StringLength(10)]
        public string firstname { get; set; }

        [StringLength(10)]
        public string lastname { get; set; }

        [Key]
        [Column(Order = 1)]
        [StringLength(10)]
        public string password { get; set; }

        [Key]
        [Column(Order = 2)]
        [StringLength(10)]
        public string role { get; set; }

        [Key]
        [Column(Order = 3)]
        [StringLength(200)]
        public string photoUrl { get; set; }
    }
}
