using System;

namespace LoginProvider.Domain
{
    public class LogEntry : Entity
    {
        /// <summary>
        /// Gets or sets the type of the logged action.
        /// </summary>
        public virtual LogType LogType { get; set; }

        /// <summary>
        /// Gets or sets the date and time of the logged action.
        /// </summary>
        public virtual DateTime LogDateTime { get; set; }

        /// <summary>
        /// Gets or sets the user that performed the action.
        /// </summary>
        public virtual User LogUser { get; set; }

        /// <summary>
        /// Gets or sets the entity type associated with this log.
        /// </summary>
        public virtual EntityType EntityType { get; set; }

        /// <summary>
        /// Gets or sets the numeric ID of the entity being logged.
        /// </summary>
        public virtual long EntityId { get; set; }

        /// <summary>
        /// Gets or sets meaningful data for the performed action.
        /// </summary>
        /// <remarks>
        /// <para>If LogType is:</para>
        /// <para>Create => null</para>
        /// <para>Update => each changed Property / Old value</para>
        /// <para>Delete => each Property / Value</para>
        /// <para>Other => affected Properties / how to undo</para>
        /// </remarks>
        public virtual string EntityData { get; set; }
    }
}