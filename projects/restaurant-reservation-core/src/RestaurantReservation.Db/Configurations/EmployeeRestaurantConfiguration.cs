using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RestaurantReservation.Db.Results;

namespace RestaurantReservation.Db.Configurations;

public class EmployeeRestaurantDetailsConfiguration : IEntityTypeConfiguration<EmployeeRestaurantDetails>
{
    public void Configure(EntityTypeBuilder<EmployeeRestaurantDetails> builder)
    {
        builder.HasNoKey();
        builder.ToView("vw_EmployeeRestaurantDetails", "dbo");
    }
}