using Microsoft.WindowsAzure.Storage.Table;
using Microsoft.WindowsAzure.Storage.Table.DataServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
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

        public ITableEntity toArticleEntity()
        {
            ArticleEntity entity = new ArticleEntity(Feed, Title){
                Summary = Summary,
                PermLink = PermLink,
                Published = Published
            };
            return entity;
        }
    }

    public class ArticleEntity : TableEntity
    {
        public ArticleEntity(Feed feed, string title)
        {
            this.PartitionKey = feed == null? "Unknown":feed.Name;
            this.RowKey = title.GetHashCode().ToString();
            this.Title = title;
        }

        public ArticleEntity() { }

        public string Title { set; get; }
        public string Summary { set; get; }
        public string PermLink { set; get; }
        public DateTime Published { set; get; }

        public Article ToArticle()
        {
            return new Article()
            {
                ArticleId = Int32.Parse(RowKey),
                Title = Title,
                Summary = Summary,
                PermLink = PermLink,
                Published = Published
            };
        }
    }
}