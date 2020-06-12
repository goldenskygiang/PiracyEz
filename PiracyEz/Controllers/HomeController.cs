using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using PiracyEz.Data;
using PiracyEz.Models;

namespace PiracyEz.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _db;
        private readonly FacebookAPI _apiCredential;
        private readonly AppConfig _config;
        private readonly UserManager<IdentityUser> _userManager;

        public HomeController(
            ILogger<HomeController> logger,
            ApplicationDbContext db,
            IOptions<FacebookAPI> options,
            IOptions<AppConfig> config,
            UserManager<IdentityUser> userManager)
        {
            _logger = logger;
            _db = db;
            _apiCredential = options.Value;
            _config = config.Value;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            return View(_db.Films.AsNoTracking());
        }

        public async Task<IActionResult> Watch(int? id)
        {
            if (!id.HasValue) return Index();
            Film fm = _db.Films.Find(id);

            if (fm == null) return Index();

            if (!await _TestUrlAsync(fm.RawUrl))
            {
                fm.RawUrl = await _updateUrlAsync(fm.FacebookId);
                _db.SaveChanges();
            }

            return View(fm);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [Authorize]
        public async Task<IActionResult> Create()
        {
            if (await _GetUserEmailAsync() != _config.AdminEmail)
            {
                return Unauthorized();
            }

            return View();
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Film model)
        {
            if (await _GetUserEmailAsync() != _config.AdminEmail)
            {
                return Unauthorized();
            }

            if (ModelState.IsValid)
            {
                _db.Films.Add(model);
                _db.SaveChanges();

                return Index();
            }

            return Index();
        }

        private async Task<bool> _TestUrlAsync(string url)
        {
            if (string.IsNullOrWhiteSpace(url)) return false;
            if (Uri.TryCreate(url, UriKind.Absolute, out Uri uri))
            {
                using (HttpClient client = new HttpClient())
                {
                    var result = await client.GetAsync(url);
                    return result.IsSuccessStatusCode;
                }
            }

            return false;
        }

        private async Task<string> _updateUrlAsync(string facebookId)
        {
            string url = $"https://graph.facebook.com/v7.0/{facebookId}?fields=source&access_token={_apiCredential.AccessToken}";
            
            using (HttpClient client = new HttpClient())
            {
                string json = await client.GetStringAsync(url);
                JObject obj = JObject.Parse(json);
                return obj["source"].ToString();
            }
        }

        private async Task<string> _GetUserEmailAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            return user.Email;
        }
    }
}
