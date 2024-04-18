﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TextEditor.Data;
using TextEditor.Models;

namespace TextEditor.Controllers
{
    public class DocsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DocsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Docs.Include(d => d.User);
            return View(await applicationDbContext.ToListAsync());
        }

        public IActionResult Create()
        {
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Title,Content,UserId")] Doc doc)
        {
            if (ModelState.IsValid)
            {
                _context.Add(doc);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", doc.UserId);
            return View(doc);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Docs == null)
            {
                return NotFound();
            }

            var doc = await _context.Docs.FindAsync(id);
            if (doc == null)
            {
                return NotFound();
            }
            
            if (doc.UserId != User.FindFirstValue(ClaimTypes.NameIdentifier))
            {
                return NotFound();
            }

            return View(doc);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Content,UserId")] Doc doc)
        {
            if (id != doc.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(doc);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DocExists(doc.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", doc.UserId);
            return View(doc);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Docs == null)
            {
                return NotFound();
            }

            var doc = await _context.Docs
                .Include(d => d.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (doc == null)
            {
                return NotFound();
            }

            if (doc.UserId != User.FindFirstValue(ClaimTypes.NameIdentifier))
            {
                return NotFound();
            }

            return View(doc);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Docs == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Docs'  is null.");
            }
            var doc = await _context.Docs.FindAsync(id);
            if (doc != null)
            {
                _context.Docs.Remove(doc);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool DocExists(int id)
        {
          return (_context.Docs?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
