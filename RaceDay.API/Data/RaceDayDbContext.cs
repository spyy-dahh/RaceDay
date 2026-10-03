using RaceDay.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace RaceDay.API.Data
{
    public class RaceDayDbContext : DbContext
    {
        public RaceDayDbContext(DbContextOptions<RaceDayDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<EventOrganiser> EventOrganisers { get; set; }
        public DbSet<Participant> Participants { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<EventCategory> EventCategories { get; set; }
        public DbSet<RaceDay.Api.Models.Route> Routes { get; set; }//to avoid aspnet route error
        public DbSet<EventEnrolment> EventEnrolments { get; set; }
        public DbSet<Result> Results { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Users
            modelBuilder.Entity<User>(user =>
            {
                user.ToTable("Users");

                user.HasKey(user => user.UserId);

                user.Property(user => user.UserId)
                    .HasColumnName("userID")
                    .ValueGeneratedOnAdd();

                user.Property(user => user.UserName)
                    .HasColumnName("userName")
                    .HasMaxLength(100)
                    .IsRequired();

                user.Property(user => user.EmailAddress)
                    .HasColumnName("emailAddress")
                    .HasMaxLength(100)
                    .IsRequired();

                user.Property(user => user.ContactNumber)
                    .HasColumnName("contactNumber")
                    .HasMaxLength(10)
                    .IsRequired();

                user.Property(user => user.Password)
                    .HasColumnName("password")
                    .HasMaxLength(255)
                    .IsRequired();

                user.HasIndex(user => user.EmailAddress)
                    .IsUnique()
                    .HasDatabaseName("UQ_Users_EmailAddress");
            });


            // Event Organiser
            modelBuilder.Entity<EventOrganiser>(eventOrganiser =>
            {
                eventOrganiser.ToTable("Event_Organiser");

                eventOrganiser.HasKey(eventOrganiser => eventOrganiser.OrganiserId);

                eventOrganiser.Property(eventOrganiser => eventOrganiser.OrganiserId)
                    .HasColumnName("organiserID")
                    .ValueGeneratedOnAdd();

                eventOrganiser.Property(eventOrganiser => eventOrganiser.UserId)
                    .HasColumnName("userID")
                    .IsRequired();

                eventOrganiser.HasIndex(eventOrganiser => eventOrganiser.UserId)
                    .IsUnique()
                    .HasDatabaseName("UQ_EventOrganiser_UserID");

                eventOrganiser.HasOne<User>()
                    .WithMany()
                    .HasForeignKey(eventOrganiser => eventOrganiser.UserId)
                    .OnDelete(DeleteBehavior.NoAction)
                    .HasConstraintName("FK_Organiser_User");
            });


            // Participant
            modelBuilder.Entity<Participant>(participant =>
            {
                participant.ToTable("Participant");

                participant.HasKey(participant => participant.ParticipantId);

                participant.Property(participant => participant.ParticipantId)
                    .HasColumnName("participantID")
                    .ValueGeneratedOnAdd();

                participant.Property(participant => participant.UserId)
                    .HasColumnName("userID")
                    .IsRequired();

                participant.HasIndex(participant => participant.UserId)
                    .IsUnique()
                    .HasDatabaseName("UQ_Participant_UserID");

                participant.HasOne<User>()
                    .WithMany()
                    .HasForeignKey(participant => participant.UserId)
                    .OnDelete(DeleteBehavior.NoAction)
                    .HasConstraintName("FK_Participant_User");
            });


            // Event
            modelBuilder.Entity<Event>(eventEntity =>
            {
                eventEntity.HasKey(eventEntity => eventEntity.EventId);

                eventEntity.Property(eventEntity => eventEntity.EventId)
                    .HasColumnName("eventID")
                    .ValueGeneratedOnAdd();

                eventEntity.Property(eventEntity => eventEntity.OrganiserId)
                    .HasColumnName("organiserID")
                    .IsRequired();

                eventEntity.Property(eventEntity => eventEntity.EventName)
                    .HasColumnName("eventName")
                    .HasMaxLength(100)
                    .IsRequired();

                eventEntity.Property(eventEntity => eventEntity.Description)
                    .HasColumnName("description")
                    .HasMaxLength(255);

                eventEntity.Property(eventEntity => eventEntity.EventDate)
                    .HasColumnName("eventDate")
                    .HasColumnType("date")
                    .IsRequired();

                eventEntity.Property(eventEntity => eventEntity.Location)
                    .HasColumnName("location")
                    .HasMaxLength(100)
                    .IsRequired();

                eventEntity.Property(eventEntity => eventEntity.EventType)
                    .HasColumnName("eventType")
                    .HasMaxLength(20)
                    .IsRequired();

                eventEntity.ToTable("Event", eventTable =>
                {
                    eventTable.HasCheckConstraint(
                        "CK_Event_EventType",
                        "[eventType] IN ('Run', 'Walk', 'Cycle')");
                });

                eventEntity.HasOne<EventOrganiser>()
                    .WithMany()
                    .HasForeignKey(eventEntity => eventEntity.OrganiserId)
                    .OnDelete(DeleteBehavior.NoAction)
                    .HasConstraintName("FK_Event_Organiser");
            });


            // Event Categories
            modelBuilder.Entity<EventCategory>(eventCategory =>
            {
                eventCategory.ToTable("Event_Categories");

                eventCategory.HasKey(eventCategory => eventCategory.CategoryId);

                eventCategory.Property(eventCategory => eventCategory.CategoryId)
                    .HasColumnName("categoryID")
                    .ValueGeneratedOnAdd();

                eventCategory.Property(eventCategory => eventCategory.EventId)
                    .HasColumnName("eventID")
                    .IsRequired();

                eventCategory.Property(eventCategory => eventCategory.CategoryName)
                    .HasColumnName("categoryName")
                    .HasMaxLength(50)
                    .IsRequired();

                eventCategory.Property(eventCategory => eventCategory.Distance)
                    .HasColumnName("distance")
                    .HasMaxLength(20);

                eventCategory.Property(eventCategory => eventCategory.AgeRange)
                    .HasColumnName("ageRange")
                    .HasMaxLength(30);

                eventCategory.HasIndex(eventCategory => new
                {
                    eventCategory.EventId,
                    eventCategory.CategoryName
                })
                .IsUnique()
                .HasDatabaseName("UQ_Category_Event_Name");

                eventCategory.HasOne<Event>()
                    .WithMany()
                    .HasForeignKey(eventCategory => eventCategory.EventId)
                    .OnDelete(DeleteBehavior.NoAction)
                    .HasConstraintName("FK_Category_Event");
            });


            // Route
            modelBuilder.Entity<RaceDay.Api.Models.Route>(route =>
            {
                route.ToTable("Route");

                route.HasKey(route => route.RouteId);

                route.Property(route => route.RouteId)
                    .HasColumnName("routeID")
                    .ValueGeneratedOnAdd();

                route.Property(route => route.EventId)
                    .HasColumnName("eventID")
                    .IsRequired();

                route.Property(route => route.RouteDescription)
                    .HasColumnName("routeDescription")
                    .HasMaxLength(255);

                route.Property(route => route.Distance)
                    .HasColumnName("distance")
                    .HasPrecision(5, 2)
                    .IsRequired();

                route.Property(route => route.MapURL)
                    .HasColumnName("mapURL")
                    .HasMaxLength(255);

                route.HasIndex(route => route.EventId)
                    .IsUnique()
                    .HasDatabaseName("UQ_Route_EventID");

                route.HasOne<Event>()
                    .WithMany()
                    .HasForeignKey(route => route.EventId)
                    .OnDelete(DeleteBehavior.NoAction)
                    .HasConstraintName("FK_Route_Event");
            });


            // Event Enrolment
            modelBuilder.Entity<EventEnrolment>(eventEnrolment =>
            {
                eventEnrolment.HasKey(eventEnrolment => eventEnrolment.EnrolmentId);

                eventEnrolment.Property(eventEnrolment => eventEnrolment.EnrolmentId)
                    .HasColumnName("enrolmentID")
                    .ValueGeneratedOnAdd();

                eventEnrolment.Property(eventEnrolment => eventEnrolment.EventId)
                    .HasColumnName("eventID")
                    .IsRequired();

                eventEnrolment.Property(eventEnrolment => eventEnrolment.CategoryId)
                    .HasColumnName("categoryID")
                    .IsRequired();

                eventEnrolment.Property(eventEnrolment => eventEnrolment.ParticipantId)
                    .HasColumnName("participantID")
                    .IsRequired();

                eventEnrolment.Property(eventEnrolment => eventEnrolment.EnrolmentDate)
                    .HasColumnName("enrolmentDate")
                    .HasColumnType("date")
                    .HasDefaultValueSql("CAST(GETDATE() AS DATE)")
                    .IsRequired();

                eventEnrolment.Property(eventEnrolment => eventEnrolment.Status)
                    .HasColumnName("status")
                    .HasMaxLength(20)
                    .HasDefaultValue("Registered")
                    .IsRequired();

                eventEnrolment.ToTable("Event_Enrolment", eventEnrolmentTable =>
                {
                    eventEnrolmentTable.HasCheckConstraint(
                        "CK_Enrolment_Status",
                        "[status] IN ('Registered', 'Cancelled')");
                });

                eventEnrolment.HasIndex(eventEnrolment => new
                {
                    eventEnrolment.ParticipantId,
                    eventEnrolment.EventId,
                    eventEnrolment.CategoryId
                })
                .IsUnique()
                .HasDatabaseName("UQ_Enrolment_Participant_Event_Category");

                eventEnrolment.HasOne<Event>()
                    .WithMany()
                    .HasForeignKey(eventEnrolment => eventEnrolment.EventId)
                    .OnDelete(DeleteBehavior.NoAction)
                    .HasConstraintName("FK_Enrolment_Event");

                eventEnrolment.HasOne<EventCategory>()
                    .WithMany()
                    .HasForeignKey(eventEnrolment => eventEnrolment.CategoryId)
                    .OnDelete(DeleteBehavior.NoAction)
                    .HasConstraintName("FK_Enrolment_Category");

                eventEnrolment.HasOne<Participant>()
                    .WithMany()
                    .HasForeignKey(eventEnrolment => eventEnrolment.ParticipantId)
                    .OnDelete(DeleteBehavior.NoAction)
                    .HasConstraintName("FK_Enrolment_Participant");
            });


            // Results
            modelBuilder.Entity<Result>(result =>
            {
                result.HasKey(result => result.ResultId);

                result.Property(result => result.ResultId)
                    .HasColumnName("resultID")
                    .ValueGeneratedOnAdd();

                result.Property(result => result.EnrolmentId)
                    .HasColumnName("enrolmentID")
                    .IsRequired();

                result.Property(result => result.Position)
                    .HasColumnName("position")
                    .IsRequired();

                result.Property(result => result.FinishTime)
                    .HasColumnName("finishTime")
                    .HasColumnType("time")
                    .IsRequired();

                result.Property(result => result.DateOfResults)
                    .HasColumnName("dateOfResults")
                    .HasColumnType("date")
                    .HasDefaultValueSql("CAST(GETDATE() AS DATE)")
                    .IsRequired();

                result.ToTable("Results", resultTable =>
                {
                    resultTable.HasCheckConstraint(
                        "CK_Result_Position",
                        "[position] > 0");
                });

                result.HasIndex(result => result.EnrolmentId)
                    .IsUnique()
                    .HasDatabaseName("UQ_Result_EnrolmentID");

                result.HasOne<EventEnrolment>()
                    .WithMany()
                    .HasForeignKey(result => result.EnrolmentId)
                    .OnDelete(DeleteBehavior.NoAction)
                    .HasConstraintName("FK_Result_Enrolment");
            });
        }
    }
}