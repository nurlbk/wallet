using System.ComponentModel.DataAnnotations;
using Wallet.Core.Models;

namespace Wallet.Dtos {
    public class SubCategoryRequest {

        [Required]
        public string SubCategoryId { get; set; } = string.Empty;

        public string? SubCategoryName { get; set; } = null!;

        public bool? IsCustom { get; set; } = null!;

        public string? CategoryId { get; set; } = null!;
    }
}
