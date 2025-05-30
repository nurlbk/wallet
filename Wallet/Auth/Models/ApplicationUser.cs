﻿using AspNetCore.Identity.MongoDbCore.Models;
using MongoDbGenericRepository.Attributes;

namespace Wallet.Auth.Models {
    [CollectionName("users")]
    public class ApplicationUser : MongoIdentityUser<Guid> {
        public string FullName { get; set; } = string.Empty;
    }
}

