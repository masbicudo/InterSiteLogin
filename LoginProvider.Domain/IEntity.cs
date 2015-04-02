using System;

namespace LoginProvider.Domain
{
    public interface IEntity
    {
        /// <summary>
        /// Gets or sets an unique numeric Id.
        /// </summary>
        long Id { get; set; }

        /// <summary>
        /// Gets or sets an unique <see cref="Guid"/>.
        /// </summary>
        Guid Guid { get; set; }

        /// <summary>
        /// Gets or sets an unique name.
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// Gets or sets a Log object indicating the creation of the current object.
        /// </summary>
        LogEntry CreateLog { get; set; }

        /// <summary>
        /// Gets or sets a Log object indicating the last change to the current object.
        /// </summary>
        LogEntry ChangeLog { get; set; }

        /// <summary>
        /// Gets or sets a Log object indicating the deletion of the current object.
        /// </summary>
        LogEntry DeleteLog { get; set; }
    }
}