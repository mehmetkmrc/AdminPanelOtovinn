namespace AuthManual.Models
{
    public class Tab_Screen
    {
        public Guid Id { get; set; }
        public string Context { get; set; }

        // Tabs_Main ile ilişkilendirilecek özellik
        public Guid? Tabs_MainId { get; set; }
        public Tabs_Main Tabs_Main { get; set; }
    }
}
