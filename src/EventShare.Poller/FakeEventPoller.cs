using EventShare.Data;
using EventShare.Data.Enums;
using EventShare.Data.Models;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace EventShare.Poller
{
    public class FakeEventPoller : IEventPoller
    {
        private readonly EventShareDbContext _eventShareDbContext;

        public FakeEventPoller(EventShareDbContext eventShareDbContext)
        {
            _eventShareDbContext = eventShareDbContext;
        }

        public async Task<IEnumerable<Event>> DoPollAsync(CancellationToken cancellationToken)
        {
            if (_currentPoll != PollResults.Count) return PollResults[_currentPoll++];

            _currentPoll = 0;

            var eventsToRemove = _eventShareDbContext.Events
                .Where(e => e.Title == "Fake Event #1" || 
                            e.Title == "Fake Event #2" || 
                            e.Title == "Fake Event #3" ||
                            e.Title == "Fake Event #4");

            await eventsToRemove
                .ForEachAsync(e =>
                              {
                                  var entity = _eventShareDbContext.Find<Event>(e.Id);
                                  _eventShareDbContext.Entry(entity).State = EntityState.Deleted;
                              }, cancellationToken)
                .ConfigureAwait(false);

            await _eventShareDbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            return Enumerable.Empty<Event>();
        }

        #region Fake Data

        private static int _currentPoll;

        private List<IEnumerable<Event>> PollResults => new List<IEnumerable<Event>>
        {
            new List<Event>
            {
                new Event
                {
                    Id = ObjectId.GenerateNewId(DateTime.Now.AddHours(1)).ToString(),
                    Title = "Fake Event #1", 
                    Details = "Some fake details of Event #1.",
                    DateAndTime = DateTime.Today.AddDays(2), 
                    Status = EventStatus.Published
                }
            },
            new List<Event>
            {
                new Event
                {
                    Id = ObjectId.GenerateNewId(DateTime.Now.AddHours(2)).ToString(),
                    Title = "Fake Event #2", 
                    Details = "Some fake details of Event #2.",
                    DateAndTime = DateTime.Today.AddDays(1), 
                    Status = EventStatus.Published
                },
                new Event
                {
                    Id = ObjectId.GenerateNewId(DateTime.Now.AddHours(3)).ToString(),
                    Title = "Fake Event #1", 
                    Details = "Some fake details of Event #1.",
                    DateAndTime = DateTime.Today.AddDays(2), 
                    Status = EventStatus.Published
                }
            },
            new List<Event>
            {
                new Event
                {
                    Id = ObjectId.GenerateNewId(DateTime.Now.AddHours(4)).ToString(),
                    Title = "Fake Event #2", 
                    Details = "Some fake details of Event #2.",
                    DateAndTime = DateTime.Today.AddDays(1), 
                    Status = EventStatus.Published
                },
                new Event
                {
                    Id = ObjectId.GenerateNewId(DateTime.Now.AddHours(5)).ToString(),
                    Title = "Fake Event #1", 
                    Details = "Some fake details of Event #1.",
                    DateAndTime = DateTime.Today.AddDays(2), 
                    Status = EventStatus.Published
                },
                new Event
                {
                    Id = ObjectId.GenerateNewId(DateTime.Now.AddHours(6)).ToString(),
                    Title = "Fake Event #3", 
                    Details = "Some fake details of Event #3.",
                    DateAndTime = DateTime.Today.AddDays(3), 
                    Status = EventStatus.Published
                },
                new Event
                {
                    Id = ObjectId.GenerateNewId(DateTime.Now.AddHours(7)).ToString(),
                    Title = "Fake Event #4",
                    Details = "Some fake details of Event #4.",
                    DateAndTime = DateTime.Today.AddDays(4),
                    Status = EventStatus.Published
                }
            }
        };

        #endregion
    }
}
