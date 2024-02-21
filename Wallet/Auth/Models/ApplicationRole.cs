using System;
using AspNetCore.Identity.MongoDbCore.Models;
using MongoDbGenericRepository.Attributes;

namespace Wallet.Auth.Models {
    [CollectionName("roles")]
    public class ApplicationRole : MongoIdentityRole<Guid> {

    }
}

