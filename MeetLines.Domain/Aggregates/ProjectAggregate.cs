using System;
using System.Collections.Generic;
using MeetLines.Domain.Entities;

namespace MeetLines.Domain.Aggregates
{
    public sealed class ProjectAggregate
    {
        public Project Project { get; private set; }
        public IReadOnlyCollection<Channel> Channels => _channels.AsReadOnly();
        public IReadOnlyCollection<Template> Templates => _templates.AsReadOnly();
        public IReadOnlyCollection<Tag> Tags => _tags.AsReadOnly();

        private readonly List<Channel> _channels = new();
        private readonly List<Template> _templates = new();
        private readonly List<Tag> _tags = new();

        public ProjectAggregate(Project project)
        {
            Project = project ?? throw new ArgumentNullException(nameof(project));
        }

        public void AddChannel(Channel channel)
        {
            if (channel == null) throw new ArgumentNullException(nameof(channel));
            if (channel.ProjectId != Project.Id) throw new InvalidOperationException("Channel project mismatch");
            _channels.Add(channel);
        }
    }
}
