namespace Grasindo.API.Models
{
    public class Customer
    {
        public string name {get;set;}
        public Address[] addresses {get;set;}
        public Contact[] contacts {get;set;}
    }
}