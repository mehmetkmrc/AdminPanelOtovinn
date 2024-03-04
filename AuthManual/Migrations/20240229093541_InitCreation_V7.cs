using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AuthManual.Migrations
{
    public partial class InitCreation_V7 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte[]>(
                name: "ImageData",
                table: "TBLImages",
                type: "bytea",
                nullable: false,
                defaultValue: new byte[0]);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageData",
                table: "TBLImages");
        }
    }
}
