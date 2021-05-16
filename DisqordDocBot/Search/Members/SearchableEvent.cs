using System.Reflection;
using Disqord;
using DisqordDocBot.Extensions;

namespace DisqordDocBot.Search
{
    public class SearchableEvent : SearchableMember
    {
        public override EventInfo Info { get; }

        public SearchableEvent(EventInfo info, SearchableType parent, string summary)
            : base(parent, summary)
        {
            Info = info;
        }
        
        public override LocalEmbedBuilder CreateInfoEmbed()
        {
            var eb = base.CreateInfoEmbed();

            if (Info.EventHandlerType is not null)
            {
                // do some quick black magic
                var method = Info.EventHandlerType.GetMethod("Invoke");
                
                eb.AddCodeBlockField("Arguments", $"({method.CreateArgString()})");
                eb.AddCodeBlockField("Return Type", method!.ReturnType.Humanize());
            }

            return eb;
        }

        public override string ToString() 
            => $"Event: {base.ToString()}";
    }
}