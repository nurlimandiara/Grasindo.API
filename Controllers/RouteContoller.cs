using System;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Google.OrTools.ConstraintSolver;
using Grasindo.API.Models;

namespace Grasindo.API.Controllers
{

    public class Student
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    [ApiController]
    [Route("/")]
    public class RouteController : ControllerBase
    {
        [HttpPost]
        public ActionResult Post(RouteData data)
        {
            
            var jsonString = JsonSerializer.Serialize(data);
            return Ok(solve(data));
        }


        private void loadData()
        {
        
        }

        private static LongLongToLong createWeightCallback(RoutingIndexManager manager, RouteData data)
        {
            // Create a callback to return the weight between points.
            return (long fromIndex, long toIndex) =>
                {
                    // Convert from routing variable Index to distance matrix NodeIndex.
                    var fromNode = manager.IndexToNode(fromIndex);
                    var toNode = manager.IndexToNode(toIndex);
                    return data.weights[fromNode][ toNode];
                };
        }

        private static LongToLong createDemandCallback(RoutingIndexManager manager, RouteData data)
        {
            // Create a callback to get demands at each location.
            return (long fromIndex) =>
             {
                // Convert from routing variable Index to demand NodeIndex.
                var fromNode = manager.IndexToNode(fromIndex);
                 return data.demands[fromNode];
             };

        }

        private static void addCapacityConstraints(
            RoutingModel routing,
            RoutingIndexManager manager,
            RouteData data,
            int demandCallbackIndex)
            {
                // Add capacity constraints.
                routing.AddDimensionWithVehicleCapacity(
                demandCallbackIndex, 0, // null capacity slack
                data.vehicleCapacities, // vehicle maximum capacities
                true,                   // start cumul to zero
                "Capacity");
            }
        
        private static LongLongToLong createTimeCallback(RoutingIndexManager manager,RouteData data)
        {
            // Create a callback to get total times between locations.
            return (long fromIndex, long toIndex)=>
            {
                var fromNode = manager.IndexToNode(fromIndex);
                var toNode = manager.IndexToNode(toIndex);

                // Get the service time to the specified location
                var servTime = data.serviceTimes[fromNode];

                // Get the travel times between two locations
                var travTime = data.weights[fromNode][toNode];

                return servTime + travTime;
            };
        }


        private static void addTimeWindowConstraints(
            RoutingModel routing,
            RoutingIndexManager manager,
            RouteData data,
            int timeCallbackIndex
        )
        {
            // Add time window constraints.
            var horizon = 120;
            routing.AddDimension(
                timeCallbackIndex,  // transit callback
                horizon,            // allow waiting time
                horizon,            // vehicle maximum capacities
                // Don't force start cumul to zero. This doesn't have any effect in this example,
                // since the depot has a start window of (0, 0).
                false,              // start cumul to zero
                "Time");

            var timeDimension = routing.GetDimensionOrDie("Time");
            // Add time window constraints for each location except depot.
            for (int i = 1; i < data.timeWindows.GetLength(0); ++i) {
            long index = manager.NodeToIndex(i);
            timeDimension.CumulVar(index).SetRange(
                data.timeWindows[i][ 0],
                data.timeWindows[i][ 1]);
            }

        }

        // [START solution_printer]
        /// <summary>
        ///   Print the solution.
        /// </summary>
        public static void PrintSolution(
            in RouteData data,
            in RoutingModel routing,
            in RoutingIndexManager manager,
            in Assignment solution)
        {
            RoutingDimension timeDimension = routing.GetMutableDimension("Time");
            // Inspect solution.
            long totalTime = 0;
            for (int i = 0; i < data.vehicleNumber; ++i)
            {
                Console.WriteLine("Route for Vehicle {0}:", i);
                var index = routing.Start(i);
                while (routing.IsEnd(index) == false)
                {
                    var timeVar = timeDimension.CumulVar(index);
                    Console.Write("{0} Time({1},{2}) -> ",
                        manager.IndexToNode(index),
                        solution.Min(timeVar),
                        solution.Max(timeVar));
                    index = solution.Value(routing.NextVar(index));
                }
                var endTimeVar = timeDimension.CumulVar(index);
                Console.WriteLine("{0} Time({1},{2})",
                    manager.IndexToNode(index),
                    solution.Min(endTimeVar),
                    solution.Max(endTimeVar));
                Console.WriteLine("Time of the route: {0}min", solution.Min(endTimeVar));
                totalTime += solution.Min(endTimeVar);
            }
            Console.WriteLine("Total time of all routes: {0}min", totalTime);
        }
        // [END solution_printer]

        private string solve(RouteData data)
        {
            //var data = new RouteData();

            // Create the Routing Index Manager and Routing Model
            var manager = new RoutingIndexManager(data.weights.Length,data.vehicleNumber,data.depot);
            var routing = new RoutingModel(manager);

            // Define weight of each edge
            var weightCallbackIndex = routing.RegisterTransitCallback(createWeightCallback(manager,data));
            routing.SetArcCostEvaluatorOfAllVehicles(weightCallbackIndex);

            // Add capacity constraints
            var demandCallback = createDemandCallback(manager, data);
            var demandCallbackIndex = routing.RegisterUnaryTransitCallback(demandCallback);
            addCapacityConstraints(routing, manager, data, demandCallbackIndex);

            // Add time window constraints
            var timeCallbackIndex = routing.RegisterTransitCallback(createTimeCallback(manager, data));
            addTimeWindowConstraints(routing, manager, data, timeCallbackIndex);

            // Set first solution heuristic (cheapest addition)
            var searchParameters = operations_research_constraint_solver.DefaultRoutingSearchParameters();
            searchParameters.FirstSolutionStrategy = FirstSolutionStrategy.Types.Value.PathCheapestArc;
            
            // Solve the problem
            Assignment solution;
            
            try
            {
                solution = routing.SolveWithParameters(searchParameters);
                PrintSolution(data,routing,manager,solution);
                
                return JsonSerializer.Serialize(solution);
            }
            catch (System.Exception)
            {
                return "Failed";
            }
            
        }
    }
}