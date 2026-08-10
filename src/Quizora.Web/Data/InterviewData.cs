namespace Quizora.Web.Data;

public static class InterviewData
{
    public record Resource(string Title, string Url, string Description, string Icon, string Tag, string BadgeClass);
    public record TopicItem(string Slug, string Name, string Icon, string Short, string Description, List<Resource> Resources);

    public static List<TopicItem> All { get; } = new()
    {
        T("oop", "OOP", "🧱", "Classes & SOLID",
            "Object-Oriented Programming fundamentals for interviews",
            R("GeeksforGeeks – OOP", "https://www.geeksforgeeks.org/dsa/introduction-of-object-oriented-programming/", "Complete OOP concepts with examples", "📘", "GFG", "bg-success"),
            R("Programiz – OOP", "https://www.programiz.com/cpp-programming/oop", "Beginner-friendly OOP intro", "🟢", "Programiz", "bg-primary"),
            R("LeetCode – OOPs", "https://leetcode.com/discuss/post/3828150/oops-cheatsheet-for-interviews-30-questi-7nt4/", "OOPS Cheatsheet for Interviews | 30 questions - Discuss\r\n", "📄", "LeetCode", "bg-secondary"),
            R("GreeksforGreek - OOP", "https://www.geeksforgeeks.org/system-design/solid-principle-in-programming-understand-with-real-life-examples/", "SOLID principles overview", "📚", "GreeksforGreek", "bg-info text-dark")
        ),

        T("dsa", "Data Structures", "📊", "Array to Graph",
            "Core data structures asked in coding interviews",
            R("GeeksforGeeks – DSA", "https://www.geeksforgeeks.org/data-structures/", "Huge DSA library + practice", "📘", "GFG", "bg-success"),
            R("Programiz – DS", "https://www.programiz.com/dsa", "Visual explanations", "🟢", "Programiz", "bg-primary"),
            R("VisuAlgo", "https://visualgo.net/en", "Interactive visualizations", "👁️", "VisuAlgo", "bg-warning text-dark"),
            R("CP-Algorithms", "https://cp-algorithms.com/", "Advanced implementations", "⚡", "CP-Algo", "bg-dark")
        ),

        T("algorithms", "Algorithms", "🧮", "Sorting & complexity",
            "Sorting, searching, complexity analysis",
            R("GeeksforGeeks – Algorithms", "https://www.geeksforgeeks.org/fundamentals-of-algorithms/", "Algorithm catalog", "📘", "GFG", "bg-success"),
            R("Programiz – Algorithms", "https://www.programiz.com/dsa/algorithm", "Clear step-by-step guides", "🟢", "Programiz", "bg-primary"),
            R("Khan Academy", "https://www.khanacademy.org/computing/computer-science/algorithms", "Free video lessons", "🎓", "Khan", "bg-info text-dark"),
            R("Big-O Cheat Sheet", "https://www.bigocheatsheet.com/", "Complexity reference", "📈", "Cheatsheet", "bg-secondary")
        ),

        T("dbms", "DBMS", "🗄️", "SQL & design",
            "Databases, SQL, normalization, transactions",
            R("GeeksforGeeks – DBMS", "https://www.geeksforgeeks.org/dbms/", "Full DBMS theory", "📘", "GFG", "bg-success"),
            R("Programiz – SQL", "https://www.programiz.com/sql", "SQL with examples", "🟢", "Programiz", "bg-primary"),
            R("W3Schools – SQL", "https://www.w3schools.com/sql/", "Quick SQL practice", "🌐", "W3Schools", "bg-warning text-dark"),
            R("Use The Index Luke", "https://use-the-index-luke.com/", "Indexing deep dive", "📑", "Index", "bg-dark")
        ),

        T("os", "Operating Systems", "💻", "Process & memory",
            "Processes, threads, memory, deadlocks",
            R("GeeksforGeeks – OS", "https://www.geeksforgeeks.org/operating-systems/", "OS interview topics", "📘", "GFG", "bg-success"),
            R("JavaTpoint – OS", "https://www.javatpoint.com/operating-system", "Structured notes", "📄", "JavaTpoint", "bg-secondary"),
            R("TutorialsPoint – OS", "https://www.tutorialspoint.com/operating_system/index.htm", "Chapter-wise OS", "📚", "TutorialsPoint", "bg-info text-dark"),
            R("OSDev Wiki", "https://wiki.osdev.org/Main_Page", "Low-level OS concepts", "⚙️", "OSDev", "bg-dark")
        ),

        T("networking", "Computer Networks", "🌐", "OSI & protocols",
            "OSI, TCP/IP, HTTP, DNS",
            R("GeeksforGeeks – CN", "https://www.geeksforgeeks.org/computer-network-tutorials/", "Networks full course", "📘", "GFG", "bg-success"),
            R("JavaTpoint – CN", "https://www.javatpoint.com/computer-network-tutorial", "Interview notes", "📄", "JavaTpoint", "bg-secondary"),
            R("Cloudflare Learning", "https://www.cloudflare.com/learning/", "Modern internet concepts", "☁️", "Cloudflare", "bg-warning text-dark"),
            R("MDN – HTTP", "https://developer.mozilla.org/en-US/docs/Web/HTTP", "HTTP reference", "📕", "MDN", "bg-primary")
        ),

        T("system-design", "System Design", "🏗️", "Scale & architecture",
            "Basics of scalable system design",
            R("System Design Primer", "https://github.com/donnemartin/system-design-primer", "Best free SD guide", "⭐", "GitHub", "bg-dark"),
            R("GeeksforGeeks – SD", "https://www.geeksforgeeks.org/system-design-tutorial/", "SD tutorial series", "📘", "GFG", "bg-success"),
            R("ByteByteGo", "https://bytebytego.com/", "Visual system design", "📐", "ByteByteGo", "bg-info text-dark"),
            R("Educative – Grokking", "https://www.educative.io/courses/grokking-the-system-design-interview", "Popular SD patterns (free previews)", "🎯", "Educative", "bg-primary")
        ),

        T("csharp", "C# / .NET", "💜", "Language & runtime",
            "C# and .NET interview topics",
            R("Microsoft Learn – C#", "https://learn.microsoft.com/en-us/dotnet/csharp/", "Official C# docs", "🪟", "Microsoft", "bg-primary"),
            R("GeeksforGeeks – C#", "https://www.geeksforgeeks.org/csharp-programming-language/", "C# articles", "📘", "GFG", "bg-success"),
            R("TutorialsPoint – C#", "https://www.tutorialspoint.com/csharp/index.htm", "C# tutorial", "📚", "TutorialsPoint", "bg-info text-dark"),
            R("C# Corner", "https://www.c-sharpcorner.com/", "Community articles", "📰", "C# Corner", "bg-secondary")
        ),

        T("rest-api", "REST API & HTTP", "🔗", "APIs & methods",
            "REST principles, status codes, HTTP verbs",
            R("MDN – HTTP", "https://developer.mozilla.org/en-US/docs/Web/HTTP", "HTTP deep dive", "📕", "MDN", "bg-primary"),
            R("RestfulAPI.net", "https://restfulapi.net/", "REST best practices", "🔗", "REST", "bg-success"),
            R("GeeksforGeeks – REST", "https://www.geeksforgeeks.org/rest-api-introduction/", "REST intro", "📘", "GFG", "bg-success"),
            R("HTTP Status Codes", "https://httpstatuses.com/", "Status code reference", "📡", "Ref", "bg-dark")
        ),

        T("auth", "Auth & Security", "🔐", "JWT & OAuth",
            "Authentication, authorization, JWT",
            R("JWT.io", "https://jwt.io/introduction", "JWT introduction", "🔑", "JWT", "bg-dark"),
            R("OAuth 2.0 Simplified", "https://aaronparecki.com/oauth-2-simplified/", "Clear OAuth guide", "🛡️", "OAuth", "bg-primary"),
            R("OWASP Top 10", "https://owasp.org/www-project-top-ten/", "Web security risks", "⚠️", "OWASP", "bg-danger"),
            R("Microsoft Identity", "https://learn.microsoft.com/en-us/aspnet/core/security/", "ASP.NET security", "🪟", "Microsoft", "bg-info text-dark")
        ),

        T("git", "Git & GitHub", "🌿", "Version control",
            "Git commands and workflows",
            R("Git Official Book", "https://git-scm.com/book/en/v2", "Pro Git (free)", "📖", "Official", "bg-dark"),
            R("Atlassian Git Tutorials", "https://www.atlassian.com/git/tutorials", "Practical guides", "📘", "Atlassian", "bg-primary"),
            R("Oh My Git!", "https://ohmygit.org/", "Interactive learning", "🎮", "Game", "bg-success"),
            R("GitHub Docs", "https://docs.github.com/en/get-started", "GitHub basics", "🐙", "GitHub", "bg-secondary")
        ),

        T("docker", "Docker", "🐳", "Containers",
            "Images, containers, Dockerfile",
            R("Docker Official Docs", "https://docs.docker.com/get-started/", "Official getting started", "🐳", "Docker", "bg-primary"),
            R("GeeksforGeeks – Docker", "https://www.geeksforgeeks.org/docker-tutorial/", "Docker tutorial", "📘", "GFG", "bg-success"),
            R("Play with Docker", "https://labs.play-with-docker.com/", "Free browser labs", "🧪", "Labs", "bg-info text-dark"),
            R("Docker Curriculum", "https://docker-curriculum.com/", "Friendly full guide", "📚", "Curriculum", "bg-dark")
        ),

        T("javascript", "JavaScript", "💛", "Language basics",
            "JS fundamentals for interviews",
            R("MDN – JavaScript", "https://developer.mozilla.org/en-US/docs/Web/JavaScript/Guide", "Official guide", "📕", "MDN", "bg-primary"),
            R("javascript.info", "https://javascript.info/", "Modern JS tutorial", "⭐", "JS.info", "bg-warning text-dark"),
            R("GeeksforGeeks – JS", "https://www.geeksforgeeks.org/javascript/", "JS articles", "📘", "GFG", "bg-success"),
            R("W3Schools – JS", "https://www.w3schools.com/js/", "Quick reference", "🌐", "W3Schools", "bg-secondary")
        ),

        T("hr", "HR & Behavioral", "🗣️", "Soft skills",
            "Common HR and behavioral questions",
            R("The Muse – Behavioral", "https://www.themuse.com/advice/behavioral-interview-questions", "STAR method examples", "💬", "Muse", "bg-primary"),
            R("Indeed – HR Questions", "https://www.indeed.com/career-advice/interviewing/common-interview-questions-and-answers", "Common Q&A", "📋", "Indeed", "bg-secondary"),
            R("GFG – HR Interview", "https://www.geeksforgeeks.org/hr-interview-questions-and-answers/", "HR question bank", "📘", "GFG", "bg-success"),
            R("Amazon Leadership Principles", "https://www.amazon.jobs/content/en/our-workplace/leadership-principles", "Behavioral framework", "📦", "Amazon", "bg-dark")
        ),
    };

    public static TopicItem? Get(string slug)
        => All.FirstOrDefault(t => t.Slug.Equals(slug, StringComparison.OrdinalIgnoreCase));

    private static TopicItem T(string slug, string name, string icon, string shortDesc, string desc, params Resource[] resources)
        => new(slug, name, icon, shortDesc, desc, resources.ToList());

    private static Resource R(string title, string url, string description, string icon, string tag, string badge)
        => new(title, url, description, icon, tag, badge);
}