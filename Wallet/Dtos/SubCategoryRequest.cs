using System.ComponentModel.DataAnnotations;
using Wallet.Core.Models;

namespace Wallet.Dtos {
    public class CreateSubCategoryRequest {

        public string? SubCategoryName { get; set; } = null!;

        public bool? IsCustom { get; set; } = null!;

        public string? CategoryId { get; set; } = null!;
    }

    public class UpdateSubCategoryRequest
    {

        [Required]
        public string SubCategoryId { get; set; } = string.Empty;

        public string? NewSubCategoryName { get; set; } = null!;



    }
}
