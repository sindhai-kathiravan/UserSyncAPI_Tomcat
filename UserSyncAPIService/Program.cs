using System.ServiceProcess;

namespace UserSyncAPIService
{
    public class Program
    {
        public static void Main()
        {
            ServiceBase.Run(new ApiServiceWrapper());
        }
    }
}