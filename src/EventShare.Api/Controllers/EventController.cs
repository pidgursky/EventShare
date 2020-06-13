using EventShare.Data;
using EventShare.Data.Enums;
using EventShare.Data.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace EventShare.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class EventController : ControllerBase
    {
        private readonly EventShareDbContext _eventShareDbContext;
        private readonly IDatabase _redisCache;
        private readonly ILogger<EventController> _logger;

        public EventController(EventShareDbContext eventShareDbContext, IDatabase redisCache, ILogger<EventController> logger)
        {
            _eventShareDbContext = eventShareDbContext;
            _redisCache = redisCache;
            _logger = logger;
        }

        [HttpGet("all")]
        public async Task<IEnumerable<Event>> GetAll()
        {
            return await _eventShareDbContext.Events.ToListAsync().ConfigureAwait(false);
        }

        [HttpGet("actual")]
        public async Task<IEnumerable<Event>> GetActual()
        {
            return await GetActual(null).ConfigureAwait(false);
        }

        [HttpGet("actual/{userId}")]
        public async Task<IEnumerable<Event>> GetActual(string userId)
        {
            IEnumerable<Event> actualEvents;

            try
            {
                var cacheKey = userId == null ? "Events" : $"Event:{userId}";
                var events = await _redisCache.StringGetAsync(cacheKey).ConfigureAwait(false);
                if (events.HasValue)
                {
                    actualEvents = JsonConvert.DeserializeObject<Event[]>(events);
                }
                else
                {
                    actualEvents = await GetActualInternal(userId).ConfigureAwait(false);
                    _redisCache.StringSet(cacheKey, JsonConvert.SerializeObject(actualEvents), TimeSpan.FromSeconds(30));
                }

            }
            catch (Exception ex)
            {
                _logger.LogError("An error occured while caching events.", ex);
                return await GetActualInternal(userId).ConfigureAwait(false);
            }

            return actualEvents;
        }

        [HttpGet("user/{userId}")]
        public async Task<IEnumerable<Event>> GetForUser(string userId)
        {
            return await _eventShareDbContext.Events
                .Where(e => e.PublisherId == userId)
                .ToListAsync()
                .ConfigureAwait(false);
        }

        [HttpGet("{id}")]
        public async Task<Event> Get(string id)
        {
            var @event = await _eventShareDbContext.Events
                .FirstOrDefaultAsync(m => m.Id == id)
                .ConfigureAwait(false);

            @event.LikersCount = await _eventShareDbContext.EventLikers
                .CountAsync(el => el.EventId == @event.Id)
                .ConfigureAwait(false);

            return @event;
        }

        [HttpPost]
        public async Task<Event> Post(Event @event)
        {
            _eventShareDbContext.Events.Add(@event);
            await _eventShareDbContext.SaveChangesAsync().ConfigureAwait(false);
            return @event;
        }

        [HttpPut("{id}")]
        public async Task Put(string id, Event @event)
        {
            var eventToEdit = await _eventShareDbContext.Events
                .FirstOrDefaultAsync(e => e.Id == id)
                .ConfigureAwait(false);

            if (eventToEdit == null)
            {
                throw new ArgumentException($"Event with Id:{@event.Id} does not exist.");
            }

            eventToEdit.Title = @event.Title;
            eventToEdit.Details = @event.Details;
            eventToEdit.DateAndTime = @event.DateAndTime;
            eventToEdit.Status = @event.Status;

            await _eventShareDbContext.SaveChangesAsync().ConfigureAwait(false);
        }

        [HttpDelete("{id}")]
        public async Task Delete(string id)
        {
            var @event = await _eventShareDbContext.Events
                .FirstOrDefaultAsync(m => m.Id == id)
                .ConfigureAwait(false);

            _eventShareDbContext.Events.Remove(@event);

            await _eventShareDbContext.SaveChangesAsync().ConfigureAwait(false);
        }

        [HttpGet("like/{eventId}/{userId}")]
        public async Task ToggleLike(string eventId, string userId)
        {
            var liker = await _eventShareDbContext.EventLikers
                .FirstOrDefaultAsync(el => el.EventId == eventId && el.UserId == userId)
                .ConfigureAwait(false);

            if (liker != null)
            {
                _eventShareDbContext.EventLikers.Remove(liker);
            }
            else
            {
                liker = new EventLiker {EventId = eventId, UserId = userId};
                _eventShareDbContext.EventLikers.Add(liker);
            }

            await _eventShareDbContext.SaveChangesAsync().ConfigureAwait(false);
        }

        public async Task<IEnumerable<Event>> GetActualInternal(string userId)
        {
            var actualEvents = await _eventShareDbContext.Events
                .Where(e => e.Status == EventStatus.Published && e.DateAndTime >= DateTime.Now)
                .ToArrayAsync()
                .ConfigureAwait(false);

            foreach (var @event in actualEvents)
            {
                var likers = await _eventShareDbContext.EventLikers
                    .Where(el => el.EventId == @event.Id)
                    .Select(el => el.UserId)
                    .ToArrayAsync()
                    .ConfigureAwait(false);

                @event.LikersCount = likers.Length;
                @event.Liked = likers.Any(l => l == userId);
            }

            return actualEvents;
        }
    }
}
