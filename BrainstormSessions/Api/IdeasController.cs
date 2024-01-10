using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BrainstormSessions.ClientModels;
using BrainstormSessions.Core.Interfaces;
using BrainstormSessions.Core.Model;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace BrainstormSessions.Api
{
    public class IdeasController : ControllerBase
    {
        private readonly IBrainstormSessionRepository _sessionRepository;

        public IdeasController(IBrainstormSessionRepository sessionRepository)
        {
            _sessionRepository = sessionRepository;
        }

        #region snippet_ForSessionAndCreate
        [HttpGet("forsession/{sessionId}")]
        public async Task<IActionResult> ForSession(int sessionId)
        {
            try
            {
                var session = await _sessionRepository.GetByIdAsync(sessionId);
                if (session == null)
                {
                    Log.Warning("Session not found for sessionId: {SessionId}", sessionId);
                    return NotFound(sessionId);
                }

                var result = session.Ideas.Select(idea => new IdeaDTO()
                {
                    Id = idea.Id,
                    Name = idea.Name,
                    Description = idea.Description,
                    DateCreated = idea.DateCreated
                }).ToList();

                Log.Information("Successfully found ideas for session with sessionId: {SessionId}", sessionId);

                return Ok(result);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred in the ForSession method.");
                return StatusCode(500, "Internal Server Error");
            }
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] NewIdeaModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    Log.Error("Invalid model state in Create method.");
                    return BadRequest(ModelState);
                }

                var session = await _sessionRepository.GetByIdAsync(model.SessionId);
                if (session == null)
                {
                    Log.Warning("Session not found for sessionId: {SessionId}", model.SessionId);
                    return NotFound(model.SessionId);
                }

                var idea = new Idea()
                {
                    DateCreated = DateTimeOffset.Now,
                    Description = model.Description,
                    Name = model.Name
                };
                session.AddIdea(idea);

                await _sessionRepository.UpdateAsync(session);
                Log.Information("Successfully created a new idea for session with sessionId: {SessionId}", model.SessionId);

                return Ok(session);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred in the Create method.");
                return StatusCode(500, "Internal Server Error");
            }
        }
        #endregion

        #region snippet_ForSessionActionResult
        [HttpGet("forsessionactionresult/{sessionId}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<List<IdeaDTO>>> ForSessionActionResult(int sessionId)
        {
            var session = await _sessionRepository.GetByIdAsync(sessionId);

            if (session == null)
            {
                return NotFound(sessionId);
            }

            var result = session.Ideas.Select(idea => new IdeaDTO()
            {
                Id = idea.Id,
                Name = idea.Name,
                Description = idea.Description,
                DateCreated = idea.DateCreated
            }).ToList();

            return result;
        }
        #endregion

        #region snippet_CreateActionResult
        [HttpPost("createactionresult")]
        [ProducesResponseType(201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<BrainstormSession>> CreateActionResult([FromBody] NewIdeaModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var session = await _sessionRepository.GetByIdAsync(model.SessionId);

            if (session == null)
            {
                return NotFound(model.SessionId);
            }

            var idea = new Idea()
            {
                DateCreated = DateTimeOffset.Now,
                Description = model.Description,
                Name = model.Name
            };
            session.AddIdea(idea);

            await _sessionRepository.UpdateAsync(session);

            return CreatedAtAction(nameof(CreateActionResult), new { id = session.Id }, session);
        }
        #endregion
    }
}
