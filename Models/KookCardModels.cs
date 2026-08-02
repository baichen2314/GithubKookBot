namespace GithubKookBot.Models;

public class KookCard
{
    public string type { get; set; } = "card";
    public string theme { get; set; } = "success";
    public string size { get; set; } = "lg";
    public List<object> modules { get; set; } = new();
}

public class KookHeaderModule
{
    public string type { get; set; } = "header";
    public KookPlainText text { get; set; } = new();
}

public class KookDividerModule
{
    public string type { get; set; } = "divider";
}

public class KookSectionWithImage
{
    public string type { get; set; } = "section";
    public KookImage accessory { get; set; } = new();
    public KookPlainText text { get; set; } = new();
}

public class KookSection
{
    public string type { get; set; } = "section";

    public object text { get; set; } = new KookPlainText();
}

public class KookContextModule
{
    public string type { get; set; } = "context";
    public List<object> elements { get; set; } = new();
}

public class KookPlainText
{
    public string type { get; set; } = "plain-text";
    public string content { get; set; } = string.Empty;
}

public class KookMarkdownText
{
    public string type { get; set; } = "kmarkdown";
    public string content { get; set; } = string.Empty;
}

public class KookImage
{
    public string type { get; set; } = "image";
    public string src { get; set; } = string.Empty;
    public string size { get; set; } = "sm";
}

public class KookActionGroup
{
    public string type { get; set; } = "action-group";
    public List<KookButton> elements { get; set; } = new();
}

public class KookButton
{
    public string type { get; set; } = "button";
    public string theme { get; set; } = "link";
    public string click { get; set; } = "link";
    public string value { get; set; } = string.Empty;
    public KookPlainText text { get; set; } = new();
}