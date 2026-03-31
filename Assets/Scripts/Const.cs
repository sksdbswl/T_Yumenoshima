public static class Const
{
    // 직업
    public enum JobType
    {
        None,
        Citizen,
        Firefighter,
        Doctor,
        Police,
        Thief,
        Banker,
    }
    
    // 감정
    public enum EEmotion
    {
        None = 0,

        Neutral,    // 기본
        Happy,      // 기쁨
        Angry,      // 분노
        Sad,        // 슬픔
        Fear,       // 두려움
        Surprised,  // 놀람

        Tired,      // 지침 : 상태 이상
        Sleepy,     // 졸림
        Bored,      // 심심함

        Excited,    // 신남
        Embarrassed,// 당황
        Exclamation,// 느낌표
    }
    
    // 효과
    public enum EEffect
    {
        None,
        Get,
        Catch,
        Change,
        Gacha
    }
}