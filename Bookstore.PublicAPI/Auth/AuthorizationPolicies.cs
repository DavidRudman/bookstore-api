namespace Bookstore.PublicAPI.Auth
{
    public static class Roles 
    {
        public const string Read = "Read";
        public const string ReadWrite = "ReadWrite";
    }

    public static class Policies
    {
        public const string CanRead = "CanRead";
        public const string CanWrite = "CanWrite";
    }
}
