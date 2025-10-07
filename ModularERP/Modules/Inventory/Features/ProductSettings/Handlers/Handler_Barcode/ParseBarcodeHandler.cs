using MediatR;
using Microsoft.EntityFrameworkCore;
using ModularERP.Common.Enum.Finance_Enum;
using ModularERP.Common.Exceptions;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Inventory.Features.ProductSettings.Commends.Commends_Barcode;
using ModularERP.Modules.Inventory.Features.ProductSettings.DTO.DTO_Barcode;
using ModularERP.Modules.Inventory.Features.ProductSettings.Models;
using ModularERP.Shared.Interfaces;

namespace ModularERP.Modules.Inventory.Features.ProductSettings.Handlers.Handler_Barcode
{
    public class ParseBarcodeHandler : IRequestHandler<ParseBarcodeCommand, ResponseViewModel<ParsedBarcodeDto>>
    {
        private readonly IGeneralRepository<BarcodeSettings> _repository;

        public ParseBarcodeHandler(IGeneralRepository<BarcodeSettings> repository)
        {
            _repository = repository;
        }

        public async Task<ResponseViewModel<ParsedBarcodeDto>> Handle(ParseBarcodeCommand request, CancellationToken cancellationToken)
        {
            var settings = await _repository.GetAll()
                .Where(x => x.IsDefault)
                .FirstOrDefaultAsync(cancellationToken);

            if (settings == null)
            {
                throw new NotFoundException(
                    "No default barcode settings found. Please configure barcode settings first.",
                    FinanceErrorCode.NotFound);
            }

            var parsedData = new ParsedBarcodeDto
            {
                OriginalBarcode = request.Barcode,
                BarcodeType = settings.BarcodeType,
                IsValid = true
            };

            if (settings.EnableWeightEmbedded && !string.IsNullOrEmpty(settings.EmbeddedBarcodeFormat))
            {
                ParseEmbeddedBarcode(request.Barcode, settings, parsedData);
            }
            else
            {
                parsedData.ProductCode = request.Barcode;
            }

            // Validate check digit for EAN13/UPC
            if (settings.BarcodeType.Equals("EAN13", StringComparison.OrdinalIgnoreCase) ||
                settings.BarcodeType.Equals("UPC", StringComparison.OrdinalIgnoreCase))
            {
                parsedData.IsValid = ValidateCheckDigit(request.Barcode);
                parsedData.CheckDigit = request.Barcode[^1].ToString();
            }

            return ResponseViewModel<ParsedBarcodeDto>.Success(parsedData, "Barcode parsed successfully");
        }

        private void ParseEmbeddedBarcode(string barcode, BarcodeSettings settings, ParsedBarcodeDto parsedData)
        {
            try
            {
                var format = settings.EmbeddedBarcodeFormat;

                // Format example: "PPPPPPWWWWWC" where P=Product, W=Weight, C=Check digit
                int productLength = format.Count(c => c == 'P');
                int weightLength = format.Count(c => c == 'W');
                int priceLength = format.Count(c => c == 'R'); // R for pRice

                int currentIndex = 0;

                // Extract product code
                if (productLength > 0 && barcode.Length >= currentIndex + productLength)
                {
                    parsedData.ProductCode = barcode.Substring(currentIndex, productLength);
                    currentIndex += productLength;
                }

                // Extract weight
                if (weightLength > 0 && barcode.Length >= currentIndex + weightLength)
                {
                    var weightStr = barcode.Substring(currentIndex, weightLength);
                    if (decimal.TryParse(weightStr, out var weight) && settings.WeightUnitDivider.HasValue)
                    {
                        parsedData.Weight = weight / settings.WeightUnitDivider.Value;
                    }
                    currentIndex += weightLength;
                }

                // Extract price
                if (priceLength > 0 && barcode.Length >= currentIndex + priceLength)
                {
                    var priceStr = barcode.Substring(currentIndex, priceLength);
                    if (decimal.TryParse(priceStr, out var price) && settings.CurrencyDivider.HasValue)
                    {
                        parsedData.Price = price / settings.CurrencyDivider.Value;
                    }
                    currentIndex += priceLength;
                }

                // Extract check digit
                if (barcode.Length > currentIndex)
                {
                    parsedData.CheckDigit = barcode.Substring(currentIndex);
                }
            }
            catch (Exception)
            {
                parsedData.IsValid = false;
                parsedData.ProductCode = barcode;
            }
        }

        private bool ValidateCheckDigit(string barcode)
        {
            if (string.IsNullOrEmpty(barcode) || barcode.Length < 8)
                return false;

            try
            {
                var digits = barcode.Select(c => int.Parse(c.ToString())).ToArray();
                var checkDigit = digits[^1];
                var sum = 0;

                for (int i = 0; i < digits.Length - 1; i++)
                {
                    sum += digits[i] * (i % 2 == 0 ? 1 : 3);
                }

                var calculatedCheckDigit = (10 - (sum % 10)) % 10;
                return checkDigit == calculatedCheckDigit;
            }
            catch
            {
                return false;
            }
        }
    }
}
