using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Inta.Inspectio.Command;
using Inta.Inspectio.Model;
using Microsoft.EntityFrameworkCore;
using SampleWebApp.Authentication;
using Inta.Authentication.Shared.Extensions;

namespace Inta.Inspectio.Controllers
{
    [Route("api/[controller]")]
    [PermissionAuthorize]
    [ApiController]
    public class ValidationController : ControllerBase
    {
        private readonly DatabaseContext _db;

        public ValidationController(DatabaseContext db)
        {
            _db = db;
        }

        [HttpPost("Request")]
        public async Task<IActionResult> SubmitRequest(SubmitRequestCommand command)
        {
            var request = new Request()
            {
                CompanyCode = command.CompanyCode,
                CompanyCountry = command.CompanyCountry,
                CompanyName = command.CompanyName,
                CustomerId = this.HttpContext.User.Claims.ToList().ToUserInfo().UserId,
                Description = command.Description,
                Id = Guid.NewGuid(),
                RequestType = command.RequestType
            };
           await _db.Requests.AddAsync(request);
            await _db.SaveChangesAsync();

            return Ok();
        }

        [HttpGet("Request")]
        public async Task<IActionResult> ShowRequest()
        {
            var requests =await _db.Requests.Where(x => x.CustomerId == this.HttpContext.User.Claims.ToList().ToUserInfo().UserId).ToListAsync();
            return Ok(requests);
        }
    }
}
