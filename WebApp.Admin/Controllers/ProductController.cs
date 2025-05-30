using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text;
using WebApp.Admin.Models;
using WebApp.Shared.Responses;

namespace WebApp.MVC.Controllers
{
    public class ProductController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;


        public ProductController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var client = _httpClientFactory.CreateClient("WebAppApi");
            // var token = HttpContext.Session.GetString("JwtToken");
            var token = Request.Cookies["JwtToken"];

            var validToken = TokenValidator.IsTokenValid(token, out var msg);

            if (validToken)
            {
                // Add token to the header if valid
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var response = await client.GetAsync("api/product/get-all");
                if (response.IsSuccessStatusCode)
                {
                    var jsonResult = await response.Content.ReadAsStringAsync();
                    var data = JObject.Parse(jsonResult)["data"]?.ToString();

                    var products = JsonConvert.DeserializeObject<List<ProductViewModel>>(data ?? string.Empty);

                    return View(products);
                }
            }
            TempData["ErrorMessage"] = msg;
            return RedirectToAction("Login", "User");
        }

        [HttpGet]
        public async Task<IActionResult> Details(string id)
        {
            var client = _httpClientFactory.CreateClient("WebAppApi");
            var token = Request.Cookies["JwtToken"];

            var validToken = TokenValidator.IsTokenValid(token, out var msg);

            if (validToken)
            {
                // Add token to the header if valid
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                var response = await client.GetAsync($"api/product/get/{id}");

                if (response.IsSuccessStatusCode)
                {
                    var jsonResult = await response.Content.ReadAsStringAsync();
                    var data = JObject.Parse(jsonResult)["data"]?.ToString();
                    var product = JsonConvert.DeserializeObject<ProductViewModel>(data ?? string.Empty);
                    return View(product);
                }
            }

            TempData["ErrorMessage"] = msg;
            return RedirectToAction("Login", "User");
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var client = _httpClientFactory.CreateClient("WebAppApi");
            var token = Request.Cookies["JwtToken"];
            var validToken = TokenValidator.IsTokenValid(token, out var msg);
            if (validToken)
            {
                // Add token to the header if valid
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                var selectListCategory = await GetSelectListOfCategory();
                if (selectListCategory != null)
                {
                    ViewBag.Categories = selectListCategory;
                }
                else
                {
                    ViewBag.Categories = new SelectList(new List<SelectListItem>(), "Value", "Text");
                }
            }
            else
            {
                TempData["ErrorMessage"] = msg;
                return RedirectToAction("Login", "User");
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(ProductViewModel product)
        {
            var client = _httpClientFactory.CreateClient("WebAppApi");
            var token = Request.Cookies["JwtToken"];
            var validToken = TokenValidator.IsTokenValid(token, out var msg);
            if (validToken)
            {
                // Add token to the header if valid
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                product.Id = Guid.NewGuid().ToString();
                ModelState.Remove("Id");
                if (ModelState.IsValid)
                {
                    var content = new StringContent(JsonConvert.SerializeObject(product), Encoding.UTF8, "application/json");
                    var response = await client.PostAsync("api/product/create", content);
                    if (response.IsSuccessStatusCode)
                    {
                        return RedirectToAction(nameof(Index));
                    }
                }

                return View(product);
            }

            TempData["ErrorMessage"] = msg;
            return RedirectToAction("Login", "User");
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            var client = _httpClientFactory.CreateClient("WebAppApi");
            var token = Request.Cookies["JwtToken"];

            var validToken = TokenValidator.IsTokenValid(token, out var msg);

            if (validToken)
            {
                // Add token to the header if valid
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                var response = await client.GetAsync($"api/product/get/{id}");

                if (response.IsSuccessStatusCode)
                {
                    var jsonResult = await response.Content.ReadAsStringAsync();
                    var data = JObject.Parse(jsonResult)["data"]?.ToString();
                    var product = JsonConvert.DeserializeObject<ProductResponse>(data ?? string.Empty);

                    var selectListCategory = await GetSelectListOfCategory();
                    if (selectListCategory != null)
                    {
                        ViewBag.Categories = selectListCategory;
                    }
                    else
                    {
                        ViewBag.Categories = new SelectList(new List<SelectListItem>(), "Value", "Text");
                    }

                    return View(product);
                }
            }

            TempData["ErrorMessage"] = msg;
            return RedirectToAction("Login", "User");
        }

        [HttpPost]
        public async Task<IActionResult> Edit(ProductViewModel product)
        {
            var token = Request.Cookies["JwtToken"];
            var validToken = TokenValidator.IsTokenValid(token, out var msg);
            if (validToken)
            {
                var client = _httpClientFactory.CreateClient("WebAppApi");
                // Add token to the header if valid
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                if (ModelState.IsValid)
                {
                    var content = new StringContent(JsonConvert.SerializeObject(product), Encoding.UTF8, "application/json");
                    var response = await client.PutAsync($"api/product/edit", content);
                    if (response.IsSuccessStatusCode)
                    {
                        var jsonResult = await response.Content.ReadAsStringAsync();
                        var message = JObject.Parse(jsonResult)["message"]?.ToString();
                        TempData["SuccessMessage"] = message;
                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Api call failed.";
                        return View(product);
                    }
                }
                else
                {
                    TempData["ErrorMessage"] = "Input data is not valid.";
                    return View(product);
                }
            }

            TempData["ErrorMessage"] = msg;
            return RedirectToAction("Login", "User");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var token = Request.Cookies["JwtToken"];
            var validToken = TokenValidator.IsTokenValid(token, out var msg);
            if (validToken)
            {
                // Add token to the header if valid
                var client = _httpClientFactory.CreateClient("WebAppApi");
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                
                var response = await client.DeleteAsync($"api/product/delete/{id}");
                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Product deleted successfully.";
                    return RedirectToAction(nameof(Index));
                }
            }
            return NotFound();
        }

        private async Task<List<SelectListItem>?> GetSelectListOfCategory()
        {
            var client = _httpClientFactory.CreateClient("WebAppApi");
            var token = Request.Cookies["JwtToken"];

            var validToken = TokenValidator.IsTokenValid(token, out var msg);

            if (validToken)
            {
                // Add token to the header if valid
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var response = await client.GetAsync("api/categoryproduct/get-all");

                if (response.IsSuccessStatusCode)
                {
                    var jsonResult = await response.Content.ReadAsStringAsync();
                    var data = JObject.Parse(jsonResult)["data"]?.ToString();
                    var categoriesData = JsonConvert.DeserializeObject<List<CategoryProductVM>>(data ?? string.Empty);

                    return categoriesData?.Select(c => new SelectListItem
                    {
                        Value = c.Id,
                        Text = c.Name
                    }).ToList();
                }
            }

            return null;
        }
    }
}