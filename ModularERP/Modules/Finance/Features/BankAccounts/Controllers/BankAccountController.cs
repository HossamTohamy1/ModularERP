using MediatR;
using Microsoft.AspNetCore.Mvc;
using ModularERP.Modules.Finance.Features.Treasuries.Controllers;


namespace ModularERP.Modules.Finance.Features.BankAccounts.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BankAccountController : WalletPermissionsControllerBase
    {
        protected override string WalletType => "BankAccount";
        public BankAccountController(IMediator mediator) : base(mediator) { }

    }
}

