using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace MovieApp.Controllers
{
    [Route("api/movies")]
    [ApiController]
    public class APIController : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<string>> Get()
        {
            string url= "https://api.themoviedb.org/3/discover/movie?api_key=122f0c50ae02fa84601c07025cb6d2f1&sort_by=popularity.desc"; // sample url
            using (HttpClient client = new HttpClient())
            {
                return  await client.GetStringAsync(url);
            }
        }
    }
}