using System.ComponentModel.DataAnnotations;

namespace Todoki.Data.ViewModels
{
    public class DeleteRequest
    {
        [Required, EmailAddress]
        public string EmailAddress { get; set; }
    }
}
