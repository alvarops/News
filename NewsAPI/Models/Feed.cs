using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace NewsAPI.Models
{
    [Serializable]
    [DataContract]
    public class Feed
    {
        [DataMember]
        public int FeedId { get; set; }
        [Required]
        [DataMember]
        public string Name { get; set; }
        [Required]
        [DataMember]
        public string Url { get; set; }
        [IgnoreDataMember]
        public virtual ICollection<User> Users { get; set; }

        public Feed ()
        {
            Users = new List<User>();
        }
    }
}