using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SupportPlatform.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class TenantAndReferenceFkDeleteBehavior : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_submitting_bodies_reference_body_types_BodyTypeCode",
                table: "submitting_bodies");

            migrationBuilder.DropForeignKey(
                name: "FK_submitting_bodies_reference_districts_DistrictCode",
                table: "submitting_bodies");

            migrationBuilder.DropForeignKey(
                name: "FK_support_requests_reference_domains_SupportDomainCode",
                table: "support_requests");

            migrationBuilder.DropForeignKey(
                name: "FK_support_requests_reference_statuses_StatusCode",
                table: "support_requests");

            migrationBuilder.AddForeignKey(
                name: "FK_submitting_bodies_reference_body_types_BodyTypeCode",
                table: "submitting_bodies",
                column: "BodyTypeCode",
                principalTable: "reference_body_types",
                principalColumn: "Code",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_submitting_bodies_reference_districts_DistrictCode",
                table: "submitting_bodies",
                column: "DistrictCode",
                principalTable: "reference_districts",
                principalColumn: "Code",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_support_requests_reference_domains_SupportDomainCode",
                table: "support_requests",
                column: "SupportDomainCode",
                principalTable: "reference_domains",
                principalColumn: "Code",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_support_requests_reference_statuses_StatusCode",
                table: "support_requests",
                column: "StatusCode",
                principalTable: "reference_statuses",
                principalColumn: "Code",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_support_requests_tenants_TenantId",
                table: "support_requests",
                column: "TenantId",
                principalTable: "tenants",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_submitting_bodies_reference_body_types_BodyTypeCode",
                table: "submitting_bodies");

            migrationBuilder.DropForeignKey(
                name: "FK_submitting_bodies_reference_districts_DistrictCode",
                table: "submitting_bodies");

            migrationBuilder.DropForeignKey(
                name: "FK_support_requests_reference_domains_SupportDomainCode",
                table: "support_requests");

            migrationBuilder.DropForeignKey(
                name: "FK_support_requests_reference_statuses_StatusCode",
                table: "support_requests");

            migrationBuilder.DropForeignKey(
                name: "FK_support_requests_tenants_TenantId",
                table: "support_requests");

            migrationBuilder.AddForeignKey(
                name: "FK_submitting_bodies_reference_body_types_BodyTypeCode",
                table: "submitting_bodies",
                column: "BodyTypeCode",
                principalTable: "reference_body_types",
                principalColumn: "Code",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_submitting_bodies_reference_districts_DistrictCode",
                table: "submitting_bodies",
                column: "DistrictCode",
                principalTable: "reference_districts",
                principalColumn: "Code",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_support_requests_reference_domains_SupportDomainCode",
                table: "support_requests",
                column: "SupportDomainCode",
                principalTable: "reference_domains",
                principalColumn: "Code",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_support_requests_reference_statuses_StatusCode",
                table: "support_requests",
                column: "StatusCode",
                principalTable: "reference_statuses",
                principalColumn: "Code",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
