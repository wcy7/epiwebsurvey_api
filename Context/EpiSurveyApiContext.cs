using Epi.Web.Common.Security;
using  epiwebsurvey_api.Context.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;

namespace epiwebsurvey_api.Context
{
    public class  epiwebsurvey_apiContext : DbContext
    {
        public static readonly string _connectionString;

        public  epiwebsurvey_apiContext(DbContextOptions< epiwebsurvey_apiContext> options)
          : base(options)
        {
        }
        static  epiwebsurvey_apiContext()
        {
            try
            {
                var builder = new ConfigurationBuilder();
                builder.AddJsonFile("appsettings.json", optional: false);
                var configuration = builder.Build();
                var appsettings = configuration.GetSection("ApplicationSettings").GetChildren().ToList();
                foreach (var item in appsettings)
                {
                    if (item.Key == "KeyForConnectionStringPassphrase")
                        Cryptography.passPhrase_config = item.Value;
                    if (item.Key == "KeyForConnectionStringSalt")
                        Cryptography.saltValue_config = item.Value;
                    if (item.Key == "KeyForConnectionStringVector")
                        Cryptography.initVector_config = item.Value;
                }
                //_connectionString = Cryptography.Decrypt(configuration.GetConnectionString("epiinfosurveyapiContext").ToString());
                _connectionString = Cryptography.Decrypt((Environment.GetEnvironmentVariable("epiinfosurveyapiContext").ToString()));
                Cryptography.passPhrase_config = null; Cryptography.saltValue_config = null; Cryptography.initVector_config = null;
            }
            catch (Exception ex)
            {
                throw (ex);
            }

        }
        public  epiwebsurvey_apiContext() : base()
        {
            // _connectionString = connectionString;
           // this.Database.SetCommandTimeout(160);
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer(_connectionString);
            }
        }
        public virtual DbSet<Address> Address { get; set; }
        public virtual DbSet<Admin> Admin { get; set; }
        public virtual DbSet<Canvas> Canvas { get; set; }
        public virtual DbSet<DataAccessRules> DataAccessRules { get; set; }
        public virtual DbSet<Datasource> Datasource { get; set; }
        public virtual DbSet<DatasourceUser> DatasourceUser { get; set; }
        public virtual DbSet<Eidatasource> Eidatasource { get; set; }
        public virtual DbSet<LkRecordSource> LkRecordSource { get; set; }
        public virtual DbSet<LkStatus> LkStatus { get; set; }
        public virtual DbSet<LkSurveyType> LkSurveyType { get; set; }
        public virtual DbSet<Organization> Organization { get; set; }
        public virtual DbSet<ResponseDisplaySettings> ResponseDisplaySettings { get; set; }
        public virtual DbSet<ResponseXml> ResponseXml { get; set; }
        public virtual DbSet<Role> Role { get; set; }
        public virtual DbSet<SharedCanvases> SharedCanvases { get; set; }
        public virtual DbSet<SourceTables> SourceTables { get; set; }
        public virtual DbSet<SurveyMetaData> SurveyMetaData { get; set; }
        public virtual DbSet<SurveyMetadataOrganization> SurveyMetadataOrganization { get; set; }
        public virtual DbSet<SurveyMetaDataUser> SurveyMetaDataUser { get; set; }
        public virtual DbSet<SurveyMetaDataView> SurveyMetaDataView { get; set; }
        public virtual DbSet<SurveyResponse> SurveyResponse { get; set; }
        public virtual DbSet<SurveyResponseTracking> SurveyResponseTracking { get; set; }
        public virtual DbSet<SurveyResponseUser> SurveyResponseUser { get; set; }
        public virtual DbSet<User> User { get; set; }
        public virtual DbSet<UserOrganization> UserOrganization { get; set; }       
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Address>(entity =>
            {
                entity.Property(e => e.AddressLine1)
                    .IsRequired()
                    .HasMaxLength(60);

                entity.Property(e => e.AddressLine2).HasMaxLength(60);

                entity.Property(e => e.City)
                    .IsRequired()
                    .HasMaxLength(30);

                entity.Property(e => e.PostalCode)
                    .IsRequired()
                    .HasMaxLength(15);

                entity.HasOne(d => d.Admin)
                    .WithMany(p => p.Address)
                    .HasForeignKey(d => d.AdminId)
                    .HasConstraintName("FK_Address_Admin");
            });

            modelBuilder.Entity<Admin>(entity =>
            {
                entity.Property(e => e.AdminId).ValueGeneratedNever();

                entity.Property(e => e.AdminEmail)
                    .IsRequired()
                    .HasMaxLength(300)
                    .IsUnicode(false);

                entity.Property(e => e.FirstName)
                    .HasMaxLength(150)
                    .IsUnicode(false);

                entity.Property(e => e.IsActive).HasDefaultValueSql("((1))");

                entity.Property(e => e.LastName)
                    .HasMaxLength(150)
                    .IsUnicode(false);

                entity.Property(e => e.PhoneNumber)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.HasOne(d => d.Organization)
                    .WithMany(p => p.Admin)
                    .HasForeignKey(d => d.OrganizationId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Admin_Organization");
            });

            modelBuilder.Entity<Canvas>(entity =>
            {
                entity.Property(e => e.CanvasId).HasColumnName("CanvasID");

                entity.Property(e => e.CanvasContent).IsRequired();

                entity.Property(e => e.CanvasDescription).IsUnicode(false);

                entity.Property(e => e.CanvasGuid)
                    .HasColumnName("CanvasGUID")
                    .HasDefaultValueSql("(newid())");

                entity.Property(e => e.CanvasName)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.CreatedDate).HasColumnType("datetime");

                entity.Property(e => e.DatasourceId).HasColumnName("DatasourceID");

                entity.Property(e => e.ModifiedDate).HasColumnType("datetime");

                entity.Property(e => e.UserId).HasColumnName("UserID");

                entity.HasOne(d => d.Datasource)
                    .WithMany(p => p.Canvas)
                    .HasForeignKey(d => d.DatasourceId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Canvas _Datasource");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.Canvas)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Canvas_CreatorUser");
            });

            modelBuilder.Entity<DataAccessRules>(entity =>
            {
                entity.HasKey(e => e.RuleId);

                entity.Property(e => e.RuleId).ValueGeneratedNever();

                entity.Property(e => e.RuleDescription).IsUnicode(false);

                entity.Property(e => e.RuleName)
                    .HasMaxLength(150)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<Datasource>(entity =>
            {
                entity.HasIndex(e => e.DatasourceName)
                    .HasName("IX_DatasourceName")
                    .IsUnique();

                entity.Property(e => e.DatasourceId).HasColumnName("DatasourceID");

                entity.Property(e => e.Active).HasColumnName("active");

                entity.Property(e => e.DatabaseObject)
                    .IsRequired()
                    .IsUnicode(false);

                entity.Property(e => e.DatabaseType)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.DatabaseUserId)
                    .HasColumnName("DatabaseUserID")
                    .HasMaxLength(200)
                    .IsUnicode(false);

                entity.Property(e => e.DatasourceName).HasMaxLength(300);

                entity.Property(e => e.DatasourceServerName)
                    .HasMaxLength(300)
                    .IsUnicode(false);

                entity.Property(e => e.Eiwsdatasource).HasColumnName("EIWSDatasource");

                entity.Property(e => e.EiwssurveyId).HasColumnName("EIWSSurveyId");

                entity.Property(e => e.InitialCatalog)
                    .HasMaxLength(300)
                    .IsUnicode(false);

                entity.Property(e => e.OrganizationId).HasColumnName("OrganizationID");

                entity.Property(e => e.Password)
                    .HasMaxLength(200)
                    .IsUnicode(false);

                entity.Property(e => e.PersistSecurityInfo)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Portnumber)
                    .HasColumnName("portnumber")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Sqlquery).HasColumnName("SQLQuery");

                entity.Property(e => e.Sqltext)
                    .HasColumnName("SQLText")
                    .IsUnicode(false);

                entity.HasOne(d => d.Organization)
                    .WithMany(p => p.Datasource)
                    .HasForeignKey(d => d.OrganizationId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Datasource_Organization");
            });

            modelBuilder.Entity<DatasourceUser>(entity =>
            {
                entity.HasKey(e => new { e.DatasourceId, e.UserId });

                entity.Property(e => e.DatasourceId).HasColumnName("DatasourceID");

                entity.Property(e => e.UserId).HasColumnName("UserID");

                entity.HasOne(d => d.Datasource)
                    .WithMany(p => p.DatasourceUser)
                    .HasForeignKey(d => d.DatasourceId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_DatasourceUser_Datasource");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.DatasourceUser)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_DatasourceUser_User");
            });

            modelBuilder.Entity<Eidatasource>(entity =>
            {
                entity.HasKey(e => e.DatasourceId);

                entity.ToTable("EIDatasource");

                entity.Property(e => e.DatasourceId).HasColumnName("DatasourceID");

                entity.Property(e => e.DatabaseType)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.DatabaseUserId)
                    .HasColumnName("DatabaseUserID")
                    .HasMaxLength(200)
                    .IsUnicode(false);

                entity.Property(e => e.DatasourceServerName)
                    .HasMaxLength(300)
                    .IsUnicode(false);

                entity.Property(e => e.InitialCatalog)
                    .HasMaxLength(300)
                    .IsUnicode(false);

                entity.Property(e => e.Password)
                    .HasMaxLength(200)
                    .IsUnicode(false);

                entity.Property(e => e.PersistSecurityInfo)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.HasOne(d => d.Survey)
                    .WithMany(p => p.Eidatasource)
                    .HasForeignKey(d => d.SurveyId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_Datasource_SurveyMetaData");
            });

            modelBuilder.Entity<LkRecordSource>(entity =>
            {
                entity.HasKey(e => e.RecordSourceId);

                entity.ToTable("lk_RecordSource");

                entity.Property(e => e.RecordSourceId).ValueGeneratedNever();

                entity.Property(e => e.RecordSource)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.RecordSourceDescription).HasMaxLength(100);
            });

            modelBuilder.Entity<LkStatus>(entity =>
            {
                entity.HasKey(e => e.StatusId);

                entity.ToTable("lk_Status");

                entity.Property(e => e.StatusId).ValueGeneratedNever();

                entity.Property(e => e.Status)
                    .IsRequired()
                    .HasMaxLength(20);
            });

            modelBuilder.Entity<LkSurveyType>(entity =>
            {
                entity.HasKey(e => e.SurveyTypeId);

                entity.ToTable("lk_SurveyType");

                entity.Property(e => e.SurveyTypeId).ValueGeneratedNever();

                entity.Property(e => e.SurveyType)
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<Organization>(entity =>
            {
                entity.HasIndex(e => e.Organization1)
                    .HasName("UK_Organization")
                    .IsUnique();

                entity.Property(e => e.Organization1)
                    .IsRequired()
                    .HasColumnName("Organization")
                    .HasMaxLength(400);

                entity.Property(e => e.OrganizationKey)
                    .IsRequired()
                    .HasMaxLength(500);
            });

            modelBuilder.Entity<ResponseDisplaySettings>(entity =>
            {
                entity.HasKey(e => new { e.FormId, e.ColumnName });

                entity.Property(e => e.ColumnName)
                    .HasMaxLength(200)
                    .IsUnicode(false);

                entity.HasOne(d => d.Form)
                    .WithMany(p => p.ResponseDisplaySettings)
                    .HasForeignKey(d => d.FormId)
                    .HasConstraintName("FK_ResponseGridcolumns_SurveyMetaData");
            });

            modelBuilder.Entity<ResponseXml>(entity =>
            {
                entity.HasKey(e => e.ResponseId);

                entity.Property(e => e.ResponseId).ValueGeneratedNever();

                entity.Property(e => e.Xml)
                    .IsRequired()
                    .HasColumnType("xml");
            });

            modelBuilder.Entity<Role>(entity =>
            {
                entity.HasIndex(e => e.RoleValue)
                    .HasName("IX_SecurityLevel")
                    .IsUnique();

                entity.Property(e => e.RoleId).HasColumnName("RoleID");

                entity.Property(e => e.RoleDescription)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<SharedCanvases>(entity =>
            {
                entity.HasKey(e => new { e.CanvasId, e.UserId });

                entity.Property(e => e.CanvasId).HasColumnName("CanvasID");

                entity.Property(e => e.UserId).HasColumnName("UserID");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.SharedCanvases)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_CanvasUser_User");
            });

            modelBuilder.Entity<SourceTables>(entity =>
            {
                entity.HasKey(e => new { e.SourceTableName, e.FormId });

                entity.Property(e => e.SourceTableName)
                    .HasMaxLength(200)
                    .IsUnicode(false);

                entity.Property(e => e.SourceTableXml)
                    .IsRequired()
                    .HasColumnType("xml");
            });

            modelBuilder.Entity<SurveyMetaData>(entity =>
            {
                entity.HasKey(e => e.SurveyId);

                entity.Property(e => e.SurveyId).ValueGeneratedNever();

                entity.Property(e => e.DataAccessRuleId).HasDefaultValueSql("((1))");

                entity.Property(e => e.DateCreated).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.DepartmentName).HasMaxLength(500);

                entity.Property(e => e.IsSqlproject).HasColumnName("IsSQLProject");

                entity.Property(e => e.OrganizationName).HasMaxLength(500);

                entity.Property(e => e.OwnerId).HasDefaultValueSql("((0))");

                entity.Property(e => e.StartDate).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.SurveyName)
                    .IsRequired()
                    .HasMaxLength(500);

                entity.Property(e => e.SurveyNumber).HasMaxLength(50);

                entity.Property(e => e.TemplateXml)
                    .IsRequired()
                    .HasColumnName("TemplateXML")
                    .HasColumnType("xml");

                entity.Property(e => e.TemplateXmlsize).HasColumnName("TemplateXMLSize");

                entity.HasOne(d => d.Organization)
                    .WithMany(p => p.SurveyMetaData)
                    .HasForeignKey(d => d.OrganizationId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_SurveyMetaData_Organization");

                entity.HasOne(d => d.SurveyType)
                    .WithMany(p => p.SurveyMetaData)
                    .HasForeignKey(d => d.SurveyTypeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_SurveyMetaData_lk_SurveyType");
            });

            modelBuilder.Entity<SurveyMetadataOrganization>(entity =>
            {
                entity.HasKey(e => new { e.SurveyId, e.OrganizationId });

                entity.HasOne(d => d.Organization)
                    .WithMany(p => p.SurveyMetadataOrganization)
                    .HasForeignKey(d => d.OrganizationId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_SurveyMetadataOrganization_Organization");

                entity.HasOne(d => d.Survey)
                    .WithMany(p => p.SurveyMetadataOrganization)
                    .HasForeignKey(d => d.SurveyId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_SurveyMetadataOrganization_SurveyMetaData");
            });

            modelBuilder.Entity<SurveyMetaDataUser>(entity =>
            {
                entity.HasKey(e => new { e.UserId, e.FormId });

                entity.HasOne(d => d.Form)
                    .WithMany(p => p.SurveyMetaDataUser)
                    .HasForeignKey(d => d.FormId)
                    .HasConstraintName("FK_SurveyMetaDataUser_SurveyMetaData");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.SurveyMetaDataUser)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_SurveyMetaDataUser_User");
            });

            modelBuilder.Entity<SurveyMetaDataView>(entity =>
            {
                entity.HasKey(e => e.SurveyId);

                entity.Property(e => e.SurveyId).ValueGeneratedNever();

                entity.Property(e => e.ViewTableName)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<SurveyResponse>(entity =>
            {
                entity.HasKey(e => e.ResponseId);

                entity.Property(e => e.ResponseId).ValueGeneratedNever();

                entity.Property(e => e.RecordSourceId).HasDefaultValueSql("((1))");

                entity.Property(e => e.ResponsePasscode).HasMaxLength(30);

                entity.Property(e => e.ResponseXML)
                    .IsRequired()
                    .HasColumnName("ResponseXML")
                    .HasColumnType("xml");

                entity.Property(e => e.ResponseXMLSize).HasColumnName("ResponseXMLSize");

                entity.HasOne(d => d.lk_RecordSource)
                    .WithMany(p => p.SurveyResponse)
                    .HasForeignKey(d => d.RecordSourceId)
                    .HasConstraintName("FK_SurveyResponse_lk_RecordSource");

                entity.HasOne(d => d.RelateParent)
                     //.WithMany(p => p.InverseRelateParent)
                     .WithMany(p => p.SurveyResponse2)
                    .HasForeignKey(d => d.RelateParentId)
                    .HasConstraintName("FK_SurveyResponse_SurveyResponse");

                entity.HasOne(d => d.lk_Status)
                    .WithMany(p => p.SurveyResponse)
                    .HasForeignKey(d => d.StatusId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_SurveyResponse_lk_Status");

                entity.HasOne(d => d.SurveyMetaData)
                    .WithMany(p => p.SurveyResponse)
                    .HasForeignKey(d => d.SurveyId)
                    .HasConstraintName("FK_SurveyResponse_SurveyMetaData");
            });

            modelBuilder.Entity<SurveyResponseTracking>(entity =>
            {
                entity.HasKey(e => e.ResponseId);

                entity.Property(e => e.ResponseId).ValueGeneratedNever();

                entity.Property(e => e.IsSqlresponse).HasColumnName("IsSQLResponse");

                entity.HasOne(d => d.Response)
                    .WithOne(p => p.SurveyResponseTracking)
                    .HasForeignKey<SurveyResponseTracking>(d => d.ResponseId)
                    .HasConstraintName("FK_SurveyResponseTracking_SurveyResponse");
            });

            modelBuilder.Entity<SurveyResponseUser>(entity =>
            {
                entity.HasKey(e => new { e.UserId, e.ResponseId });

                entity.HasOne(d => d.Response)
                    .WithMany(p => p.SurveyResponseUser)
                    .HasForeignKey(d => d.ResponseId)
                    .HasConstraintName("FK_SurveyResponseUser_SurveyResponse");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.SurveyResponseUser)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_SurveyResponseUser_User");
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.Property(e => e.UserId).HasColumnName("UserID");

                entity.Property(e => e.EmailAddress)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.FirstName)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.LastName)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.PasswordHash)
                    .IsRequired()
                    .IsUnicode(false);

                entity.Property(e => e.PhoneNumber)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Uguid).HasColumnName("UGuid");

                entity.Property(e => e.UserName)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<UserOrganization>(entity =>
            {
                entity.HasKey(e => new { e.UserId, e.OrganizationId });

                entity.Property(e => e.UserId).HasColumnName("UserID");

                entity.Property(e => e.OrganizationId).HasColumnName("OrganizationID");

                entity.HasOne(d => d.Organization)
                    .WithMany(p => p.UserOrganization)
                    .HasForeignKey(d => d.OrganizationId)
                    .HasConstraintName("FK_UserOrganization_Organization");

                entity.HasOne(d => d.Role)
                    .WithMany(p => p.UserOrganization)
                    .HasForeignKey(d => d.RoleId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_UserOrganization_Role");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.UserOrganization)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_UserOrganization_User");
            });
        }
        
    }   
}
