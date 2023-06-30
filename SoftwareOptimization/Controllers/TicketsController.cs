using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SoftwareOptimization;
using SoftwareOptimization.Models.Entities;

namespace SoftwareOptimization.Controllers
{
    public class TicketsController : Controller
    {
        private readonly DatabaseContext dbContext;

        public TicketsController(DatabaseContext dbContext)
        {
            this.dbContext = dbContext;
        }

        // GET: Tickets
        public async Task<IActionResult> Index()
        {
            var databaseContext = dbContext.Tickets.Include(t => t.User);
            int userId = int.Parse(User.Identity.Name.Split('|')[0]);
            return View(await databaseContext.Where(x => x.UserId == userId).ToListAsync());
        }

        // GET: Tickets/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ticket = await dbContext.Tickets.AsNoTracking()
                .Include(t => t.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (ticket == null)
            {
                return NotFound();
            }

            if (ticket.UserId != int.Parse(User.Identity.Name.Split('|')[0]))
            {
                return NotFound();
            }

            return View(ticket);
        }

        // GET: Tickets/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Tickets/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Title,Body")] Ticket ticket)
        {
            if (ModelState.IsValid)
            {
                ticket.UserId = int.Parse(User.Identity.Name.Split('|')[0]);
                ticket.CreatedAt = DateTime.Now;
                dbContext.Add(ticket);
                await dbContext.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(ticket);
        }

        // GET: Tickets/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ticket = await dbContext.Tickets.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
            if (ticket == null)
            {
                return NotFound();
            }
            if (ticket.UserId != int.Parse(User.Identity.Name.Split('|')[0]))
            {
                return NotFound();
            }

            return View(ticket);
        }

        // POST: Tickets/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Body")] Ticket ticket)
        {
            if (id != ticket.Id)
            {
                return NotFound();
            }

            var oldTicket = await dbContext.Tickets.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
            if (oldTicket == null)
            {
                return NotFound();
            }
            if (oldTicket.UserId != int.Parse(User.Identity.Name.Split('|')[0]))
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    ticket.UserId = oldTicket.UserId;
                    ticket.CreatedAt = oldTicket.CreatedAt;
                    dbContext.Update(ticket);
                    await dbContext.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TicketExists(ticket.Id))
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
            ViewData["UserId"] = new SelectList(dbContext.Users, "Id", "Id", ticket.UserId);
            return View(ticket);
        }

        // GET: Tickets/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ticket = await dbContext.Tickets.AsNoTracking()
                .Include(t => t.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (ticket == null)
            {
                return NotFound();
            }

            if (ticket.UserId != int.Parse(User.Identity.Name.Split('|')[0]))
            {
                return NotFound();
            }

            return View(ticket);
        }

        // POST: Tickets/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var ticket = await dbContext.Tickets.FindAsync(id);
            if (ticket.UserId != int.Parse(User.Identity.Name.Split('|')[0]))
            {
                return NotFound();
            }
            dbContext.Tickets.Remove(ticket);
            await dbContext.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TicketExists(int id)
        {
            return dbContext.Tickets.Any(e => e.Id == id);
        }
    }
}
