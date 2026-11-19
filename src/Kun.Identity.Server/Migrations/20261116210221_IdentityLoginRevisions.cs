using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Qian.Identity.Server.Migrations
{
    /// <inheritdoc />
    internal partial class IdentityLoginRevisions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 登录票据与连接器流程记下认证所依据的 Person 身份修订。加列之前签发的票据与已认证的绑定流程没有这个值，兑换与确认绑定一律要求重新登录。
            migrationBuilder.AddColumn<long>(
                name: "person_revision",
                schema: "plugin_qian_identity",
                table: "login_tickets",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "person_revision",
                schema: "plugin_qian_identity",
                table: "connector_flows",
                type: "bigint",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "person_revision",
                schema: "plugin_qian_identity",
                table: "login_tickets");

            migrationBuilder.DropColumn(
                name: "person_revision",
                schema: "plugin_qian_identity",
                table: "connector_flows");
        }
    }
}
