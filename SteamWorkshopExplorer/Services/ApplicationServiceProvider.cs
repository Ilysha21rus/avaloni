using SteamWorkshopExplorer.PageParser;

namespace SteamWorkshopExplorer.Services
{
    public class ApplicationServiceProvider
    {
        private readonly Parser _parser;

        public ApplicationServiceProvider()
        {
            _parser = new Parser();
        }

        public string ParsePageTitle(string html)
        {
            return _parser.GetPageTitle(html);
        }
    }
}