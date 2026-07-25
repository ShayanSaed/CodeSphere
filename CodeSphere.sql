USE [master]
GO
/****** Object:  Database [CodeSphere]    Script Date: 7/3/2026 3:32:54 PM ******/
CREATE DATABASE [CodeSphere]
 CONTAINMENT = NONE
 ON  PRIMARY 
( NAME = N'CodeSphere', FILENAME = N'C:\Program Files\Microsoft SQL Server\MSSQL15.MSSQLSERVER\MSSQL\DATA\CodeSphere.mdf' , SIZE = 8192KB , MAXSIZE = UNLIMITED, FILEGROWTH = 65536KB )
 LOG ON 
( NAME = N'CodeSphere_log', FILENAME = N'C:\Program Files\Microsoft SQL Server\MSSQL15.MSSQLSERVER\MSSQL\DATA\CodeSphere_log.ldf' , SIZE = 8192KB , MAXSIZE = 2048GB , FILEGROWTH = 65536KB )
 WITH CATALOG_COLLATION = DATABASE_DEFAULT
GO
ALTER DATABASE [CodeSphere] SET COMPATIBILITY_LEVEL = 150
GO
IF (1 = FULLTEXTSERVICEPROPERTY('IsFullTextInstalled'))
begin
EXEC [CodeSphere].[dbo].[sp_fulltext_database] @action = 'enable'
end
GO
ALTER DATABASE [CodeSphere] SET ANSI_NULL_DEFAULT OFF 
GO
ALTER DATABASE [CodeSphere] SET ANSI_NULLS OFF 
GO
ALTER DATABASE [CodeSphere] SET ANSI_PADDING OFF 
GO
ALTER DATABASE [CodeSphere] SET ANSI_WARNINGS OFF 
GO
ALTER DATABASE [CodeSphere] SET ARITHABORT OFF 
GO
ALTER DATABASE [CodeSphere] SET AUTO_CLOSE OFF 
GO
ALTER DATABASE [CodeSphere] SET AUTO_SHRINK OFF 
GO
ALTER DATABASE [CodeSphere] SET AUTO_UPDATE_STATISTICS ON 
GO
ALTER DATABASE [CodeSphere] SET CURSOR_CLOSE_ON_COMMIT OFF 
GO
ALTER DATABASE [CodeSphere] SET CURSOR_DEFAULT  GLOBAL 
GO
ALTER DATABASE [CodeSphere] SET CONCAT_NULL_YIELDS_NULL OFF 
GO
ALTER DATABASE [CodeSphere] SET NUMERIC_ROUNDABORT OFF 
GO
ALTER DATABASE [CodeSphere] SET QUOTED_IDENTIFIER OFF 
GO
ALTER DATABASE [CodeSphere] SET RECURSIVE_TRIGGERS OFF 
GO
ALTER DATABASE [CodeSphere] SET  DISABLE_BROKER 
GO
ALTER DATABASE [CodeSphere] SET AUTO_UPDATE_STATISTICS_ASYNC OFF 
GO
ALTER DATABASE [CodeSphere] SET DATE_CORRELATION_OPTIMIZATION OFF 
GO
ALTER DATABASE [CodeSphere] SET TRUSTWORTHY OFF 
GO
ALTER DATABASE [CodeSphere] SET ALLOW_SNAPSHOT_ISOLATION OFF 
GO
ALTER DATABASE [CodeSphere] SET PARAMETERIZATION SIMPLE 
GO
ALTER DATABASE [CodeSphere] SET READ_COMMITTED_SNAPSHOT OFF 
GO
ALTER DATABASE [CodeSphere] SET HONOR_BROKER_PRIORITY OFF 
GO
ALTER DATABASE [CodeSphere] SET RECOVERY FULL 
GO
ALTER DATABASE [CodeSphere] SET  MULTI_USER 
GO
ALTER DATABASE [CodeSphere] SET PAGE_VERIFY CHECKSUM  
GO
ALTER DATABASE [CodeSphere] SET DB_CHAINING OFF 
GO
ALTER DATABASE [CodeSphere] SET FILESTREAM( NON_TRANSACTED_ACCESS = OFF ) 
GO
ALTER DATABASE [CodeSphere] SET TARGET_RECOVERY_TIME = 60 SECONDS 
GO
ALTER DATABASE [CodeSphere] SET DELAYED_DURABILITY = DISABLED 
GO
EXEC sys.sp_db_vardecimal_storage_format N'CodeSphere', N'ON'
GO
ALTER DATABASE [CodeSphere] SET QUERY_STORE = OFF
GO
USE [CodeSphere]
GO
/****** Object:  UserDefinedFunction [dbo].[GetEngagementScore]    Script Date: 7/3/2026 3:32:54 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE   FUNCTION [dbo].[GetEngagementScore]
(
    @ArticleID INT
)
RETURNS INT
AS
BEGIN
    DECLARE
        @ViewCount INT = 0,
        @CommentCount INT = 0,
        @ReactionCount INT = 0,
        @EngagementScore INT;

    -- Retrieve View Count --
    SELECT
        @ViewCount = ViewCount
    FROM Articles
    WHERE ArticleID = @ArticleID;

    -- Count Comments --
    SELECT
        @CommentCount = COUNT(*)
    FROM Comments
    WHERE ArticleID = @ArticleID;

    -- Count Reactions --
    SELECT
        @ReactionCount = COUNT(*)
    FROM Reactions
    WHERE ArticleID = @ArticleID;

    -- Calculate Score --
    SET @EngagementScore =
            ISNULL(@ViewCount,0)
          + (@CommentCount * 3)
          + (@ReactionCount * 2);

    RETURN @EngagementScore;
END;
GO
/****** Object:  UserDefinedFunction [dbo].[GetTotalFollowers]    Script Date: 7/3/2026 3:32:54 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE   FUNCTION [dbo].[GetTotalFollowers]
(
    @UserID INT
)
RETURNS INT
AS
BEGIN
    DECLARE
        @FollowerCount INT;

    SELECT
        @FollowerCount = COUNT(*)
    FROM Follows
    WHERE FollowingUserID = @UserID;

    RETURN ISNULL(@FollowerCount,0);
END;
GO
/****** Object:  Table [dbo].[Users]    Script Date: 7/3/2026 3:32:54 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Users](
	[UserID] [int] IDENTITY(1,1) NOT NULL,
	[Username] [varchar](50) NOT NULL,
	[Email] [varchar](100) NOT NULL,
	[PasswordHash] [varchar](255) NOT NULL,
	[JoinDate] [datetime] NOT NULL,
	[Status] [nvarchar](20) NOT NULL,
 CONSTRAINT [PK_Users] PRIMARY KEY CLUSTERED 
(
	[UserID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Categories]    Script Date: 7/3/2026 3:32:54 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Categories](
	[CategoryID] [int] IDENTITY(1,1) NOT NULL,
	[CategoryName] [varchar](100) NOT NULL,
	[Description] [nvarchar](500) NULL,
 CONSTRAINT [PK_Categories] PRIMARY KEY CLUSTERED 
(
	[CategoryID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Articles]    Script Date: 7/3/2026 3:32:54 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Articles](
	[ArticleID] [int] IDENTITY(1,1) NOT NULL,
	[UserID] [int] NOT NULL,
	[CategoryID] [int] NOT NULL,
	[Title] [nvarchar](200) NOT NULL,
	[Content] [nvarchar](max) NOT NULL,
	[PublishDate] [datetime] NULL,
	[ViewCount] [int] NOT NULL,
	[ReadingTime] [int] NOT NULL,
	[Status] [varchar](20) NOT NULL,
 CONSTRAINT [PK_Articles] PRIMARY KEY CLUSTERED 
(
	[ArticleID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  View [dbo].[PublishedArticlesView]    Script Date: 7/3/2026 3:32:54 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [dbo].[PublishedArticlesView]
AS
SELECT
    A.ArticleID,
    A.Title,
    U.Username AS Author,
    C.CategoryName,
    A.PublishDate,
    A.ViewCount,
    A.ReadingTime
FROM Articles A
INNER JOIN Users U
    ON A.UserID = U.UserID
INNER JOIN Categories C
    ON A.CategoryID = C.CategoryID
WHERE A.Status = 'Published';
GO
/****** Object:  Table [dbo].[Comments]    Script Date: 7/3/2026 3:32:54 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Comments](
	[CommentID] [int] IDENTITY(1,1) NOT NULL,
	[ArticleID] [int] NOT NULL,
	[UserID] [int] NOT NULL,
	[CommentText] [nvarchar](1000) NOT NULL,
	[CommentDate] [datetime] NOT NULL,
 CONSTRAINT [PK_Comments] PRIMARY KEY CLUSTERED 
(
	[CommentID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Reactions]    Script Date: 7/3/2026 3:32:54 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Reactions](
	[ReactionID] [int] IDENTITY(1,1) NOT NULL,
	[UserID] [int] NOT NULL,
	[ArticleID] [int] NOT NULL,
	[ReactionType] [varchar](20) NOT NULL,
	[ReactionDate] [datetime] NOT NULL,
 CONSTRAINT [PK_Reactions] PRIMARY KEY CLUSTERED 
(
	[ReactionID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  View [dbo].[UserActivityView]    Script Date: 7/3/2026 3:32:54 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [dbo].[UserActivityView]
AS
SELECT
	U.UserID,
	U.Username,
	COUNT(DISTINCT A.ArticleID) AS 'Total Articles',
	COUNT(DISTINCT C.CommentID) AS 'Total Comments',
	COUNT(DISTINCT R.ReactionID) AS 'Total Reactions'
FROM Users U
LEFT JOIN Articles A
	ON U.UserID = A.UserID
LEFT JOIN Comments C
	ON U.UserID = C.UserID
LEFT JOIN Reactions R
	ON U.UserID = R.UserID
GROUP BY
	U.UserID,
	U.Username
GO
/****** Object:  View [dbo].[TrendingArticlesView]    Script Date: 7/3/2026 3:32:54 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [dbo].[TrendingArticlesView]
AS
SELECT
    A.ArticleID,
    A.Title,
    U.Username AS Author,
    C.CategoryName,
    A.ViewCount,
    COUNT(DISTINCT CM.CommentID) AS CommentCount,
    COUNT(DISTINCT R.ReactionID) AS ReactionCount,
    (
        A.ViewCount
        + (COUNT(DISTINCT CM.CommentID) * 3)
        + (COUNT(DISTINCT R.ReactionID) * 2)
    ) AS EngagementScore
FROM Articles A
INNER JOIN Users U
    ON A.UserID = U.UserID
INNER JOIN Categories C
    ON A.CategoryID = C.CategoryID
LEFT JOIN Comments CM
    ON A.ArticleID = CM.ArticleID
LEFT JOIN Reactions R
    ON A.ArticleID = R.ArticleID
GROUP BY
    A.ArticleID,
    A.Title,
    U.Username,
    C.CategoryName,
    A.ViewCount;
GO
/****** Object:  Table [dbo].[ArticleTags]    Script Date: 7/3/2026 3:32:54 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ArticleTags](
	[ArticleID] [int] NOT NULL,
	[TagID] [int] NOT NULL,
 CONSTRAINT [PK_ArticleTags] PRIMARY KEY CLUSTERED 
(
	[ArticleID] ASC,
	[TagID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Bookmarks]    Script Date: 7/3/2026 3:32:54 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Bookmarks](
	[BookmarkID] [int] IDENTITY(1,1) NOT NULL,
	[UserID] [int] NOT NULL,
	[ArticleID] [int] NOT NULL,
	[SavedDate] [datetime] NOT NULL,
 CONSTRAINT [PK_Bookmarks] PRIMARY KEY CLUSTERED 
(
	[BookmarkID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Follows]    Script Date: 7/3/2026 3:32:54 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Follows](
	[FollowID] [int] IDENTITY(1,1) NOT NULL,
	[FollowerUserID] [int] NOT NULL,
	[FollowingUserID] [int] NOT NULL,
	[FollowDate] [datetime] NOT NULL,
 CONSTRAINT [PK_Follows_1] PRIMARY KEY CLUSTERED 
(
	[FollowID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Tags]    Script Date: 7/3/2026 3:32:54 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Tags](
	[TagID] [int] IDENTITY(1,1) NOT NULL,
	[TagName] [varchar](50) NOT NULL,
	[Description] [nvarchar](200) NULL,
 CONSTRAINT [PK_Tags] PRIMARY KEY CLUSTERED 
(
	[TagID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[UserProfiles]    Script Date: 7/3/2026 3:32:54 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[UserProfiles](
	[ProfileID] [int] IDENTITY(1,1) NOT NULL,
	[UserID] [int] NOT NULL,
	[FullName] [nvarchar](100) NULL,
	[Bio] [nvarchar](500) NULL,
	[Country] [varchar](50) NULL,
	[WebsiteURL] [nvarchar](255) NULL,
	[ProfileImageURL] [nvarchar](255) NULL,
 CONSTRAINT [PK_UserProfiles] PRIMARY KEY CLUSTERED 
(
	[ProfileID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
SET IDENTITY_INSERT [dbo].[Articles] ON 

INSERT [dbo].[Articles] ([ArticleID], [UserID], [CategoryID], [Title], [Content], [PublishDate], [ViewCount], [ReadingTime], [Status]) VALUES (1, 10, 2, N'SQL Optimization Tips', N'Content about SQL performance...', CAST(N'2025-06-01T00:00:00.000' AS DateTime), 123, 5, N'Published')
INSERT [dbo].[Articles] ([ArticleID], [UserID], [CategoryID], [Title], [Content], [PublishDate], [ViewCount], [ReadingTime], [Status]) VALUES (2, 11, 1, N'Modern JS Practices', N'Content about JavaScript...', CAST(N'2025-06-02T00:00:00.000' AS DateTime), 200, 7, N'Published')
INSERT [dbo].[Articles] ([ArticleID], [UserID], [CategoryID], [Title], [Content], [PublishDate], [ViewCount], [ReadingTime], [Status]) VALUES (3, 12, 2, N'Database Normalization', N'Normalization explained...', CAST(N'2025-06-03T00:00:00.000' AS DateTime), 180, 6, N'Published')
INSERT [dbo].[Articles] ([ArticleID], [UserID], [CategoryID], [Title], [Content], [PublishDate], [ViewCount], [ReadingTime], [Status]) VALUES (4, 13, 3, N'Intro to AI', N'AI basics...', CAST(N'2025-06-04T00:00:00.000' AS DateTime), 300, 10, N'Published')
INSERT [dbo].[Articles] ([ArticleID], [UserID], [CategoryID], [Title], [Content], [PublishDate], [ViewCount], [ReadingTime], [Status]) VALUES (5, 14, 4, N'Docker Essentials', N'Docker guide...', CAST(N'2025-06-05T00:00:00.000' AS DateTime), 150, 8, N'Published')
INSERT [dbo].[Articles] ([ArticleID], [UserID], [CategoryID], [Title], [Content], [PublishDate], [ViewCount], [ReadingTime], [Status]) VALUES (6, 15, 1, N'NodeJS Architecture', N'NodeJS internals...', CAST(N'2025-06-06T00:00:00.000' AS DateTime), 90, 6, N'Published')
INSERT [dbo].[Articles] ([ArticleID], [UserID], [CategoryID], [Title], [Content], [PublishDate], [ViewCount], [ReadingTime], [Status]) VALUES (7, 16, 3, N'Machine Learning Basics', N'ML intro...', CAST(N'2025-06-07T00:00:00.000' AS DateTime), 400, 12, N'Published')
INSERT [dbo].[Articles] ([ArticleID], [UserID], [CategoryID], [Title], [Content], [PublishDate], [ViewCount], [ReadingTime], [Status]) VALUES (8, 17, 2, N'Advanced SQL Joins', N'JOIN types...', CAST(N'2025-06-08T00:00:00.000' AS DateTime), 250, 7, N'Published')
INSERT [dbo].[Articles] ([ArticleID], [UserID], [CategoryID], [Title], [Content], [PublishDate], [ViewCount], [ReadingTime], [Status]) VALUES (9, 18, 5, N'C# Best Practices', N'C# coding standards...', CAST(N'2025-06-09T00:00:00.000' AS DateTime), 110, 5, N'Published')
INSERT [dbo].[Articles] ([ArticleID], [UserID], [CategoryID], [Title], [Content], [PublishDate], [ViewCount], [ReadingTime], [Status]) VALUES (10, 19, 1, N'Fullstack Development Guide', N'Fullstack overview...', CAST(N'2025-06-10T00:00:00.000' AS DateTime), 500, 15, N'Published')
INSERT [dbo].[Articles] ([ArticleID], [UserID], [CategoryID], [Title], [Content], [PublishDate], [ViewCount], [ReadingTime], [Status]) VALUES (11, 14, 3, N'AI Roadmap', N'Artificial Intelligence Zero to Hero!', CAST(N'2026-06-17T17:30:11.410' AS DateTime), 0, 10, N'Draft')
INSERT [dbo].[Articles] ([ArticleID], [UserID], [CategoryID], [Title], [Content], [PublishDate], [ViewCount], [ReadingTime], [Status]) VALUES (12, 10, 2, N'SQL Server Indexing', N'Sample article for test PublishArticle Stored Procedure!', CAST(N'2026-07-02T10:34:31.570' AS DateTime), 0, 8, N'Published')
INSERT [dbo].[Articles] ([ArticleID], [UserID], [CategoryID], [Title], [Content], [PublishDate], [ViewCount], [ReadingTime], [Status]) VALUES (13, 11, 1, N'Understanding SQL Server Indexes', N'Sample content...', CAST(N'2026-07-02T16:05:55.387' AS DateTime), 0, 7, N'Published')
SET IDENTITY_INSERT [dbo].[Articles] OFF
INSERT [dbo].[ArticleTags] ([ArticleID], [TagID]) VALUES (1, 1)
INSERT [dbo].[ArticleTags] ([ArticleID], [TagID]) VALUES (2, 3)
INSERT [dbo].[ArticleTags] ([ArticleID], [TagID]) VALUES (3, 7)
INSERT [dbo].[ArticleTags] ([ArticleID], [TagID]) VALUES (4, 6)
INSERT [dbo].[ArticleTags] ([ArticleID], [TagID]) VALUES (5, 5)
INSERT [dbo].[ArticleTags] ([ArticleID], [TagID]) VALUES (6, 4)
INSERT [dbo].[ArticleTags] ([ArticleID], [TagID]) VALUES (7, 6)
INSERT [dbo].[ArticleTags] ([ArticleID], [TagID]) VALUES (8, 1)
INSERT [dbo].[ArticleTags] ([ArticleID], [TagID]) VALUES (8, 7)
INSERT [dbo].[ArticleTags] ([ArticleID], [TagID]) VALUES (9, 2)
INSERT [dbo].[ArticleTags] ([ArticleID], [TagID]) VALUES (10, 3)
SET IDENTITY_INSERT [dbo].[Bookmarks] ON 

INSERT [dbo].[Bookmarks] ([BookmarkID], [UserID], [ArticleID], [SavedDate]) VALUES (1, 11, 2, CAST(N'2025-06-16T00:00:00.000' AS DateTime))
INSERT [dbo].[Bookmarks] ([BookmarkID], [UserID], [ArticleID], [SavedDate]) VALUES (2, 10, 3, CAST(N'2025-06-16T00:00:00.000' AS DateTime))
INSERT [dbo].[Bookmarks] ([BookmarkID], [UserID], [ArticleID], [SavedDate]) VALUES (3, 12, 1, CAST(N'2025-06-16T00:00:00.000' AS DateTime))
INSERT [dbo].[Bookmarks] ([BookmarkID], [UserID], [ArticleID], [SavedDate]) VALUES (4, 13, 4, CAST(N'2025-06-16T00:00:00.000' AS DateTime))
INSERT [dbo].[Bookmarks] ([BookmarkID], [UserID], [ArticleID], [SavedDate]) VALUES (5, 14, 5, CAST(N'2025-06-16T00:00:00.000' AS DateTime))
INSERT [dbo].[Bookmarks] ([BookmarkID], [UserID], [ArticleID], [SavedDate]) VALUES (6, 15, 6, CAST(N'2025-06-16T00:00:00.000' AS DateTime))
INSERT [dbo].[Bookmarks] ([BookmarkID], [UserID], [ArticleID], [SavedDate]) VALUES (7, 16, 7, CAST(N'2025-06-16T00:00:00.000' AS DateTime))
INSERT [dbo].[Bookmarks] ([BookmarkID], [UserID], [ArticleID], [SavedDate]) VALUES (8, 17, 8, CAST(N'2025-06-16T00:00:00.000' AS DateTime))
INSERT [dbo].[Bookmarks] ([BookmarkID], [UserID], [ArticleID], [SavedDate]) VALUES (9, 18, 9, CAST(N'2025-06-16T00:00:00.000' AS DateTime))
INSERT [dbo].[Bookmarks] ([BookmarkID], [UserID], [ArticleID], [SavedDate]) VALUES (10, 19, 10, CAST(N'2025-06-16T00:00:00.000' AS DateTime))
SET IDENTITY_INSERT [dbo].[Bookmarks] OFF
SET IDENTITY_INSERT [dbo].[Categories] ON 

INSERT [dbo].[Categories] ([CategoryID], [CategoryName], [Description]) VALUES (1, N'Web Development', N'Frontend and Backend development topics')
INSERT [dbo].[Categories] ([CategoryID], [CategoryName], [Description]) VALUES (2, N'Database', N'SQL, NoSQL and database design')
INSERT [dbo].[Categories] ([CategoryID], [CategoryName], [Description]) VALUES (3, N'AI & ML', N'Artificial Intelligence and Machine Learning')
INSERT [dbo].[Categories] ([CategoryID], [CategoryName], [Description]) VALUES (4, N'DevOps', N'CI/CD, Docker, Kubernetes')
INSERT [dbo].[Categories] ([CategoryID], [CategoryName], [Description]) VALUES (5, N'Programming Languages', N'Languages like C#, Python, JS')
SET IDENTITY_INSERT [dbo].[Categories] OFF
SET IDENTITY_INSERT [dbo].[Comments] ON 

INSERT [dbo].[Comments] ([CommentID], [ArticleID], [UserID], [CommentText], [CommentDate]) VALUES (2, 1, 12, N'Great SQL tips!', CAST(N'2025-06-11T00:00:00.000' AS DateTime))
INSERT [dbo].[Comments] ([CommentID], [ArticleID], [UserID], [CommentText], [CommentDate]) VALUES (3, 1, 13, N'Very useful', CAST(N'2025-06-11T00:00:00.000' AS DateTime))
INSERT [dbo].[Comments] ([CommentID], [ArticleID], [UserID], [CommentText], [CommentDate]) VALUES (4, 2, 10, N'Nice JS article', CAST(N'2025-06-12T00:00:00.000' AS DateTime))
INSERT [dbo].[Comments] ([CommentID], [ArticleID], [UserID], [CommentText], [CommentDate]) VALUES (5, 3, 14, N'Good explanation', CAST(N'2025-06-12T00:00:00.000' AS DateTime))
INSERT [dbo].[Comments] ([CommentID], [ArticleID], [UserID], [CommentText], [CommentDate]) VALUES (6, 4, 15, N'AI is fascinating', CAST(N'2025-06-13T00:00:00.000' AS DateTime))
INSERT [dbo].[Comments] ([CommentID], [ArticleID], [UserID], [CommentText], [CommentDate]) VALUES (7, 5, 16, N'Helpful Docker guide', CAST(N'2025-06-13T00:00:00.000' AS DateTime))
INSERT [dbo].[Comments] ([CommentID], [ArticleID], [UserID], [CommentText], [CommentDate]) VALUES (8, 6, 17, N'NodeJS architecture well explained', CAST(N'2025-06-14T00:00:00.000' AS DateTime))
INSERT [dbo].[Comments] ([CommentID], [ArticleID], [UserID], [CommentText], [CommentDate]) VALUES (9, 7, 18, N'ML content is great', CAST(N'2025-06-14T00:00:00.000' AS DateTime))
INSERT [dbo].[Comments] ([CommentID], [ArticleID], [UserID], [CommentText], [CommentDate]) VALUES (10, 8, 19, N'Advanced JOINs explained well', CAST(N'2025-06-15T00:00:00.000' AS DateTime))
INSERT [dbo].[Comments] ([CommentID], [ArticleID], [UserID], [CommentText], [CommentDate]) VALUES (11, 10, 11, N'Excellent overview!', CAST(N'2025-06-15T00:00:00.000' AS DateTime))
INSERT [dbo].[Comments] ([CommentID], [ArticleID], [UserID], [CommentText], [CommentDate]) VALUES (12, 1, 11, N'This is a very useful article!', CAST(N'2026-07-02T11:32:03.640' AS DateTime))
INSERT [dbo].[Comments] ([CommentID], [ArticleID], [UserID], [CommentText], [CommentDate]) VALUES (13, 1, 13, N'Excellent article. Thanks for sharing!', CAST(N'2026-07-02T16:15:46.840' AS DateTime))
INSERT [dbo].[Comments] ([CommentID], [ArticleID], [UserID], [CommentText], [CommentDate]) VALUES (14, 1, 12, N'Very helpful article!', CAST(N'2026-07-02T16:26:18.017' AS DateTime))
SET IDENTITY_INSERT [dbo].[Comments] OFF
SET IDENTITY_INSERT [dbo].[Follows] ON 

INSERT [dbo].[Follows] ([FollowID], [FollowerUserID], [FollowingUserID], [FollowDate]) VALUES (1, 10, 12, CAST(N'2025-06-01T00:00:00.000' AS DateTime))
INSERT [dbo].[Follows] ([FollowID], [FollowerUserID], [FollowingUserID], [FollowDate]) VALUES (2, 10, 13, CAST(N'2025-06-01T00:00:00.000' AS DateTime))
INSERT [dbo].[Follows] ([FollowID], [FollowerUserID], [FollowingUserID], [FollowDate]) VALUES (3, 12, 10, CAST(N'2025-06-02T00:00:00.000' AS DateTime))
INSERT [dbo].[Follows] ([FollowID], [FollowerUserID], [FollowingUserID], [FollowDate]) VALUES (4, 13, 14, CAST(N'2025-06-02T00:00:00.000' AS DateTime))
INSERT [dbo].[Follows] ([FollowID], [FollowerUserID], [FollowingUserID], [FollowDate]) VALUES (5, 14, 15, CAST(N'2025-06-03T00:00:00.000' AS DateTime))
INSERT [dbo].[Follows] ([FollowID], [FollowerUserID], [FollowingUserID], [FollowDate]) VALUES (6, 15, 16, CAST(N'2025-06-03T00:00:00.000' AS DateTime))
INSERT [dbo].[Follows] ([FollowID], [FollowerUserID], [FollowingUserID], [FollowDate]) VALUES (7, 16, 17, CAST(N'2025-06-04T00:00:00.000' AS DateTime))
INSERT [dbo].[Follows] ([FollowID], [FollowerUserID], [FollowingUserID], [FollowDate]) VALUES (8, 17, 18, CAST(N'2025-06-04T00:00:00.000' AS DateTime))
INSERT [dbo].[Follows] ([FollowID], [FollowerUserID], [FollowingUserID], [FollowDate]) VALUES (9, 18, 19, CAST(N'2025-06-05T00:00:00.000' AS DateTime))
INSERT [dbo].[Follows] ([FollowID], [FollowerUserID], [FollowingUserID], [FollowDate]) VALUES (10, 19, 11, CAST(N'2025-06-05T00:00:00.000' AS DateTime))
SET IDENTITY_INSERT [dbo].[Follows] OFF
SET IDENTITY_INSERT [dbo].[Reactions] ON 

INSERT [dbo].[Reactions] ([ReactionID], [UserID], [ArticleID], [ReactionType], [ReactionDate]) VALUES (1, 12, 1, N'Like', CAST(N'2025-06-11T00:00:00.000' AS DateTime))
INSERT [dbo].[Reactions] ([ReactionID], [UserID], [ArticleID], [ReactionType], [ReactionDate]) VALUES (2, 13, 1, N'Love', CAST(N'2025-06-11T00:00:00.000' AS DateTime))
INSERT [dbo].[Reactions] ([ReactionID], [UserID], [ArticleID], [ReactionType], [ReactionDate]) VALUES (3, 11, 2, N'Like', CAST(N'2025-06-12T00:00:00.000' AS DateTime))
INSERT [dbo].[Reactions] ([ReactionID], [UserID], [ArticleID], [ReactionType], [ReactionDate]) VALUES (4, 14, 3, N'Idea', CAST(N'2025-06-12T00:00:00.000' AS DateTime))
INSERT [dbo].[Reactions] ([ReactionID], [UserID], [ArticleID], [ReactionType], [ReactionDate]) VALUES (5, 15, 4, N'Love', CAST(N'2025-06-13T00:00:00.000' AS DateTime))
INSERT [dbo].[Reactions] ([ReactionID], [UserID], [ArticleID], [ReactionType], [ReactionDate]) VALUES (6, 16, 5, N'Like', CAST(N'2025-06-13T00:00:00.000' AS DateTime))
INSERT [dbo].[Reactions] ([ReactionID], [UserID], [ArticleID], [ReactionType], [ReactionDate]) VALUES (7, 17, 6, N'Like', CAST(N'2025-06-14T00:00:00.000' AS DateTime))
INSERT [dbo].[Reactions] ([ReactionID], [UserID], [ArticleID], [ReactionType], [ReactionDate]) VALUES (8, 18, 7, N'Love', CAST(N'2025-06-14T00:00:00.000' AS DateTime))
INSERT [dbo].[Reactions] ([ReactionID], [UserID], [ArticleID], [ReactionType], [ReactionDate]) VALUES (9, 19, 8, N'Idea', CAST(N'2025-06-15T00:00:00.000' AS DateTime))
INSERT [dbo].[Reactions] ([ReactionID], [UserID], [ArticleID], [ReactionType], [ReactionDate]) VALUES (10, 10, 10, N'Love', CAST(N'2025-06-15T00:00:00.000' AS DateTime))
INSERT [dbo].[Reactions] ([ReactionID], [UserID], [ArticleID], [ReactionType], [ReactionDate]) VALUES (11, 13, 1, N'Love', CAST(N'2026-07-02T16:28:47.103' AS DateTime))
SET IDENTITY_INSERT [dbo].[Reactions] OFF
SET IDENTITY_INSERT [dbo].[Tags] ON 

INSERT [dbo].[Tags] ([TagID], [TagName], [Description]) VALUES (1, N'SQL', N'SQL related topics')
INSERT [dbo].[Tags] ([TagID], [TagName], [Description]) VALUES (2, N'CSharp', N'C# programming')
INSERT [dbo].[Tags] ([TagID], [TagName], [Description]) VALUES (3, N'JavaScript', N'Frontend scripting')
INSERT [dbo].[Tags] ([TagID], [TagName], [Description]) VALUES (4, N'NodeJS', N'Backend JavaScript')
INSERT [dbo].[Tags] ([TagID], [TagName], [Description]) VALUES (5, N'Docker', N'Containerization')
INSERT [dbo].[Tags] ([TagID], [TagName], [Description]) VALUES (6, N'AI', N'Artificial Intelligence')
INSERT [dbo].[Tags] ([TagID], [TagName], [Description]) VALUES (7, N'DatabaseDesign', N'Database modeling')
SET IDENTITY_INSERT [dbo].[Tags] OFF
SET IDENTITY_INSERT [dbo].[UserProfiles] ON 

INSERT [dbo].[UserProfiles] ([ProfileID], [UserID], [FullName], [Bio], [Country], [WebsiteURL], [ProfileImageURL]) VALUES (1, 10, N'Ali Mohammadi', N'Backend developer focused on SQL and .NET systems.', N'Iran', N'https://ali.dev', N'https://img.com/u1.png')
INSERT [dbo].[UserProfiles] ([ProfileID], [UserID], [FullName], [Bio], [Country], [WebsiteURL], [ProfileImageURL]) VALUES (2, 11, N'Sara Karimi', N'Frontend developer passionate about JavaScript and UI design.', N'Iran', N'https://sara.dev', N'https://img.com/u2.png')
INSERT [dbo].[UserProfiles] ([ProfileID], [UserID], [FullName], [Bio], [Country], [WebsiteURL], [ProfileImageURL]) VALUES (3, 12, N'Reza Hosseini', N'Database engineer and SQL optimization enthusiast.', N'Iran', N'https://reza.dev', N'https://img.com/u3.png')
INSERT [dbo].[UserProfiles] ([ProfileID], [UserID], [FullName], [Bio], [Country], [WebsiteURL], [ProfileImageURL]) VALUES (4, 13, N'Parsa Ahmadi', N'Backend developer working with Node.js and APIs.', N'Iran', N'https://parsa.dev', N'https://img.com/u4.png')
INSERT [dbo].[UserProfiles] ([ProfileID], [UserID], [FullName], [Bio], [Country], [WebsiteURL], [ProfileImageURL]) VALUES (5, 14, N'John Doe', N'Fullstack developer exploring modern web technologies.', N'USA', N'https://john.dev', N'https://img.com/u5.png')
INSERT [dbo].[UserProfiles] ([ProfileID], [UserID], [FullName], [Bio], [Country], [WebsiteURL], [ProfileImageURL]) VALUES (6, 15, N'Emma Wilson', N'Software engineer interested in scalable systems.', N'UK', N'https://emma.dev', N'https://img.com/u6.png')
INSERT [dbo].[UserProfiles] ([ProfileID], [UserID], [FullName], [Bio], [Country], [WebsiteURL], [ProfileImageURL]) VALUES (7, 16, N'Armin Nouri', N'AI/ML researcher and Python developer.', N'Iran', N'https://armin.dev', N'https://img.com/u7.png')
INSERT [dbo].[UserProfiles] ([ProfileID], [UserID], [FullName], [Bio], [Country], [WebsiteURL], [ProfileImageURL]) VALUES (8, 17, N'Nina Brown', N'Cloud engineer working with AWS and DevOps tools.', N'Germany', N'https://nina.dev', N'https://img.com/u8.png')
INSERT [dbo].[UserProfiles] ([ProfileID], [UserID], [FullName], [Bio], [Country], [WebsiteURL], [ProfileImageURL]) VALUES (9, 18, N'Hossein Rahimi', N'Network engineer and backend systems designer.', N'Iran', N'https://hossein.dev', N'https://img.com/u9.png')
INSERT [dbo].[UserProfiles] ([ProfileID], [UserID], [FullName], [Bio], [Country], [WebsiteURL], [ProfileImageURL]) VALUES (10, 19, N'Tina Safari', N'Fullstack developer specializing in React and .NET.', N'Iran', N'https://tina.dev', N'https://img.com/u10.png')
SET IDENTITY_INSERT [dbo].[UserProfiles] OFF
SET IDENTITY_INSERT [dbo].[Users] ON 

INSERT [dbo].[Users] ([UserID], [Username], [Email], [PasswordHash], [JoinDate], [Status]) VALUES (10, N'ali_dev', N'ali@example.com', N'hash1', CAST(N'2025-01-10T00:00:00.000' AS DateTime), N'Active')
INSERT [dbo].[Users] ([UserID], [Username], [Email], [PasswordHash], [JoinDate], [Status]) VALUES (11, N'sara_js', N'sara@example.com', N'hash2', CAST(N'2025-02-12T00:00:00.000' AS DateTime), N'Active')
INSERT [dbo].[Users] ([UserID], [Username], [Email], [PasswordHash], [JoinDate], [Status]) VALUES (12, N'reza_sql', N'reza@example.com', N'hash3', CAST(N'2025-03-05T00:00:00.000' AS DateTime), N'Active')
INSERT [dbo].[Users] ([UserID], [Username], [Email], [PasswordHash], [JoinDate], [Status]) VALUES (13, N'parsa_backend', N'parsa@example.com', N'hash4', CAST(N'2025-03-20T00:00:00.000' AS DateTime), N'Deactive')
INSERT [dbo].[Users] ([UserID], [Username], [Email], [PasswordHash], [JoinDate], [Status]) VALUES (14, N'john_doe', N'john@example.com', N'hash5', CAST(N'2025-04-01T00:00:00.000' AS DateTime), N'Active')
INSERT [dbo].[Users] ([UserID], [Username], [Email], [PasswordHash], [JoinDate], [Status]) VALUES (15, N'emma_dev', N'emma@example.com', N'hash6', CAST(N'2025-04-15T00:00:00.000' AS DateTime), N'Active')
INSERT [dbo].[Users] ([UserID], [Username], [Email], [PasswordHash], [JoinDate], [Status]) VALUES (16, N'armin_ai', N'armin@example.com', N'hash7', CAST(N'2025-05-10T00:00:00.000' AS DateTime), N'Active')
INSERT [dbo].[Users] ([UserID], [Username], [Email], [PasswordHash], [JoinDate], [Status]) VALUES (17, N'nina_cloud', N'nina@example.com', N'hash8', CAST(N'2025-05-22T00:00:00.000' AS DateTime), N'Active')
INSERT [dbo].[Users] ([UserID], [Username], [Email], [PasswordHash], [JoinDate], [Status]) VALUES (18, N'hossein_net', N'hossein@example.com', N'hash9', CAST(N'2025-06-09T00:00:00.000' AS DateTime), N'Active')
INSERT [dbo].[Users] ([UserID], [Username], [Email], [PasswordHash], [JoinDate], [Status]) VALUES (19, N'tina_fullstack', N'tina@example.com', N'hash10', CAST(N'2025-06-12T00:00:00.000' AS DateTime), N'Active')
SET IDENTITY_INSERT [dbo].[Users] OFF
/****** Object:  Index [IX_Articles_CategoryID]    Script Date: 7/3/2026 3:32:55 PM ******/
CREATE NONCLUSTERED INDEX [IX_Articles_CategoryID] ON [dbo].[Articles]
(
	[CategoryID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_Articles_Status_PublishDate]    Script Date: 7/3/2026 3:32:55 PM ******/
CREATE NONCLUSTERED INDEX [IX_Articles_Status_PublishDate] ON [dbo].[Articles]
(
	[Status] ASC,
	[PublishDate] DESC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_Articles_UserID]    Script Date: 7/3/2026 3:32:55 PM ******/
CREATE NONCLUSTERED INDEX [IX_Articles_UserID] ON [dbo].[Articles]
(
	[UserID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_Bookmarks_UserID]    Script Date: 7/3/2026 3:32:55 PM ******/
CREATE NONCLUSTERED INDEX [IX_Bookmarks_UserID] ON [dbo].[Bookmarks]
(
	[UserID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [UQ_Categories_CategoryName]    Script Date: 7/3/2026 3:32:55 PM ******/
ALTER TABLE [dbo].[Categories] ADD  CONSTRAINT [UQ_Categories_CategoryName] UNIQUE NONCLUSTERED 
(
	[CategoryName] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_Comments_ArticleID]    Script Date: 7/3/2026 3:32:55 PM ******/
CREATE NONCLUSTERED INDEX [IX_Comments_ArticleID] ON [dbo].[Comments]
(
	[ArticleID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [UQ_Follows]    Script Date: 7/3/2026 3:32:55 PM ******/
ALTER TABLE [dbo].[Follows] ADD  CONSTRAINT [UQ_Follows] UNIQUE NONCLUSTERED 
(
	[FollowerUserID] ASC,
	[FollowingUserID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_Follows_FollowingUserID]    Script Date: 7/3/2026 3:32:55 PM ******/
CREATE NONCLUSTERED INDEX [IX_Follows_FollowingUserID] ON [dbo].[Follows]
(
	[FollowingUserID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_Reactions_ArticleID]    Script Date: 7/3/2026 3:32:55 PM ******/
CREATE NONCLUSTERED INDEX [IX_Reactions_ArticleID] ON [dbo].[Reactions]
(
	[ArticleID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [UQ_Tags_TagName]    Script Date: 7/3/2026 3:32:55 PM ******/
ALTER TABLE [dbo].[Tags] ADD  CONSTRAINT [UQ_Tags_TagName] UNIQUE NONCLUSTERED 
(
	[TagName] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [UQ_UserProfiles_UserID]    Script Date: 7/3/2026 3:32:55 PM ******/
ALTER TABLE [dbo].[UserProfiles] ADD  CONSTRAINT [UQ_UserProfiles_UserID] UNIQUE NONCLUSTERED 
(
	[UserID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [UQ_Users_Email]    Script Date: 7/3/2026 3:32:55 PM ******/
ALTER TABLE [dbo].[Users] ADD  CONSTRAINT [UQ_Users_Email] UNIQUE NONCLUSTERED 
(
	[Email] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [UQ_Users_Username]    Script Date: 7/3/2026 3:32:55 PM ******/
ALTER TABLE [dbo].[Users] ADD  CONSTRAINT [UQ_Users_Username] UNIQUE NONCLUSTERED 
(
	[Username] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
ALTER TABLE [dbo].[Articles] ADD  CONSTRAINT [DF_Articles_PublishDate]  DEFAULT (getdate()) FOR [PublishDate]
GO
ALTER TABLE [dbo].[Articles] ADD  CONSTRAINT [DF_Articles_ViewCount]  DEFAULT ((0)) FOR [ViewCount]
GO
ALTER TABLE [dbo].[Articles] ADD  CONSTRAINT [DF_Articles_ReadingTime]  DEFAULT ((1)) FOR [ReadingTime]
GO
ALTER TABLE [dbo].[Articles] ADD  CONSTRAINT [DF_Articles_Status]  DEFAULT ('Published') FOR [Status]
GO
ALTER TABLE [dbo].[Bookmarks] ADD  CONSTRAINT [DF_Bookmarks_SavedDate]  DEFAULT (getdate()) FOR [SavedDate]
GO
ALTER TABLE [dbo].[Comments] ADD  CONSTRAINT [DF_Comments_CommentDate]  DEFAULT (getdate()) FOR [CommentDate]
GO
ALTER TABLE [dbo].[Follows] ADD  CONSTRAINT [DF_Follows_FollowDate]  DEFAULT (getdate()) FOR [FollowDate]
GO
ALTER TABLE [dbo].[Reactions] ADD  CONSTRAINT [DF_Reactions_ReactionType]  DEFAULT ('Like') FOR [ReactionType]
GO
ALTER TABLE [dbo].[Reactions] ADD  CONSTRAINT [DF_Reactions_ReactionDate]  DEFAULT (getdate()) FOR [ReactionDate]
GO
ALTER TABLE [dbo].[Users] ADD  CONSTRAINT [DF_Users_JoinDate]  DEFAULT (getdate()) FOR [JoinDate]
GO
ALTER TABLE [dbo].[Users] ADD  CONSTRAINT [DF_Users_Status]  DEFAULT ('Active') FOR [Status]
GO
ALTER TABLE [dbo].[Articles]  WITH CHECK ADD  CONSTRAINT [FK_Articles_Categories] FOREIGN KEY([CategoryID])
REFERENCES [dbo].[Categories] ([CategoryID])
GO
ALTER TABLE [dbo].[Articles] CHECK CONSTRAINT [FK_Articles_Categories]
GO
ALTER TABLE [dbo].[Articles]  WITH CHECK ADD  CONSTRAINT [FK_Articles_Users] FOREIGN KEY([UserID])
REFERENCES [dbo].[Users] ([UserID])
GO
ALTER TABLE [dbo].[Articles] CHECK CONSTRAINT [FK_Articles_Users]
GO
ALTER TABLE [dbo].[ArticleTags]  WITH CHECK ADD  CONSTRAINT [FK_ArticleTags_Articles] FOREIGN KEY([ArticleID])
REFERENCES [dbo].[Articles] ([ArticleID])
GO
ALTER TABLE [dbo].[ArticleTags] CHECK CONSTRAINT [FK_ArticleTags_Articles]
GO
ALTER TABLE [dbo].[ArticleTags]  WITH CHECK ADD  CONSTRAINT [FK_ArticleTags_Tags] FOREIGN KEY([TagID])
REFERENCES [dbo].[Tags] ([TagID])
GO
ALTER TABLE [dbo].[ArticleTags] CHECK CONSTRAINT [FK_ArticleTags_Tags]
GO
ALTER TABLE [dbo].[Bookmarks]  WITH CHECK ADD  CONSTRAINT [FK_Bookmarks_Articles] FOREIGN KEY([ArticleID])
REFERENCES [dbo].[Articles] ([ArticleID])
GO
ALTER TABLE [dbo].[Bookmarks] CHECK CONSTRAINT [FK_Bookmarks_Articles]
GO
ALTER TABLE [dbo].[Bookmarks]  WITH CHECK ADD  CONSTRAINT [FK_Bookmarks_Users] FOREIGN KEY([UserID])
REFERENCES [dbo].[Users] ([UserID])
GO
ALTER TABLE [dbo].[Bookmarks] CHECK CONSTRAINT [FK_Bookmarks_Users]
GO
ALTER TABLE [dbo].[Comments]  WITH CHECK ADD  CONSTRAINT [FK_Comments_Articles] FOREIGN KEY([ArticleID])
REFERENCES [dbo].[Articles] ([ArticleID])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Comments] CHECK CONSTRAINT [FK_Comments_Articles]
GO
ALTER TABLE [dbo].[Comments]  WITH CHECK ADD  CONSTRAINT [FK_Comments_Users] FOREIGN KEY([UserID])
REFERENCES [dbo].[Users] ([UserID])
GO
ALTER TABLE [dbo].[Comments] CHECK CONSTRAINT [FK_Comments_Users]
GO
ALTER TABLE [dbo].[Follows]  WITH CHECK ADD  CONSTRAINT [FK_Follows_Users_FollowerUserID] FOREIGN KEY([FollowerUserID])
REFERENCES [dbo].[Users] ([UserID])
GO
ALTER TABLE [dbo].[Follows] CHECK CONSTRAINT [FK_Follows_Users_FollowerUserID]
GO
ALTER TABLE [dbo].[Follows]  WITH CHECK ADD  CONSTRAINT [FK_Follows_Users_FollowingUserID] FOREIGN KEY([FollowingUserID])
REFERENCES [dbo].[Users] ([UserID])
GO
ALTER TABLE [dbo].[Follows] CHECK CONSTRAINT [FK_Follows_Users_FollowingUserID]
GO
ALTER TABLE [dbo].[Reactions]  WITH CHECK ADD  CONSTRAINT [FK_Reactions_Articles] FOREIGN KEY([ArticleID])
REFERENCES [dbo].[Articles] ([ArticleID])
GO
ALTER TABLE [dbo].[Reactions] CHECK CONSTRAINT [FK_Reactions_Articles]
GO
ALTER TABLE [dbo].[Reactions]  WITH CHECK ADD  CONSTRAINT [FK_Reactions_Users] FOREIGN KEY([UserID])
REFERENCES [dbo].[Users] ([UserID])
GO
ALTER TABLE [dbo].[Reactions] CHECK CONSTRAINT [FK_Reactions_Users]
GO
ALTER TABLE [dbo].[UserProfiles]  WITH CHECK ADD  CONSTRAINT [FK_UserProfiles_UserID] FOREIGN KEY([UserID])
REFERENCES [dbo].[Users] ([UserID])
GO
ALTER TABLE [dbo].[UserProfiles] CHECK CONSTRAINT [FK_UserProfiles_UserID]
GO
ALTER TABLE [dbo].[Articles]  WITH CHECK ADD  CONSTRAINT [CK_Articles_ReadingTime] CHECK  (([ReadingTime]>(0)))
GO
ALTER TABLE [dbo].[Articles] CHECK CONSTRAINT [CK_Articles_ReadingTime]
GO
ALTER TABLE [dbo].[Articles]  WITH CHECK ADD  CONSTRAINT [CK_Articles_Status] CHECK  (([Status]='Draft' OR [Status]='Published'))
GO
ALTER TABLE [dbo].[Articles] CHECK CONSTRAINT [CK_Articles_Status]
GO
ALTER TABLE [dbo].[Articles]  WITH CHECK ADD  CONSTRAINT [CK_Articles_ViewCount] CHECK  (([ViewCount]>=(0)))
GO
ALTER TABLE [dbo].[Articles] CHECK CONSTRAINT [CK_Articles_ViewCount]
GO
ALTER TABLE [dbo].[Follows]  WITH CHECK ADD  CONSTRAINT [CK_Follows_FollowerUserID] CHECK  (([FollowerUserID]<>[FollowingUserID]))
GO
ALTER TABLE [dbo].[Follows] CHECK CONSTRAINT [CK_Follows_FollowerUserID]
GO
ALTER TABLE [dbo].[Reactions]  WITH CHECK ADD  CONSTRAINT [CK_Reactions_ReactionType] CHECK  (([ReactionType]='Idea' OR [ReactionType]='Love' OR [ReactionType]='Like'))
GO
ALTER TABLE [dbo].[Reactions] CHECK CONSTRAINT [CK_Reactions_ReactionType]
GO
ALTER TABLE [dbo].[Users]  WITH CHECK ADD  CONSTRAINT [CK_Users_Email] CHECK  (([Email] like '%@%.%'))
GO
ALTER TABLE [dbo].[Users] CHECK CONSTRAINT [CK_Users_Email]
GO
ALTER TABLE [dbo].[Users]  WITH CHECK ADD  CONSTRAINT [CK_Users_Status] CHECK  (([Status]='Deactive' OR [Status]='Active'))
GO
ALTER TABLE [dbo].[Users] CHECK CONSTRAINT [CK_Users_Status]
GO
ALTER TABLE [dbo].[Users]  WITH CHECK ADD  CONSTRAINT [CK_Users_Username] CHECK  ((len([Username])>=(3)))
GO
ALTER TABLE [dbo].[Users] CHECK CONSTRAINT [CK_Users_Username]
GO
/****** Object:  StoredProcedure [dbo].[AddComment]    Script Date: 7/3/2026 3:32:55 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE   PROCEDURE [dbo].[AddComment]
(
    @ArticleID INT,
    @UserID INT,
    @CommentText NVARCHAR(1000)
)
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        -- Validate Article --
        IF NOT EXISTS
        (
            SELECT 1
            FROM Articles
            WHERE ArticleID = @ArticleID
        )
        BEGIN
            THROW 50020, 'Article not found.', 1;
        END

        -- Validate User --
        IF NOT EXISTS
        (
            SELECT 1
            FROM Users
            WHERE UserID = @UserID
        )
        BEGIN
            THROW 50021, 'User not found.', 1;
        END

        -- Validate Comment --
        IF LEN(LTRIM(RTRIM(@CommentText))) = 0
        BEGIN
            THROW 50022, 'Comment cannot be empty.', 1;
        END

        -- Insert Comment --
        INSERT INTO Comments
        (
            ArticleID,
            UserID,
            CommentText
        )
        VALUES
        (
            @ArticleID,
            @UserID,
            @CommentText
        );

        COMMIT TRANSACTION;

        SELECT
            'Success' AS Status,
            SCOPE_IDENTITY() AS CommentID,
            'Comment added successfully.' AS Message;
    END TRY

    BEGIN CATCH
        IF XACT_STATE() <> 0
            ROLLBACK TRANSACTION;

        SELECT
            'Error' AS Status,
            ERROR_NUMBER() AS ErrorNumber,
            ERROR_MESSAGE() AS ErrorMessage,
            ERROR_LINE() AS ErrorLine;
    END CATCH
END;
GO
/****** Object:  StoredProcedure [dbo].[GetArticleByCategory]    Script Date: 7/3/2026 3:32:55 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetArticleByCategory]
(
	@CategoryName VARCHAR(100)
)
AS
BEGIN

SET NOCOUNT ON;

SELECT
	A.Title,
    U.Username,
    A.PublishDate,
    A.ViewCount
FROM Articles A
INNER JOIN Categories C
	ON A.CategoryID = C.CategoryID
INNER JOIN Users U
	ON A.UserID = U.UserID
WHERE C.CategoryName = @CategoryName
ORDER BY A.PublishDate DESC;

END;
GO
/****** Object:  StoredProcedure [dbo].[GetArticlesByCategory]    Script Date: 7/3/2026 3:32:55 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE   PROCEDURE [dbo].[GetArticlesByCategory]
(
    @CategoryName VARCHAR(100)
)
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        IF NOT EXISTS
        (
            SELECT 1
            FROM Categories
            WHERE CategoryName = @CategoryName
        )
        BEGIN
            THROW 50010, 'Category not found.', 1;
        END

        SELECT
            A.ArticleID,
            A.Title,
            U.Username AS Author,
            A.PublishDate,
            A.ViewCount,
            A.ReadingTime
        FROM Articles A
        INNER JOIN Categories C
            ON A.CategoryID = C.CategoryID
        INNER JOIN Users U
            ON A.UserID = U.UserID
        WHERE
            C.CategoryName = @CategoryName
            AND A.Status = 'Published'
        ORDER BY
            A.PublishDate DESC;
    END TRY

    BEGIN CATCH
        SELECT
            ERROR_NUMBER() AS ErrorNumber,
            ERROR_MESSAGE() AS ErrorMessage;
    END CATCH
END;
GO
/****** Object:  StoredProcedure [dbo].[PublishArticle]    Script Date: 7/3/2026 3:32:55 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE   PROCEDURE [dbo].[PublishArticle]
(
    @UserID INT,
    @CategoryID INT,
    @Title NVARCHAR(200),
    @Content NVARCHAR(MAX),
    @ReadingTime INT
)
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY

        BEGIN TRANSACTION;

        -------------------------------------------------
        -- Validate User
        -------------------------------------------------
        IF NOT EXISTS
        (
            SELECT 1
            FROM Users
            WHERE UserID = @UserID
        )
        BEGIN
            THROW 50001, 'The specified user does not exist.', 1;
        END

        -------------------------------------------------
        -- Validate Category
        -------------------------------------------------
        IF NOT EXISTS
        (
            SELECT 1
            FROM Categories
            WHERE CategoryID = @CategoryID
        )
        BEGIN
            THROW 50002, 'The specified category does not exist.', 1;
        END

        -------------------------------------------------
        -- Validate Reading Time
        -------------------------------------------------
        IF @ReadingTime <= 0
        BEGIN
            THROW 50003, 'Reading time must be greater than zero.', 1;
        END

        -------------------------------------------------
        -- Insert Article
        -------------------------------------------------
        INSERT INTO Articles
        (
            UserID,
            CategoryID,
            Title,
            Content,
            ReadingTime
        )
        VALUES
        (
            @UserID,
            @CategoryID,
            @Title,
            @Content,
            @ReadingTime
        );

        COMMIT TRANSACTION;

        SELECT
            'Success' AS Status,
            SCOPE_IDENTITY() AS ArticleID,
            'Article published successfully.' AS Message;

    END TRY

    BEGIN CATCH

        IF XACT_STATE() <> 0
            ROLLBACK TRANSACTION;

        SELECT
            'Error' AS Status,
            ERROR_NUMBER() AS ErrorNumber,
            ERROR_MESSAGE() AS ErrorMessage,
            ERROR_LINE() AS ErrorLine;

    END CATCH
END;
GO
/****** Object:  StoredProcedure [dbo].[SearchArticles]    Script Date: 7/3/2026 3:32:55 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE   PROCEDURE [dbo].[SearchArticles]
(
    @ColumnName SYSNAME,
    @SearchValue NVARCHAR(200)
)
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        -- Validate Column Name --
        IF @ColumnName NOT IN
        (
            'Title',
            'Status',
            'Content'
        )
        BEGIN
            THROW 51000,
            'Invalid column name. Allowed columns: Title, Status, Content.',
            1;
        END;

        DECLARE @SQL NVARCHAR(MAX);
        DECLARE @Keyword NVARCHAR(202);

        SET @Keyword = '%' + @SearchValue + '%';

        -- Build Dynamic SQL --
        SET @SQL = N'
            SELECT
                ArticleID,
                Title,
                Status,
                PublishDate,
                ViewCount,
                ReadingTime
            FROM Articles
            WHERE ' + QUOTENAME(@ColumnName) + N' LIKE @Keyword
            ORDER BY PublishDate DESC;
        ';

        -- Execute Dynamic SQL --
        EXEC sp_executesql
            @SQL,
            N'@Keyword NVARCHAR(202)',
            @Keyword = @Keyword;
    END TRY

    BEGIN CATCH
        SELECT
            ERROR_NUMBER() AS ErrorNumber,
            ERROR_MESSAGE() AS ErrorMessage,
            ERROR_LINE() AS ErrorLine;

    END CATCH
END;
GO
USE [master]
GO
ALTER DATABASE [CodeSphere] SET  READ_WRITE 
GO
