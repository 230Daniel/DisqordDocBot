using Disqord;

namespace DisqordDocBot.Extensions
{
    public static class EmbedBuildingExtensions
    {


        public static LocalEmbed WithDefaultColor(this LocalEmbed eb)
            => eb.WithColor(Global.DefaultEmbedColor);

        public static LocalEmbed FillLineWithEmptyFields(this LocalEmbed eb)
        {
            var currentInlineFieldCount = 0;

            for (var i = eb.Fields.Count - 1; i >= 0; i--)
            {
                if (!eb.Fields[i].IsInline)
                    break;

                currentInlineFieldCount++;
            }

            // line is already full
            if (currentInlineFieldCount % 3 == 0)
                return eb;

            while (currentInlineFieldCount % 3 != 0)
            {
                eb.AddInlineBlankField();
                currentInlineFieldCount++;
            }

            return eb;
        }

        public static LocalEmbed AddInlineField(this LocalEmbed eb, string name, string value)
            => eb.AddField(name, value, true);

        public static LocalEmbed AddInlineField(this LocalEmbed eb, string name, object value)
            => eb.AddField(name, value, true);

        public static LocalEmbed AddInlineField(this LocalEmbed eb, LocalEmbedField efb)
            => eb.AddField(efb);

        public static LocalEmbed AddInlineBlankField(this LocalEmbed eb)
            => eb.AddBlankField(true);

        public static LocalEmbed AddCodeBlockField(this LocalEmbed eb, string name, string value)
            => eb.AddField(name, Markdown.CodeBlock("csharp", value));

        public static LocalEmbed AddCodeBlockField(this LocalEmbed eb, string name, object value)
            => eb.AddField(name, Markdown.CodeBlock("csharp", value.ToString()));

        public static LocalEmbed AddInlineCodeBlockField(this LocalEmbed eb, string name, string value)
            => eb.AddField(name, Markdown.CodeBlock("csharp", value), true);

        public static LocalEmbed AddInlineCodeBlockField(this LocalEmbed eb, string name, object value)
            => eb.AddField(name, Markdown.CodeBlock("csharp", value.ToString()), true);
    }
}
