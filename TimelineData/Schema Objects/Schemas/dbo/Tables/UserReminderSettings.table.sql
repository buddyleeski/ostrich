CREATE TABLE [dbo].[UserReminderSettings](
	[UserId] [int] NOT NULL,
	[Enabled] [bit] NOT NULL CONSTRAINT [DF_UserReminderSettings_Enabled]  DEFAULT ((1)),
	[Frequency] [int] NOT NULL CONSTRAINT [DF_UserReminderSettings_Frequency]  DEFAULT ((5)),
 CONSTRAINT [PK_UserReminderSettings] PRIMARY KEY CLUSTERED 
(
	[UserId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON),
 CONSTRAINT [FK_UserReminderSettings_Users] FOREIGN KEY([UserId])
REFERENCES [dbo].[Users] ([Id])
)


