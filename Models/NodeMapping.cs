namespace DanskLogistikAPI.Models
{
    //Mapping which connects this node, to its road, and its other end
    public class NodeMapping 
    {
        public int Id { get; set; }
        public Connection Connection { get; set; } = null!;

        public Node Start { get; set; } = null!;
        public Node End { get; set; } = null!;
    }
}
