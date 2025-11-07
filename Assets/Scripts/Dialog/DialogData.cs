public enum DialogType { Ambient, Story, Tutorial }

public class DialogData {
    public DialogType Type { get; set; }
    public string NPCId { get; set; }
    public string StoryId { get; set; }
    public int StoryStageMin { get; set; } = 0;
    public int StoryStageMax { get; set; } = -1;
    public string[] Tags { get; set; } = Array.Empty<string>();
    public string[] RequiredFlags { get; set; } = Array.Empty<string>();
    public string[] ForbiddenFlags { get; set; } = Array.Empty<string>();
    public bool Once { get; set; }
    public float CooldownSec { get; set; } = 0f;
    public int Priority { get; set; } = 0;
    public int Weight { get; set; } = 1;
    public string Key { get; set; }
    public string Kor { get; set; }
}
