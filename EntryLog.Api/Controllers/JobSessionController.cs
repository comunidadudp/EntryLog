using EntryLog.Business.DTOs;
using EntryLog.Business.Interfaces;
using EntryLog.Business.Pagination;
using EntryLog.Business.QueryFilters;
using Microsoft.AspNetCore.Mvc;

namespace EntryLog.Api.Controllers
{
    [Route("api/job_session")]
    [ApiController]
    public class JobSessionController(
        IWorkSessionServices workSessionServices) : ControllerBase
    {
        [HttpPost("open")]
        public async Task<object> OpenJobSessionAsync([FromForm] CreateJobSessionDTO jobSessionDTO)
        {
            (bool success, string message) = await workSessionServices.OpenJobSessionAsync(jobSessionDTO);
            return Ok(new
            {
                success,
                message
            });
        }

        [HttpPost("close")]
        public async Task<object> CloseJobSessionAsync([FromForm] CloseJobSessionDTO jobSessionDTO)
        {
            (bool success, string message) = await workSessionServices.CloseJobSessionAsync(jobSessionDTO);
            return Ok(new
            {
                success,
                message
            });
        }

        [HttpPost("filter")]
        public async Task<PaginatedResult<GetWorkSessionDTO>> FilterAsync
            ([FromQuery] WorkSessionQueryFilter filter)
            => await workSessionServices.GetSessionListByFilterAsync(filter);
    }
}
