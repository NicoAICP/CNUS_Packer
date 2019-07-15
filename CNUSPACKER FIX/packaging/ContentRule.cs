namespace CNUS_packer.packaging
{
    public class ContentRule
    {
        private string pattern = "";
        private ContentDetails details = null;
        private bool contentPerMatch = false;

        public ContentRule(string pattern, ContentDetails details, bool contentPerMatch)
        {
            setPattern(pattern);
            setDetails(details);
            setContentPerMatch(contentPerMatch);
        }

        public string getPattern()
        {
            return pattern;
        }

        public void setPattern(string pattern)
        {
            this.pattern = pattern;
        }

        public ContentDetails getDetails()
        {
            return details;
        }

        public void setDetails(ContentDetails details)
        {
            this.details = details;
        }

        public bool isContentPerMatch()
        {
            return contentPerMatch;
        }

        public void setContentPerMatch(bool contentPerMatch)
        {
            this.contentPerMatch = contentPerMatch;
        }
    }
}
