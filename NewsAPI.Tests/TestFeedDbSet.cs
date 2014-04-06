using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NewsAPI.Models;

namespace NewsAPI.Tests
{
    class TestFeedDbSet : TestDbSet<Feed>
    {
        public override Feed Find(params object[] keyValues)
        {
            return this.SingleOrDefault(feed => feed.FeedId == (int)keyValues.Single());
        }
    }
}
