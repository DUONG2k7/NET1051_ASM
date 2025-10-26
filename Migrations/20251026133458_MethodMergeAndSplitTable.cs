using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ASM_1.Migrations
{
    /// <inheritdoc />
    public partial class MethodMergeAndSplitTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "OldInvoiceId",
                table: "TableInvoices",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsMerged",
                table: "Invoices",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "MergeGroupId",
                table: "Invoices",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OldInvoiceId",
                table: "TableInvoices");

            migrationBuilder.DropColumn(
                name: "IsMerged",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "MergeGroupId",
                table: "Invoices");
        }
    }
}
