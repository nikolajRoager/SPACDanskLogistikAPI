namespace DanskLogistikAPI.DTOs
{
    public class NodeMappingDTO
    {
        public int Id { get; set; }
        public int ConnectionId { get; set; }
        public int EndId { get; set; }
        public string EndName { get; set; } = null!;
        public int startId {  get; set; }
        public string startName { get; set; } = null!;
    }
}
