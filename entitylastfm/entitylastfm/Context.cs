using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Entity;

namespace entitylastfm
{
    class Context:DbContext
    {
        public DbSet<Band> Bands { get; set; }
        public DbSet<Genre> Genres { get; set; }

        public Context()
            : base("EntDb")
        { }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Band>().HasOptional(x => x.MainGenre).WithMany(x => x.ArtistList);
        }
    }
}
