using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;
using System.Threading;

namespace entitylastfm
{
    public class Band
    {
        [Key] 
        public string Name { get; set; }
        public string Bio { get; set; }
        public Genre MainGenre { get; set; }

    }
}
