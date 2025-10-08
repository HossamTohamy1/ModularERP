using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Inventory.Features.PriceLists.Commends.Commends_PriceRule;
using ModularERP.Modules.Inventory.Features.PriceLists.DTO.DTO_PriceRule;
using ModularERP.Modules.Inventory.Features.PriceLists.Qeuries.Qeuires_PriceRule;

namespace ModularERP.Modules.Inventory.Features.PriceLists.Controllers
{
    [Route("api/price-lists/{priceListId}/rules")]
    [ApiController]
    public class PriceListRulesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public PriceListRulesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        public async Task<ActionResult<ResponseViewModel<PriceListRuleResponseDTO>>> CreateRule(
            Guid priceListId,
            [FromBody] CreatePriceListRuleDTO dto)
        {
            var command = new CreatePriceListRuleCommand
            {
                PriceListId = priceListId,
                Data = dto
            };

            var result = await _mediator.Send(command);
            return Ok(ResponseViewModel<PriceListRuleResponseDTO>.Success(result, "Rule created successfully"));
        }

        [HttpGet]
        public async Task<ActionResult<ResponseViewModel<List<PriceListRuleResponseDTO>>>> GetAllRules(Guid priceListId)
        {
            var query = new GetPriceListRulesQuery { PriceListId = priceListId };
            var result = await _mediator.Send(query);
            return Ok(ResponseViewModel<List<PriceListRuleResponseDTO>>.Success(result, "Rules retrieved successfully"));
        }

        [HttpGet("{ruleId}")]
        public async Task<ActionResult<ResponseViewModel<PriceListRuleResponseDTO>>> GetRuleById(
            Guid priceListId,
            Guid ruleId)
        {
            var query = new GetPriceListRuleByIdQuery
            {
                PriceListId = priceListId,
                RuleId = ruleId
            };

            var result = await _mediator.Send(query);
            return Ok(ResponseViewModel<PriceListRuleResponseDTO>.Success(result, "Rule retrieved successfully"));
        }

        [HttpPut("{ruleId}")]
        public async Task<ActionResult<ResponseViewModel<PriceListRuleResponseDTO>>> UpdateRule(
            Guid priceListId,
            Guid ruleId,
            [FromBody] UpdatePriceListRuleDTO dto)
        {
            var command = new UpdatePriceListRuleCommand
            {
                PriceListId = priceListId,
                RuleId = ruleId,
                Data = dto
            };

            var result = await _mediator.Send(command);
            return Ok(ResponseViewModel<PriceListRuleResponseDTO>.Success(result, "Rule updated successfully"));
        }

        [HttpDelete("{ruleId}")]
        public async Task<ActionResult<ResponseViewModel<bool>>> DeleteRule(
            Guid priceListId,
            Guid ruleId)
        {
            var command = new DeletePriceListRuleCommand
            {
                PriceListId = priceListId,
                RuleId = ruleId
            };

            await _mediator.Send(command);
            return Ok(ResponseViewModel<bool>.Success(true, "Rule deleted successfully"));
        }

        [HttpPut("reorder")]
        public async Task<ActionResult<ResponseViewModel<bool>>> ReorderRules(
            Guid priceListId,
            [FromBody] ReorderPriceListRulesDTO dto)
        {
            var command = new ReorderPriceListRulesCommand
            {
                PriceListId = priceListId,
                Data = dto
            };

            await _mediator.Send(command);
            return Ok(ResponseViewModel<bool>.Success(true, "Rules reordered successfully"));
        }
    }
}