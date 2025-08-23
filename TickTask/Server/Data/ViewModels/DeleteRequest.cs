using System.ComponentModel.DataAnnotations;

namespace TickTask.Data.ViewModels
{
    public class DeleteRequest
    {
        [Required, EmailAddress]
        public string EmailAddress { get; set; }
    }
}
