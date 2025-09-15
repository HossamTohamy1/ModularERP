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
    [Route("api/[controller]")]
    public abstract class WalletPermissionsControllerBase : ControllerBase
    {
        protected readonly IMediator _mediator;
        protected abstract string WalletType { get; }

        public WalletPermissionsControllerBase(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("{id}/permissions")]
        public async Task<ActionResult<ResponseViewModel<WalletPermissionDto>>> GetPermissions(Guid id)
        {
            var query = new GetWalletPermissionsQuery { WalletId = id, WalletType = WalletType };
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        [HttpPut("{id}/permissions")]
        public async Task<ActionResult<ResponseViewModel<bool>>> UpdatePermissions(Guid id, UpdateWalletPermissionDto permissions)
        {
            var command = new UpdateWalletPermissionCommand
            {
                WalletId = id,
                WalletType = WalletType,
                Permissions = permissions
            };
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        [HttpPost("{id}/permissions/users")]
        public async Task<ActionResult<ResponseViewModel<bool>>> AddPermission(Guid id, AddWalletPermissionCommand command)
        {
            command.WalletId = id;
            command.WalletType = WalletType;
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        [HttpDelete("{id}/permissions/{userId}")]
        public async Task<ActionResult<ResponseViewModel<bool>>> DeletePermission(Guid id, Guid userId)
        {
            var command = new DeleteWalletPermissionCommand
            {
                WalletId = id,
                UserId = userId,
                WalletType = WalletType
            };
            var result = await _mediator.Send(command);
            return Ok(result);
        }
    }

}
