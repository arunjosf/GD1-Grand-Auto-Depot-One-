using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using System;
using System.Collections.Concurrent;
using GD1.Application.Interfaces;
using GD1.Domain.Entities;
using GD1.Domain.Interfaces;

namespace GD1.Api.Hubs
{
    public class TrackingHub : Hub
    {
        // Tracks the last time a location was saved for a specific booking
        private static readonly ConcurrentDictionary<long, DateTime> _lastSaved = new();
        private readonly IGenericRepository<JourneyLocation> _locationRepo;

        public TrackingHub(IGenericRepository<JourneyLocation> locationRepo)
        {
            _locationRepo = locationRepo;
        }

        // Manager sends location updates here
        // The frontend manager app will call this method with the bookingId, lat, and lng
        public async Task UpdateLocation(long bookingId, double latitude, double longitude)
        {
            // 1. Instantly broadcast to anyone listening for this specific booking
            await Clients.Group(bookingId.ToString()).SendAsync("ReceiveLocationUpdate", latitude, longitude);

            // 2. Throttled Saving (Every 15 Seconds)
            var now = DateTime.UtcNow;
            if (!_lastSaved.TryGetValue(bookingId, out var lastTime) || (now - lastTime).TotalSeconds >= 15)
            {
                var newLocation = new JourneyLocation
                {
                    BookingId = bookingId,
                    Latitude = latitude,
                    Longitude = longitude,
                    Timestamp = now
                };
                
                await _locationRepo.AddAsync(newLocation);
                _lastSaved[bookingId] = now;
            }
        }

        // Vehicle Owner connects and calls this to join the group for their specific booking
        public async Task JoinTrackingGroup(long bookingId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, bookingId.ToString());
        }

        // Vehicle Owner disconnects or stops tracking
        public async Task LeaveTrackingGroup(long bookingId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, bookingId.ToString());
        }
    }
}
