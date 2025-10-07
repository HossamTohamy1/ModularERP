using MediatR;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Inventory.Features.ProductSettings.Commends.Commends_Barcode;
using ModularERP.Modules.Inventory.Features.ProductSettings.DTO.DTO_Barcode;
using System.Text.RegularExpressions;

namespace ModularERP.Modules.Inventory.Features.ProductSettings.Handlers.Handler_Barcode
{
    public class ValidateBarcodeHandler : IRequestHandler<ValidateBarcodeCommand, ResponseViewModel<BarcodeValidationDto>>
    {
        public async Task<ResponseViewModel<BarcodeValidationDto>> Handle(ValidateBarcodeCommand request, CancellationToken cancellationToken)
        {
            var result = new BarcodeValidationDto
            {
                IsValid = true,
                Message = "Barcode is valid"
            };

            // Validate based on barcode type
            switch (request.BarcodeType.ToUpper())
            {
                case "EAN13":
                    ValidateEAN13(request.Barcode, result);
                    break;
                case "UPC":
                    ValidateUPC(request.Barcode, result);
                    break;
                case "CODE128":
                    ValidateCode128(request.Barcode, result);
                    break;
                case "CODE39":
                    ValidateCode39(request.Barcode, result);
                    break;
                case "QRCODE":
                    ValidateQRCode(request.Barcode, result);
                    break;
                default:
                    result.Errors.Add($"Unsupported barcode type: {request.BarcodeType}");
                    result.IsValid = false;
                    break;
            }

            if (!result.IsValid)
            {
                result.Message = "Barcode validation failed";
            }

            return await Task.FromResult(
                ResponseViewModel<BarcodeValidationDto>.Success(result, result.Message));
        }

        private void ValidateEAN13(string barcode, BarcodeValidationDto result)
        {
            if (barcode.Length != 13)
            {
                result.Errors.Add("EAN13 must be exactly 13 digits");
                result.IsValid = false;
                return;
            }

            if (!Regex.IsMatch(barcode, @"^\d{13}$"))
            {
                result.Errors.Add("EAN13 must contain only digits");
                result.IsValid = false;
                return;
            }

            if (!ValidateCheckDigit(barcode))
            {
                result.Errors.Add("Invalid check digit");
                result.IsValid = false;
            }
        }

        private void ValidateUPC(string barcode, BarcodeValidationDto result)
        {
            if (barcode.Length != 12)
            {
                result.Errors.Add("UPC must be exactly 12 digits");
                result.IsValid = false;
                return;
            }

            if (!Regex.IsMatch(barcode, @"^\d{12}$"))
            {
                result.Errors.Add("UPC must contain only digits");
                result.IsValid = false;
                return;
            }

            if (!ValidateCheckDigit(barcode))
            {
                result.Errors.Add("Invalid check digit");
                result.IsValid = false;
            }
        }

        private void ValidateCode128(string barcode, BarcodeValidationDto result)
        {
            if (string.IsNullOrWhiteSpace(barcode))
            {
                result.Errors.Add("Code128 cannot be empty");
                result.IsValid = false;
                return;
            }

            if (barcode.Length < 1 || barcode.Length > 50)
            {
                result.Errors.Add("Code128 must be between 1 and 50 characters");
                result.IsValid = false;
            }

            // Code128 can contain ASCII characters 0-127
            if (barcode.Any(c => c > 127))
            {
                result.Errors.Add("Code128 contains invalid characters");
                result.IsValid = false;
            }
        }

        private void ValidateCode39(string barcode, BarcodeValidationDto result)
        {
            if (string.IsNullOrWhiteSpace(barcode))
            {
                result.Errors.Add("Code39 cannot be empty");
                result.IsValid = false;
                return;
            }

            // Code39 supports: 0-9, A-Z, space, and special characters: - . $ / + %
            if (!Regex.IsMatch(barcode, @"^[0-9A-Z\-\.\ \$\/\+\%]+$"))
            {
                result.Errors.Add("Code39 contains invalid characters");
                result.IsValid = false;
            }
        }

        private void ValidateQRCode(string barcode, BarcodeValidationDto result)
        {
            if (string.IsNullOrWhiteSpace(barcode))
            {
                result.Errors.Add("QR Code cannot be empty");
                result.IsValid = false;
                return;
            }

            // QR Code can contain up to 4296 alphanumeric characters
            if (barcode.Length > 4296)
            {
                result.Errors.Add("QR Code exceeds maximum length of 4296 characters");
                result.IsValid = false;
            }
        }

        private bool ValidateCheckDigit(string barcode)
        {
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