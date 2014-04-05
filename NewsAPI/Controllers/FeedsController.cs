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

namespace NewsAPI.Controllers
{
    public class FeedsController : ApiController
    {
        private NewsAPIContext db = new NewsAPIContext();

        // GET api/Feeds
        public IQueryable<Feed> GetFeeds()
        {
            return db.Feeds;
        }

        // GET api/Feeds/5
        [ResponseType(typeof(Feed))]
        public IHttpActionResult GetFeed(int id)
        {
            Feed feed = db.Feeds.Find(id);
            if (feed == null)
            {
                return NotFound();
            }

            return Ok(feed);
        }

        // PUT api/Feeds/5
        public IHttpActionResult PutFeed(int id, Feed feed)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != feed.FeedId)
            {
                return BadRequest();
            }

            db.Entry(feed).State = EntityState.Modified;

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!FeedExists(id))
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

        // POST api/Feeds
        [ResponseType(typeof(Feed))]
        public IHttpActionResult PostFeed(Feed feed)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            db.Feeds.Add(feed);
            db.SaveChanges();

            return CreatedAtRoute("DefaultApi", new { id = feed.FeedId }, feed);
        }

        // DELETE api/Feeds/5
        [ResponseType(typeof(Feed))]
        public IHttpActionResult DeleteFeed(int id)
        {
            Feed feed = db.Feeds.Find(id);
            if (feed == null)
            {
                return NotFound();
            }

            db.Feeds.Remove(feed);
            db.SaveChanges();

            return Ok(feed);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool FeedExists(int id)
        {
            return db.Feeds.Count(e => e.FeedId == id) > 0;
        }
    }
}