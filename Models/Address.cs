using System.Drawing;

namespace Grasindo.API.Models
{
    public class Address
    {
        public string name {get;set;}
        public string line1 {get;set;}
        public string line2 {get;set;}
        public string city {get;set;}
        public string subDistrict {get;set;}
        public string village {get;set;}
        public Point coordinate {get;set;}
    }
}