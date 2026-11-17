using System;

namespace MMF.Data
{
    /// <summary>Includes a tuning asset type in the designer window.</summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public sealed class TuningGroupAttribute : Attribute
    {
        public string Group { get; }
        public int Order { get; }

        public TuningGroupAttribute(string group, int order)
        {
            Group = group;
            Order = order;
        }
    }

    /// <summary>Supplies a designer-facing label for a serialized tuning field.</summary>
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class TuningLabelAttribute : Attribute
    {
        public string Label { get; }

        public TuningLabelAttribute(string label)
        {
            Label = label;
        }
    }
}
