
using System.Security.AccessControl;

namespace API.Entities
{
    public class AppUser
    {
        public int Id { get; set; }     
        public string UserName { get; set; }   

        public byte[] PasswordHash { get; set; }     
        public byte[] PaasswordSalt { get; set; }
    }
}