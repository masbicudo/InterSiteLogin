namespace LoginProvider.Domain
{
    /// <summary>
    /// Models a user that can be recognized by this login provider.
    /// </summary>
    public class User : Entity
    {
        /// <summary>
        /// Gets or sets the bytes of the hashed password of the user.
        /// </summary>
        public virtual byte[] Password { get; set; }

        /// <summary>
        /// Gets or sets the bytes appended to the original password before hashing it.
        /// </summary>
        public virtual byte[] PasswordSalt { get; set; }

        /// <summary>
        /// Gets or sets the user display name.
        /// </summary>
        public virtual string DisplayName { get; set; }

        /// <summary>
        /// Gets or sets the login name used to log this user in.
        /// </summary>
        public virtual string Login { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this user can be used or not.
        /// </summary>
        public virtual bool Enabled { get; set; }
    }
}