using System;
using System.Linq;
using System.Threading.Tasks;
using Disqord;
using Disqord.Bot;
using Disqord.Gateway;
using Disqord.Rest;
using DisqordDocBot.Extensions;
using DisqordDocBot.Services;
using DisqordDocBot.Tags;
using Microsoft.EntityFrameworkCore;
using Qmmands;

namespace DisqordDocBot.Commands.Modules
{
    [Group("tag")]
    public class TagModule : DiscordGuildModuleBase
    {
        private readonly TagService _tagService;
        
        public TagModule(TagService tagService)
        {
            _tagService = tagService;
        }

        [Command("")]
        public async Task<DiscordCommandResult> Help()
        {
            return Response(new LocalEmbedBuilder()
                .WithDefaultColor()
                .WithTitle("Tag")
                .WithDescription("`tag [name]` - View a tag\n" +
                                 "`tag create [name] [content...]` - Create a new tag"));
        }

        [Command("")]
        public async Task<DiscordCommandResult> Tag([Remainder] string name)
        {
            var tag = await _tagService.GetTagAsync(Context.GuildId, name);
            if (tag is null) return await TagNotFoundResponse(name);
            
            tag.Uses += 1;
            await _tagService.UpdateTagAsync(tag);
            return Response(tag.Content);
        }

        [Command("create")]
        public async Task<DiscordCommandResult> Create(string name, [Remainder] string value)
        {
            if (Global.ForbiddenTagNames.Contains(name.ToLower()))
                return Response($"The tag name \"{name}\" is forbidden, please choose another name.");
            if(await _tagService.GetTagAsync(Context.GuildId, name) is not null)
                return Response($"The tag \"{name}\" already exists, please choose another name.");
            
            var tag = new Tag
            {
                GuildId = Context.GuildId,
                MemberId = Context.Message.Author.Id,
                Name = name,
                Content = value,
                CreatedAt = DateTimeOffset.UtcNow
            };
            await _tagService.CreateTagAsync(tag);
            
            return Response($"The tag \"{name}\" was created successfully.");
        }
        
        [Command("edit")]
        public async Task<DiscordCommandResult> Edit(string name, [Remainder] string content)
        {
            var tag = await _tagService.GetTagAsync(Context.GuildId, name);
            if (tag is null) return await TagNotFoundResponse(name);

            if (tag.MemberId != Context.CurrentMember.Id)
                Response($"The tag \"{name}\" does not belong to you.");

            tag.Content = content;
            await _tagService.UpdateTagAsync(tag);
            
            return Response($"The tag \"{name}\" was edited successfully.");
        }
        
        [Command("remove")]
        public async Task<DiscordCommandResult> Remove(string name)
        {
            var tag = await _tagService.GetTagAsync(Context.GuildId, name);
            if (tag is null) return await TagNotFoundResponse(name);

            if (tag.MemberId != Context.CurrentMember.Id)
                Response($"The tag \"{name}\" does not belong to you.");

            await _tagService.RemoveTagAsync(tag);
            
            return Response($"The tag \"{name}\" was removed successfully.");
        }

        [Command("info")]
        public async Task<DiscordCommandResult> Info(string name)
        {
            var tag = await _tagService.GetTagAsync(Context.GuildId, name);
            if (tag is null) return await TagNotFoundResponse(name);
            
            IMember member = Context.Guild.GetMember(tag.MemberId) ?? 
                             await Context.Guild.FetchMemberAsync(tag.MemberId);
                
            return Response(new LocalEmbedBuilder()
                .WithDefaultColor()
                .WithTitle($"Tag: {tag.Name}")
                .AddField("Owner", member is null ? $"{tag.MemberId} (not in server)" : member.Mention, true)
                .AddField("Uses", tag.Uses, true)
                .AddField("Rank", $"#{await _tagService.GetTagRankAsync(tag)}", true)
                .WithFooter(new LocalEmbedFooterBuilder().WithText("Created at"))
                .WithTimestamp(tag.CreatedAt));
        }
        
        private async Task<DiscordCommandResult> TagNotFoundResponse(string name)
        {
            var closeTags = await _tagService.SearchTagsAsync(Context.GuildId, name);
            if (closeTags.Count == 0) return Response($"I couldn't find a tag with the name \"{name}\".");
            
            string didYouMean = " • " + string.Join("\n • ", closeTags.Take(3).Select(x => x.Name));
            return Response($"I couldn't find a tag with the name \"{name}\", did you mean...\n{didYouMean}");
        }
    }
}