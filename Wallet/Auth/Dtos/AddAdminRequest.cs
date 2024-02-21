using System.ComponentModel.DataAnnotations;

namespace Wallet.Auth.Dtos {
    public class AddAdminRequest
    {
        [Required, EmailAddress]
        public string UserEmail { get; set; } = string.Empty;       
    }
}
