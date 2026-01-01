using System;
using System.Collections.Generic;
using System.Linq;
using AppEntradaSalidaDESO.Models;

namespace AppEntradaSalidaDESO.Algorithms
{
    public class LOOKNAlgorithm : IDiskSchedulingAlgorithm
    {
        public string Name => "LOOK-N";
        public string Description => "LOOK con paso N: Atiende peticiones en lotes de tamaño N en la dirección actual.";
        public bool RequiresDirection => true;

        public ExerciseResult Execute(int initialPosition, List<DiskRequest> requests, int minCylinder, int maxCylinder, string direction, double timePerTrack, double timePerRequest, int nStep)
        {
            var result = new ExerciseResult("LOOK-N", initialPosition, requests.Select(r => r.Position).ToList())
            {
                Direction = direction,
                ProcessingOrder = new List<int>(),
                DetailedSteps = new List<AlgorithmStep>()
            };

            // 1. Queue all requests by Arrival Time (FIFO)
            var mainQueue = requests.OrderBy(r => r.ArrivalTime).ThenBy(r => r.OriginalIndex).ToList();
            
            int currentHead = initialPosition;
            double currentTime = 0.0;
            string currentDirection = direction; // "up" or "down"

            // Log initial state
            result.DetailedSteps.Add(new AlgorithmStep
            {
                From = currentHead,
                To = currentHead,
                Distance = 0,
                Instant = currentTime,
                Remaining = mainQueue.Select(r => r.Position).ToList(),
                Buffer = new List<int>()
            });

            while (mainQueue.Count > 0)
            {
                // 2. Fetch up to N ARRIVED requests
                // If nothing has arrived, jump time
                var arrived = mainQueue.Where(r => r.ArrivalTime <= currentTime).ToList();
                if (arrived.Count == 0)
                {
                    if (mainQueue.Count > 0)
                    {
                        var next = mainQueue.OrderBy(r => r.ArrivalTime).First();
                        currentTime = next.ArrivalTime;
                        continue; 
                    }
                    else
                    {
                        break; 
                    }
                }

                // Take N from FIFO
                var buffer = arrived.Take(nStep).ToList();
                
                // Remove from main queue
                foreach (var req in buffer) mainQueue.Remove(req);

                // 3. Process Buffer using LOOK logic
                var activeBatch = buffer.ToList();

                while (activeBatch.Count > 0)
                {
                    DiskRequest nextRequest = null;

                    if (currentDirection == "up")
                    {
                        // Try finding closest >= currentHead
                        var cleanBatch = activeBatch.OrderBy(r => r.Position).ToList();
                        var candidates = cleanBatch.Where(r => r.Position >= currentHead).ToList();

                        if (candidates.Count > 0)
                        {
                            nextRequest = candidates.First();
                        }
                        else
                        {
                            // No requests above. 
                            // LOOK: Just reverse direction. Do NOT go to edge.
                            currentDirection = "down";
                            continue; // Re-evaluate in new direction
                        }
                    }
                    else // down
                    {
                        // Try finding closest <= currentHead
                        var cleanBatch = activeBatch.OrderByDescending(r => r.Position).ToList();
                        var candidates = cleanBatch.Where(r => r.Position <= currentHead).ToList();

                        if (candidates.Count > 0)
                        {
                            nextRequest = candidates.First();
                        }
                        else
                        {
                            // No requests below. Reverse.
                            currentDirection = "up";
                            continue;
                        }
                    }

                    // Move to request
                    int distance = Math.Abs(nextRequest.Position - currentHead);
                    double travelTime = distance * timePerTrack;

                    // Log Step BEFORE processing (to show 'Siguiente', 'Pendientes' etc)
                    // Logic check: Reference table shows line at T=0 with "Pendientes" (the full batch).
                    // Then next line T=1 shows result of move.
                    
                    int from = currentHead;
                    currentHead = nextRequest.Position;
                    currentTime += travelTime;

                    result.TotalHeadMovement += distance;
                    result.ProcessingOrder.Add(nextRequest.Position);
                    activeBatch.Remove(nextRequest);

                    // Add step
                    result.DetailedSteps.Add(new AlgorithmStep
                    {
                        From = from,
                        To = currentHead,
                        Distance = distance,
                        Instant = currentTime, // Arrival at destination
                        // Show remaining MAIN QUEUE + remaining BATCH in "Remaining" list? 
                        // User wants "Pendientes" column. Usually this means all unfinished requests.
                        Remaining = mainQueue.Select(r => r.Position).Concat(activeBatch.Select(r => r.Position)).ToList(),
                        Buffer = buffer.Select(r => r.Position).ToList() // Show original batch content or remaining?
                    });

                    // Add processing time
                    currentTime += timePerRequest;
                }
            }

            result.TotalTime = currentTime;
            return result;
        }
    }
}
