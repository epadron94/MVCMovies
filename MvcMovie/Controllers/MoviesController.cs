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
            return View();
        }

        // POST: Movies/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Title,ReleaseDate,Genre,Price")] Movie movie)
        {
            /*if (ModelState.IsValid)
            {*/
                /*_context.Add(movie);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));*/
                try
                {
                    var response = _cosmosDbService.CreateItemAsync(movie).Result;
                    if(response.StatusCode.Equals(System.Net.HttpStatusCode.Created))
                        return RedirectToAction(nameof(Index));
                    else
                        throw new Exception("Error creating item in CosmosDB");

                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    throw ex;
                }
            //}
            //return View(movie);
        }

        // GET: Movies/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                {
                    return NotFound();
                }

                //var movie = await _context.Movie.FindAsync(id);
                var movie = _cosmosDbService.GetItemAsync<Movie>(id).Result;
                if (movie == null)
                {
                    return NotFound();
                }
                return View(movie);
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                return NotFound();
            }
            
        }

        // POST: Movies/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("id,Title,ReleaseDate,Genre,Price")] Movie movie)
        {
            if (id != movie.id)
            {
                return NotFound();
            }

            //replace ModelState.IsValidwith custom data validation method
            /*if (ModelState.IsValid)
            {*/
                try
                {
                    var response = _cosmosDbService.UpdateItemAsync(movie).Result;
                    //if(response.StatusCode == System.Net.HttpStatusCode)
                    return RedirectToAction(nameof(Index));
                    /*_context.Update(movie);
                    await _context.SaveChangesAsync();*/
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    throw ex;
                }
                /*catch (DbUpdateConcurrencyException)
                {
                    int _id = 0;
                    Int32.TryParse(movie.Id, out _id);
                    
                    if (!MovieExists(id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                //return RedirectToAction(nameof(Index));
            }
            return View(movie);*/
        }

        // GET: Movies/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var movie = await _context.Movie
                .FirstOrDefaultAsync(m => m.id == id);
            if (movie == null)
            {
                return NotFound();
            }

            return View(movie);
        }

        // POST: Movies/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var movie = await _context.Movie.FindAsync(id);
            if (movie != null)
            {
                _context.Movie.Remove(movie);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool MovieExists(string id)
        {
            return _context.Movie.Any(e => e.id == id);
        }
    }
}
