namespace Auth.Identity.Configurations
{
    using Entities;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class RefreshTokenEntityConfiguration : IEntityTypeConfiguration<RefreshToken>
    {
        public void Configure(EntityTypeBuilder<RefreshToken> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Token).IsRequired();
            builder.Property(x => x.JwtTokenId).IsRequired();
            builder.Property(x => x.Expires).IsRequired();
            builder.Property(x => x.Used).IsRequired();
            builder.Property(x => x.Invalidated).IsRequired();
            builder.HasOne(x => x.User)
                   .WithMany(u => u.RefreshTokens)
                   .HasForeignKey(x => x.UserId)
                   .IsRequired();
        }
    }
}