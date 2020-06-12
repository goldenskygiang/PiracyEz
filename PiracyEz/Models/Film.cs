using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace PiracyEz.Models
{
    public class Film
    {
        public int FilmId { get; set; }
        [Required]
        public string Title { get; set; }
        [Required]
        public string Description { get; set; }
        [Required]
        public string FacebookId { get; set; }
        public string RawUrl { get; set; }
    }
}
