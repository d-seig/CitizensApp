using DTO;

namespace Web.Models
{
    public class CitizenResponse
    {
        public List<CitizenDTO> Items { get; set; } = [];
        public int TotalCount { get; set; }
    }
}
