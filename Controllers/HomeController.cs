using System;
using System.Net;
using System.Net.Http;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using MovieApp.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json;

namespace MovieApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly MyContext dbContext;

        public HomeController(MyContext context)
        {
            dbContext = context; 
        }
        public User GetCurrentUser()
        {
            int? userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
            {
                return null;
            }
            return dbContext
                .Users
                .First(u => u.UserId == userId);
        }

        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            // Movie[] movieList = new Movie();
            Movie movieList = new Movie();
            using (var httpClient = new HttpClient())
            {
                using (var response = await httpClient.GetAsync("https://api.themoviedb.org/3/discover/movie?api_key=[API_KEY]&language=en-US&sort_by=popularity.desc&include_adult=false&sort_by=vote_count.desc"))
                {
                    string apiResponse = await response.Content.ReadAsStringAsync();
                    movieList = JsonConvert.DeserializeObject<Movie>(apiResponse);
                }
            }

            ViewBag.currentUser = GetCurrentUser();

            return View(movieList.results);

        }

        [HttpGet("upcoming")]
        public async Task<IActionResult> Upcoming()
        {
            Movie movieList = new Movie();
            using (var httpClient = new HttpClient())
            {
                using (var response = await httpClient.GetAsync("https://api.themoviedb.org/3/movie/upcoming?language=en-US&api_key=[API_KEY]&page=1"))
                {
                    string apiResponse = await response.Content.ReadAsStringAsync();
                    movieList = JsonConvert.DeserializeObject<Movie>(apiResponse);                    
                }
            }
            return View("Index", movieList.results);

        }

        [HttpGet("/watch/{movieId}")]
        public IActionResult AddToWatch(int movieId)
        {
            var likeToAdd = new Watch 
            {
                UserId = GetCurrentUser().UserId,
                MovieId = movieId
            };

            dbContext.Add(likeToAdd);
            dbContext.SaveChanges();

            return RedirectToAction("Index");

        }
        [HttpGet("/movie/{movieId}")]
        public async Task<IActionResult> DetailPage(int movieId)
        {
            // Movie[] movieList = new Movie();
            Movie movieList = new Movie();
            using (var httpClient = new HttpClient())
            {
                using (var response = await httpClient.GetAsync("https://api.themoviedb.org/3/movie/{movieid}?api_key=[API_KEY]&language=en-US"))
                {
                    string apiResponse = await response.Content.ReadAsStringAsync();
                    movieList = JsonConvert.DeserializeObject<Movie>(apiResponse);                    
                }
            }

            return View(movieList);

        }



    
        [HttpGet("register")]
        public IActionResult RegisterPage()
        {
            return View();
        }
        [HttpPost("register-process")]
        public IActionResult Register(User userToRegister)
        {
            if (dbContext.Users.Any(u => u.Email == userToRegister.Email))
            {
                ModelState.AddModelError("Email", "Please use a different email.");
            }

            if (ModelState.IsValid)
            {
                PasswordHasher<User> Hasher = new PasswordHasher<User>();
                userToRegister.Password = Hasher.HashPassword(userToRegister, userToRegister.Password);
                dbContext.Add(userToRegister);
                dbContext.SaveChanges();
                HttpContext.Session.SetInt32("UserId", userToRegister.UserId);

                return RedirectToAction("Index");
            }

            return View("RegisterPage");
        }
        [HttpGet("login")]
        public IActionResult LoginPage()
        {
            return View();
        }

        [HttpPost("login-process")]
        public IActionResult Login(LoginUser userToLogin)
        {
            if (ModelState.IsValid)
            {
                // look in the DB              if we don't find the user at all, the default is null
                var foundUser = dbContext.Users.FirstOrDefault(u => u.Email == userToLogin.LoginEmail);

                if (foundUser == null)
                {
                    ModelState.AddModelError("LoginEmail", "Please check your email and password");
                    return View("Index");
                }
                var hasher = new PasswordHasher<LoginUser>();
                var result = hasher.VerifyHashedPassword(userToLogin, foundUser.Password, userToLogin.LoginPassword);

                if (result == 0)
                {
                    ModelState.AddModelError("LoginPassword", "Please check your email and password");
                    return View("Index");
                }

                // set ID in session
                HttpContext.Session.SetInt32("UserId", foundUser.UserId);
                return RedirectToAction("Index");
            }
            return View("LoginPage");
        }

        [HttpGet("logout")]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index");
        }
        
        // [HttpGet("")]
        // public JsonResult MovieList(string movieTitle)
        // {
        //     var response = new{title=movieTitle};
        //     return Json(Response);
        // }

        

    }
}
