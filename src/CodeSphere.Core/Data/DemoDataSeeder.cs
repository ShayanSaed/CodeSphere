using CodeSphere.Core.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CodeSphere.Core.Data;

/// <summary>
/// Generates a large, realistic sample dataset (200+ rows in every table
/// defined by the original CodeSphere.sql schema) so the platform's
/// capabilities — search, filtering, pagination, trending/engagement
/// ranking, reporting, and the social features — are actually visible with
/// a meaningful amount of data, rather than the handful of rows a
/// hand-written seed list could realistically provide.
///
/// Content is produced from curated word/sentence banks combined
/// pseudo-randomly (fixed seed, so the generated dataset is reproducible)
/// rather than Lorem Ipsum, so article bodies read as coherent, on-topic
/// technical writing instead of placeholder text.
///
/// This runs exactly once, only when the database has no categories yet
/// (see <see cref="DbSeeder"/>).
/// </summary>
public static class DemoDataSeeder
{
    private const string DemoPassword = "Reader@12345";

    public static async Task SeedAsync(CodeSphereDbContext context, UserManager<ApplicationUser> userManager)
    {
        var rng = new Random(20260710); // fixed seed: reproducible sample data

        var categories = await SeedCategoriesAsync(context);
        var tags = await SeedTagsAsync(context);
        var users = await SeedUsersAsync(context, userManager, rng);
        var articles = await SeedArticlesAsync(context, rng, users, categories);

        await SeedEngagementDataAsync(context, rng, users, articles, tags);
    }

    // ==================================================================
    // Categories — 20 domains x 11 subtopics = 220 rows
    // ==================================================================
    private static async Task<List<Category>> SeedCategoriesAsync(CodeSphereDbContext context)
    {
        string[] domains =
        {
            "Web Development", "Mobile Development", "Cloud Computing", "Database Engineering",
            "DevOps", "Artificial Intelligence", "Machine Learning", "Cybersecurity",
            "Software Architecture", "Data Engineering", "Game Development", "Embedded Systems",
            "Quality Assurance", "UI/UX Engineering", "Programming Languages", "Distributed Systems",
            "Blockchain", "Networking", "Site Reliability Engineering", "Developer Tools"
        };

        string[] subtopics =
        {
            "Fundamentals", "Best Practices", "Performance Optimization", "Design Patterns",
            "Case Studies", "Advanced Techniques", "Tooling", "Testing Strategies",
            "Security Considerations", "Scalability", "Migration Guides"
        };

        var wantedNames = new List<string>();
        var descriptions = new Dictionary<string, string>();
        foreach (var domain in domains)
        {
            foreach (var sub in subtopics)
            {
                var name = $"{domain} — {sub}";
                wantedNames.Add(name);
                descriptions[name] = $"Articles about {sub.ToLowerInvariant()} in {domain.ToLowerInvariant()}.";
            }
        }

        // Defensive: if this seeder runs against a database that already has
        // some categories (e.g. left over from an earlier, smaller seed, or a
        // previous partial run), only insert the ones that don't already
        // exist by name — CategoryName is unique, so inserting a duplicate
        // would otherwise throw.
        var existingNames = await context.Categories.Select(c => c.CategoryName).ToListAsync();
        var namesToInsert = wantedNames.Except(existingNames).ToList();

        if (namesToInsert.Count > 0)
        {
            var newCategories = namesToInsert
                .Select(name => new Category { CategoryName = name, Description = descriptions[name] })
                .ToList();
            context.Categories.AddRange(newCategories);
            await context.SaveChangesAsync();
        }

        // Return the full set this run cares about (pre-existing + newly inserted),
        // so downstream steps (articles) always have the complete pool to pick from.
        var categories = await context.Categories.Where(c => wantedNames.Contains(c.CategoryName)).ToListAsync();
        return categories;
    }

    // ==================================================================
    // Tags — 225+ distinct, real technology names
    // ==================================================================
    private static async Task<List<Tag>> SeedTagsAsync(CodeSphereDbContext context)
    {
        string[] rawTagNames =
        {
            // Languages
            "CSharp", "Java", "Python", "JavaScript", "TypeScript", "Go", "Rust", "Kotlin", "Swift",
            "Ruby", "PHP", "Cpp", "C", "Scala", "Elixir", "Haskell", "Dart", "R", "Perl", "Lua",
            "FSharp", "Clojure", "Julia", "Zig", "VisualBasic",
            // Frameworks & libraries
            "AspNetCore", "EntityFrameworkCore", "React", "Angular", "VueJs", "Svelte", "NextJs",
            "NuxtJs", "Django", "Flask", "FastAPI", "SpringBoot", "ExpressJs", "NestJs",
            "RubyOnRails", "Laravel", "Blazor", "JQuery", "Redux", "RxJs", "GraphQL", "gRPC",
            "Electron", "Xamarin", "Flutter", "ReactNative", "SwiftUI", "JetpackCompose",
            "TailwindCSS", "Bootstrap",
            // Databases
            "SqlServer", "PostgreSQL", "MySQL", "SQLite", "MongoDB", "Redis", "Cassandra",
            "DynamoDB", "Elasticsearch", "Neo4j", "CockroachDB", "MariaDB", "OracleDatabase",
            "Firebase", "Supabase", "InfluxDB", "CouchDB", "Snowflake", "BigQuery", "Memcached",
            // Cloud & DevOps
            "AWS", "Azure", "GoogleCloud", "Docker", "Kubernetes", "Terraform", "Ansible",
            "Jenkins", "GitHubActions", "GitLabCI", "CircleCI", "Helm", "Prometheus", "Grafana",
            "Istio", "Nginx", "ApacheKafka", "RabbitMQ", "ServerlessComputing", "AWSLambda",
            "CloudFormation", "Pulumi", "Vagrant", "Chef", "Puppet", "Consul", "Vault", "ArgoCD",
            "Datadog", "Splunk",
            // Testing & tools
            "Jest", "xUnit", "NUnit", "Selenium", "Cypress", "Playwright", "Postman", "OpenAPI",
            "Git", "GitHub", "GitLab", "Npm", "Yarn", "Webpack", "Vite", "ESLint", "Prettier",
            "Mocha", "Chai", "JMeter",
            // Concepts & architecture
            "Microservices", "DomainDrivenDesign", "CleanArchitecture", "SOLIDPrinciples",
            "DesignPatterns", "RESTAPIs", "WebSockets", "OAuth2", "JWT", "CICD", "UnitTesting",
            "TestDrivenDevelopment", "Agile", "Scrum", "DevSecOps", "SiteReliabilityEngineering",
            "LoadBalancing", "Caching", "MessageQueues", "EventDrivenArchitecture", "CQRS",
            "EdgeComputing", "WebAssembly", "ProgressiveWebApps", "Accessibility",
            "ResponsiveDesign", "SEO", "BigData", "DataScience", "ETLPipelines", "DataWarehousing",
            "APIGateway", "ServiceMesh", "Observability", "ChaosEngineering",
            // AI / ML
            "MachineLearning", "DeepLearning", "NaturalLanguageProcessing", "ComputerVision",
            "ReinforcementLearning", "TensorFlow", "PyTorch", "ScikitLearn",
            "LargeLanguageModels", "GenerativeAI", "MLOps", "NeuralNetworks", "DataAnnotation",
            "ModelDeployment", "FeatureEngineering",
            // Security
            "Cryptography", "PenetrationTesting", "ZeroTrustSecurity", "ApplicationSecurity",
            "NetworkSecurity", "IdentityManagement", "SecureCoding", "ThreatModeling",
            "VulnerabilityScanning", "OWASP",
            // Mobile / OS / infra
            "AndroidDevelopment", "iOSDevelopment", "Linux", "BashScripting", "PowerShell",
            "WindowsServer", "NetworkingFundamentals", "TCPIP", "DNS",
            "ContentDeliveryNetworks", "LoadBalancers", "VirtualMachines", "Containers",
            "ServerlessArchitecture",
            // Misc
            "MobileUXDesign", "GameEngines", "Unity", "UnrealEngine", "AugmentedReality",
            "VirtualReality", "IoT", "RaspberryPi", "Arduino", "RoboticsProgramming",
            "QuantumComputing", "EdgeAI", "SelfHostedApps", "StaticSiteGenerators", "Jamstack",
            "HeadlessCMS", "ContentManagementSystems", "PaymentGateways", "Stripe", "Twilio",
            "SendGrid", "WebRTC", "GraphicsProgramming", "OpenGL", "Vulkan", "CompilerDesign",
            "OperatingSystemsTheory", "FunctionalProgramming", "ObjectOrientedDesign"
        };

        // Defensive: guarantee uniqueness even if a name were accidentally
        // duplicated above (Tags.TagName has a unique constraint).
        var tagNames = rawTagNames.Distinct().ToArray();

        // Also defensive against a database that already has some of these
        // tags from an earlier, smaller seed (e.g. "CSharp", "Docker",
        // "JavaScript" all existed in the original hand-written sample data)
        // — only insert names that aren't already present.
        var existingTagNames = await context.Tags.Select(t => t.TagName).ToListAsync();
        var tagNamesToInsert = tagNames.Except(existingTagNames).ToArray();

        if (tagNamesToInsert.Length > 0)
        {
            var newTags = tagNamesToInsert
                .Select(name => new Tag { TagName = name, Description = $"{name}-related content." })
                .ToList();
            context.Tags.AddRange(newTags);
            await context.SaveChangesAsync();
        }

        // Return the full set this run cares about (pre-existing + newly inserted).
        var tags = await context.Tags.Where(t => tagNames.Contains(t.TagName)).ToListAsync();
        return tags;
    }

    // ==================================================================
    // Users + UserProfiles — ~230 accounts, each with a full profile
    // ==================================================================
    private static async Task<List<ApplicationUser>> SeedUsersAsync(CodeSphereDbContext context, UserManager<ApplicationUser> userManager, Random rng)
    {
        string[] firstNames =
        {
            "Ali", "Sara", "Reza", "John", "Emma", "Armin", "Nina", "Hossein", "Tina", "Maria",
            "David", "Sophia", "Liam", "Olivia", "Noah", "Ava", "Ethan", "Mia", "Lucas", "Isabella",
            "Mason", "Amelia", "Logan", "Charlotte", "James", "Harper", "Benjamin", "Evelyn",
            "Elijah", "Abigail", "Daniel", "Emily", "Matthew", "Elizabeth", "Henry", "Sofia",
            "Jackson", "Avery", "Sebastian", "Ella", "Jack", "Scarlett", "Owen", "Grace", "Samuel",
            "Chloe", "Leo", "Victoria", "Gabriel", "Zoey"
        };

        string[] lastNames =
        {
            "Mohammadi", "Karimi", "Hosseini", "Doe", "Wilson", "Nouri", "Brown", "Rahimi",
            "Safari", "Garcia", "Smith", "Johnson", "Williams", "Jones", "Miller", "Davis",
            "Rodriguez", "Martinez", "Hernandez", "Lopez", "Gonzalez", "Perez", "Taylor",
            "Anderson", "Thomas", "Moore", "Jackson", "Martin", "Lee", "Thompson", "White",
            "Harris", "Clark", "Lewis", "Walker", "Young", "Allen", "King", "Wright", "Scott",
            "Green", "Baker", "Adams", "Nelson", "Carter", "Mitchell", "Roberts", "Turner",
            "Phillips", "Campbell"
        };

        string[] countries =
        {
            "Iran", "United States", "United Kingdom", "Germany", "Canada", "India", "Brazil",
            "Japan", "France", "Spain", "Netherlands", "Australia", "Nigeria", "South Korea",
            "Italy", "Sweden", "Poland", "Turkey", "Mexico", "Argentina", "South Africa", "Egypt",
            "Indonesia", "Vietnam", "Ukraine", "Portugal", "Ireland", "Israel", "Norway", "Finland"
        };

        string[] bioExtras =
        {
            "Passionate about clean code and mentoring junior developers.",
            "Enjoys writing about real-world lessons learned from production incidents.",
            "Contributes to open-source projects in spare time.",
            "Believes good documentation is as important as good code.",
            "Speaks occasionally at local meetups about developer tooling.",
            "Spends weekends experimenting with new frameworks and side projects.",
            "Advocates for pragmatic testing over 100% coverage for its own sake.",
            "Previously worked in DevOps before moving into full-time backend engineering.",
            "Interested in the intersection of software architecture and team structure.",
            "Still occasionally answers questions on Stack Overflow for fun."
        };

        var users = new List<ApplicationUser>();
        var usedUsernames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        const int userAttempts = 230;

        for (var i = 0; i < userAttempts; i++)
        {
            var first = firstNames[rng.Next(firstNames.Length)];
            var last = lastNames[rng.Next(lastNames.Length)];
            var baseUsername = (first + "_" + last).ToLowerInvariant();
            var username = baseUsername;
            var suffix = 1;
            while (!usedUsernames.Add(username))
                username = baseUsername + suffix++;

            var user = new ApplicationUser
            {
                UserName = username,
                Email = $"{username}@example.com",
                EmailConfirmed = true,
                Status = "Active",
                JoinDate = DateTime.UtcNow.AddDays(-rng.Next(20, 720))
            };

            var result = await userManager.CreateAsync(user, DemoPassword);
            if (!result.Succeeded)
                continue; // extremely unlikely given the dedup above, but never let seeding crash on it

            await userManager.AddToRoleAsync(user, "Reader");
            users.Add(user);
        }

        var focusTopics = new[]
        {
            "backend systems", "frontend engineering", "cloud infrastructure", "data engineering",
            "mobile development", "DevOps automation", "machine learning", "database performance",
            "distributed systems", "application security", "developer tooling", "site reliability"
        };

        var profiles = new List<UserProfile>();
        foreach (var user in users)
        {
            var focus = focusTopics[rng.Next(focusTopics.Length)];
            var extra = bioExtras[rng.Next(bioExtras.Length)];
            var fullName = CapitalizeUsername(user.UserName!);

            profiles.Add(new UserProfile
            {
                UserID = user.Id,
                FullName = fullName,
                Bio = $"Software engineer focused on {focus}. {extra}",
                Country = countries[rng.Next(countries.Length)],
                WebsiteURL = $"https://{user.UserName}.dev",
                // ui-avatars.com generates a deterministic initials-based avatar image
                // from a name — a safe, well-known placeholder service (https only),
                // appropriate for seed/demo data. See ImageUrlValidator for the rules
                // applied to any URL a real user supplies at registration.
                ProfileImageURL = $"https://ui-avatars.com/api/?name={Uri.EscapeDataString(fullName)}&background=random&size=256"
            });
        }

        context.UserProfiles.AddRange(profiles);
        await context.SaveChangesAsync();
        return users;
    }

    private static string CapitalizeUsername(string userName)
    {
        var parts = userName.Split('_', StringSplitOptions.RemoveEmptyEntries);
        var capitalized = parts.Select(p => p.Length == 0 ? p : char.ToUpperInvariant(p[0]) + p[1..]);
        return string.Join(' ', capitalized);
    }

    // ==================================================================
    // Articles — 225 rows, generated technical write-ups
    // ==================================================================
    private static async Task<List<Article>> SeedArticlesAsync(CodeSphereDbContext context, Random rng, List<ApplicationUser> users, List<Category> categories)
    {
        // A curated pool of real, human-readable subjects to write about —
        // deliberately not identical to the Tags list (though overlapping),
        // so article titles read naturally.
        string[] topics =
        {
            "ASP.NET Core", "Entity Framework Core", "React", "Angular", "Vue.js", "Docker",
            "Kubernetes", "PostgreSQL", "SQL Server", "MongoDB", "Redis", "GraphQL", "REST APIs",
            "Microservices", "Domain-Driven Design", "Clean Architecture", "CI/CD pipelines",
            "Terraform", "AWS Lambda", "Azure Functions", "Apache Kafka", "RabbitMQ",
            "Machine Learning", "Natural Language Processing", "Computer Vision", "TensorFlow",
            "PyTorch", "Python", "TypeScript", "Rust", "Go", "Kotlin", "Swift", "Flutter",
            "React Native", "WebAssembly", "Progressive Web Apps", "OAuth2 and JWT",
            "Zero Trust Security", "Penetration Testing", "Site Reliability Engineering",
            "Observability and Distributed Tracing", "Event-Driven Architecture", "CQRS",
            "Database Indexing", "Query Optimization", "Caching Strategies", "Load Balancing",
            "Message Queues", "API Gateways", "Service Meshes", "Chaos Engineering",
            "Unit Testing", "Test-Driven Development", "Property-Based Testing", "GitOps",
            "Infrastructure as Code", "Serverless Architecture", "Edge Computing",
            "Large Language Models", "Generative AI", "MLOps", "Data Warehousing",
            "ETL Pipelines", "Big Data Processing", "WebSockets", "gRPC", "Blazor", "Next.js",
            "Svelte", "Django", "FastAPI", "Spring Boot", "Ruby on Rails", "Laravel"
        };

        string[] titleTemplates =
        {
            "Getting Started with {0}",
            "A Practical Guide to {0}",
            "5 Lessons Learned Building With {0}",
            "Why {0} Matters for Modern Software Teams",
            "Deep Dive: How {0} Works Under the Hood",
            "Common Pitfalls When Using {0}",
            "Scaling {0} in Production",
            "{0} Best Practices for 2026",
            "From Zero to Production with {0}",
            "Debugging {0}: A Field Guide",
            "Migrating Legacy Systems to {0}",
            "Performance Tuning for {0}",
            "Securing Applications Built with {0}",
            "An Architect's Perspective on {0}",
            "Testing Strategies for {0}"
        };

        string[] introParagraphs =
        {
            "{0} has become a cornerstone of how modern engineering teams build and ship software. Whether you are evaluating it for a greenfield project or maintaining a system that already depends on it, understanding its core mechanics pays off quickly. In this article we walk through the concepts that matter most in day-to-day work, skipping the marketing pitch in favor of practical detail you can apply immediately.",
            "When I first started working with {0}, most of the available material either stayed too high-level or dove straight into edge cases without covering the fundamentals. This article tries to close that gap: a grounded walkthrough of what {0} actually does, where it shines, and where it introduces real trade-offs worth knowing about before you commit to it.",
            "Teams adopt {0} for different reasons — some chase performance, others want a smaller operational footprint, and a few are simply standardizing on tooling the rest of the industry has converged on. Regardless of the motivation, getting the fundamentals right early saves a lot of rework later. That is the goal of this piece.",
            "There is no shortage of introductory material on {0}, but most of it stops at a trivial example and leaves the harder questions — how does this behave under load, what happens when it fails, how do you debug it in production — unanswered. This article picks up exactly where those tutorials leave off.",
            "{0} is frequently misunderstood by teams that adopt it based on a blog post rather than a careful evaluation of their own requirements. This is not a criticism of the technology itself, which is genuinely solid, but a reminder that context matters. Below, we unpack what {0} is actually good at, and where it is not the right tool.",
            "A recurring theme in the projects I have worked on is that {0} gets introduced early, then quietly becomes load-bearing infrastructure that nobody fully understands anymore. This article is an attempt to document the mental model that would have saved my own team a lot of debugging time.",
            "This piece assumes you already know what {0} is for and skips straight to the parts that matter once you are past the initial setup: configuration choices that age well, patterns that keep the codebase maintainable, and the handful of mistakes that show up again and again in code review.",
            "Every technology has an onboarding tax — the time it takes a new engineer to become productive with it. {0} is no exception. What follows is the condensed version of that onboarding process, focused on the concepts that took the longest for our team to internalize."
        };

        string[] coreConceptParagraphs =
        {
            "At its core, {0} solves a fairly narrow problem well, and most of the confusion around it comes from stretching it to do more than that. Once you separate what {0} is actually responsible for from the surrounding tooling people bundle around it, the mental model becomes much simpler to reason about.",
            "The key abstraction in {0} is worth understanding before anything else, because almost every higher-level feature is built on top of it. Once that clicks, features that initially seemed like unrelated add-ons start to look like natural extensions of the same underlying idea.",
            "One thing that is easy to miss with {0} is how much of its behavior is configurable, and how much of that configuration has sensible-looking defaults that are actually wrong for production workloads. Reading through the defaults line by line, rather than trusting them blindly, is time well spent.",
            "{0} trades some flexibility for a much simpler operational story, and that trade-off is the right one for most teams. Understanding exactly where that line is drawn — what {0} handles for you automatically, and what it still expects you to own — avoids a lot of surprises later.",
            "Internally, {0} is built around a small number of primitives that compose well together. Once you can name those primitives and describe what each one is responsible for, most of the documentation starts making a lot more sense, because it stops feeling like a list of disconnected features.",
            "A lot of the perceived complexity in {0} actually comes from the ecosystem around it, not the core tool itself. Separating the two — what {0} does natively versus what a plugin or community convention adds on top — makes it much easier to reason about what is actually happening when something goes wrong.",
            "The design philosophy behind {0} favors explicit configuration over hidden magic, which means the learning curve is front-loaded: it takes a bit longer to get your first working example running, but the resulting system is much easier to reason about six months later when you are debugging it under pressure.",
            "{0} is often compared directly to its closest alternatives, but that comparison misses the point if you do not first understand the specific constraints it was designed around. Once those constraints are clear, it becomes obvious why certain design decisions were made, even the ones that feel unusual at first."
        };

        string[] practicalParagraphs =
        {
            "In practice, most of the pain with {0} shows up not in the happy path but in how it behaves under partial failure — a timeout, a dropped connection, a dependency that returns malformed data. Building in defensive handling for these cases from day one is significantly cheaper than retrofitting it after an incident.",
            "A pattern that has served us well when working with {0} is to keep the integration code isolated behind a small internal interface, rather than scattering direct calls throughout the codebase. It costs a little extra structure up front, but it makes both testing and eventually swapping out {0} dramatically easier.",
            "Observability is where teams most often under-invest when adopting {0}. Structured logging, meaningful metrics, and clear error messages around the integration points pay for themselves the first time something goes wrong in production and you need to understand what happened without reproducing the issue locally.",
            "Configuration drift is a quiet but common source of bugs with {0}: the settings that work perfectly in a developer's local environment do not always match what is deployed, and the gap only becomes visible when something breaks. Treating configuration as code, reviewed the same way application code is, closes that gap.",
            "When integrating {0} into an existing system, resist the temptation to migrate everything at once. Wrapping the new dependency behind a feature flag and rolling it out to a small percentage of traffic first turns a risky big-bang migration into a series of small, reversible steps.",
            "Testing code that depends on {0} is much easier if you design for it from the start — favor dependency injection over static access, and keep the parts of your code that talk to {0} as thin as possible so the business logic around them can be tested in isolation.",
            "Documentation that only covers the happy path is incomplete. For {0} specifically, it is worth explicitly documenting what happens on timeout, on partial success, and on a version mismatch, since those are exactly the situations where a tired on-call engineer needs clear guidance rather than a wall of source code to read through.",
            "A small but effective habit: whenever you make a non-obvious configuration choice for {0}, leave a short comment explaining why, not just what. Six months later, that context is what prevents someone — often you — from \"cleaning up\" a setting that was actually load-bearing."
        };

        string[] pitfallParagraphs =
        {
            "The most common mistake teams make with {0} is treating the default configuration as production-ready without reviewing it against their actual traffic patterns. What works fine in a demo can behave very differently once real concurrency and real data volumes are involved.",
            "It is worth setting explicit limits — timeouts, retry counts, connection pool sizes — rather than relying on defaults for {0}. Unbounded retries in particular have a way of turning a small, contained failure into a full outage once they start competing for the same limited resources.",
            "A best practice that pays off repeatedly: version-pin your {0} dependencies and upgrade deliberately, on a schedule, with a changelog review — rather than letting a routine dependency update silently pull in a breaking change the week before a release.",
            "Security reviews of systems built on {0} tend to focus on the obvious surface (authentication, input validation) and miss the quieter risks: verbose error messages that leak internal details, or default credentials left unchanged in a staging environment that is more exposed than anyone realized.",
            "Capacity planning for {0} deserves more attention than it usually gets. Load testing with realistic data shapes — not just realistic request volume — regularly surfaces bottlenecks that a simple throughput test would never catch.",
            "One anti-pattern worth calling out explicitly: reaching for {0} because it is trendy rather than because it fits the problem at hand. The technology itself is solid, but adopting it without a clear use case just adds operational surface area without a corresponding benefit.",
            "Rollback plans are often an afterthought when adopting {0}, but they should be designed alongside the rollout plan, not after something has already gone wrong. Knowing exactly how to revert — and having practiced it — turns a stressful incident into a routine procedure.",
            "Code review is where a lot of subtle {0} mistakes get caught, provided reviewers know what to look for. Building a short internal checklist of the mistakes your team has actually made in the past is far more useful than a generic list borrowed from a blog post."
        };

        string[] conclusionParagraphs =
        {
            "{0} is a strong choice when used deliberately, with a clear understanding of its trade-offs rather than as a default reach. Hopefully the patterns above save you some of the trial and error it took to arrive at them.",
            "None of this is a reason to avoid {0} — quite the opposite. It is a mature, well-supported piece of the modern toolchain. The goal here was simply to shortcut the learning curve for the next engineer who has to work with it.",
            "As with most infrastructure decisions, the right way to use {0} depends heavily on your specific constraints. Treat the guidance above as a starting point to adapt, not a rulebook to follow blindly.",
            "If there is one takeaway, it is this: invest in understanding {0} at the level of its core primitives, not just its high-level API. That investment keeps paying off long after the initial integration is done.",
            "{0} will keep evolving, and some of the specifics here will age. The underlying principles — explicit configuration, defensive error handling, and deliberate rollout — will still apply to whatever comes next.",
            "Thanks for reading. If your team has found different trade-offs with {0} in production, that kind of real-world experience is exactly what is missing from most of the documentation out there — consider writing it up."
        };

        var articles = new List<Article>();
        const int articleCount = 225;

        for (var i = 0; i < articleCount; i++)
        {
            var topic = topics[rng.Next(topics.Length)];
            var category = categories[rng.Next(categories.Count)];
            var author = users[rng.Next(users.Count)];

            var titleTemplate = titleTemplates[rng.Next(titleTemplates.Length)];
            var title = string.Format(titleTemplate, topic);

            var body = string.Join("\n\n", new[]
            {
                string.Format(introParagraphs[rng.Next(introParagraphs.Length)], topic),
                string.Format(coreConceptParagraphs[rng.Next(coreConceptParagraphs.Length)], topic),
                string.Format(practicalParagraphs[rng.Next(practicalParagraphs.Length)], topic),
                string.Format(pitfallParagraphs[rng.Next(pitfallParagraphs.Length)], topic),
                string.Format(conclusionParagraphs[rng.Next(conclusionParagraphs.Length)], topic)
            });

            var wordCount = body.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;
            var isPublished = rng.NextDouble() > 0.08; // ~92% published, the rest left as drafts

            articles.Add(new Article
            {
                UserID = author.Id,
                CategoryID = category.CategoryID,
                Title = title,
                Content = body,
                ReadingTime = Math.Max(2, wordCount / 200),
                Status = isPublished ? "Published" : "Draft",
                PublishDate = isPublished ? DateTime.UtcNow.AddDays(-rng.Next(1, 400)) : null,
                ViewCount = isPublished ? rng.Next(0, 5000) : 0
            });
        }

        context.Articles.AddRange(articles);
        await context.SaveChangesAsync();
        return articles;
    }

    // ==================================================================
    // ArticleTags, Comments, Reactions, Bookmarks, Follows
    // ==================================================================
    private static async Task SeedEngagementDataAsync(CodeSphereDbContext context, Random rng, List<ApplicationUser> users, List<Article> articles, List<Tag> tags)
    {
        // ---- ArticleTags: 1-4 tags per article ----
        var articleTagPairs = new HashSet<(int ArticleId, int TagId)>();
        var articleTags = new List<ArticleTag>();
        foreach (var article in articles)
        {
            var tagCount = rng.Next(1, 5);
            for (var t = 0; t < tagCount; t++)
            {
                var tag = tags[rng.Next(tags.Count)];
                if (articleTagPairs.Add((article.ArticleID, tag.TagID)))
                    articleTags.Add(new ArticleTag { ArticleID = article.ArticleID, TagID = tag.TagID });
            }
        }
        context.ArticleTags.AddRange(articleTags);

        // ---- Comments: ~260 ----
        string[] commentTemplates =
        {
            "Great write-up, thanks for sharing!",
            "I ran into a similar issue last month — this saved me a lot of time.",
            "Have you tried this approach with larger datasets? Curious how it scales.",
            "This is exactly what I needed for my current project.",
            "Well explained, especially the section on best practices.",
            "I'd push back slightly on one point — in my experience this trade-off doesn't always hold, but still a solid overview overall.",
            "Bookmarking this for future reference.",
            "Could you elaborate more on the performance implications in a follow-up post?",
            "Solid introduction for beginners, wish I had this when I started.",
            "We adopted this pattern at work and it's been a net positive so far.",
            "Small correction: I think the configuration example needs a trailing slash, but otherwise spot on.",
            "This matches what we found in production almost exactly.",
            "Appreciate you including the failure-mode discussion, most articles skip that part.",
            "Sharing this with my team, it's a good reference for our onboarding docs.",
            "Curious how this compares to the alternative approach mentioned in the intro.",
            "The section on observability alone was worth the read.",
            "I've bookmarked this three times now, finally leaving a comment to say thanks.",
            "This helped me debug an issue I'd been stuck on for two days.",
            "Would love to see a part two covering the migration process in more detail.",
            "Clear, concise, and technically accurate — not always a given these days.",
            "We hit a very similar pitfall last quarter, wish this had been published sooner.",
            "Good reminder about setting explicit timeouts, we learned that one the hard way too.",
            "Nice breakdown of the trade-offs, this is the kind of context that's usually missing.",
            "I disagree with the framing in the second paragraph, but the rest is spot on.",
            "This is now required reading for new hires on my team.",
            "Appreciate the honesty about where this doesn't fit well — too many posts oversell.",
            "Following up after trying this in a side project: works as described.",
            "Any recommendations for monitoring this in production?",
            "The code review checklist idea at the end is something I'm stealing for my team.",
            "This lines up with what we saw during our last incident postmortem.",
            "Thanks for the field guide, saved me from a rabbit hole of trial and error.",
            "Great timing, I'm evaluating this exact decision right now.",
            "The rollback advice is underrated — most teams only think about roll-forward.",
            "Adding this to our internal wiki, thanks for taking the time to write it up.",
            "This is a much better explanation than the official docs, honestly."
        };

        var comments = new List<Comment>();
        const int commentCount = 260;
        for (var i = 0; i < commentCount; i++)
        {
            var article = articles[rng.Next(articles.Count)];
            var commenter = users[rng.Next(users.Count)];
            comments.Add(new Comment
            {
                ArticleID = article.ArticleID,
                UserID = commenter.Id,
                CommentText = commentTemplates[rng.Next(commentTemplates.Length)],
                CommentDate = DateTime.UtcNow.AddDays(-rng.Next(0, 350))
            });
        }
        context.Comments.AddRange(comments);

        // ---- Reactions: ~260, at most one reaction per (user, article) pair ----
        string[] reactionTypes = { "Like", "Love", "Idea" };
        var reactionPairs = new HashSet<(int ArticleId, int UserId)>();
        var reactions = new List<Reaction>();
        var reactionAttempts = 0;
        while (reactions.Count < 260 && reactionAttempts < 6000)
        {
            reactionAttempts++;
            var article = articles[rng.Next(articles.Count)];
            var reactor = users[rng.Next(users.Count)];
            if (reactionPairs.Add((article.ArticleID, reactor.Id)))
            {
                reactions.Add(new Reaction
                {
                    ArticleID = article.ArticleID,
                    UserID = reactor.Id,
                    ReactionType = reactionTypes[rng.Next(reactionTypes.Length)],
                    ReactionDate = DateTime.UtcNow.AddDays(-rng.Next(0, 350))
                });
            }
        }
        context.Reactions.AddRange(reactions);

        // ---- Bookmarks: ~230, at most one per (user, article) pair ----
        var bookmarkPairs = new HashSet<(int UserId, int ArticleId)>();
        var bookmarks = new List<Bookmark>();
        var bookmarkAttempts = 0;
        while (bookmarks.Count < 230 && bookmarkAttempts < 6000)
        {
            bookmarkAttempts++;
            var article = articles[rng.Next(articles.Count)];
            var user = users[rng.Next(users.Count)];
            if (bookmarkPairs.Add((user.Id, article.ArticleID)))
            {
                bookmarks.Add(new Bookmark
                {
                    UserID = user.Id,
                    ArticleID = article.ArticleID,
                    SavedDate = DateTime.UtcNow.AddDays(-rng.Next(0, 300))
                });
            }
        }
        context.Bookmarks.AddRange(bookmarks);

        // ---- Follows: ~230, unique pairs, no self-follows ----
        var followPairs = new HashSet<(int FollowerId, int FollowingId)>();
        var follows = new List<Follow>();
        var followAttempts = 0;
        while (follows.Count < 230 && followAttempts < 6000)
        {
            followAttempts++;
            var follower = users[rng.Next(users.Count)];
            var following = users[rng.Next(users.Count)];
            if (follower.Id == following.Id)
                continue;
            if (followPairs.Add((follower.Id, following.Id)))
            {
                follows.Add(new Follow
                {
                    FollowerUserID = follower.Id,
                    FollowingUserID = following.Id,
                    FollowDate = DateTime.UtcNow.AddDays(-rng.Next(0, 300))
                });
            }
        }
        context.Follows.AddRange(follows);

        await context.SaveChangesAsync();
    }
}
