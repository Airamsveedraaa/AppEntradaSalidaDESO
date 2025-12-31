using System;
using System.Collections.Generic;
using System.Linq;
using AppEntradaSalidaDESO.Models;

namespace AppEntradaSalidaDESO.Algorithms
{
    public static class SimulationHelper
    {
        public record InterceptionResult(double ArrivalTimeAtTrack, DiskRequest Request);

        /// <summary>
        /// Finds if any pending request intercepts the head's path before it reaches the target.
        /// </summary>
        public static InterceptionResult? FindEarliestIntercept(
            int currentTrack,
            int targetTrack,
            double currentTime,
            double timePerTrack,
            List<DiskRequest> pendingQueue,
            string direction) // "up" or "down" (or "asc"/"desc")
        {
            if (pendingQueue == null || pendingQueue.Count == 0) return null;

            // Normalize direction string
            bool isAscending = direction.ToLower().Contains("up") || direction.ToLower().Contains("asc");

            // Filter requests that are strictly in the path
            var possibleIntercepts = pendingQueue.Where(req =>
            {
                if (isAscending)
                    return req.Position > currentTrack && req.Position < targetTrack;
                else
                    return req.Position < currentTrack && req.Position > targetTrack;
            }).ToList();

            if (possibleIntercepts.Count == 0) return null;

            // Sort by proximity to current track
            possibleIntercepts.Sort((a, b) => Math.Abs(a.Position - currentTrack).CompareTo(Math.Abs(b.Position - currentTrack)));

            foreach (var req in possibleIntercepts)
            {
                double distance = Math.Abs(req.Position - currentTrack);
                double arrivalTimeAtTrack = currentTime + (distance * timePerTrack);

                // Check if request arrives before or exactly when head passes
                if (req.ArrivalTime <= arrivalTimeAtTrack)
                {
                    return new InterceptionResult(arrivalTimeAtTrack, req);
                }
            }

            return null;
        }

        // Helper to clone list to avoid modifying original reference
        public static List<DiskRequest> CloneRequests(List<DiskRequest> requests)
        {
            return requests.Select(r => new DiskRequest(r.Position, r.Order, r.ArrivalTime) { IsProcessed = r.IsProcessed, OriginalIndex = r.OriginalIndex }).ToList();
        }
    }
}
