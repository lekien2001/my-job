using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using RealEstate.Core.Interfaces;

namespace RealEstate.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LocationsController : ControllerBase
    {
        private readonly ILocationRepository _locationRepository;

        public LocationsController(ILocationRepository locationRepository)
        {
            _locationRepository = locationRepository;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var locations = await _locationRepository.GetAllAsync();
            return Ok(locations);
        }
    }
}
