using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace NewsAPI.Models
{
    public class User
    {
        public int UserId { get; set;}
        [Required]
        public string Name { get; set; }
        public virtual ICollection<Feed> Feeds { get; set; }
        public User ()
        {
            Feeds = new List<Feed>();
        }
    }
}