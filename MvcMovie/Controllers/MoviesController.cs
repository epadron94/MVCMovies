using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis.Elfie.Serialization;
using Microsoft.EntityFrameworkCore;
using MvcMovie.Data;
using MvcMovie.Models;
using MvcMovie.Services;
using System.Text.Json;
using Microsoft.Azure.Cosmos;

namespace MvcMovie.Controllers
{
    public class MoviesController : Controller
    {
        //dbcontext como dependencia
        private readonly MvcMovieContext _context;
        private readonly CosmosDbService _cosmosDbService;

        public MoviesController(MvcMovieContext context, CosmosDbService cosmosDbService)
        {
            _context = context;
            _cosmosDbService = cosmosDbService;
        }   

        
        // GET: Movies
        public async Task<IActionResult> Index()
        {
            //reetorna la vista (asincrona) con la lista de peliculas
            var movies = _cosmosDbService.GetAllItemsAsync<Movie>().Result;
            return View(movies);
        }

        // GET: Movies/Details/5
        public async Task<IActionResult> Details(string id) 
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                {
                    throw new Exception("id is null or empty");
                }
                
            /*var movie = await _context.Movie
                    .FirstOrDefaultAsync(m => m.Id == id);
                if (movie == null)
                {
                    return NotFound();
                }
                List<Movie> testvalue = await _cosmosDbService.GetAllItemsAsync<Movie>();//_cosmosDbService.GetItemAsync<string>("replace_with_new_document_id");*/
                Movie movie = _cosmosDbService.GetItemAsync<Movie>(id.ToString()).Result;
                
                //var movie = testvalue.Where(t => t.Id == id).FirstOrDefault();
                if(movie is not null)
                    return View(movie);
                else
                    return NotFound(); 
                //movie.Title += JsonSerializer.Serialize(testvalue.Where(m => m.Id == id)).ToString();
                //return View(movie);    
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                return NotFound();
            }
            //recibe como parametro el id de la pelicula,
            
        }

        // GET: Movies/Create
        public IActionResult Create()
        {
            return View(new Movie());
        }

        // POST: Movies/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("id,Title,ReleaseDate,Genre,Price")] Movie movie)
        {
            try
            {
                string validations = ValidateEntity(movie);
                if(string.IsNullOrEmpty(validations))
                {
                    var response = _cosmosDbService.CreateItemAsync(movie).Result;
                    return RedirectToAction(nameof(Index));
                }
                else
                    throw new Exception(validations);
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                ViewBag.ErrorMessage = ex.Message;
                return View(movie);
            }
        }

        // GET: Movies/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                    throw new Exception("Id is required");//return NotFound();

                var movie = _cosmosDbService.GetItemAsync<Movie>(id).Result;
                if (movie is null)
                    throw new Exception("Movie not found");//return NotFound();
                return View(movie);
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                ViewBag.ErrorMessage = ex.Message;
                return View();
                //return NotFound();
            }
            
        }

        // POST: Movies/Edit/5–––~~
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("id,Title,ReleaseDate,Genre,Price")] Movie movie)
        {
            try
            {    
                /*if (id != movie.id)            
                    return NotFound();*/
                string validations = ValidateEntity(movie);
                if(string.IsNullOrEmpty(validations))
                {
                    var response = _cosmosDbService.UpdateItemAsync(movie).Result;                    
                    return RedirectToAction(nameof(Index));    
                }
                else
                    throw new Exception(validations);                                
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                ViewBag.ErrorMessage = ex.Message;
                return View();
            }
        }

        // GET: Movies/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                Movie movie = _cosmosDbService.GetItemAsync<Movie>(id).Result;
                if(movie is null)
                    throw new Exception("Movie not found");
                return View(movie);
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.ToString());
                ViewBag.ErrorMessage = ex.Message;
                return View();
            }
        }


//RETO: create a stored procedure to delete items
        // POST: Movies/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            try
            {
                Movie movie = _cosmosDbService.GetItemAsync<Movie>(id).Result;
                if(movie is null)
                    throw new Exception("Movie not found");
                string response = await _cosmosDbService.DeleteItemAsync(id);

                Console.WriteLine(response.ToString());

            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return RedirectToAction(nameof(Index));
        }

        public string ValidateEntity(Movie movie)
        {
            string errorMessage = "";
            try
            {
                if(string.IsNullOrEmpty(movie.Title))
                    errorMessage = "<p>Title is required </p>";

                if(movie.ReleaseDate is null || movie.ReleaseDate == DateTime.MinValue)                
                    errorMessage += "<p>Release Date is required </p>";
                
                if(string.IsNullOrEmpty(movie.Genre))
                    errorMessage += "<p>Genre is required </p>";
                
                if(movie.Price is null || movie.Price <= 0)
                    errorMessage += "<p>Price must be greater than 0 </p>";
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                errorMessage += ex.Message;

            }
            return errorMessage;
        }
    }
}
