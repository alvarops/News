using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;
using NewsAPI.Models;
using System.Collections;

namespace NewsAPI.Controllers
{
    public class UsersController : ApiController
    {
        private INewsAPIContext db = new NewsAPIContext();

        public UsersController ()
        {
           // db.Configuration.ProxyCreationEnabled = false;
        }

        public UsersController(INewsAPIContext context)
        {
            db = context;
            // db.Configuration.ProxyCreationEnabled = false;
        }

        // GET api/Users
        public IQueryable<User> GetUsers()
        {
            return db.Users.Include(u => u.Feeds);
        }

        // GET api/Users/5
        [ResponseType(typeof(User))]
        public IHttpActionResult GetUser(int id)
        {
            User user = db.Users.Include(u => u.Feeds).Where(u => u.UserId == id).First<User>();
            if (user == null)
            {
                return NotFound();
            }

            return Ok(user);
        }

        // GET api/Users/5/articles
        public IQueryable<Article> GetArticles(int id, string full)
        {
            User user = db.Users.Include(u => u.Feeds).Where(u => u.UserId == id).FirstOrDefault<User>();
            List<Article> articles = new List<Article>();
            if (user == null)
            {
                return articles.AsQueryable();
            }
            foreach (Feed feed in user.Feeds)
            {
                var arts = db.Articles.Where(a => a.Feed.FeedId == feed.FeedId);
                if (articles == null)
                    articles = arts.ToList<Article>();
                else
                    articles.AddRange(arts.ToList<Article>());
            }
            return articles.AsQueryable();
        }

        // PUT api/Users/5
        public IHttpActionResult PutUser(int id, User user)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != user.UserId)
            {
                return BadRequest();
            }

           // db.Entry(user).State = EntityState.Modified;
            db.MarkAsModified(user);
            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return StatusCode(HttpStatusCode.NoContent);
        }

        // POST api/Users
        [ResponseType(typeof(User))]
        public IHttpActionResult PostUser(User user)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            db.Users.Add(user);
            db.SaveChanges();

            return CreatedAtRoute("DefaultApi", new { id = user.UserId }, user);
        }

        // DELETE api/Users/5
        [ResponseType(typeof(User))]
        public IHttpActionResult DeleteUser(int id)
        {
            User user = db.Users.Find(id);
            if (user == null)
            {
                return NotFound();
            }

            db.Users.Remove(user);
            db.SaveChanges();

            return Ok(user);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool UserExists(int id)
        {
            return db.Users.Count(e => e.UserId == id) > 0;
        }

    }
}