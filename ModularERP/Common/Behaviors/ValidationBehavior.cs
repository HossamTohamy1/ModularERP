using FluentValidation;
using MediatR;
using Serilog;
using ILogger = Serilog.ILogger;

namespace ModularERP.Common.Behaviors
{
    public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : notnull
    {
        private readonly IEnumerable<IValidator<TRequest>> _validators;
        private readonly ILogger _logger = Log.ForContext<ValidationBehavior<TRequest, TResponse>>();

        public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
        {
            _validators = validators;
        }

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            if (_validators.Any())
            {
                var context = new ValidationContext<TRequest>(request);

                var validationResults = await Task.WhenAll(
                    _validators.Select(v => v.ValidateAsync(context, cancellationToken)));

                var failures = validationResults
                    .SelectMany(r => r.Errors)
                    .Where(f => f != null)
                    .ToList();

                if (failures.Count != 0)
                {
                    _logger.Warning("Validation failed for {RequestType}. Errors: {@ValidationErrors}",
                        typeof(TRequest).Name, failures.Select(f => new { f.PropertyName, f.ErrorMessage }));

                    var validationErrors = failures
                        .GroupBy(x => x.PropertyName)
                        .ToDictionary(
                            g => g.Key,
                            g => g.Select(x => x.ErrorMessage).ToArray());

                    throw new Common.Exceptions.ValidationException("Validation failed", validationErrors);
                }
            }

            return await next();
        }
    }
}
