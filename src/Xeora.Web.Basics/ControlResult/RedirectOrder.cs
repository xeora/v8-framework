namespace Xeora.Web.Basics.ControlResult
{
    public class RedirectOrder
    {
        public RedirectOrder(string location) =>
            this.Location = location;

        public string Location { get; }
    }
}
