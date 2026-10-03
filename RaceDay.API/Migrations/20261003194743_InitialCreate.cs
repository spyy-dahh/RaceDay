using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RaceDay.API.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    userID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    userName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    emailAddress = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    contactNumber = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    password = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.userID);
                });

            migrationBuilder.CreateTable(
                name: "Event_Organiser",
                columns: table => new
                {
                    organiserID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    userID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Event_Organiser", x => x.organiserID);
                    table.ForeignKey(
                        name: "FK_Organiser_User",
                        column: x => x.userID,
                        principalTable: "Users",
                        principalColumn: "userID");
                });

            migrationBuilder.CreateTable(
                name: "Participant",
                columns: table => new
                {
                    participantID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    userID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Participant", x => x.participantID);
                    table.ForeignKey(
                        name: "FK_Participant_User",
                        column: x => x.userID,
                        principalTable: "Users",
                        principalColumn: "userID");
                });

            migrationBuilder.CreateTable(
                name: "Event",
                columns: table => new
                {
                    eventID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    organiserID = table.Column<int>(type: "int", nullable: false),
                    eventName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    eventDate = table.Column<DateTime>(type: "date", nullable: false),
                    location = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    eventType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Event", x => x.eventID);
                    table.CheckConstraint("CK_Event_EventType", "[eventType] IN ('Run', 'Walk', 'Cycle')");
                    table.ForeignKey(
                        name: "FK_Event_Organiser",
                        column: x => x.organiserID,
                        principalTable: "Event_Organiser",
                        principalColumn: "organiserID");
                });

            migrationBuilder.CreateTable(
                name: "Event_Categories",
                columns: table => new
                {
                    categoryID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    eventID = table.Column<int>(type: "int", nullable: false),
                    categoryName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    distance = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    ageRange = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Event_Categories", x => x.categoryID);
                    table.ForeignKey(
                        name: "FK_Category_Event",
                        column: x => x.eventID,
                        principalTable: "Event",
                        principalColumn: "eventID");
                });

            migrationBuilder.CreateTable(
                name: "Route",
                columns: table => new
                {
                    routeID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    eventID = table.Column<int>(type: "int", nullable: false),
                    routeDescription = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    distance = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    mapURL = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Route", x => x.routeID);
                    table.ForeignKey(
                        name: "FK_Route_Event",
                        column: x => x.eventID,
                        principalTable: "Event",
                        principalColumn: "eventID");
                });

            migrationBuilder.CreateTable(
                name: "Event_Enrolment",
                columns: table => new
                {
                    enrolmentID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    eventID = table.Column<int>(type: "int", nullable: false),
                    categoryID = table.Column<int>(type: "int", nullable: false),
                    participantID = table.Column<int>(type: "int", nullable: false),
                    enrolmentDate = table.Column<DateTime>(type: "date", nullable: false, defaultValueSql: "CAST(GETDATE() AS DATE)"),
                    status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Registered")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Event_Enrolment", x => x.enrolmentID);
                    table.CheckConstraint("CK_Enrolment_Status", "[status] IN ('Registered', 'Cancelled')");
                    table.ForeignKey(
                        name: "FK_Enrolment_Category",
                        column: x => x.categoryID,
                        principalTable: "Event_Categories",
                        principalColumn: "categoryID");
                    table.ForeignKey(
                        name: "FK_Enrolment_Event",
                        column: x => x.eventID,
                        principalTable: "Event",
                        principalColumn: "eventID");
                    table.ForeignKey(
                        name: "FK_Enrolment_Participant",
                        column: x => x.participantID,
                        principalTable: "Participant",
                        principalColumn: "participantID");
                });

            migrationBuilder.CreateTable(
                name: "Results",
                columns: table => new
                {
                    resultID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    enrolmentID = table.Column<int>(type: "int", nullable: false),
                    position = table.Column<int>(type: "int", nullable: false),
                    finishTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    dateOfResults = table.Column<DateTime>(type: "date", nullable: false, defaultValueSql: "CAST(GETDATE() AS DATE)")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Results", x => x.resultID);
                    table.CheckConstraint("CK_Result_Position", "[position] > 0");
                    table.ForeignKey(
                        name: "FK_Result_Enrolment",
                        column: x => x.enrolmentID,
                        principalTable: "Event_Enrolment",
                        principalColumn: "enrolmentID");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Event_organiserID",
                table: "Event",
                column: "organiserID");

            migrationBuilder.CreateIndex(
                name: "UQ_Category_Event_Name",
                table: "Event_Categories",
                columns: new[] { "eventID", "categoryName" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Event_Enrolment_categoryID",
                table: "Event_Enrolment",
                column: "categoryID");

            migrationBuilder.CreateIndex(
                name: "IX_Event_Enrolment_eventID",
                table: "Event_Enrolment",
                column: "eventID");

            migrationBuilder.CreateIndex(
                name: "UQ_Enrolment_Participant_Event_Category",
                table: "Event_Enrolment",
                columns: new[] { "participantID", "eventID", "categoryID" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_EventOrganiser_UserID",
                table: "Event_Organiser",
                column: "userID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_Participant_UserID",
                table: "Participant",
                column: "userID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_Result_EnrolmentID",
                table: "Results",
                column: "enrolmentID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_Route_EventID",
                table: "Route",
                column: "eventID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_Users_EmailAddress",
                table: "Users",
                column: "emailAddress",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Results");

            migrationBuilder.DropTable(
                name: "Route");

            migrationBuilder.DropTable(
                name: "Event_Enrolment");

            migrationBuilder.DropTable(
                name: "Event_Categories");

            migrationBuilder.DropTable(
                name: "Participant");

            migrationBuilder.DropTable(
                name: "Event");

            migrationBuilder.DropTable(
                name: "Event_Organiser");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
