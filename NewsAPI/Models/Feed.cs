using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace NewsAPI.Models
{
    public class Feed
    {
        public int FeedId { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string Url { get; set; }
    }
}