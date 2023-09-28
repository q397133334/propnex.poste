using Microsoft.EntityFrameworkCore;
using Volo.Abp.AuditLogging.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.FeatureManagement.EntityFrameworkCore;
using Volo.Abp.Identity.EntityFrameworkCore;
using Volo.Abp.OpenIddict.EntityFrameworkCore;
using Volo.Abp.PermissionManagement.EntityFrameworkCore;
using Volo.Abp.SettingManagement.EntityFrameworkCore;
using Volo.Abp.TenantManagement.EntityFrameworkCore;
using Propnex.Poster.WebServer.Entities;
using Propnex.Poster.WebServer;
using Volo.Abp.EntityFrameworkCore.Modeling;
using System.Reflection.Emit;
using AppAny.Quartz.EntityFrameworkCore.Migrations;
using AppAny.Quartz.EntityFrameworkCore.Migrations.MySql;

namespace Propnex.Poster.WebServer.Data;

public class PosterDbContext : AbpDbContext<PosterDbContext>
{
    public DbSet<PnTask> PnTasks { get; set; }

    public DbSet<PnTaskItem> PnTaskItems { get; set; }

    public DbSet<PnTaskLog> PnTaskLogs { get; set; }

    public DbSet<Machine> Machines { get; set; }

    public DbSet<PnUser> PnUsers { get; set; }
    public PosterDbContext(DbContextOptions<PosterDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.AddQuartz(builder => builder.UseMySql());
        /* Include modules to your migration db context */

        builder.ConfigurePermissionManagement();
        builder.ConfigureSettingManagement();
        builder.ConfigureAuditLogging();
        builder.ConfigureIdentity();
        builder.ConfigureOpenIddict();
        builder.ConfigureFeatureManagement();
        builder.ConfigureTenantManagement();

        /* Configure your own entities here */

        builder.Entity<PnTask>(b =>
        {
            b.ToTable(WebServerConsts.DbTablePrefix + "PnTasks");
            b.ConfigureByConvention(); //auto configure for the base class props
            b.Property(x => x.Number).IsRequired().HasMaxLength(128);
        });

        builder.Entity<PnTaskItem>(b =>
        {
            b.ToTable(WebServerConsts.DbTablePrefix + "PnTaskItems");
            b.ConfigureByConvention();
            b.Property(x => x.Number).IsRequired().HasMaxLength(128);
        });

        builder.Entity<PnTaskLog>(b =>
        {
            b.ToTable(WebServerConsts.DbTablePrefix + "PnTaskLog");
            b.ConfigureByConvention();
        });

        builder.Entity<Machine>(b =>
        {
            b.ToTable(WebServerConsts.DbTablePrefix + "Machines");
            b.ConfigureByConvention();
        });

        builder.Entity<PnUser>(b =>
        {
            b.ToTable(WebServerConsts.DbTablePrefix + "PnUsers");
            b.ConfigureByConvention();
        });
    }
}
