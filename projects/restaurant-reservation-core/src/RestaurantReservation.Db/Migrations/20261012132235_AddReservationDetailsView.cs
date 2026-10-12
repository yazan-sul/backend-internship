using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RestaurantReservation.Db.Migrations
{
    /// <inheritdoc />
    public partial class AddReservationDetailsView : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.Sql("""
    CREATE VIEW dbo.vw_ReservationDetails AS
    SELECT
        r.reservation_id AS ReservationId,
        r.customer_id AS CustomerId,
        r.restaurant_id AS RestaurantId,
        r.table_id AS TableId,
        r.party_size AS PartySize,
        r.reservation_date AS ReservationDate,
        c.first_name AS CustomerFirstName,
        c.last_name AS CustomerLastName,
        c.email AS CustomerEmail,
        c.phone_number AS CustomerPhoneNumber,
        restaurant.name AS RestaurantName,
        restaurant.address AS RestaurantAddress
    FROM dbo.Reservations AS r
    INNER JOIN dbo.Customers AS c
        ON r.customer_id = c.customer_id
    INNER JOIN dbo.Restaurants AS restaurant
        ON r.restaurant_id = restaurant.restaurant_id;
    """);

            migrationBuilder.AlterColumn<string>(
                name: "email",
                table: "Customers",
                type: "nvarchar(254)",
                maxLength: 254,
                nullable: false,
                collation: "Latin1_General_100_CI_AS",
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200,
                oldCollation: "Latin1_General_100_CI_AS");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP VIEW dbo.vw_ReservationDetails;");

            migrationBuilder.AlterColumn<string>(
                name: "email",
                table: "Customers",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                collation: "Latin1_General_100_CI_AS",
                oldClrType: typeof(string),
                oldType: "nvarchar(254)",
                oldMaxLength: 254,
                oldCollation: "Latin1_General_100_CI_AS");
        }
    }
}
