using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Wallet.Auth.Models;
using Wallet.Core.Models;
using Wallet.Dtos;

namespace Wallet.Controllers {
    [ApiController]
    [Route("api/[controller]")]
    public class WalletTransactionController : ControllerBase {

        private readonly ILogger<WalletTransactionController> _logger;
        private readonly WalletTransactionService _walletTransactionService;
        private readonly SubCategoryService _subCategoryService;
        private readonly CategoryService _categoryService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;

        public WalletTransactionController(ILogger<WalletTransactionController> logger, WalletTransactionService walletTransactionService, SubCategoryService subCategoryService, CategoryService categoryService, UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager) {
            _logger = logger;
            _walletTransactionService = walletTransactionService;
            _subCategoryService = subCategoryService;
            _categoryService = categoryService;
            _userManager = userManager;
            _roleManager = roleManager;
        }


        [HttpGet]
        [Authorize]
        [Route(nameof(GetNetAmount))]
        public async Task<long> GetNetAmount() {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return await _walletTransactionService.CalculateNetAmount(userId);
        }




        [HttpGet]
        [Route(nameof(GetById))]
        public async Task<WalletTransaction> GetById([FromQuery] string walletTransactionId) {
            var walletTransaction = await _walletTransactionService.GetAsyncById(walletTransactionId);
            if (walletTransaction == null) {
                throw new ArgumentException($"WalletTransaction with id: {walletTransactionId} not exists");
            }
            return walletTransaction;
        }


        [HttpGet]
        [Authorize]
        [Route(nameof(List))]
        public async Task<List<WalletTransaction>> List() {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var user = await _userManager.FindByIdAsync(userId);
            var adminRoleGuid = await _roleManager.FindByNameAsync("ADMIN");

            if (user.Roles.Contains(adminRoleGuid.Id)) {
                return await _walletTransactionService.GetListToAdminAsync();
            }
            return await _walletTransactionService.GetListAsync(userId);
        }


        [HttpPost]
        [Authorize]
        [Route(nameof(Create))]
        public async Task<string> Create(WalletTransactionRequest request) {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
             
            try {
                var subCategory = await _subCategoryService.GetAsyncById(request.SubCategoryId);

                if (subCategory == null) {
                    return "subCategory not exists";
                }
                await _walletTransactionService.CreateAsync(new WalletTransaction {
                    UserId = userId,
                    SubCategoryId = request.SubCategoryId,
                    Amount = request.Amount,
                    OnTime = DateTime.Now,
                    Description = request.Description,
                    TransactionType = request.TransactionType,
                    IsActive = true
                });

                return "Wallet Transaction created";

            } catch(Exception _) {
                return "Error";

            }
        }

        [HttpPost]
        [Authorize]
        [Route(nameof(Update))]
        public async Task<string> Update([FromQuery] string walletTransactionId, WalletTransactionRequest request) {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            try {

                var walletTransaction = await _walletTransactionService.GetAsyncById(walletTransactionId);

                if (walletTransaction == null) {
                    return "wallet Transaction not exists";
                }


                var subCategory = await _subCategoryService.GetAsyncById(request.SubCategoryId);

                if (subCategory == null) {
                    return "subCategory not exists";
                }


                await _walletTransactionService.UpdateAsync(walletTransactionId, new WalletTransaction {
                    UserId = userId,
                    SubCategoryId = request.SubCategoryId ?? walletTransaction.SubCategoryId,
                    Amount = request.Amount ?? walletTransaction.Amount,
                    OnTime = walletTransaction.OnTime,
                    Description = request.Description ?? walletTransaction.Description
                });

                return "Wallet Transaction updated";

            } catch (Exception _) {
                return "Error";

            }
        }


        [HttpPost]
        [Route(nameof(Delete))]
        public async Task<bool> Delete(string walletTransactionId) {
            var walletTransactionFromDb = await _walletTransactionService.GetAsyncById(walletTransactionId);

            if (walletTransactionFromDb == null) {
                throw new ArgumentException($"WalletTransaction with id: {walletTransactionId} not exists");
            }

            await _walletTransactionService.InactiveAsync(
                walletTransactionId
            );

            return true;
        }

    }
}