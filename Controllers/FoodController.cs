using ASM_1.Data;
using ASM_1.Models.Food;
using ASM_1.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ASM_1.Controllers
{
    [Route("{tableCode:regex(^((?!admin$).)*$)}")]
    public class FoodController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly TableCodeService _tableCodeService;
        private readonly UserSessionService _userSessionService;

        public FoodController(ApplicationDbContext context, TableCodeService tableCodeService, UserSessionService userSessionService)
        {
            _context = context;
            _tableCodeService = tableCodeService;
            _userSessionService = userSessionService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string tableCode)
        {
            var tableId = _tableCodeService.DecryptTableCode(tableCode);
            if (tableId == null)
            {
                return RedirectToAction("InvalidTable");
            }

            var table = await _context.Tables.FirstOrDefaultAsync(b => b.TableId == tableId);
            if (table == null)
            {
                return RedirectToAction("InvalidTable");
            }

            _userSessionService.GetOrCreateUserSessionId(tableCode);

            var model = new MenuOverviewViewModel
            {
                Categories = await _context.Categories.ToListAsync(),
                Combos = await _context.Combos.Include(c => c.ComboDetails!).ThenInclude(cd => cd.FoodItem).ToListAsync(),
                FoodItems = await _context.FoodItems.Include(f => f.Category).ToListAsync()
            };

            HttpContext.Session.SetString("CurrentTableCode", tableCode);

            return View(model);
        }

        //public async Task<IActionResult> Details(int id)
        //{
        //    var foodItem = await _context.FoodItems
        //        .Include(f => f.Category)
        //        .Include(f => f.FoodOptions)
        //        .FirstOrDefaultAsync(f => f.FoodItemId == id);
        //    if (foodItem == null)
        //    {
        //        return NotFound();
        //    }
        //    return View(foodItem);
        //}

        // /{tableCode}/food/{slug}
        [HttpGet("/food/{slug}")]
        public IActionResult Detail(string tableCode, string slug)
        {
            var tableId = _tableCodeService.DecryptTableCode(tableCode);
            if (tableId == null) return RedirectToAction("InvalidTable");

            // 1) Lấy món
            var item = _context.FoodItems
                               .AsNoTracking()
                               .FirstOrDefault(f => f.Slug == slug);
            if (item == null) return NotFound();

            // 2) Tính giá gốc hiệu lực
            decimal basePrice = item.DiscountPrice > 0
                ? item.DiscountPrice
                : (item.DiscountPercent > 0
                    ? item.BasePrice * (100 - item.DiscountPercent) / 100
                    : item.BasePrice);

            // 3) Lấy các nhóm tuỳ chọn đã gắn vào món + values
            var migs = _context.MenuItemOptionGroups
                               .AsNoTracking()
                               .Where(m => m.FoodItemId == item.FoodItemId)
                               .Include(m => m.OptionGroup)
                                   .ThenInclude(g => g.Values)
                               .OrderBy(m => m.DisplayOrder)
                               .AsSplitQuery()
                               .ToList();

            // 4) Lấy override giá trị theo món
            var valueOverrides = _context.MenuItemOptionValues
                                         .AsNoTracking()
                                         .Where(v => v.FoodItemId == item.FoodItemId)
                                         .ToList();

            // 5) Build ViewModel.Groups (đã merge override & loại ẩn)
            var groups = migs.Select(m =>
            {
                var g = m.OptionGroup;

                bool required = m.Required ?? g.Required;
                int min = m.MinSelect ?? g.MinSelect;
                int max = m.MaxSelect ?? g.MaxSelect;

                var values = g.Values
                              .Select(v =>
                              {
                                  var ov = valueOverrides.FirstOrDefault(o => o.OptionValueId == v.OptionValueId);
                                  return new ProductDetailViewModel.ValueVM
                                  {
                                      ValueId = v.OptionValueId,
                                      Name = v.Name,
                                      Code = v.Code,
                                      PriceDelta = ov?.PriceDeltaOverride ?? v.PriceDelta,
                                      IsDefault = ov?.IsDefaultOverride ?? v.IsDefault,
                                      IsHidden = ov?.IsHidden ?? false,
                                      SortOrder = ov?.SortOrderOverride ?? v.SortOrder,
                                      ScaleValue = v.ScaleValue
                                  };
                              })
                              .Where(v => !v.IsHidden)
                              .OrderBy(v => v.SortOrder)
                              .ToList();

                return new ProductDetailViewModel.GroupVM
                {
                    GroupId = g.OptionGroupId,
                    Name = g.Name,
                    GroupType = g.GroupType,
                    Required = required,
                    Min = min,
                    Max = max,
                    ScaleMin = g.ScaleMin,
                    ScaleMax = g.ScaleMax,
                    ScaleStep = g.ScaleStep,
                    ScaleUnit = g.ScaleUnit,
                    Values = values
                };
            })
            .ToList();

            // 6) Trả ViewModel cho Razor View detail mới
            var vm = new ProductDetailViewModel
            {
                Item = item,
                BasePriceEffective = basePrice,
                Groups = groups
            };

            return View(vm); // nếu View name khác, đổi: return View("DetailV2", vm);
        }
    }
}
