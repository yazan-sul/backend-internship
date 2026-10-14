using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RestaurantReservation.Db.Migrations
{
    /// <inheritdoc />
    public partial class AddRestaurantRevenueFunction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
        CREATE FUNCTION dbo.CalculateRestaurantRevenue
        (
            @RestaurantId int
        )
        RETURNS decimal(18,2)
        AS
        BEGIN
            DECLARE @Revenue decimal(18,2);

            SELECT @Revenue = COALESCE(SUM(o.total_amount), 0)
            FROM dbo.Orders AS o
            INNER JOIN dbo.Reservations AS r
                ON o.reservation_id = r.reservation_id
            WHERE r.restaurant_id = @RestaurantId;

            RETURN @Revenue;
        END;
        """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                   "DROP FUNCTION dbo.CalculateRestaurantRevenue;");
        }
    }
}
