using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using BrainstormSessions.Core.Interfaces;
using BrainstormSessions.Core.Model;
using BrainstormSessions.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace BrainstormSessions.Controllers
{
    public class HomeController : Controller
    {
        private readonly IBrainstormSessionRepository _sessionRepository;

        public HomeController(IBrainstormSessionRepository sessionRepository)
        {
            _sessionRepository = sessionRepository;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var sessionList = await _sessionRepository.ListAsync();

                var model = sessionList.Select(session => new StormSessionViewModel()
                {
                    Id = session.Id,
                    DateCreated = session.DateCreated,
                    Name = session.Name,
                    IdeaCount = session.Ideas.Count
                });

                Log.Information("Index method successfully executed.");

                return View(model);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred in the Index method.");
                return RedirectToAction("Error");
            }

        }

        public class NewSessionModel
        {
            [Required]
            public string SessionName { get; set; }
        }

        [HttpPost]
        public async Task<IActionResult> Index(NewSessionModel model)
        {
            if (!ModelState.IsValid)
            {
                Log.Warning("Invalid model state in Index method.");
                return BadRequest(ModelState);
            }
            else
            {
                try
                {
                    await _sessionRepository.AddAsync(new BrainstormSession()
                    {
                        DateCreated = DateTimeOffset.Now,
                        Name = model.SessionName
                    });
                    Log.Information("New brainstorm session added successfully.");
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "An error occurred while adding a new brainstorm session.");
                    return RedirectToAction("Error");
                }

            }

            return RedirectToAction(actionName: nameof(Index));
        }
    }
}
