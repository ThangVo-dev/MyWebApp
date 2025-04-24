using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WebApp.Admin.Models;
using WebApp.Data.Entities;
using WebApp.Shared.Models.Category;

namespace WebApp.Admin.Controllers
{
    public class CategoryProductController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public CategoryProductController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
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

                    var categoryProducts = JsonConvert.DeserializeObject<List<CategoryProductVM>>(data ?? string.Empty);

                    return View(categoryProducts);
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
                var response = await client.GetAsync($"api/categoryproduct/get/{id}");
                if (response.IsSuccessStatusCode)
                {
                    var jsonResult = await response.Content.ReadAsStringAsync();
                    var data = JObject.Parse(jsonResult)["data"]?.ToString();

                    var categoryProduct = JsonConvert.DeserializeObject<CategoryProductVM>(data ?? string.Empty);

                    TempData["SuccessMessage"] = msg;
                    return View(categoryProduct);
                }
            }
            TempData["ErrorMessage"] = msg;
            return RedirectToAction("Login", "User");
        }

        [HttpGet]
        public IActionResult Create()
        {
            var client = _httpClientFactory.CreateClient("WebAppApi");
            var token = Request.Cookies["JwtToken"];
            var validToken = TokenValidator.IsTokenValid(token, out var msg);
            if (validToken)
            {
                // Add token to the header if valid
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                return View();
            }
            else
            {
                TempData["ErrorMessage"] = msg;
                return RedirectToAction("Login", "User");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create(CategoryProductMdl categoryProduct)
        {
            var client = _httpClientFactory.CreateClient("WebAppApi");
            var token = Request.Cookies["JwtToken"];

            var validToken = TokenValidator.IsTokenValid(token, out var msg);

            if (validToken)
            {
                // Add token to the header if valid
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                ModelState.Remove("Id");
                if (ModelState.IsValid)
                {
                    categoryProduct.Id = Guid.NewGuid().ToString();
                    var jsonContent = JsonConvert.SerializeObject(categoryProduct);
                    var content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");

                    var response = await client.PostAsync("api/categoryproduct/create", content);
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
                        return View(categoryProduct);
                    }
                }
                else
                {
                    TempData["ErrorMessage"] = "Input data is not valid.";
                    return View(categoryProduct);
                }

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
                var response = await client.GetAsync($"api/categoryproduct/get/{id}");
                if (response.IsSuccessStatusCode)
                {
                    var jsonResult = await response.Content.ReadAsStringAsync();
                    var data = JObject.Parse(jsonResult)["data"]?.ToString();

                    var categoryProduct = JsonConvert.DeserializeObject<CategoryProductMdl>(data ?? string.Empty);

                    return View(categoryProduct);
                }
            }

            TempData["ErrorMessage"] = msg;
            return RedirectToAction("Login", "User");
        }

        [HttpPost]
        public async Task<IActionResult> Edit(CategoryProductMdl categoryProduct)
        {
            var client = _httpClientFactory.CreateClient("WebAppApi");
            var token = Request.Cookies["JwtToken"];

            var validToken = TokenValidator.IsTokenValid(token, out var msg);

            if (validToken)
            {
                // Add token to the header if valid
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                var jsonContent = JsonConvert.SerializeObject(categoryProduct);
                var content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");

                var response = await client.PutAsync("api/categoryproduct/update", content);
                if (response.IsSuccessStatusCode)
                {
                    var jsonResult = await response.Content.ReadAsStringAsync();
                    var message = JObject.Parse(jsonResult)["message"]?.ToString();
                    TempData["SuccessMessage"] = message;
                    return RedirectToAction("Index");
                }
            }

            TempData["ErrorMessage"] = msg;
            return RedirectToAction("Login", "User");
        }

        [HttpPost]
        public async Task<IActionResult> Delete(string id)
        {
            var client = _httpClientFactory.CreateClient("WebAppApi");
            var token = Request.Cookies["JwtToken"];

            var validToken = TokenValidator.IsTokenValid(token, out var msg);

            if (validToken)
            {
                // Add token to the header if valid
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                var response = await client.DeleteAsync($"api/categoryproduct/delete/{id}");
                if (response.IsSuccessStatusCode)
                {
                    var jsonResult = await response.Content.ReadAsStringAsync();
                    var message = JObject.Parse(jsonResult)["message"]?.ToString();
                    TempData["SuccessMessage"] = message;
                    return RedirectToAction("Index");
                }
            }
            TempData["ErrorMessage"] = msg;
            return RedirectToAction("Login", "User");
        }
    }
}