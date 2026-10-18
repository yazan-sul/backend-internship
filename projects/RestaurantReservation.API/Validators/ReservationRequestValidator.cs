using FluentValidation;
using RestaurantReservation.API.Contracts;

namespace RestaurantReservation.API.Validators;

public sealed class ReservationRequestValidator : AbstractValidator<ReservationRequest>
{
    public ReservationRequestValidator()
    {
        RuleFor(request => request.CustomerId)
            .GreaterThan(0).WithMessage("Customer ID must be greater than zero.")
            .OverridePropertyName("customerId");
        RuleFor(request => request.RestaurantId)
            .GreaterThan(0).WithMessage("Restaurant ID must be greater than zero.")
            .OverridePropertyName("restaurantId");
        RuleFor(request => request.TableId)
            .GreaterThan(0).WithMessage("Table ID must be greater than zero.")
            .OverridePropertyName("tableId");
        RuleFor(request => request.PartySize)
            .GreaterThan(0).WithMessage("Party size must be at least 1.")
            .OverridePropertyName("partySize");
        RuleFor(request => request.ReservationDate)
            .Must(date => date != default && date.Kind == DateTimeKind.Utc)
            .WithMessage("Provide a UTC date and time ending in Z.")
            .OverridePropertyName("reservationDate");
    }
}
