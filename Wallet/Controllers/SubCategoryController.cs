using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Wallet.Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Wallet.Auth.Models;
using Wallet.Dtos;

namespace Wallet.Controllers {
    [ApiController]
    [Route("api/[controller]")]
    public class SubCategoryController : ControllerBase {

        private readonly ILogger<SubCategoryController> _logger;
        private readonly SubCategoryService _subCategoryService;
        private readonly CategoryService _categoryService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly WalletTransactionService _walletTransactionService;

        public SubCategoryController(ILogger<SubCategoryController> logger, SubCategoryService subCategoryService, CategoryService categoryService, UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager, WalletTransactionService walletTransactionService) {
            _logger = logger;
            _subCategoryService = subCategoryService;
            _categoryService = categoryService;
            _userManager = userManager;
            _roleManager = roleManager;
            _walletTransactionService = walletTransactionService;
        }






        [HttpGet]
        [Authorize]
        [Route(nameof(GetReccomendationSubCategories))]
        public async Task<SubCategory[]> GetReccomendationSubCategories() {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var subCategoryIds = await _walletTransactionService.GetRecommendationSubCategories(userId);

            var subCategories = new SubCategory[] { };

            foreach (var subCategoryId in subCategoryIds) {
                var subCategory = await _subCategoryService.GetAsyncById(subCategoryId);
                subCategories = subCategories.Append(subCategory).ToArray();
            }
            return subCategories.ToArray();

        }


        [HttpGet]
        [Route(nameof(GetById))]
        public async Task<SubCategory> GetById([FromQuery] string subCategoryId) {
            var subCategory = await _subCategoryService.GetAsyncById(subCategoryId);

            if (subCategory == null) {
                throw new ArgumentException($"SubCategory with id: {subCategoryId} not exists");

            }
            return subCategory;
        }

        [HttpGet]
        [Route(nameof(GetByName))]
        public async Task<SubCategory> GetByName([FromQuery] string subCategoryName) {
            var subCategory = await _subCategoryService.GetAsyncByName(subCategoryName);

            if (subCategory == null) {
                throw new ArgumentException($"SubCategory with name: {subCategoryName} not exists");

            }
            return subCategory;
        }

        [HttpGet]
        [Authorize]
        [Route(nameof(List))]
        public async Task<List<SubCategory>> List(string categoryIds) {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var user = await _userManager.FindByIdAsync(userId);
            var adminRoleGuid = await _roleManager.FindByNameAsync("ADMIN");

            if (user.Roles.Contains(adminRoleGuid.Id)) {
                return await _subCategoryService.GetListToAdminAsync();
            }
            return await _subCategoryService.GetListAsync(userId, categoryIds);
        }


        [HttpPost]
        [Authorize]
        [Route(nameof(Create))]
        public async Task<string> Create(SubCategoryRequest request) {
            var category = await _categoryService.GetAsyncById(request.CategoryId);
            if (category == null) {
                return "category not exists";
            }

            var subCategoryFromDb = await _subCategoryService.GetAsyncByName(request.SubCategoryName);

            if (!request.IsCustom.Value && !category.IsCustom.Value) {
                if (subCategoryFromDb != null) {
                    return "subCategory already exists";
                }

                var newSubCategory = new SubCategory() {
                    Name = request.SubCategoryName,
                    IsActive = true,
                    IsCustom = false,
                    CategoryId = category.CategoryId
                };

                await _subCategoryService.CreateAsync(newSubCategory);

                return "new subCategory created";

            }


            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);


            if (subCategoryFromDb != null) {

                subCategoryFromDb.UserIds4Custom = subCategoryFromDb.UserIds4Custom.Append(userId).ToArray();
                await _subCategoryService.UpdateAsync(subCategoryFromDb.CategoryId, subCategoryFromDb);


                return "new custom subCategory created";
            }
            var newCustomCategory = new SubCategory() {
                Name = request.SubCategoryName,
                IsActive = true,
                IsCustom = true,
                UserIds4Custom = new string[] { userId },
                CategoryId = category.CategoryId
            };


            await _subCategoryService.CreateAsync(newCustomCategory);

            return "new custom subCategory created";
        }

        [HttpPost]
        [Authorize]
        [Route(nameof(Update))]
        public async Task<SubCategory> Update(SubCategoryRequest request) {
            var subCategoryFromDb = await _subCategoryService.GetAsyncById(request.SubCategoryId);

            if (subCategoryFromDb == null) {
                throw new ArgumentException($"SubCategory with id: {request.SubCategoryId} not exists");
            }
            subCategoryFromDb.Name = request.SubCategoryName ?? subCategoryFromDb.Name;           

            await _subCategoryService.UpdateAsync(
                request.SubCategoryId,
                subCategoryFromDb
            );

            var updatedSubCategory = await _subCategoryService.GetAsyncById(request.SubCategoryId);

            if (updatedSubCategory == null) {
                throw new ArgumentException("SubCategory not exists");

            }
            return updatedSubCategory;
        }


        [HttpPost]
        [Route(nameof(Delete))]
        public async Task<bool> Delete(string subCategoryId) {
            var subCategoryFromDb = await _subCategoryService.GetAsyncById(subCategoryId);

            if (subCategoryFromDb == null) {
                throw new ArgumentException($"SubCategory with id: {subCategoryId} not exists");
            }

            await _subCategoryService.InactiveAsync(
                subCategoryId
            );

            return true;
        }

    }
}