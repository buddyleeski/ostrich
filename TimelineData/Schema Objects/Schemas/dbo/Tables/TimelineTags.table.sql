CREATE TABLE [dbo].[TimelineTags](
	[TimelineId] [int] NOT NULL,
	[TagId] [int] NOT NULL,
 CONSTRAINT [PK_TimelineTags] PRIMARY KEY CLUSTERED 
(
	[TimelineId] ASC,
	[TagId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON),
 CONSTRAINT [FK_TimelineTags_Timelines] FOREIGN KEY([TimelineId])
REFERENCES [dbo].[Timelines] ([Id]),
 CONSTRAINT [FK_TimelineTags_UserTags] FOREIGN KEY([TagId])
REFERENCES [dbo].[UserTags] ([Id])
)


