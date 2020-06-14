using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Markt.Models
{
    public class Category
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public ICollection<Subcategory> Subcategories { get; set; }
    }
}
