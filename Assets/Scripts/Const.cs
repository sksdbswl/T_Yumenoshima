public static class Const
{
    public enum JobType
    {
        None,
        Firefighter,
        Doctor,
        Police 
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

        Tired,      // 지침
        Sleepy,     // 졸림
        Bored,      // 심심함

        Excited,    // 신남
        Embarrassed // 당황
    }
    
    // 상태 이상
    public enum EStatusEffect
    {
        None        = 0,

        Sleepy      = 1 << 0, // 졸림
        Sleeping    = 1 << 1, // 잠듦
        Tired       = 1 << 2, // 힘듦 / 피로
        Stunned     = 1 << 3, // 기절
        Slowed      = 1 << 4, // 둔화
        Silenced    = 1 << 5, // 침묵
        Confused    = 1 << 6, // 혼란
        Poisoned    = 1 << 7, // 중독
        Burning     = 1 << 8, // 화상
        Frozen      = 1 << 9, // 빙결
        Immobilized = 1 << 10 // 속박 / 이동불가
    }
}