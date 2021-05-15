using System.Reflection;
using Disqord;
using DisqordDocBot.Extensions;

namespace DisqordDocBot.Search
{
    public class SearchableField : SearchableMember
    {
        public override FieldInfo Info { get; }

        public SearchableField(FieldInfo info, SearchableType parent)
            : base(parent)
        {
            Info = info;
        }
        
        public override LocalEmbedBuilder CreateInfoEmbed()
        {
            var eb = base.CreateInfoEmbed().AddField("Type", Info.FieldType.Name);
            
            
            if (Info.IsConstantField() && Info.GetRawConstantValue() is { } value)
                eb.AddField("Value: ", value);
            else if (Info.IsBootlegConstantField())
                eb.AddField("Value", Info.GetValue(null));
            
            return eb;
        }

        public override string ToString()
            => Info.IsConstantField() ? $"Constant: {base.ToString()}" : $"Field: {base.ToString()}";
    }
}