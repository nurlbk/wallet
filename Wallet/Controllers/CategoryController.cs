using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Wallet.Auth.Models;
using Wallet.Core.Models;

namespace Wallet.Controllers {
    [ApiController]
    [Route("api/[controller]")]
    public class CategoryController : ControllerBase {

        private readonly ILogger<CategoryController> _logger;
        private readonly CategoryService _categoryService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;

        public CategoryController(ILogger<CategoryController> logger, CategoryService categoryService, UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager) {
            _logger = logger;
            _categoryService = categoryService;
            _userManager = userManager;
            _roleManager = roleManager;
        }



        [HttpGet]
        [Route(nameof(GetById))]
        public async Task<Category> GetById([FromQuery] string categoryId) {
            var category = await _categoryService.GetAsyncById(categoryId);

            if (category == null) {
                throw new ArgumentException($"Category with id: {categoryId} not exists");

            }
            return category;
        }

        [HttpGet]
        [Route(nameof(GetByName))]
        public async Task<Category> GetByName([FromQuery] string categoryName) {
            var category = await _categoryService.GetAsyncByName(categoryName);

            if (category == null) {
                throw new ArgumentException($"Category with name {categoryName} not exists");

            }
            return category;
        }


        [HttpGet]
        [Authorize]
        [Route(nameof(List))]
        public async Task<List<Category>> List() {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var user = await _userManager.FindByIdAsync(userId);
            var adminRoleGuid = await _roleManager.FindByNameAsync("ADMIN");

            if (user.Roles.Contains(adminRoleGuid.Id)) {
                return await _categoryService.GetListToAdminAsync();
            }
            return await _categoryService.GetListAsync(userId);
        }



        [HttpGet]
        [Authorize]
        [Route(nameof(Create))]
        public async Task<bool> Create([FromQuery] string categoryName, [FromQuery] bool isCustom) {
            var categoryFromDb = await _categoryService.GetAsyncByName(categoryName);


            if (!isCustom) {
                if (categoryFromDb != null) { 
                    return false;
                }
                var newCategory = new Category() {
                    Name = categoryName,
                    IsActive = true,
                    IsCustom = false,
                };
                await _categoryService.CreateAsync(newCategory);

                return true;

            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);


            if (categoryFromDb != null) {
                //ids = categoryFromDb["userIds4Custom"].AsBsonArray.Select(t => t.AsString).ToArray();
                

                //categoryFromDb["userIds4Custom"] = ids.ToBsonDocument();
                categoryFromDb.UserIds4Custom = categoryFromDb.UserIds4Custom.Append(userId).ToArray();
                //await _categoryService.UpdateBsonAsync(categoryFromDb["CategoryId"].AsString, categoryFromDb);
                await _categoryService.UpdateAsync(categoryFromDb.CategoryId, categoryFromDb);


                return true;
            }
            var newCustomCategory = new Category() {
                Name = categoryName,
                IsActive = true,
                IsCustom = true,
                UserIds4Custom = new string[] { userId },
            };


            await _categoryService.CreateAsync(newCustomCategory);
            

            var category = await _categoryService.GetBsonAsyncByName(categoryName);

            if (category == null) {
                throw new ArgumentException("Category not exists");

            }
            return true;
        }

   


        [HttpGet]
        [Route(nameof(Update))]
        public async Task<Category> Update([FromQuery] string categoryId, [FromQuery] string? newCategoryName, bool? isCustom = null, bool? isActive = null) {
            var categoryFromDb = await _categoryService.GetAsyncById(categoryId);

            if (categoryFromDb == null) {
                throw new ArgumentException($"Category with id: {categoryId} not exists");
            }
            categoryFromDb.Name = newCategoryName ?? categoryFromDb.Name;
            categoryFromDb.IsCustom = isCustom ?? categoryFromDb.IsCustom;
            categoryFromDb.IsActive = isActive ?? categoryFromDb.IsActive;

            await _categoryService.UpdateAsync(
                categoryId,
                categoryFromDb
            );

            var updatedCategory = await _categoryService.GetAsyncById(categoryId);

            if (updatedCategory == null) {
                throw new ArgumentException("Category not exists");

            }
            return updatedCategory;
        }


        [HttpGet]
        [Route(nameof(Delete))]
        public async Task<bool> Delete([FromQuery] string categoryId) {
            var categoryFromDb = await _categoryService.GetAsyncById(categoryId);

            if (categoryFromDb == null) {
                throw new ArgumentException($"Category with id: {categoryId} not exists");
            }

            await _categoryService.InactiveAsync(
                categoryId
            );

            return true;
        }

    }
}