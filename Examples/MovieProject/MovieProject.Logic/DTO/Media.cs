namespace MovieProject.Logic.DTO
{
    public class Media
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Year { get; set; }
        public string Plot { get; set; }
        public string Runtime { get; set; }
        public Languages Language { get; set; }

        public enum Languages
        {
            English,
            Spanish,
            French,
            Other
        }
    }
}
