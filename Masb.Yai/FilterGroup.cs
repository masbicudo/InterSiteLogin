namespace Masb.Yai
{
    public enum FilterGroup
    {
        /// <summary>
        /// Indicates the group of expression initializers, 
        /// responsible for creating the first expression given a component type.
        /// </summary>
        Initializer,

        /// <summary>
        /// Indicates the group of expression decorators, 
        /// responsible for decorating an already existing expression for a component type.
        /// </summary>
        Decorator,

        /// <summary>
        /// Indicates the group of expression post-processors, 
        /// responsible for changing parts of an already existing expression for a component type.
        /// </summary>
        PostProcessor,
    }
}