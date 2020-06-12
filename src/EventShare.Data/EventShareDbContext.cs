using Blueshift.EntityFrameworkCore.MongoDB.Annotations;
using Blueshift.EntityFrameworkCore.MongoDB.Infrastructure;
using EventShare.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace EventShare.Data
{
    [MongoDatabase("eventShare")]
    public class EventShareDbContext : DbContext
    {
        public DbSet<Event> Events { get; set; }

        public EventShareDbContext()
            : this(new DbContextOptions<EventShareDbContext>())
        {
        }

        public EventShareDbContext(DbContextOptions<EventShareDbContext> eventShareDbContextOptions)
            : base(eventShareDbContextOptions)
        {
        }
    }
}
