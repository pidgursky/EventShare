using EventShare.Data;
using EventShare.Data.Enums;
using EventShare.Data.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EventShare.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class EventController : ControllerBase
    {
        private readonly EventShareDbContext _eventShareDbContext;

        public EventController(EventShareDbContext eventShareDbContext)
        {
            _eventShareDbContext = eventShareDbContext;
        }

        [HttpGet("all")]
        public async Task<IEnumerable<Event>> GetAll()
        {
            return await _eventShareDbContext.Events.ToListAsync();
        }

        [HttpGet("actual")]
        public async Task<IEnumerable<Event>> GetActual()
        {
            return await _eventShareDbContext.Events
                .Where(e => e.Status == EventStatus.Published && e.DateAndTime >= DateTime.Now)
                .ToArrayAsync();
        }

        [HttpGet("user/{userId}")]
        public async Task<IEnumerable<Event>> GetForUser(string userId)
        {
            return await _eventShareDbContext.Events
                .Where(e => e.PublisherId == userId)
                .ToListAsync();
        }

        [HttpGet("{id}")]
        public Task<Event> Get(string id)
        {
            return _eventShareDbContext.Events.FirstOrDefaultAsync(m => m.Id == id);
        }

        [HttpPost]
        public async Task<Event> Post(Event @event)
        {
            _eventShareDbContext.Events.Add(@event);
            await _eventShareDbContext.SaveChangesAsync();
            return @event;
        }

        [HttpPut("{id}")]
        public async Task Put(string id, Event @event)
        {
            var eventToEdit = await _eventShareDbContext.Events.FirstOrDefaultAsync(e => e.Id == id);
            if (eventToEdit == null)
            {
                throw new ArgumentException($"Event with Id:{@event.Id} does not exist.");
            }

            eventToEdit.Title = @event.Title;
            eventToEdit.Details = @event.Details;
            eventToEdit.DateAndTime = @event.DateAndTime;
            eventToEdit.Status = @event.Status;

            await _eventShareDbContext.SaveChangesAsync();
        }

        [HttpDelete("{id}")]
        public async Task Delete(string id)
        {
            var @event = await _eventShareDbContext.Events.FirstOrDefaultAsync(m => m.Id == id);
            _eventShareDbContext.Events.Remove(@event);
            await _eventShareDbContext.SaveChangesAsync();
        }
    }
}
