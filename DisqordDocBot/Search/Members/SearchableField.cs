using System.Reflection;
using Disqord;
using DisqordDocBot.Extensions;

namespace DisqordDocBot.Search
{
    public class SearchableField : SearchableMember
    {
        public override FieldInfo Info { get; }

        public SearchableField(FieldInfo info, SearchableType parent, string summary)
            : base(parent, summary)
        {
            Info = info;
        }
        
        public override LocalEmbedBuilder CreateInfoEmbed()
        {
            var eb = base.CreateInfoEmbed().AddCodeBlockField("Type", Info.FieldType.Humanize());

            if (Info.IsConstantField() && Info.GetRawConstantValue() is { } value)
                eb.AddCodeBlockField("Value", value);
            else if (Info.IsBootlegConstantField())
                eb.AddCodeBlockField("Value", Info.GetValue(null));
            
            return eb;
        }

        public override string ToString()
            => Info.IsConstantField() ? $"Constant: {base.ToString()}" : $"Field: {base.ToString()}";
    }
}