using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class PhoneOnlyAuthPhoneIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                IF EXISTS (
                    SELECT [NormalizedPhoneNumber]
                    FROM (
                        SELECT CASE
                            WHEN LEFT([PhoneNumber], 1) <> N'+' AND LEFT([PhoneNumber], 2) = N'00'
                                THEN SUBSTRING([PhoneNumber], 3, 32)
                            ELSE REPLACE([PhoneNumber], N'+', N'')
                        END AS [NormalizedPhoneNumber]
                        FROM [AspNetUsers]
                        WHERE [PhoneNumber] IS NOT NULL AND [PhoneNumber] <> N''
                    ) AS [Phones]
                    GROUP BY [NormalizedPhoneNumber]
                    HAVING COUNT(*) > 1
                )
                BEGIN
                    ;THROW 51000, 'Phone numbers are duplicated after normalization. Resolve duplicates before applying this migration.', 1;
                END;

                IF EXISTS (
                    SELECT 1
                    FROM [AspNetUsers]
                    WHERE [PhoneNumber] IS NOT NULL AND LEN([PhoneNumber]) > 32
                )
                BEGIN
                    ;THROW 51001, 'A stored phone number exceeds the supported length. Resolve it before applying this migration.', 1;
                END;

                UPDATE [AspNetUsers]
                SET [PhoneNumber] = CASE
                    WHEN LEFT([PhoneNumber], 1) <> N'+' AND LEFT([PhoneNumber], 2) = N'00'
                        THEN SUBSTRING([PhoneNumber], 3, 32)
                    ELSE REPLACE([PhoneNumber], N'+', N'')
                END
                WHERE [PhoneNumber] IS NOT NULL AND [PhoneNumber] <> N'';
                """);

            migrationBuilder.AlterColumn<string>(
                name: "PhoneNumber",
                table: "AspNetUsers",
                type: "nvarchar(32)",
                maxLength: 32,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_PhoneNumber",
                table: "AspNetUsers",
                column: "PhoneNumber",
                unique: true,
                filter: "[PhoneNumber] IS NOT NULL AND [PhoneNumber] <> N''");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_PhoneNumber",
                table: "AspNetUsers");

            migrationBuilder.AlterColumn<string>(
                name: "PhoneNumber",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(32)",
                oldMaxLength: 32,
                oldNullable: true);
        }
    }
}
