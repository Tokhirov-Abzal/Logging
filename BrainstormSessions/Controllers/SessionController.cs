using System;
using System.Threading.Tasks;
using BrainstormSessions.Core.Interfaces;
using BrainstormSessions.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace BrainstormSessions.Controllers
{
    public class SessionController : Controller
    {
        private readonly IBrainstormSessionRepository _sessionRepository;

        public SessionController(IBrainstormSessionRepository sessionRepository)
        {
            _sessionRepository = sessionRepository;
        }

        public async Task<IActionResult> Index(int? id)
        {
            try
            {
                if (!id.HasValue)
                {
                    Log.Error("Redirecting to Home/Index because the 'id' parameter is null.");
                    return RedirectToAction(actionName: nameof(Index),
                        controllerName: "Home");
                }

                var session = await _sessionRepository.GetByIdAsync(id.Value);
                if (session == null)
                {
                    Log.Debug("Session not found for id: {SessionId}", id.Value);
                    return Content("Session not found.");
                }

                var viewModel = new StormSessionViewModel()
                {
                    DateCreated = session.DateCreated,
                    Name = session.Name,
                    Id = session.Id
                };
                Log.Information("Successfully found session details for id: {SessionId}", id.Value);

                return View(viewModel);
            }
            catch (Exception ex)
            {
                Log.Debug(ex, "An error occurred in the Index method.");
                return RedirectToAction("Error");
            }

        }
    }
}
