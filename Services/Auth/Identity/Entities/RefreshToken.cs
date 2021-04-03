namespace Auth.Identity.Entities
{
    using System;
    using NodaTime;

    public class RefreshToken
    {
        public int Id { get; set; }
        public string Token { get; set; }
        public string JwtTokenId { get; set; }
        public Instant Expires { get; set; }
        public bool Used { get; set; }
        public bool Invalidated { get; set; }

        public Guid UserId { get; set; }
        public virtual AppUser User { get; set; }
    }
}