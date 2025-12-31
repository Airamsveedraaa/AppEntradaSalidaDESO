using AppEntradaSalidaDESO.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AppEntradaSalidaDESO.Algorithms
{
    public class SCANNAlgorithm : IDiskSchedulingAlgorithm
    {
        public string Name => "SCAN-N";
        public string Description => "N-Step SCAN: Atiende peticiones en lotes de tamaño N";
        public bool RequiresDirection => true;

        public ExerciseResult Execute(
            int initialPosition, 
            List<DiskRequest> requests, 
            int minCylinder, 
            int maxCylinder, 
            string direction = "up", 
            double timePerTrack = 1.0, 
            double timePerRequest = 0.0)
        {
            // Default N=2 if not specified (Wait, interface doesn't have N yet. I need to update interface first or assume a default/hack)
            // I will update the interface in the next step. For now I'll use a hardcoded default or property if I can.
            // But Execute signature is fixed. 
            // I will assume I updating the interface to include 'nStep'
            return Execute(initialPosition, requests, minCylinder, maxCylinder, direction, timePerTrack, timePerRequest, 2);
        }

        public ExerciseResult Execute(
            int initialPosition,
            List<DiskRequest> requests,
            int minCylinder,
            int maxCylinder,
            string direction,
            double timePerTrack,
            double timePerRequest,
            int nStep)
        {
            var result = new ExerciseResult(Name, initialPosition, requests.Select(r => r.Position).ToList());
            
            var pendingArchive = requests.OrderBy(r => r.ArrivalTime).ThenBy(r => r.OriginalIndex).ToList();
            
            int currentTrack = initialPosition;
            double currentTime = 0.0;
            bool goingUp = direction == "up";

            // Simulation Loop
            while (pendingArchive.Any())
            {
                // 1. Formar Lote (Batch)
                var arrived = pendingArchive.Where(r => r.ArrivalTime <= currentTime).ToList();

                if (!arrived.Any())
                {
                    if (pendingArchive.Any())
                    {
                         // Wait logic similar to reference
                         // Go to edge if empty? Reference says yes.
                         int targetEdge = goingUp ? maxCylinder : minCylinder;
                         
                         if (currentTrack != targetEdge) {
                             int dist = Math.Abs(targetEdge - currentTrack);
                             double moveTime = dist * timePerTrack;
                             
                             result.DetailedSteps.Add(new AlgorithmStep
                             {
                                 From = currentTrack,
                                 To = targetEdge,
                                 Distance = dist,
                                 Remaining = new List<int>(),
                                 Instant = currentTime,
                                 Buffer = new List<int>()
                             });
                             currentTrack = targetEdge;
                             currentTime += moveTime;
                             goingUp = !goingUp;
                         }

                         var nextRequest = pendingArchive.First();
                         if (currentTime < nextRequest.ArrivalTime)
                         {
                             // Wait step
                             currentTime = nextRequest.ArrivalTime;
                         }
                         
                         arrived = pendingArchive.Where(r => r.ArrivalTime <= currentTime).ToList();
                    }
                    else
                    {
                        break;
                    }
                }

                // Take N
                var batch = arrived.Take(nStep).ToList();
                
                // Remove from archive
                foreach (var r in batch) pendingArchive.Remove(r);

                var activeQueue = batch.ToList();

                // 2. SCAN on batch
                while (activeQueue.Any())
                {
                    // Check current track
                    var atCurrent = activeQueue.FirstOrDefault(r => r.Position == currentTrack);
                    if (atCurrent != null)
                    {
                        result.DetailedSteps.Add(new AlgorithmStep
                        {
                            From = currentTrack,
                            To = currentTrack,
                            Distance = 0,
                            Remaining = activeQueue.Select(r => r.Position).ToList(), // Queue state
                            Instant = currentTime,
                            ArrivalInstant = atCurrent.ArrivalTime
                        });
                        result.ProcessingOrder.Add(currentTrack);
                        currentTime += timePerRequest;
                        activeQueue.Remove(atCurrent);
                        continue;
                    }

                    int targetTrack = -1;
                    DiskRequest targetRequest = null;
                    bool isEdgeMove = false;

                    if (goingUp)
                    {
                        var above = activeQueue.Where(r => r.Position > currentTrack).OrderBy(r => r.Position).ToList();
                        if (above.Any())
                        {
                            targetRequest = above.First();
                            targetTrack = targetRequest.Position;
                        }
                        else
                        {
                            targetTrack = maxCylinder;
                            isEdgeMove = true;
                        }
                    }
                    else
                    {
                        var below = activeQueue.Where(r => r.Position < currentTrack).OrderByDescending(r => r.Position).ToList();
                        if (below.Any())
                        {
                            targetRequest = below.First();
                            targetTrack = targetRequest.Position;
                        }
                        else
                        {
                            targetTrack = minCylinder;
                            isEdgeMove = true;
                        }
                    }

                    if (currentTrack == targetTrack)
                    {
                        goingUp = !goingUp;
                        continue;
                    }

                    // Check for intercepts INSIDE the batch
                    // "Mini-SCAN"
                    var actualDest = targetTrack;
                    
                    // Logic: find closest request in ActiveQueue in current direction
                    // Actually, we already picked the closest 'above' or 'below'.
                    // Since activeQueue is fixed, there are no "new arrivals" effectively.
                    // So direct move is correct.
                    // Except if 'targetTrack' is Edge, but there is a request at Edge? 
                    // No, if request at edge, it would be in above/below list.
                    
                    // So just move.
                    int distance = Math.Abs(actualDest - currentTrack);
                    double moveTime = distance * timePerTrack;

                    result.DetailedSteps.Add(new AlgorithmStep
                    {
                        From = currentTrack,
                        To = actualDest,
                        Distance = distance,
                        Remaining = activeQueue.Select(r => r.Position).ToList(),
                        Instant = currentTime,
                        ArrivalInstant = targetRequest?.ArrivalTime ?? 0 // Approx
                    });

                    if (!isEdgeMove)
                    {
                         result.ProcessingOrder.Add(actualDest);
                    }

                    currentTrack = actualDest;
                    currentTime += moveTime;

                    if (!isEdgeMove)
                    {
                        currentTime += timePerRequest;
                        activeQueue.Remove(targetRequest);
                    }
                    else
                    {
                        goingUp = !goingUp;
                    }
                }
            }

            result.TotalHeadMovement = result.DetailedSteps.Sum(s => s.Distance);
            result.TotalTime = currentTime;
            
            // Calculate access times
            // We can delegate this or just populate if needed. 
            // MainViewModel calls formatted output.
            
            return result;
        }
    }
}
