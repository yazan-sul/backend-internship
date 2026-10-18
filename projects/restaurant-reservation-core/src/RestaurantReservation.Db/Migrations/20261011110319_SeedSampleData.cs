using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RestaurantReservation.Db.Migrations
{
    /// <inheritdoc />
    public partial class SeedSampleData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Fictional businesses and contacts; fixed IDs and dates keep demos repeatable.
            migrationBuilder.InsertData(
                table: "Restaurants",
                columns: new[] { "restaurant_id", "name", "address", "phone_number", "opening_hours" },
                values: new object[,]
                {
                    { 1, "Dar Al-Zaytoun", "Ramallah, Al-Manara Square (demo address)", "+970-2-000-0001", "10:00-22:00" },
                    { 2, "Bayt Nablus", "Nablus, Old City (demo address)", "+970-9-000-0002", "11:00-23:00" },
                    { 3, "Khalil Kitchen", "Hebron, Ein Sarah Street (demo address)", "+970-2-000-0003", "10:00-21:00" },
                    { 4, "Yafa Bakery", "Bethlehem, Manger Square (demo address)", "+970-2-000-0004", "08:00-20:00" },
                    { 5, "Jenin Sweets", "Jenin, City Center (demo address)", "+970-4-000-0005", "09:00-22:00" }
                });

            migrationBuilder.InsertData(
                table: "Customers",
                columns: new[] { "customer_id", "first_name", "last_name", "email", "phone_number" },
                values: new object[,]
                {
                    { 1, "Yazan", "Sulaiman", "yazan.sulaiman@example.test", "+970-59-000-0001" },
                    { 2, "Dalia", "Nassar", "dalia.nassar@example.test", "+970-59-000-0002" },
                    { 3, "Omar", "Khalil", "omar.khalil@example.test", "+970-59-000-0003" },
                    { 4, "Lina", "Hamad", "lina.hamad@example.test", "+970-59-000-0004" },
                    { 5, "Rami", "Darwish", "rami.darwish@example.test", "+970-59-000-0005" }
                });

            migrationBuilder.InsertData(
                table: "RestaurantTables",
                columns: new[] { "table_id", "restaurant_id", "capacity" },
                values: new object[,]
                {
                    { 1, 1, 6 }, { 2, 2, 4 }, { 3, 3, 8 }, { 4, 4, 2 }, { 5, 5, 6 }
                });

            migrationBuilder.InsertData(
                table: "Employees",
                columns: new[] { "employee_id", "restaurant_id", "first_name", "last_name", "position" },
                values: new object[,]
                {
                    { 1, 1, "Ahmad", "Barghouti", "Manager" },
                    { 2, 1, "Hiba", "Mansour", "Waiter" },
                    { 3, 2, "Mahmoud", "Masri", "Manager" },
                    { 4, 3, "Salma", "Tamimi", "Chef" },
                    { 5, 4, "Tariq", "Awad", "Waiter" },
                    { 6, 5, "Nour", "Jarrar", "Waiter" }
                });

            migrationBuilder.InsertData(
                table: "MenuItems",
                columns: new[] { "menu_item_id", "restaurant_id", "name", "description", "price" },
                values: new object[,]
                {
                    { 1, 1, "Lentil Soup", "Lentil soup with cumin and lemon.", 10.00m },
                    { 2, 1, "Musakhan", "Chicken with sumac, onions, and olive oil on taboon bread.", 15.00m },
                    { 3, 2, "Maqluba", "Rice with chicken and vegetables, served upside down.", 20.00m },
                    { 4, 3, "Fattoush", "Vegetable salad with toasted bread and sumac.", 12.50m },
                    { 5, 4, "Falafel Sandwich", "Falafel, vegetables, and tahini in fresh bread.", 8.00m },
                    { 6, 5, "Nabulsi Knafeh", null, 9.00m }
                });

            // All fixture dates represent UTC; SQL Server datetime2 does not retain DateTime.Kind.
            migrationBuilder.InsertData(
                table: "Reservations",
                columns: new[] { "reservation_id", "customer_id", "restaurant_id", "table_id", "reservation_date", "party_size" },
                values: new object[,]
                {
                    { 1, 1, 1, 1, new System.DateTime(2026, 9, 1, 18, 0, 0, System.DateTimeKind.Utc), 2 },
                    { 2, 1, 1, 1, new System.DateTime(2026, 9, 2, 18, 0, 0, System.DateTimeKind.Utc), 5 },
                    { 3, 2, 2, 2, new System.DateTime(2026, 9, 1, 18, 0, 0, System.DateTimeKind.Utc), 4 },
                    { 4, 3, 3, 3, new System.DateTime(2026, 9, 1, 18, 0, 0, System.DateTimeKind.Utc), 6 },
                    { 5, 4, 4, 4, new System.DateTime(2026, 9, 1, 18, 0, 0, System.DateTimeKind.Utc), 2 },
                    { 6, 5, 5, 5, new System.DateTime(2026, 9, 1, 18, 0, 0, System.DateTimeKind.Utc), 3 }
                });

            migrationBuilder.InsertData(
                table: "Orders",
                columns: new[] { "order_id", "reservation_id", "employee_id", "order_date", "total_amount" },
                values: new object[,]
                {
                    { 1, 1, 1, new System.DateTime(2026, 9, 1, 18, 10, 0, System.DateTimeKind.Utc), 35.00m },
                    { 2, 1, 2, new System.DateTime(2026, 9, 1, 18, 20, 0, System.DateTimeKind.Utc), 10.00m },
                    { 3, 2, 2, new System.DateTime(2026, 9, 2, 18, 10, 0, System.DateTimeKind.Utc), 45.00m },
                    { 4, 3, 3, new System.DateTime(2026, 9, 1, 18, 10, 0, System.DateTimeKind.Utc), 40.00m },
                    { 5, 4, 4, new System.DateTime(2026, 9, 1, 18, 10, 0, System.DateTimeKind.Utc), 25.00m },
                    { 6, 5, 5, new System.DateTime(2026, 9, 1, 18, 10, 0, System.DateTimeKind.Utc), 16.00m }
                });

            migrationBuilder.InsertData(
                table: "OrderItems",
                columns: new[] { "order_item_id", "order_id", "item_id", "quantity" },
                values: new object[,]
                {
                    { 1, 1, 1, 2 }, { 2, 1, 2, 1 }, { 3, 2, 1, 1 },
                    { 4, 3, 2, 3 }, { 5, 4, 3, 2 }, { 6, 5, 4, 2 }, { 7, 6, 5, 2 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Delete only this fixture, in reverse dependency order.
            migrationBuilder.DeleteData("OrderItems", "order_item_id", new object[] { 1, 2, 3, 4, 5, 6, 7 });
            migrationBuilder.DeleteData("Orders", "order_id", new object[] { 1, 2, 3, 4, 5, 6 });
            migrationBuilder.DeleteData("Reservations", "reservation_id", new object[] { 1, 2, 3, 4, 5, 6 });
            migrationBuilder.DeleteData("MenuItems", "menu_item_id", new object[] { 1, 2, 3, 4, 5, 6 });
            migrationBuilder.DeleteData("Employees", "employee_id", new object[] { 1, 2, 3, 4, 5, 6 });
            migrationBuilder.DeleteData("RestaurantTables", "table_id", new object[] { 1, 2, 3, 4, 5 });
            migrationBuilder.DeleteData("Customers", "customer_id", new object[] { 1, 2, 3, 4, 5 });
            migrationBuilder.DeleteData("Restaurants", "restaurant_id", new object[] { 1, 2, 3, 4, 5 });
        }
    }
}
