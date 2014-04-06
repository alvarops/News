using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NewsAPI.Models;

namespace NewsAPI.Tests
{
    class TestArticleDbSet : TestDbSet<Article>
    {
        public override Article Find(params object[] keyValues)
        {
            return this.SingleOrDefault(article => article.ArticleId == (int)keyValues.Single());
        }
    }
}
