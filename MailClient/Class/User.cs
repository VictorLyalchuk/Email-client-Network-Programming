using System.ComponentModel.DataAnnotations;

namespace MailClient
{
    public static class User
    {
        [EmailAddress]
        public static string Login { get; set; }
        public static string Password { get; set; }
    }
}
