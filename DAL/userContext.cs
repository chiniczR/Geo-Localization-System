namespace DAL
{
    using System;
    using System.Data.Entity;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;

    public partial class userContext : DbContext
    {
        public userContext()
            : base("name=userContext")
        {
        }

        public virtual DbSet<user> users { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<user>()
                .Property(e => e.username)
                .IsFixedLength();

            modelBuilder.Entity<user>()
                .Property(e => e.firstname)
                .IsFixedLength();

            modelBuilder.Entity<user>()
                .Property(e => e.lastname)
                .IsFixedLength();

            modelBuilder.Entity<user>()
                .Property(e => e.password)
                .IsFixedLength();

            modelBuilder.Entity<user>()
                .Property(e => e.role)
                .IsFixedLength();
        }
    }
}
