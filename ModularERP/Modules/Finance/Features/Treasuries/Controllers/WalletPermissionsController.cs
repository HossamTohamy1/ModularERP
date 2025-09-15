using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Finance.Features.Treasuries.Commands;
using ModularERP.Modules.Finance.Features.Treasuries.DTO;
using ModularERP.Modules.Finance.Features.Treasuries.Queries;

namespace ModularERP.Modules.Finance.Features.Treasuries.Controllers
{

    [ApiController]
    [Route("api/finance/wallets")]
    public class WalletPermissionsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public WalletPermissionsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("{id}/permissions")]
        public async Task<ActionResult<ResponseViewModel<WalletPermissionDto>>> GetWalletPermissions(
            [FromRoute] Guid id,
            [FromQuery] string type)
        {
            var query = new GetWalletPermissionsQuery { WalletId = id, WalletType = type };
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        [HttpPut("{id}/permissions")]
        public async Task<ActionResult<ResponseViewModel<bool>>> UpdateWalletPermissions(
            [FromRoute] Guid id,
            [FromQuery] string type,
            [FromBody] UpdateWalletPermissionDto permissions)
        {
            var command = new UpdateWalletPermissionCommand
            {
                WalletId = id,
                WalletType = type,
                Permissions = permissions
            };
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        [HttpPost("{id}/permissions/users")]
        public async Task<ActionResult<ResponseViewModel<bool>>> AddWalletPermission(
            [FromRoute] Guid id,
            [FromQuery] string type,
            [FromBody] AddWalletPermissionCommand command)
        {
            command.WalletId = id;
            command.WalletType = type;
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        [HttpDelete("{id}/permissions/{userId}")]
        public async Task<ActionResult<ResponseViewModel<bool>>> DeleteWalletPermission(
            [FromRoute] Guid id,
            [FromRoute] Guid userId,
            [FromQuery] string type)
        {
            var command = new DeleteWalletPermissionCommand
            {
                WalletId = id,
                UserId = userId,
                WalletType = type
            };
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        [HttpGet]
        public async Task<ActionResult<ResponseViewModel<List<WalletPermissionDto>>>> GetWallets(
            [FromQuery] string? type = null,
            [FromQuery] Guid? companyId = null)
        {
            var query = new GetAllWalletsQuery { WalletType = type, CompanyId = companyId };
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        [HttpGet("users/{userId}")]
        public async Task<ActionResult<ResponseViewModel<List<WalletPermissionDto>>>> GetUserWallets(
            [FromRoute] Guid userId,
            [FromQuery] string? type = null)
        {
            var query = new GetUserWalletsQuery { UserId = userId, WalletType = type };
            var result = await _mediator.Send(query);
            return Ok(result);
        }
    }
}

