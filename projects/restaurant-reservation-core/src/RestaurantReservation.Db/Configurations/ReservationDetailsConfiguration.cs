using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RestaurantReservation.Db.Results;

namespace RestaurantReservation.Db.Configurations;

public class ReservationDetailsConfiguration
    : IEntityTypeConfiguration<ReservationDetails>
{
    public void Configure(EntityTypeBuilder<ReservationDetails> builder)
    {
        builder.HasNoKey();
        builder.ToView("vw_ReservationDetails", "dbo");
    }
}
