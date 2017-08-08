using System;
using Microsoft.EntityFrameworkCore;

namespace LeanCode.PushNotifications.EF
{
    public class PushNotificationTokenEntity
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public DeviceType DeviceType { get; set; }
        public string Token { get; set; }
        public DateTime DateCreated { get; set; }

        public static void Configure(ModelBuilder builder)
        {
            builder.Entity<PushNotificationTokenEntity>(c =>
            {
                c.HasKey(e => e.Id).ForSqlServerIsClustered(false);
                c.HasIndex(e => new { e.UserId, e.DeviceType }).ForSqlServerIsClustered(true);
                c.HasIndex(e => new { e.UserId, e.DeviceType, e.Token });

                c.Property(e => e.Token).IsRequired(true).HasMaxLength(512);
            });
        }
    }
}
