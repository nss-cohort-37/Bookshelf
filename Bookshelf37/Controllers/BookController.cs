using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bookshelf37.Data;
using Bookshelf37.Models;
using Bookshelf37.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Bookshelf37.Controllers
{
    [Authorize]
    public class BookController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public BookController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Books
        public async Task<ActionResult> Index()
        {
            var user = await GetUserAsync();
            var books = await _context.Book
                .Where(b => b.ApplicationuserId == user.Id)
                .Include(b => b.BookGenres)
                    .ThenInclude(bg => bg.Genre)
                .ToListAsync();

            return View(books);
        }

        // GET: Books/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: Books/Create
        public async Task<ActionResult> Create()
        {
            var viewModel = new BookFormViewModel();
            var genreOptions = await _context.Genre
                .Select(g => new SelectListItem()
                {
                    Text = g.Name,
                    Value = g.Id.ToString()
                }).ToListAsync();

            viewModel.GenreOptions = genreOptions;

            return View(viewModel);
        }

        // POST: Books/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(BookFormViewModel bookViewModel)
        {
            try
            {
                var user = await GetUserAsync();
                var book = new Book()
                {
                    Title = bookViewModel.Title,
                    Author = bookViewModel.Author,
                    ApplicationuserId = user.Id,
                };

                book.BookGenres = bookViewModel.SelectedGenreIds.Select(genreId => new BookGenre()
                {
                    Book = book,
                    GenreId = genreId
                }).ToList();

                _context.Book.Add(book);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: Books/Edit/5
        public async Task<ActionResult> Edit(int id)
        {
            var viewModel = new BookFormViewModel();
            var book = await _context.Book.Include(b => b.BookGenres).FirstOrDefaultAsync(b => b.Id == id);
            var genreOptions = await _context.Genre
                .Select(g => new SelectListItem()
                {
                    Text = g.Name,
                    Value = g.Id.ToString()
                }).ToListAsync();

            viewModel.Title = book.Title;
            viewModel.Author = book.Author;
            viewModel.GenreOptions = genreOptions;
            viewModel.SelectedGenreIds = book.BookGenres.Select(bg => bg.GenreId).ToList();

            return View(viewModel);
        }

        // POST: Books/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(int id, BookFormViewModel bookViewModel)
        {
            try
            {
                var bookDataModel = await _context.Book.Include(b => b.BookGenres).FirstOrDefaultAsync(b => b.Id == id);

                bookDataModel.Title = bookViewModel.Title;
                bookDataModel.Author = bookViewModel.Author;
                bookDataModel.BookGenres.Clear();
                bookDataModel.BookGenres = bookViewModel.SelectedGenreIds.Select(genreId => new BookGenre()
                {
                    BookId = bookDataModel.Id,
                    GenreId = genreId
                }).ToList();

                _context.Book.Update(bookDataModel);
                await _context.SaveChangesAsync();


                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: Books/Delete/5
        public async Task<ActionResult> Delete(int id)
        {
            return View();
        }

        // POST: Books/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Delete(int id, IFormCollection collection)
        {
            try
            {
                // TODO: Add delete logic here

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        private async Task<ApplicationUser> GetUserAsync() => await _userManager.GetUserAsync(HttpContext.User);
    }
}