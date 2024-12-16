using DanskLogistikAPI.Models;
using DanskLogistikAPI.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Collections;
using System.Runtime.CompilerServices;

namespace DanskLogistikAPI.Services.SVGGenerator
{
    public interface IPathfinder
    {
        public Task<Ticket?> findPath(string Start, string End, DateTime startTime, bool allowFly = true, bool allowSea = true, bool allowRail = true, bool allowRoad = true); 
    }

    public class Pathfinder (IMapRepository _mapRepository) : IPathfinder
    {
        IMapRepository mapRepository = _mapRepository;

        /// <summary>
        /// Compare nodes, used in Dijkstra pathfinding, the first element is the closest
        /// </summary>
        private class CompareNodes : IComparer<(Node, DateTime?)>
        {
            public int Compare((Node, DateTime?) x, (Node, DateTime?) y)
            {
                int TimeCompare;
                if (x.Item2==null)
                {
                    if (y.Item2==null)
                        TimeCompare = 0;
                    else
                        TimeCompare = 1;
                }
                else
                {

                    if (y.Item2==null)
                        TimeCompare = -1;
                    else
                    {
                        TimeCompare= ((DateTime)x.Item2).CompareTo((DateTime)y.Item2);
                    }
                }
                    
                if (TimeCompare == 0)
                {
                    return x.Item1.Id.CompareTo(y.Item1.Id);
                }
                else
                    return TimeCompare;
            }
        }

        public async Task<Ticket?> findPath(string Start, string End, DateTime startTime, bool allowFly = true, bool allowSea = true, bool allowRail = true, bool allowRoad = true)
        {
            var Nodes = mapRepository.GetAllNode();

            var StartNode = await Nodes.FirstOrDefaultAsync(N => N.Name == Start);
            var EndNode   = await Nodes.FirstOrDefaultAsync(N => N.Name == End);

            if (StartNode == null)
                throw new Exception($"Node {Start} not found");
            if (EndNode == null)
                throw new Exception($"Node {End} not found");
            

            
            //Dijkstra's algorithm

            //A queue, sorted so the nearest nodes go first, also stores the parent node (null if unvisited or origin)
            SortedSet<(Node, DateTime?)> Queue = new(new CompareNodes());

            Dictionary<Node, NodeMapping?> Parents=new();
            Dictionary<Node, DateTime?> Times=new();

            HashSet<Node> Visited=new();
            
            foreach (var N in Nodes)
            {
                Times[N] = null;
            }

            Times[StartNode] = startTime;
            Queue.Add((StartNode, startTime));

            Parents.Add(StartNode,null);
            bool found = false;
            while (Queue.Any() && !found )
            {

                //Get the nearest node, in terms of time
                var This = Queue.First();

                //If we got to something which do not have an arrival time, we know all nodes are disconnected
                if (This.Item2 == null)
                    break;

                //Check all neighbours
                foreach (var Nmaps in This.Item1.Neighbors)
                {
                    //If we have already visited it, we already have a better way, and if we are not allowed to travel through this mode, skip it
                    if (!Visited.Contains(Nmaps.End) &&
                        (//Check that we are allowed to consider this mode, due to custom permissions
                            (Nmaps.Connection.mode==Connection.Mode.Air && allowFly) ||
                            (Nmaps.Connection.mode==Connection.Mode.Road && allowRoad) ||
                            (Nmaps.Connection.mode==Connection.Mode.Sea && allowSea) ||
                            (Nmaps.Connection.mode==Connection.Mode.Rail && allowRail) 
                        ) && Nmaps.Start.Location.Owner.Access && Nmaps.End.Location.Owner.Access
                        )
                    {

                        //Get when we would arrive here by this particular mode of transport
                        var That = Nmaps.End;
                        DateTime newTime = ((DateTime)This.Item2) +Nmaps.Connection.Time;

                        //If this is the better way to get there, this is the new parent node
                        if (Times[That] == null || Times[That] > newTime)
                        {
                            Times[That] = newTime;
                            Parents[That] = Nmaps;

                            if (That.Id == EndNode.Id)//We are done! hooray
                            {
                                found = true;
                                break;
                            }
                            
                            //We need to remove the existing from the queue to trigger a re-sorting
                            Queue.Remove(Queue.FirstOrDefault(X=> X.Item1.Id==That.Id));
                            Queue.Add((That,newTime));
                        }

                    }
                }

                Visited.Add(This.Item1);//Well, we have certainly been here now
                Queue.Remove(This);
            }

            if (found && Parents[EndNode]!=null)
            {
                //Build up the ticked in reverse, by re-traversing through the parents from our destination
                TransitStep? NextStep=null;
                int steps = 0;
                for (NodeMapping? NM = Parents[EndNode]; NM != null; NM = Parents[NM.Start])
                {
                    //There is no practical way this is null, this just gets the compiler to SHUT UP ABOUT IT ALREADY
                    var NotNullDammit= Times[NM.Start];
                    if (NotNullDammit!=null)
                    {
                        TransitStep ThisStep = new TransitStep
                        {
                            next = NextStep,
                            Departure = (DateTime)(NotNullDammit),
                            Via= NM,
                        };
                        NextStep = ThisStep;
                    }
                    ++steps;
                }

                if (NextStep != null)
                    return new Ticket
                    {
                        status = Ticket.StatusType.Ongoing,
                        Destination = EndNode,
                        Steps = steps,
                        CurrentStep = NextStep,
                        FirstStep = NextStep,

                    };

            }
            return null;

            //Queue.Add(StartNode,startTime); 
        }
    }
}
