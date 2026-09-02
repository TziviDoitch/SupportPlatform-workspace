using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SupportPlatform.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "filter_field_registry",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Label = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Kind = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    ReferenceList = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    Operators = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Segmentable = table.Column<bool>(type: "bit", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_filter_field_registry", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "reference_body_types",
                columns: table => new
                {
                    Code = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Label = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_reference_body_types", x => x.Code);
                });

            migrationBuilder.CreateTable(
                name: "reference_districts",
                columns: table => new
                {
                    Code = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Label = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_reference_districts", x => x.Code);
                });

            migrationBuilder.CreateTable(
                name: "reference_domains",
                columns: table => new
                {
                    Code = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Label = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_reference_domains", x => x.Code);
                });

            migrationBuilder.CreateTable(
                name: "reference_statuses",
                columns: table => new
                {
                    Code = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Label = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_reference_statuses", x => x.Code);
                });

            migrationBuilder.CreateTable(
                name: "tenants",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tenants", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "submitting_bodies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    TenantId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    BodyTypeCode = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    DistrictCode = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_submitting_bodies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_submitting_bodies_reference_body_types_BodyTypeCode",
                        column: x => x.BodyTypeCode,
                        principalTable: "reference_body_types",
                        principalColumn: "Code",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_submitting_bodies_reference_districts_DistrictCode",
                        column: x => x.DistrictCode,
                        principalTable: "reference_districts",
                        principalColumn: "Code",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_submitting_bodies_tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Username = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    TenantId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Role = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.Id);
                    table.ForeignKey(
                        name: "FK_users_tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "support_requests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    SubmittingBodyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SupportDomainCode = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    StatusCode = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    SupportYear = table.Column<int>(type: "int", nullable: false),
                    AmountRequested = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    AmountApproved = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_support_requests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_support_requests_reference_domains_SupportDomainCode",
                        column: x => x.SupportDomainCode,
                        principalTable: "reference_domains",
                        principalColumn: "Code",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_support_requests_reference_statuses_StatusCode",
                        column: x => x.StatusCode,
                        principalTable: "reference_statuses",
                        principalColumn: "Code",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_support_requests_submitting_bodies_SubmittingBodyId",
                        column: x => x.SubmittingBodyId,
                        principalTable: "submitting_bodies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_submitting_bodies_BodyTypeCode",
                table: "submitting_bodies",
                column: "BodyTypeCode");

            migrationBuilder.CreateIndex(
                name: "IX_submitting_bodies_DistrictCode",
                table: "submitting_bodies",
                column: "DistrictCode");

            migrationBuilder.CreateIndex(
                name: "IX_submitting_bodies_TenantId",
                table: "submitting_bodies",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_support_requests_StatusCode",
                table: "support_requests",
                column: "StatusCode");

            migrationBuilder.CreateIndex(
                name: "IX_support_requests_SubmittingBodyId",
                table: "support_requests",
                column: "SubmittingBodyId");

            migrationBuilder.CreateIndex(
                name: "IX_support_requests_SupportDomainCode",
                table: "support_requests",
                column: "SupportDomainCode");

            migrationBuilder.CreateIndex(
                name: "IX_support_requests_TenantId",
                table: "support_requests",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_support_requests_TenantId_SupportYear",
                table: "support_requests",
                columns: new[] { "TenantId", "SupportYear" });

            migrationBuilder.CreateIndex(
                name: "IX_users_TenantId",
                table: "users",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_users_Username",
                table: "users",
                column: "Username",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "filter_field_registry");

            migrationBuilder.DropTable(
                name: "support_requests");

            migrationBuilder.DropTable(
                name: "users");

            migrationBuilder.DropTable(
                name: "reference_domains");

            migrationBuilder.DropTable(
                name: "reference_statuses");

            migrationBuilder.DropTable(
                name: "submitting_bodies");

            migrationBuilder.DropTable(
                name: "reference_body_types");

            migrationBuilder.DropTable(
                name: "reference_districts");

            migrationBuilder.DropTable(
                name: "tenants");
        }
    }
}
