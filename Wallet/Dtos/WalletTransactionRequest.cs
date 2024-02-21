using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;
using Wallet.Core.Models;

namespace Wallet.Dtos {
    public class WalletTransactionRequest {

        [Required]
        public string SubCategoryId { get; set; } = string.Empty;

        public long? Amount { get; set; } = null!;

        public string? Description { get; set; } = string.Empty;

        public TransactionType? TransactionType { get; set; } = null!;

    }
}
