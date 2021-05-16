using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Threading.Tasks;
using Disqord;
using Disqord.Bot;
using Disqord.Extensions.Interactivity;
using Disqord.Extensions.Interactivity.Menus.Paged;
using Disqord.Gateway;
using Disqord.Rest;
using DisqordDocBot.Extensions;
using DisqordDocBot.Services;
using DisqordDocBot.Tags;
using Qmmands;

namespace DisqordDocBot.Commands.Modules
{
    [Group("tag")]
    public class TagModule : DiscordGuildModuleBase
    {
        private readonly TagService _tagService;
        private readonly CommandService _commandService;

        public TagModule(TagService tagService, CommandService commandService)
        {
            _tagService = tagService;
            _commandService = commandService;
        }

        [Command("")]
        public DiscordCommandResult Help()
        {
            var admin = (Context.Message.Author as IMember).GetGuildPermissions().ManageGuild ? "\nAs a server admin, you override permission checks" : "";

            return Response(new LocalEmbedBuilder()
                .WithDefaultColor()
                .WithTitle("Tag")
                .WithDescription("`tag [name]` - Use a tag\n" +
                                 "`tag list` - List all tags in the server\n" +
                                 "`tag info [name...]` - Get information about a tag\n" +
                                 "`tag create [name] [content...]` - Create a new tag\n" +
                                 "`tag edit [name] [content...]` - Edit a tag\n" +
                                 "`tag remove [name...]` - Remove a tag\n" +
                                 "`tag claim [name...]` - Claim a stale tag\n" +
                                 "`tag transfer [name] [member...]` - Transfer ownership of a tag\n" +
                                 "`tag clone [name] [new name...]` - Clone a tag with a new name\n" +
                                 "`tag clone [name] [server id] (new name...)` - Clone a tag to another server\n" +
                                 admin));
        }

        [Command("")]
        public async Task<DiscordCommandResult> Tag([Remainder] string name)
        {
            var tag = await _tagService.GetTagAsync(Context.GuildId, name);
            if (tag is null) return await TagNotFoundResponse(name);
            
            tag.Uses ++;
            await _tagService.UpdateTagAsync(tag);
            return Response(tag.Content);
        }

        [Command("list")]
        public async Task List()
        {
            var tags = await _tagService.GetTagsAsync(Context.GuildId);
            tags = tags.OrderByDescending(x => x.Uses).ToList();

            var i = 0;
            var tagStrings = tags.Select(x =>
            {
                i++;
                return $"`#{i}` {x.Name} ({Mention.User(x.MemberId)})\n";
            });
            var stringPages = new List<string>();
            
            var current = "";
            foreach (var tagString in tagStrings)
            {
                if((current + tagString).Length <= 2048)
                    current += tagString;
                else
                {
                    stringPages.Add(current);
                    current = tagString;
                }
            }
            if(!string.IsNullOrWhiteSpace(current))
                stringPages.Add(current);

            var pages = stringPages.Select(x => new Page(
                new LocalEmbedBuilder()
                    .WithDefaultColor()
                    .WithTitle("Tags")
                    .WithDescription(x)
                    .WithFooter($"Page {stringPages.IndexOf(x) + 1} of {stringPages.Count}")))
                .ToList();

            switch (pages.Count)
            {
                case 0:
                    await Response("There are no tags for this server.");
                    return;
                case 1:
                    await Response(pages[0].Embed);
                    return;
            }

            IPageProvider pageProvider = new DefaultPageProvider(pages);
            PagedMenu menu = new(Context.Author.Id, pageProvider);
            var ext = Context.Bot.GetRequiredExtension<InteractivityExtension>();
            await ext.StartMenuAsync(Context.Channel.Id, menu);
        }
        
        [Command("info")]
        public async Task<DiscordCommandResult> Info([Remainder] string name)
        {
            var tag = await _tagService.GetTagAsync(Context.GuildId, name);
            if (tag is null) return await TagNotFoundResponse(name);
            
            var member = Context.Guild.GetMember(tag.MemberId) ?? 
                         await Context.Guild.FetchMemberAsync(tag.MemberId);

            return Response(new LocalEmbedBuilder()
                .WithDefaultColor()
                .WithTitle($"Tag: {tag.Name}")
                .AddField("Owner", member is null ? $"{tag.MemberId} (not in server)" : member.Mention, true)
                .AddField("Uses", tag.Uses, true)
                .AddField("Rank", $"#{await _tagService.GetTagRankAsync(tag)}", true)
                .AddField("Created at", $"{tag.CreatedAt:yyyy-MM-dd}", true)
                .AddField("Edited at", $"{tag.EditedAt:yyyy-MM-dd}", true)
                .AddField("Revisions", tag.Revisions, true));
        }
        
        [Command("create")]
        public async Task<DiscordCommandResult> Create(string name, [Remainder] string value)
        {
            if (!IsTagNameValid(name))
                return Response($"The tag name \"{name}\" is forbidden, please choose another name.");
            if(await _tagService.GetTagAsync(Context.GuildId, name) is not null)
                return Response($"The tag \"{name}\" already exists, please choose another name.");
            
            var tag = new Tag
            {
                GuildId = Context.GuildId,
                MemberId = Context.Message.Author.Id,
                Name = name,
                Content = value,
                CreatedAt = DateTimeOffset.UtcNow,
                EditedAt = DateTimeOffset.UtcNow
            };
            await _tagService.CreateTagAsync(tag);
            
            return Response($"The tag \"{name}\" was created successfully.");
        }
        
        [Command("edit")]
        public async Task<DiscordCommandResult> Edit(string name, [Remainder] string content)
        {
            var tag = await _tagService.GetTagAsync(Context.GuildId, name);
            if (tag is null) return await TagNotFoundResponse(name);

            if (tag.MemberId != Context.Message.Author.Id && !(Context.Message.Author as IMember).GetGuildPermissions().ManageGuild)
                return Response($"The tag \"{name}\" does not belong to you.");

            tag.Content = content;
            tag.EditedAt = DateTimeOffset.UtcNow;
            tag.Revisions++;
            await _tagService.UpdateTagAsync(tag);
            
            return Response($"The tag \"{name}\" was edited successfully.");
        }

        [Command("remove")]
        public async Task<DiscordCommandResult> Remove([Remainder] string name)
        {
            var tag = await _tagService.GetTagAsync(Context.GuildId, name);
            if (tag is null) return await TagNotFoundResponse(name);

            if (tag.MemberId != Context.Message.Author.Id && !(Context.Message.Author as IMember).GetGuildPermissions().ManageGuild)
                return Response($"The tag \"{name}\" does not belong to you.");

            await _tagService.RemoveTagAsync(tag);
            
            return Response($"The tag \"{name}\" was removed successfully.");
        }

        [Command("claim")]
        public async Task<DiscordCommandResult> Claim([Remainder] string name)
        {
            var tag = await _tagService.GetTagAsync(Context.GuildId, name);
            if (tag is null) return await TagNotFoundResponse(name);
            
            var member = Context.Guild.GetMember(tag.MemberId) ?? 
                         await Context.Guild.FetchMemberAsync(tag.MemberId);
                
            if(member is not null)
                return Response($"The owner of the tag \"{name}\" is still in the server.");

            tag.MemberId = Context.Message.Author.Id;
            await _tagService.UpdateTagAsync(tag);
            
            return Response($"Ownership of the tag \"{name}\" was successfully transferred to you.");
        }

        [Command("transfer")]
        public async Task<DiscordCommandResult> Transfer(string name, [Remainder] [RequireNotBot] IMember member)
        {
            var tag = await _tagService.GetTagAsync(Context.GuildId, name);
            if (tag is null) return await TagNotFoundResponse(name);
            
            if (tag.MemberId != Context.Message.Author.Id && !(Context.Message.Author as IMember).GetGuildPermissions().ManageGuild)
                return Response($"The tag \"{name}\" does not belong to you.");

            tag.MemberId = member.Id;
            await _tagService.UpdateTagAsync(tag);
            
            return Response($"Ownership of the tag \"{name}\" was successfully transferred to {member.Mention}.");
        }
        
        [Command("clone")]
        public async Task<DiscordCommandResult> Clone(string name, [Remainder] string newName)
        {
            if (!IsTagNameValid(newName))
                return Response($"The tag name \"{newName}\" is forbidden, please choose another name.");
            
            var tag = await _tagService.GetTagAsync(Context.GuildId, name);
            if (tag is null) return await TagNotFoundResponse(name);

            var otherTag = await _tagService.GetTagAsync(Context.GuildId, newName);
            if(otherTag is not null)
                return Response($"The tag \"{newName}\" already exists, please choose another name.");

            otherTag = new Tag
            {
                GuildId = Context.GuildId,
                MemberId = Context.Message.Author.Id,
                Name = newName,
                Content = tag.Content,
                CreatedAt = DateTimeOffset.UtcNow,
                EditedAt = DateTimeOffset.UtcNow
            };
            await _tagService.CreateTagAsync(otherTag);
            
            return Response($"The tag \"{name}\" was cloned successfully to \"{newName}\".");
        }
        
        [Command("clone")]
        public async Task<DiscordCommandResult> Clone(string name, Snowflake guildId, [Remainder] string newName = null)
        {
            newName ??= name;
            if (!IsTagNameValid(newName))
                return Response($"The tag name \"{newName}\" is forbidden, please choose another name.");
            
            var tag = await _tagService.GetTagAsync(Context.GuildId, name);
            if (tag is null) return await TagNotFoundResponse(name);
            
            IGuild guild = Context.Bot.GetGuild(guildId);
            var member = guild is null ? null : 
                guild.GetMember(Context.Message.Author.Id) ?? 
                await guild.FetchMemberAsync(Context.Message.Author.Id);

            if (guild is null || member is null)
                return Response($"I couldn't find a guild with id {guildId}.");
            
            var otherTag = await _tagService.GetTagAsync(guild.Id, newName);
            if(otherTag is not null)
                return Response($"The tag \"{newName}\" already exists in {guild.Name}, please choose another name.");

            otherTag = new Tag
            {
                GuildId = guild.Id,
                MemberId = Context.Message.Author.Id,
                Name = newName,
                Content = tag.Content,
                CreatedAt = DateTimeOffset.UtcNow,
                EditedAt = DateTimeOffset.UtcNow
            };
            await _tagService.CreateTagAsync(otherTag);
            
            return Response($"The tag \"{name}\" was cloned successfully to \"{newName}\" in {guild.Name}.");
        }

        private bool IsTagNameValid(string name)
            => _commandService
                .GetAllModules()
                .First(x => x.Type == typeof(TagModule)).Commands
                .All(x => x.Aliases
                    .All(y => !string.Equals(y, name, StringComparison.CurrentCultureIgnoreCase)));

        private async Task<DiscordCommandResult> TagNotFoundResponse(string name)
        {
            var closeTags = await _tagService.SearchTagsAsync(Context.GuildId, name);
            if (closeTags.Count == 0) return Response($"I couldn't find a tag with the name \"{name}\".");
            
            var didYouMean = " • " + string.Join("\n • ", closeTags.Take(3).Select(x => x.Name));
            return Response($"I couldn't find a tag with the name \"{name}\", did you mean...\n{didYouMean}");
        }
    }
}