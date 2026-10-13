using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RestaurantReservation.Db.Migrations
{
    /// <inheritdoc />
    public partial class AddCustomersByMinimumPartySizeProcedure : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
        CREATE PROCEDURE dbo.GetCustomersByMinimumPartySize
            @MinimumPartySize int
        AS
        BEGIN
            SET NOCOUNT ON;

            IF @MinimumPartySize < 0
            BEGIN
                THROW 50001, 'Minimum party size cannot be negative.', 1;
            END;

            SELECT c.*
            FROM dbo.Customers AS c
            WHERE EXISTS
            (
                SELECT 1
                FROM dbo.Reservations AS r
                WHERE r.customer_id = c.customer_id
                  AND r.party_size > @MinimumPartySize
            );
        END;
        """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                  "DROP PROCEDURE dbo.GetCustomersByMinimumPartySize;");
        }
    }
}
