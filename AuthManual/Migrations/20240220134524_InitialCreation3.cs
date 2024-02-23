using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AuthManual.Migrations
{
    public partial class InitialCreation3 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageData",
                table: "TBLImages");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte[]>(
                name: "ImageData",
                table: "TBLImages",
                type: "bytea",
                nullable: false,
                defaultValue: new byte[0]);
        }
    }
}
