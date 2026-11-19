using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kun.Sample.Order.Infrastructure.Migrations;

/// <inheritdoc />
public partial class DropSagaTables : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "saga_instances",
            schema: "order");

        migrationBuilder.DropTable(
            name: "saga_step_queue",
            schema: "order");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "saga_instances",
            schema: "order",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                CompletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                ContextPayload = table.Column<string>(type: "text", nullable: false),
                CurrentStepIndex = table.Column<int>(type: "integer", nullable: false),
                FailureReason = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                SagaName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                StartedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                State = table.Column<int>(type: "integer", nullable: false),
                TenantId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                TimeoutAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                TraceId = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_saga_instances", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "saga_step_queue",
            schema: "order",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                AttemptCount = table.Column<int>(type: "integer", nullable: false),
                Direction = table.Column<int>(type: "integer", nullable: false),
                EnqueuedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                NextAttemptAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                ProcessedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                SagaId = table.Column<Guid>(type: "uuid", nullable: false),
                StepIndex = table.Column<int>(type: "integer", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_saga_step_queue", x => x.Id);
            });

        migrationBuilder.CreateIndex(
            name: "ix_saga_instances_saga_name",
            schema: "order",
            table: "saga_instances",
            column: "SagaName");

        migrationBuilder.CreateIndex(
            name: "ix_saga_instances_state_timeout_at",
            schema: "order",
            table: "saga_instances",
            columns: new[] { "State", "TimeoutAt" });

        migrationBuilder.CreateIndex(
            name: "ix_saga_step_queue_processed_at_enqueued_at",
            schema: "order",
            table: "saga_step_queue",
            columns: new[] { "ProcessedAt", "EnqueuedAt" });

        migrationBuilder.CreateIndex(
            name: "ix_saga_step_queue_saga_id",
            schema: "order",
            table: "saga_step_queue",
            column: "SagaId");
    }
}
