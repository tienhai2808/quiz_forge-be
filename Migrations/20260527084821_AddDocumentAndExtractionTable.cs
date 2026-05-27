using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuizForge.Migrations
{
    /// <inheritdoc />
    public partial class AddDocumentAndExtractionTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameIndex(
                name: "IX_refresh_tokens_user_id",
                table: "refresh_tokens",
                newName: "ix_refresh_tokens_user_id");

            migrationBuilder.CreateTable(
                name: "extractions",
                columns: table => new
                {
                    hash_sha256 = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    version = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    raw_json = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_extractions", x => x.hash_sha256);
                });

            migrationBuilder.CreateTable(
                name: "documents",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false),
                    user_id = table.Column<long>(type: "bigint", nullable: false),
                    source_type = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    file_key = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    file_hash_sha256 = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    is_public = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_documents", x => x.id);
                    table.CheckConstraint("ck_documents_source_type", "source_type IN ('docx', 'pdf_text', 'pdf_ocr', 'image')");
                    table.CheckConstraint("ck_documents_status", "status IN ('uploaded', 'processing', 'parsed', 'failed')");
                    table.ForeignKey(
                        name: "FK_documents_extractions_file_hash_sha256",
                        column: x => x.file_hash_sha256,
                        principalTable: "extractions",
                        principalColumn: "hash_sha256",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_documents_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_documents_file_hash_sha256",
                table: "documents",
                column: "file_hash_sha256");

            migrationBuilder.CreateIndex(
                name: "IX_documents_user_id",
                table: "documents",
                column: "user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "documents");

            migrationBuilder.DropTable(
                name: "extractions");

            migrationBuilder.RenameIndex(
                name: "ix_refresh_tokens_user_id",
                table: "refresh_tokens",
                newName: "IX_refresh_tokens_user_id");
        }
    }
}
