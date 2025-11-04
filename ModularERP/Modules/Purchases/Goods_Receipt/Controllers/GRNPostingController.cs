using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ModularERP.Modules.Purchases.Goods_Receipt.Commends.Commends_GRN;
using System.Security.Claims;

namespace ModularERP.Modules.Purchases.Goods_Receipt.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GRNPostingController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<GRNPostingController> _logger;

        public GRNPostingController(IMediator mediator, ILogger<GRNPostingController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        /// <summary>
        /// Post GRN to inventory
        /// </summary>
        [HttpPost("post")]
        public async Task<IActionResult> PostGRN([FromRoute] Guid id)
        {
            _logger.LogInformation("Posting GRN {GRNId} to inventory", id);

            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var command = new PostGRNCommand
            {
                GRNId = id,
                UserId = userId
            };

            var result = await _mediator.Send(command);

            return Ok(new
            {
                success = true,
                data = result,
                message = "GRN posted to inventory successfully"
            });
        }

        /// <summary>
        /// Reverse posted GRN
        /// </summary>
        [HttpPost("reverse")]
        public async Task<IActionResult> ReverseGRN(
            [FromRoute] Guid id,
            [FromBody] ReverseGRNCommand command)
        {
            _logger.LogInformation("Reversing GRN {GRNId}", id);

            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            command.GRNId = id;
            command.UserId = userId;

            var result = await _mediator.Send(command);

            return Ok(new
            {
                success = true,
                data = result,
                message = "GRN reversed successfully"
            });
        }
    }
}