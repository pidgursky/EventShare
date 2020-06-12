﻿using System.Collections.Generic;
using System.Threading.Tasks;
using EventShare.Data.Models;

namespace EventShare.Web.Services
{
    public interface IEventService
    {
        Task<IEnumerable<Event>> GetAllEventsAsync();
        Task<IEnumerable<Event>> GetActualEventsAsync(string userId);
        Task<IEnumerable<Event>> GetUserEventsAsync(string userId);
        Task<Event> GetEventAsync(string id);
        Task<Event> CreateEventAsync(Event @event);
        Task EditEventAsync(string id, Event @event);
        Task DeleteEventAsync(string id);
        Task ToggleLikeAsync(string eventId, string userId);
    }
}