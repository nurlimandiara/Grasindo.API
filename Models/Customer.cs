namespace Grasindo.API.Models
{
    public class Customer
    {
        public int id {get;}
        public string name {get;set;}
        public Address[] addresses {get;set;}
        public Contact[] contacts {get;set;}
    }
}