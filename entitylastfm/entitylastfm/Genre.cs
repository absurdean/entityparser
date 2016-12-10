using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace entitylastfm
{
    public class Genre
    {
        [Key]
        public string Name { get; set; }
        public virtual List<Band> ArtistList { get; set; }

    }
}
