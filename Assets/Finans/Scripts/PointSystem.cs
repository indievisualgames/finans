
public class PointSystem
{
    private static int xp = 0;
    private static int visit = 0;
    private static int stars = 0;
    private static int view = 0;
    private static int coins = 0;
    private static int passes = 0;
    private static int level_passes = 0;
    // private static int lesson_points = 0;
    // private static int flash_points = 0;
    ///private static int video_points = 0; 
    private static int life = 3;
    private static int game_points = 0;
    private static int powerup_health = 0;

    public static int XP
    {
        get { return xp; }
        set { xp = value; }
    }
    public static int Visit
    {
        get { return visit; }
        set { visit = value; }
    }
    public static int Stars
    {
        get { return stars; }
        set { stars = value; }
    }
    public static int View
    {
        get { return view; }
        set { view = value; }
    }
    public static int Coins
    {
        get { return coins; }
        set { coins = value; }
    }
    public static int Passes
    {
        get { return passes; }
        set { passes = value; }
    }
    public static int LevelPasses
    {
        get { return level_passes; }
        set { level_passes = value; }
    }

    /*
         public static int FlashPoints
         {
             get { return flash_points; }
             set { flash_points = value; }
         }  
         public static int VideoPoints
         {
             get { return video_points; }
             set { video_points = value; }
         }*/
    public static int Life
    {
        get { return life; }
        set { life = value; }
    }
    public static int GamePoints
    {
        get { return game_points; }
        set { game_points = value; }
    }
    public static int PowerupHealth
    {
        get { return powerup_health; }
        set { powerup_health = value; }
    }

}


