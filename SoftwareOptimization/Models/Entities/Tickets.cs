using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace SoftwareOptimization.Models.Entities {
    public class Ticket {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Title { get; set; }
        public string Body { get; set; }
        public DateTime? CreatedAt { get; set; }
        [ForeignKey("UserId")]
        public User User { get; set; }
    }
}
