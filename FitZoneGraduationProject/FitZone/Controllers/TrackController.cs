using FitZone.Service.DTOs;
using FitZone.Service.Services.Contract;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FitZone.APIs.Controllers
{
    public class TrackController : BaseApiController
    {
        private readonly ITrackService _trackService;

        public TrackController(ITrackService trackService)
        {
            _trackService = trackService;
        }

        // GET api/tracks
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TrackDto>>> GetAll()
        {
            var tracks = await _trackService.GetAllTracksAsync();
            return Ok(tracks);
        }
    }
}
