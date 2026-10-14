using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RestaurantReservation.Db.Migrations
{
    /// <inheritdoc />
    public partial class AddEmployeeRestaurantDetailsView : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.Sql("""
    CREATE VIEW dbo.vw_EmployeeRestaurantDetails AS
    SELECT
        e.employee_id AS EmployeeId,
        e.restaurant_id AS RestaurantId,
        e.first_name AS FirstName,
        e.last_name AS LastName,
        e.position AS Position,
        r.name AS RestaurantName,
        r.address AS RestaurantAddress
    FROM dbo.Employees AS e
    INNER JOIN dbo.Restaurants AS r
        ON e.restaurant_id = r.restaurant_id;
    """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP VIEW dbo.vw_EmployeeRestaurantDetails;");

        }
    }
}
