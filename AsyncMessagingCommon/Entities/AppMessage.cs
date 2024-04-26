using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AsyncMessagingCommon.Entities;

public class AppMessage
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    public string? Text { get; set; }
    public DateTime TimeStamp { get; set; }
    public int MailSent { get; set; } 
    public Guid Guid { get; set; }
}