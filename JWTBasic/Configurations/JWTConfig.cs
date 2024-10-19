namespace JWTBasic.Configurations
{
    /*
        Class which can refer to the configurations in the app settings. Added to the DI container. Used instead of directly accessing the key from appsettings.json  
    */
    public class JWTConfig
    {
        public string Secret { get; set; } = string.Empty;
    }
}
