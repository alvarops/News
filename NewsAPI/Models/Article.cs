using Microsoft.WindowsAzure.Storage.Table.DataServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NewsAPI.Models
{
    public class Article
    {
        public int ArticleId { set; get; }
        public string Title { set; get; }
        public string Summary { set; get; }
        public string PermLink { set; get; }
        public DateTime Published { set; get; }
        public virtual Feed Feed { set; get; }
    }
}