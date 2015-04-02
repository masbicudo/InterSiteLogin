using System;

namespace LoginProvider.Domain
{
    public abstract class Entity :
        IEntity
    {
        /// <summary>
        /// Gets or sets an unique numeric Id.
        /// </summary>
        public virtual long Id { get; set; }

        /// <summary>
        /// Gets or sets an unique <see cref="Guid"/>.
        /// </summary>
        public virtual Guid Guid { get; set; }

        /// <summary>
        /// Gets or sets an unique name.
        /// </summary>
        public virtual string Name { get; set; }

        /// <summary>
        /// Gets or sets a Log object indicating the creation of the current object.
        /// </summary>
        public virtual LogEntry CreateLog { get; set; }

        /// <summary>
        /// Gets or sets a Log object indicating the last change to the current object.
        /// </summary>
        public virtual LogEntry ChangeLog { get; set; }

        /// <summary>
        /// Gets or sets a Log object indicating the deletion of the current object.
        /// </summary>
        public virtual LogEntry DeleteLog { get; set; }
    }
}