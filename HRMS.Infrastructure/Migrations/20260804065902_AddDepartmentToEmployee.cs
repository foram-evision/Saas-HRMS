using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDepartmentToEmployee : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("ALTER TABLE \"Employees\" DROP COLUMN IF EXISTS \"Designation\";");
            migrationBuilder.Sql("ALTER TABLE \"Employees\" DROP COLUMN IF EXISTS \"JoiningDate\";");
            migrationBuilder.Sql("ALTER TABLE \"Employees\" DROP COLUMN IF EXISTS \"Department\";");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Department",
                table: "Employees",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<string>(
                name: "Designation",
                table: "Employees",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "JoiningDate",
                table: "Employees",
                type: "timestamp with time zone",
                nullable: true);
        }
    }
}
