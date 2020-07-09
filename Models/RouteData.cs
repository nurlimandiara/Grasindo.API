

namespace Grasindo.API.Models
{
    public class RouteData
    {
        public int[][] weights {get;set;}
        public int[] serviceTimes {get;set;}
        public int[] demands {get;set;}
        public int[][] timeWindows {get;set;}
        public long[] vehicleCapacities {get;set;}
        public int depot {get;set;}
        public int vehicleNumber {get;set;}

    }
}