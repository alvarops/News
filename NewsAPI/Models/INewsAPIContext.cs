using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;

namespace NewsAPI.Models
{
    public interface INewsAPIContext : IDisposable
    {
        DbSet<User> Users { get; }
        DbSet<Feed> Feeds { get; }
       // DbSet<Article> Articles { get; }

        int SaveChanges();
        void MarkAsModified(Object item);
    }
}
