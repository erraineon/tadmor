using E621;
using Tadmor.Furry.Interfaces;

namespace Tadmor.Furry.Services
{
    public class E621ClientWrapper : E621Client, IE621Client
    {
        public E621ClientWrapper() : base("tadmor/errai")
        {
        }
    }
}