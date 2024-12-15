using DanskLogistikAPI.Models;
using DanskLogistikAPI.Repositories;
using System.Runtime.CompilerServices;

namespace DanskLogistikAPI.Services.SVGGenerator
{
    public interface IPathfinder
    {
        public void findPath(string Start, string End, DateTime startTime);
    }

    public class Pathfinder (IMapRepository _mapRepository) : IPathfinder
    {
        IMapRepository mapRepository = _mapRepository;


        private class CompareNodes : IComparer<(Node, DateTime)>
        {
            public int Compare((Node, DateTime) x, (Node, DateTime) y)
            {
                var TimeCompare = x.Item2.CompareTo(y.Item2);
                if (TimeCompare == 0)
                {
                    return x.Item1.Id.CompareTo(y.Item1.Id);
                }
                else
                    return TimeCompare;
            }
        }

        public void findPath(string Start, string End, DateTime startTime)
        {
            var Nodes = mapRepository.GetAllNode();

            var StartNode = Nodes.FirstOrDefault(N => N.Name == Start);
            var EndNode   = Nodes.FirstOrDefault(N => N.Name == End);

            if (StartNode == null)
                throw new Exception($"Node {Start} not found");
            if (EndNode == null)
                throw new Exception($"Node {End} not found");
            

            
            //Dijkstra's algorithm

            //How long till the given Node? and where did we come from (Null means we do not arrive, or we have not found previous node)
            Dictionary<Node, (DateTime?,Node?)> Times=new();

            //A queue, sorted so the nearest nodes go first
            SortedSet<(Node, DateTime)> Queue = new(new CompareNodes());

            Queue.Add((StartNode,startTime));

            while (Queue.Any())
            {
                //Get the nearest node, in terms of time
                var This = Queue.First();
                Console.WriteLine($"Search {This.Item1.Name} at {This.Item2}");
                foreach (var N in This.Item1.Neighbors)
                {
                    Console.WriteLine($"\t{N.End.Name} at {N.Connection.Time}");

                }
                Queue.Remove(This);

            }
            //Queue.Add(StartNode,startTime); 



        }

    }
}
