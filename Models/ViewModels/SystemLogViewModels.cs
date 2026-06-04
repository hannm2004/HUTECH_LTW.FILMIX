using System.Collections.Generic;
using untitled1.Models.Entities;

namespace untitled1.Models.ViewModels
{
    public class SystemLogIndexViewModel
    {
        public IEnumerable<SystemLog> Logs { get; set; } = new List<SystemLog>();
        public string? Search { get; set; }
        public string ActionFilter { get; set; } = "all";
        public List<string> ActionTypes { get; set; } = new();
        public int Page { get; set; } = 1;
        public int TotalPages { get; set; }
        public int TotalCount { get; set; }
    }
}
