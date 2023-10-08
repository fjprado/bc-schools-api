using bc_schools_api.Infra.Interfaces;
using bc_schools_api.Repository.DatabaseModels;
using Microsoft.EntityFrameworkCore;

namespace bc_schools_api.Repository
{
    public class DatabaseContext : DbContext
    {
        private readonly ISettings _settings;
        public DatabaseContext(DbContextOptions<DatabaseContext> options, ISettings settings)
            : base(options)
        {
            _settings = settings;
        }

        public virtual DbSet<DbDistrict> District { get; set; }
        public virtual DbSet<DbSchoolCategory> SchoolCategory { get; set; }
        public virtual DbSet<DbSchoolType> SchoolType { get; set; }
        public virtual DbSet<DbSchool> School { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {

            if (!optionsBuilder.IsConfigured)
            {
                IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();
                var connectionString = _settings.SchoolDbConnectionString;
                optionsBuilder.UseSqlServer(connectionString);
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<DbDistrict>(entity =>
            {
                entity.ToTable("district", "dbo");

                entity.HasKey(d => d.Number)
                    .HasName("PK_district");

                entity.Property(d => d.Number)
                    .HasColumnName("number")
                    .IsRequired();

                entity.Property(d => d.Name)
                    .HasColumnName("name")
                    .IsRequired();
            });

            modelBuilder.Entity<DbSchoolCategory>(entity =>
            {
                entity.ToTable("school_category", "dbo");

                entity.HasKey(sc => sc.Id)
                    .HasName("PK_school_category");

                entity.Property(sc => sc.Id)
                    .HasColumnName("id")
                    .IsRequired();

                entity.Property(sc => sc.Description)
                    .HasColumnName("description")
                    .IsRequired();
            });

            modelBuilder.Entity<DbSchoolType>(entity =>
            {
                entity.ToTable("school_type", "dbo");

                entity.HasKey(st => st.Id)
                    .HasName("PK_school_type");

                entity.Property(st => st.Id)
                    .HasColumnName("id")
                    .IsRequired();

                entity.Property(st => st.Description)
                    .HasColumnName("description")
                    .IsRequired();
            });

            modelBuilder.Entity<DbSchool>(entity =>
            {
                entity.ToTable("school", "dbo");

                entity.HasKey(s => s.Id)
                    .HasName("PK_school");

                entity.Property(s => s.Id)
                    .HasColumnName("id")
                    .IsRequired();

                entity.Property(s => s.Code)
                    .HasColumnName("code")
                    .IsRequired();

                entity.Property(s => s.Name)
                    .HasColumnName("name")
                    .IsRequired();

                entity.Property(s => s.Address)
                    .HasColumnName("address")
                    .IsRequired();

                entity.Property(s => s.City)
                    .HasColumnName("city")
                    .IsRequired();

                entity.Property(s => s.Province)
                    .HasColumnName("province")
                    .IsRequired();

                entity.Property(s => s.PostalCode)
                    .HasColumnName("postal_code")
                    .IsRequired();

                entity.Property(s => s.Latitude)
                    .HasColumnName("latitude")
                    .IsRequired();

                entity.Property(s => s.Longitude)
                    .HasColumnName("longitude")
                    .IsRequired();

                entity.Property(s => s.GradeRange)
                    .HasColumnName("grade_range");

                entity.Property(s => s.Phone)
                    .HasColumnName("phone");

                entity.Property(s => s.Fax)
                    .HasColumnName("fax");

                entity.Property(s => s.DistrictNumber).HasColumnName("district_number");
                entity.HasOne(s => s.District)
                    .WithMany(d => d.Schools)
                    .HasForeignKey(s => s.DistrictNumber)
                    .HasConstraintName("FK_school_district");

                entity.Property(s => s.SchoolCategoryId).HasColumnName("school_category_id");
                entity.HasOne(s => s.SchoolCategory)
                    .WithMany(d => d.Schools)
                    .HasForeignKey(s => s.SchoolCategoryId)
                    .HasConstraintName("FK_school_school_category");

                entity.Property(s => s.SchoolTypeId).HasColumnName("school_type_id");
                entity.HasOne(s => s.SchoolType)
                    .WithMany(d => d.Schools)
                    .HasForeignKey(s => s.SchoolTypeId)
                    .HasConstraintName("FK_school_school_type");
            });
        }
    }
}
