using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SqlAnalyzer.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddPgLockMetrics : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PgLocks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CreateAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LockType = table.Column<string>(type: "text", nullable: false),
                    DatabaseOid = table.Column<long>(type: "bigint", nullable: false),
                    RelationOid = table.Column<long>(type: "bigint", nullable: true),
                    Page = table.Column<int>(type: "integer", nullable: true),
                    Tuple = table.Column<int>(type: "integer", nullable: true),
                    VirtualXid = table.Column<string>(type: "text", nullable: true),
                    TransactionId = table.Column<long>(type: "bigint", nullable: true),
                    ClassId = table.Column<long>(type: "bigint", nullable: true),
                    ObjectId = table.Column<long>(type: "bigint", nullable: true),
                    ObjectSubId = table.Column<int>(type: "integer", nullable: true),
                    VirtualTransaction = table.Column<string>(type: "text", nullable: false),
                    Pid = table.Column<int>(type: "integer", nullable: false),
                    Granted = table.Column<bool>(type: "boolean", nullable: false),
                    Mode = table.Column<string>(type: "text", nullable: false),
                    FastPath = table.Column<bool>(type: "boolean", nullable: false),
                    Query = table.Column<string>(type: "text", nullable: true),
                    ApplicationName = table.Column<string>(type: "text", nullable: true),
                    UserName = table.Column<string>(type: "text", nullable: true),
                    QueryStart = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    State = table.Column<string>(type: "text", nullable: true),
                    WaitTimeMs = table.Column<long>(type: "bigint", nullable: true),
                    DbConnectionId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PgLocks", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PgLocks");
        }
    }
}
