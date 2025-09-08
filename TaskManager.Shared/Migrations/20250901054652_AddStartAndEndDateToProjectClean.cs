using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskManager.Shared.Migrations
{
    /// <inheritdoc />
    public partial class AddStartAndEndDateToProjectClean : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
            name: "StartDate",
            table: "Projects",
            type: "datetime2",
            nullable: false,
            defaultValue: DateTime.Now);

            migrationBuilder.AddColumn<DateTime>(
            name: "EndDate",
            table: "Projects",
            type: "datetime2",
            nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
            name: "StartDate",
            table: "Projects");

            migrationBuilder.DropColumn(
            name: "EndDate",
            table: "Projects");
        }
    }
}
